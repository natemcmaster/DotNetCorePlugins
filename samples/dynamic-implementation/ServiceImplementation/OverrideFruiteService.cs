using System.Collections.Generic;
using Contracts;

namespace ServiceImplementation
{
    public class OverrideFruiteService : IFruitService
    {
        public List<Fruit> GetFruits()
        {
            return new List<Fruit>()
            {
                new Fruit { Name="Banana" }
            };
        }
    }
}
