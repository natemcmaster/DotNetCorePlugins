// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Contracts;

namespace Mixer
{
    public class StandardFruiteService : IFruitService
    {
        public List<Fruit> GetFruits()
        {
            return new List<Fruit>()
            {
                new Fruit { Name="Banana" },
                new Fruit { Name="Apple" },
                new Fruit { Name="Carrot" },
            };
        }
    }
}
