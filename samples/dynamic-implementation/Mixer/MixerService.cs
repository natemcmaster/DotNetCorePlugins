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
