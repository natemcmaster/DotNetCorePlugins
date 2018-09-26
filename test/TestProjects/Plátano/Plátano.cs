using System;
using Plátano;
using Test.Referenced.Library;

namespace Test
{
    internal class Plátano : IFruit
    {
        public string GetFlavor() => Strings.Flavor;
    }
}
