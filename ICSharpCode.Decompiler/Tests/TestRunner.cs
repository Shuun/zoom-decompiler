// Copyright (c) AlphaSierraPapa for the SharpDevelop Team (for details please see \doc\copyright.txt)
// This code is distributed under MIT X11 license (for details please see \doc\license.txt)

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using DiffLib;
using Mi.Decompiler.Ast;
using Mi.Decompiler.Tests.Helpers;
using Mi.Assemblies;
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