﻿// 
// DoWhileStatement.cs
//  
// Author:
//       Mike Krüger <mkrueger@novell.com>
// 
// Copyright (c) 2011 Novell, Inc (http://www.novell.com)
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
// THE SOFTWARE.using System;

using System;
using System.Collections.Generic;
using System.Linq;

namespace Mi.CSharp.Ast.Statements
{
    using Mi.NRefactory.PatternMatching;
    using Mi.CSharp.Ast.Expressions;

    /// <summary>
	/// "do EmbeddedStatement while(Condition);"
	/// </summary>
	public class DoWhileStatement : Statement
	{
		public static readonly Role<CSharpTokenNode> DoKeywordRole = new Role<CSharpTokenNode>("DoKeyword", CSharpTokenNode.Null);
		public static readonly Role<CSharpTokenNode> WhileKeywordRole = new Role<CSharpTokenNode>("WhileKeyword", CSharpTokenNode.Null);
		
		public CSharpTokenNode DoToken {
			get { return GetChildByRole (DoKeywordRole); }
		}
		
		public Statement EmbeddedStatement {
			get { return GetChildByRole (Roles.EmbeddedStatement); }
			set { SetChildByRole (Roles.EmbeddedStatement, value); }
		}
		
		public CSharpTokenNode WhileToken {
			get { return GetChildByRole (WhileKeywordRole); }
		}
		
		public CSharpTokenNode LParToken {
			get { return GetChildByRole (Roles.LPar); }
		}
		
		public Expression Condition {
			get { return GetChildByRole (Roles.Condition); }
			set { SetChildByRole (Roles.Condition, value); }
		}
		
		public CSharpTokenNode RParToken {
			get { return GetChildByRole (Roles.RPar); }
		}
		
		public CSharpTokenNode SemicolonToken {
			get { return GetChildByRole (Roles.Semicolon); }
		}
		
		public override S AcceptVisitor<T, S> (IAstVisitor<T, S> visitor, T data)
		{
			return visitor.VisitDoWhileStatement (this, data);
		}
		
		protected internal override bool DoMatch(AstNode other, Match match)
		{
			DoWhileStatement o = other as DoWhileStatement;
			return o != null && this.EmbeddedStatement.DoMatch(o.EmbeddedStatement, match) && this.Condition.DoMatch(o.Condition, match);
		}
	}
}
