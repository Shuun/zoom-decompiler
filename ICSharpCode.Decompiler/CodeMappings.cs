﻿// Copyright (c) AlphaSierraPapa for the SharpDevelop Team (for details please see \doc\copyright.txt)
// This code is distributed under MIT X11 license (for details please see \doc\license.txt)

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

using ICSharpCode.Decompiler.Ast;
using ICSharpCode.Decompiler.Disassembler;
using ICSharpCode.Decompiler.ILAst;
using Mono.Cecil;

namespace ICSharpCode.Decompiler
{
	public enum DecompiledLanguages
	{
		IL,
		CSharp
	}
	
	/// <summary>
	/// Maps the source code to IL.
	/// </summary>
	public class SourceCodeMapping
	{
		/// <summary>
		/// Gets or sets the source code line number in the output.
		/// </summary>
		public int SourceCodeLine { get; internal set; }
		
		/// <summary>
		/// Gets or sets IL Range offset for the source code line. E.g.: 13-19 &lt;-&gt; 135.
		/// </summary>
		public ILRange ILInstructionOffset { get; internal set; }
		
		/// <summary>
		/// Gets or sets the member mapping this source code mapping belongs to.
		/// </summary>
		public MemberMapping MemberMapping { get; internal set; }
		
		/// <summary>
		/// Retrieves the array that contains the IL range and the missing gaps between ranges.
		/// </summary>
		/// <returns>The array representation of the step aranges.</returns>
		public int[] ToArray(bool isMatch)
		{
			var currentList = new List<ILRange>();
			
			// add list for the current source code line
			currentList.AddRange(ILRange.OrderAndJoint(MemberMapping.MemberCodeMappings
			                                           .FindAll(m => m.SourceCodeLine == this.SourceCodeLine)
			                                           .ConvertAll<ILRange>(m => m.ILInstructionOffset)));
			
			if (!isMatch) {
				// add inverted
				currentList.AddRange(MemberMapping.InvertedList);
			} else {
				// if the current list contains the last mapping, add also the last gap
				var lastInverted = MemberMapping.InvertedList.LastOrDefault();
				if (lastInverted != null && lastInverted.From == currentList[currentList.Count - 1].To)
					currentList.Add(lastInverted);
			}
			
			// set the output
			var resultList = new List<int>();
			foreach (var element in ILRange.OrderAndJoint(currentList)) {
				resultList.Add(element.From);
				resultList.Add(element.To);
			}
			
			return resultList.ToArray();
		}
	}
	
	/// <summary>
	/// Stores the method information and its source code mappings.
	/// </summary>
	public sealed class MemberMapping
	{
		IEnumerable<ILRange> invertedList;
		
		/// <summary>
		/// Gets or sets the type of the mapping.
		/// </summary>
		public TypeDefinition Type { get; internal set; }
		
		/// <summary>
		/// Metadata token of the method.
		/// </summary>
		public uint MetadataToken { get; internal set; }
		
		/// <summary>
		/// Gets or sets the code size for the member mapping.
		/// </summary>
		public int CodeSize { get; internal set; }
		
		/// <summary>
		/// Gets or sets the source code mappings.
		/// </summary>
		public List<SourceCodeMapping> MemberCodeMappings { get; internal set; }
		
		/// <summary>
		/// Gets the inverted IL Ranges.<br/>
		/// E.g.: for (0-9, 11-14, 14-18, 21-25) => (9-11,18-21).
		/// </summary>
		/// <returns>IL Range inverted list.</returns>
		public IEnumerable<ILRange> InvertedList
		{
			get {
				if (invertedList == null) {
					var list = MemberCodeMappings.ConvertAll<ILRange>(
						s => new ILRange { From = s.ILInstructionOffset.From, To = s.ILInstructionOffset.To });
					invertedList = ILRange.OrderAndJoint(ILRange.Invert(list, CodeSize));
				}
				return invertedList;
			}
		}
	}
	
	/// <summary>
	/// Code mappings helper class.
	/// </summary>
	public static class CodeMappings
	{
		/// <summary>
		/// Gets the storage of code mappings for a language.
		/// </summary>
		/// <param name="language">A language.</param>
		/// <returns>The storage of code mappings.</returns>
		public static ConcurrentDictionary<string, List<MemberMapping>> GetStorage(DecompiledLanguages language)
		{
			ConcurrentDictionary<string, List<MemberMapping>> storage = null;
			
			switch (language) {
				case DecompiledLanguages.IL:
					storage = ILCodeMapping.SourceCodeMappings;
					break;
				case DecompiledLanguages.CSharp:
					storage = CSharpCodeMapping.SourceCodeMappings;
					break;
				default:
					throw new System.Exception("Invalid value for DecompiledLanguages");
			}
			
			return storage;
		}
		
		/// <summary>
		/// Create code mapping for a method.
		/// </summary>
		/// <param name="method">Method to create the mapping for.</param>
		/// <param name="sourceCodeMappings">Source code mapping storage.</param>
		internal static MemberMapping CreateCodeMapping(
			this MethodDefinition member,
			ConcurrentDictionary<string, List<MemberMapping>> codeMappings)
		{
			if (member == null || !member.HasBody)
				return null;
			
			if (codeMappings == null)
				throw new ArgumentNullException("CodeMappings storage must be valid!");
			
			// create IL/CSharp code mappings - used in debugger
			MemberMapping currentMemberMapping = null;
			if (codeMappings.ContainsKey(member.DeclaringType.FullName)) {
				var mapping = codeMappings[member.DeclaringType.FullName];
				if (mapping.Find(map => (int)map.MetadataToken == member.MetadataToken.ToInt32()) == null) {
					currentMemberMapping = new MemberMapping() {
						MetadataToken = (uint)member.MetadataToken.ToInt32(),
						Type = member.DeclaringType.Resolve(),
						MemberCodeMappings = new List<SourceCodeMapping>(),
						CodeSize = member.Body.CodeSize
					};
					mapping.Add(currentMemberMapping);
				}
			}
			
			return currentMemberMapping;
		}
		
		/// <summary>
		/// Gets source code mapping and metadata token based on type name and line number.
		/// </summary>
		/// <param name="codeMappings">Code mappings storage.</param>
		/// <param name="typeName">Type name.</param>
		/// <param name="lineNumber">Line number.</param>
		/// <param name="metadataToken">Metadata token.</param>
		/// <returns></returns>
		public static SourceCodeMapping GetInstructionByTypeAndLine(
			this ConcurrentDictionary<string, List<MemberMapping>> codeMappings,
			string typeName,
			int lineNumber,
			out uint metadataToken)
		{
			if (codeMappings == null)
				throw new ArgumentNullException("CodeMappings storage must be valid!");
			
			if (!codeMappings.ContainsKey(typeName)) {
				metadataToken = 0;
				return null;
			}
			
			if (lineNumber <= 0) {
				metadataToken = 0;
				return null;
			}
			
			var methodMappings = codeMappings[typeName];
			foreach (var maping in methodMappings) {
				var map = maping.MemberCodeMappings.Find(m => m.SourceCodeLine == lineNumber);
				if (map != null) {
					metadataToken = maping.MetadataToken;
					return map;
				}
			}
			
			metadataToken = 0;
			return null;
		}
		
		/// <summary>
		/// Gets a mapping given a type, a token and an IL offset.
		/// </summary>
		/// <param name="codeMappings">Code mappings storage.</param>
		/// <param name="typeName">Type name.</param>
		/// <param name="token">Token.</param>
		/// <param name="ilOffset">IL offset.</param>
		/// <param name="isMatch">True, if perfect match.</param>
		/// <returns>A code mapping.</returns>
		public static SourceCodeMapping GetInstructionByTypeTokenAndOffset(
			this ConcurrentDictionary<string, List<MemberMapping>> codeMappings,
			string typeName,
			uint token,
			int ilOffset, out bool isMatch)
		{
			isMatch = false;
			typeName = typeName.Replace("+", "/");
			
			if (codeMappings == null)
				throw new ArgumentNullException("CodeMappings storage must be valid!");
			
			if (!codeMappings.ContainsKey(typeName)) {
				return null;
			}
			
			var methodMappings = codeMappings[typeName];
			var maping = methodMappings.Find(m => m.MetadataToken == token);
			
			if (maping == null) {
				return null;
			}
			
			// try find an exact match
			var map = maping.MemberCodeMappings.Find(m => m.ILInstructionOffset.From <= ilOffset && ilOffset < m.ILInstructionOffset.To);
			
			if (map == null) {
				// get the immediate next one
				map = maping.MemberCodeMappings.Find(m => m.ILInstructionOffset.From >= ilOffset);
				isMatch = false;
				if (map == null)
					map = maping.MemberCodeMappings.LastOrDefault(); // get the last
				
				return map;
			}
			
			isMatch = true;
			return map;
		}
		
		/// <summary>
		/// Gets the source code and type name from metadata token and offset.
		/// </summary>
		/// <param name="codeMappings">Code mappings storage.</param>
		/// <param name="typeName">Current type name.</param>
		/// <param name="token">Metadata token.</param>
		/// <param name="ilOffset">IL offset.</param>
		/// <param name="typeName">Type definition.</param>
		/// <param name="line">Line number.</param>
		/// <remarks>It is possible to exist to different types from different assemblies with the same metadata token.</remarks>
		public static bool GetSourceCodeFromMetadataTokenAndOffset(
			this ConcurrentDictionary<string, List<MemberMapping>> codeMappings,
			string typeName,
			uint token,
			int ilOffset,
			out TypeDefinition type,
			out int line)
		{
			type = null;
			line = 0;
			
			if (codeMappings == null)
				throw new ArgumentNullException("CodeMappings storage must be valid!");
			
			typeName = typeName.Replace("+", "/");
			if (!codeMappings.ContainsKey(typeName))
				return false;
			
			var mapping = codeMappings[typeName].Find(m => m.MetadataToken == token);
			if (mapping == null)
				return false;
			
			var codeMapping = mapping.MemberCodeMappings.Find(
				cm => cm.ILInstructionOffset.From <= ilOffset && ilOffset <= cm.ILInstructionOffset.To - 1);
			if (codeMapping == null) {
				codeMapping = mapping.MemberCodeMappings.Find(cm => (cm.ILInstructionOffset.From >= ilOffset));
				if (codeMapping == null) {
					codeMapping = mapping.MemberCodeMappings.LastOrDefault();
					if (codeMapping == null)
						return false;
				}
			}
			
			type = mapping.Type;
			line = codeMapping.SourceCodeLine;
			return true;
		}
	}
}
