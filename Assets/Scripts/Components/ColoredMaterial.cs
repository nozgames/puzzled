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
        private void OnStartEvent (StartEvent evt)
        {
            if (null == _renderer || null == _colored)
                return;

            if(_submesh < 0 || _submesh >= _renderer.sharedMaterials.Length)
            {
                Debug.LogError("Invalid submesh on ColoredMaterial");
                return;
            }

            _renderer.sharedMaterials[_submesh].color = _colored.color;
        }
    }
}
