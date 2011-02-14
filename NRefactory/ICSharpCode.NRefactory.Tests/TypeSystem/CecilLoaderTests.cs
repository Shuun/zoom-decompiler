﻿// Copyright (c) 2010 AlphaSierraPapa for the SharpDevelop Team (for details please see \doc\copyright.txt)
// This code is distributed under MIT X11 license (for details please see \doc\license.txt)

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ICSharpCode.NRefactory.TypeSystem.Implementation;
using NUnit.Framework;

namespace ICSharpCode.NRefactory.TypeSystem
{
	[TestFixture]
	public class CecilLoaderTests : TypeSystemTests
	{
		public static readonly IProjectContent Mscorlib = new CecilLoader().LoadAssemblyFile(typeof(object).Assembly.Location);
		ITypeResolveContext ctx = Mscorlib;
		
		[TestFixtureSetUp]
		public void FixtureSetUp()
		{
			// use "IncludeInternalMembers" so that Cecil results match C# parser results
			CecilLoader loader = new CecilLoader() { IncludeInternalMembers = true };
			testCasePC = loader.LoadAssemblyFile(typeof(TestCase.SimplePublicClass).Assembly.Location);
		}
		
		[Test]
		public void InheritanceTest()
		{
			ITypeDefinition c = Mscorlib.GetClass(typeof(SystemException));
			ITypeDefinition c2 = Mscorlib.GetClass(typeof(Exception));
			Assert.IsNotNull(c, "c is null");
			Assert.IsNotNull(c2, "c2 is null");
			//Assert.AreEqual(3, c.BaseTypes.Count); // Inherited interfaces are not reported by Cecil
			// which matches the behaviour of our C#/VB parsers
			Assert.AreEqual("System.Exception", c.BaseTypes[0].Resolve(ctx).FullName);
			Assert.AreSame(c2, c.BaseTypes[0]);
			
			string[] superTypes = c.GetAllBaseTypes(ctx).Select(t => t.ToString()).ToArray();
			Assert.AreEqual(new string[] {
			                	"System.SystemException", "System.Exception", "System.Object",
			                	"System.Runtime.Serialization.ISerializable", "System.Runtime.InteropServices._Exception"
			                }, superTypes);
		}
		
		[Test]
		public void GenericPropertyTest()
		{
			ITypeDefinition c = Mscorlib.GetClass(typeof(Comparer<>));
			IProperty def = c.Properties.Single(p => p.Name == "Default");
			ParameterizedType pt = (ParameterizedType)def.ReturnType.Resolve(ctx);
			Assert.AreEqual("System.Collections.Generic.Comparer", pt.FullName);
			Assert.AreSame(c.TypeParameters[0], pt.TypeArguments[0]);
		}
		
		[Test]
		public void PointerTypeTest()
		{
			ITypeDefinition c = Mscorlib.GetClass(typeof(IntPtr));
			IMethod toPointer = c.Methods.Single(p => p.Name == "ToPointer");
			Assert.AreEqual("System.Void*", toPointer.ReturnType.Resolve(ctx).ReflectionName);
			Assert.IsTrue (toPointer.ReturnType.Resolve(ctx) is PointerType);
			Assert.AreEqual("System.Void", ((PointerType)toPointer.ReturnType.Resolve(ctx)).ElementType.FullName);
		}
		
		[Test]
		public void DateTimeDefaultConstructor()
		{
			ITypeDefinition c = Mscorlib.GetClass(typeof(DateTime));
			Assert.IsFalse(c.Methods.Any(m => m.IsConstructor && m.Parameters.Count == 0)); // struct ctor isn't declared
			// but it is implicit:
			Assert.IsTrue(c.GetConstructors(ctx).Any(m => m.Parameters.Count == 0));
		}
		
		[Test]
		public void NoEncodingInfoDefaultConstructor()
		{
			ITypeDefinition c = Mscorlib.GetClass(typeof(EncodingInfo));
			// EncodingInfo only has an internal constructor
			Assert.IsFalse(c.Methods.Any(m => m.IsConstructor));
			// and no implicit ctor should be added:
			Assert.AreEqual(0, c.GetConstructors(ctx).Count());
		}
		
		[Test]
		public void StaticModifierTest()
		{
			ITypeDefinition c = Mscorlib.GetClass(typeof(Environment));
			Assert.IsNotNull(c, "System.Environment not found");
			Assert.IsTrue(c.IsAbstract, "class should be abstract");
			Assert.IsTrue(c.IsSealed, "class should be sealed");
			Assert.IsTrue(c.IsStatic, "class should be static");
		}
		
		[Test]
		public void InnerClassReferenceTest()
		{
			ITypeDefinition c = Mscorlib.GetClass(typeof(Environment));
			Assert.IsNotNull(c, "System.Environment not found");
			ITypeReference rt = c.Methods.First(m => m.Name == "GetFolderPath").Parameters[0].Type;
			Assert.AreSame(c.InnerClasses.Single(ic => ic.Name == "SpecialFolder"), rt.Resolve(ctx));
		}
		
		[Test]
		public void InnerClassesTest()
		{
			ITypeDefinition c = Mscorlib.GetClass(typeof(Environment.SpecialFolder));
			Assert.IsNotNull(c, "c is null");
			Assert.AreEqual("System.Environment.SpecialFolder", c.FullName);
			Assert.AreEqual("System.Environment+SpecialFolder", c.ReflectionName);
		}
		
		[Test]
		public void VoidTest()
		{
			ITypeDefinition c = Mscorlib.GetClass(typeof(void));
			Assert.IsNotNull(c, "System.Void not found");
			Assert.AreEqual(0, c.GetMethods(ctx).Count());
			Assert.AreEqual(0, c.GetProperties(ctx).Count());
			Assert.AreEqual(0, c.GetEvents(ctx).Count());
			Assert.AreEqual(0, c.GetFields(ctx).Count());
			Assert.AreEqual(
				new string[] {
					"[System.SerializableAttribute]",
					"[System.Runtime.InteropServices.StructLayoutAttribute(0, Size=1)]",
					"[System.Runtime.InteropServices.ComVisibleAttribute(true)]"
				},
				c.Attributes.Select(a => a.ToString()).ToArray());
		}
		
		[Test]
		public void NestedClassInGenericClassTest()
		{
			ITypeDefinition dictionary = Mscorlib.GetClass(typeof(Dictionary<,>));
			Assert.IsNotNull(dictionary);
			ITypeDefinition valueCollection = Mscorlib.GetClass(typeof(Dictionary<,>.ValueCollection));
			Assert.IsNotNull(valueCollection);
			var dictionaryRT = new ParameterizedType(dictionary, new[] { Mscorlib.GetClass(typeof(string)), Mscorlib.GetClass(typeof(int)) });
			IProperty valueProperty = dictionaryRT.GetProperties(ctx).Single(p => p.Name == "Values");
			IType parameterizedValueCollection = valueProperty.ReturnType.Resolve(ctx);
			Assert.AreEqual("System.Collections.Generic.Dictionary`2+ValueCollection[[System.String],[System.Int32]]", parameterizedValueCollection.ReflectionName);
			Assert.AreSame(valueCollection, parameterizedValueCollection.GetDefinition());
		}
		
		[Test]
		public void ValueCollectionCountModifiers()
		{
			ITypeDefinition valueCollection = Mscorlib.GetClass(typeof(Dictionary<,>.ValueCollection));
			Assert.AreEqual(Accessibility.Public, valueCollection.Accessibility);
			Assert.IsTrue(valueCollection.IsSealed);
			Assert.IsFalse(valueCollection.IsAbstract);
			Assert.IsFalse(valueCollection.IsStatic);
			
			IProperty count = valueCollection.Properties.Single(p => p.Name == "Count");
			Assert.AreEqual(Accessibility.Public, count.Accessibility);
			// It's sealed on the IL level; but in C# it's just a normal non-virtual method that happens to implement an interface
			Assert.IsFalse(count.IsSealed);
			Assert.IsFalse(count.IsVirtual);
			Assert.IsFalse(count.IsAbstract);
		}
		
		[Test]
		public void MathAcosModifiers()
		{
			ITypeDefinition math = Mscorlib.GetClass(typeof(Math));
			Assert.AreEqual(Accessibility.Public, math.Accessibility);
			Assert.IsTrue(math.IsSealed);
			Assert.IsTrue(math.IsAbstract);
			Assert.IsTrue(math.IsStatic);
			
			IMethod acos = math.Methods.Single(p => p.Name == "Acos");
			Assert.AreEqual(Accessibility.Public, acos.Accessibility);
			Assert.IsTrue(acos.IsStatic);
			Assert.IsFalse(acos.IsAbstract);
			Assert.IsFalse(acos.IsSealed);
			Assert.IsFalse(acos.IsVirtual);
			Assert.IsFalse(acos.IsOverride);
		}
		
		[Test]
		public void EncodingModifiers()
		{
			ITypeDefinition encoding = Mscorlib.GetClass(typeof(Encoding));
			Assert.AreEqual(Accessibility.Public, encoding.Accessibility);
			Assert.IsFalse(encoding.IsSealed);
			Assert.IsTrue(encoding.IsAbstract);
			
			IMethod getDecoder = encoding.Methods.Single(p => p.Name == "GetDecoder");
			Assert.AreEqual(Accessibility.Public, getDecoder.Accessibility);
			Assert.IsFalse(getDecoder.IsStatic);
			Assert.IsFalse(getDecoder.IsAbstract);
			Assert.IsFalse(getDecoder.IsSealed);
			Assert.IsTrue(getDecoder.IsVirtual);
			Assert.IsFalse(getDecoder.IsOverride);
			
			IMethod getMaxByteCount = encoding.Methods.Single(p => p.Name == "GetMaxByteCount");
			Assert.AreEqual(Accessibility.Public, getMaxByteCount.Accessibility);
			Assert.IsFalse(getMaxByteCount.IsStatic);
			Assert.IsTrue(getMaxByteCount.IsAbstract);
			Assert.IsFalse(getMaxByteCount.IsSealed);
			Assert.IsFalse(getMaxByteCount.IsVirtual);
			Assert.IsFalse(getMaxByteCount.IsOverride);
			
			IProperty encoderFallback = encoding.Properties.Single(p => p.Name == "EncoderFallback");
			Assert.AreEqual(Accessibility.Public, encoderFallback.Accessibility);
			Assert.IsFalse(encoderFallback.IsStatic);
			Assert.IsFalse(encoderFallback.IsAbstract);
			Assert.IsFalse(encoderFallback.IsSealed);
			Assert.IsFalse(encoderFallback.IsVirtual);
			Assert.IsFalse(encoderFallback.IsOverride);
		}
		
		[Test]
		public void UnicodeEncodingModifiers()
		{
			ITypeDefinition encoding = Mscorlib.GetClass(typeof(UnicodeEncoding));
			Assert.AreEqual(Accessibility.Public, encoding.Accessibility);
			Assert.IsFalse(encoding.IsSealed);
			Assert.IsFalse(encoding.IsAbstract);
			
			IMethod getDecoder = encoding.Methods.Single(p => p.Name == "GetDecoder");
			Assert.AreEqual(Accessibility.Public, getDecoder.Accessibility);
			Assert.IsFalse(getDecoder.IsStatic);
			Assert.IsFalse(getDecoder.IsAbstract);
			Assert.IsFalse(getDecoder.IsSealed);
			Assert.IsFalse(getDecoder.IsVirtual);
			Assert.IsTrue(getDecoder.IsOverride);
		}
		
		[Test]
		public void UTF32EncodingModifiers()
		{
			ITypeDefinition encoding = Mscorlib.GetClass(typeof(UTF32Encoding));
			Assert.AreEqual(Accessibility.Public, encoding.Accessibility);
			Assert.IsTrue(encoding.IsSealed);
			Assert.IsFalse(encoding.IsAbstract);
			
			IMethod getDecoder = encoding.Methods.Single(p => p.Name == "GetDecoder");
			Assert.AreEqual(Accessibility.Public, getDecoder.Accessibility);
			Assert.IsFalse(getDecoder.IsStatic);
			Assert.IsFalse(getDecoder.IsAbstract);
			Assert.IsFalse(getDecoder.IsSealed);
			Assert.IsFalse(getDecoder.IsVirtual);
			Assert.IsTrue(getDecoder.IsOverride);
		}
	}
}
