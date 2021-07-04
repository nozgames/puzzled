using UnityEngine;
using NoZ;

namespace Puzzled
{
    public class LightFlicker : MonoBehaviour
    {
        [SerializeField] private Light _light = null;
        [SerializeField] private float _intensityMin = 0.5f;
        [SerializeField] private float _intensityMax = 1.0f;
        [SerializeField] private float _speed = 1.0f;
        [SerializeField] private Vector2 _perlinScale = Vector2.zero;

        public void OnEnable()
        {
            Tween.Custom(LerpLightIntensity, new Vector4(_intensityMin, _intensityMax, 0, 0), Vector4.zero)
                .Key("pulse")
                .Loop()
                .Duration(1.0f)
                .Start(gameObject);
        }

        public void OnDisable()
        {
            Tween.Stop(gameObject);
        }

        private bool LerpLightIntensity(Tween tween, float t)
        {
            _light.intensity = Mathf.PerlinNoise(Time.time * _perlinScale.x, Time.time * _perlinScale.y) * (_intensityMax - _intensityMin) + _intensityMin;
            return true;
        }
    }
}
