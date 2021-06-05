using UnityEngine;
using UnityEngine.VFX;

namespace Puzzled
{
    public enum BeamDirection
    {
        North,
        NorthEast,
        East,
        SouthEast,
        South,
        SouthWest,
        West,
        NorthWest
    }

    public class Beam : MonoBehaviour
    {
        private static readonly Vector2Int[] _dirs = {
            new Vector2Int(0,1),   // North
            new Vector2Int(1,1),   // NorthEast
            new Vector2Int(1,0),   // East
            new Vector2Int(1,-1),  // SouthEast
            new Vector2Int(0,-1),  // South
            new Vector2Int(-1,-1), // SouthWest
            new Vector2Int(-1,0),  // West
            new Vector2Int(-1,1)   // NorthWest
        };

        [SerializeField] private LineRenderer _line = null;
        [SerializeField] private VisualEffect _impactFX = null;
        [SerializeField] private float _maxLength = 100;
        [SerializeField] private LayerMask _collisionMask = (LayerMask)0x7FFFFFFF;

        private BeamDirection _direction;
        private BeamTerminal _terminal = null;
        private BeamEmitter _emitter = null;

        public BeamDirection direction {
            get => _direction;
            set {
                _direction = value;
                //Update();
            }
        }

        public BeamEmitter emitter {
            get => _emitter;
            set => _emitter = value;
        }

        public static Quaternion GetRotation (BeamDirection dir) =>
            Quaternion.LookRotation(_dirs[(int)dir].ToVector3Int().ToVector3().XYToXZ().normalized, Vector3.up);

        private void OnDisable() => Disconnect();

        public void Disconnect()
        {
            if (_terminal == null)
                return;

            var terminal = _terminal;
            _terminal = null;
            terminal.Disconnect(this);
        }

        private void Connect (BeamTerminal terminal)
        {
            if (null == terminal)
            {
                Disconnect();
                return;
            }

            if (_terminal == terminal)
                return;

            if (_terminal != null)
                Disconnect();

            // Only one beam wins for any given direction on a terminal
            if (terminal.GetBeam(direction) != null)
                return;

            _terminal = terminal;
            _terminal.Connect(this);
        }

        public void Update()         
        {
            var dir = _dirs[(int)_direction].ToVector3Int().ToVector3().XYToXZ().normalized;
            var length = _maxLength;

            // Ray-cast the beam to see what it is currently hitting
            if (Physics.Raycast(transform.position, dir, out var hit, _maxLength, _collisionMask))
            {
                length = hit.distance;

                Connect(hit.collider.GetComponentInParent<BeamTerminal>());

                if (_impactFX != null)
                {
                    _impactFX.transform.position = hit.point;
                    _impactFX.transform.forward = hit.normal;
                    _impactFX.gameObject.SetActive(true);
                }
            }
            else 
            {
                Disconnect();

                _impactFX.gameObject.SetActive(false);
            }

            if (length == 0)
                return;

            _line.gameObject.SetActive(true);
            _line.positionCount = 2;
            _line.SetPosition(0, transform.position);
            _line.SetPosition(1, transform.position + dir * length);
        }
    }
}

