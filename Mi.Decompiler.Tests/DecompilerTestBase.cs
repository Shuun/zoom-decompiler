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
            string text = SampleInputFiles.ResourceManager.GetString(samplesFileName+"_cs");
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

		public static string RemoveIgnorableLines(IEnumerable<string> lines)
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
			AssemblyDefinition assembly = SampleInputLoader.LoadAssembly(testClassName);
            var ctx = new DecompilerContext(assembly.MainModule) { Settings = settings };
			AstBuilder decompiler = new AstBuilder(ctx);
			decompiler.AddAssembly(assembly);
			new Helpers.RemoveCompilerAttribute().Run(decompiler.CompilationUnit);
			StringWriter output = new StringWriter();
			decompiler.GenerateCode(new PlainTextOutput(output));
			return output.ToString();
		}
	}
}
