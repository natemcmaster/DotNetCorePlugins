using System;
using Test.Referenced.Library;

namespace Test
{
    internal class Banana : IFruit
    {
        public string GetFlavor() => nameof(Banana);
    }
}
