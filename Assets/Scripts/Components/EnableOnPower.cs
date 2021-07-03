using UnityEngine;
using NoZ;

namespace Puzzled
{
    public class EnableOnPower : TileComponent
    {
        [SerializeField] private Powerable _powerable = null;
        [SerializeField] private GameObject[] _targets = null;
        [SerializeField] private bool _invert = false;

        [ActorEventHandler]
        private void OnStartEvent (StartEvent evt)
        {
            UpdatePower();
        }

        [ActorEventHandler]
        private void OnWirePowerChangedEvent (WirePowerChangedEvent evt)
        {
            UpdatePower();
        }

        private void UpdatePower()
        {
            if (_targets == null)
                return;

            var hasPower = _invert ? !_powerable.hasPower : _powerable.hasPower;
            foreach(var target in _targets)
            {
                target.SetActive(hasPower);
            }
        }
    }
}
