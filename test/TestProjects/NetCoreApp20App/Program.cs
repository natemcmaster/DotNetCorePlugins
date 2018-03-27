using System;

namespace NetCoreApp20App
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine(GetGreeting());
        }

        public static string GetGreeting() => "Hello world!";
    }
}
