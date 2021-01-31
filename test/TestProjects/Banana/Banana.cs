// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Test.Referenced.Library;

namespace Test
{
    internal class Banana : IFruit
    {
        public string GetFlavor() => nameof(Banana);
    }
}
