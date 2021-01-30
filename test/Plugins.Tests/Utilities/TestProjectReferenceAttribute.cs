// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace McMaster.NETCore.Plugins.Tests
{
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public class TestProjectReferenceAttribute : Attribute
    {
        public TestProjectReferenceAttribute(string name, string path)
        {
            Name = name;
            Path = path;
        }

        public string Name { get; }
        public string Path { get; }
    }
}
