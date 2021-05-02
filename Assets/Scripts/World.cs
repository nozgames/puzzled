﻿using System;
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

        IWorldArchive _archive;
        WorldManager.IWorldEntry _worldEntry;

        public string displayName => string.IsNullOrEmpty(_displayName) ? Path.GetFileNameWithoutExtension(_worldEntry.name) : _displayName;

        public interface IPuzzleEntry
        {
            string name { get; }
            World world { get; }

            Puzzle Load();

            void Save(Puzzle puzzle);
        }

        private class PuzzleEntry : IPuzzleEntry
        {
            public PuzzleEntry(World world, IWorldArchiveEntry entry)
            {
                archiveEntry = entry;
                this.world = world;
            }

            public IWorldArchiveEntry archiveEntry { get; set; }
            public string name => Path.GetFileNameWithoutExtension(archiveEntry.name);
            public World world { get; private set; }

            public Puzzle Load() => world.LoadPuzzle(this);

            public void Save(Puzzle puzzle) => world.SavePuzzle(this, puzzle);
        };

        List<PuzzleEntry> _puzzles;

        public int puzzleCount => _puzzles.Count;
        public IEnumerable<IPuzzleEntry> puzzles => _puzzles;

        private World(WorldManager.IWorldEntry worldEntry)
        {
            _worldEntry = worldEntry;
            _archive = _worldEntry.OpenArchive();
            _puzzles = new List<PuzzleEntry>();

            foreach (IWorldArchiveEntry entry in _archive.entries.Where(e => (Path.GetExtension(e.name) == ".puzzle")))
                _puzzles.Add(new PuzzleEntry(this, entry));
        }

        public static World New(WorldManager.IWorldEntry worldEntry)
        {
            return new World(worldEntry);
        }

        public static World Load(WorldManager.IWorldEntry worldEntry) 
        {
            return new World(worldEntry);
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
                IWorldArchiveEntry newEntry = _archive.CreateEntry(name);
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
            name = $"{name}.puzzle";

            if (_archive.Contains(name))
                return null;

            IWorldArchiveEntry entry = _archive.CreateEntry(name);
            PuzzleEntry newPuzzle = new PuzzleEntry(this, entry);

            var puzzle = GameManager.InstantiatePuzzle();
            SavePuzzle(newPuzzle, puzzle);
            puzzle.Destroy();

            _puzzles.Add(newPuzzle);

            return newPuzzle;
        }

        private Puzzle LoadPuzzle(IPuzzleEntry entry)
        {
            if (entry is PuzzleEntry puzzleEntry)
            {
                try
                {
                    using var file = puzzleEntry.archiveEntry.Open();
                    return Puzzle.Load(file);
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }

            return null;
        }

        private void SavePuzzle(IPuzzleEntry entry, Puzzle puzzle)
        {
            if (entry is PuzzleEntry puzzleEntry)
            {
                try
                {
                    using (var file = puzzleEntry.archiveEntry.Open())
                        puzzle.Save(file);
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }
        }

        public void Dispose()
        {
            if (_archive != null)
                _archive.Dispose();
        }
    }
}
