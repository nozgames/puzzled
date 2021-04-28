using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Puzzled
{
    public interface IWorldArchive
    {
        public int entryCount { get; }
        public IEnumerable<IWorldArchiveEntry> entries { get; }
        public IWorldArchiveEntry CreateEntry(string name);
        public void Dispose();
    }

    public interface IWorldArchiveEntry
    {
        public string name { get; }
        public string path { get; }

        public Stream Open();
        public void Delete();
    }
}