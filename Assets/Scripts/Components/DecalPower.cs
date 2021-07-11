using System.Linq;
using UnityEngine;
using NoZ;

namespace Puzzled
{
    public class DecalPower : TileComponent
    {
        private DecalSurface[] _surfaces;

        [Editable(hiddenIfFalse = "hasDecal")]
        [Port(PortFlow.Input, PortType.Power, PortFlags.AllowSelfWire, customIcon = "decalPowerPortIcon")]
        public Port decalPowerPort { get; private set; }

        /// <summary>
        /// Custom icon for decal power
        /// </summary>
        public Sprite decalPowerPortIcon => Editor.UIPuzzleEditor.instance.spriteDecalPower;

        [Editable(hidden = true, serialized = false)]
        private bool hasDecal => _surfaces.Any(s => s.decal != Decal.none);

        private void Awake()
        {
            _surfaces = GetComponentsInChildren<DecalSurface>();
        }

        [ActorEventHandler]
        private void OnStartEvent(StartEvent evt)
        {
            UpdateDecalPower();
        }

        [ActorEventHandler]
        private void OnWirePowerChangedEvent(WirePowerChangedEvent evt)
        {
            UpdateDecalPower();
        }

        private void UpdateDecalPower()
        {
            var light = decalPowerPort.hasPower ? 1.0f : 0.0f;
            foreach (var surface in _surfaces)
                surface.decalLight = light;
        }
    }
}
