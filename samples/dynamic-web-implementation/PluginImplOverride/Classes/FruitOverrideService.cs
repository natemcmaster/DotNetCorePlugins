using PluginLib.Classes;
using System;
using System.Collections.Generic;
using System.Text;

namespace PluginImplOverride.Classes
{
    public class FruitOverrideService: IFruitService
    {
        public List<Fruit> GetFruits()
        {
            return new List<Fruit> { new Fruit { Name = "Mela" } };
        }
    }
}

