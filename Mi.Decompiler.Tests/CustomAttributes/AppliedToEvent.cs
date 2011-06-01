// Copyright (c) AlphaSierraPapa for the SharpDevelop Team (for details please see \doc\copyright.txt)
// This code is distributed under MIT X11 license (for details please see \doc\license.txt)

using System;

//[module: CLSCompliantAttribute(false)]
namespace AppliedToEvent
{
    [AttributeUsage(AttributeTargets.Event)]
    public class MyAttributeAttribute : Attribute
    {
    }
    public class TestClass
    {
        [MyAttribute]
        public event EventHandler MyEvent;
    }
}
