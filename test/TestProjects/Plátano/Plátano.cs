// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Plátano;
using Test.Referenced.Library;

namespace Test
{
    internal class Plátano : IFruit
    {
        public string GetFlavor() => Strings.Flavor;
    }
}
