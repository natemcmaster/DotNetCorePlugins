// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Test.Transitive;

namespace Test.Shared.Abstraction
{
    public class SharedType
    {
        public Type GetTransitive() => typeof(TransitiveSharedType);
    }
}
