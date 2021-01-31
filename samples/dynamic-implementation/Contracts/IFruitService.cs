// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace Contracts
{
    public interface IFruitService
    {
        List<Fruit> GetFruits();
    }
}
