// Copyright (c) AlphaSierraPapa for the SharpDevelop Team
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
