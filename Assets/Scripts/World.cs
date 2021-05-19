using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using Puzzled.Extensions;

namespace Puzzled
{
    public class World
    {
        public enum PuzzleUnlockRuleType
        {
            Unknown,        // 0
            Completed,      // 1
            AnyOf,          // 2
            AllOf           // 3
        }

        [Serializable]
        public class PuzzleUnlockRule
        {
            public PuzzleUnlockRuleType type;
            public string guid;
        }

        [Serializable]
        private class TextureMeta
        {
            public string guid;
            public bool hasColor;
        }

        [Serializable]
        private class SerializedTransition
        {
            public string text;
            public string texture;
        }

        [Serializable]
        private class WorldMeta
        {
            public string guid;
            public bool test;
            public string[] order;
            public SerializedTransition transitionIn;
            public SerializedTransition transitionOut;
        }

        [Serializable]
        private class PuzzleMeta
        {
            public string guid;
            public bool hideWhenLocked;
            public PuzzleUnlockRule[] unlock;
            public SerializedTransition transitionIn;
            public SerializedTransition transitionOut;
        }

        private class TextureEntry
        {
            public Guid guid;
            public string path;
            public Texture2D cached;
            public bool hasColor;
        }

        public class Transition
        {
            public Texture2D texture;
            public string text;
        }

        public interface IPuzzleEntry
        {
            bool isCompleted { get; }
            bool isLocked { get; }
            string name { get; }
            World world { get; }

            bool hideWhenLocked { get; set; }

            Puzzle Load();
            void MarkCompleted();

            Transition transitionIn { get; }
            Transition transitionOut { get; }

            void Save(Puzzle puzzle);

            public List<PuzzleUnlockRule> unlockRules { get; }
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
            public bool hideWhenLocked { get; set; }

            public List<PuzzleUnlockRule> unlockRules { get; set; }

            public Puzzle Load() => world.LoadPuzzle(this);

            public void Save(Puzzle puzzle) => world.SavePuzzle(this, puzzle);

            public bool isLocked => !IsPuzzleUnlocked(this);

            /// <summary>
            /// Returns true if the puzzle has been marked as completed
            /// </summary>
            public bool isCompleted => PlayerPrefs.GetInt(completedKey) != 0;

            /// <summary>
            /// Mark the puzzle as completed
            /// </summary>
            public void MarkCompleted() => PlayerPrefs.SetInt(completedKey, 1);

            /// <summary>
            /// Key used to save completed state in the player preferences
            /// </summary>
            private string completedKey => $"{world.guid}_{guid}_COMPLETED";

            public SerializedTransition serializedTransitionIn { get; set; }
            public SerializedTransition serializedTransitionOut { get; set; }

            public Transition transitionIn => world.LoadTransition(serializedTransitionIn);

            public Transition transitionOut => world.LoadTransition(serializedTransitionOut);
        };

        private const string WorldInfoFilename = "world.info";
        private const string PuzzleExtension = ".puzzle";
        private const string MetaExtension = ".meta";
        private const string TextureExtension = ".png";

        private string _displayName;
        private WorldManager.IWorldEntry _worldEntry;
        private List<PuzzleEntry> _puzzles;
        private List<TextureEntry> _textures;
        private SerializedTransition _serializedTransitionIn;
        private SerializedTransition _serializedTransitionOut;

        public string displayName => string.IsNullOrEmpty(_displayName) ? Path.GetFileNameWithoutExtension(_worldEntry.name) : _displayName;

        public int puzzleCount => _puzzles.Count;
        public IEnumerable<IPuzzleEntry> puzzles => _puzzles;
        public Guid guid { get; private set; }

        public bool isModified { get; private set; }

        public void SetModified() => isModified = true;

        /// <summary>
        /// Returns true if this is the first time playing this world
        /// </summary>
        public bool isFirstPlay => PlayerPrefs.GetInt($"{guid}_PLAYED") != 0;

        /// <summary>
        /// Mark this world as being played for the first time
        /// </summary>
        public void MarkFirstPlay () => PlayerPrefs.SetInt($"{guid}_PLAYED", 1);

        /// <summary>
        /// Returns all loaded textures as decals
        /// </summary>
        public IEnumerable<Decal> decals => _textures.Where(t => t.cached != null).Select(t => GetDecal(null, t));

        public Transition transitionIn => LoadTransition(_serializedTransitionIn);

        public Transition transitionOut => LoadTransition(_serializedTransitionOut);

        /// <summary>
        /// THIS IS JUST FOR TESTING, REMOVE WHEN THERE ARE REAL PROPERTIES
        /// </summary>
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

            using var archive = _worldEntry.OpenArchive();

            LoadPuzzleEntries(archive);
            LoadWorldMeta(archive);
            LoadTextureEntries(archive);

            isModified = false;
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
            SaveWorldMeta(archive);
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
        private void SaveWorldMeta (IWorldArchive archive)
        {
            SaveMeta(archive, MetaExtension, new WorldMeta {
                guid = guid.ToString(),
                test = test,
                order = _puzzles.Select(p => p.guid.ToString()).ToArray(),
                transitionIn = _serializedTransitionIn,
                transitionOut = _serializedTransitionOut
            });

            isModified = false;
        }

        /// <summary>
        /// Load the puzzle world info from the given archive
        /// </summary>
        /// <param name="archive">Source archive</param>
        private void LoadWorldMeta (IWorldArchive archive)
        {
            var meta = LoadMeta<WorldMeta>(archive, MetaExtension);
            if(null == meta)
            {
                guid = Guid.NewGuid();
                SaveWorldMeta(archive);
                return;
            }

            test = meta.test;
            _serializedTransitionIn = meta.transitionIn;
            _serializedTransitionOut = meta.transitionOut;

            // Fix invalid guids
            if (!Guid.TryParse(meta.guid, out var metaGuid) || metaGuid == Guid.Empty)
            {
                guid = metaGuid = Guid.NewGuid();
                SaveWorldMeta(archive);
            }

            guid = metaGuid;
            
            // Optionally load the puzzle order
            if(meta.order != null)
            {
                int index = 0;
                for(int i=0; i<meta.order.Length; i++)
                {
                    if (!Guid.TryParse(meta.order[i], out var guid))
                        continue;

                    var puzzleEntry = GetPuzzleEntry(guid);
                    if (null == puzzleEntry)
                        continue;

                    SetPuzzleEntryIndex(puzzleEntry, index++);
                }
            }
        }

        /// <summary>
        /// Load the list of textures from the world
        /// </summary>
        /// <param name="archive">Archive to load textures from</param>
        private void LoadTextureEntries (IWorldArchive archive)
        {
            _textures = archive.entries.Where(e => Path.GetExtension(e.name) == TextureExtension).ToArray().Select(e => {
                var meta = LoadMeta<TextureMeta>(archive, e.name + MetaExtension);                                
                var textureEntry = new TextureEntry {
                    guid = Guid.TryParse(meta?.guid, out var guid) ? guid : Guid.NewGuid(),
                    path = e.name,
                    hasColor = meta?.hasColor ?? false
                };

                if (meta == null)
                {                    
                    if (!SaveTextureMeta(archive, textureEntry))
                    {
                        Debug.LogWarning($"world texture '{e.name}' has no meta file and will be ignored");
                        return null;
                    }                        
                }

                return textureEntry;
            }).Where(te => te != null).ToList();
        }

        private T LoadMeta<T> (IWorldArchive archive, string metaPath) where T : class
        {
            var metaEntry = archive.entries.FirstOrDefault(e => e.name == metaPath);
            if (null == metaEntry)
                return null;

            try
            {
                using var metaStream = metaEntry.Open();
                using var metaReader = new StreamReader(metaStream);
                return JsonUtility.FromJson<T>(metaReader.ReadToEnd());
            }
            catch
            {
                return null;
            }
        }

        private bool SaveMeta (IWorldArchive archive, string metaPath, object meta)
        {           
            try
            {
                var metaEntry = archive.entries.FirstOrDefault(e => e.name == metaPath);
                if (null == metaEntry)
                    metaEntry = archive.CreateEntry(metaPath);

                using var metaStream = metaEntry.Open();
                using var metaWriter = new StreamWriter(metaStream);
                metaWriter.Write(JsonUtility.ToJson(meta, true));
                return true;
            }
            catch
            {
                return false;
            }
        }

        private bool SaveTextureMeta(IWorldArchive archive, TextureEntry entry) =>
            SaveMeta(archive, entry.path + MetaExtension, new TextureMeta {
                guid = entry.guid.ToString(),
                hasColor = entry.hasColor
            });

        private bool SavePuzzleMeta(IWorldArchive archive, PuzzleEntry entry) =>
            SaveMeta(archive, entry.path + MetaExtension, new PuzzleMeta {
                guid = entry.guid.ToString(),
                hideWhenLocked = entry.hideWhenLocked,
                unlock = entry.unlockRules?.ToArray(),
                transitionIn = entry.serializedTransitionIn,
                transitionOut = entry.serializedTransitionOut
            });

        /// <summary>
        /// Load an embedded texture from the world using the texture's guid
        /// </summary>
        /// <param name="guid">Texture guid</param>
        /// <returns>Loaded texture or null if the texture was not found or could not be loaded</returns>
        public Texture2D GetTexture (Guid guid)
        {
            var textureEntry = _textures.FirstOrDefault(t => t.guid == guid);
            if (null == textureEntry)
                return null;

            if (textureEntry.cached)
                return textureEntry.cached;

            using var archive = _worldEntry.OpenArchive();
            if (null == archive)
                return null;

            return LoadTexture(archive, textureEntry);
        }

        /// <summary>
        /// Load all embedded textures into memory
        /// </summary>
        public void LoadAllTextures ()
        {
            using var archive = _worldEntry.OpenArchive();
            foreach (var textureEntry in _textures)
                LoadTexture(archive, textureEntry);
        }

        /// <summary>
        /// Unload all embedded textures that are in memory
        /// </summary>
        public void UnloadAllTextures ()
        {
            foreach (var textureEntry in _textures)
                textureEntry.cached = null;

            Resources.UnloadUnusedAssets();
        }

        private Texture2D LoadTexture (IWorldArchive archive, TextureEntry textureEntry)
        {
            if (textureEntry == null)
                return null;

            // If no archive is given then open the archive just for this call.
            if(archive == null)
            {
                using var openedArchive = _worldEntry.OpenArchive();
                return LoadTexture(openedArchive, textureEntry);
            }

            try
            {
                // Already loaded?
                if (textureEntry.cached != null)
                    return textureEntry.cached;

                // Find the archive entry for the texture
                var archiveEntry = archive.entries.FirstOrDefault(e => e.name == textureEntry.path);
                if (null == archiveEntry)
                    return null;

                // Load the texture
                using var stream = archiveEntry.Open();

                textureEntry.cached = new Texture2D(1, 1);
                if (!textureEntry.cached.LoadImage(stream.ReadAllBytes(), false))
                    textureEntry.cached = null;
                else
                {
                    textureEntry.cached.name = Path.GetFileNameWithoutExtension(textureEntry.path);
                    textureEntry.cached.Apply();
                }

                return textureEntry.cached;
            } 
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Get the decal that matches the given guid
        /// </summary>
        /// <param name="guid">Decal guid</param>
        /// <returns>Decal that matches the given guid</returns>
        public Decal GetDecal(Guid guid) => GetDecal(null, _textures.FirstOrDefault(t => t.guid == guid));

        private Decal GetDecal(IWorldArchive archive, TextureEntry textureEntry)
        {
            if (null == textureEntry)
                return Decal.none;

            return new Decal(textureEntry.guid, LoadTexture(archive, textureEntry)) { isAutoColor = !textureEntry.hasColor, color = Color.white };
        }

        /// <summary>
        /// Add a new embedded texture to the world 
        /// </summary>
        /// <param name="path">Path to the source file</param>
        /// <param name="name">Name of the embedded texture</param>
        /// <param name="overwrite">True if the texture should be overwritten if it already exists</param>
        /// <returns></returns>
        public Guid AddTexture(string sourcePath, string name, bool overwrite = false)
        {
            // Only pngs supported
            if (Path.GetExtension(sourcePath) != ".png")
                return Guid.Empty;

            // TODO: validate the texture loads ok and do any conversions necessary
            using var sourceStream = File.OpenRead(sourcePath);
            var texture = new Texture2D(1, 1);
            if (!texture.LoadImage(sourceStream.ReadAllBytes(), false))
                return Guid.Empty;

            texture.name = Path.GetFileNameWithoutExtension(name);
            
            using var archive = _worldEntry.OpenArchive();
            if (null == archive)
                return Guid.Empty;

            // Is there already a texture with that name?
            var archiveEntry = archive.entries.FirstOrDefault(e => e.name == name);
            if (!overwrite && null != archiveEntry)
                return Guid.Empty;

            if (null == archiveEntry)
            {
                archiveEntry = archive.CreateEntry(name);
                if (null == archiveEntry)
                    return Guid.Empty;
            }

            // Copy the texture to the archive
            using var stream = archiveEntry.Open();
            var streamWriter = new StreamWriter(stream);
            streamWriter.Write(texture.EncodeToPNG());

            var textureEntry = new TextureEntry {
                guid = Guid.NewGuid(),
                cached = texture
            };

            _textures.Add(textureEntry);

            SaveTextureMeta(archive, textureEntry);

            return textureEntry.guid;
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
                Puzzle.Duplicate(puzzleEntry, oldFile, newFile);
            }

            var newPuzzleEntry = new PuzzleEntry(this, newEntry.name, Guid.NewGuid());
            _puzzles.Add(newPuzzleEntry);

            SavePuzzleMeta(archive, newPuzzleEntry);

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

            // Create the puzzle entry
            var puzzleEntry = new PuzzleEntry(this, entry.name, Guid.NewGuid());
            _puzzles.Add(puzzleEntry);

            SavePuzzleMeta(archive, puzzleEntry);

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
                    return Puzzle.Load(entry, file);
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }

            return null;
        }

        private void LoadPuzzleEntries (IWorldArchive archive)
        {
            _puzzles = archive.entries.Where(e => (Path.GetExtension(e.name) == PuzzleExtension)).ToArray().Select(e => {
                try
                {
                    var metaPath = e.name + MetaExtension;
                    var archiveEntry = archive.entries.Where(e => e.name == metaPath).FirstOrDefault();

                    PuzzleMeta meta;

                    if (archiveEntry != null)
                    {
                        using var stream = archiveEntry.Open();
                        using var reader = new StreamReader(stream);
                        meta = JsonUtility.FromJson<PuzzleMeta>(reader.ReadToEnd());
                    } else
                    {
                        meta = new PuzzleMeta { guid = Guid.NewGuid().ToString() };
                    }

                    if (!Guid.TryParse(meta.guid, out var metaGuid))
                        return null;

                    var puzzleEntry = new PuzzleEntry(this, e.name, metaGuid) {
                        hideWhenLocked = meta.hideWhenLocked,
                        unlockRules = meta.unlock?.ToList(),
                        serializedTransitionIn = meta.transitionIn,
                        serializedTransitionOut = meta.transitionOut
                    };

                    if (archiveEntry == null)
                        SavePuzzleMeta(archive, puzzleEntry);

                    return puzzleEntry;

                } catch
                {
                }

                return null;
            }).Where(e => e != null).ToList();
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

        private Transition LoadTransition (SerializedTransition serializedTransition)
        {
            if (string.IsNullOrEmpty(serializedTransition.text) && string.IsNullOrEmpty(serializedTransition.texture))
                return null;

            var transition = new Transition();
            if(Guid.TryParse(serializedTransition.texture, out var textureGuid))
                transition.texture = GetTexture(textureGuid);

            transition.text = serializedTransition.text;
            return transition;
        }

        public static void Clone (WorldManager.IWorldEntry worldEntry, IWorldArchive target)
        {
            using var archive = worldEntry.OpenArchive();
            if (null == archive)
                return;

            foreach(var sourceEntry in archive.entries)
            {
                var targetEntry = target.CreateEntry(sourceEntry.name);

                using var sourceStream = sourceEntry.Open();
                using var targetStream = targetEntry.Open();
                sourceStream.CopyTo(targetStream);
            }
        }

        /// <summary>
        /// Returns true if the given puzzle entry is currently unlocked
        /// </summary>
        /// <param name="puzzleEntry">Puzzle entry</param>
        /// <returns>True if the puzzle is unlocked, false if not</returns>
        private static bool IsPuzzleUnlocked(PuzzleEntry puzzleEntry)
        {
            if (puzzleEntry.unlockRules == null || puzzleEntry.unlockRules.Count == 0)
                return true;

            var parentRule = PuzzleUnlockRuleType.Unknown;
            var parentResult = false;
            var childCount = 0;
            foreach(var rule in puzzleEntry.unlockRules)
            {
                var childResult = false;
                switch (rule.type)
                {
                    case PuzzleUnlockRuleType.Completed:
                        childResult = puzzleEntry.world.GetPuzzleEntry(Guid.TryParse(rule.guid, out var guid) ? guid : Guid.Empty)?.isCompleted ?? false;
                        childCount++;
                        break;

                    case PuzzleUnlockRuleType.AllOf:
                    case PuzzleUnlockRuleType.AnyOf:
                        // If the previous parent rule failed then fail
                        if(parentRule != PuzzleUnlockRuleType.Unknown && childCount >= 0 && !parentResult)
                            return false;                            

                        // Start a new parent rule
                        parentRule = rule.type;
                        parentResult = false;
                        continue;

                    default:
                        continue;
                }

                // Handle the child result depending on the current parent rule
                switch (parentRule)
                {
                    case PuzzleUnlockRuleType.AllOf:
                    default:
                        if (!childResult)
                            return false;
                        parentResult = true;
                        break;

                    case PuzzleUnlockRuleType.AnyOf:
                        parentResult = true;
                        break;
                }
            }

            if (parentRule != PuzzleUnlockRuleType.Unknown && childCount > 0)
                return parentResult;

            return true;
        }
    }
}
