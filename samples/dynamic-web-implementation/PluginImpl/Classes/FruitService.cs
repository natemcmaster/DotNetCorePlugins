using PluginLib.Classes;
using System;
using System.Collections.Generic;
using System.Text;

namespace PluginImpl.Classes
{
    public class FruitService : IFruitService
    {
        public List<Fruit> GetFruits()
        {
            return new List<Fruit> { new Fruit { Name = "Banana" }, new Fruit { Name = "Pera" } };
        }
    }
}
