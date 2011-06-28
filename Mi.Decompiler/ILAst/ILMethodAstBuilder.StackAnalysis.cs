#region Copyright
// Copyright (c) 2011 AlphaSierraPapa for the SharpDevelop Team
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this
// software and associated documentation files (the "Software"), to deal in the Software
// without restriction, including without limitation the rights to use, copy, modify, merge,
// publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons
// to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or
// substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
// INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR
// PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE
// FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR
// OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.
#endregion

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

using Mi.Assemblies;
using Mi.Assemblies.Cil;
using Cecil = Mi.Assemblies;

namespace Mi.Decompiler.ILAst
{
	partial class ILMethodAstBuilder
	{
		/// <summary> Immutable </summary>
		private sealed class StackSlot
		{
			public readonly ByteCode[] PushedBy;  // One of those
			public readonly ILVariable LoadFrom;  // Where can we get the value from in AST
			
			public StackSlot(ByteCode[] pushedBy, ILVariable loadFrom)
			{
				this.PushedBy = pushedBy;
				this.LoadFrom = loadFrom;
			}
			
			public StackSlot(ByteCode pushedBy)
			{
				this.PushedBy = new[] { pushedBy };
				this.LoadFrom = null;
			}
			
			public static List<StackSlot> CloneStack(List<StackSlot> stack, int? popCount)
			{
				if (popCount.HasValue) {
					return stack.GetRange(0, stack.Count - popCount.Value);
				} else {
					return new List<StackSlot>(0);
				}
			}
		}
		
		List<ByteCode> StackAnalysis(MethodDefinition methodDef)
		{
			Dictionary<Instruction, ByteCode> instrToByteCode = new Dictionary<Instruction, ByteCode>();
			
			// Create temporary structure for the stack analysis
			var result = new List<ByteCode>(methodDef.Body.Instructions.Count);
			List<Instruction> prefixes = null;

			foreach(Instruction inst in methodDef.Body.Instructions) {
				if (inst.OpCode.OpCodeType == OpCodeType.Prefix) {
					if (prefixes == null)
						prefixes = new List<Instruction>(1);
					prefixes.Add(inst);
					continue;
				}

                // take the normal IL opcode as an extended IL opcode 'invented' specially for the decompiler
				ILCode code  = (ILCode)inst.OpCode.Code;

				object operand = inst.Operand;
                
                // convert positional opcodes like ldarg.0 to explicitly-argumented like ldarg(parameter)
                ILCodeUtil.ExpandMacro(ref code, ref operand, methodDef.Body);

				ByteCode byteCode = new ByteCode() {
					Offset      = inst.Offset,
					EndOffset   = inst.Next != null ? inst.Next.Offset : methodDef.Body.CodeSize,
					Code        = code,
					Operand     = operand,
					PopCount    = inst.GetPopDelta(),
					PushCount   = inst.GetPushDelta()
				};

                // populate instrToByteCode
				if (prefixes != null) {
                    // instruction starts from its first prefix
					instrToByteCode[prefixes[0]] = byteCode;
					byteCode.Offset = prefixes[0].Offset;
					byteCode.Prefixes = prefixes.ToArray();
					prefixes = null;
				} else {
					instrToByteCode[inst] = byteCode;
				}

                // populate the result
				result.Add(byteCode);
			}

            // roll through the results and assign 'Next'
			for (int i = 0; i < result.Count - 1; i++) {
				result[i].Next = result[i + 1];
			}
			
			Stack<ByteCode> agenda = new Stack<ByteCode>();
			
			int varCount = methodDef.Body.Variables.Count;
			
			var exceptionHandlerStarts = new List<ByteCode>(
                from eh in methodDef.Body.ExceptionHandlers
                select instrToByteCode[eh.HandlerStart]);
			
			// Add known states
			if(methodDef.Body.HasExceptionHandlers) {
				foreach(ExceptionHandler ex in methodDef.Body.ExceptionHandlers)
                {					
                    ByteCode handlerStart = instrToByteCode[ex.HandlerStart];
					handlerStart.StackBefore = new List<StackSlot>();
					handlerStart.VariablesBefore = VariableSlot.MakeFullState(varCount);

					if (ex.HandlerType == ExceptionHandlerType.Catch || ex.HandlerType == ExceptionHandlerType.Filter) {
						// Catch and Filter handlers start with the exeption on the stack
						ByteCode ldexception = new ByteCode() {
							Code = ILCode.Ldexception,
							Operand = ex.CatchType,
							PopCount = 0,
							PushCount = 1
						};
						ldexceptions[ex] = ldexception;
						handlerStart.StackBefore.Add(new StackSlot(ldexception));
					}
					
                    agenda.Push(handlerStart);
					
					if (ex.HandlerType == ExceptionHandlerType.Filter)
					{
						ByteCode filterStart = instrToByteCode[ex.FilterStart];
						filterStart.StackBefore = new List<StackSlot>();
						filterStart.VariablesBefore = VariableSlot.MakeFullState(varCount);
						
                        ByteCode ldexception = new ByteCode() {
							Code = ILCode.Ldexception,
							Operand = ex.CatchType,
							PopCount = 0,
							PushCount = 1
						};

						// TODO: ldexceptions[ex] = ldexception;
						filterStart.StackBefore.Add(new StackSlot(ldexception));
						agenda.Push(filterStart);
					}
				}
			}
			
			result[0].StackBefore = new List<StackSlot>();
			result[0].VariablesBefore = VariableSlot.MakeEmptyState(varCount);
			agenda.Push(result[0]);
			
			// Process agenda
			while(agenda.Count > 0) {
				ByteCode byteCode = agenda.Pop();
				
				// Calculate new stack
				List<StackSlot> newStack = StackSlot.CloneStack(byteCode.StackBefore, byteCode.PopCount);
				for (int i = 0; i < byteCode.PushCount; i++) {
					newStack.Add(new StackSlot(byteCode));
				}
				
				// Calculate new variable state
				VariableSlot[] newVariableState = VariableSlot.CloneVariableState(byteCode.VariablesBefore);
				if (byteCode.Code == ILCode.Stloc) {
					int varIndex = ((VariableReference)byteCode.Operand).Index;
					newVariableState[varIndex] = new VariableSlot(byteCode);
				}
				
				// After the leave, finally block might have touched the variables
				if (byteCode.Code == ILCode.Leave) {
					newVariableState = VariableSlot.MakeFullState(varCount);
				}
				
				// Find all successors
				List<ByteCode> branchTargets = new List<ByteCode>();
				if (!byteCode.Code.IsUnconditionalControlFlow()) {
					if (exceptionHandlerStarts.Contains(byteCode.Next)) {
						// Do not fall though down to exception handler
						// It is invalid IL as per ECMA-335 §12.4.2.8.1, but some obfuscators produce it						
					} else {
						branchTargets.Add(byteCode.Next);
					}
				}
				if (byteCode.Operand is Instruction[]) {
					foreach(Instruction inst in (Instruction[])byteCode.Operand) {
						ByteCode target = instrToByteCode[inst];
						branchTargets.Add(target);
						// The target of a branch must have label
						if (target.Label == null) {
							target.Label = new ILLabel()
                            {
                                Name = "IL_"+target.Offset.ToString("X2")
                            };
						}
					}
				} else if (byteCode.Operand is Instruction) {
					ByteCode target = instrToByteCode[(Instruction)byteCode.Operand];
					branchTargets.Add(target);
					// The target of a branch must have label
					if (target.Label == null) {
						target.Label = new ILLabel()
                        {
                            Name = "IL_" + target.Offset.ToString("X2")
                        };
					}
				}
				
				// Apply the state to successors
				foreach (ByteCode branchTarget in branchTargets) {
					if (branchTarget.StackBefore == null && branchTarget.VariablesBefore == null) {
						if (branchTargets.Count == 1) {
							branchTarget.StackBefore = newStack;
							branchTarget.VariablesBefore = newVariableState;
						} else {
							// Do not share data for several bytecodes
							branchTarget.StackBefore = StackSlot.CloneStack(newStack, 0);
							branchTarget.VariablesBefore = VariableSlot.CloneVariableState(newVariableState);
						}
						agenda.Push(branchTarget);
					} else {
						if (branchTarget.StackBefore.Count != newStack.Count) {
							throw new InvalidOperationException(
                                "Inconsistent stack size at " + byteCode.Code+" offset "+byteCode.Offset.ToString("X2")+".");
						}
						
						// Be careful not to change our new data - it might be reused for several branch targets.
						// In general, be careful that two bytecodes never share data structures.
						
						bool modified = false;
						
						// Merge stacks - modify the target
						for (int i = 0; i < newStack.Count; i++) {
							ByteCode[] oldPushedBy = branchTarget.StackBefore[i].PushedBy;
							ByteCode[] newPushedBy = Union(oldPushedBy, newStack[i].PushedBy);
							if (newPushedBy.Length > oldPushedBy.Length) {
								branchTarget.StackBefore[i] = new StackSlot(newPushedBy, null);
								modified = true;
							}
						}
						
						// Merge variables - modify the target
						for (int i = 0; i < newVariableState.Length; i++) {
							VariableSlot oldSlot = branchTarget.VariablesBefore[i];
							VariableSlot newSlot = newVariableState[i];
							// All can not be unioned further
							if (!oldSlot.StoredByAll) {
								if (newSlot.StoredByAll) {
									branchTarget.VariablesBefore[i] = newSlot;
									modified = true;
								} else {
									ByteCode[] oldStoredBy = oldSlot.StoredBy;
									ByteCode[] newStoredBy = Union(oldStoredBy, newSlot.StoredBy);
									if (newStoredBy.Length > oldStoredBy.Length) {
										branchTarget.VariablesBefore[i] = new VariableSlot(newStoredBy, false);
										modified = true;
									}
								}
							}
						}
						
						if (modified) {
							agenda.Push(branchTarget);
						}
					}
				}
			}
			
			// Occasionally the compilers or obfuscators generate unreachable code (which migt be intentonally invalid)
			// I belive it is safe to just remove it
			result.RemoveAll(b => b.StackBefore == null);
			
			// Genertate temporary variables to replace stack
			foreach(ByteCode byteCode in result) {
				int argIdx = 0;
				int popCount = byteCode.PopCount ?? byteCode.StackBefore.Count;
				for (int i = byteCode.StackBefore.Count - popCount; i < byteCode.StackBefore.Count; i++) {
					ILVariable tmpVar = new ILVariable() { Name = string.Format("arg_{0:X2}_{1}", byteCode.Offset, argIdx), IsGenerated = true };
					byteCode.StackBefore[i] = new StackSlot(byteCode.StackBefore[i].PushedBy, tmpVar);
					foreach(ByteCode pushedBy in byteCode.StackBefore[i].PushedBy) {
						if (pushedBy.StoreTo == null) {
							pushedBy.StoreTo = new List<ILVariable>(1);
						}
						pushedBy.StoreTo.Add(tmpVar);
					}
					argIdx++;
				}
			}
			
			// Try to use single temporary variable insted of several if possilbe (especially useful for dup)
			// This has to be done after all temporary variables are assigned so we know about all loads
			foreach(ByteCode byteCode in result) {
				if (byteCode.StoreTo != null && byteCode.StoreTo.Count > 1) {
					var locVars = byteCode.StoreTo;
					// For each of the variables, find the location where it is loaded - there should be preciesly one
					var loadedBy = locVars.Select(locVar => result.SelectMany(bc => bc.StackBefore).Single(s => s.LoadFrom == locVar)).ToList();
					// We now know that all the variables have a single load,
					// Let's make sure that they have also a single store - us
					if (loadedBy.All(slot => slot.PushedBy.Length == 1 && slot.PushedBy[0] == byteCode)) {
						// Great - we can reduce everything into single variable
						ILVariable tmpVar = new ILVariable() { Name = string.Format("expr_{0:X2}", byteCode.Offset), IsGenerated = true };
						byteCode.StoreTo = new List<ILVariable>() { tmpVar };
						foreach(ByteCode bc in result) {
							for (int i = 0; i < bc.StackBefore.Count; i++) {
								// Is it one of the variable to be merged?
								if (locVars.Contains(bc.StackBefore[i].LoadFrom)) {
									// Replace with the new temp variable
									bc.StackBefore[i] = new StackSlot(bc.StackBefore[i].PushedBy, tmpVar);
								}
							}
						}
					}
				}
			}
			
			// Split and convert the normal local variables
			ConvertLocalVariables(result);
			
			// Convert branch targets to labels
			foreach(ByteCode byteCode in result) {
				if (byteCode.Operand is Instruction[]) {
					List<ILLabel> newOperand = new List<ILLabel>();
					foreach(Instruction target in (Instruction[])byteCode.Operand) {
						newOperand.Add(instrToByteCode[target].Label);
					}
					byteCode.Operand = newOperand.ToArray();
				} else if (byteCode.Operand is Instruction) {
					byteCode.Operand = instrToByteCode[(Instruction)byteCode.Operand].Label;
				}
			}
			
			// Convert parameters to ILVariables
			ConvertParameters(result);
			
			return result;
		}
		
		/// <summary>
		/// If possible, separates local variables into several independent variables.
		/// It should undo any compilers merging.
		/// </summary>
		void ConvertLocalVariables(List<ByteCode> body)
		{
			if (optimize) {
				int varCount = methodDef.Body.Variables.Count;
				
				for(int variableIndex = 0; variableIndex < varCount; variableIndex++) {
					// Find all stores and loads for this variable
					List<ByteCode> stores = body.Where(b => b.Code == ILCode.Stloc && b.Operand is VariableDefinition && b.OperandAsVariable.Index == variableIndex).ToList();
					List<ByteCode> loads  = body.Where(b => (b.Code == ILCode.Ldloc || b.Code == ILCode.Ldloca) && b.Operand is VariableDefinition && b.OperandAsVariable.Index == variableIndex).ToList();
					TypeReference varType = methodDef.Body.Variables[variableIndex].VariableType;
					
					List<VariableInfo> newVars;
					
					bool isPinned = methodDef.Body.Variables[variableIndex].IsPinned;
					// If the variable is pinned, use single variable.
					// If any of the loads is from "all", use single variable
					// If any of the loads is ldloca, fallback to single variable as well
					if (isPinned || loads.Any(b => b.VariablesBefore[variableIndex].StoredByAll || b.Code == ILCode.Ldloca)) {
						newVars = new List<VariableInfo>(1) { new VariableInfo() {
							Variable = new ILVariable() {
								Name = "var_" + variableIndex,
								Type = isPinned ? ((PinnedType)varType).ElementType : varType,
								OriginalVariable = methodDef.Body.Variables[variableIndex]
							},
							Stores = stores,
							Loads  = loads
						}};
					} else {
						// Create a new variable for each store
						newVars = stores.Select(st => new VariableInfo() {
							Variable = new ILVariable() {
						    		Name = "var_" + variableIndex + "_" + st.Offset.ToString("X2"),
						    		Type = varType,
						    		OriginalVariable = methodDef.Body.Variables[variableIndex]
						    },
						    Stores = new List<ByteCode>() {st},
						    Loads  = new List<ByteCode>()
						}).ToList();
						
						// VB.NET uses the 'init' to allow use of uninitialized variables.
						// We do not really care about them too much - if the original variable
						// was uninitialized at that point it means that no store was called and
						// thus all our new variables must be uninitialized as well.
						// So it does not matter which one we load.
						
						// TODO: We should add explicit initialization so that C# code compiles.
						// Remember to handle cases where one path inits the variable, but other does not.
						
						// Add loads to the data structure; merge variables if necessary
						foreach(ByteCode load in loads) {
							ByteCode[] storedBy = load.VariablesBefore[variableIndex].StoredBy;
							if (storedBy.Length == 0) {
								// Load which always loads the default ('uninitialized') value
								// Create a dummy variable just for this load
								newVars.Add(new VariableInfo() {
									Variable = new ILVariable() {
								    		Name = "var_" + variableIndex + "_" + load.Offset.ToString("X2") + "_default",
								    		Type = varType,
								    		OriginalVariable = methodDef.Body.Variables[variableIndex]
								    },
								    Stores = new List<ByteCode>(),
								    Loads  = new List<ByteCode>() { load }
								});
							} else if (storedBy.Length == 1) {
								VariableInfo newVar = newVars.Single(v => v.Stores.Contains(storedBy[0]));
								newVar.Loads.Add(load);
							} else {
								List<VariableInfo> mergeVars = newVars.Where(v => v.Stores.Union(storedBy).Any()).ToList();
								VariableInfo mergedVar = new VariableInfo() {
									Variable = mergeVars[0].Variable,
									Stores = mergeVars.SelectMany(v => v.Stores).ToList(),
									Loads  = mergeVars.SelectMany(v => v.Loads).ToList()
								};
								mergedVar.Loads.Add(load);
								newVars = newVars.Except(mergeVars).ToList();
								newVars.Add(mergedVar);
							}
						}
					}
					
					// Set bytecode operands
					foreach(VariableInfo newVar in newVars) {
						foreach(ByteCode store in newVar.Stores) {
							store.Operand = newVar.Variable;
						}
						foreach(ByteCode load in newVar.Loads) {
							load.Operand = newVar.Variable;
						}
					}
				}
			} else {
				var variables = methodDef.Body.Variables.Select(v => new ILVariable() { Name = string.IsNullOrEmpty(v.Name) ?  "var_" + v.Index : v.Name, Type = v.VariableType, OriginalVariable = v }).ToList();
				foreach(ByteCode byteCode in body) {
					if (byteCode.Code == ILCode.Ldloc || byteCode.Code == ILCode.Stloc || byteCode.Code == ILCode.Ldloca) {
						int index = ((VariableDefinition)byteCode.Operand).Index;
						byteCode.Operand = variables[index];
					}
				}
			}
		}
		
		void ConvertParameters(List<ByteCode> body)
		{
			ILVariable thisParameter = null;
			if (methodDef.HasThis) {
				TypeReference type = methodDef.DeclaringType;
				thisParameter = new ILVariable();
				thisParameter.Type = type.IsValueType ? new ByReferenceType(type) : type;
				thisParameter.Name = "this";
				thisParameter.OriginalParameter = methodDef.Body.ThisParameter;
			}
			foreach (ParameterDefinition p in methodDef.Parameters) {
				this.parameterList.Add(new ILVariable { Type = p.ParameterType, Name = p.Name, OriginalParameter = p });
			}
			foreach (ByteCode byteCode in body) {
				ParameterDefinition p;
				switch (byteCode.Code) {
					case ILCode.__Ldarg:
						p = (ParameterDefinition)byteCode.Operand;
						byteCode.Code = ILCode.Ldloc;
						byteCode.Operand = p.Index < 0 ? thisParameter : this.parameterList[p.Index];
						break;
					case ILCode.__Starg:
						p = (ParameterDefinition)byteCode.Operand;
						byteCode.Code = ILCode.Stloc;
                        byteCode.Operand = p.Index < 0 ? thisParameter : this.parameterList[p.Index];
						break;
					case ILCode.__Ldarga:
						p = (ParameterDefinition)byteCode.Operand;
						byteCode.Code = ILCode.Ldloca;
                        byteCode.Operand = p.Index < 0 ? thisParameter : this.parameterList[p.Index];
						break;
				}
			}
			if (thisParameter != null)
				this.parameterList.Add(thisParameter);
		}

        static T[] Union<T>(T[] a, T[] b)
        {
            if (a.Length == 0)
                return b;
            if (b.Length == 0)
                return a;
            if (a.Length == 1 && b.Length == 1 && a[0].Equals(b[0]))
                return a;
            return Enumerable.Union(a, b).ToArray();
        }
    }
}