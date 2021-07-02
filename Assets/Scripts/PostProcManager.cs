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

        private static PostProcManager _instance = null;

        public static BlackAndWhiteEffect blackAndWhite => _instance._blackAndWhite;
        public static SepiaEffect sepia => _instance._sepia;
        public static OldFilmEffect oldFilm => _instance._oldFilm;

        public static bool disableAll { get; set; }

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
    }
}
