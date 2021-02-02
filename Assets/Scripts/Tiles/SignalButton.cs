using NoZ;
using UnityEngine;

namespace Puzzled
{
    class SignalButton : UsableTileComponent
    {
        [SerializeField] private Animator _animator = null;
        [SerializeField] private AudioClip _useSound = null;

        [Editable]
        [Port(PortFlow.Output, PortType.Signal, legacy = true)]
        private Port signalOutPort { get; set; }

        [ActorEventHandler]
        private void OnUse(UseEvent evt)
        {
            evt.IsHandled = true;

            if (!isUsable)
                return;

            signalOutPort.SendSignal();

            _animator.SetTrigger("Use");

            PlaySound(_useSound);
        }
    }
}
