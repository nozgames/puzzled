using UnityEngine;
using UnityEngine.Rendering;

namespace Puzzled
{
    /// <summary>
    /// Manages the postprocs
    /// </summary>
    public class PostProcManager : MonoBehaviour
    {
        [SerializeField] private Volume _globalVolume;

        private BlackAndWhiteEffect _blackAndWhite;
        private SepiaEffect _sepia;
        private OldFilmEffect _oldFilm;
        private bool _disableAll;

        private static PostProcManager _instance = null;

        public static BlackAndWhiteEffect blackAndWhite => _instance._blackAndWhite;
        public static SepiaEffect sepia => _instance._sepia;
        public static OldFilmEffect oldFilm => _instance._oldFilm;

        public static bool disableAll 
        {
            get => _instance._disableAll;
            set {
                _instance._disableAll = value;
                UpdateActive(blackAndWhite);
                UpdateActive(sepia);
                UpdateActive(oldFilm);
            }
        }

        private void Awake()
        {
            if (null != _instance)
            {
                Debug.Log("Multiple instances of PostProcManager in scene");
                return;
            }

            _instance = this;

            _blackAndWhite = ScriptableObject.CreateInstance<BlackAndWhiteEffect>();
            _sepia = ScriptableObject.CreateInstance<SepiaEffect>();
            _oldFilm = ScriptableObject.CreateInstance<OldFilmEffect>();

            UpdateActive(blackAndWhite);
            UpdateActive(sepia);
            UpdateActive(oldFilm);
        }

        public static void Initialize()
        {
            Debug.Assert(_instance != null);

            _instance._globalVolume.profile.TryGet(out _instance._blackAndWhite);
            _instance._globalVolume.profile.TryGet(out _instance._sepia);
            _instance._globalVolume.profile.TryGet(out _instance._oldFilm);
        }

        public static void Shutdown()
        {
        }

        public static void SetBlend (BlendableVolumeComponent component, float blend)
        {
            component.blend.value = blend;
            UpdateActive(component);
        }

        private static void UpdateActive(BlendableVolumeComponent component)
        {
            component.active = component.blend.value > 0.0f && !disableAll;
        }
    }
}
