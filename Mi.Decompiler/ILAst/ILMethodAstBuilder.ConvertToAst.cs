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
		List<ILNode> ConvertToAst(List<ByteCode> body, HashSet<ExceptionHandler> ehs)
		{
			List<ILNode> ast = new List<ILNode>();
			
			while (ehs.Any()) {
				ILTryCatchBlock tryCatchBlock = new ILTryCatchBlock();
				
				// Find the first and widest scope
				int tryStart = ehs.Min(eh => eh.TryStart.Offset);
				int tryEnd   = ehs.Where(eh => eh.TryStart.Offset == tryStart).Max(eh => eh.TryEnd.Offset);
				var handlers = ehs.Where(eh => eh.TryStart.Offset == tryStart && eh.TryEnd.Offset == tryEnd).OrderBy(eh => eh.TryStart.Offset).ToList();
				
				// Remember that any part of the body migt have been removed due to unreachability
				
				// Cut all instructions up to the try block
				{
					int tryStartIdx = 0;
					while (tryStartIdx < body.Count && body[tryStartIdx].Offset < tryStart) tryStartIdx++;
                    ast.AddRange(ConvertToAst(CutRange(body, 0, tryStartIdx)));
				}
				
				// Cut the try block
				{
					HashSet<ExceptionHandler> nestedEHs = new HashSet<ExceptionHandler>(ehs.Where(eh => (tryStart <= eh.TryStart.Offset && eh.TryEnd.Offset < tryEnd) || (tryStart < eh.TryStart.Offset && eh.TryEnd.Offset <= tryEnd)));
					ehs.ExceptWith(nestedEHs);
					int tryEndIdx = 0;
					while (tryEndIdx < body.Count && body[tryEndIdx].Offset < tryEnd) tryEndIdx++;
                    tryCatchBlock.TryBlock = new ILBlock(ConvertToAst(CutRange(body, 0, tryEndIdx), nestedEHs));
				}
				
				// Cut all handlers
				tryCatchBlock.CatchBlocks = new List<ILTryCatchBlock.CatchBlock>();
				foreach(ExceptionHandler eh in handlers) {
					int handlerEndOffset = eh.HandlerEnd == null ? methodDef.Body.CodeSize : eh.HandlerEnd.Offset;
					int startIdx = 0;
					while (startIdx < body.Count && body[startIdx].Offset < eh.HandlerStart.Offset) startIdx++;
					int endIdx = 0;
					while (endIdx < body.Count && body[endIdx].Offset < handlerEndOffset) endIdx++;
					HashSet<ExceptionHandler> nestedEHs = new HashSet<ExceptionHandler>(ehs.Where(e => (eh.HandlerStart.Offset <= e.TryStart.Offset && e.TryEnd.Offset < handlerEndOffset) || (eh.HandlerStart.Offset < e.TryStart.Offset && e.TryEnd.Offset <= handlerEndOffset)));
					ehs.ExceptWith(nestedEHs);
                    List<ILNode> handlerAst = ConvertToAst(CutRange(body, startIdx, endIdx - startIdx), nestedEHs);
					if (eh.HandlerType == ExceptionHandlerType.Catch) {
						ILTryCatchBlock.CatchBlock catchBlock = new ILTryCatchBlock.CatchBlock() {
							ExceptionType = eh.CatchType,
							Body = handlerAst
						};
						// Handle the automatically pushed exception on the stack
						ByteCode ldexception = ldexceptions[eh];
						if (ldexception.StoreTo.Count == 0) {
							throw new Exception("Exception should be consumed by something");
						} else if (ldexception.StoreTo.Count == 1) {
							ILExpression first = catchBlock.Body[0] as ILExpression;
							if (first != null &&
							    first.Code == ILCode.Pop &&
							    first.Arguments[0].Code == ILCode.Ldloc &&
							    first.Arguments[0].Operand == ldexception.StoreTo[0])
							{
								// The exception is just poped - optimize it all away;
								catchBlock.ExceptionVariable = null;
								catchBlock.Body.RemoveAt(0);
							} else {
								catchBlock.ExceptionVariable = ldexception.StoreTo[0];
							}
						} else {
							ILVariable exTemp = new ILVariable() { Name = "ex_" + eh.HandlerStart.Offset.ToString("X2"), IsGenerated = true };
							catchBlock.ExceptionVariable = exTemp;
							foreach(ILVariable storeTo in ldexception.StoreTo) {
								catchBlock.Body.Insert(0, new ILExpression(ILCode.Stloc, storeTo, new ILExpression(ILCode.Ldloc, exTemp)));
							}
						}
						tryCatchBlock.CatchBlocks.Add(catchBlock);
					} else if (eh.HandlerType == ExceptionHandlerType.Finally) {
						tryCatchBlock.FinallyBlock = new ILBlock(handlerAst);
					} else if (eh.HandlerType == ExceptionHandlerType.Fault) {
						tryCatchBlock.FaultBlock = new ILBlock(handlerAst);
					} else {
						// TODO: ExceptionHandlerType.Filter
					}
				}
				
				ehs.ExceptWith(handlers);
				
				ast.Add(tryCatchBlock);
			}
			
			// Add whatever is left
			ast.AddRange(ConvertToAst(body));
			
			return ast;
		}
		
		List<ILNode> ConvertToAst(List<ByteCode> body)
		{
			List<ILNode> ast = new List<ILNode>();
			
			// Convert stack-based IL code to ILAst tree
			foreach(ByteCode byteCode in body) {
				ILRange ilRange = new ILRange() { From = byteCode.Offset, To = byteCode.EndOffset };
				
				if (byteCode.StackBefore == null) {
					// Unreachable code
					continue;
				}
				
				ILExpression expr = new ILExpression(byteCode.Code, byteCode.Operand);
				expr.ILRanges.Add(ilRange);
				if (byteCode.Prefixes != null && byteCode.Prefixes.Length > 0) {
					ILExpressionPrefix[] prefixes = new ILExpressionPrefix[byteCode.Prefixes.Length];
					for (int i = 0; i < prefixes.Length; i++) {
						prefixes[i] = new ILExpressionPrefix((ILCode)byteCode.Prefixes[i].OpCode.Code, byteCode.Prefixes[i].Operand);
					}
					expr.Prefixes = prefixes;
				}
				
				// Label for this instruction
				if (byteCode.Label != null) {
					ast.Add(byteCode.Label);
				}
				
				// Reference arguments using temporary variables
				int popCount = byteCode.PopCount ?? byteCode.StackBefore.Count;
				for (int i = byteCode.StackBefore.Count - popCount; i < byteCode.StackBefore.Count; i++) {
					StackSlot slot = byteCode.StackBefore[i];
					expr.Arguments.Add(new ILExpression(ILCode.Ldloc, slot.LoadFrom));
				}
			
				// Store the result to temporary variable(s) if needed
				if (byteCode.StoreTo == null || byteCode.StoreTo.Count == 0) {
					ast.Add(expr);
				} else if (byteCode.StoreTo.Count == 1) {
					ast.Add(new ILExpression(ILCode.Stloc, byteCode.StoreTo[0], expr));
				} else {
					ILVariable tmpVar = new ILVariable() { Name = "expr_" + byteCode.Offset.ToString("X2"), IsGenerated = true };
					ast.Add(new ILExpression(ILCode.Stloc, tmpVar, expr));
					foreach(ILVariable storeTo in byteCode.StoreTo.AsEnumerable().Reverse()) {
						ast.Add(new ILExpression(ILCode.Stloc, storeTo, new ILExpression(ILCode.Ldloc, tmpVar)));
					}
				}
			}
			
			return ast;
		}

        static List<T> CutRange<T>(List<T> list, int start, int count)
        {
            List<T> ret = new List<T>(count);
            for (int i = 0; i < count; i++)
            {
                ret.Add(list[start + i]);
            }
            list.RemoveRange(start, count);
            return ret;
        }
    }
}