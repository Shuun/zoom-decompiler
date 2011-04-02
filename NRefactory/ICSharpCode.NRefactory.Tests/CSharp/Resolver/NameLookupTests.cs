﻿// Copyright (c) AlphaSierraPapa for the SharpDevelop Team (for details please see \doc\copyright.txt)
// This code is distributed under MIT X11 license (for details please see \doc\license.txt)

using System;
using System.Collections.Generic;
using System.Linq;

using ICSharpCode.NRefactory.TypeSystem;
using NUnit.Framework;

namespace ICSharpCode.NRefactory.CSharp.Resolver
{
	[TestFixture]
	public class NameLookupTests : ResolverTestBase
	{
		[Test]
		public void SimpleNameLookupWithoutContext()
		{
			// nothing should be found without specifying any UsingScope - however, the resolver also must not crash
			resolver.UsingScope = null;
			Assert.IsTrue(resolver.ResolveSimpleName("System", new IType[0]).IsError);
		}
		
		[Test]
		public void SimpleNamespaceLookup()
		{
			NamespaceResolveResult nrr = (NamespaceResolveResult)resolver.ResolveSimpleName("System", new IType[0]);
			Assert.AreEqual("System", nrr.NamespaceName);
			Assert.AreSame(SharedTypes.UnknownType, nrr.Type);
		}
		
		[Test]
		public void NamespaceInParentNamespaceLookup()
		{
			resolver.UsingScope = MakeUsingScope("System.Collections.Generic");
			NamespaceResolveResult nrr = (NamespaceResolveResult)resolver.ResolveSimpleName("Text", new IType[0]);
			Assert.AreEqual("System.Text", nrr.NamespaceName);
		}
		
		[Test]
		public void NamespacesAreNotImported()
		{
			AddUsing("System");
			Assert.IsTrue(resolver.ResolveSimpleName("Collections", new IType[0]).IsError);
		}
		
		[Test]
		public void ImportedType()
		{
			AddUsing("System");
			TypeResolveResult trr = (TypeResolveResult)resolver.ResolveSimpleName("String", new IType[0]);
			Assert.AreEqual("System.String", trr.Type.FullName);
		}
		
		[Test]
		public void UnknownIdentifierTest()
		{
			UnknownIdentifierResolveResult uirr = (UnknownIdentifierResolveResult)resolver.ResolveSimpleName("xyz", new IType[0]);
			Assert.IsTrue(uirr.IsError);
			Assert.AreEqual("xyz", uirr.Identifier);
		}
		
		[Test]
		public void GlobalIsUnknownIdentifier()
		{
			Assert.IsTrue(resolver.ResolveSimpleName("global", new IType[0]).IsError);
		}
		
		[Test]
		public void GlobalIsAlias()
		{
			NamespaceResolveResult nrr = (NamespaceResolveResult)resolver.ResolveAlias("global");
			Assert.AreEqual("", nrr.NamespaceName);
		}
		
		[Test]
		public void AliasToImportedType()
		{
			AddUsing("System");
			AddUsingAlias("x", "String");
			TypeResolveResult trr = (TypeResolveResult)resolver.ResolveSimpleName("x", new IType[0]);
			// Unknown type (as String isn't looked up in System)
			Assert.AreSame(SharedTypes.UnknownType, trr.Type);
		}
		
		[Test]
		public void AliasToImportedType2()
		{
			AddUsing("System");
			resolver.UsingScope = new UsingScope(resolver.UsingScope, "SomeNamespace");
			AddUsingAlias("x", "String");
			TypeResolveResult trr = (TypeResolveResult)resolver.ResolveSimpleName("x", new IType[0]);
			Assert.AreEqual("System.String", trr.Type.FullName);
		}
		
		[Test]
		public void AliasOperatorOnTypeAlias()
		{
			AddUsingAlias("x", "System.String");
			Assert.IsTrue(resolver.ResolveAlias("x").IsError);
		}
		
		[Test]
		public void AliasOperatorOnNamespaceAlias()
		{
			AddUsingAlias("x", "System.Collections.Generic");
			NamespaceResolveResult nrr = (NamespaceResolveResult)resolver.ResolveAlias("x");
			Assert.AreEqual("System.Collections.Generic", nrr.NamespaceName);
		}
		
		[Test]
		public void AliasOperatorOnNamespace()
		{
			Assert.IsTrue(resolver.ResolveAlias("System").IsError);
		}
		
		[Test]
		public void FindClassInCurrentNamespace()
		{
			resolver.UsingScope = MakeUsingScope("System.Collections");
			TypeResolveResult trr = (TypeResolveResult)resolver.ResolveSimpleName("String", new IType[0]);
			Assert.AreEqual("System.String", trr.Type.FullName);
		}
		
		[Test]
		public void FindNeighborNamespace()
		{
			resolver.UsingScope = MakeUsingScope("System.Collections");
			NamespaceResolveResult nrr = (NamespaceResolveResult)resolver.ResolveSimpleName("Text", new IType[0]);
			Assert.AreEqual("System.Text", nrr.NamespaceName);
		}
		
		[Test]
		public void FindTypeParameters()
		{
			resolver.UsingScope = MakeUsingScope("System.Collections.Generic");
			resolver.CurrentTypeDefinition = context.GetClass(typeof(List<>));
			resolver.CurrentMember = resolver.CurrentTypeDefinition.Methods.Single(m => m.Name == "ConvertAll");
			
			TypeResolveResult trr;
			trr = (TypeResolveResult)resolver.ResolveSimpleName("TOutput", new IType[0]);
			Assert.AreSame(((IMethod)resolver.CurrentMember).TypeParameters[0], trr.Type);
			
			trr = (TypeResolveResult)resolver.ResolveSimpleName("T", new IType[0]);
			Assert.AreSame(resolver.CurrentTypeDefinition.TypeParameters[0], trr.Type);
		}
		
		[Test]
		public void SimpleParameter()
		{
			string program = @"class A {
	void Method(string a) {
		string b = $a$;
	}
}
";
			LocalResolveResult result = Resolve<LocalResolveResult>(program);
			Assert.AreEqual("a", result.Variable.Name);
			Assert.IsTrue(result.IsParameter);
			Assert.AreEqual("System.String", result.Type.FullName);
		}
		
		[Test]
		public void SimpleLocalVariable()
		{
			string program = @"class A {
	void Method() {
		string a;
		string b = $a$;
	}
}
";
			LocalResolveResult result = Resolve<LocalResolveResult>(program);
			Assert.AreEqual("a", result.Variable.Name);
			Assert.IsFalse(result.IsParameter);
			
			Assert.AreEqual("System.String", result.Type.FullName);
		}
		
		[Test]
		public void UnknownTypeTest()
		{
			string program = @"class A {
	void Method($StringBuilder$ b) {
	}
}
";
			UnknownIdentifierResolveResult result = Resolve<UnknownIdentifierResolveResult>(program);
			Assert.AreEqual("StringBuilder", result.Identifier);
			
			Assert.AreSame(SharedTypes.UnknownType, result.Type);
		}
		
		[Test, Ignore("not yet implemented (depends on distuishing types and expressions in the DOM)")]
		public void PropertyNameAmbiguousWithTypeName()
		{
			string program = @"class A {
	public Color Color { get; set; }
	
	void Method() {
		$
	}
}
class Color { public static readonly Color Empty = null; }
";
			TypeResolveResult trr = Resolve<TypeResolveResult>(program.Replace("$", "$Color$ c;"));
			Assert.AreEqual("Color", trr.Type.Name);
			
			MemberResolveResult mrr = Resolve<MemberResolveResult>(program.Replace("$", "x = $Color$;"));
			Assert.AreEqual("Color", mrr.Member.Name);
			
			Resolve<MemberResolveResult>(program.Replace("$", "$Color$ = Color.Empty;"));
			Resolve<TypeResolveResult>(program.Replace("$", "Color = $Color$.Empty;"));
			Resolve<MemberResolveResult>(program.Replace("$", "x = $Color$.ToString();"));
		}
		
		[Test]
		public void ValueInsideSetterTest()
		{
			string program = @"class A {
	public string Property {
		set {
			var a = $value$;
		}
	}
}
";
			LocalResolveResult result = Resolve<LocalResolveResult>(program);
			Assert.AreEqual("System.String", result.Type.FullName);
			Assert.AreEqual("value", result.Variable.Name);
		}
		
		[Test, Ignore("type references not supported")]
		public void ValueInsideEventTest()
		{
			string program = @"using System; class A {
	public event EventHandler Ev {
		add {
			var a = $value$;
		}
		remove {}
	}
}
";
			LocalResolveResult result = Resolve<LocalResolveResult>(program);
			Assert.AreEqual("System.EventHandler", result.Type.FullName);
			Assert.AreEqual("value", result.Variable.Name);
		}
		
		[Test]
		public void ValueInsideIndexerSetterTest()
		{
			string program = @"using System; class A {
	public string this[int arg] {
		set {
			var a = $value$;
		}
	}
}
";
			LocalResolveResult result = Resolve<LocalResolveResult>(program);
			Assert.AreEqual("System.String", result.Type.FullName);
			Assert.AreEqual("value", result.Variable.Name);
		}
		
		[Test, Ignore("Anonymous method parameters not supported by parser")]
		public void AnonymousMethodParameters()
		{
			string program = @"using System;
class A {
	void Method() {
		SomeEvent += delegate(object sender, EventArgs e) {
			$e$.ToString();
		};
	} }
";
			LocalResolveResult result = Resolve<LocalResolveResult>(program);
			Assert.AreEqual("System.EventArgs", result.Type.FullName);
			Assert.AreEqual("e", result.Variable.Name);
		}
		
		[Test]
		public void DefaultTypeCSharp()
		{
			string program = @"class A {
	void Method() {
		$int$ a;
	} }
";
			TypeResolveResult result = Resolve<TypeResolveResult>(program);
			Assert.AreEqual("System.Int32", result.Type.FullName);
		}
		
		[Test]
		public void LoopVariableScopeTest()
		{
			string program = @"using System;
class TestClass {
	void Test() {
		for (int i = 0; i < 10; i++) {
			$1$.ToString();
		}
		for (long i = 0; i < 10; i++) {
			$2$.ToString();
		}
	}
}
";
			LocalResolveResult lr = Resolve<LocalResolveResult>(program.Replace("$1$", "$i$").Replace("$2$", "i"));
			Assert.AreEqual("System.Int32", lr.Type.ReflectionName);
			
			lr = Resolve<LocalResolveResult>(program.Replace("$1$", "i").Replace("$2$", "$i$"));
			Assert.AreEqual("System.Int64", lr.Type.ReflectionName);
		}
		
		[Test]
		public void NamespacePreferenceTest()
		{
			// Classes in the current namespace are preferred over classes from
			// imported namespaces
			string program = @"using System;
namespace Testnamespace {
class A {
	$Activator$ a;
}

class Activator {
	
}
}
";
			var result = Resolve<TypeResolveResult>(program);
			Assert.AreEqual("Testnamespace.Activator", result.Type.FullName);
		}
		
		[Test]
		public void ParentNamespaceTypeLookup()
		{
			string program = @"using System;
namespace Root {
  class Alpha {}
}
namespace Root.Child {
  class Beta {
    $Alpha$ a;
  }
}
";
			var result = Resolve<TypeResolveResult>(program);
			Assert.AreEqual("Root.Alpha", result.Type.FullName);
		}
		
		[Test, Ignore("type references not implemented")]
		public void ImportAliasTest()
		{
			string program = @"using COL = System.Collections;
class TestClass {
	COL.ArrayList ff;
}
";
			TypeResolveResult type = Resolve<TypeResolveResult>(program, "COL.ArrayList");
			Assert.IsNotNull(type, "COL.ArrayList should resolve to a type");
			Assert.AreEqual("System.Collections.ArrayList", type.Type.FullName, "TypeResolveResult");
			
			MemberResolveResult member = Resolve<MemberResolveResult>(program, "ff");
			Assert.AreEqual("System.Collections.ArrayList", member.Type.FullName, "the full type should be resolved");
		}
		
		[Test, Ignore("Parser position bug")]
		public void ImportAliasNamespaceResolveTest()
		{
			NamespaceResolveResult ns;
			string program = "using COL = System.Collections;\r\nclass A {\r\n$.ArrayList a;\r\n}\r\n";
			ns = Resolve<NamespaceResolveResult>(program.Replace("$", "$COL$"));
			Assert.AreEqual("System.Collections", ns.NamespaceName, "COL");
			ns = Resolve<NamespaceResolveResult>(program.Replace("$", "$COL.Generic$"));
			Assert.AreEqual("System.Collections.Generic", ns.NamespaceName, "COL.Generic");
		}
		
		[Test, Ignore("Cannot resolve type references")]
		public void ImportAliasClassResolveTest()
		{
			string program = @"using COL = System.Collections.ArrayList;
class TestClass {
	void Test() {
		COL a = new COL();
		
	}
}
";
			TypeResolveResult trr = Resolve<TypeResolveResult>(program.Replace("COL a", "$COL$ a"));
			Assert.AreEqual("System.Collections.ArrayList", trr.Type.FullName, "COL");
			ResolveResult rr = Resolve<ResolveResult>(program.Replace("new COL()", "$new COL()$"));
			Assert.AreEqual("System.Collections.ArrayList", rr.Type.FullName, "a");
		}
		
		[Test, Ignore("Parser position bug")]
		public void ResolveNamespaceSD_863()
		{
			string program = @"using System;
namespace A.C { class D {} }
namespace A.B.C { class D {} }
namespace A.B {
	class TestClass {
		void Test() {
			C.D x;
		}
	}
}
";
			NamespaceResolveResult nrr = Resolve<NamespaceResolveResult>(program.Replace("C.D", "$C$.D"));
			Assert.AreEqual("A.B.C", nrr.NamespaceName, "nrr.Name");
			TypeResolveResult trr = Resolve<TypeResolveResult>(program.Replace("C.D", "$C.D$"));
			Assert.AreEqual("A.B.C.D", trr.Type.FullName);
		}
		
		[Test, Ignore("Broken due to parser returning incorrect positions")]
		public void ResolveTypeSD_863()
		{
			string program = @"using System;
namespace A { class C {} }
namespace A.B {
	class C {}
	class TestClass {
		void Test() {
			$C$ a;
		}
	}
}
";
			TypeResolveResult trr = Resolve<TypeResolveResult>(program);
			Assert.AreEqual("A.B.C", trr.Type.FullName);
		}
		
		[Test, Ignore("parser is broken and produces IdentifierExpression instead of PrimitiveType")]
		public void ShortMaxValueTest()
		{
			string program = @"using System;
class TestClass {
	object a = $short.MaxValue$;
}
";
			MemberResolveResult rr = Resolve<MemberResolveResult>(program);
			Assert.AreEqual("System.Int16", rr.Type.FullName);
			Assert.AreEqual("System.Int16.MaxValue", rr.Member.FullName);
			Assert.AreEqual(short.MaxValue, rr.ConstantValue);
		}
		
		[Test, Ignore("Parser produces incorrect positions for :: operator")]
		public void ClassWithSameNameAsNamespace()
		{
			string program = @"using System; namespace XX {
	class Test {
		static void X() {
			a = $;
		}
	}
	class XX {
		public static void Test() {}
	} }";
			TypeResolveResult trr = Resolve<TypeResolveResult>(program.Replace("$", "$XX$"));
			Assert.AreEqual("XX.XX", trr.Type.FullName);
			
			NamespaceResolveResult nrr = Resolve<NamespaceResolveResult>(program.Replace("$", "$global::XX$.T"));
			Assert.AreEqual("XX", nrr.NamespaceName);
			
			trr = Resolve<TypeResolveResult>(program.Replace("$", "$global::XX.XX$"));
			Assert.AreEqual("XX.XX", trr.Type.FullName);
			
			MemberResolveResult mrr = Resolve<MemberResolveResult>(program.Replace("$", "$XX.Test()$"));
			Assert.AreEqual("XX.XX.Test", mrr.Member.FullName);
		}
		
		[Test, Ignore("Parser position bug")]
		public void ClassNameLookup1()
		{
			string program = @"namespace MainNamespace {
	using Test.Subnamespace;
	class Program {
		static void M($Test.TheClass$ c) {}
	}
}

namespace Test { public class TheClass { } }
namespace Test.Subnamespace {
	public class Test { public class TheClass { } }
}
";
			TypeResolveResult trr = Resolve<TypeResolveResult>(program);
			Assert.AreEqual("Test.Subnamespace.Test.TheClass", trr.Type.FullName);
		}
		
		[Test, Ignore("Parser position bug")]
		public void ClassNameLookup2()
		{
			string program = @"using Test.Subnamespace;
namespace MainNamespace {
	class Program {
		static void M($Test.TheClass$ c) {}
	}
}

namespace Test { public class TheClass { } }
namespace Test.Subnamespace {
	public class Test { public class TheClass { } }
}
";
			TypeResolveResult trr = Resolve<TypeResolveResult>(program);
			Assert.AreEqual("Test.TheClass", trr.Type.FullName);
		}
		
		[Test]
		public void ClassNameLookup3()
		{
			string program = @"namespace MainNamespace {
	using Test.Subnamespace;
	class Program {
		static void M($Test$ c) {}
	}
}

namespace Test { public class TheClass { } }
namespace Test.Subnamespace {
	public class Test { public class TheClass { } }
}
";
			TypeResolveResult trr = Resolve<TypeResolveResult>(program);
			Assert.AreEqual("Test.Subnamespace.Test", trr.Type.FullName);
		}
		
		[Test]
		public void ClassNameLookup4()
		{
			string program = @"using Test.Subnamespace;
namespace MainNamespace {
	class Program {
		static void M($Test$ c) {}
	}
}

namespace Test { public class TheClass { } }
namespace Test.Subnamespace {
	public class Test { public class TheClass { } }
}
";
			NamespaceResolveResult nrr = Resolve<NamespaceResolveResult>(program);
			Assert.AreEqual("Test", nrr.NamespaceName);
		}
		
		[Test]
		public void ClassNameLookup5()
		{
			string program = @"namespace MainNamespace {
	using A;
	
	class M {
		void X($Test$ a) {}
	}
	namespace Test { class B {} }
}

namespace A {
	class Test {}
}";
			NamespaceResolveResult nrr = Resolve<NamespaceResolveResult>(program);
			Assert.AreEqual("MainNamespace.Test", nrr.NamespaceName);
		}
		
		[Test, Ignore("Fails because parser does not support base type references")]
		public void InvocableRule()
		{
			string program = @"using System;
	class DerivedClass : BaseClass {
		static void X() {
			a = $;
		}
		private static new int Test;
	}
	class BaseClass {
		public static string Test() {}
	}";
			MemberResolveResult mrr = Resolve<MemberResolveResult>(program.Replace("$", "$BaseClass.Test()$"));
			Assert.AreEqual("BaseClass.Test", mrr.Member.FullName);
			
			mrr = Resolve<MemberResolveResult>(program.Replace("$", "$Test$"));
			Assert.AreEqual("DerivedClass.Test", mrr.Member.FullName);
			
			mrr = Resolve<MemberResolveResult>(program.Replace("$", "$DerivedClass.Test$"));
			Assert.AreEqual("DerivedClass.Test", mrr.Member.FullName);
			
			// returns BaseClass.Test because DerivedClass.Test is not invocable
			mrr = Resolve<MemberResolveResult>(program.Replace("$", "$DerivedClass.Test()$"));
			Assert.AreEqual("BaseClass.Test", mrr.Member.FullName);
		}
		
		[Test, Ignore("Fails because parser does not support base type references")]
		public void InvocableRule2()
		{
			string program = @"using System;
	class DerivedClass : BaseClass {
		static void X() {
			a = $;
		}
		private static new int Test;
	}
	delegate string SomeDelegate();
	class BaseClass {
		public static SomeDelegate Test;
	}";
			MemberResolveResult mrr = Resolve<MemberResolveResult>(program.Replace("$", "$BaseClass.Test$()"));
			Assert.AreEqual("BaseClass.Test", mrr.Member.FullName);
			
			mrr = Resolve<MemberResolveResult>(program.Replace("$", "$Test$"));
			Assert.AreEqual("DerivedClass.Test", mrr.Member.FullName);
			
			mrr = Resolve<MemberResolveResult>(program.Replace("$", "$DerivedClass.Test$"));
			Assert.AreEqual("DerivedClass.Test", mrr.Member.FullName);
			
			// returns BaseClass.Test because DerivedClass.Test is not invocable
			mrr = Resolve<MemberResolveResult>(program.Replace("$", "$DerivedClass.Test$()"));
			Assert.AreEqual("BaseClass.Test", mrr.Member.FullName);
		}
		
		[Test, Ignore("Fails because parser does not support base type references")]
		public void AccessibleRule()
		{
			string program = @"using System;
	class BaseClass {
		static void X() {
			a = $DerivedClass.Test$;
		}
		public static int Test;
	}
	class DerivedClass : BaseClass {
		private static new int Test;
	}
	";
			// returns BaseClass.Test because DerivedClass.Test is not accessible
			MemberResolveResult mrr = Resolve<MemberResolveResult>(program);
			Assert.AreEqual("BaseClass.Test", mrr.Member.FullName);
		}
		
		[Test]
		public void FieldHidingProperty()
		{
			string program = @"using System;
	class DerivedClass : BaseClass {
		static void X() {
			a = $Test$;
		}
		public static new int Test;
	}
	class BaseClass {
		public static int Test { get { return 0; } }
	}
	";
			MemberResolveResult mrr = Resolve<MemberResolveResult>(program);
			Assert.AreEqual("DerivedClass.Test", mrr.Member.FullName);
		}
		
		[Test]
		public void PropertyHidingField()
		{
			string program = @"using System;
	class DerivedClass : BaseClass {
		static void X() {
			a = $Test$;
		}
		public static new int Test { get { return 0; } }
	}
	class BaseClass {
		public static int Test;
	}
	";
			MemberResolveResult mrr = Resolve<MemberResolveResult>(program);
			Assert.AreEqual("DerivedClass.Test", mrr.Member.FullName);
		}
		
		[Test, Ignore("Parser doesn't support inheritance")]
		public void SD_1487()
		{
			string program = @"using System;
class C2 : C1 {
	public static void M() {
		a = $;
	}
}
class C1 {
	protected static int Field;
}";
			MemberResolveResult mrr;
			mrr = Resolve<MemberResolveResult>(program.Replace("$", "$Field$"));
			Assert.AreEqual("C1.Field", mrr.Member.FullName);
			
			mrr = Resolve<MemberResolveResult>(program.Replace("$", "$C1.Field$"));
			Assert.AreEqual("C1.Field", mrr.Member.FullName);
			
			mrr = Resolve<MemberResolveResult>(program.Replace("$", "$C2.Field$"));
			Assert.AreEqual("C1.Field", mrr.Member.FullName);
		}
		
		[Test]
		public void NullableValue()
		{
			string program = @"using System;
class Test {
	public static void M(int? a) {
		$a.Value$.ToString();
	}
}";
			MemberResolveResult rr = Resolve<MemberResolveResult>(program);
			Assert.AreEqual("System.Nullable.Value", rr.Member.FullName);
			Assert.AreEqual("System.Int32", rr.Member.ReturnType.Resolve(context).FullName);
		}
		
		[Test, Ignore("Parser doesn't support inheritance")]
		public void MethodHidesEvent()
		{
			// see SD-1542
			string program = @"using System;
class Test : Form {
	public Test() {
		a = $base.KeyDown$;
	}
	void KeyDown(object sender, EventArgs e) {}
}
class Form {
	public event EventHandler KeyDown;
}";
			var mrr = Resolve<MemberResolveResult>(program);
			Assert.AreEqual("Form.KeyDown", mrr.Member.FullName);
			
			var mgrr = Resolve<MethodGroupResolveResult>(program.Replace("base", "this"));
			Assert.AreEqual("Test.KeyDown", mgrr.Methods.Single().FullName);
		}
		
		[Test, Ignore("partial classes not yet supported")]
		public void ProtectedMemberVisibleWhenBaseTypeReferenceIsInOtherPart()
		{
			string program = @"using System;
partial class A {
	void M1() {
		$x$ = 0;
	}
}
partial class A : B { }
class B
{
	protected int x;
}";
			var mrr = Resolve<MemberResolveResult>(program);
			Assert.AreEqual("B.x", mrr.Member.FullName);
		}
	}
}
