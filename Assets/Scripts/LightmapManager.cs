using UnityEngine;

namespace Puzzled
{
    class LightmapManager : MonoBehaviour
    {
        private static LightmapManager _instance = null;

        [SerializeField] private Camera _camera = null;
        [SerializeField] private RenderTexture _renderTexture = null;

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
        }

        public void LateUpdate()
        {
            //if(GameManager.isPlaying)
            {
                Render();
            }
        }

        public static void Render()
        {
            _instance._camera.Render();
        }
    }
}
