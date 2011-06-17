// Copyright (c) AlphaSierraPapa for the SharpDevelop Team (for details please see \doc\copyright.txt)
// This code is distributed under MIT X11 license (for details please see \doc\license.txt)

using System;

//[module: CLSCompliantAttribute(false)]
namespace AttributeWithTypeArgument
{
    [AttributeUsage(AttributeTargets.All)]
    public class MyTypeAttribute : Attribute
    {
        public MyTypeAttribute(Type t)
        {
        }
    }

    [MyType(typeof(Attribute))]
    public class SomeClass
    {
    }
}
