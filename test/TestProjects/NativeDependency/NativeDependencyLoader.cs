// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data.SQLite;
using System.IO;

namespace NativeDependency
{
    public static class NativeDependencyLoader
    {
        public static void Load()
        {
            using var tempFile = new TempFile("db.sqlite");
            using var dbConnection = new SQLiteConnection($"Data Source={tempFile.FilePath}");

            dbConnection.Open();
        }
    }

    public class TempFile : IDisposable
    {
        public TempFile(string fileName)
        {
            FilePath = Path.Combine(Path.GetTempPath(), fileName);
        }

        public string FilePath { get; }

        public void Dispose()
        {
            if (!File.Exists(FilePath))
            {
                return;
            }

            File.Delete(FilePath);
        }
    }
}
