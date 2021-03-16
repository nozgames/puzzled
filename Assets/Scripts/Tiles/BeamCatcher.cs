using System;
using UnityEngine;
using NoZ;

namespace Puzzled
{
    public class BeamCatcher : TileComponent
    {
        [SerializeField] private BeamTerminal _terminal = null;
        [SerializeField] private GameObject _visualsOn = null;
        [SerializeField] private GameObject _visualsOff = null;

        [Editable]
        [Port(PortFlow.Output, PortType.Power)]
        private Port powerOutPort { get; set; }

        [ActorEventHandler]
        private void OnStart(StartEvent evt) => UpdateState();

        [ActorEventHandler]
        private void OnBeamChangedEvent(BeamChangedEvent evt) => UpdateState();

        private void UpdateState()
        {
            powerOutPort.SetPowered(_terminal.isPowered);
            _visualsOff.SetActive(!_terminal.isPowered);
            _visualsOn.SetActive(_terminal.isPowered);
        }
    }
}
