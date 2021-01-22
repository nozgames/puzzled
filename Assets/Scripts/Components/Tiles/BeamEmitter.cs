using UnityEngine;
using NoZ;

namespace Puzzled
{
    class BeamEmitter : UsableTileComponent
    {
        [SerializeField] private GameObject _visualsOn = null;
        [SerializeField] private GameObject _visualsOff = null;
        [SerializeField] private AudioClip _useSound = null;
        [SerializeField] private Beam _beam = null;

        private int _value = 0;

        private static readonly Cell[] _dirs = { 
            Cell.north, 
            Cell.northEast,
            Cell.east, 
            Cell.southEast,
            Cell.south, 
            Cell.southWest,
            Cell.west,
            Cell.northWest
        };

        [Editable]
        private int value {
            get => _value;
            set {
                _value = (value % _dirs.Length);
                UpdateBeam();
            }
        }

        [ActorEventHandler]
        private void OnStart(StartEvent evt)
        {
            UpdateBeam();
        }

        [ActorEventHandler]
        private void OnValueEvent(ValueEvent evt)
        {
            value = evt.value - 1;
            UpdateBeam();
        }

        [ActorEventHandler]
        private void OnUseEvent(UseEvent evt)
        {
            if (!isUsable)
                return;

            evt.IsHandled = true;
            value++;
            PlaySound(_useSound);
        }

        private void UpdateBeam()
        {
            if(!isUsable)
            {
                _beam.gameObject.SetActive(false);
                return;
            }

            _beam.gameObject.SetActive(true);
            _beam.direction = _dirs[value];
            _beam.length = 10;
        }

        protected override void OnUsableChanged()
        {
            base.OnUsableChanged();
            _visualsOff.gameObject.SetActive(!isUsable);
            _visualsOn.gameObject.SetActive(isUsable);
            UpdateBeam();
        }
    }
}

