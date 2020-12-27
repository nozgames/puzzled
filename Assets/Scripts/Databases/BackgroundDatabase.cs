using System;
using System.Linq;
using UnityEngine;

namespace Puzzled
{
    public class BackgroundDatabase : AddressableDatabase<Background>
    {
        [SerializeField] private Material _gradientMaterial = null;

        private static BackgroundDatabase _instance = null;

        private void Awake()
        {
            _instance = this;
        }

        public static bool isLoaded => _instance != null && _instance.loaded;

        public static Background[] GetBackgrounds() => _instance._cache.Values.ToArray();

        public static Background GetBackground(Guid guid) => _instance.GetAsset(guid);

        protected override string label => "background";

        protected override void OnLoaded()
        {
            foreach (var kv in _instance._cache)
            {
                var background = kv.Value;
                background.guid = kv.Key;
                
                var gradientTexture = new Texture2D(128, 64, TextureFormat.ARGB32, false);

                for (int i = 0; i < 31; i++)
                    for (int x = 0; x < 128; x++)
                        gradientTexture.SetPixel(x, 63 - i, new Color(1, 1, 1, 0));

                for(int i = 31; i<33; i++)
                for (int x = 0; x < 128; x++)
                    gradientTexture.SetPixel(x, 31, new Color(1, 1, 1, 1));

                for (int i = 33; i < 64; i++)
                    for (int x = 0; x < 128; x++)
                        gradientTexture.SetPixel(x, 63 - i, new Color(1, 1, 1, Mathf.Min(1.0f, (i - 33) / 30.0f + 0.25f)));


                gradientTexture.filterMode = FilterMode.Bilinear;
                gradientTexture.alphaIsTransparency = true;
                gradientTexture.Apply();

                var mat = new Material(_gradientMaterial);
                mat.mainTexture = gradientTexture;
                mat.color = background.color;
                background.gradient = mat;
            }
        }
    }
}
