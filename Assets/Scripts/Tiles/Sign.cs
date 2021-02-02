using UnityEngine;
using NoZ;
using System;

namespace Puzzled
{
    public class Sign : TileComponent
    {
        public bool isUsable = true;

        [SerializeField] private UIPopup _popupPrefab = null;

        [Editable(multiline = true, hidden = true)]
        public string text { get; set; }

        [Editable(multiline = true)]
        public string[] pages { get; set; }

        /// <summary>
        /// Import use port to activate sign
        /// </summary>
        [Editable]
        [Port(PortFlow.Input, PortType.Signal, legacy = true, signalEvent = typeof(UseSignal))]
        private Port usePort { get; set; }

        /// <summary>
        /// Import signal output port triggered when the sign is closed
        /// </summary>
        [Editable]
        [Port(PortFlow.Output, PortType.Signal, signalEvent = typeof(DoneSignal))]
        private Port donePort { get; set; }

        [ActorEventHandler]
        private void OnStartEvent (StartEvent evt)
        {
            // Automatically convert old signs to use paging system
            if(!string.IsNullOrWhiteSpace(text) && (pages == null || pages.Length == 0))
            {
                pages = new string[] { text };
                text = "";
            }
        }

        [ActorEventHandler]
        private void OnUseSignal(UseSignal evt) => HandleUse();

        [ActorEventHandler]
        private void OnUse(UseEvent evt)
        {
            if (!isUsable)
                return;

            evt.IsHandled = true;
            HandleUse();
        }

        private void HandleUse()
        {
            if (pages == null || pages.Length == 0)
                return;

            var popup = UIManager.ShowPopup(_popupPrefab, DoneCallback);
            popup.GetComponent<UIPopupText>().pages = pages;
        }

        private void DoneCallback()
        {
            donePort.SendSignal();
        }
    }
}
