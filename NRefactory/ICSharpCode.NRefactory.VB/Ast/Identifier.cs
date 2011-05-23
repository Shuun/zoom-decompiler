﻿// Copyright (c) AlphaSierraPapa for the SharpDevelop Team (for details please see \doc\copyright.txt)
// This code is distributed under the GNU LGPL (for details please see \doc\license.txt)

using System;

namespace ICSharpCode.NRefactory.VB.Ast
{
	/// <summary>
	/// Represents an identifier in VB.
	/// </summary>
	public class Identifier : AstNode
	{
		public static readonly new Identifier Null = new NullIdentifier ();
		class NullIdentifier : Identifier
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
			
			protected internal override bool DoMatch(AstNode other, PatternMatching.Match match)
			{
				return other == null || other.IsNull;
			}
		}
		
		string name;
		
		public string Name {
			get { return name; }
			set {
				if (value == null)
					throw new ArgumentNullException("value");
				name = value;
			}
		}
		
		public TypeCode TypeCharacter { get; set; }
		
		AstLocation startLocation;
		public override AstLocation StartLocation {
			get {
				return startLocation;
			}
		}
		
		public override AstLocation EndLocation {
			get {
				return new AstLocation (StartLocation.Line, StartLocation.Column + Name.Length);
			}
		}
		
		private Identifier()
		{
			this.name = string.Empty;
		}
		
		public Identifier (string name, AstLocation location)
		{
			if (name == null)
				throw new ArgumentNullException("name");
			this.Name = name;
			this.startLocation = location;
		}
		
		protected internal override bool DoMatch(AstNode other, ICSharpCode.NRefactory.PatternMatching.Match match)
		{
			var node = other as Identifier;
			return node != null
				&& MatchString(node.name, name)
				&& node.TypeCharacter == TypeCharacter;
		}
		
		public override S AcceptVisitor<T, S>(IAstVisitor<T, S> visitor, T data)
		{
			return visitor.VisitIdentifier(this, data);
		}
		
		public override string ToString()
		{
			return string.Format("[Identifier Name={0}, StartLocation={1}, TypeCharacter{4}]",
			                     name, startLocation, TypeCharacter);
		}
	}
}
