// Copyright (c) AlphaSierraPapa for the SharpDevelop Team (for details please see \doc\copyright.txt)
// This code is distributed under MIT X11 license (for details please see \doc\license.txt)

using System;

//[module: CLSCompliantAttribute(false)]
namespace NamedInitializerPropertyEnum
{
    [AttributeUsage(AttributeTargets.All)]
    public class MyAttributeAttribute : Attribute
    {
        public AttributeTargets Prop
        {
            get
            {
                return AttributeTargets.All;
            }
            set
            {
            }
        }
    }
    [MyAttribute(Prop = (AttributeTargets.Class | AttributeTargets.Method))]
    public class MyClass
    {
    }
}
