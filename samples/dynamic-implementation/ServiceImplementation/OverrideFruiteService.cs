using Contracts;
using System;
using System.Collections.Generic;
using System.Text;

namespace ServiceImplementation
{
    public class OverrideFruiteService : IFruitService
    {
        public List<Fruit> GetFruits()
        {
            return new List<Fruit>()
            {
                new Fruit(){ Name="Banana"}
            };
        }
    }
}
