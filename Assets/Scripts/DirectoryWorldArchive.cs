using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Puzzled
{
    public class DirectoryWorldArchive : IWorldArchive
    {
        public class DirectoryWorldArchiveEntry : IWorldArchiveEntry
        {
            public DirectoryWorldArchiveEntry(string path)
            {
                _path = path;
            }

            public string _path;

            public string name => Path.GetFileName(path);
            public string path => _path;

            public Stream Open()
            {
                return File.Open(path, FileMode.Open);
            }

            public void Delete()
            {
                File.Delete(path);
            }
        }

        private string[] files;

        public DirectoryWorldArchive(string path)
        {
            files = Directory.GetFiles(path);
        }

        public int entryCount => files.Length;
        public IEnumerable<IWorldArchiveEntry> entries
        {
            get
            {
                foreach (string entry in files)
                    yield return new DirectoryWorldArchiveEntry(entry);
            }
        }

        public IWorldArchiveEntry CreateEntry(string name)
        {
            File.Create(name);
            return new DirectoryWorldArchiveEntry(name);
        }

        public void Dispose()
        {
            // FIXME?
        }
    }
}