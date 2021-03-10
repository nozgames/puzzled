using NoZ;
using UnityEngine;

namespace Puzzled
{
    [RequireComponent(typeof(Usable))]
    abstract class UsableSpinner : Spinner
    {
        [Header("Visuals")]
        [SerializeField] private Transform _rotator = null;
        [SerializeField] private AudioClip _useSound = null;

        protected abstract int stepCount { get; }

        private int _rotateIndex = 0;
        protected int rotateIndex => _rotateIndex;
        private float step => 360.0f / stepCount;

        protected abstract Vector3 initialEulerAngles { get; }
        protected abstract Vector3 rotationAxis { get; }

        private Vector3 currentEulerAngles => initialEulerAngles + rotationAxis * _rotateIndex * step;
        private Vector3 previousEulerAngles => initialEulerAngles + rotationAxis * (_rotateIndex - 1) * step;

        protected override void OnStart(StartEvent evt)
        {
            _rotator.localEulerAngles = currentEulerAngles;

            base.OnStart(evt);
        }

        [ActorEventHandler]
        private void OnUse(UseEvent evt)
        {
            evt.IsHandled = true;

            var oldIndex = _rotateIndex;
            _rotateIndex = (_rotateIndex + 1) % stepCount;

            PlaySound(_useSound, 1, 1.4f);

            GameManager.busy++;
            Tween.Rotate(previousEulerAngles, currentEulerAngles)
                .Duration(0.25f)
                .OnStop(OnUseComplete)
                .Start(_rotator.gameObject);
        }

        protected virtual void OnUseComplete()
        {
            value++;
            GameManager.busy--;
        }

        [ActorEventHandler]
        private void OnIncrement(IncrementSignal evt) => value++;
    }
}
