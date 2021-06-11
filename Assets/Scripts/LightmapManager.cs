using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Puzzled
{
    class LightmapManager : MonoBehaviour
    {
        private static LightmapManager _instance = null;

        [SerializeField] private Camera _camera = null;
        [SerializeField] private RenderTexture _renderTexture = null;

#if UNITY_EDITOR
        private static Texture2D _white = null;

        private static void ResetWhite()
        {
            if (_instance != null)
                return;

            if(_white == null)
            {
                _white = new Texture2D(1, 1);
                _white.SetPixel(0, 0, Color.white);
                _white.Apply();
            }

            Shader.SetGlobalTexture("_void_texture", _white);

            EditorApplication.update -= ResetWhite;
        }

        [InitializeOnLoadMethod]
        private static void InitializeOnLoad ()
        {
            EditorApplication.playModeStateChanged += (PlayModeStateChange state) => {
                if(state == PlayModeStateChange.EnteredEditMode)
                    EditorApplication.update += ResetWhite;
            };

            if(!EditorApplication.isPlaying)
                EditorApplication.update += ResetWhite;
        }
#endif

        private void Awake()
        {
            _instance = this;
        }

        public static void Initialize ()
        {
            Shader.SetGlobalTexture("_void_texture", _instance._renderTexture);
        }

        public static void Shutdown()
        {
            _instance = null;
            ResetWhite();
        }

        public static void RenderDefault ()
        {
            var color = _instance._camera.backgroundColor;
            _instance._camera.backgroundColor = Color.white;
            Render();
            _instance._camera.backgroundColor = color;
        }

        public static void Render()
        {
            _instance._camera.Render();
        }
    }
}
