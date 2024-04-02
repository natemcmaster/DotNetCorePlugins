// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace McMaster.NETCore.Plugins
{
    internal class RuntimeConfig
    {
        public RuntimeOptions? RuntimeOptions { get; set; }

        [Obsolete("This property is obsolete and will be removed in a future version. Use 'RuntimeOptions' instead.", false)]
        public RuntimeOptions? runtimeOptions { get => RuntimeOptions; set => RuntimeOptions = value; }
    }
}
