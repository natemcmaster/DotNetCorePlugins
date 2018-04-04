using System;

namespace Microsoft.Extensions.Plugins.Tests
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
