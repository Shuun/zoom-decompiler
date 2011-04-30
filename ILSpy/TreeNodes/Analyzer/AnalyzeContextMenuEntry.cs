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
using System.Linq;
using ICSharpCode.TreeView;
using Mono.Cecil;

namespace ICSharpCode.ILSpy.TreeNodes.Analyzer
{
	[ExportContextMenuEntry(Header = "Analyze", Icon = "images/Search.png")]
	internal sealed class AnalyzeContextMenuEntry : IContextMenuEntry
	{
		public bool IsVisible(SharpTreeNode[] selectedNodes)
		{
			return selectedNodes.All(n => n is IMemberTreeNode);
		}

		public bool IsEnabled(SharpTreeNode[] selectedNodes)
		{
			foreach (IMemberTreeNode node in selectedNodes) {
				if (!(node.Member is TypeDefinition
					|| node.Member is FieldDefinition
					|| node.Member is MethodDefinition
					|| Analyzer.AnalyzedPropertyTreeNode.CanShow(node.Member)
					|| Analyzer.AnalyzedEventTreeNode.CanShow(node.Member)))
					return false;
			}

			return true;
		}

		public void Execute(SharpTreeNode[] selectedNodes)
		{
			// TODO: figure out when equivalent nodes are already present
			// and focus those instead.
			foreach (IMemberTreeNode node in selectedNodes) {
				TypeDefinition type = node.Member as TypeDefinition;
				if (type != null)
					MainWindow.Instance.AddToAnalyzer(new AnalyzedTypeTreeNode(type));
				FieldDefinition field = node.Member as FieldDefinition;
				if (field != null)
					MainWindow.Instance.AddToAnalyzer(new AnalyzedFieldTreeNode(field));
				MethodDefinition method = node.Member as MethodDefinition;
				if (method != null)
					MainWindow.Instance.AddToAnalyzer(new AnalyzedMethodTreeNode(method));
				var propertyAnalyzer = Analyzer.AnalyzedPropertyTreeNode.TryCreateAnalyzer(node.Member);
				if (propertyAnalyzer != null)
					MainWindow.Instance.AddToAnalyzer(propertyAnalyzer);
				var eventAnalyzer = Analyzer.AnalyzedEventTreeNode.TryCreateAnalyzer(node.Member);
				if (eventAnalyzer != null)
					MainWindow.Instance.AddToAnalyzer(eventAnalyzer);
			}
		}
	}
}
