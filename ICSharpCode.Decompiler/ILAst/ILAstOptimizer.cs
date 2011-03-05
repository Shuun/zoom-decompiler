using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using ICSharpCode.Decompiler.FlowAnalysis;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.CSharp;

namespace ICSharpCode.Decompiler.ILAst
{
	public enum ILAstOptimizationStep
	{
		ReduceBranchInstructionSet,
		SplitToMovableBlocks,
		PeepholeOptimizations,
		FindLoops,
		FindConditions,
		FlattenNestedMovableBlocks,
		GotoRemoval,
		DuplicateReturns,
		FlattenIfStatements,
		PeepholeTransforms,
		TypeInference,
		None
	}
	
	public class ILAstOptimizer
	{
		int nextLabelIndex = 0;
		
		Dictionary<ILLabel, ControlFlowNode> labelToCfNode = new Dictionary<ILLabel, ControlFlowNode>();
		
		public void Optimize(DecompilerContext context, ILBlock method, ILAstOptimizationStep abortBeforeStep = ILAstOptimizationStep.None)
		{
			if (abortBeforeStep == ILAstOptimizationStep.ReduceBranchInstructionSet) return;
			foreach(ILBlock block in method.GetSelfAndChildrenRecursive<ILBlock>().ToList()) {
				ReduceBranchInstructionSet(block);
			}
			
			if (abortBeforeStep == ILAstOptimizationStep.SplitToMovableBlocks) return;
			foreach(ILBlock block in method.GetSelfAndChildrenRecursive<ILBlock>().ToList()) {
				SplitToBasicBlocks(block);
			}
			
			if (abortBeforeStep == ILAstOptimizationStep.PeepholeOptimizations) return;
			AnalyseLabels(method);
			foreach(ILBlock block in method.GetSelfAndChildrenRecursive<ILBlock>().ToList()) {
				PeepholeOptimizations(block);
			}
			
			if (abortBeforeStep == ILAstOptimizationStep.FindLoops) return;
			foreach(ILBlock block in method.GetSelfAndChildrenRecursive<ILBlock>().ToList()) {
				ControlFlowGraph graph;
				graph = BuildGraph(block.Body, (ILLabel)block.EntryGoto.Operand);
				graph.ComputeDominance(context.CancellationToken);
				graph.ComputeDominanceFrontier();
				block.Body = FindLoops(new HashSet<ControlFlowNode>(graph.Nodes.Skip(3)), graph.EntryPoint, false);
			}
			
			if (abortBeforeStep == ILAstOptimizationStep.FindConditions) return;
			foreach(ILBlock block in method.GetSelfAndChildrenRecursive<ILBlock>().ToList()) {
				ControlFlowGraph graph;
				graph = BuildGraph(block.Body, (ILLabel)block.EntryGoto.Operand);
				// TODO: Fix
				if (graph == null)
					continue;
				graph.ComputeDominance(context.CancellationToken);
				graph.ComputeDominanceFrontier();
				block.Body = FindConditions(new HashSet<ControlFlowNode>(graph.Nodes.Skip(3)), graph.EntryPoint);
			}
			
			if (abortBeforeStep == ILAstOptimizationStep.FlattenNestedMovableBlocks) return;
			FlattenBasicBlocks(method);
			
			if (abortBeforeStep == ILAstOptimizationStep.GotoRemoval) return;
			new GotoRemoval().RemoveGotos(method);
			
			if (abortBeforeStep == ILAstOptimizationStep.DuplicateReturns) return;
			DuplicateReturnStatements(method);
			
			if (abortBeforeStep == ILAstOptimizationStep.FlattenIfStatements) return;
			FlattenIfStatements(method);
			
			if (abortBeforeStep == ILAstOptimizationStep.PeepholeTransforms) return;
			PeepholeTransforms.Run(context, method);
			
			if (abortBeforeStep == ILAstOptimizationStep.TypeInference) return;
			TypeAnalysis.Run(context, method);
			
			GotoRemoval.RemoveRedundantCode(method);
		}
		
		/// <summary>
		/// Reduces the branch codes to just br and brtrue.
		/// Moves ILRanges to the branch argument
		/// </summary>
		void ReduceBranchInstructionSet(ILBlock block)
		{
			for (int i = 0; i < block.Body.Count; i++) {
				ILExpression expr = block.Body[i] as ILExpression;
				if (expr != null && expr.Prefixes == null) {
					switch(expr.Code) {
						case ILCode.Brtrue:
							expr.Arguments.Single().ILRanges.AddRange(expr.ILRanges);
							expr.ILRanges.Clear();
							continue;
						case ILCode.__Brfalse:  block.Body[i] = new ILExpression(ILCode.Brtrue, expr.Operand, new ILExpression(ILCode.LogicNot, null, expr.Arguments.Single())); break;
						case ILCode.__Beq:      block.Body[i] = new ILExpression(ILCode.Brtrue, expr.Operand, new ILExpression(ILCode.Ceq, null, expr.Arguments)); break;
						case ILCode.__Bne_Un:   block.Body[i] = new ILExpression(ILCode.Brtrue, expr.Operand, new ILExpression(ILCode.LogicNot, null, new ILExpression(ILCode.Ceq, null, expr.Arguments))); break;
						case ILCode.__Bgt:      block.Body[i] = new ILExpression(ILCode.Brtrue, expr.Operand, new ILExpression(ILCode.Cgt, null, expr.Arguments)); break;
						case ILCode.__Bgt_Un:   block.Body[i] = new ILExpression(ILCode.Brtrue, expr.Operand, new ILExpression(ILCode.Cgt_Un, null, expr.Arguments)); break;
						case ILCode.__Ble:      block.Body[i] = new ILExpression(ILCode.Brtrue, expr.Operand, new ILExpression(ILCode.LogicNot, null, new ILExpression(ILCode.Cgt, null, expr.Arguments))); break;
						case ILCode.__Ble_Un:   block.Body[i] = new ILExpression(ILCode.Brtrue, expr.Operand, new ILExpression(ILCode.LogicNot, null, new ILExpression(ILCode.Cgt_Un, null, expr.Arguments))); break;
						case ILCode.__Blt:      block.Body[i] = new ILExpression(ILCode.Brtrue, expr.Operand, new ILExpression(ILCode.Clt, null, expr.Arguments)); break;
						case ILCode.__Blt_Un:   block.Body[i] = new ILExpression(ILCode.Brtrue, expr.Operand, new ILExpression(ILCode.Clt_Un, null, expr.Arguments)); break;
						case ILCode.__Bge:	    block.Body[i] = new ILExpression(ILCode.Brtrue, expr.Operand, new ILExpression(ILCode.LogicNot, null, new ILExpression(ILCode.Clt, null, expr.Arguments))); break;
						case ILCode.__Bge_Un:   block.Body[i] = new ILExpression(ILCode.Brtrue, expr.Operand, new ILExpression(ILCode.LogicNot, null, new ILExpression(ILCode.Clt_Un, null, expr.Arguments))); break;
						default:
							continue;
					}
					((ILExpression)block.Body[i]).Arguments.Single().ILRanges.AddRange(expr.ILRanges);
				}
			}
		}
		
		/// <summary>
		/// Group input into a set of blocks that can be later arbitraliby schufled.
		/// The method adds necessary branches to make control flow between blocks
		/// explicit and thus order independent.
		/// </summary>
		void SplitToBasicBlocks(ILBlock block)
		{
			// Remve no-ops
			// TODO: Assign the no-op range to someting
			block.Body = block.Body.Where(n => !(n is ILExpression && ((ILExpression)n).Code == ILCode.Nop)).ToList();
			
			List<ILNode> basicBlocks = new List<ILNode>();
			
			ILBasicBlock basicBlock = new ILBasicBlock() {
				EntryLabel = new ILLabel() { Name = "Block_" + (nextLabelIndex++) }
			};
			basicBlocks.Add(basicBlock);
			block.EntryGoto = new ILExpression(ILCode.Br, basicBlock.EntryLabel);
			
			if (block.Body.Count > 0) {
				basicBlock.Body.Add(block.Body[0]);
				
				for (int i = 1; i < block.Body.Count; i++) {
					ILNode lastNode = block.Body[i - 1];
					ILNode currNode = block.Body[i];
					
					// Insert split
					if (currNode is ILLabel ||
					    lastNode is ILTryCatchBlock ||
					    currNode is ILTryCatchBlock ||
					    (lastNode is ILExpression) && ((ILExpression)lastNode).IsBranch() ||
					    (currNode is ILExpression) && (((ILExpression)currNode).IsBranch() && ((ILExpression)currNode).Code.CanFallThough() && basicBlock.Body.Count > 0))
					{
						ILBasicBlock lastBlock = basicBlock;
						basicBlock = new ILBasicBlock();
						basicBlocks.Add(basicBlock);
						
						if (currNode is ILLabel) {
							// Insert as entry label
							basicBlock.EntryLabel = (ILLabel)currNode;
						} else {
							basicBlock.EntryLabel = new ILLabel() { Name = "Block_" + (nextLabelIndex++) };
							basicBlock.Body.Add(currNode);
						}
						
						// Explicit branch from one block to other
						// (unless the last expression was unconditional branch)
						if (!(lastNode is ILExpression) || ((ILExpression)lastNode).Code.CanFallThough()) {
							lastBlock.FallthoughGoto = new ILExpression(ILCode.Br, basicBlock.EntryLabel);
						}
					} else {
						basicBlock.Body.Add(currNode);						
					}
				}
			}
			
			foreach (ILBasicBlock bb in basicBlocks) {
				if (bb.Body.Count > 0 &&
				    bb.Body.Last() is ILExpression &&
				    ((ILExpression)bb.Body.Last()).Code == ILCode.Br)
				{
					Debug.Assert(bb.FallthoughGoto == null);
					bb.FallthoughGoto = (ILExpression)bb.Body.Last();
					bb.Body.RemoveAt(bb.Body.Count - 1);
				}
			}
			
			block.Body = basicBlocks;
			return;
		}
		
		Dictionary<ILLabel, int> labelGlobalRefCount;
		Dictionary<ILLabel, ILBasicBlock> labelToBasicBlock;
		
		void AnalyseLabels(ILBlock method)
		{
			labelGlobalRefCount = new Dictionary<ILLabel, int>();
			foreach(ILLabel target in method.GetSelfAndChildrenRecursive<ILExpression>().SelectMany(e => e.GetBranchTargets())) {
				if (!labelGlobalRefCount.ContainsKey(target))
					labelGlobalRefCount[target] = 0;
				labelGlobalRefCount[target]++;
			}
			
			labelToBasicBlock = new Dictionary<ILLabel, ILBasicBlock>();
			foreach(ILBasicBlock bb in method.GetSelfAndChildrenRecursive<ILBasicBlock>()) {
				foreach(ILLabel label in bb.GetChildren().OfType<ILLabel>()) {
					labelToBasicBlock[label] = bb;
				}
			}
		}
		
		void PeepholeOptimizations(ILBlock block)
		{
			bool modified;
			do {
				modified = false;
				for (int i = 0; i < block.Body.Count;) {
					ILBasicBlock bb = (ILBasicBlock)block.Body[i];
					if (TrySimplifyShortCircuit(block.Body, bb)) {
						modified = true;
						continue;
					} 
					if (TrySimplifyTernaryOperator(block.Body, bb)) {
						modified = true;
						continue;
					}
					i++;
				}
			} while(modified);
		}
		
		bool IsStloc(ILBasicBlock bb, ref ILVariable locVar, ref ILExpression val, ref ILLabel fallLabel)
		{
			if (bb.Body.Count == 1) {
				ILExpression expr;
				if (bb.Body[0].Match(ILCode.Stloc, out expr)) {
					locVar = (ILVariable)expr.Operand;
					val    = expr.Arguments[0];
					fallLabel = (ILLabel)bb.FallthoughGoto.Operand;
					return true;
				}
			}
			return false;
		}
		
		// scope is modified if successful
		bool TrySimplifyTernaryOperator(List<ILNode> scope, ILBasicBlock head)
		{
			Debug.Assert(scope.Contains(head));
			
			ILExpression condExpr = null;
			ILLabel trueLabel = null;
			ILLabel falseLabel = null;
			ILVariable trueLocVar = null;
			ILExpression trueExpr = null;
			ILLabel trueFall = null;
			ILVariable falseLocVar = null;
			ILExpression falseExpr = null;
			ILLabel falseFall = null;
			
			if(head.MatchBrTure(out condExpr, out trueLabel, out falseLabel) &&
			   labelGlobalRefCount[trueLabel] == 1 &&
			   labelGlobalRefCount[falseLabel] == 1 &&
			   IsStloc(labelToBasicBlock[trueLabel], ref trueLocVar, ref trueExpr, ref trueFall) &&
			   IsStloc(labelToBasicBlock[falseLabel], ref falseLocVar, ref falseExpr, ref falseFall) &&
			   trueLocVar == falseLocVar &&
			   trueFall == falseFall)
			{
				// Create the ternary expression
				head.Body = new List<ILNode>() { new ILExpression(ILCode.Stloc, trueLocVar, new ILExpression(ILCode.TernaryOp, null, condExpr, trueExpr, falseExpr)) };
				head.FallthoughGoto = new ILExpression(ILCode.Br, trueFall);
				
				// Remove the old basic blocks
				scope.RemoveOrThrow(labelToBasicBlock[trueLabel]);
				scope.RemoveOrThrow(labelToBasicBlock[falseLabel]);
				labelToBasicBlock.RemoveOrThrow(trueLabel);
				labelToBasicBlock.RemoveOrThrow(falseLabel);
				labelGlobalRefCount.RemoveOrThrow(trueLabel);
				labelGlobalRefCount.RemoveOrThrow(falseLabel);
				
				return true;
			}
			return false;
		}
		
		// scope is modified if successful
		bool TrySimplifyShortCircuit(List<ILNode> scope, ILBasicBlock head)
		{
			Debug.Assert(scope.Contains(head));
			
			ILExpression condExpr;
			ILLabel trueLabel;
			ILLabel falseLabel;
			if(head.MatchBrTure(out condExpr, out trueLabel, out falseLabel)) {
				for (int pass = 0; pass < 2; pass++) {
					
					// On the second pass, swap labels and negate expression of the first branch
					// It is slightly ugly, but much better then copy-pasting this whole block
					ILLabel nextLabel   = (pass == 0) ? trueLabel  : falseLabel;
					ILLabel otherLablel = (pass == 0) ? falseLabel : trueLabel;
					bool    negate      = (pass == 1);
					
					ILBasicBlock nextBasicBlock = labelToBasicBlock[nextLabel];
					ILExpression nextCondExpr;
					ILLabel nextTrueLablel;
					ILLabel nextFalseLabel;
					if (scope.Contains(nextBasicBlock) &&
					    nextBasicBlock != head &&
					    labelGlobalRefCount[nextBasicBlock.EntryLabel] == 1 &&
					    nextBasicBlock.MatchBrTure(out nextCondExpr, out nextTrueLablel, out nextFalseLabel) &&
					    (otherLablel == nextFalseLabel || otherLablel == nextTrueLablel))
					{
						// Create short cicuit branch
						if (otherLablel == nextFalseLabel) {
							head.Body[0] = new ILExpression(ILCode.Brtrue, nextTrueLablel, new ILExpression(ILCode.LogicAnd, null, negate ? new ILExpression(ILCode.LogicNot, null, condExpr) : condExpr, nextCondExpr));
						} else {
							head.Body[0] = new ILExpression(ILCode.Brtrue, nextTrueLablel, new ILExpression(ILCode.LogicOr, null, negate ? condExpr : new ILExpression(ILCode.LogicNot, null, condExpr), nextCondExpr));
						}
						head.FallthoughGoto = new ILExpression(ILCode.Br, nextFalseLabel);
						
						// Remove the inlined branch from scope
						labelGlobalRefCount.RemoveOrThrow(nextBasicBlock.EntryLabel);
						labelToBasicBlock.RemoveOrThrow(nextBasicBlock.EntryLabel);
						scope.RemoveOrThrow(nextBasicBlock);
						
						return true;
					}
				}
			}
			return false;
		}
		
		void DuplicateReturnStatements(ILBlock method)
		{
			Dictionary<ILLabel, ILNode> nextSibling = new Dictionary<ILLabel, ILNode>();
			
			// Build navigation data
			foreach(ILBlock block in method.GetSelfAndChildrenRecursive<ILBlock>()) {
				for (int i = 0; i < block.Body.Count - 1; i++) {
					ILLabel curr = block.Body[i] as ILLabel;
					if (curr != null) {
						nextSibling[curr] = block.Body[i + 1];
					}
				}
			}
			
			// Duplicate returns
			foreach(ILBlock block in method.GetSelfAndChildrenRecursive<ILBlock>()) {
				for (int i = 0; i < block.Body.Count; i++) {
					ILLabel targetLabel;
					if (block.Body[i].Match(ILCode.Br, out targetLabel) ||
					    block.Body[i].Match(ILCode.Leave, out targetLabel))
					{
						// Skip extra labels
						while(nextSibling.ContainsKey(targetLabel) && nextSibling[targetLabel] is ILLabel) {
							targetLabel = (ILLabel)nextSibling[targetLabel];
						}
						
						// Inline return statement
						ILNode target;
						ILExpression retExpr;
						if (nextSibling.TryGetValue(targetLabel, out target) &&
						    target.Match(ILCode.Ret, out retExpr))
						{
							ILVariable locVar;
							object constValue;
							if (retExpr.Arguments.Count == 0) {
								block.Body[i] = new ILExpression(ILCode.Ret, null);
							} else if (retExpr.Arguments.Single().Match(ILCode.Ldloc, out locVar)) {
								block.Body[i] = new ILExpression(ILCode.Ret, null, new ILExpression(ILCode.Ldloc, locVar));
							} else if (retExpr.Arguments.Single().Match(ILCode.Ldc_I4, out constValue)) {
								block.Body[i] = new ILExpression(ILCode.Ret, null, new ILExpression(ILCode.Ldc_I4, constValue));
							}
						}
					}
				}
			}
		}
		
		ControlFlowGraph BuildGraph(List<ILNode> nodes, ILLabel entryLabel)
		{
			int index = 0;
			List<ControlFlowNode> cfNodes = new List<ControlFlowNode>();
			ControlFlowNode entryPoint = new ControlFlowNode(index++, 0, ControlFlowNodeType.EntryPoint);
			cfNodes.Add(entryPoint);
			ControlFlowNode regularExit = new ControlFlowNode(index++, -1, ControlFlowNodeType.RegularExit);
			cfNodes.Add(regularExit);
			ControlFlowNode exceptionalExit = new ControlFlowNode(index++, -1, ControlFlowNodeType.ExceptionalExit);
			cfNodes.Add(exceptionalExit);
			
			// Create graph nodes
			labelToCfNode = new Dictionary<ILLabel, ControlFlowNode>();
			Dictionary<ILNode, ControlFlowNode> astNodeToCfNode = new Dictionary<ILNode, ControlFlowNode>();
			foreach(ILNode node in nodes) {
				ControlFlowNode cfNode = new ControlFlowNode(index++, -1, ControlFlowNodeType.Normal);
				cfNodes.Add(cfNode);
				astNodeToCfNode[node] = cfNode;
				cfNode.UserData = node;
				
				// Find all contained labels
				foreach(ILLabel label in node.GetSelfAndChildrenRecursive<ILLabel>()) {
					labelToCfNode[label] = cfNode;
				}
			}
			
			if (!labelToCfNode.ContainsKey(entryLabel))
				return null;
			
			// Entry endge
			ControlFlowNode entryNode = labelToCfNode[entryLabel];
			ControlFlowEdge entryEdge = new ControlFlowEdge(entryPoint, entryNode, JumpType.Normal);
			entryPoint.Outgoing.Add(entryEdge);
			entryNode.Incoming.Add(entryEdge);
			
			// Create edges
			foreach(ILNode node in nodes) {
				ControlFlowNode source = astNodeToCfNode[node];
				
				// Find all branches
				foreach(ILExpression child in node.GetSelfAndChildrenRecursive<ILExpression>()) {
					IEnumerable<ILLabel> targets = child.GetBranchTargets();
					if (targets != null) {
						foreach(ILLabel target in targets) {
							ControlFlowNode destination;
							// Labels which are out of out scope will not be int the collection
							if (labelToCfNode.TryGetValue(target, out destination) && destination != source) {
								ControlFlowEdge edge = new ControlFlowEdge(source, destination, JumpType.Normal);
								source.Outgoing.Add(edge);
								destination.Incoming.Add(edge);
							}
						}
					}
				}
			}
			
			return new ControlFlowGraph(cfNodes.ToArray());
		}
		
		List<ILNode> FindLoops(HashSet<ControlFlowNode> scope, ControlFlowNode entryPoint, bool excludeEntryPoint)
		{
			List<ILNode> result = new List<ILNode>();
			
			// Do not modify entry data
			scope = new HashSet<ControlFlowNode>(scope);
			
			Queue<ControlFlowNode> agenda  = new Queue<ControlFlowNode>();
			agenda.Enqueue(entryPoint);
			while(agenda.Count > 0) {
				ControlFlowNode node = agenda.Dequeue();
				
				if (scope.Contains(node)
				    && node.DominanceFrontier.Contains(node)
				    && (node != entryPoint || !excludeEntryPoint))
				{
					HashSet<ControlFlowNode> loopContents = FindLoopContent(scope, node);
					
					ILBasicBlock basicBlock = (ILBasicBlock)node.UserData;
					ILExpression condExpr;
					ILLabel trueLabel;
					ILLabel falseLabel;
					if(basicBlock.MatchBrTure(out condExpr, out trueLabel, out falseLabel)) {
						loopContents.RemoveOrThrow(node);
						scope.RemoveOrThrow(node);
						
						// TODO: Does 'true' really point into the loop body?  Swap if necessary
						
						ControlFlowNode postLoopTarget;
						labelToCfNode.TryGetValue(falseLabel, out postLoopTarget);
						if (postLoopTarget != null) {
							// Pull more nodes into the loop
							HashSet<ControlFlowNode> postLoopContents = FindDominatedNodes(scope, postLoopTarget);
							var pullIn = scope.Except(postLoopContents).Where(n => node.Dominates(n));
							loopContents.UnionWith(pullIn);
						}
						
						// Use loop to implement the condition
						result.Add(new ILBasicBlock() {
						    EntryLabel = basicBlock.EntryLabel,
							Body = new List<ILNode>() {
								new ILWhileLoop() {
									Condition = condExpr,
									BodyBlock = new ILBlock() {
										EntryGoto = new ILExpression(ILCode.Br, trueLabel),
										Body = FindLoops(loopContents, node, true)
									}
								},
								new ILExpression(ILCode.Br, falseLabel)
							},
							FallthoughGoto = null
						});
					} else {
						result.Add(new ILBasicBlock() {
						    EntryLabel = new ILLabel() { Name = "Loop_" + (nextLabelIndex++) },
							Body = new List<ILNode>() {
								new ILWhileLoop() {
									BodyBlock = new ILBlock() {
										EntryGoto = new ILExpression(ILCode.Br, basicBlock.EntryLabel),
										Body = FindLoops(loopContents, node, true)
									}
								},
							},
							FallthoughGoto = null
						});
					}
					
					// Move the content into loop block
					scope.ExceptWith(loopContents);
				}

				// Using the dominator tree should ensure we find the the widest loop first
				foreach(var child in node.DominatorTreeChildren) {
					agenda.Enqueue(child);
				}
			}
			
			// Add whatever is left
			foreach(var node in scope) {
				result.Add((ILNode)node.UserData);
			}
			scope.Clear();
			
			return result;
		}
		
		List<ILNode> FindConditions(HashSet<ControlFlowNode> scope, ControlFlowNode entryNode)
		{
			List<ILNode> result = new List<ILNode>();
			
			// Do not modify entry data
			scope = new HashSet<ControlFlowNode>(scope);
			
			HashSet<ControlFlowNode> agenda  = new HashSet<ControlFlowNode>();
			agenda.Add(entryNode);
			while(agenda.Any()) {
				ControlFlowNode node = agenda.First();
				// Attempt for a good order
				while(agenda.Contains(node.ImmediateDominator)) {
					node = node.ImmediateDominator;
				}
				agenda.Remove(node);
				
				// Find a block that represents a simple condition
				if (scope.Contains(node)) {
					
					ILBasicBlock block = node.UserData as ILBasicBlock;
					
					if (block != null && block.Body.Count == 1) {
						
						ILExpression condBranch = block.Body[0] as ILExpression;
						
						// Switch
						if (condBranch != null && condBranch.Operand is ILLabel[] && condBranch.Arguments.Count > 0) {
							
							ILLabel[] caseLabels = (ILLabel[])condBranch.Operand;
							
							// The labels will not be used - kill them
							condBranch.Operand = null;
							
							ILSwitch ilSwitch = new ILSwitch() {
								Condition = condBranch,
								DefaultGoto = block.FallthoughGoto
							};
							result.Add(new ILBasicBlock() {
							           	EntryLabel = block.EntryLabel,  // Keep the entry label
							           	Body = { ilSwitch }
							           });

							// Remove the item so that it is not picked up as content
							scope.RemoveOrThrow(node);
							
							// Pull in code of cases
							ControlFlowNode fallTarget = null;
							labelToCfNode.TryGetValue((ILLabel)block.FallthoughGoto.Operand, out fallTarget);
							
							HashSet<ControlFlowNode> frontiers = new HashSet<ControlFlowNode>();
							if (fallTarget != null)
								frontiers.UnionWith(fallTarget.DominanceFrontier);
							
							foreach(ILLabel condLabel in caseLabels) {
								ControlFlowNode condTarget = null;
								labelToCfNode.TryGetValue(condLabel, out condTarget);
								if (condTarget != null)
									frontiers.UnionWith(condTarget.DominanceFrontier);
							}
							
							foreach(ILLabel condLabel in caseLabels) {
								ControlFlowNode condTarget = null;
								labelToCfNode.TryGetValue(condLabel, out condTarget);
								
								ILBlock caseBlock = new ILBlock() {
									EntryGoto = new ILExpression(ILCode.Br, condLabel)
								};
								if (condTarget != null && !frontiers.Contains(condTarget)) {
									HashSet<ControlFlowNode> content = FindDominatedNodes(scope, condTarget);
									scope.ExceptWith(content);
									caseBlock.Body.AddRange(FindConditions(content, condTarget));
								}
								ilSwitch.CaseBlocks.Add(caseBlock);
							}
						}
						
						// Two-way branch
						ILExpression condExpr;
						ILLabel trueLabel;
						ILLabel falseLabel;
						if(block.MatchBrTure(out condExpr, out trueLabel, out falseLabel)) {
							
							// Swap bodies since that seems to be the usual C# order
							ILLabel temp = trueLabel;
							trueLabel = falseLabel;
							falseLabel = temp;
							condExpr = new ILExpression(ILCode.LogicNot, null, condExpr);
							
							// Convert the basic block to ILCondition
							ILCondition ilCond = new ILCondition() {
								Condition  = condExpr,
								TrueBlock  = new ILBlock() { EntryGoto = new ILExpression(ILCode.Br, trueLabel) },
								FalseBlock = new ILBlock() { EntryGoto = new ILExpression(ILCode.Br, falseLabel) }
							};
							result.Add(new ILBasicBlock() {
								EntryLabel = block.EntryLabel,  // Keep the entry label
								Body = { ilCond }
							});
							
							// Remove the item immediately so that it is not picked up as content
							scope.RemoveOrThrow(node);
							
							ControlFlowNode trueTarget = null;
							labelToCfNode.TryGetValue(trueLabel, out trueTarget);
							ControlFlowNode falseTarget = null;
							labelToCfNode.TryGetValue(falseLabel, out falseTarget);
							
							// Pull in the conditional code
							HashSet<ControlFlowNode> frontiers = new HashSet<ControlFlowNode>();
							if (trueTarget != null)
								frontiers.UnionWith(trueTarget.DominanceFrontier);
							if (falseTarget != null)
								frontiers.UnionWith(falseTarget.DominanceFrontier);
							
							if (trueTarget != null && !frontiers.Contains(trueTarget)) {
								HashSet<ControlFlowNode> content = FindDominatedNodes(scope, trueTarget);
								scope.ExceptWith(content);
								ilCond.TrueBlock.Body.AddRange(FindConditions(content, trueTarget));
							}
							if (falseTarget != null && !frontiers.Contains(falseTarget)) {
								HashSet<ControlFlowNode> content = FindDominatedNodes(scope, falseTarget);
								scope.ExceptWith(content);
								ilCond.FalseBlock.Body.AddRange(FindConditions(content, falseTarget));
							}
						}
					}
					
					// Add the node now so that we have good ordering
					if (scope.Contains(node)) {
						result.Add((ILNode)node.UserData);
						scope.Remove(node);
					}
				}

				// Using the dominator tree should ensure we find the the widest loop first
				foreach(var child in node.DominatorTreeChildren) {
					agenda.Add(child);
				}
			}
			
			// Add whatever is left
			foreach(var node in scope) {
				result.Add((ILNode)node.UserData);
			}
			
			return result;
		}
		
		static HashSet<ControlFlowNode> FindDominatedNodes(HashSet<ControlFlowNode> scope, ControlFlowNode head)
		{
			HashSet<ControlFlowNode> agenda = new HashSet<ControlFlowNode>();
			HashSet<ControlFlowNode> result = new HashSet<ControlFlowNode>();
			agenda.Add(head);
			
			while(agenda.Count > 0) {
				ControlFlowNode addNode = agenda.First();
				agenda.Remove(addNode);
				
				if (scope.Contains(addNode) && head.Dominates(addNode) && result.Add(addNode)) {
					foreach (var successor in addNode.Successors) {
						agenda.Add(successor);
					}
				}
			}
			
			return result;
		}
		
		static HashSet<ControlFlowNode> FindLoopContent(HashSet<ControlFlowNode> scope, ControlFlowNode head)
		{
			var exitNodes = head.DominanceFrontier.SelectMany(n => n.Predecessors);
			HashSet<ControlFlowNode> agenda = new HashSet<ControlFlowNode>(exitNodes);
			HashSet<ControlFlowNode> result = new HashSet<ControlFlowNode>();
			
			while(agenda.Count > 0) {
				ControlFlowNode addNode = agenda.First();
				agenda.Remove(addNode);
				
				if (scope.Contains(addNode) && head.Dominates(addNode) && result.Add(addNode)) {
					foreach (var predecessor in addNode.Predecessors) {
						agenda.Add(predecessor);
					}
				}
			}
			if (scope.Contains(head))
				result.Add(head);
			
			return result;
		}
		
		/// <summary>
		/// Flattens all nested basic blocks, except the the top level 'node' argument
		/// </summary>
		void FlattenBasicBlocks(ILNode node)
		{
			ILBlock block = node as ILBlock;
			if (block != null) {
				List<ILNode> flatBody = new List<ILNode>();
				foreach (ILNode child in block.GetChildren()) {
					FlattenBasicBlocks(child);
					if (child is ILBasicBlock) {
						flatBody.AddRange(child.GetChildren());
					} else {
						flatBody.Add(child);
					}
				}
				block.EntryGoto = null;
				block.Body = flatBody;
			} else if (node is ILExpression) {
				// Optimization - no need to check expressions
			} else if (node != null) {
				// Recursively find all ILBlocks
				foreach(ILNode child in node.GetChildren()) {
					FlattenBasicBlocks(child);
				}
			}
		}
		
		/// <summary>
		/// Reduce the nesting of conditions.
		/// It should be done on flat data that already had most gotos removed
		/// </summary>
		void FlattenIfStatements(ILNode node)
		{
			ILBlock block = node as ILBlock;
			if (block != null) {
				for (int i = 0; i < block.Body.Count; i++) {
					ILCondition cond = block.Body[i] as ILCondition;
					if (cond != null) {
						bool trueExits = cond.TrueBlock.Body.Count > 0 && !cond.TrueBlock.Body.Last().CanFallthough();
						bool falseExits = cond.FalseBlock.Body.Count > 0 && !cond.FalseBlock.Body.Last().CanFallthough();
						
						if (trueExits) {
							// Move the false block after the condition
							block.Body.InsertRange(i + 1, cond.FalseBlock.GetChildren());
							cond.FalseBlock = new ILBlock();
						} else if (falseExits) {
							// Move the true block after the condition
							block.Body.InsertRange(i + 1, cond.TrueBlock.GetChildren());
							cond.TrueBlock = new ILBlock();
						}
						
						// Eliminate empty true block
						if (!cond.TrueBlock.GetChildren().Any() && cond.FalseBlock.GetChildren().Any()) {
							// Swap bodies
							ILBlock tmp = cond.TrueBlock;
							cond.TrueBlock = cond.FalseBlock;
							cond.FalseBlock = tmp;
							cond.Condition = new ILExpression(ILCode.LogicNot, null, cond.Condition);
						}
					}
				}
			}
			
			// We are changing the number of blocks so we use plain old recursion to get all blocks
			foreach(ILNode child in node.GetChildren()) {
				if (child != null && !(child is ILExpression))
					FlattenIfStatements(child);
			}
		}
	}
	
	public static class ILAstOptimizerExtensionMethods
	{
		public static bool Match(this ILNode node, ILCode code)
		{
			ILExpression expr = node as ILExpression;
			return expr != null && expr.Prefixes == null && expr.Code == code;
		}
		
		public static bool Match(this ILNode node, ILCode code, out ILExpression expr)
		{
			expr = node as ILExpression;
			return expr != null && expr.Prefixes == null && expr.Code == code;
		}
		
		public static bool Match<T>(this ILNode node, ILCode code, out T operand)
		{
			ILExpression expr = node as ILExpression;
			if (expr != null && expr.Prefixes == null && expr.Code == code) {
				operand = (T)expr.Operand;
				return true;
			}
			operand = default(T);
			return false;
		}
		
		public static bool MatchBrTure(this ILBasicBlock bb, out ILExpression condition, out ILLabel trueLabel, out ILLabel falseLabel)
		{
			if (bb.Body.Count == 1) {
				if (bb.Body[0].Match(ILCode.Brtrue, out trueLabel)) {
					condition  = ((ILExpression)bb.Body[0]).Arguments.Single();
					falseLabel = (ILLabel)((ILExpression)bb.FallthoughGoto).Operand;
					return true;
				}
			}
			condition  = null;
			trueLabel  = null;
			falseLabel = null;
			return false;
		}
		
		public static bool CanFallthough(this ILNode node)
		{
			ILExpression expr = node as ILExpression;
			if (expr != null) {
				switch(expr.Code) {
					case ILCode.Br:
					case ILCode.Ret:
					case ILCode.Throw:
					case ILCode.Rethrow:
					case ILCode.LoopContinue:
					case ILCode.LoopBreak:
						return false;
				}
			}
			return true;
		}
		
		public static void RemoveOrThrow<T>(this ICollection<T> collection, T item)
		{
			if (!collection.Remove(item))
				throw new Exception("The item was not found in the collection");
		}
		
		public static void RemoveOrThrow<K,V>(this Dictionary<K,V> collection, K key)
		{
			if (!collection.Remove(key))
				throw new Exception("The key was not found in the dictionary");
		}
	}
}
