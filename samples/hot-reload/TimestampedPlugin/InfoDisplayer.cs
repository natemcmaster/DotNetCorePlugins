using System;
using System.Linq;
using System.Reflection;

namespace TimestampedPlugin
{
    public class InfoDisplayer
    {
        public static void Print()
        {
            var compileTimestamp = typeof(InfoDisplayer)
                .Assembly
                .GetCustomAttributes<AssemblyMetadataAttribute>()
                .First(a => a.Key == "CompileTimestamp")
                .Value;
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("TimestampedPlugin: ");
            Console.ResetColor();
            Console.WriteLine($"this plugin was compiled at {compileTimestamp}");
        }
    }
}
