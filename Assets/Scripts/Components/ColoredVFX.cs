using UnityEngine;
using NoZ;

namespace Puzzled
{
    public class ColoredVFX : TileComponent
    {
        [SerializeField] private Colored _colored = null;
        [SerializeField] private UnityEngine.VFX.VisualEffect _vfx = null;
        [SerializeField] private string _property = "Color";

        private int _propertyId = 0;

        [ActorEventHandler]
        private void OnAwakeEvent(AwakeEvent evt)
        {
            _propertyId = Shader.PropertyToID(_property);

            _colored.onColorChanged += (color) => UpdateColor();
        }

        [ActorEventHandler]
        private void OnStartEvent(StartEvent evt)
        {
            if (null == _vfx || null == _colored)
                return;

            UpdateColor();
        }

        private void UpdateColor()
        {
            _vfx.SetVector4(_propertyId, _colored.color);
        }
    }
}
