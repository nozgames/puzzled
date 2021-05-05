using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.IO.Compression;
using UnityEngine;
using System;

namespace Puzzled
{
    public class ZipWorldArchive : IWorldArchive, IDisposable
    {
        public class ZipWorldArchiveEntry : IWorldArchiveEntry
        {
            private ZipArchiveEntry entry;

            public ZipWorldArchiveEntry(ZipArchiveEntry entry)
            {
                this.entry = entry;
            }

            public string name => entry.Name;

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
            zipArchive = new ZipArchive(stream, ZipArchiveMode.Update);
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

        public bool Contains(string name) =>
            zipArchive.Entries.Any(e => string.Compare(e.Name, name, true) == 0);

        public void Dispose()
        {
            zipArchive.Dispose();
        }
    }

}