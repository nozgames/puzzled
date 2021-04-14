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
        ZipArchive archive;

        public string displayName => string.IsNullOrEmpty(_displayName) ? Path.GetFileNameWithoutExtension(_path) : _displayName;

        public interface IPuzzleEntry
        {
            string filename { get; }
        }

        private class PuzzleEntry : IPuzzleEntry
        {
            public PuzzleEntry(ZipArchiveEntry entry)
            {
                zipEntry = entry;
            }

            public ZipArchiveEntry zipEntry { get; set; }
            public string filename => zipEntry.FullName;
        };

        List<PuzzleEntry> _puzzles;

        public int puzzleCount => _puzzles.Count;
        public IPuzzleEntry GetPuzzleEntry(int index) => _puzzles[index];
        public int GetPuzzleEntryIndex(IPuzzleEntry entry) => _puzzles.IndexOf(entry as PuzzleEntry);

        private World(Stream stream)
        {
            archive = new ZipArchive(stream);
            _puzzles = new List<PuzzleEntry>();

            foreach (ZipArchiveEntry entry in archive.Entries.Where(e => (Path.GetExtension(e.Name) == ".puzzle")))
                _puzzles.Add(new PuzzleEntry(entry));
        }

        public static World New(string path)
        {
            Stream stream = File.Create(path);
            return new World(stream);
        }

        public static World Load(string path) 
        {
            Stream stream = File.Open(path, FileMode.Open, FileAccess.ReadWrite);
            return new World(stream);
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
                ZipArchiveEntry newEntry = archive.CreateEntry(name);
                ZipArchiveEntry oldEntry = puzzleEntry.zipEntry;

                using var oldFile = puzzleEntry.zipEntry.Open();
                using var newFile = newEntry.Open();
                oldFile.CopyTo(newFile);

                puzzleEntry.zipEntry = newEntry;
                oldEntry.Delete();
            }
        }

        public void DeletePuzzleEntry(IPuzzleEntry entry)
        {
            if (entry is PuzzleEntry puzzleEntry)
            {
                puzzleEntry.zipEntry.Delete();
                _puzzles.Remove(puzzleEntry);
            }
        }

        public IPuzzleEntry NewPuzzleEntry(string name)
        {
            ZipArchiveEntry entry = archive.CreateEntry(name);
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
                    using var file = puzzleEntry.zipEntry.Open();
                    return Puzzle.Load(file, puzzleEntry.zipEntry.FullName);
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
                    using (var file = puzzleEntry.zipEntry.Open())
                        puzzle.Save(file, puzzleEntry.zipEntry.FullName);

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
