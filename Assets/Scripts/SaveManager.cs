using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;

namespace Puzzled
{
    class SaveManager : MonoBehaviour
    {
        private static SaveManager _instance = null;
        private Dictionary<string, int> _integers = new Dictionary<string, int>();
        private bool _sandbox = false;

        private const int Version = 1;

        private void Awake()
        {
            _instance = this;
        }

        public static void Initialize()
        {
            Load();
        }

        public static void Shutdown()
        {
            Save();
        }

        public static void BeginSandbox ()
        {
            if (_instance._sandbox)
                return;

            Save();

            ClearInternal();
            _instance._sandbox = true;
        }

        public static void EndSandbox ()
        {
            if (!_instance._sandbox)
                return;

            ClearInternal();
            Load();

            _instance._sandbox = false;
        }

        private static void ClearInternal()
        {
            _instance._integers.Clear();
        }

        public static void Clear ()
        {
            ClearInternal();
            Save();
        }

        public static bool GetBool (string key, bool defaultValue=false) =>
            _instance._integers.TryGetValue(key, out var value) ? (value != 0) : defaultValue;

        public static int GetInt32(string key, int defaultValue=0) => 
            _instance._integers.TryGetValue(key, out var value) ? value : defaultValue;

        public static void SetBool(string key, bool value) => SetInt32(key, value ? 1 : 0);

        public static void SetInt32(string key, int value)
        {
            _instance._integers[key] = value;
            Save();
        }

        private static string GetFilePath () => 
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "My Games", "Puzzled", "puzzled.sav");
            

        private static void Load ()
        {
            ClearInternal();

            try
            {
                using var stream = File.OpenRead(GetFilePath());
                using var reader = new BinaryReader(stream);

                if (!reader.ReadFourCC('P', 'S', 'A', 'V'))
                    return;

                var version = reader.ReadInt32();
                if (version <= 0 || version > Version)
                    return;

                // Read integers
                var intCount = reader.ReadInt32();
                for(int i=0; i<intCount; i++)
                {
                    var key = reader.ReadString();
                    var value = reader.ReadInt32();
                    _instance._integers[key] = value;
                }
            }
            catch
            {
                Clear();
            }            
        }

        private static void Save ()
        {
            // When in sandbox mode dont actually save anything
            if (_instance._sandbox)
                return;

            try
            {
                var path = GetFilePath();
                Directory.CreateDirectory(Path.GetDirectoryName(path));

                using var stream = File.Create(path);
                using var writer = new BinaryWriter(stream);

                writer.WriteFourCC('P', 'S', 'A', 'V');
                writer.Write(Version);
                writer.Write(_instance._integers.Count);

                // Read integers
                foreach(var kv in _instance._integers)
                {
                    writer.Write(kv.Key);
                    writer.Write(kv.Value);
                }
            } 
            catch
            {
                File.Delete(GetFilePath());
            }
        }
    }
}
