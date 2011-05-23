﻿// 
// BlockStatement.cs
//
// Author:
//       Mike Krüger <mkrueger@novell.com>
// 
// Copyright (c) 2009 Novell, Inc (http://www.novell.com)
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Mi.CSharp
{
    using Mi.NRefactory.PatternMatching;
    
    /// <summary>
	/// { Statements }
	/// </summary>
	public class BlockStatement : Statement, IEnumerable<Statement>
	{
		public static readonly Role<Statement> StatementRole = new Role<Statement>("Statement", Statement.Null);
		
		#region Null
		public static readonly new BlockStatement Null = new NullBlockStatement ();
		sealed class NullBlockStatement : BlockStatement
		{
			public override bool IsNull {
				get {
					return true;
				}
			}
			
			public override S AcceptVisitor<T, S> (IAstVisitor<T, S> visitor, T data)
			{
				return default (S);
			}
			
			protected internal override bool DoMatch(AstNode other, Match match)
			{
				return other == null || other.IsNull;
			}
		}
		#endregion
		
		#region PatternPlaceholder
		public static implicit operator BlockStatement(Pattern pattern)
		{
			return pattern != null ? new PatternPlaceholder(pattern) : null;
		}
		
		sealed class PatternPlaceholder : BlockStatement, INode
		{
			readonly Pattern child;
			
			public PatternPlaceholder(Pattern child)
			{
				this.child = child;
			}
			
			public override NodeType NodeType {
				get { return NodeType.Pattern; }
			}
			
			public override S AcceptVisitor<T, S>(IAstVisitor<T, S> visitor, T data)
			{
				return visitor.VisitPatternPlaceholder(this, child, data);
			}
			
			protected internal override bool DoMatch(AstNode other, Match match)
			{
				return child.DoMatch(other, match);
			}
			
			bool INode.DoMatchCollection(Role role, INode pos, Match match, BacktrackingInfo backtrackingInfo)
			{
				return child.DoMatchCollection(role, pos, match, backtrackingInfo);
			}
		}
		#endregion
		
		public CSharpTokenNode LBraceToken {
			get { return GetChildByRole (Roles.LBrace); }
		}
		
		public AstNodeCollection<Statement> Statements {
			get { return GetChildrenByRole (StatementRole); }
		}
		
		public CSharpTokenNode RBraceToken {
			get { return GetChildByRole (Roles.RBrace); }
		}
		
		public override S AcceptVisitor<T, S> (IAstVisitor<T, S> visitor, T data)
		{
			return visitor.VisitBlockStatement (this, data);
		}
		
		protected internal override bool DoMatch(AstNode other, Match match)
		{
			BlockStatement o = other as BlockStatement;
			return o != null && !o.IsNull && this.Statements.DoMatch(o.Statements, match);
		}
		
		#region Builder methods
		public void Add(Statement statement)
		{
			AddChild(statement, StatementRole);
		}
		
		public void Add(Expression expression)
		{
			AddChild(new ExpressionStatement { Expression = expression }, StatementRole);
		}
		
		public void AddRange(IEnumerable<Statement> statements)
		{
			foreach (Statement st in statements)
				AddChild(st, StatementRole);
		}
		
		public void AddAssignment(Expression left, Expression right)
		{
			Add(new AssignmentExpression { Left = left, Operator = AssignmentOperatorType.Assign, Right = right });
		}
		
		public void AddReturnStatement(Expression expression)
		{
			Add(new ReturnStatement { Expression = expression });
		}
		#endregion
		
		IEnumerator<Statement> IEnumerable<Statement>.GetEnumerator()
		{
			return this.Statements.GetEnumerator();
		}
		
		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return this.Statements.GetEnumerator();
		}
	}
}
