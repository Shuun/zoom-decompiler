// Copyright (c) AlphaSierraPapa for the SharpDevelop Team (for details please see \doc\copyright.txt)
// This code is distributed under MIT X11 license (for details please see \doc\license.txt)

using System;
using System.IO;
using NUnit.Framework;

namespace ICSharpCode.NRefactory.CSharp
{
	[TestFixture]
	public class InsertParenthesesVisitorTests
	{
		CSharpFormattingOptions policy;
		
		[SetUp]
		public void SetUp()
		{
			policy = new CSharpFormattingOptions();
		}
		
		string InsertReadable(Expression expr)
		{
			expr = expr.Clone();
			expr.AcceptVisitor(new InsertParenthesesVisitor { InsertParenthesesForReadability = true }, null);
			StringWriter w = new StringWriter();
			expr.AcceptVisitor(new OutputVisitor(w, policy), null);
			return w.ToString();
		}
		
		string InsertRequired(Expression expr)
		{
			expr = expr.Clone();
			expr.AcceptVisitor(new InsertParenthesesVisitor { InsertParenthesesForReadability = false }, null);
			StringWriter w = new StringWriter();
			expr.AcceptVisitor(new OutputVisitor(w, policy), null);
			return w.ToString();
		}
		
		[Test]
		public void EqualityInAssignment()
		{
			Expression expr = new AssignmentExpression(
				new IdentifierExpression("cond"),
				new BinaryOperatorExpression(
					new IdentifierExpression("a"),
					BinaryOperatorType.Equality,
					new IdentifierExpression("b")
				)
			);
			
			Assert.AreEqual("cond = a == b", InsertRequired(expr));
			Assert.AreEqual("cond = (a == b)", InsertReadable(expr));
		}
		
		[Test]
		public void TrickyCast1()
		{
			Expression expr = new UnaryOperatorExpression(
				UnaryOperatorType.Minus, new IdentifierExpression("a")
			).CastTo(new PrimitiveType("int"));
			
			Assert.AreEqual("(int)-a", InsertRequired(expr));
			Assert.AreEqual("(int)(-a)", InsertReadable(expr));
		}
		
		[Test]
		public void TrickyCast2()
		{
			Expression expr = new UnaryOperatorExpression(
				UnaryOperatorType.Minus, new IdentifierExpression("a")
			).CastTo(new SimpleType("MyType"));
			
			Assert.AreEqual("(MyType)(-a)", InsertRequired(expr));
			Assert.AreEqual("(MyType)(-a)", InsertReadable(expr));
		}
		
		[Test]
		public void TrickyCast3()
		{
			Expression expr = new UnaryOperatorExpression(
				UnaryOperatorType.Not, new IdentifierExpression("a")
			).CastTo(new SimpleType("MyType"));
			
			Assert.AreEqual("(MyType)!a", InsertRequired(expr));
			Assert.AreEqual("(MyType)(!a)", InsertReadable(expr));
		}
		
		[Test]
		public void CastAndInvoke()
		{
			Expression expr = new IdentifierExpression("a")
				.CastTo(new PrimitiveType("string"))
				.Member("Length");
			
			Assert.AreEqual("((string)a).Length", InsertRequired(expr));
			Assert.AreEqual("((string)a).Length", InsertReadable(expr));
		}
		
		[Test]
		public void DoubleNegation()
		{
			Expression expr = new UnaryOperatorExpression(
				UnaryOperatorType.Minus,
				new UnaryOperatorExpression(UnaryOperatorType.Minus, new IdentifierExpression("a"))
			);
			
			Assert.AreEqual("- -a", InsertRequired(expr));
			Assert.AreEqual("-(-a)", InsertReadable(expr));
		}
		
		[Test]
		public void AdditionWithConditional()
		{
			Expression expr = new BinaryOperatorExpression {
				Left = new IdentifierExpression("a"),
				Operator = BinaryOperatorType.Add,
				Right = new ConditionalExpression {
					Condition = new BinaryOperatorExpression {
						Left = new IdentifierExpression("b"),
						Operator = BinaryOperatorType.Equality,
						Right = new PrimitiveExpression(null)
					},
					TrueExpression = new IdentifierExpression("c"),
					FalseExpression = new IdentifierExpression("d")
				}
			};
			
			Assert.AreEqual("a + (b == null ? c : d)", InsertRequired(expr));
			Assert.AreEqual("a + ((b == null) ? c : d)", InsertReadable(expr));
		}
		
		[Test]
		public void TypeTestInConditional()
		{
			Expression expr = new ConditionalExpression {
				Condition = new IdentifierExpression("a").IsType(
					new ComposedType {
						BaseType = new PrimitiveType("int"),
						HasNullableSpecifier = true
					}
				),
				TrueExpression = new IdentifierExpression("b"),
				FalseExpression = new IdentifierExpression("c")
			};
			
			Assert.AreEqual("a is int? ? b : c", InsertRequired(expr));
			Assert.AreEqual("(a is int?) ? b : c", InsertReadable(expr));
			
			policy.SpaceBeforeConditionalOperatorCondition = false;
			policy.SpaceAfterConditionalOperatorCondition = false;
			policy.SpaceBeforeConditionalOperatorSeparator = false;
			policy.SpaceAfterConditionalOperatorSeparator = false;
			
			Assert.AreEqual("a is int? ?b:c", InsertRequired(expr));
			Assert.AreEqual("(a is int?)?b:c", InsertReadable(expr));
		}
		
		[Test]
		public void MethodCallOnQueryExpression()
		{
			Expression expr = new QueryExpression {
				Clauses = {
					new QueryFromClause {
						Identifier = "a",
						Expression = new IdentifierExpression("b")
					},
					new QuerySelectClause {
						Expression = new IdentifierExpression("a").Invoke("c")
					}
				}
			}.Invoke("ToArray");
			
			Assert.AreEqual("(from a in b" + Environment.NewLine + "select a.c ()).ToArray ()", InsertRequired(expr));
			Assert.AreEqual("(from a in b" + Environment.NewLine + "select a.c ()).ToArray ()", InsertReadable(expr));
		}
		
		[Test]
		public void SumOfQueries()
		{
			QueryExpression query = new QueryExpression {
				Clauses = {
					new QueryFromClause {
						Identifier = "a",
						Expression = new IdentifierExpression("b")
					},
					new QuerySelectClause {
						Expression = new IdentifierExpression("a")
					}
				}
			};
			Expression expr = new BinaryOperatorExpression(
				query,
				BinaryOperatorType.Add,
				query.Clone()
			);
			
			Assert.AreEqual("(from a in b" + Environment.NewLine +
			                "select a) + from a in b" + Environment.NewLine +
			                "select a", InsertRequired(expr));
			Assert.AreEqual("(from a in b" + Environment.NewLine +
			                "select a) + (from a in b" + Environment.NewLine +
			                "select a)", InsertReadable(expr));
		}
		
		[Test]
		public void QueryInTypeTest()
		{
			Expression expr = new QueryExpression {
				Clauses = {
					new QueryFromClause {
						Identifier = "a",
						Expression = new IdentifierExpression("b")
					},
					new QuerySelectClause {
						Expression = new IdentifierExpression("a")
					}
				}
			}.IsType(new PrimitiveType("int"));
			
			Assert.AreEqual("(from a in b" + Environment.NewLine +
			                "select a) is int", InsertRequired(expr));
			Assert.AreEqual("(from a in b" + Environment.NewLine +
			                "select a) is int", InsertReadable(expr));
		}
		
		[Test]
		public void PrePost()
		{
			Expression expr = new UnaryOperatorExpression(
				UnaryOperatorType.Increment,
				new UnaryOperatorExpression(
					UnaryOperatorType.PostIncrement,
					new IdentifierExpression("a")
				)
			);
			
			Assert.AreEqual("++a++", InsertRequired(expr));
			Assert.AreEqual("++(a++)", InsertReadable(expr));
		}
		
		[Test]
		public void PostPre()
		{
			Expression expr = new UnaryOperatorExpression(
				UnaryOperatorType.PostIncrement,
				new UnaryOperatorExpression(
					UnaryOperatorType.Increment,
					new IdentifierExpression("a")
				)
			);
			
			Assert.AreEqual("(++a)++", InsertRequired(expr));
			Assert.AreEqual("(++a)++", InsertReadable(expr));
		}
		
		[Test]
		public void Logical1()
		{
			Expression expr = new BinaryOperatorExpression(
				new BinaryOperatorExpression(
					new IdentifierExpression("a"),
					BinaryOperatorType.ConditionalAnd,
					new IdentifierExpression("b")
				),
				BinaryOperatorType.ConditionalAnd,
				new IdentifierExpression("c")
			);
			
			Assert.AreEqual("a && b && c", InsertRequired(expr));
			Assert.AreEqual("a && b && c", InsertReadable(expr));
		}
		
		[Test]
		public void Logical2()
		{
			Expression expr = new BinaryOperatorExpression(
				new IdentifierExpression("a"),
				BinaryOperatorType.ConditionalAnd,
				new BinaryOperatorExpression(
					new IdentifierExpression("b"),
					BinaryOperatorType.ConditionalAnd,
					new IdentifierExpression("c")
				)
			);
			
			Assert.AreEqual("a && (b && c)", InsertRequired(expr));
			Assert.AreEqual("a && (b && c)", InsertReadable(expr));
		}
		
		[Test]
		public void Logical3()
		{
			Expression expr = new BinaryOperatorExpression(
				new IdentifierExpression("a"),
				BinaryOperatorType.ConditionalOr,
				new BinaryOperatorExpression(
					new IdentifierExpression("b"),
					BinaryOperatorType.ConditionalAnd,
					new IdentifierExpression("c")
				)
			);
			
			Assert.AreEqual("a || b && c", InsertRequired(expr));
			Assert.AreEqual("a || (b && c)", InsertReadable(expr));
		}
		
		[Test]
		public void Logical4()
		{
			Expression expr = new BinaryOperatorExpression(
				new IdentifierExpression("a"),
				BinaryOperatorType.ConditionalAnd,
				new BinaryOperatorExpression(
					new IdentifierExpression("b"),
					BinaryOperatorType.ConditionalOr,
					new IdentifierExpression("c")
				)
			);
			
			Assert.AreEqual("a && (b || c)", InsertRequired(expr));
			Assert.AreEqual("a && (b || c)", InsertReadable(expr));
		}
	}
}
