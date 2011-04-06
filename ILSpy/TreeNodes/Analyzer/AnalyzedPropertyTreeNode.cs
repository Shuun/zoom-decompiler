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
	class AnalyzedPropertyTreeNode : AnalyzerTreeNode
	{
		PropertyDefinition analyzedProperty;
		string prefix;
		
		public AnalyzedPropertyTreeNode(PropertyDefinition analyzedProperty, string prefix = "")
		{
			if (analyzedProperty == null)
				throw new ArgumentNullException("analyzedMethod");

			this.analyzedProperty = analyzedProperty;
			this.prefix = prefix;
			this.LazyLoading = true;
		}
		
		public override object Icon {
			get { return PropertyTreeNode.GetIcon(analyzedProperty); }
		}
		
		public override object Text {
			get {
				return prefix + Language.TypeToString(analyzedProperty.DeclaringType, true) + "." + PropertyTreeNode.GetText(analyzedProperty, Language); }
		}
		
		public override void ActivateItem(System.Windows.RoutedEventArgs e)
		{
			e.Handled = true;
			MainWindow.Instance.JumpToReference(analyzedProperty);
		}
		
		protected override void LoadChildren()
		{
			if(AnalyzedPropertyAccessorsTreeNode.CanShow(analyzedProperty))
				this.Children.Add(new AnalyzedPropertyAccessorsTreeNode(analyzedProperty));
			if (AnalyzedPropertyOverridesTreeNode.CanShowAnalyzer(analyzedProperty))
				this.Children.Add(new AnalyzedPropertyOverridesTreeNode(analyzedProperty));
			//if (analyzedProperty.HasBody)
			//    this.Children.Add(new AnalyzedMethodUsesNode(analyzedProperty));
			//this.Children.Add(new AnalyzedMethodUsedByTreeNode(analyzedProperty));
		}

		public static AnalyzerTreeNode TryCreateAnalyzer(MemberReference member)
		{
			if (CanShow(member))
				return new AnalyzedPropertyTreeNode(member as PropertyDefinition);
			else
				return null;
		}

		public static bool CanShow(MemberReference member)
		{
			var property = member as PropertyDefinition;
			if (property == null)
				return false;

			return AnalyzedPropertyAccessorsTreeNode.CanShow(property)
				|| AnalyzedPropertyOverridesTreeNode.CanShowAnalyzer(property);
		}
	}
}
