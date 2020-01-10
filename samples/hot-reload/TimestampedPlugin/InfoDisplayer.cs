using System;
using System.Linq;
using System.Reflection;
using Microsoft.Data.Sqlite;

namespace TimestampedPlugin
{
    public class InfoDisplayer
    {
        public static void Print()
        {
            // Use something from Microsoft.Data.Sqlite to trigger loading of native dependency
            var connectionString = new SqliteConnectionStringBuilder
            {
                DataSource = "HELLO"
            };

            var compileTimestamp = typeof(InfoDisplayer)
                .Assembly
                .GetCustomAttributes<AssemblyMetadataAttribute>()
                .First(a => a.Key == "CompileTimestamp")
                .Value;
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("TimestampedPlugin: ");
            Console.ResetColor();
            Console.WriteLine($"this plugin was compiled at {compileTimestamp}. {connectionString.DataSource}!");
        }
    }
}
