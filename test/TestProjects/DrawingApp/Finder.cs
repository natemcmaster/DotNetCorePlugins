// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Drawing.Printing;

public class Finder
{
    public static string FindDrawingAssembly()
    {
        return typeof(PrinterUnit).Assembly.Location;
    }
}

