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
	public sealed partial class ILMethodAstBuilder
	{
		/// <summary> Immutable </summary>
		private sealed class VariableSlot
		{			
			public readonly ByteCode[] StoredBy;    // One of those
			public readonly bool       StoredByAll; // Overestimate which is useful for exceptional control flow.
			
			public VariableSlot(ByteCode[] storedBy, bool storedByAll)
			{
				this.StoredBy = storedBy;
				this.StoredByAll = storedByAll;
			}
			
			public VariableSlot(ByteCode storedBy)
			{
				this.StoredBy = new[] { storedBy };
				this.StoredByAll = false;
			}
			
			public static VariableSlot[] CloneVariableState(VariableSlot[] state)
			{
				VariableSlot[] clone = new VariableSlot[state.Length];
				for (int i = 0; i < clone.Length; i++) {
					clone[i] = state[i];
				}
				return clone;
			}
			
			public static VariableSlot[] MakeEmptyState(int varCount)
			{
				VariableSlot[] emptyVariableState = new VariableSlot[varCount];
				for (int i = 0; i < emptyVariableState.Length; i++) {
					emptyVariableState[i] = new VariableSlot(Empty.Array<ByteCode>(), false);
				}
				return emptyVariableState;
			}
			
			public static VariableSlot[] MakeFullState(int varCount)
			{
				VariableSlot[] unknownVariableState = new VariableSlot[varCount];
				for (int i = 0; i < unknownVariableState.Length; i++) {
					unknownVariableState[i] = new VariableSlot(Empty.Array<ByteCode>(), true);
				}
				return unknownVariableState;
			}
		}
		
		private sealed class ByteCode
		{
			public ILLabel Label;      // Non-null only if needed
			
            public int Offset;
			
            public int EndOffset;
			
            public ILCode Code;

			public object Operand;

            /// <summary> Null means pop all. </summary>
			public int? PopCount;

			public int PushCount;

			public ByteCode Next;

            /// <summary> Non-null only if needed. </summary>
			public Instruction[] Prefixes;

            /// <summary> Unique per bytecode; not shared. </summary>
			public List<StackSlot> StackBefore;

            /// <summary> Store result of instruction to those AST variables. </summary>
			public List<ILVariable> StoreTo;

            /// <summary> Unique per bytecode; not shared. </summary>
			public VariableSlot[] VariablesBefore;
			
			public VariableDefinition OperandAsVariable { get { return (VariableDefinition)this.Operand; } }
			
			public override string ToString()
			{
				StringBuilder sb = new StringBuilder();
				
				// Label
                sb.Append("IL_");
				sb.Append(this.Offset.ToString("X2"));
				sb.Append(':');
				if (this.Label != null)
					sb.Append('*');
				
				// Name
				sb.Append(' ');
				if (this.Prefixes != null) {
					foreach (var prefix in this.Prefixes) {
						sb.Append(prefix.OpCode.Name);
						sb.Append(' ');
					}
				}
				sb.Append(this.Code.GetName());
				
				if (this.Operand != null) {
					sb.Append(' ');
					if (this.Operand is Instruction) {
						sb.Append("IL_" + ((Instruction)this.Operand).Offset.ToString("X2"));
					} else if (this.Operand is Instruction[]) {
						foreach(Instruction inst in (Instruction[])this.Operand) {
							sb.Append("IL_" + inst.Offset.ToString("X2"));
							sb.Append(" ");
						}
					} else if (this.Operand is ILLabel) {
						sb.Append(((ILLabel)this.Operand).Name);
					} else if (this.Operand is ILLabel[]) {
						foreach(ILLabel label in (ILLabel[])this.Operand) {
							sb.Append(label.Name);
							sb.Append(" ");
						}
					} else {
						sb.Append(this.Operand.ToString());
					}
				}
				
				if (this.StackBefore != null) {
					sb.Append(" StackBefore={");
					bool first = true;
					foreach (StackSlot slot in this.StackBefore) {
						if (!first) sb.Append(",");
						bool first2 = true;
						foreach(ByteCode pushedBy in slot.PushedBy) {
							if (!first2) sb.Append("|");
							sb.AppendFormat("IL_{0:X2}", pushedBy.Offset);
							first2 = false;
						}
						first = false;
					}
					sb.Append("}");
				}
				
				if (this.StoreTo != null && this.StoreTo.Count > 0) {
					sb.Append(" StoreTo={");
					bool first = true;
					foreach (ILVariable stackVar in this.StoreTo) {
						if (!first) sb.Append(",");
						sb.Append(stackVar.Name);
						first = false;
					}
					sb.Append("}");
				}
				
				if (this.VariablesBefore != null) {
					sb.Append(" VarsBefore={");
					bool first = true;
					foreach (VariableSlot varSlot in this.VariablesBefore) {
						if (!first) sb.Append(",");
						if (varSlot.StoredByAll) {
							sb.Append("*");
						} else if (varSlot.StoredBy.Length == 0) {
							sb.Append("_");
						} else {
							bool first2 = true;
							foreach (ByteCode storedBy in varSlot.StoredBy) {
								if (!first2) sb.Append("|");
								sb.AppendFormat("IL_{0:X2}", storedBy.Offset);
								first2 = false;
							}
						}
						first = false;
					}
					sb.Append("}");
				}
				
				return sb.ToString();
			}
		}

        private sealed class VariableInfo
        {
            public ILVariable Variable;
            public List<ByteCode> Stores;
            public List<ByteCode> Loads;
        }

		readonly MethodDefinition methodDef;
		readonly bool optimize;
		
		// Virtual instructions to load exception on stack
		readonly Dictionary<ExceptionHandler, ByteCode> ldexceptions = new Dictionary<ExceptionHandler, ILMethodAstBuilder.ByteCode>();

        readonly List<ILVariable> parameterList = new List<ILVariable>();

        private ILMethodAstBuilder(MethodDefinition methodDef, bool optimize)
        {
            this.methodDef = methodDef;
            this.optimize = optimize;
        }

		public static ILMethodAst Build(MethodDefinition methodDef, bool optimize)
		{
            if (methodDef.Body.Instructions.Count == 0)
            {
                return ILMethodAst.Empty;
            }
            else
            {
                var ilastBuilder = new ILMethodAstBuilder(methodDef, optimize);
                List<ByteCode> body = ilastBuilder.StackAnalysis(methodDef);
                List<ILNode> ast = ilastBuilder.ConvertToAst(body, new HashSet<ExceptionHandler>(methodDef.Body.ExceptionHandlers));

                return new ILMethodAst(ilastBuilder.parameterList, ast);
            }
        }
    }
}