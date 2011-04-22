﻿// Copyright (c) AlphaSierraPapa for the SharpDevelop Team (for details please see \doc\copyright.txt)
// This code is distributed under MIT X11 license (for details please see \doc\license.txt)

using System;
using System.Linq;
using NUnit.Framework;

namespace ICSharpCode.NRefactory.CSharp.Parser.Expression
{
	[TestFixture]
	public class InvocationExpressionTests
	{
		[Test]
		public void SimpleInvocationExpressionTest()
		{
			var ie = ParseUtilCSharp.ParseExpression<InvocationExpression>("myMethod()");
			Assert.AreEqual(0, ie.Arguments.Count());
			Assert.IsTrue(ie.Target is IdentifierExpression);
			Assert.AreEqual("myMethod", ((IdentifierExpression)ie.Target).Identifier);
		}
		
		[Test]
		public void GenericInvocationExpressionTest()
		{
			ParseUtilCSharp.AssertExpression(
				"myMethod<char>('a')",
				new InvocationExpression {
					Target = new IdentifierExpression {
						Identifier = "myMethod",
						TypeArguments = { new PrimitiveType("char") }
					},
					Arguments = { new PrimitiveExpression('a') }
				}
			);
		}
		
		[Test]
		public void GenericInvocation2ExpressionTest()
		{
			ParseUtilCSharp.AssertExpression(
				"myMethod<T,bool>()",
				new InvocationExpression {
					Target = new IdentifierExpression {
						Identifier = "myMethod",
						TypeArguments = {
							new SimpleType("T"),
							new PrimitiveType("bool")
						}
					}
				}
			);
		}
		
		[Test]
		public void AmbiguousGrammarGenericMethodCall()
		{
			ParseUtilCSharp.AssertExpression(
				"F(G<A,B>(7))",
				new InvocationExpression {
					Target = new IdentifierExpression("F"),
					Arguments = {
						new InvocationExpression {
							Target = new IdentifierExpression {
								Identifier = "G",
								TypeArguments = { new SimpleType("A"), new SimpleType("B") }
							},
							Arguments = { new PrimitiveExpression(7) }
						}}});
		}
		
		[Test, Ignore("Mono Parser Bug???")]
		public void AmbiguousGrammarNotAGenericMethodCall()
		{
			ParseUtilCSharp.AssertExpression(
				"F<A>+y",
				new BinaryOperatorExpression {
					Left = new BinaryOperatorExpression {
						Left = new IdentifierExpression("F"),
						Operator = BinaryOperatorType.LessThan,
						Right = new IdentifierExpression("A")
					},
					Operator = BinaryOperatorType.GreaterThan,
					Right = new UnaryOperatorExpression {
						Operator = UnaryOperatorType.Plus,
						Expression = new IdentifierExpression("y")
					}});
		}
		
		[Test]
		public void InvalidNestedInvocationExpressionTest()
		{
			// this test was written because this bug caused the AbstractASTVisitor to crash
			
			InvocationExpression expr = ParseUtilCSharp.ParseExpression<InvocationExpression>("WriteLine(myMethod(,))", true);
			Assert.IsTrue(expr.Target is IdentifierExpression);
			Assert.AreEqual("WriteLine", ((IdentifierExpression)expr.Target).Identifier);
			
			Assert.AreEqual(1, expr.Arguments.Count); // here a second null parameter was added incorrectly
			
			Assert.IsTrue(expr.Arguments.Single() is InvocationExpression);
		}
		
		[Test, Ignore("Positions not yet accurate when parsing expression only (because class/method is added around it)")]
		public void NestedInvocationPositions()
		{
			InvocationExpression expr = ParseUtilCSharp.ParseExpression<InvocationExpression>("a.B().C(args)");
			Assert.AreEqual(new AstLocation(1, 8), expr.StartLocation);
			Assert.AreEqual(new AstLocation(1, 14), expr.EndLocation);
			MemberReferenceExpression mre = (MemberReferenceExpression)expr.Target;
			Assert.AreEqual(new AstLocation(1, 6), mre.StartLocation);
			Assert.AreEqual(new AstLocation(1, 8), mre.EndLocation);
			
			Assert.AreEqual(new AstLocation(1, 4), mre.Target.StartLocation);
			Assert.AreEqual(new AstLocation(1, 6), mre.Target.EndLocation);
		}
		
		[Test]
		public void InvocationOnGenericType()
		{
			ParseUtilCSharp.AssertExpression(
				"A<T>.Foo()",
				new IdentifierExpression {
					Identifier = "A",
					TypeArguments = { new SimpleType("T") }
				}.Invoke("Foo")
			);
		}
		
		[Test]
		public void InvocationOnInnerClassInGenericType()
		{
			ParseUtilCSharp.AssertExpression(
				"A<T>.B.Foo()",
				new IdentifierExpression {
					Identifier = "A",
					TypeArguments = { new SimpleType("T") }
				}.Member("B").Invoke("Foo")
			);
		}
		
		[Test]
		public void InvocationOnGenericInnerClassInGenericType()
		{
			ParseUtilCSharp.AssertExpression(
				"A<T>.B.C<U>.Foo()",
				new MemberReferenceExpression {
					Target = new IdentifierExpression {
						Identifier = "A",
						TypeArguments = { new SimpleType("T") }
					}.Member("B"),
					MemberName = "C",
					TypeArguments = { new SimpleType("U") }
				}.Invoke("Foo"));
		}
		
		[Test]
		public void InvocationWithNamedArgument()
		{
			ParseUtilCSharp.AssertExpression(
				"a(arg: ref v)",
				new InvocationExpression {
					Target = new IdentifierExpression("a"),
					Arguments = {
						new NamedArgumentExpression {
							Identifier = "arg",
							Expression = new DirectionExpression {
								FieldDirection = FieldDirection.Ref,
								Expression = new IdentifierExpression("v")
							}}}});
		}
	}
}
