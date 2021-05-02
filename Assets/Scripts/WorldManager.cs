using System.Linq;
using System.IO;
using System.Collections.Generic;
using UnityEngine;

namespace Puzzled
{
    public static class WorldManager
    {
        private const string WorldExtension = ".world";

        public interface IWorldEntry
        {
            bool isEditable { get; }
            bool isInternal { get; }
            string name { get; }
            string path { get; }

            IWorldArchive OpenArchive();
        }

        private class WorldEntry : IWorldEntry
        {
            public string name { get; set; }
            public string path { get; set; }

            public bool isEditable => false;
            public bool isInternal => false;

            public IWorldArchive OpenArchive()
            {
                if (IsZipArchive(path))
                    return new ZipWorldArchive(File.Open(path, FileMode.Create));
                else
                    return new DirectoryWorldArchive(path);
            }
        }

        private static bool IsZipArchive(string path) => Path.GetExtension(path) == WorldExtension;

#if UNITY_EDITOR
        private static readonly string DirectoryArchivePath = Path.Combine(Application.dataPath, "../Worlds");
#else
        private static readonly string DirectoryArchivePath = Path.Combine(Application.persistentDataPath, "MyWorlds");
#endif

        private static readonly string InternalZipArchivePath = Path.Combine(Application.streamingAssetsPath, "Worlds");
        private static readonly string ExternalZipArchivePath = Path.Combine(Application.persistentDataPath, "Worlds");

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
            foreach (var entry in FindZipWorldArchives(InternalZipArchivePath))
                yield return entry;

            foreach (var entry in FindZipWorldArchives(ExternalZipArchivePath))
                yield return entry;
        }

        public static IEnumerable<IWorldEntry> GetEditableWorldEntries()
        {
            if (!Directory.Exists(DirectoryArchivePath))
                yield break;

            foreach (var path in Directory.GetDirectories(DirectoryArchivePath))
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

        public static World NewWorld(string name)
        {
            var fullPath = Path.Combine(DirectoryArchivePath, name);

            // TODO: does the world exist already?

            Directory.CreateDirectory(fullPath);

            //return World.New(new DirectoryWorldArchive(fullPath));r
            return null;
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
                InternalZipArchivePath;
#else
                ExternalZipArchivePAth;
#endif

            var zipArchive = new ZipWorldArchive(File.Create(Path.Combine(InternalZipArchivePath, $"{entry.name}.{WorldExtension}")));

            foreach(var sourceEntry in directoryArchive.entries)
            {
                var targetEntry = zipArchive.CreateEntry(sourceEntry.name);
                using var sourceStream = sourceEntry.Open();
                using var targetStream = targetEntry.Open();
                sourceStream.CopyTo(targetStream);
            }
        }
    }
}
