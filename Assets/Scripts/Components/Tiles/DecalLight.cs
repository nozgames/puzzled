using UnityEngine;
using NoZ;

namespace Puzzled
{
    public class DecalLight : TileComponent
    {
        [SerializeField] private Light _light = null;

        private DecalSurface _decalSurface;
        private Vector3 _defaultPosition;

        [Editable]
        [Port(PortFlow.Input, PortType.Power)]
        private Port powerInPort { get; set; }

        [Editable]
        [Port(PortFlow.Output, PortType.Power)]
        private Port powerOutPort { get; set; }

        [ActorEventHandler]
        private void OnAwakeEvent (AwakeEvent evt)
        {
            _defaultPosition = _light != null ? _light.transform.localPosition : Vector3.zero;

            // Move the light back to the static layer so it shows up
            if (_light != null)
                _light.gameObject.layer = CameraManager.TileLayerToObjectLayer(TileLayer.Static);
        }

        [ActorEventHandler]
        private void OnStartEvent (StartEvent evt)
        {
            AttachToDecal();
            UpdateState();
        }

        [ActorEventHandler]
        private void OnCellChangedEvent (CellChangedEvent evt)
        {
            AttachToDecal();
            UpdateState();
        }

        [ActorEventHandler]
        private void OnWirePowerChanged(WirePowerChangedEvent evt) => UpdateState();

        private void UpdateState()
        {
            var on = powerInPort.hasPower;
            if(_decalSurface != null)
            {
                if (_light != null)
                    _light.gameObject.SetActive(on);

                if (on)
                    _decalSurface.color = _decalSurface.lightColor;
                else
                    _decalSurface.ResetColor();
            }

            powerOutPort.SetPowered(on);
        }

        private void AttachToDecal ()
        {
            if(_decalSurface != null)
                _decalSurface.ResetColor();

            _decalSurface = DecalSurface.FromCell(puzzle, tile.cell);
            if(null != _decalSurface)
            {
                if (_light != null)
                    _light.transform.localPosition = _defaultPosition + _decalSurface.lightOffset;
            }
        }
    }
}
