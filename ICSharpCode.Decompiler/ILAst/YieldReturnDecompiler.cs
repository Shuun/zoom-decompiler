﻿// Copyright (c) AlphaSierraPapa for the SharpDevelop Team (for details please see \doc\copyright.txt)
// This code is distributed under MIT X11 license (for details please see \doc\license.txt)

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Mono.Cecil;

namespace ICSharpCode.Decompiler.ILAst
{
	public class YieldReturnDecompiler
	{
		// For a description on the code generated by the C# compiler for yield return:
		// http://csharpindepth.com/Articles/Chapter6/IteratorBlockImplementation.aspx
		
		// The idea here is:
		// - Figure out whether the current method is instanciating an enumerator
		// - Figure out which of the fields is the state field
		// - Construct an exception table based on states. This allows us to determine, for each state, what the parent try block is.
		
		/// <summary>
		/// This exception is thrown when we find something else than we expect from the C# compiler.
		/// This aborts the analysis and makes the whole transform fail.
		/// </summary>
		class YieldAnalysisFailedException : Exception {}
		
		DecompilerContext context;
		TypeDefinition enumeratorType;
		MethodDefinition enumeratorCtor;
		MethodDefinition disposeMethod;
		FieldDefinition stateField;
		FieldDefinition currentField;
		Dictionary<FieldDefinition, ParameterDefinition> fieldToParameterMap = new Dictionary<FieldDefinition, ParameterDefinition>();
		List<ILNode> newBody;
		
		#region Run() method
		public static void Run(DecompilerContext context, ILBlock method)
		{
			if (!context.Settings.YieldReturn)
				return; // abort if enumerator decompilation is disabled
			var yrd = new YieldReturnDecompiler();
			yrd.context = context;
			if (!yrd.MatchEnumeratorCreationPattern(method))
				return;
			yrd.enumeratorType = yrd.enumeratorCtor.DeclaringType;
			#if DEBUG
			if (Debugger.IsAttached) {
				yrd.Run();
			} else {
				#endif
				try {
					yrd.Run();
				} catch (YieldAnalysisFailedException) {
					return;
				}
				#if DEBUG
			}
			#endif
			method.Body.Clear();
			method.EntryGoto = null;
			method.Body.AddRange(yrd.newBody);
		}
		
		void Run()
		{
			AnalyzeCtor();
			AnalyzeCurrentProperty();
			ResolveIEnumerableIEnumeratorFieldMapping();
			ConstructExceptionTable();
			AnalyzeMoveNext();
			TranslateFieldsToLocalAccess();
		}
		#endregion
		
		#region Match the enumerator creation pattern
		bool MatchEnumeratorCreationPattern(ILBlock method)
		{
			if (method.Body.Count == 0)
				return false;
			ILExpression newObj;
			if (method.Body.Count == 1) {
				// ret(newobj(...))
				if (method.Body[0].Match(ILCode.Ret, out newObj))
					return MatchEnumeratorCreationNewObj(newObj, out enumeratorCtor);
				else
					return false;
			}
			// stloc(var_1, newobj(..)
			ILVariable var1;
			if (!method.Body[0].Match(ILCode.Stloc, out var1, out newObj))
				return false;
			if (!MatchEnumeratorCreationNewObj(newObj, out enumeratorCtor))
				return false;
			
			int i;
			for (i = 1; i < method.Body.Count; i++) {
				// stfld(..., ldloc(var_1), ldarg(...))
				FieldReference storedField;
				ILExpression ldloc, ldarg;
				if (!method.Body[i].Match(ILCode.Stfld, out storedField, out ldloc, out ldarg))
					break;
				if (ldloc.Code != ILCode.Ldloc || ldarg.Code != ILCode.Ldarg)
					return false;
				if (ldloc.Operand != var1 || !(storedField is FieldDefinition))
					return false;
				fieldToParameterMap[(FieldDefinition)storedField] = (ParameterDefinition)ldarg.Operand;
			}
			ILVariable var2;
			ILExpression ldlocForStloc2;
			if (i < method.Body.Count && method.Body[i].Match(ILCode.Stloc, out var2, out ldlocForStloc2)) {
				// stloc(var_2, ldloc(var_1))
				if (ldlocForStloc2.Code != ILCode.Ldloc || ldlocForStloc2.Operand != var1)
					return false;
				i++;
			} else {
				// the compiler might skip the above instruction in release builds; in that case, it directly returns stloc.Operand
				var2 = var1;
			}
			if (!SkipDummyBr(method, ref i))
				return false;
			ILExpression retArg;
			if (i < method.Body.Count && method.Body[i].Match(ILCode.Ret, out retArg)) {
				// ret(ldloc(var_2))
				if (retArg.Code == ILCode.Ldloc && retArg.Operand == var2) {
					return true;
				}
			}
			return false;
		}
		
		bool SkipDummyBr(ILBlock method, ref int i)
		{
			ILLabel target;
			if (i + 1 < method.Body.Count && method.Body[i].Match(ILCode.Br, out target)) {
				if (target != method.Body[i + 1])
					return false;
				i += 2;
			}
			return true;
		}
		
		bool MatchEnumeratorCreationNewObj(ILExpression expr, out MethodDefinition ctor)
		{
			// newobj(CurrentType/...::.ctor, ldc.i4(-2))
			ctor = expr.Operand as MethodDefinition;
			if (expr.Code != ILCode.Newobj || expr.Arguments.Count != 1)
				return false;
			if (expr.Arguments[0].Code != ILCode.Ldc_I4)
				return false;
			int initialState = (int)expr.Arguments[0].Operand;
			if (!(initialState == -2 || initialState == 0))
				return false;
			if (ctor == null || ctor.DeclaringType.DeclaringType != context.CurrentType)
				return false;
			return IsCompilerGeneratorEnumerator(ctor.DeclaringType);
		}
		
		public static bool IsCompilerGeneratorEnumerator(TypeDefinition type)
		{
			if (!(type.Name.StartsWith("<", StringComparison.Ordinal) && type.IsCompilerGenerated()))
				return false;
			foreach (TypeReference i in type.Interfaces) {
				if (i.Namespace == "System.Collections" && i.Name == "IEnumerator")
					return true;
			}
			return false;
		}
		#endregion
		
		#region Figure out what the 'state' field is (analysis of .ctor())
		/// <summary>
		/// Looks at the enumerator's ctor and figures out which of the fields holds the state.
		/// </summary>
		void AnalyzeCtor()
		{
			ILBlock method = CreateILAst(enumeratorCtor);
			
			ILExpression stfldPattern = new ILExpression(ILCode.Stfld, ILExpression.AnyOperand, LoadFromArgument.This, new LoadFromArgument(0));
			
			foreach (ILNode node in method.Body) {
				if (stfldPattern.Match(node)) {
					stateField = ((ILExpression)node).Operand as FieldDefinition;
				}
			}
			if (stateField == null)
				throw new YieldAnalysisFailedException();
		}
		
		/// <summary>
		/// Creates ILAst for the specified method, optimized up to before the 'YieldReturn' step.
		/// </summary>
		ILBlock CreateILAst(MethodDefinition method)
		{
			if (method == null || !method.HasBody)
				throw new YieldAnalysisFailedException();
			
			ILBlock ilMethod = new ILBlock();
			ILAstBuilder astBuilder = new ILAstBuilder();
			ilMethod.Body = astBuilder.Build(method, true);
			ILAstOptimizer optimizer = new ILAstOptimizer();
			optimizer.Optimize(context, ilMethod, ILAstOptimizationStep.YieldReturn);
			return ilMethod;
		}
		#endregion
		
		#region Figure out what the 'current' field is (analysis of get_Current())
		static readonly ILExpression returnFieldFromThisPattern = new ILExpression(ILCode.Ret, null, new ILExpression(ILCode.Ldfld, ILExpression.AnyOperand, LoadFromArgument.This));
		
		/// <summary>
		/// Looks at the enumerator's get_Current method and figures out which of the fields holds the current value.
		/// </summary>
		void AnalyzeCurrentProperty()
		{
			MethodDefinition getCurrentMethod = enumeratorType.Methods.FirstOrDefault(
				m => m.Name.StartsWith("System.Collections.Generic.IEnumerator", StringComparison.Ordinal)
				&& m.Name.EndsWith(".get_Current", StringComparison.Ordinal));
			ILBlock method = CreateILAst(getCurrentMethod);
			if (method.Body.Count == 1) {
				// release builds directly return the current field
				if (returnFieldFromThisPattern.Match(method.Body[0])) {
					currentField = ((ILExpression)method.Body[0]).Arguments[0].Operand as FieldDefinition;
				}
			} else {
				StoreToVariable v = new StoreToVariable(new ILExpression(ILCode.Ldfld, ILExpression.AnyOperand, LoadFromArgument.This));
				if (v.Match(method.Body[0])) {
					int i = 1;
					if (SkipDummyBr(method, ref i) && i == method.Body.Count - 1) {
						if (new ILExpression(ILCode.Ret, null, new LoadFromVariable(v)).Match(method.Body[i])) {
							currentField = ((ILExpression)method.Body[0]).Arguments[0].Operand as FieldDefinition;
						}
					}
				}
			}
			if (currentField == null)
				throw new YieldAnalysisFailedException();
		}
		#endregion
		
		#region Figure out the mapping of IEnumerable fields to IEnumerator fields  (analysis of GetEnumerator())
		void ResolveIEnumerableIEnumeratorFieldMapping()
		{
			MethodDefinition getEnumeratorMethod = enumeratorType.Methods.FirstOrDefault(
				m => m.Name.StartsWith("System.Collections.Generic.IEnumerable", StringComparison.Ordinal)
				&& m.Name.EndsWith(".GetEnumerator", StringComparison.Ordinal));
			if (getEnumeratorMethod == null)
				return; // no mappings (maybe it's just an IEnumerator implementation?)
			
			ILExpression mappingPattern = new ILExpression(
				ILCode.Stfld, ILExpression.AnyOperand, new AnyILExpression(),
				new ILExpression(ILCode.Ldfld, ILExpression.AnyOperand, LoadFromArgument.This));
			
			ILBlock method = CreateILAst(getEnumeratorMethod);
			foreach (ILNode node in method.Body) {
				if (mappingPattern.Match(node)) {
					ILExpression stfld = (ILExpression)node;
					FieldDefinition storedField = stfld.Operand as FieldDefinition;
					FieldDefinition loadedField = stfld.Arguments[1].Operand as FieldDefinition;
					if (storedField != null && loadedField != null) {
						ParameterDefinition mappedParameter;
						if (fieldToParameterMap.TryGetValue(loadedField, out mappedParameter))
							fieldToParameterMap[storedField] = mappedParameter;
					}
				}
			}
		}
		#endregion
		
		#region Construction of the exception table (analysis of Dispose())
		// We construct the exception table by analyzing the enumerator's Dispose() method.
		
		// Assumption: there are no loops/backward jumps
		// We 'run' the code, with "state" being a symbolic variable
		// so it can form expressions like "state + x" (when there's a sub instruction)
		// For each instruction, we maintain a list of value ranges for state for which the instruction is reachable.
		// This is (int.MinValue, int.MaxValue) for the first instruction.
		// These ranges are propagated depending on the conditional jumps performed by the code.
		
		Dictionary<MethodDefinition, Interval> finallyMethodToStateInterval;
		
		void ConstructExceptionTable()
		{
			disposeMethod = enumeratorType.Methods.FirstOrDefault(m => m.Name == "System.IDisposable.Dispose");
			ILBlock ilMethod = CreateILAst(disposeMethod);
			
			finallyMethodToStateInterval = new Dictionary<MethodDefinition, Interval>();
			
			InitStateRanges(ilMethod.Body[0]);
			AssignStateRanges(ilMethod.Body, ilMethod.Body.Count, forDispose: true);
			
			// Now look at the finally blocks:
			foreach (var tryFinally in ilMethod.GetSelfAndChildrenRecursive<ILTryCatchBlock>()) {
				Interval interval = ranges[tryFinally.TryBlock.Body[0]].ToEnclosingInterval();
				var finallyBody = tryFinally.FinallyBlock.Body;
				if (!(finallyBody.Count == 2 || finallyBody.Count == 3))
					throw new YieldAnalysisFailedException();
				ILExpression call = finallyBody[0] as ILExpression;
				if (call == null || call.Code != ILCode.Call || call.Arguments.Count != 1)
					throw new YieldAnalysisFailedException();
				if (call.Arguments[0].Code != ILCode.Ldarg || ((ParameterDefinition)call.Arguments[0].Operand).Index >= 0)
					throw new YieldAnalysisFailedException();
				if (finallyBody.Count == 3 && !finallyBody[1].Match(ILCode.Nop))
					throw new YieldAnalysisFailedException();
				if (!finallyBody[finallyBody.Count - 1].Match(ILCode.Endfinally))
					throw new YieldAnalysisFailedException();
				
				MethodDefinition mdef = call.Operand as MethodDefinition;
				if (mdef == null || finallyMethodToStateInterval.ContainsKey(mdef))
					throw new YieldAnalysisFailedException();
				finallyMethodToStateInterval.Add(mdef, interval);
			}
			ranges = null;
		}
		#endregion
		
		#region Assign StateRanges / Symbolic Execution (used for analysis of Dispose() and MoveNext())
		#region struct Interval / class StateRange
		struct Interval
		{
			public readonly int Start, End;
			
			public Interval(int start, int end)
			{
				Debug.Assert(start <= end || (start == 0 && end == -1));
				this.Start = start;
				this.End = end;
			}
			
			public override string ToString()
			{
				return string.Format("({0} to {1})", Start, End);
			}
		}
		
		class StateRange
		{
			readonly List<Interval> data = new List<Interval>();
			
			public StateRange()
			{
			}
			
			public StateRange(int start, int end)
			{
				this.data.Add(new Interval(start, end));
			}
			
			public bool Contains(int val)
			{
				foreach (Interval v in data) {
					if (v.Start <= val && val <= v.End)
						return true;
				}
				return false;
			}
			
			public void UnionWith(StateRange other)
			{
				data.AddRange(other.data);
			}
			
			/// <summary>
			/// Unions this state range with (other intersect (minVal to maxVal))
			/// </summary>
			public void UnionWith(StateRange other, int minVal, int maxVal)
			{
				foreach (Interval v in other.data) {
					int start = Math.Max(v.Start, minVal);
					int end = Math.Min(v.End, maxVal);
					if (start <= end)
						data.Add(new Interval(start, end));
				}
			}
			
			/// <summary>
			/// Merges overlapping interval ranges.
			/// </summary>
			public void Simplify()
			{
				if (data.Count < 2)
					return;
				data.Sort((a, b) => a.Start.CompareTo(b.Start));
				Interval prev = data[0];
				int prevIndex = 0;
				for (int i = 1; i < data.Count; i++) {
					Interval next = data[i];
					Debug.Assert(prev.Start <= next.Start);
					if (next.Start <= prev.End + 1) { // intervals overlapping or touching
						prev = new Interval(prev.Start, Math.Max(prev.End, next.End));
						data[prevIndex] = prev;
						data[i] = new Interval(0, -1); // mark as deleted
					} else {
						prev = next;
						prevIndex = i;
					}
				}
				data.RemoveAll(i => i.Start > i.End); // remove all entries that were marked as deleted
			}
			
			public override string ToString()
			{
				return string.Join(",", data);
			}
			
			public Interval ToEnclosingInterval()
			{
				if (data.Count == 0)
					throw new YieldAnalysisFailedException();
				return new Interval(data[0].Start, data[data.Count - 1].End);
			}
		}
		#endregion
		
		DefaultDictionary<ILNode, StateRange> ranges;
		ILVariable rangeAnalysisStateVariable;
		
		/// <summary>
		/// Initializes the state range logic:
		/// Clears 'ranges' and sets 'ranges[entryPoint]' to the full range (int.MinValue to int.MaxValue)
		/// </summary>
		void InitStateRanges(ILNode entryPoint)
		{
			ranges = new DefaultDictionary<ILNode, StateRange>(n => new StateRange());
			ranges[entryPoint] = new StateRange(int.MinValue, int.MaxValue);
			rangeAnalysisStateVariable = null;
		}
		
		int AssignStateRanges(List<ILNode> body, int bodyLength, bool forDispose)
		{
			if (bodyLength == 0)
				return 0;
			for (int i = 0; i < bodyLength; i++) {
				StateRange nodeRange = ranges[body[i]];
				nodeRange.Simplify();
				
				ILLabel label = body[i] as ILLabel;
				if (label != null) {
					ranges[body[i + 1]].UnionWith(nodeRange);
					continue;
				}
				
				ILTryCatchBlock tryFinally = body[i] as ILTryCatchBlock;
				if (tryFinally != null) {
					if (!forDispose || tryFinally.CatchBlocks.Count != 0 || tryFinally.FaultBlock != null || tryFinally.FinallyBlock == null)
						throw new YieldAnalysisFailedException();
					ranges[tryFinally.TryBlock].UnionWith(nodeRange);
					AssignStateRanges(tryFinally.TryBlock.Body, tryFinally.TryBlock.Body.Count, forDispose);
					continue;
				}
				
				ILExpression expr = body[i] as ILExpression;
				if (expr == null)
					throw new YieldAnalysisFailedException();
				switch (expr.Code) {
					case ILCode.Switch:
						{
							SymbolicValue val = Eval(expr.Arguments[0]);
							if (val.Type != SymbolicValueType.State)
								throw new YieldAnalysisFailedException();
							ILLabel[] targetLabels = (ILLabel[])expr.Operand;
							for (int j = 0; j < targetLabels.Length; j++) {
								int state = j - val.Constant;
								ranges[targetLabels[j]].UnionWith(nodeRange, state, state);
							}
							StateRange nextRange = ranges[body[i + 1]];
							nextRange.UnionWith(nodeRange, int.MinValue, -1 - val.Constant);
							nextRange.UnionWith(nodeRange, targetLabels.Length - val.Constant, int.MaxValue);
							break;
						}
					case ILCode.Br:
					case ILCode.Leave:
						ranges[(ILLabel)expr.Operand].UnionWith(nodeRange);
						break;
					case ILCode.Brtrue:
						{
							SymbolicValue val = Eval(expr.Arguments[0]);
							if (val.Type == SymbolicValueType.StateEquals) {
								ranges[(ILLabel)expr.Operand].UnionWith(nodeRange, val.Constant, val.Constant);
								StateRange nextRange = ranges[body[i + 1]];
								nextRange.UnionWith(nodeRange, int.MinValue, val.Constant - 1);
								nextRange.UnionWith(nodeRange, val.Constant + 1, int.MaxValue);
							} else if (val.Type == SymbolicValueType.StateInEquals) {
								ranges[body[i + 1]].UnionWith(nodeRange, val.Constant, val.Constant);
								StateRange targetRange = ranges[(ILLabel)expr.Operand];
								targetRange.UnionWith(nodeRange, int.MinValue, val.Constant - 1);
								targetRange.UnionWith(nodeRange, val.Constant + 1, int.MaxValue);
							} else {
								throw new YieldAnalysisFailedException();
							}
							break;
						}
					case ILCode.Nop:
						ranges[body[i + 1]].UnionWith(nodeRange);
						break;
					case ILCode.Ret:
						break;
					case ILCode.Stloc:
						{
							SymbolicValue val = Eval(expr.Arguments[0]);
							if (val.Type == SymbolicValueType.State && val.Constant == 0 && rangeAnalysisStateVariable == null)
								rangeAnalysisStateVariable = (ILVariable)expr.Operand;
							else
								throw new YieldAnalysisFailedException();
							goto case ILCode.Nop;
						}
					case ILCode.Call:
						// in some cases (e.g. foreach over array) the C# compiler produces a finally method outside of try-finally blocks
						if (forDispose) {
							MethodDefinition mdef = expr.Operand as MethodDefinition;
							if (mdef == null || finallyMethodToStateInterval.ContainsKey(mdef))
								throw new YieldAnalysisFailedException();
							finallyMethodToStateInterval.Add(mdef, nodeRange.ToEnclosingInterval());
						} else {
							throw new YieldAnalysisFailedException();
						}
						break;
					default:
						if (forDispose)
							throw new YieldAnalysisFailedException();
						else
							return i;
				}
			}
			return bodyLength;
		}
		
		enum SymbolicValueType
		{
			/// <summary>
			/// int: Constant (result of ldc.i4)
			/// </summary>
			IntegerConstant,
			/// <summary>
			/// int: State + Constant
			/// </summary>
			State,
			/// <summary>
			/// This pointer (result of ldarg.0)
			/// </summary>
			This,
			/// <summary>
			/// bool: State == Constant
			/// </summary>
			StateEquals,
			/// <summary>
			/// bool: State != Constant
			/// </summary>
			StateInEquals
		}
		
		struct SymbolicValue
		{
			public readonly int Constant;
			public readonly SymbolicValueType Type;
			
			public SymbolicValue(SymbolicValueType type, int constant = 0)
			{
				this.Type = type;
				this.Constant = constant;
			}
			
			public override string ToString()
			{
				return string.Format("[SymbolicValue {0}: {1}]", this.Type, this.Constant);
			}
		}
		
		SymbolicValue Eval(ILExpression expr)
		{
			SymbolicValue left, right;
			switch (expr.Code) {
				case ILCode.Sub:
					left = Eval(expr.Arguments[0]);
					right = Eval(expr.Arguments[1]);
					if (left.Type != SymbolicValueType.State && left.Type != SymbolicValueType.IntegerConstant)
						throw new YieldAnalysisFailedException();
					if (right.Type != SymbolicValueType.IntegerConstant)
						throw new YieldAnalysisFailedException();
					return new SymbolicValue(left.Type, unchecked ( left.Constant - right.Constant ));
				case ILCode.Ldfld:
					if (Eval(expr.Arguments[0]).Type != SymbolicValueType.This)
						throw new YieldAnalysisFailedException();
					if (expr.Operand != stateField)
						throw new YieldAnalysisFailedException();
					return new SymbolicValue(SymbolicValueType.State);
				case ILCode.Ldloc:
					if (expr.Operand == rangeAnalysisStateVariable)
						return new SymbolicValue(SymbolicValueType.State);
					else
						throw new YieldAnalysisFailedException();
				case ILCode.Ldarg:
					if (((ParameterDefinition)expr.Operand).Index < 0)
						return new SymbolicValue(SymbolicValueType.This);
					else
						throw new YieldAnalysisFailedException();
				case ILCode.Ldc_I4:
					return new SymbolicValue(SymbolicValueType.IntegerConstant, (int)expr.Operand);
				case ILCode.Ceq:
					left = Eval(expr.Arguments[0]);
					right = Eval(expr.Arguments[1]);
					if (left.Type != SymbolicValueType.State || right.Type != SymbolicValueType.IntegerConstant)
						throw new YieldAnalysisFailedException();
					// bool: (state + left.Constant == right.Constant)
					// bool: (state == right.Constant - left.Constant)
					return new SymbolicValue(SymbolicValueType.StateEquals, unchecked ( right.Constant - left.Constant ));
				case ILCode.LogicNot:
					SymbolicValue val = Eval(expr.Arguments[0]);
					if (val.Type == SymbolicValueType.StateEquals)
						return new SymbolicValue(SymbolicValueType.StateInEquals, val.Constant);
					else if (val.Type == SymbolicValueType.StateInEquals)
						return new SymbolicValue(SymbolicValueType.StateEquals, val.Constant);
					else
						throw new YieldAnalysisFailedException();
				default:
					throw new YieldAnalysisFailedException();
			}
		}
		#endregion
		
		#region Analysis of MoveNext()
		ILVariable returnVariable;
		ILLabel returnLabel;
		ILLabel returnFalseLabel;
		
		void AnalyzeMoveNext()
		{
			MethodDefinition moveNextMethod = enumeratorType.Methods.FirstOrDefault(m => m.Name == "MoveNext");
			ILBlock ilMethod = CreateILAst(moveNextMethod);
			
			if (ilMethod.Body.Count == 0)
				throw new YieldAnalysisFailedException();
			ILExpression lastReturnArg;
			if (!ilMethod.Body.Last().Match(ILCode.Ret, out lastReturnArg))
				throw new YieldAnalysisFailedException();
			
			ilMethod.Body.RemoveAll(n => n.Match(ILCode.Nop)); // remove nops
			
			// There are two possibilities:
			if (lastReturnArg.Code == ILCode.Ldloc) {
				// a) the compiler uses a variable for returns (in debug builds, or when there are try-finally blocks)
				returnVariable = (ILVariable)lastReturnArg.Operand;
				returnLabel = ilMethod.Body.ElementAtOrDefault(ilMethod.Body.Count - 2) as ILLabel;
				if (returnLabel == null)
					throw new YieldAnalysisFailedException();
			} else {
				// b) the compiler directly returns constants
				returnVariable = null;
				returnLabel = null;
				// In this case, the last return must return false.
				if (lastReturnArg.Code != ILCode.Ldc_I4 || (int)lastReturnArg.Operand != 0)
					throw new YieldAnalysisFailedException();
			}
			
			ILTryCatchBlock tryFaultBlock = ilMethod.Body[0] as ILTryCatchBlock;
			List<ILNode> body;
			int bodyLength;
			if (tryFaultBlock != null) {
				// there are try-finally blocks
				if (returnVariable == null) // in this case, we must use a return variable
					throw new YieldAnalysisFailedException();
				// must be a try-fault block:
				if (tryFaultBlock.CatchBlocks.Count != 0 || tryFaultBlock.FinallyBlock != null || tryFaultBlock.FaultBlock == null)
					throw new YieldAnalysisFailedException();
				
				ILBlock faultBlock = tryFaultBlock.FaultBlock;
				// Ensure the fault block contains the call to Dispose().
				if (!(faultBlock.Body.Count == 2 || faultBlock.Body.Count == 3))
					throw new YieldAnalysisFailedException();
				if (!new ILExpression(ILCode.Call, disposeMethod, LoadFromArgument.This).Match(faultBlock.Body[0]))
					throw new YieldAnalysisFailedException();
				if (faultBlock.Body.Count == 3 && !faultBlock.Body[1].Match(ILCode.Nop))
					throw new YieldAnalysisFailedException();
				if (!faultBlock.Body[faultBlock.Body.Count - 1].Match(ILCode.Endfinally))
					throw new YieldAnalysisFailedException();
				
				body = tryFaultBlock.TryBlock.Body;
				body.RemoveAll(n => n.Match(ILCode.Nop)); // remove nops
				bodyLength = body.Count;
			} else {
				// no try-finally blocks
				body = ilMethod.Body;
				if (returnVariable == null)
					bodyLength = body.Count - 1; // all except for the return statement
				else
					bodyLength = body.Count - 2; // all except for the return label and statement
			}
			
			// Now verify that the last instruction in the body is 'ret(false)'
			if (returnVariable != null) {
				// If we don't have a return variable, we already verified that above.
				if (bodyLength < 2)
					throw new YieldAnalysisFailedException();
				ILExpression leave = body[bodyLength - 1] as ILExpression;
				if (leave == null || leave.Operand != returnLabel || !(leave.Code == ILCode.Br || leave.Code == ILCode.Leave))
					throw new YieldAnalysisFailedException();
				ILExpression store0 = body[bodyLength - 2] as ILExpression;
				if (store0 == null || store0.Code != ILCode.Stloc || store0.Operand != returnVariable)
					throw new YieldAnalysisFailedException();
				if (store0.Arguments[0].Code != ILCode.Ldc_I4 || (int)store0.Arguments[0].Operand != 0)
					throw new YieldAnalysisFailedException();
				
				bodyLength -= 2; // don't conside the 'ret(false)' part of the body
			}
			// verify that the last element in the body is a label pointing to the 'ret(false)'
			returnFalseLabel = body.ElementAtOrDefault(bodyLength - 1) as ILLabel;
			if (returnFalseLabel == null)
				throw new YieldAnalysisFailedException();
			
			InitStateRanges(body[0]);
			int pos = AssignStateRanges(body, bodyLength, forDispose: false);
			if (pos > 0 && body[pos - 1] is ILLabel) {
				pos--;
			} else {
				// ensure that the first element at body[pos] is a label:
				ILLabel newLabel = new ILLabel();
				newLabel.Name = "YieldReturnEntryPoint";
				ranges[newLabel] = ranges[body[pos]]; // give the label the range of the instruction at body[pos]
				
				body.Insert(pos, newLabel);
				bodyLength++;
			}
			
			List<KeyValuePair<ILLabel, StateRange>> labels = new List<KeyValuePair<ILLabel, StateRange>>();
			for (int i = pos; i < bodyLength; i++) {
				ILLabel label = body[i] as ILLabel;
				if (label != null) {
					labels.Add(new KeyValuePair<ILLabel, StateRange>(label, ranges[label]));
				}
			}
			
			ConvertBody(body, pos, bodyLength, labels);
		}
		#endregion
		
		#region ConvertBody
		struct SetState
		{
			public readonly int NewBodyPos;
			public readonly int NewState;
			
			public SetState(int newBodyPos, int newState)
			{
				this.NewBodyPos = newBodyPos;
				this.NewState = newState;
			}
		}
		
		void ConvertBody(List<ILNode> body, int startPos, int bodyLength, List<KeyValuePair<ILLabel, StateRange>> labels)
		{
			newBody = new List<ILNode>();
			newBody.Add(MakeGoTo(labels, 0));
			List<SetState> stateChanges = new List<SetState>();
			int currentState = -1;
			// Copy all instructions from the old body to newBody.
			for (int pos = startPos; pos < bodyLength; pos++) {
				ILExpression expr = body[pos] as ILExpression;
				if (expr != null && expr.Code == ILCode.Stfld && LoadFromArgument.This.Match(expr.Arguments[0])) {
					// Handle stores to 'state' or 'current'
					if (expr.Operand == stateField) {
						if (expr.Arguments[1].Code != ILCode.Ldc_I4)
							throw new YieldAnalysisFailedException();
						currentState = (int)expr.Arguments[1].Operand;
						stateChanges.Add(new SetState(newBody.Count, currentState));
					} else if (expr.Operand == currentField) {
						newBody.Add(new ILExpression(ILCode.YieldReturn, null, expr.Arguments[1]));
					} else {
						newBody.Add(body[pos]);
					}
				} else if (returnVariable != null && expr != null && expr.Code == ILCode.Stloc && expr.Operand == returnVariable) {
					// handle store+branch to the returnVariable
					ILExpression br = body.ElementAtOrDefault(++pos) as ILExpression;
					if (br == null || !(br.Code == ILCode.Br || br.Code == ILCode.Leave) || br.Operand != returnLabel || expr.Arguments[0].Code != ILCode.Ldc_I4)
						throw new YieldAnalysisFailedException();
					int val = (int)expr.Arguments[0].Operand;
					if (val == 0) {
						newBody.Add(MakeGoTo(returnFalseLabel));
					} else if (val == 1) {
						newBody.Add(MakeGoTo(labels, currentState));
					} else {
						throw new YieldAnalysisFailedException();
					}
				} else if (expr != null && expr.Code == ILCode.Ret) {
					if (expr.Arguments.Count != 1 || expr.Arguments[0].Code != ILCode.Ldc_I4)
						throw new YieldAnalysisFailedException();
					// handle direct return (e.g. in release builds)
					int val = (int)expr.Arguments[0].Operand;
					if (val == 0) {
						newBody.Add(MakeGoTo(returnFalseLabel));
					} else if (val == 1) {
						newBody.Add(MakeGoTo(labels, currentState));
					} else {
						throw new YieldAnalysisFailedException();
					}
				} else if (expr != null && expr.Code == ILCode.Call && expr.Arguments.Count == 1 && LoadFromArgument.This.Match(expr.Arguments[0])) {
					MethodDefinition method = expr.Operand as MethodDefinition;
					if (method == null)
						throw new YieldAnalysisFailedException();
					Interval interval;
					if (method == disposeMethod) {
						// Explicit call to dispose is used for "yield break;" within the method.
						ILExpression br = body.ElementAtOrDefault(++pos) as ILExpression;
						if (br == null || !(br.Code == ILCode.Br || br.Code == ILCode.Leave) || br.Operand != returnFalseLabel)
							throw new YieldAnalysisFailedException();
						newBody.Add(MakeGoTo(returnFalseLabel));
					} else if (finallyMethodToStateInterval.TryGetValue(method, out interval)) {
						// Call to Finally-method
						int index = stateChanges.FindIndex(ss => ss.NewState >= interval.Start && ss.NewState <= interval.End);
						if (index < 0)
							throw new YieldAnalysisFailedException();
						
						ILLabel label = new ILLabel();
						label.Name = "JumpOutOfTryFinally" + interval.Start + "_" + interval.End;
						newBody.Add(new ILExpression(ILCode.Leave, label));
						
						SetState stateChange = stateChanges[index];
						// Move all instructions from stateChange.Pos to newBody.Count into a try-block
						stateChanges.RemoveRange(index, stateChanges.Count - index); // remove all state changes up to the one we found
						ILTryCatchBlock tryFinally = new ILTryCatchBlock();
						tryFinally.TryBlock = new ILBlock(newBody.GetRange(stateChange.NewBodyPos, newBody.Count - stateChange.NewBodyPos));
						newBody.RemoveRange(stateChange.NewBodyPos, newBody.Count - stateChange.NewBodyPos); // remove all nodes that we just moved into the try block
						tryFinally.CatchBlocks = new List<ILTryCatchBlock.CatchBlock>();
						tryFinally.FinallyBlock = ConvertFinallyBlock(method);
						newBody.Add(tryFinally);
						newBody.Add(label);
					}
				} else {
					newBody.Add(body[pos]);
				}
			}
			newBody.Add(new ILExpression(ILCode.YieldBreak, null));
		}
		
		ILExpression MakeGoTo(ILLabel targetLabel)
		{
			if (targetLabel == returnFalseLabel)
				return new ILExpression(ILCode.YieldBreak, null);
			else
				return new ILExpression(ILCode.Br, targetLabel);
		}
		
		ILExpression MakeGoTo(List<KeyValuePair<ILLabel, StateRange>> labels, int state)
		{
			foreach (var pair in labels) {
				if (pair.Value.Contains(state))
					return MakeGoTo(pair.Key);
			}
			throw new YieldAnalysisFailedException();
		}
		
		ILBlock ConvertFinallyBlock(MethodDefinition finallyMethod)
		{
			ILBlock block = CreateILAst(finallyMethod);
			block.Body.RemoveAll(n => n.Match(ILCode.Nop));
			// Get rid of assignment to state
			FieldReference stfld;
			List<ILExpression> args;
			if (block.Body.Count > 0 && block.Body[0].Match(ILCode.Stfld, out stfld, out args)) {
				if (stfld == stateField && LoadFromArgument.This.Match(args[0]))
					block.Body.RemoveAt(0);
			}
			// Convert ret to endfinally
			foreach (ILExpression expr in block.GetSelfAndChildrenRecursive<ILExpression>()) {
				if (expr.Code == ILCode.Ret)
					expr.Code = ILCode.Endfinally;
			}
			return block;
		}
		#endregion
		
		#region TranslateFieldsToLocalAccess
		void TranslateFieldsToLocalAccess()
		{
			var fieldToLocalMap = new DefaultDictionary<FieldDefinition, ILVariable>(f => new ILVariable { Name = f.Name, Type = f.FieldType });
			foreach (ILNode node in newBody) {
				foreach (ILExpression expr in node.GetSelfAndChildrenRecursive<ILExpression>()) {
					FieldDefinition field = expr.Operand as FieldDefinition;
					if (field != null) {
						switch (expr.Code) {
							case ILCode.Ldfld:
								if (LoadFromArgument.This.Match(expr.Arguments[0])) {
									if (fieldToParameterMap.ContainsKey(field)) {
										expr.Code = ILCode.Ldarg;
										expr.Operand = fieldToParameterMap[field];
									} else {
										expr.Code = ILCode.Ldloc;
										expr.Operand = fieldToLocalMap[field];
									}
									expr.Arguments.Clear();
								}
								break;
							case ILCode.Stfld:
								if (LoadFromArgument.This.Match(expr.Arguments[0])) {
									if (fieldToParameterMap.ContainsKey(field)) {
										expr.Code = ILCode.Starg;
										expr.Operand = fieldToParameterMap[field];
									} else {
										expr.Code = ILCode.Stloc;
										expr.Operand = fieldToLocalMap[field];
									}
									expr.Arguments.RemoveAt(0);
								}
								break;
							case ILCode.Ldflda:
								if (fieldToParameterMap.ContainsKey(field)) {
									expr.Code = ILCode.Ldarga;
									expr.Operand = fieldToParameterMap[field];
								} else {
									expr.Code = ILCode.Ldloca;
									expr.Operand = fieldToLocalMap[field];
								}
								expr.Arguments.Clear();
								break;
						}
					}
				}
			}
		}
		#endregion
	}
}
