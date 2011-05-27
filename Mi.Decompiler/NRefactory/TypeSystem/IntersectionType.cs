// Copyright (c) AlphaSierraPapa for the SharpDevelop Team (for details please see \doc\copyright.txt)
// This code is distributed under MIT X11 license (for details please see \doc\license.txt)

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Mi.NRefactory.TypeSystem.Implementation;

namespace Mi.NRefactory.TypeSystem
{
	/// <summary>
	/// Represents the intersection of several types.
	/// </summary>
	public class IntersectionType : AbstractType
	{
		readonly ReadOnlyCollection<IType> types;
		
		public ReadOnlyCollection<IType> Types {
			get { return types; }
		}
		
		private IntersectionType(IType[] types)
		{
			Debug.Assert(types.Length >= 2);
			this.types = Array.AsReadOnly(types);
		}
		
		public static IType Create(IEnumerable<IType> types)
		{
			IType[] arr = types.Where(t => t != null).Distinct().ToArray();
			if (arr.Length == 0)
				return SharedTypes.UnknownType;
			else if (arr.Length == 1)
				return arr[0];
			else
				return new IntersectionType(arr);
		}
		
		public override string Name {
			get {
				StringBuilder b = new StringBuilder();
				foreach (var t in types) {
					if (b.Length > 0)
						b.Append(" & ");
					b.Append(t.Name);
				}
				return b.ToString();
			}
		}
		
		public override string ReflectionName {
			get {
				StringBuilder b = new StringBuilder();
				foreach (var t in types) {
					if (b.Length > 0)
						b.Append(" & ");
					b.Append(t.ReflectionName);
				}
				return b.ToString();
			}
		}
		
		public override Nullable<bool> IsReferenceType {
			get { return null; }
		}
		
		public override int GetHashCode()
		{
			int hashCode = 0;
			unchecked {
				foreach (var t in types) {
					hashCode *= 7137517;
					hashCode += t.GetHashCode();
				}
			}
			return hashCode;
		}
		
		public override bool Equals(IType other)
		{
			IntersectionType o = other as IntersectionType;
			if (o != null && types.Count == o.types.Count) {
				for (int i = 0; i < types.Count; i++) {
					if (!types[i].Equals(o.types[i]))
						return false;
				}
				return true;
			}
			return false;
		}
		
		public override IEnumerable<IType> GetBaseTypes(ITypeResolveContext context)
		{
			return types;
		}
		
		public override IEnumerable<Event> GetEvents(ITypeResolveContext context, Predicate<Event> filter)
		{
			filter = FilterNonStatic(filter);
			return types.SelectMany(t => t.GetEvents(context, filter));
		}
		
		public override IEnumerable<Method> GetMethods(ITypeResolveContext context, Predicate<Method> filter)
		{
			filter = FilterNonStatic(filter);
			return types.SelectMany(t => t.GetMethods(context, filter));
		}
		
		public override IEnumerable<Property> GetProperties(ITypeResolveContext context, Predicate<Property> filter)
		{
			filter = FilterNonStatic(filter);
			return types.SelectMany(t => t.GetProperties(context, filter));
		}
		
		static Predicate<T> FilterNonStatic<T>(Predicate<T> filter) where T : class, IMember
		{
			return member => !member.IsStatic && (filter == null || filter(member));
		}
	}
}
