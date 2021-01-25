using UnityEngine;
using NoZ;

namespace Puzzled
{
    public class DecalSurface : TileComponent
    {
        [SerializeField] private SpriteRenderer _renderer = null;
        [SerializeField] private Color _lightColor = Color.white;
        [SerializeField] private Light _light = null;

        [Editable(hiddenIfFalse = "decal")]
        [Port(PortFlow.Input, PortType.Power)]
        public Port decalPowerPort { get; private set; }

        private Decal _decal;
        private Color _defaultColor;

        public Color color {
            get => _renderer.color;
            set => _renderer.color = value;
        }

        public Color lightColor => _lightColor;

        [ActorEventHandler]
        private void OnAwakeEvent(AwakeEvent evt)
        {
            _defaultColor = _renderer.color;
        }

        [ActorEventHandler]
        private void OnStartEvent (StartEvent evt)
        {
            UpdateDecalPower();
        }

        [ActorEventHandler]
        private void OnWirePowerChangedEvent (WirePowerChangedEvent evt)
        {
            UpdateDecalPower();
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

        private void UpdateDecalPower()
        {
            if (_light != null)
                _light.gameObject.SetActive(decalPowerPort.hasPower);

            _renderer.color = decalPowerPort.hasPower ? _lightColor : _defaultColor;
        }

        public void ResetColor() => color = _defaultColor;

        public static DecalSurface FromCell(Puzzle puzzle, Cell cell, TileLayer layer)
        {
            var tile = puzzle.grid.CellToTile(cell, layer);
            if (null == tile)
                return null;

            return FromTile(tile);            
        }

        public static DecalSurface FromCell (Puzzle puzzle, Cell cell)
        {
            var result = FromCell(puzzle, cell, TileLayer.Static);
            if (null != result)
                return result;

            return FromCell(puzzle, cell, TileLayer.Floor);
        }

        public static DecalSurface FromTile (Tile tile) => tile.GetComponentInChildren<DecalSurface>();
    }
}
