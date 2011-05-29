﻿// Copyright (c) AlphaSierraPapa for the SharpDevelop Team (for details please see \doc\copyright.txt)
// This code is distributed under MIT X11 license (for details please see \doc\license.txt)

using System;
using System.Collections.Generic;
using System.Linq;
using Mi.NRefactory.TypeSystem.Implementation;

namespace Mi.NRefactory.TypeSystem
{
	/// <summary>
	/// Default implementation of <see cref="Accessor"/>.
	/// </summary>
	public sealed class Accessor : AbstractFreezable, ISupportsInterning
	{
		static readonly Accessor[] defaultAccessors = CreateDefaultAccessors();
		
		static Accessor[] CreateDefaultAccessors()
		{
			Accessor[] accessors = new Accessor[(int)Accessibility.ProtectedAndInternal + 1];
			for (int i = 0; i < accessors.Length; i++) {
				accessors[i] = new Accessor();
				accessors[i].accessibility = (Accessibility)i;
				accessors[i].Freeze();
			}
			return accessors;
		}
		
		/// <summary>
		/// Gets the default accessor with the specified accessibility (and without attributes or region).
		/// </summary>
		public static Accessor GetFromAccessibility(Accessibility accessibility)
		{
			int index = (int)accessibility;
			if (index >= 0 && index < defaultAccessors.Length) {
				return defaultAccessors[index];
			} else {
				Accessor a = new Accessor();
				a.accessibility = accessibility;
				a.Freeze();
				return a;
			}
		}
		
		Accessibility accessibility;
		DomRegion region;
		
		protected override void FreezeInternal()
		{
			base.FreezeInternal();
		}
		
		public Accessibility Accessibility {
			get { return accessibility; }
			set {
				CheckBeforeMutation();
				accessibility = value;
			}
		}
		
		public DomRegion Region {
			get { return region; }
			set {
				CheckBeforeMutation();
				region = value;
			}
		}
		
		void ISupportsInterning.PrepareForInterning(IInterningProvider provider)
		{
		}
		
		int ISupportsInterning.GetHashCodeForInterning()
		{
			return region.GetHashCode() ^ (int)accessibility;
		}
		
		bool ISupportsInterning.EqualsForInterning(ISupportsInterning other)
		{
			Accessor a = other as Accessor;
			return a != null && (accessibility == a.accessibility && region == a.region);
		}
	}
}