// Copyright (c) 2010 AlphaSierraPapa for the SharpDevelop Team (for details please see \doc\copyright.txt)
// This code is distributed under MIT X11 license (for details please see \doc\license.txt)

using System;
using System.Collections.Generic;
using System.Linq;

using Mi.NRefactory.Utils;
using Mi.NRefactory.TypeSystem.Implementation;

namespace Mi.NRefactory.TypeSystem
{
    /// <summary>
    /// Represents the variance of a type parameter.
    /// </summary>
    public enum VarianceModifier : byte
    {
        /// <summary>
        /// The type parameter is not variant.
        /// </summary>
        Invariant,
        /// <summary>
        /// The type parameter is covariant (used in output position).
        /// </summary>
        Covariant,
        /// <summary>
        /// The type parameter is contravariant (used in input position).
        /// </summary>
        Contravariant
    }
}
