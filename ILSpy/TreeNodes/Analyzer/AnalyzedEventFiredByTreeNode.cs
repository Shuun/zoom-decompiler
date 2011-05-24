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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using ICSharpCode.TreeView;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace ICSharpCode.ILSpy.TreeNodes.Analyzer
{
	internal sealed class AnalyzedEventFiredByTreeNode : AnalyzerTreeNode
	{
		private readonly EventDefinition analyzedEvent;
		private readonly FieldDefinition eventBackingField;
		private readonly MethodDefinition eventFiringMethod;

		private readonly ThreadingSupport threading;
		private ConcurrentDictionary<MethodDefinition, int> foundMethods;

		public AnalyzedEventFiredByTreeNode(EventDefinition analyzedEvent)
		{
			if (analyzedEvent == null)
				throw new ArgumentNullException("analyzedEvent");

			this.analyzedEvent = analyzedEvent;
			this.threading = new ThreadingSupport();
			this.LazyLoading = true;

			this.eventBackingField = analyzedEvent.DeclaringType.Fields.First(
				fd => (fd.Name == analyzedEvent.Name || fd.Name == (analyzedEvent.Name + "Event")) &&
					  fd.FieldType.FullName == analyzedEvent.EventType.FullName);
			this.eventFiringMethod = analyzedEvent.EventType.Resolve().Methods.First(md => md.Name == "Invoke");
		}

		public override object Text
		{
			get { return "Raised By"; }
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
			foundMethods = new ConcurrentDictionary<MethodDefinition, int>();

			foreach (var child in FindReferencesInType()) {
				yield return child;
			}

			foundMethods = null;
		}

		private IEnumerable<SharpTreeNode> FindReferencesInType()
		{
			foreach (MethodDefinition method in analyzedEvent.DeclaringType.Methods) {
				bool readBackingField = false;
				bool found = false;
				if (!method.HasBody)
					continue;
				foreach (Instruction instr in method.Body.Instructions) {
					Code code = instr.OpCode.Code;
					if (code == Code.Ldfld || code == Code.Ldflda) {
						FieldReference fr = instr.Operand as FieldReference;
						if (fr != null && fr.Name == eventBackingField.Name && fr == eventBackingField) {
							readBackingField = true;
						}
					}
					if (readBackingField && (code == Code.Callvirt || code == Code.Call)) {
						MethodReference mr = instr.Operand as MethodReference;
						if (mr != null && mr.Name == eventFiringMethod.Name && mr.Resolve() == eventFiringMethod) {
							found = true;
							break;
						}
					}
				}

				method.Body = null;

				if (found) {
					MethodDefinition codeLocation = this.Language.GetOriginalCodeLocation(method) as MethodDefinition;
					if (codeLocation != null && !HasAlreadyBeenFound(codeLocation)) {
						yield return new AnalyzedMethodTreeNode(codeLocation);
					}
				}
			}
		}

		private bool HasAlreadyBeenFound(MethodDefinition method)
		{
			return !foundMethods.TryAdd(method, 0);
		}


		public static bool CanShow(EventDefinition ev)
		{
			var fieldName = ev.Name;
			var vbStyleFieldName = fieldName + "Event";
			var fieldType = ev.EventType;

			foreach (var fd in ev.DeclaringType.Fields) {
				if (fd.Name == fieldName || fd.Name == vbStyleFieldName)
					if (fd.FieldType.FullName == fieldType.FullName)
						return true;
			}
			return false;
		}
	}
}
