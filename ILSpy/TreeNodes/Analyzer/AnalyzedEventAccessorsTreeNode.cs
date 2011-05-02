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
using Mono.Cecil;

namespace ICSharpCode.ILSpy.TreeNodes.Analyzer
{
	class AnalyzedEventAccessorsTreeNode : AnalyzerTreeNode
	{
		EventDefinition analyzedEvent;

		public AnalyzedEventAccessorsTreeNode(EventDefinition analyzedEvent)
		{
			if (analyzedEvent == null)
				throw new ArgumentNullException("analyzedEvent");
			this.analyzedEvent = analyzedEvent;

			if (analyzedEvent.AddMethod != null)
				this.Children.Add(new AnalyzedEventAccessorTreeNode(analyzedEvent.AddMethod, "add"));
			if (analyzedEvent.RemoveMethod != null)
				this.Children.Add(new AnalyzedEventAccessorTreeNode(analyzedEvent.RemoveMethod, "remove"));
			foreach (var accessor in analyzedEvent.OtherMethods)
				this.Children.Add(new AnalyzedEventAccessorTreeNode(accessor, null));
		}

		public override object Icon
		{
			get { return Images.Search; }
		}

		public override object Text
		{
			get { return "Accessors"; }
		}

		public static bool CanShow(EventDefinition property)
		{
			return !MainWindow.Instance.CurrentLanguage.ShowMember(property.AddMethod ?? property.RemoveMethod);
		}

		class AnalyzedEventAccessorTreeNode : AnalyzedMethodTreeNode
		{
			string name;

			public AnalyzedEventAccessorTreeNode(MethodDefinition analyzedMethod, string name)
				: base(analyzedMethod)
			{
				this.name = name;
			}

			public override object Text
			{
				get { return name ?? base.Text; }
			}
		}
	}
}
