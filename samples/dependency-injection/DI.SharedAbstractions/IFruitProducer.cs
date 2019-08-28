using System.Collections.Generic;

namespace DependencyInjection
{
    public interface IFruitProducer
    {
        IEnumerable<Fruit> Produce();
    }
}
