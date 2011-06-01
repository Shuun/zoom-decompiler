// Copyright (c) AlphaSierraPapa for the SharpDevelop Team (for details please see \doc\copyright.txt)
// This code is distributed under MIT X11 license (for details please see \doc\license.txt)

using System;

//[module: CLSCompliantAttribute(false)]
namespace TargetPropertyIndexSetMultiParam
{
    [AttributeUsage(AttributeTargets.All)]
    public class MyAttributeAttribute : Attribute
    {
        public int Field;
    }
    public class MyClass
    {
        public string this[[MyAttribute(Field = 2)] int index1, [MyAttribute(Field = 3)] int index2]
        {
            get
            {
                return "";
            }
            [param: MyAttribute]
            set
            {
            }
        }
    }
}
