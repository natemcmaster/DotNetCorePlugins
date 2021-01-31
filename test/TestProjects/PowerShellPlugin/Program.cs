// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Management.Automation;

namespace PowerShellPlugin
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine(GetGreeting());
        }

        public static string GetGreeting()
        {
            using var ps = PowerShell.Create();
            var type = typeof(AliasAttribute);
            // Console.WriteLine(type.Assembly.Location);
            var results = ps.AddScript("Write-Output hello").Invoke();
            return results[0].ToString();
        }
    }
}
