// Copyright (c) AlphaSierraPapa for the SharpDevelop Team (for details please see \doc\copyright.txt)
// This code is distributed under MIT X11 license (for details please see \doc\license.txt)

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Mi.NRefactory.TypeSystem;
using Mi.NRefactory.TypeSystem.Implementation;
using Mi.CSharp.Ast;
using Mi.CSharp.Ast.Expressions;
using Mi.CSharp.Ast.Statements;

namespace Mi.CSharp.Resolver
{
	/// <summary>
	/// Traverses the DOM and resolves expressions.
	/// </summary>
	/// <remarks>
	/// The ResolveVisitor does two jobs at the same time: it tracks the resolve context (properties on CSharpResolver)
	/// and it resolves the expressions visited.
	/// To allow using the context tracking without having to resolve every expression in the file (e.g. when you want to resolve
	/// only a single node deep within the DOM), you can use the <see cref="IResolveVisitorNavigator"/> interface.
	/// The navigator allows you to switch the between scanning mode and resolving mode.
	/// In scanning mode, the context is tracked (local variables registered etc.), but nodes are not resolved.
	/// While scanning, the navigator will get asked about every node that the resolve visitor is about to enter.
	/// This allows the navigator whether to keep scanning, whether switch to resolving mode, or whether to completely skip the
	/// subtree rooted at that node.
	/// 
	/// In resolving mode, the context is tracked and nodes will be resolved.
	/// The resolve visitor may decide that it needs to resolve other nodes as well in order to resolve the current node.
	/// In this case, those nodes will be resolved automatically, without asking the navigator interface.
	/// For child nodes that are not essential to resolving, the resolve visitor will switch back to scanning mode (and thus will
	/// ask the navigator for further instructions).
	/// 
	/// Moreover, there is the <c>ResolveAll</c> mode - it works similar to resolving mode, but will not switch back to scanning mode.
	/// The whole subtree will be resolved without notifying the navigator.
	/// </remarks>
	public sealed class ResolveVisitor : DepthFirstAstVisitor<object, ResolveResult>
	{
		static readonly ResolveResult errorResult = new ErrorResolveResult(SharedTypes.UnknownType);
		CSharpResolver resolver;
		readonly Dictionary<AstNode, ResolveResult> cache = new Dictionary<AstNode, ResolveResult>();
		
		readonly IResolveVisitorNavigator navigator;
		ResolveVisitorNavigationMode mode = ResolveVisitorNavigationMode.Scan;

		#region Constructor
		/// <summary>
		/// Creates a new ResolveVisitor instance.
		/// </summary>
		/// <param name="resolver">
		/// The CSharpResolver, describing the initial resolve context.
		/// If you visit a whole CompilationUnit with the resolve visitor, you can simply pass
		/// <c>new CSharpResolver(typeResolveContext)</c> without setting up the context.
		/// If you only visit a subtree, you need to pass a CSharpResolver initialized to the context for that subtree.
		/// </param>
		/// <param name="parsedFile">
		/// Result of the <see cref="TypeSystemConvertVisitor"/> for the file being passed. This is used for setting up the context on the resolver.
		/// You may pass <c>null</c> if you are only visiting a part of a method body and have already set up the context in the <paramref name="resolver"/>.
		/// </param>
		/// <param name="navigator">
		/// The navigator, which controls where the resolve visitor will switch between scanning mode and resolving mode.
		/// If you pass <c>null</c>, then <c>ResolveAll</c> mode will be used.
		/// </param>
		public ResolveVisitor(CSharpResolver resolver, IResolveVisitorNavigator navigator = null)
		{
			if (resolver == null)
				throw new ArgumentNullException("resolver");
			this.resolver = resolver;
			this.navigator = navigator;
			if (navigator == null)
				mode = ResolveVisitorNavigationMode.ResolveAll;
		}
		#endregion
		
		/// <summary>
		/// Gets the TypeResolveContext used by this ResolveVisitor.
		/// </summary>
		public ITypeResolveContext TypeResolveContext {
			get { return resolver.Context; }
		}
		
		/// <summary>
		/// Gets the Action used by this ResolveVisitor.
		/// </summary>
		public Action Action {
			get { return resolver.verifyProgress; }
		}
	}
}
