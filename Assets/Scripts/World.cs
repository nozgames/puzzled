using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using UnityEngine;

namespace Puzzled
{
    public class World : IDisposable
    {
        private string _displayName;
        private string _path;

        IWorldArchive archive;

        public string displayName => string.IsNullOrEmpty(_displayName) ? Path.GetFileNameWithoutExtension(_path) : _displayName;

        public interface IPuzzleEntry
        {
            string filename { get; }
        }

        private class PuzzleEntry : IPuzzleEntry
        {
            public PuzzleEntry(IWorldArchiveEntry entry)
            {
                archiveEntry = entry;
            }

            public IWorldArchiveEntry archiveEntry { get; set; }
            public string filename => archiveEntry.path;
        };

        List<PuzzleEntry> _puzzles;

        public int puzzleCount => _puzzles.Count;
        public IEnumerable<IPuzzleEntry> puzzles => _puzzles;

        private World(IWorldArchive archive)
        {
            this.archive = archive;
            _puzzles = new List<PuzzleEntry>();

            foreach (IWorldArchiveEntry entry in archive.entries.Where(e => (Path.GetExtension(e.name) == ".puzzle")))
                _puzzles.Add(new PuzzleEntry(entry));
        }

        public static World New(IWorldArchive archive)
        {
            return new World(archive);
        }

        public static World Load(IWorldArchive archive) 
        {
            return new World(archive);
        }

        public void Save() 
        {
            // TODO: save world info
        }

        public void SetPuzzleEntryIndex(IPuzzleEntry entry, int index)
        {
            _puzzles.Remove(entry as PuzzleEntry);
            _puzzles.Insert(index, entry as PuzzleEntry);
        }

        public void RenamePuzzleEntry(IPuzzleEntry entry, string name)
        {
            if (entry is PuzzleEntry puzzleEntry)
            {
                IWorldArchiveEntry newEntry = archive.CreateEntry(name);
                IWorldArchiveEntry oldEntry = puzzleEntry.archiveEntry;

                using var oldFile = puzzleEntry.archiveEntry.Open();
                using var newFile = newEntry.Open();
                oldFile.CopyTo(newFile);

                puzzleEntry.archiveEntry = newEntry;
                oldEntry.Delete();
            }
        }

        public void DeletePuzzleEntry(IPuzzleEntry entry)
        {
            if (entry is PuzzleEntry puzzleEntry)
            {
                puzzleEntry.archiveEntry.Delete();
                _puzzles.Remove(puzzleEntry);
            }
        }

        public IPuzzleEntry NewPuzzleEntry(string name)
        {
            IWorldArchiveEntry entry = archive.CreateEntry(name);
            PuzzleEntry newPuzzle = new PuzzleEntry(entry);
            _puzzles.Add(newPuzzle);

            return newPuzzle;
        }

        public Puzzle LoadPuzzle(IPuzzleEntry entry)
        {
            if (entry is PuzzleEntry puzzleEntry)
            {
                try
                {
                    using var file = puzzleEntry.archiveEntry.Open();
                    return Puzzle.Load(file, puzzleEntry.archiveEntry.path);
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }

            return null;
        }

        public void SavePuzzle(IPuzzleEntry entry, Puzzle puzzle)
        {
            if (entry is PuzzleEntry puzzleEntry)
            {
                try
                {
                    using (var file = puzzleEntry.archiveEntry.Open())
                        puzzle.Save(file, puzzleEntry.archiveEntry.path);

                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }
        }

        public void Dispose()
        {
            if (archive != null)
                archive.Dispose();
        }
    }
}
