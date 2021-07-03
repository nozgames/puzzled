using UnityEngine;
using NoZ;

namespace Puzzled
{
    public class ColoredLight : TileComponent
    {
        [SerializeField] private Colored _colored = null;
        [SerializeField] private Light _light = null;

        [ActorEventHandler]
        private void OnAwakeEvent(AwakeEvent evt)
        {
            _colored.onColorChanged += (color) => UpdateColor();
        }

        [ActorEventHandler]
        private void OnStartEvent(StartEvent evt)
        {
            UpdateColor();
        }

        private void UpdateColor()
        {
            if (null == _light || null == _colored)
                return;

            _light.color = _colored.color;
        }
    }
}
