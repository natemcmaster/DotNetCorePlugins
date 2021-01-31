// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace NetCoreApp2App
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine(GetGreeting());
        }

        public static string GetGreeting() => "Hello world!";
    }
}
