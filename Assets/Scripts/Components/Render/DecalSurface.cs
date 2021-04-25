using UnityEngine;
using NoZ;

namespace Puzzled
{
    public class DecalSurface : TileComponent
    {
        [Header("Render")]
        [SerializeField] private Renderer _renderer = null;
        [SerializeField] private int _materialIndex = 0;
        [SerializeField] private Color _color = Color.white;
        [SerializeField] private float _smoothness = 0.5f;
        [SerializeField] private float _rotation = 0.5f;
        [SerializeField] private float _scale = 1.0f;

        [Header("Property")]
        [SerializeField] private string _propertyName = null;
        [SerializeField] private string _propertyDisplayName = null;

        private Decal _decal = Decal.none;
        private float _light = 0.0f;

        public string decalName => _propertyName;
        public string decalDisplayName => _propertyDisplayName;

        public bool hasDecal => _decal != Decal.none;

        [Editable(dynamicName = "decalName", dynamicDisplayName = "decalDisplayName")]
        public Decal decal {
            get => _decal;
            set {
                _decal = value;

                if (_decal.isAutoColor)
                {
                    _decal.color = _color;
                    _decal.smoothness = _smoothness;
                }

                if (_decal != Decal.none)
                {
                    if (_renderer is SpriteRenderer spriteRenderer)
                    {
                        _renderer.enabled = true;
                        spriteRenderer.sprite = _decal.sprite;
                        spriteRenderer.flipX = _decal.isFlipped;
                        spriteRenderer.transform.transform.localRotation = Quaternion.Euler(0, 0, _decal.rotation);
                        spriteRenderer.transform.transform.localScale = Vector3.one * _decal.scale * _scale;
                        spriteRenderer.color = _decal.color;
                    } else
                    {
                        var material = _renderer.materials[_materialIndex];
                        material.EnableKeyword("DECAL_ON");
                        material.SetTexture("_decal", _decal.texture);
                        material.SetColor("_decalColor", _decal.color);
                        material.SetFloat("_decalSmoothness", _decal.smoothness);
                        material.SetVector("_decalScale", new Vector2(decal.scale * (_decal.isFlipped ? -1 : 1), decal.scale * _scale));
                        material.SetFloat("_decalRotation", _rotation + -_decal.rotation);
                    }
                } else if (_renderer is SpriteRenderer spriteRenderer)
                    _renderer.enabled = false;
            }
        }

        public float decalLight {
            get => _light;
            set {
                _light = value;

                if (_renderer is SpriteRenderer spriteRenderer)
                    return;

                _renderer.materials[_materialIndex].SetFloat("_decalLight", _light);
            }
        }

        /// <summary>
        /// Return all decal surfaces for the tile at the given cell and layer
        /// </summary>
        /// <param name="puzzle">Puzzle to search in</param>
        /// <param name="cell">Cell to search in</param>
        /// <param name="layer">Layer to search in</param>
        /// <returns>Array of decal surfaces or null if there are none</returns>
        public static DecalSurface[] FromCell(Puzzle puzzle, Cell cell, TileLayer layer)
        {
            var tile = puzzle.grid.CellToTile(cell, layer);
            if (null == tile)
                return null;

            return FromTile(tile);            
        }

        /// <summary>
        /// Return all available decal surfaces for the top most tile in the given cell
        /// </summary>
        /// <param name="puzzle">Puzzle to retrieve surfaces from</param>
        /// <param name="cell">Cell to retrieve surfaces from</param>
        /// <returns>Array of surfaces or null if the top most tile does not have any</returns>
        public static DecalSurface[] FromCell (Puzzle puzzle, Cell cell)
        {
            // Floor or static
            if(cell.edge == CellEdge.None)
            {
                var result = FromCell(puzzle, cell, TileLayer.Dynamic);
                if (null != result)
                    return result;

                result = FromCell(puzzle, cell, TileLayer.Static);
                if (null != result)
                    return result;

                return FromCell(puzzle, cell, TileLayer.Floor);
            }
            // Wall static or wall
            else
            {
                var result = FromCell(puzzle, cell, TileLayer.WallStatic);
                if (null != result)
                    return result;

                return FromCell(puzzle, cell, TileLayer.Wall);
            }
        }

        /// <summary>
        /// Returns all decal surfaces for the given tile
        /// </summary>
        /// <param name="tile">Tile to return decal surfaces for</param>
        /// <returns>Array of decal surfaces or null if the tile has none</returns>
        public static DecalSurface[] FromTile(Tile tile)
        {
            var surfaces = tile.GetComponentsInChildren<DecalSurface>();
            if (null == surfaces || surfaces.Length == 0)
                return null;

            return surfaces;
        }

        /// <summary>
        /// Return the top most tile in the given cell that has decal surfaces
        /// </summary>
        /// <param name="puzzle">Puzzle to search in</param>
        /// <param name="cell">Cell to search in</param>
        /// <returns>Top most tile with decals or null if none found</returns>
        public static Tile GetTopMostTileWithDecals (Puzzle puzzle, Cell cell)
        {
            for (int layer = (int)TileLayer.Logic; layer >= (int)TileLayer.Floor; layer--)
            {
                var tile = puzzle.grid.CellToTile(cell, (TileLayer)layer);
                if (tile == null)
                    continue;

                if (tile.HasTileComponent<DecalSurface>())
                    return tile;
            }

            return null;
        }
    }
}
