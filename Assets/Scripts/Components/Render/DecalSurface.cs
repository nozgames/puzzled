using UnityEngine;
using NoZ;

namespace Puzzled
{
    public class DecalSurface : TileComponent
    {
        [SerializeField] private SpriteRenderer _renderer = null;
        [SerializeField] private Color _lightColor = Color.white;
        [SerializeField] private Vector3 _lightOffset;

        private Decal _decal;
        private Color _defaultColor;

        public Color color {
            get => _renderer.color;
            set => _renderer.color = value;
        }

        public Color lightColor => _lightColor;
        public Vector3 lightOffset => _lightOffset;

        [ActorEventHandler]
        private void OnAwakeEvent(AwakeEvent evt)
        {
            _defaultColor = _renderer.color;
        }

        [Editable]
        public Decal decal {
            get => _decal;
            set {
                _decal = value;

                _renderer.enabled = _decal != null && _decal.sprite != null;

                if (_decal != null)
                {
                    _renderer.sprite = _decal.sprite;
                    _renderer.flipX = (_decal.flags & DecalFlags.FlipHorizontal) == DecalFlags.FlipHorizontal;
                    _renderer.flipY = (_decal.flags & DecalFlags.FlipVertical) == DecalFlags.FlipVertical;
                    _renderer.transform.transform.localRotation = Quaternion.Euler(0, 0, ((_decal.flags & DecalFlags.Rotate) == DecalFlags.Rotate) ? -90 : 0);
                }
            }
        }

        public void ResetColor() => color = _defaultColor;

        public static DecalSurface FromCell(Puzzle puzzle, Cell cell, TileLayer layer)
        {
            var tile = puzzle.grid.CellToTile(cell, layer);
            if (null == tile)
                return null;

            return tile.GetComponentInChildren<DecalSurface>();
        }

        public static DecalSurface FromCell (Puzzle puzzle, Cell cell)
        {
            var result = FromCell(puzzle, cell, TileLayer.Static);
            if (null != result)
                return result;

            return FromCell(puzzle, cell, TileLayer.Floor);
        }
    }
}
