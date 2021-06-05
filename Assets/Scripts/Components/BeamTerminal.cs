using System.Collections.Generic;

namespace Puzzled
{
    class BeamTerminal : TileComponent
    {
        private List<Beam> _beams = new List<Beam>();

        /// <summary>
        /// Returns the number of connected beams
        /// </summary>
        public int beamCount => _beams.Count;

        /// <summary>
        /// Returns a beam connection by index
        /// </summary>
        /// <param name="index">Beam connection index</param>
        /// <returns>Beam at the given index</returns>
        public Beam GetBeam(int index) => _beams[index];

        /// <summary>
        /// Get the beam coming from the given direction
        /// </summary>
        /// <param name="direction"></param>
        /// <returns></returns>
        public Beam GetBeam(BeamDirection direction)
        {
            foreach (var beam in _beams)
                if (beam.direction == direction)
                    return beam;

            return null;
        }

        /// <summary>
        /// Returns true if the terminal has at least one connected beams
        /// </summary>
        public bool hasBeams => _beams.Count > 0;

        /// <summary>
        /// Connect a new beam to the terminal
        /// </summary>
        /// <param name="beam">Beam to connect</param>
        public void Connect (Beam beam)
        {
            if (_beams.Contains(beam))
                return;

            _beams.Add(beam);

            Send(new BeamChangedEvent(beam, true));
        }

        /// <summary>
        /// Disconnect a beam from the terminal
        /// </summary>
        /// <param name="beam">Beam to disconnect</param>
        public void Disconnect (Beam beam)
        {
            if (!_beams.Contains(beam))
                return;

            _beams.Remove(beam);

            Send(new BeamChangedEvent(beam, false));
        }
    }
}
