// Copyright (c) AlphaSierraPapa for the SharpDevelop Team (for details please see \doc\copyright.txt)
// This code is distributed under MIT X11 license (for details please see \doc\license.txt)

using System;

//[module: CLSCompliantAttribute(false)]
namespace AppliedToParameter
{
    [AttributeUsage(AttributeTargets.Parameter)]
    public class MyAttributeAttribute : Attribute
    {
    }
    public class MyClass
    {
        public void Method([MyAttribute] int val)
        {
        }
    }
}
