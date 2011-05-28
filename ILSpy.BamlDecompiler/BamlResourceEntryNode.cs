﻿// Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code is distributed under the MS-PL (for details please see \doc\MS-PL.txt)

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.ILSpy;
using ICSharpCode.ILSpy.TextView;
using ICSharpCode.ILSpy.TreeNodes;
using Mono.Cecil;
using Ricciolo.StylesExplorer.MarkupReflection;

namespace ILSpy.BamlDecompiler
{
	public sealed class BamlResourceEntryNode : ResourceEntryNode
	{
		public BamlResourceEntryNode(string key, Stream data) : base(key, data)
		{
		}
		
		public override bool View(DecompilerTextView textView)
		{
			AvalonEditTextOutput output = new AvalonEditTextOutput();
			IHighlightingDefinition highlighting = null;
			
			textView.RunWithCancellation(
				token => Task.Factory.StartNew(
					() => {
						try {
							if (LoadBaml(output))
								highlighting = HighlightingManager.Instance.GetDefinitionByExtension(".xml");
						} catch (Exception ex) {
							output.Write(ex.ToString());
						}
						return output;
					}),
				t => textView.ShowNode(t.Result, this, highlighting)
			);
			return true;
		}
		
		const string XWPFNamespace = "http://schemas.microsoft.com/winfx/2006/xaml";
		const string DefaultWPFNamespace = "http://schemas.microsoft.com/winfx/2006/xaml/presentation";
		
		bool LoadBaml(AvalonEditTextOutput output)
		{
			var asm = this.Ancestors().OfType<AssemblyTreeNode>().FirstOrDefault().LoadedAssembly;
			Data.Position = 0;
			

			XDocument xamlDocument = LoadIntoDocument(asm.GetAssemblyResolver(), asm.AssemblyDefinition, Data);
			
			output.Write(xamlDocument.ToString());
			return true;
		}

		internal static XDocument LoadIntoDocument(IAssemblyResolver resolver, AssemblyDefinition asm, Stream stream)
		{
			XDocument xamlDocument;
			using (XmlBamlReader reader = new XmlBamlReader(stream, new CecilTypeResolver(resolver, asm)))
				xamlDocument = XDocument.Load(reader);
			ConvertToEmptyElements(xamlDocument.Root);
			MoveNamespacesToRoot(xamlDocument);
			return xamlDocument;
		}

		static void MoveNamespacesToRoot(XDocument xamlDocument)
		{
			var additionalXmlns = new List<XAttribute> {
				new XAttribute("xmlns", DefaultWPFNamespace),
				new XAttribute(XName.Get("x", XNamespace.Xmlns.NamespaceName), XWPFNamespace)
			};
			
			foreach (var element in xamlDocument.Root.DescendantsAndSelf()) {
				if (element.Name.NamespaceName != DefaultWPFNamespace && !additionalXmlns.Any(ka => ka.Value == element.Name.NamespaceName)) {
					string newPrefix = new string(element.Name.LocalName.Where(c => char.IsUpper(c)).ToArray()).ToLowerInvariant();
					int current = additionalXmlns.Count(ka => ka.Name.Namespace == XNamespace.Xmlns && ka.Name.LocalName.TrimEnd(ch => char.IsNumber(ch)) == newPrefix);
					if (current > 0)
						newPrefix += (current + 1).ToString();
					XName defaultXmlns = XName.Get(newPrefix, XNamespace.Xmlns.NamespaceName);
					if (element.Name.NamespaceName != DefaultWPFNamespace)
						additionalXmlns.Add(new XAttribute(defaultXmlns, element.Name.NamespaceName));
				}
			}
			
			foreach (var xmlns in additionalXmlns.Except(xamlDocument.Root.Attributes())) {
				xamlDocument.Root.Add(xmlns);
			}
		}
		
		static void ConvertToEmptyElements(XElement element)
		{
			foreach (var el in element.Elements()) {
				if (!el.IsEmpty && !el.HasElements && el.Value == "") {
					el.RemoveNodes();
					continue;
				}
				ConvertToEmptyElements(el);
			}
		}
	}
}