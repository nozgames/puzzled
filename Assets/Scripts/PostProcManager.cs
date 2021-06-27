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
        private HandDrawnEffect _handDrawn;

        private static PostProcManager _instance = null;

        public static BlackAndWhiteEffect blackAndWhite => _instance._blackAndWhite;
        public static SepiaEffect sepia => _instance._sepia;
        public static HandDrawnEffect handDrawn => _instance._handDrawn;

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
            _handDrawn = ScriptableObject.CreateInstance<HandDrawnEffect>();
        }

        public static void Initialize()
        {
            Debug.Assert(_instance != null);

            _instance._globalVolume.profile.TryGet(out _instance._blackAndWhite);
            _instance._globalVolume.profile.TryGet(out _instance._sepia);
            _instance._globalVolume.profile.TryGet(out _instance._handDrawn);
        }

        public static void Shutdown()
        {
        }

    }
}
