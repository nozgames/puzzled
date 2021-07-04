using UnityEngine;
using NoZ;

namespace Puzzled
{
    public class ColoredMaterial : TileComponent
    {
        [SerializeField] private Colored _colored = null;
        [SerializeField] private MeshRenderer _renderer = null;
        [SerializeField] private int _submesh = 0;

        [ActorEventHandler]
        private void OnAwakeEvent(AwakeEvent evt)
        {
            _colored.onColorChanged += (color) => UpdateColor();
        }

        [ActorEventHandler]
        private void OnStartEvent (StartEvent evt)
        {
            UpdateColor();
        }

        private void UpdateColor()
        {
            if (null == _renderer || null == _colored)
                return;

            if (_submesh < 0 || _submesh >= _renderer.materials.Length)
                return;

            _renderer.materials[_submesh].color = _colored.color;
        }
    }
}
