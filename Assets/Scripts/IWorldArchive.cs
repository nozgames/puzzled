using System.Collections.Generic;
using System.IO;

namespace Puzzled
{
    public interface IWorldArchive
    {
        public int entryCount { get; }
        public IEnumerable<IWorldArchiveEntry> entries { get; }
        public IWorldArchiveEntry CreateEntry(string name);
        public void Dispose();
        public bool Contains(string name);
    }

    public interface IWorldArchiveEntry
    {
        public string name { get; }

        public Stream Open();
        public void Delete();
    }
}