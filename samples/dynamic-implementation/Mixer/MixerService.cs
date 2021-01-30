// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using Contracts;

namespace Mixer
{
    public class MixerService : IMixerService
    {
        protected IFruitService fruit;
        public MixerService(IFruitService fruit)
        {
            this.fruit = fruit;
        }

        public string MixIt()
        {
            return string.Join(",", fruit.GetFruits().Select(x => x.Name).ToArray());
        }
    }
}
