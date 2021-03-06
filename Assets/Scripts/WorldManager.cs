﻿using System.Linq;
using System.IO;
using System.Collections.Generic;
using UnityEngine;

namespace Puzzled
{
    public class WorldManager : MonoBehaviour
    {
        private const string WorldExtension = ".world";

        [SerializeField] private Camera _previewCamera = null;

        private static WorldManager _instance = null;

        private static string directoryArchivePath =>
#if UNITY_EDITOR
            Path.Combine(Application.dataPath, "../Worlds");
#else
            Path.Combine(Application.persistentDataPath, "MyWorlds");
#endif

        private static string internalZipArchivePath =>
            Path.Combine(Application.streamingAssetsPath, "Worlds");

        private static string externalZipArchivePath =>
            Path.Combine(Application.persistentDataPath, "Worlds");

        private void Awake()
        {
            _instance = this;
        }

        public interface IWorldEntry
        {
            bool isEditable { get; }
            bool isInternal { get; }
            string name { get; }
            string path { get; }

            IWorldArchive OpenArchive();
        }

        private class CachedWorldArchiveWrapper : IWorldArchive
        {
            private IWorldArchive _cached;

            public int entryCount => _cached.entryCount;

            public IEnumerable<IWorldArchiveEntry> entries => _cached.entries;

            public bool isDisposed => _cached.isDisposed;

            public bool Contains(string name) => _cached.Contains(name);

            public IWorldArchiveEntry CreateEntry(string name) => _cached.CreateEntry(name);

            public CachedWorldArchiveWrapper(IWorldArchive cached) => _cached = cached;

            public void Dispose()
            {
            }
        }

        private class WorldEntry : IWorldEntry
        {
            public string name { get; set; }
            public string path { get; set; }

            public bool isEditable => false;
            public bool isInternal => false;

            private IWorldArchive _cachedArchive;

            public IWorldArchive OpenArchive()
            {
                if (_cachedArchive != null && !_cachedArchive.isDisposed)
                    return new CachedWorldArchiveWrapper(_cachedArchive);

                if (IsZipArchive(path))
                    _cachedArchive = new ZipWorldArchive(File.Open(path, FileMode.OpenOrCreate));
                else
                    _cachedArchive = new DirectoryWorldArchive(path);

                return _cachedArchive;
            }
        }

        private static bool IsZipArchive(string path) => Path.GetExtension(path) == WorldExtension;

        public static bool DoesEditableWorldExist(string path)
        {
            return false;
        }

        public static bool DoesPlayableWorldExist(string name)
        {
            return false;
        }

        public static IEnumerable<IWorldEntry> GetPlayableWorldEntries()
        {
            foreach (var entry in FindZipWorldArchives(internalZipArchivePath))
                yield return entry;

            foreach (var entry in FindZipWorldArchives(externalZipArchivePath))
                yield return entry;
        }

        public static IEnumerable<IWorldEntry> GetEditableWorldEntries()
        {
            if (!Directory.Exists(directoryArchivePath))
                yield break;

            foreach (var path in Directory.GetDirectories(directoryArchivePath))
                yield return new WorldEntry { path = path, name = Path.GetFileNameWithoutExtension(path) };
        }

        private static IEnumerable<IWorldEntry> FindZipWorldArchives(string path)
        {
            if (!Directory.Exists(path))
                yield break;

            foreach (var filename in Directory.GetFiles(path).Where(IsZipArchive))
                yield return new WorldEntry { path = filename, name = Path.GetFileNameWithoutExtension(filename) };
        }

        public static World LoadWorld(IWorldEntry ientry)
        {
            var worldEntry = ientry as WorldEntry;
            if (null == worldEntry)
                return null;

            return World.Load(ientry);
        }

        /// <summary>
        /// Create a new world with the given name
        /// </summary>
        /// <param name="name">Name of the new world</param>
        /// <returns>New world entry or null if the world failed to create</returns>
        public static IWorldEntry NewWorld(string name)
        {
            var fullPath = Path.Combine(directoryArchivePath, name);
            if (File.Exists(fullPath))
                return null;

            if (Directory.Exists(fullPath))
                return null;

            Directory.CreateDirectory(fullPath);

            return new WorldEntry { name = name, path = fullPath };
        }

        public static void ImportWorld()
        {
            // TODO: check to see if the world is locked and dont allow import
            // TODO: unzip to directory
        }

        public static void ExportWorld(IWorldEntry ientry)
        {
            // TODO: generate thumbnail images
            // TODO: strip imported assets that are not used
            // TODO: optionally mark world as locked so it cannot be imported

            var entry = ientry as WorldEntry;
            var directoryArchive = new DirectoryWorldArchive(entry.path);
            var archivePath =
#if UNITY_EDITOR
                internalZipArchivePath;
#else
                externalZipArchivePath;
#endif

            string zipFilePath = Path.Combine(archivePath, $"{entry.name}{WorldExtension}");
            
            Directory.CreateDirectory(Path.GetDirectoryName(zipFilePath));

            using var zipArchive = new ZipWorldArchive(File.Create(zipFilePath));

            World.Clone(ientry, zipArchive);
        }

        public static Texture2D CreatePreview (World.IPuzzleEntry puzzleEntry)
        {
            try
            {
                var camera = _instance._previewCamera;

                if (GameManager.puzzle != null)
                    GameManager.puzzle.Destroy();

                GameManager.puzzle = puzzleEntry.Load();
                GameManager.Play();
                Tile.Tick();
                //Tile.Tick();
                //Tile.Tick();
                //Tile.Tick();
                CameraManager.ForceUpdate();


                // Copy the main camera transform
                camera.transform.position = CameraManager.camera.transform.position;
                camera.transform.localRotation = CameraManager.camera.transform.localRotation;
                camera.transform.localScale = CameraManager.camera.transform.localScale;

                camera.Render();

                var t = new Texture2D(camera.targetTexture.width, camera.targetTexture.height, TextureFormat.ARGB32, false, true);
                t.filterMode = FilterMode.Bilinear;
                t.wrapMode = TextureWrapMode.Clamp;

                RenderTexture.active = camera.targetTexture;
                t.ReadPixels(new Rect(0, 0, t.width, t.height), 0, 0);
                t.Apply();

                GameManager.Stop();
                GameManager.UnloadPuzzle();

                return t;
            } 
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Updates all known worlds to the latest puzzle file version
        /// </summary>
        public static void UpdateWorlds ()
        {
            foreach(var worldEntry in GetEditableWorldEntries())
            {
                var world = World.Load(worldEntry);
                foreach(var puzzleEntry in world.puzzles)
                {
                    var puzzle = puzzleEntry.Load();
                    puzzleEntry.Save(puzzle);
                    puzzle.Destroy();
                }
            }
        }
    }
}
