// Copyright (c) AlphaSierraPapa for the SharpDevelop Team (for details please see \doc\copyright.txt)
// This code is distributed under MIT X11 license (for details please see \doc\license.txt)

using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using ICSharpCode.NRefactory.Documentation;
using Mono.Cecil;

namespace ICSharpCode.ILSpy.XmlDoc
{
	/// <summary>
	/// Helps finding and loading .xml documentation.
	/// </summary>
	public static class XmlDocLoader
	{
		static readonly Lazy<XmlDocumentationProvider> mscorlibDocumentation = new Lazy<XmlDocumentationProvider>(LoadMscorlibDocumentation);
		static readonly ConditionalWeakTable<ModuleDefinition, XmlDocumentationProvider> cache = new ConditionalWeakTable<ModuleDefinition, XmlDocumentationProvider>();
		
		static XmlDocumentationProvider LoadMscorlibDocumentation()
		{
			string xmlDocFile = FindXmlDocumentation("mscorlib.dll", TargetRuntime.Net_4_0)
				?? FindXmlDocumentation("mscorlib.dll", TargetRuntime.Net_2_0);
			if (xmlDocFile != null)
				return new XmlDocumentationProvider(xmlDocFile);
			else
				return null;
		}
		
		public static XmlDocumentationProvider MscorlibDocumentation {
			get { return mscorlibDocumentation.Value; }
		}
		
		public static XmlDocumentationProvider LoadDocumentation(ModuleDefinition module)
		{
			if (module == null)
				throw new ArgumentNullException("module");
			lock (cache) {
				XmlDocumentationProvider xmlDoc;
				if (!cache.TryGetValue(module, out xmlDoc)) {
					string xmlDocFile = LookupLocalizedXmlDoc(module.FullyQualifiedName);
					if (xmlDocFile == null) {
						xmlDocFile = FindXmlDocumentation(Path.GetFileName(module.FullyQualifiedName), module.Runtime);
					}
					if (xmlDocFile != null) {
						xmlDoc = new XmlDocumentationProvider(xmlDocFile);
						cache.Add(module, xmlDoc);
					} else {
						xmlDoc = null;
					}
				}
				return xmlDoc;
			}
		}
		
		static readonly string referenceAssembliesPath = Path.Combine(
#if DOTNET35
            Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles).Replace(" (x86)", ""),
#else
            Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
#endif
            @"Reference Assemblies\Microsoft\\Framework");
		static readonly string frameworkPath = Path.Combine(
#if DOTNET35
            Path.GetDirectoryName(Environment.GetFolderPath(Environment.SpecialFolder.System)),
#else
            Environment.GetFolderPath(Environment.SpecialFolder.Windows),
#endif
            @"Microsoft.NET\Framework");
		
		static string FindXmlDocumentation(string assemblyFileName, TargetRuntime runtime)
		{
			string fileName;
			switch (runtime) {
				case TargetRuntime.Net_1_0:
                    fileName = LookupLocalizedXmlDoc(DotNet35Compat.PathCombine(frameworkPath, "v1.0.3705", assemblyFileName));
					break;
				case TargetRuntime.Net_1_1:
                    fileName = LookupLocalizedXmlDoc(DotNet35Compat.PathCombine(frameworkPath, "v1.1.4322", assemblyFileName));
					break;
				case TargetRuntime.Net_2_0:
                    fileName = LookupLocalizedXmlDoc(DotNet35Compat.PathCombine(frameworkPath, "v2.0.50727", assemblyFileName))
						?? LookupLocalizedXmlDoc(Path.Combine(referenceAssembliesPath, "v3.5"))
						?? LookupLocalizedXmlDoc(Path.Combine(referenceAssembliesPath, "v3.0"))
						?? LookupLocalizedXmlDoc(Path.Combine(referenceAssembliesPath, @".NETFramework\v3.5\Profile\Client"));
					break;
				case TargetRuntime.Net_4_0:
				default:
                    fileName = LookupLocalizedXmlDoc(DotNet35Compat.PathCombine(referenceAssembliesPath, @".NETFramework\v4.0", assemblyFileName))
                        ?? LookupLocalizedXmlDoc(DotNet35Compat.PathCombine(frameworkPath, "v4.0.30319", assemblyFileName));
					break;
			}
			return fileName;
		}
		
		static string LookupLocalizedXmlDoc(string fileName)
		{
			string xmlFileName = Path.ChangeExtension(fileName, ".xml");
			string currentCulture = System.Threading.Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName;
			string localizedXmlDocFile = GetLocalizedName(xmlFileName, currentCulture);
			
			Debug.WriteLine("Try find XMLDoc @" + localizedXmlDocFile);
			if (File.Exists(localizedXmlDocFile)) {
				return localizedXmlDocFile;
			}
			Debug.WriteLine("Try find XMLDoc @" + xmlFileName);
			if (File.Exists(xmlFileName)) {
				return xmlFileName;
			}
			if (currentCulture != "en") {
				string englishXmlDocFile = GetLocalizedName(xmlFileName, "en");
				Debug.WriteLine("Try find XMLDoc @" + englishXmlDocFile);
				if (File.Exists(englishXmlDocFile)) {
					return englishXmlDocFile;
				}
			}
			return null;
		}
		
		static string GetLocalizedName(string fileName, string language)
		{
			string localizedXmlDocFile = Path.GetDirectoryName(fileName);
			localizedXmlDocFile = Path.Combine(localizedXmlDocFile, language);
			localizedXmlDocFile = Path.Combine(localizedXmlDocFile, Path.GetFileName(fileName));
			return localizedXmlDocFile;
		}
	}
}
