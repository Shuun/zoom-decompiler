﻿// Copyright (c) 2011 AlphaSierraPapa for the SharpDevelop Team
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using ICSharpCode.Decompiler;
using ICSharpCode.Decompiler.Ast;
using ICSharpCode.NRefactory.Utils;
using ICSharpCode.TreeView;
using Mono.Cecil;

namespace ICSharpCode.ILSpy.TreeNodes.Analyzer
{
	/// <summary>
	/// Searches for overrides of the analyzed method.
	/// </summary>
	internal sealed class AnalyzedMethodOverridesTreeNode : AnalyzerTreeNode
	{
		private readonly MethodDefinition analyzedMethod;
		private readonly ThreadingSupport threading;

		public AnalyzedMethodOverridesTreeNode(MethodDefinition analyzedMethod)
		{
			if (analyzedMethod == null)
				throw new ArgumentNullException("analyzedMethod");

			this.analyzedMethod = analyzedMethod;
			this.threading = new ThreadingSupport();
			this.LazyLoading = true;
		}

		public override object Text
		{
			get { return "Overridden By"; }
		}

		public override object Icon
		{
			get { return Images.Search; }
		}

		protected override void LoadChildren()
		{
			threading.LoadChildren(this, FetchChildren);
		}

		protected override void OnCollapsing()
		{
			if (threading.IsRunning) {
				this.LazyLoading = true;
				threading.Cancel();
				this.Children.Clear();
			}
		}

		private IEnumerable<SharpTreeNode> FetchChildren(CancellationToken ct)
		{
			return FindReferences(MainWindow.Instance.CurrentAssemblyList.GetAssemblies(), ct);
		}

		private IEnumerable<SharpTreeNode> FindReferences(IEnumerable<LoadedAssembly> assemblies, CancellationToken ct)
		{
			assemblies = assemblies.Where(asm => asm.AssemblyDefinition != null);

			// use parallelism only on the assembly level (avoid locks within Cecil)
			return assemblies.AsParallel().WithCancellation(ct).SelectMany((LoadedAssembly asm) => FindReferences(asm, ct));
		}

		private IEnumerable<SharpTreeNode> FindReferences(LoadedAssembly asm, CancellationToken ct)
		{
			string asmName = asm.AssemblyDefinition.Name.Name;
			string name = analyzedMethod.Name;
			string declTypeName = analyzedMethod.DeclaringType.FullName;
			foreach (TypeDefinition type in TreeTraversal.PreOrder(asm.AssemblyDefinition.MainModule.Types, t => t.NestedTypes)) {
				ct.ThrowIfCancellationRequested();
				SharpTreeNode newNode = null;
				try {
					if (!TypesHierarchyHelpers.IsBaseType(analyzedMethod.DeclaringType, type, resolveTypeArguments: false))
						continue;

					foreach (MethodDefinition method in type.Methods) {
						ct.ThrowIfCancellationRequested();

						if (TypesHierarchyHelpers.IsBaseMethod(analyzedMethod, method)) {
							bool hidesParent = !method.IsVirtual ^ method.IsNewSlot;
							newNode = new AnalyzedMethodTreeNode(method, hidesParent ? "(hides) " : "");
						}
					}
				}
				catch (ReferenceResolvingException) {
					// ignore this type definition. maybe add a notification about such cases.
				}

				if (newNode != null)
					yield return newNode;
			}
		}

		public static bool CanShow(MethodDefinition method)
		{
			return method.IsVirtual &&
				!method.IsFinal &&
				!method.DeclaringType.IsSealed &&
				!method.DeclaringType.IsInterface;	// interface methods are definitions not implementations - cannot be overridden
		}
	}
}
