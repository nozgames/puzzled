using System.Linq;
using UnityEngine;
using NoZ;

namespace Puzzled
{
    public class DecalLight : TileComponent
    {
        [Header("Render")]
        [SerializeField] private DecalSurface[] _surfaces = null;

        public bool hasDecal => _surfaces != null && _surfaces.Any(surfaceHasDecal);

        /// <summary>
        /// Return the number of non-empty decals that the decal light represents
        /// </summary>
        public int decalCount => _surfaces.Count(surfaceHasDecal);

        [Editable(hiddenIfFalse = "hasDecal")]
        [Port(PortFlow.Input, PortType.Power, PortFlags.AllowSelfWire)]
        public Port decalPowerPort { get; private set; }

        private bool surfaceHasDecal(DecalSurface surface) => surface.hasDecal;
    }
}
