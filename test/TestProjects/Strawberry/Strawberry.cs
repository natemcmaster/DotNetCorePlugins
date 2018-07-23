using System;
using Test.Referenced.Library;

namespace Test
{
    internal class Strawberry : IFruit
    {
        public string GetFlavor() => nameof(Strawberry);
    }
}
