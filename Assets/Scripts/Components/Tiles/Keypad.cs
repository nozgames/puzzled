﻿using NoZ;
using UnityEngine;

namespace Puzzled
{
    class Keypad : TileComponent
    {
        [Header("Visuals")]
        [SerializeField] private GameObject _visualLocked;
        [SerializeField] private GameObject _visualUnlocked;
        [SerializeField] private UIKeypadPopup _popupPrefab = null;

        private bool _locked = true;

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
        private int columnCount { get; set; } = 3;

        [Editable]
        private Decal[] solution { get; set; }

        [Editable]
        private Decal[] buttons { get; set; }

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
            var keypad = UIManager.ShowPopup(_popupPrefab).GetComponent<UIKeypadPopup>(); ;
            keypad.Open(buttons, solution, columnCount, () => {
                _locked = false;
                powerOutPort.SetPowered(true);
            });
        }
    }
}
