using UnityEngine;
using NoZ;

namespace Puzzled
{
    public class LightPulse : MonoBehaviour
    {
        [SerializeField] private Light _light = null;
        [SerializeField] private float _intensityMin = 0.5f;
        [SerializeField] private float _intensityMax = 1.0f;
        [SerializeField] private float _speed = 1.0f;

        public void OnEnable()
        {
            Tween.Custom(LerpLightIntensity, new Vector4(_intensityMin, _intensityMax, 0, 0), Vector4.zero)
                .EaseInOutCubic()
                .PingPong()
                .Loop()
                .Duration(1.0f / Mathf.Max(0.01f, _speed))
                .Start(gameObject);
        }

        public void OnDisable()
        {
            Tween.Stop(gameObject);
        }

        private bool LerpLightIntensity (Tween tween, float t)
        {
            _light.intensity = Mathf.Lerp(tween.Param1.x, tween.Param1.y, t);
            return true;
        }
    }
}
