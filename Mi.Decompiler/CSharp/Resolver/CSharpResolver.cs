// Copyright (c) 2010 AlphaSierraPapa for the SharpDevelop Team (for details please see \doc\copyright.txt)
// This code is distributed under MIT X11 license (for details please see \doc\license.txt)

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using Mi.NRefactory.TypeSystem;
using Mi.NRefactory.TypeSystem.Implementation;
using Mi.CSharp.Ast.Expressions;
using Mi.CSharp.Ast;

namespace Mi.CSharp.Resolver
{
	/// <summary>
	/// Contains the main resolver logic.
	/// </summary>
	public class CSharpResolver
	{
		static readonly ResolveResult ErrorResult = new ErrorResolveResult(SharedTypes.UnknownType);
		static readonly ResolveResult DynamicResult = new ResolveResult(SharedTypes.Dynamic);
		static readonly ResolveResult NullResult = new ResolveResult(SharedTypes.Null);
		
		readonly ITypeResolveContext context;
		internal readonly Action verifyProgress;

		#region Constructor
        public CSharpResolver(ITypeResolveContext context)
            : this(context, () => { })
        {
        }
		
		public CSharpResolver(ITypeResolveContext context, Action verifyProgress)
		{
			if (context == null)
				throw new ArgumentNullException("context");
			this.context = context;
			this.verifyProgress = verifyProgress;
		}
		#endregion
		
		#region Properties
		/// <summary>
		/// Gets the type resolve context used by the resolver.
		/// </summary>
		public ITypeResolveContext Context {
			get { return context; }
		}
		
		/// <summary>
		/// Gets/Sets whether the current context is <c>checked</c>.
		/// </summary>
		public bool CheckForOverflow { get; set; }
		
		/// <summary>
		/// Gets/Sets the current member definition that is used to look up identifiers as parameters
		/// or type parameters.
		/// </summary>
		/// <remarks>Don't forget to also set CurrentTypeDefinition when setting CurrentMember;
		/// setting one of the properties does not automatically set the other.</remarks>
		public IMember CurrentMember { get; set; }
		
		/// <summary>
		/// Gets/Sets the current type definition that is used to look up identifiers as simple members.
		/// </summary>
		public TypeDefinition CurrentTypeDefinition { get; set; }
		#endregion
	}
}
