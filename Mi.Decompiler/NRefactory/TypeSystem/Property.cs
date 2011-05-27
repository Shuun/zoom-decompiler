// Copyright (c) 2010 AlphaSierraPapa for the SharpDevelop Team (for details please see \doc\copyright.txt)
// This code is distributed under MIT X11 license (for details please see \doc\license.txt)

using System;
using System.Collections.Generic;
using Mi.NRefactory.TypeSystem.Implementation;

namespace Mi.NRefactory.TypeSystem
{
	/// <summary>
	/// Default implementation of <see cref="Property"/>.
	/// </summary>
    public abstract class Property : AbstractMember, IParameterizedMember
    {

        public IList<Parameter> Parameters
        {
            get { throw new NotImplementedException(); }
        }
    }
}
