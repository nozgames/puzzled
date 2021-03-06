﻿using System;
using System.Collections.Generic;
using System.IO;

namespace Puzzled
{
    public interface IWorldArchive : IDisposable
    {
        public int entryCount { get; }
        public IEnumerable<IWorldArchiveEntry> entries { get; }
        public IWorldArchiveEntry CreateEntry(string name);
        public bool Contains(string name);
        public bool isDisposed { get; }
    }

    public interface IWorldArchiveEntry
    {
        public string name { get; }

        public Stream OpenRead();
        public Stream OpenWrite();
        public void Delete();
    }
}