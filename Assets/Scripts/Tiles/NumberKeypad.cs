using NoZ;
using UnityEngine;
using Puzzled.UI;

namespace Puzzled
{
    [RequireComponent(typeof(Usable))]
    class NumberKeypad : TileComponent
    {
        [Header("Visuals")]
        //[SerializeField] private GameObject _visualLocked = null;
        //[SerializeField] private GameObject _visualUnlocked = null;
        [SerializeField] private UIKeypadPopup _popupPrefab = null;

        [SerializeField] private Texture[] _numbers = null;

        private bool _locked = true;

        private static Decal[] _buttons = null;
        private Decal[] _solution = null;

        /// <summary>
        /// Powered when keypad is unlocked
        /// </summary>
        [Editable]
        [Port(PortFlow.Output, PortType.Power)]
        private Port powerOutPort { get; set; }

        /// <summary>
        /// Import use port to activate sign
        /// </summary>
        [Editable]
        [Port(PortFlow.Input, PortType.Signal, legacy = true, signalEvent = typeof(UseSignal))]
        private Port usePort { get; set; }

        [Editable]
        private int value { get; set; } = 1111;

        [ActorEventHandler]
        private void OnUseSignal(UseSignal evt) => HandleUse();

        [ActorEventHandler]
        private void OnUse(UseEvent evt)
        {
            evt.IsHandled = true;

            if (!_locked)
                return;

            HandleUse();
        }

        private void HandleUse()
        {
            if(null == _buttons)
            {
                _buttons = new Decal[_numbers.Length];
                for (int i = 0; i < _numbers.Length; i++)
                    _buttons[i] = new Decal(System.Guid.NewGuid(), _numbers[i]);
            }

            if(null == _solution)
            {
                var v = value.ToString();
                _solution = new Decal[v.Length];
                for (int i = 0; i < v.Length; i++)
                    _solution[i] = _buttons[((v[i] - '0') + 9) % 10];
            }
            
            var keypad = UIManager.ShowPopup(_popupPrefab).GetComponent<UIKeypadPopup>();
            keypad.Open(_buttons, _solution, 3, () => {
                _locked = false;
                powerOutPort.SetPowered(true);
            });
        }
    }
}
