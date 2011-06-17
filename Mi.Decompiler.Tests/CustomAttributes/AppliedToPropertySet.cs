// Copyright (c) AlphaSierraPapa for the SharpDevelop Team (for details please see \doc\copyright.txt)
// This code is distributed under MIT X11 license (for details please see \doc\license.txt)

using System;

//[module: CLSCompliantAttribute(false)]
namespace AppliedToPropertySet
{
    [AttributeUsage(AttributeTargets.All)]
    public class MyAttributeAttribute : Attribute
    {
    }
    public class TestClass
    {
        public int Property
        {
            get
            {
                return 3;
            }
            [MyAttribute]
            set
            {
            }
        }
    }
}
