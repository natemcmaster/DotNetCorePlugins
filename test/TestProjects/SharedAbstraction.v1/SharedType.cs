using System;
using Test.Transitive;

namespace Test.Shared.Abstraction
{
    public class SharedType
    {
        public Type GetTransitive() => typeof(TransitiveSharedType);
    }
}
