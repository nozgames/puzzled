using UnityEngine;

namespace Puzzled
{
    public class DecalSurface : TileComponent
    {
        [SerializeField] private SpriteRenderer _renderer = null;

        private Decal _decal;

        [Editable]
        public Decal decal {
            get => _decal;
            set {
                _decal = value;

                _renderer.enabled = _decal != null && _decal.sprite != null;
                _renderer.sprite = _decal.sprite;
            }
        }
    }
}
