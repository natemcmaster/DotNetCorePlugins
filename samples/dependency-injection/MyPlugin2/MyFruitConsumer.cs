// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using DependencyInjection;

namespace MyPlugin2
{
    internal class MyFruitConsumer : IFruitConsumer
    {
        private readonly IEnumerable<IFruitProducer> _producers;

        public MyFruitConsumer(IEnumerable<IFruitProducer> producers)
        {
            _producers = producers;
        }

        public void Consume()
        {
            foreach (var producer in _producers)
            {
                foreach (var fruit in producer.Produce())
                {
                    Console.WriteLine($"Consumed {fruit.Name}");
                }
            }
        }
    }
}
