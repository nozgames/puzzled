using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using UnityEngine;

namespace Puzzled
{
    public class World
    {
        public interface IPuzzleEntry
        {
            bool isCompleted { get; }
            string name { get; }
            World world { get; }

            Puzzle Load();
            void MarkCompleted();

            void Save(Puzzle puzzle);
        }

        private class PuzzleEntry : IPuzzleEntry
        {
            public PuzzleEntry(World world, string path, Guid guid)
            {
                this.path = path;
                this.world = world;
                this.guid = guid;
            }

            public string path { get; set; }
            public string name => Path.GetFileNameWithoutExtension(path);
            public World world { get; private set; }
            public Guid guid { get; private set; }

            public Puzzle Load() => world.LoadPuzzle(this);

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

        private const int Version = 2;
        private const string WorldInfoFilename = "world.info";
        private const string PuzzleExtension = ".puzzle";

        private string _displayName;
        private WorldManager.IWorldEntry _worldEntry;
        private List<PuzzleEntry> _puzzles;

        public string displayName => string.IsNullOrEmpty(_displayName) ? Path.GetFileNameWithoutExtension(_worldEntry.name) : _displayName;

        public int puzzleCount => _puzzles.Count;
        public IEnumerable<IPuzzleEntry> puzzles => _puzzles;

        public bool isModified { get; private set; }

        private bool _test = false;
        public bool test {
            get => _test;
            set {
                _test = value;
                isModified = true;
            }
        }

        private World(WorldManager.IWorldEntry worldEntry)
        {
            _worldEntry = worldEntry;
            _puzzles = new List<PuzzleEntry>();

            // Load the puzzle list from the archive
            using var archive = _worldEntry.OpenArchive();
            foreach (IWorldArchiveEntry entry in archive.entries.Where(e => (Path.GetExtension(e.name) == PuzzleExtension)))
            {
                var meta = LoadPuzzleMeta(archive, entry.name);
                if (null == meta)
                    continue;

                _puzzles.Add(new PuzzleEntry(this, entry.name, meta.guid));
            }

            // Load the world info from the archive
            LoadWorldInfo(archive);
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
            using var archive = _worldEntry.OpenArchive();
            SaveWorldInfo(archive);
        }

        /// <summary>
        /// Get the world info archive entry within the given archive and optionally create one if none exists
        /// </summary>
        /// <param name="archive">Archive to retrieve entry from</param>
        /// <param name="createIfNotExist">True if an archive entry should be created if one was not found</param>
        /// <returns>World info archive entry or null if not found</returns>
        private IWorldArchiveEntry GetWorldInfoArchiveEntry (IWorldArchive archive, bool createIfNotExist = false)
        {
            var infoEntry = archive.entries.Where(e => e.name == WorldInfoFilename).FirstOrDefault();
            if (infoEntry == null && createIfNotExist)
                infoEntry = archive.CreateEntry(WorldInfoFilename);

            return infoEntry;
        }

        /// <summary>
        /// Returns the puzzle entry that matches the given globaly unique identifier
        /// </summary>
        /// <param name="guid">Globaly unique identifier to search for</param>
        /// <returns>Puzzle entry that matches the globaly unique identifier or null if not found</returns>
        public IPuzzleEntry GetPuzzleEntry(Guid guid) => _puzzles.FirstOrDefault(e => e.guid == guid);

        /// <summary>
        /// Returns the puzzle entry at the given index
        /// </summary>
        /// <param name="index">Puzzle index</param>
        /// <returns>Puzzle entry at the given inddex</returns>
        public IPuzzleEntry GetPuzzleEntry(int index) => _puzzles[index];

        /// <summary>
        /// Save the world information to the given archive
        /// </summary>
        /// <param name="archive">Target archive</param>
        private void SaveWorldInfo (IWorldArchive archive)
        {
            var infoEntry = GetWorldInfoArchiveEntry(archive, true);

            using var stream = infoEntry.Open();
            using var writer = new BinaryWriter(stream);

            writer.WriteFourCC('W', 'R', 'L', 'D');
            writer.Write(Version);
            writer.Write(puzzleCount);
            writer.Write(_displayName ?? "");
            writer.Write(_test);

            foreach (var puzzle in _puzzles)
                writer.Write(puzzle.guid);

            isModified = false;
        }

        /// <summary>
        /// Load the puzzle world info from the given archive
        /// </summary>
        /// <param name="archive">Source archive</param>
        private void LoadWorldInfo(IWorldArchive archive)
        {
            var infoEntry = GetWorldInfoArchiveEntry(archive);
            if(null == infoEntry)
                return;

            using var stream = infoEntry.Open();
            using var reader = new BinaryReader(stream);
            if (!reader.ReadFourCC('W', 'R', 'L', 'D'))
                throw new InvalidDataException();

            var version = reader.ReadInt32();
            var puzzleCount = reader.ReadInt32();

            _displayName = reader.ReadString();

            if (version > 1)
                _test = reader.ReadBoolean();

            // Reorder the puzzles based on the world info order
            for (var puzzleIndex = 0; puzzleIndex < puzzleCount; puzzleIndex++)
            {
                var guid = reader.ReadGuid();
                var puzzleEntry = GetPuzzleEntry(guid);
                if (null == puzzleEntry)
                {
                    Debug.LogWarning($"{guid}: world.info references puzzle that does not exist in the archive");
                    continue;
                }

                SetPuzzleEntryIndex(puzzleEntry, puzzleIndex);
            }

            isModified = false;
        }

        /// <summary>
        /// Return the index of the given puzzle entry within the list of puzzles.
        /// </summary>
        /// <param name="entry">Puzzle entry</param>
        /// <returns>Index of the entry within the puzzle list or -1 if not found</returns>
        public int GetPuzzleEntryIndex(IPuzzleEntry entry) => _puzzles.IndexOf(entry as PuzzleEntry);

        /// <summary>
        /// Set the index of the given puzzle entry within the list of puzzles.
        /// </summary>
        /// <param name="entry">Puzzle entry</param>
        /// <param name="index">New index</param>
        public void SetPuzzleEntryIndex(IPuzzleEntry entry, int index)
        {
            _puzzles.Remove(entry as PuzzleEntry);
            _puzzles.Insert(index, entry as PuzzleEntry);

            isModified = true;
        }

        public void RenamePuzzleEntry(IPuzzleEntry entry, string name)
        {
            if (entry is PuzzleEntry puzzleEntry)
            {
                using var archive = _worldEntry.OpenArchive();
                IWorldArchiveEntry newEntry = archive.CreateEntry($"{name}{PuzzleExtension}");
                IWorldArchiveEntry oldEntry = archive.entries.FirstOrDefault(e => e.name == puzzleEntry.path);
                
                using (var oldFile = oldEntry.Open())
                using (var newFile = newEntry.Open())
                    oldFile.CopyTo(newFile);

                puzzleEntry.path = newEntry.name;
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
            using var archive = _worldEntry.OpenArchive();
            while(archive.Contains(name + PuzzleExtension))
                name = name.GetNextName();

            var oldEntry = archive.entries.FirstOrDefault(e => e.name == puzzleEntry.path);
            var newEntry = archive.CreateEntry(name + PuzzleExtension);

            using (var oldFile = oldEntry.Open())
            using (var newFile = newEntry.Open())
            {
                Puzzle.Duplicate(oldFile, newFile);
            }

            var newPuzzleEntry = new PuzzleEntry(this, newEntry.name, LoadPuzzleMeta(archive, newEntry.name).guid);
            _puzzles.Add(newPuzzleEntry);

            isModified = true;

            return newPuzzleEntry;
        }

        public void DeletePuzzleEntry(IPuzzleEntry entry)
        {
            if (entry is PuzzleEntry puzzleEntry)
            {
                using var archive = _worldEntry.OpenArchive();
                var archiveEntry = archive.entries.FirstOrDefault(e => e.name == puzzleEntry.path);
                archiveEntry.Delete();
                _puzzles.Remove(puzzleEntry);

                isModified = true;
            }
        }

        public IPuzzleEntry NewPuzzleEntry(string name)
        {
            name = $"{name}{PuzzleExtension}";

            using var archive = _worldEntry.OpenArchive();
            if (archive.Contains(name))
                return null;

            var puzzle = GameManager.InstantiatePuzzle();
            var entry = archive.CreateEntry(name);
            using var stream = entry.Open();
            puzzle.Save(stream);

            var puzzleEntry = new PuzzleEntry(this, entry.name, puzzle.guid);
            _puzzles.Add(puzzleEntry);

            puzzle.Destroy();

            isModified = true;

            return puzzleEntry;
        }

        private Puzzle LoadPuzzle(IPuzzleEntry entry)
        {
            if (entry is PuzzleEntry puzzleEntry)
            {
                try
                {
                    using var archive = _worldEntry.OpenArchive();
                    using var file = archive.entries.FirstOrDefault(e => e.name == puzzleEntry.path).Open();
                    return Puzzle.Load(file);
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }

            return null;
        }

        private static Puzzle.Meta LoadPuzzleMeta(IWorldArchive archive, string path)
        {
            try
            {
                var archiveEntry = archive.entries.Where(e => e.name == path).FirstOrDefault();
                using var file = archiveEntry.Open();
                return Puzzle.LoadMeta(file);
            } catch (Exception e)
            {
                Debug.LogException(e);
            }

            return null;
        }

        private void SavePuzzle(IPuzzleEntry entry, Puzzle puzzle)
        {
            if (entry is PuzzleEntry puzzleEntry)
            {
                try
                {
                    using var archive = _worldEntry.OpenArchive();
                    var archiveEntry = archive.entries.Where(e => e.name == puzzleEntry.path).FirstOrDefault();
                    using var file = archiveEntry.Open();
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

        public void Export()
        {
            WorldManager.ExportWorld(_worldEntry);
        }

        public static void Clone (WorldManager.IWorldEntry worldEntry, IWorldArchive target)
        {
            var world = new World(worldEntry);
            world.SaveWorldInfo(target);
            foreach (var puzzleEntry in world.puzzles)
            {
                var puzzle = puzzleEntry.Load();
                if (null == puzzle)
                    continue;

                var targetEntry = target.CreateEntry((puzzleEntry as PuzzleEntry).path);
                using var targetStream = targetEntry.Open();
                puzzle.Save(targetStream);
                puzzle.Destroy();
            }
        }
    }
}
