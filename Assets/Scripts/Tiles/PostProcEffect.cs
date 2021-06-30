using NoZ;
using UnityEngine;
using UnityEngine.Rendering;

namespace Puzzled
{
    public class PostProcEffect : TileComponent
    {
        [Editable(rangeMin = 0, rangeMax = 100)]
        public int strength { get; private set; } = 100;

        public float strengthFraction => strength / 100.0f;

        private int _transitionTime = 4;
        private float _blendScale = 0;
        private float _blendRate = 0;
        private bool _isUpdating = false;

        private float targetScale => ((_blendRate > 0) ? 1 : 0);
        private bool isDoneBlending => (_blendScale == targetScale);

        protected float blendScale => _blendScale;

        [Editable]
        public int transitionTime
        {
            get => _transitionTime;
            set => _transitionTime = Mathf.Max(value, 0);
        }

        [Editable]
        [Port(PortFlow.Input, PortType.Power)]
        public Port powerInPort { get; set; }

        [Editable]
        [Port(PortFlow.Output, PortType.Power)]
        private Port powerOutPort { get; set; }

        [ActorEventHandler]
        private void OnStart(StartEvent evt)
        {
            _blendScale = 0;

            if (!tile.puzzle.isPreview)
                UpdatePower();
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            _blendScale = 0;
            UpdatePostProc();
        }

        [ActorEventHandler]
        private void OnWirePowerChanged(WirePowerChangedEvent evt) => UpdatePower();

        [ActorEventHandler(autoRegister = false)]
        private void OnUpdate(ActorUpdateEvent evt)
        {
            _blendScale = Mathf.Clamp(_blendScale + _blendRate * Time.deltaTime, 0, 1);
            if (isDoneBlending)
            {
                UnregisterHandler<ActorUpdateEvent>();
                _isUpdating = false;
            }

            UpdatePostProc();
        }

        protected virtual void UpdatePostProc()
        {
        }

        private void UpdatePower()
        {
            if (powerOutPort.hasPower != powerInPort.hasPower)
            {
                bool hasPower = powerInPort.hasPower;

                if (transitionTime > 0)
                {
                    float remainingBlend = hasPower ? (1 - _blendScale) : _blendScale;
                    float totalTransitionTime = (remainingBlend * (transitionTime * GameManager.tick + GameManager.tickTimeRemaining));
                    _blendRate = (1 / totalTransitionTime);

                    if (!hasPower)
                        _blendRate = -_blendRate;
                }
                else
                {
                    // nothing to blend, 
                    _blendScale = hasPower ? 1 : 0;
                }


                powerOutPort.SetPowered(powerInPort.hasPower);
            }

            if (!isDoneBlending && !_isUpdating)
            {
                _isUpdating = true;
                RegisterHandler<ActorUpdateEvent>();
            }

            if (powerInPort.wireCount == 0)
                _blendScale = 1;

            UpdatePostProc();
        }
    }
}
