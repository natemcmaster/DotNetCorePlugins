using Contracts;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace Mixer
{
    public class MixerService:IMixerService
    {

        protected IFruitService fruit;
        public MixerService(IFruitService fruit)
        {
            this.fruit = fruit;
        }

        public string MixIt()
        {
            return string.Join(",", this.fruit.GetFruits().Select(x => x.Name).ToArray());
        }
    }
}
