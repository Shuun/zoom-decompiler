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
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Mi.Assemblies;
using Mi.Decompiler.AstServices;
using Mi.Decompiler.Tests.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Mi.Decompiler.Tests
{
	[TestClass]
	public class TestRunner
	{
        // disambiguating overloads is not yet implemented
		[TestMethod]
		public void CallOverloadedMethod()
		{
			TestFile(@"CallOverloadedMethod");
		}
		
        // unncessary primitive casts
		[TestMethod]
		public void CheckedUnchecked()
		{
			TestFile(@"CheckedUnchecked");
		}
		
        // Missing cast on null
		[TestMethod]
		public void DelegateConstruction()
		{
			TestFile(@"DelegateConstruction");
		}
		
        // arg-Variables in catch clauses
		[TestMethod]
		public void ExceptionHandling()
		{
			TestFile(@"ExceptionHandling");
		}
		
		[TestMethod]
		public void Generics()
		{
			TestFile(@"Generics");
		}
		
		[TestMethod]
		public void IncrementDecrement()
		{
			TestFile(@"IncrementDecrement");
		}
		
        // Formatting issues (array initializers not on single line)
		[TestMethod]
		public void InitializerTests()
		{
			TestFile(@"InitializerTests");
		}
		
		[TestMethod]
		public void Loops()
		{
			TestFile(@"Loops");
		}
		
		[TestMethod]
		public void MultidimensionalArray()
		{
			TestFile(@"MultidimensionalArray");
		}
		
		[TestMethod]
		public void PropertiesAndEvents()
		{
			TestFile(@"PropertiesAndEvents");
		}
		
        // Formatting differences in anonymous method create expressions
		[TestMethod]
		public void QueryExpressions()
		{
			TestFile(@"QueryExpressions");
		}
		
        // switch transform doesn't recreate the exact original switch
		[TestMethod]
		public void Switch()
		{
			TestFile(@"Switch");
		}
		
		[TestMethod]
		public void UndocumentedExpressions()
		{
			TestFile(@"UndocumentedExpressions");
		}
		
        // has incorrect casts to IntPtr
		[TestMethod]
		public void UnsafeCode()
		{
			TestFile(@"UnsafeCode");
		}
		
		[TestMethod]
		public void ValueTypes()
		{
			TestFile(@"ValueTypes");
		}
		
        // Redundant yield break; not removed
		[TestMethod]
		public void YieldReturn()
		{
			TestFile(@"YieldReturn");
		}
		
		static void TestFile(string fileName)
		{
            string code = SampleInputFiles.ResourceManager.GetString(fileName+"_cs");
            AssemblyDefinition assembly = SampleInputLoader.LoadAssembly(fileName);
			AstBuilder decompiler = new AstBuilder(new DecompilerContext(assembly.MainModule));
			decompiler.AddAssembly(assembly);
			new Helpers.RemoveCompilerAttribute().Run(decompiler.CompilationUnit);
			StringWriter output = new StringWriter();
			decompiler.GenerateCode(new PlainTextOutput(output));

			CodeAssert.AreEqual(code, output.ToString());
		}
	}
}
