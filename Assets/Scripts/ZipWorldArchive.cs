using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using UnityEngine;

namespace Puzzled
{
    public class ZipWorldArchive : IWorldArchive
    {
        public class ZipWorldArchiveEntry : IWorldArchiveEntry
        {
            private ZipArchiveEntry entry;

            public ZipWorldArchiveEntry(ZipArchiveEntry entry)
            {
                this.entry = entry;
            }

            public string name => entry.Name;
            public string path => entry.FullName;

            public Stream Open()
            {
                return entry.Open();
            }

            public void Delete()
            {
                entry.Delete();
            }
        }

        public ZipWorldArchive(Stream stream)
        {
            zipArchive = new ZipArchive(stream);
        }

        private ZipArchive zipArchive;

        public int entryCount => zipArchive.Entries.Count;
        public IEnumerable<IWorldArchiveEntry> entries 
        {           
            get
            {
                foreach (ZipArchiveEntry entry in zipArchive.Entries)
                    yield return new ZipWorldArchiveEntry(entry);
            }
        }

        public IWorldArchiveEntry CreateEntry(string name)
        {
            return new ZipWorldArchiveEntry(zipArchive.CreateEntry(name));
        }

        public void Dispose()
        {
            zipArchive.Dispose();
        }
    }

}