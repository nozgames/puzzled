using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

            public string name => Path.GetFileName(_path);
            
            public Stream OpenRead()
            {
                return File.Open(_path, FileMode.Open);
            }

            public Stream OpenWrite()
            {
                return File.Open(_path, FileMode.Create);
            }

            public void Delete()
            {
                File.Delete(_path);
            }
        }

        private string _path;
        private List<string> _files;

        public DirectoryWorldArchive(string path)
        {
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            _path = path;
            _files = Directory.GetFiles(path).ToList();
        }

        public int entryCount => _files.Count;
        public IEnumerable<IWorldArchiveEntry> entries
        {
            get
            {
                foreach (string filename in _files)
                    yield return new DirectoryWorldArchiveEntry(filename);
            }
        }

        public bool isDisposed => false;

        public IWorldArchiveEntry CreateEntry(string name)
        {
            // Look for duplicates before we create a new one
            if (_files.Any(f => string.Compare(name, f, true) == 0))
                return null;

            var path = Path.Combine(_path, name);
            using var stream = File.Create(path);
            _files.Add(path);
            return new DirectoryWorldArchiveEntry(path);
        }

        public bool Contains(string name) =>
            _files.Any(f => string.Compare(Path.GetFileName(f), name, true) == 0);

        public void Dispose()
        {
        }
    }
}