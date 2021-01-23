using System.Collections.Generic;
using NoZ;
using UnityEngine;

namespace Puzzled
{
    class BeamTerminal : TileComponent
    {
        private List<Beam> _beams = new List<Beam>();
        private bool _powered = false;
        private bool _hadPoweredBeam = false;

        public int connectionCount => _beams.Count;

        public Beam GetConnection(int index) => _beams[index];

        public bool isPowered => _powered;

        public void Connect (Beam beam)
        {
            if (_beams.Contains(beam))
                return;

            _beams.Add(beam);

            UpdatePowered();

            Send(new BeamChangedEvent(beam));
        }

        public void Disconnect (Beam beam)
        {
            if (!_beams.Contains(beam))
                return;

            _beams.Remove(beam);

            UpdatePowered();

            Send(new BeamChangedEvent(beam));
        }

        [ActorEventHandler]
        private void OnDestroyEvent (DestroyEvent evt)
        {
            while (_beams.Count > 0)
                _beams[_beams.Count - 1].Disconnect();
        }

        private void UpdatePowered()
        {
            _powered = false;
            for(int i=0; i<_beams.Count && !_powered; i++)
                _powered = _beams[i].isPowered;

            if (!_powered && _beams.Count > 0 && !_hadPoweredBeam)
            {
                _powered = true;
                _hadPoweredBeam = false;
            } else
                _hadPoweredBeam = _powered;
        }
    }
}
