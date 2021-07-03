using UnityEngine;
using NoZ;

namespace Puzzled
{
    class TileIcon : TileComponent
    {
        [SerializeField] private SpriteRenderer _target = null;

        [ActorEventHandler]
        private void OnAwakeEvent (AwakeEvent evt)
        {
            if (null == _target)
                _target = GetComponentInChildren<SpriteRenderer>();

            if (null == _target)
                return;

            var icon = tile.icon;
            if (icon == null)
                return;

            _target.sprite = icon;
        }
    }
}
