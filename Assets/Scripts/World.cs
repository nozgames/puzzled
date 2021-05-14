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

        IWorldArchive _archive;
        WorldManager.IWorldEntry _worldEntry;

        public string displayName => string.IsNullOrEmpty(_displayName) ? Path.GetFileNameWithoutExtension(_worldEntry.name) : _displayName;

        public interface IPuzzleEntry
        {
            bool isCompleted { get; }
            string name { get; }
            World world { get; }

            Puzzle Load();
            Puzzle.Meta LoadMeta();
            void MarkCompleted();

            void Save(Puzzle puzzle);
        }

        private class PuzzleEntry : IPuzzleEntry
        {
            public PuzzleEntry(World world, IWorldArchiveEntry entry)
            {
                archiveEntry = entry;
                this.world = world;
                meta = world.LoadPuzzleMeta(this);
            }

            public IWorldArchiveEntry archiveEntry { get; set; }
            public string name => Path.GetFileNameWithoutExtension(archiveEntry.name);
            public World world { get; private set; }
            public Puzzle.Meta meta { get; private set; }
            public Guid guid => meta.guid;

            public Puzzle Load() => world.LoadPuzzle(this);

            public Puzzle.Meta LoadMeta() => world.LoadPuzzleMeta(this);

            public void Save(Puzzle puzzle) => world.SavePuzzle(this, puzzle);

            /// <summary>
            /// Returns true if the puzzle has been marked as completed
            /// </summary>
            public bool isCompleted => Puzzle.IsCompleted(guid);

            /// <summary>
            /// Mark the puzzle as completed
            /// </summary>
            public void MarkCompleted() => Puzzle.MarkCompleted(guid);
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
                IWorldArchiveEntry newEntry = _archive.CreateEntry($"{name}.puzzle");
                IWorldArchiveEntry oldEntry = puzzleEntry.archiveEntry;

                using (var oldFile = puzzleEntry.archiveEntry.Open())
                using (var newFile = newEntry.Open())
                    oldFile.CopyTo(newFile);

                puzzleEntry.archiveEntry = newEntry;
                oldEntry.Delete();
            }
        }

        public IPuzzleEntry DuplicatePuzzleEntry (IPuzzleEntry entry)
        {
            var puzzleEntry = entry as PuzzleEntry;
            if (null == puzzleEntry)
                return null;

            // Find the next name by adding numbers to it
            var name = entry.name.GetNextName();
            while(_archive.Contains(name + ".puzzle"))
                name = name.GetNextName();

            IWorldArchiveEntry newEntry = _archive.CreateEntry(name + ".puzzle");

            using (var oldFile = puzzleEntry.archiveEntry.Open())
            using (var newFile = newEntry.Open())
                oldFile.CopyTo(newFile);

            var newPuzzleEntry = new PuzzleEntry(this, newEntry);
            _puzzles.Add(newPuzzleEntry);

            return newPuzzleEntry;
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

        private Puzzle.Meta LoadPuzzleMeta(IPuzzleEntry entry)
        {
            if (entry is PuzzleEntry puzzleEntry)
            {
                try
                {
                    using var file = puzzleEntry.archiveEntry.Open();
                    return Puzzle.LoadMeta(file);
                } catch (Exception e)
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

        public bool Contains(string name) =>
            puzzles.Any(p => 0 == string.Compare(name, p.name, true));

        public int IndexOf(IPuzzleEntry puzzleEntry) =>
            _puzzles.IndexOf(puzzleEntry as PuzzleEntry);

        public void Dispose()
        {
            if (_archive != null)
                _archive.Dispose();
        }

        public void Export()
        {
            WorldManager.ExportWorld(_worldEntry);
        }
    }
}
