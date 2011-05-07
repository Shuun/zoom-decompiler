using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using Mi.Decompiler.Ast;
using Mi.Decompiler.Tests.Helpers;
using Mi.Assemblies;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using SampleInputAssemblyFiles = Mi.Decompiler.Tests.Decompiler.SampleInputAssemblyFiles;

namespace Mi.Decompiler.Tests
{
	public abstract class DecompilerTestBase
	{
		protected static void ValidateFileRoundtrip(string samplesFileName)
		{
			var lines = GetFile(samplesFileName);
			var testCode = RemoveIgnorableLines(lines);
			var decompiledTestCode = RoundtripCode(samplesFileName, testCode);
			CodeAssert.AreEqual(testCode, decompiledTestCode);
		}

        static IEnumerable<string> GetFile(string samplesFileName)
        {
            string text = SampleInputAssemblyFiles.ResourceManager.GetString(samplesFileName);
            var reader = new StringReader(text);
            while (true)
            {
                string line = reader.ReadLine();
                if (line == null)
                    yield break;
                else
                    yield return line;
            }
        }

		static string RemoveIgnorableLines(IEnumerable<string> lines)
		{
			return CodeSampleFileParser.ConcatLines(lines.Where(l => !CodeSampleFileParser.IsCommentOrBlank(l)));
		}

		/// <summary>
		/// Compiles and decompiles a source code.
		/// </summary>
		/// <param name="code">The source code to copile.</param>
		/// <returns>The decompilation result of compiled source code.</returns>
		static string RoundtripCode(string testClassName, string code)
		{
			DecompilerSettings settings = new DecompilerSettings();
			settings.FullyQualifyAmbiguousTypeNames = false;
			AssemblyDefinition assembly = AssemblyDefinition.ReadAssembly(new MemoryStream(SampleInputAssemblyFiles.SampleInputAssembly));
            var testClass = assembly.MainModule.Types.First(c => c.Name == testClassName); 
			AstBuilder decompiler = new AstBuilder(new DecompilerContext(assembly.MainModule) { Settings = settings });
			decompiler.AddType(testClass);
			new Helpers.RemoveCompilerAttribute().Run(decompiler.CompilationUnit);
			StringWriter output = new StringWriter();
			decompiler.GenerateCode(new PlainTextOutput(output));
			return output.ToString();
		}
	}
}
