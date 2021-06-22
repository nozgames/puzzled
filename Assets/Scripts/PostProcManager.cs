using NoZ;
using System;
using System.Collections.Generic;
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

        private static PostProcManager _instance = null;

        public static BlackAndWhiteEffect blackAndWhite => _instance._blackAndWhite;
        public static SepiaEffect sepia => _instance._sepia;

        private void Awake()
        {
            if (null != _instance)
            {
                Debug.Log("Multiple instances of PostProcManager in scene");
                return;
            }

            _instance = this;

            _blackAndWhite = new BlackAndWhiteEffect();
            _sepia = new SepiaEffect();
        }

        public static void Initialize()
        {
            Debug.Assert(_instance != null);

            _instance._globalVolume.profile.TryGet<BlackAndWhiteEffect>(out _instance._blackAndWhite);
            _instance._globalVolume.profile.TryGet<SepiaEffect>(out _instance._sepia);
        }

        public static void Shutdown()
        {
        }

    }
}
