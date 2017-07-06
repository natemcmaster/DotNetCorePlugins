using System;
using System.Reflection;
using System.Runtime.Loader;
using Microsoft.Extensions.DependencyModel;
using Xunit;

namespace LoadContext.Tests
{
    public class DependencyLoadContextTests
    {
        [Fact]
        public void LoadFromContext()
        {
            var depContext = DependencyContext.Load(typeof(DependencyLoadContextTests).GetTypeInfo().Assembly);
            var loadContext = depContext.CreateLoadContext();
            Assert.NotNull(loadContext);
        }
    }
}
