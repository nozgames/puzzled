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

                if (_decal != null)
                {
                    _renderer.sprite = _decal.sprite;
                    _renderer.flipX = (_decal.flags & DecalFlags.FlipHorizontal) == DecalFlags.FlipHorizontal;
                    _renderer.flipY = (_decal.flags & DecalFlags.FlipVertical) == DecalFlags.FlipVertical;
                    _renderer.transform.transform.localRotation = Quaternion.Euler(0, 0, ((_decal.flags & DecalFlags.Rotate) == DecalFlags.Rotate) ? -90 : 0);
                }
            }
        }

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
