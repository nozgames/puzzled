using System.Linq;
using UnityEngine;
using NoZ;

namespace Puzzled
{
    public class DecalPower : TileComponent
    {
        private DecalSurface[] _surfaces;

        [Editable(hiddenIfFalse = "hasDecal")]
        [Port(PortFlow.Input, PortType.Power, PortFlags.AllowSelfWire)]
        public Port decalPowerPort { get; private set; }

        [Editable(hidden = true, serialized = false)]
        private bool hasDecal => _surfaces.Any(s => s.decal != Decal.none);

        private void Awake()
        {
            _surfaces = GetComponentsInChildren<DecalSurface>();
        }
    }
}
