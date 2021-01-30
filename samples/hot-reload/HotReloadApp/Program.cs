// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using McMaster.NETCore.Plugins;

namespace HostApp
{
    internal class Program
    {
        public static async Task Main(string[] args)
        {
            var pluginPath = args[0];
            var loader = PluginLoader.CreateFromAssemblyFile(pluginPath,
                config => config.EnableHotReload = true);

            loader.Reloaded += ShowPluginInfo;

            var cts = new CancellationTokenSource();
            Console.CancelKeyPress += (_, __) => cts.Cancel();

            // Show info on first load
            InvokePlugin(loader);

            await Task.Delay(-1, cts.Token);
        }

        private static void ShowPluginInfo(object sender, PluginReloadedEventArgs eventArgs)
        {
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.Write("HotReloadApp: ");
            Console.ResetColor();
            Console.WriteLine("plugin was reloaded");
            InvokePlugin(eventArgs.Loader);
        }

        private static void InvokePlugin(PluginLoader loader)
        {
            var assembly = loader.LoadDefaultAssembly();
            assembly
                .GetType("TimestampedPlugin.InfoDisplayer", throwOnError: true)
                !.GetMethod("Print")
                !.Invoke(null, null);
        }
    }
}
