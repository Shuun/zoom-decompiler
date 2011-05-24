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
using System.Collections.ObjectModel;
using System.ComponentModel.Composition.Hosting;
using System.Linq;

using ICSharpCode.Decompiler;
using ICSharpCode.Decompiler.Ast;
using ICSharpCode.Decompiler.Disassembler;
using ICSharpCode.Decompiler.ILAst;
using ICSharpCode.NRefactory.CSharp;
using ICSharpCode.NRefactory.Utils;
using Mono.Cecil;

namespace ICSharpCode.ILSpy
{
	/// <summary>
	/// Decompilation event arguments.
	/// </summary>
	public sealed class DecompileEventArgs : EventArgs
	{
		/// <summary>
		/// Gets ot sets the code mappings
		/// </summary>
		public Dictionary<int, List<MemberMapping>> CodeMappings { get; internal set; }
		
		/// <summary>
		/// Gets or sets the local variables.
		/// </summary>
		public ConcurrentDictionary<int, IEnumerable<ILVariable>> LocalVariables { get; internal set; }
		
		/// <summary>
		/// Gets the list of MembeReferences that are decompiled (TypeDefinitions, MethodDefinitions, etc)
		/// </summary>
		public Dictionary<int, MemberReference> DecompiledMemberReferences { get; internal set; }
		
		/// <summary>
		/// Gets (or internal sets) the AST nodes.
		/// </summary>
		public IEnumerable<AstNode> AstNodes { get; internal set; }
	}
	
	/// <summary>
	/// Base class for language-specific decompiler implementations.
	/// </summary>
	public abstract class Language
	{
		/// <summary>
		/// Decompile finished event.
		/// </summary>
		public event EventHandler<DecompileEventArgs> DecompileFinished;
		
		/// <summary>
		/// Gets the name of the language (as shown in the UI)
		/// </summary>
		public abstract string Name { get; }

		/// <summary>
		/// Gets the file extension used by source code files in this language.
		/// </summary>
		public abstract string FileExtension { get; }

		public virtual string ProjectFileExtension
		{
			get { return null; }
		}

		/// <summary>
		/// Gets the syntax highlighting used for this language.
		/// </summary>
		public virtual ICSharpCode.AvalonEdit.Highlighting.IHighlightingDefinition SyntaxHighlighting
		{
			get
			{
				return ICSharpCode.AvalonEdit.Highlighting.HighlightingManager.Instance.GetDefinitionByExtension(this.FileExtension);
			}
		}

		public virtual void DecompileMethod(MethodDefinition method, ITextOutput output, DecompilationOptions options)
		{
			WriteCommentLine(output, TypeToString(method.DeclaringType, true) + "." + method.Name);
		}

		public virtual void DecompileProperty(PropertyDefinition property, ITextOutput output, DecompilationOptions options)
		{
			WriteCommentLine(output, TypeToString(property.DeclaringType, true) + "." + property.Name);
		}

		public virtual void DecompileField(FieldDefinition field, ITextOutput output, DecompilationOptions options)
		{
			WriteCommentLine(output, TypeToString(field.DeclaringType, true) + "." + field.Name);
		}

		public virtual void DecompileEvent(EventDefinition ev, ITextOutput output, DecompilationOptions options)
		{
			WriteCommentLine(output, TypeToString(ev.DeclaringType, true) + "." + ev.Name);
		}

		public virtual void DecompileType(TypeDefinition type, ITextOutput output, DecompilationOptions options)
		{
			WriteCommentLine(output, TypeToString(type, true));
		}

		public virtual void DecompileNamespace(string nameSpace, IEnumerable<TypeDefinition> types, ITextOutput output, DecompilationOptions options)
		{
			WriteCommentLine(output, nameSpace);
			OnDecompilationFinished(null);
		}

		public virtual void DecompileAssembly(LoadedAssembly assembly, ITextOutput output, DecompilationOptions options)
		{
			WriteCommentLine(output, assembly.FileName);
			WriteCommentLine(output, assembly.AssemblyDefinition.FullName);
		}

		public virtual void WriteCommentLine(ITextOutput output, string comment)
		{
			output.WriteLine("// " + comment);
		}

		/// <summary>
		/// Converts a type reference into a string. This method is used by the member tree node for parameter and return types.
		/// </summary>
		public virtual string TypeToString(TypeReference type, bool includeNamespace, ICustomAttributeProvider typeAttributes = null)
		{
			if (includeNamespace)
				return type.FullName;
			else
				return type.Name;
		}

		/// <summary>
		/// Converts a member signature to a string.
		/// This is used for displaying the tooltip on a member reference.
		/// </summary>
		public virtual string GetTooltip(MemberReference member)
		{
			if (member is TypeReference)
				return TypeToString((TypeReference)member, true);
			else
				return member.ToString();
		}

		public virtual string FormatPropertyName(PropertyDefinition property, bool? isIndexer = null)
		{
			if (property == null)
				throw new ArgumentNullException("property");
			return property.Name;
		}
		
		public virtual string FormatTypeName(TypeDefinition type)
		{
			if (type == null)
				throw new ArgumentNullException("type");
			return type.Name;
		}

		/// <summary>
		/// Used for WPF keyboard navigation.
		/// </summary>
		public override string ToString()
		{
			return Name;
		}

		public virtual bool ShowMember(MemberReference member)
		{
			return true;
		}

		/// <summary>
		/// Used by the analyzer to map compiler generated code back to the original code's location
		/// </summary>
		public virtual MemberReference GetOriginalCodeLocation(MemberReference member)
		{
			return member;
		}
		
		protected virtual void OnDecompilationFinished(DecompileEventArgs e)
		{
			if (DecompileFinished != null) {
				DecompileFinished(this, e);
			}
		}
		
		protected void NotifyDecompilationFinished(BaseCodeMappings b)
		{
			if (b is AstBuilder) {
				var builder = b as AstBuilder;
				
				var nodes = TreeTraversal
					.PreOrder((AstNode)builder.CompilationUnit, n => n.Children)
					.Where(n => n is AttributedNode && n.Annotation<Tuple<int, int>>() != null);
				
				OnDecompilationFinished(new DecompileEventArgs {
				                        	CodeMappings = builder.CodeMappings,
				                        	LocalVariables = builder.LocalVariables,
				                        	DecompiledMemberReferences = builder.DecompiledMemberReferences,
				                        	AstNodes = nodes
				                        });
			}
			
			if (b is ReflectionDisassembler) {
				var dis = b as ReflectionDisassembler;
				OnDecompilationFinished(new DecompileEventArgs {
				                        	CodeMappings = dis.CodeMappings,
				                        	DecompiledMemberReferences = dis.DecompiledMemberReferences,
				                        	AstNodes = null // TODO: how can I find the nodes with line numbers from dis?
				                        });
			}
		}
	}

	public static class Languages
	{
		static ReadOnlyCollection<Language> allLanguages;

		/// <summary>
		/// A list of all languages.
		/// </summary>
		public static ReadOnlyCollection<Language> AllLanguages
		{
			get
			{
				return allLanguages;
			}
		}


		internal static void Initialize(CompositionContainer composition)
		{
			List<Language> languages = new List<Language>();
			languages.AddRange(composition.GetExportedValues<Language>());
			languages.Add(new ILLanguage(true));
#if DEBUG
			languages.AddRange(ILAstLanguage.GetDebugLanguages());
			languages.AddRange(CSharpLanguage.GetDebugLanguages());
#endif
			allLanguages = languages.AsReadOnly();
		}

		/// <summary>
		/// Gets a language using its name.
		/// If the language is not found, C# is returned instead.
		/// </summary>
		public static Language GetLanguage(string name)
		{
			return AllLanguages.FirstOrDefault(l => l.Name == name) ?? AllLanguages.First();
		}
	}
}
