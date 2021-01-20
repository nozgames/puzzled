using UnityEngine;
using NoZ;

namespace Puzzled
{
    public class Sign : TileComponent
    {
        public bool isUsable = true;

        [SerializeField] private UIPopup _popupPrefab = null;

        private string _text = "";

        [Editable(multiline = true)]
        public string text {
            get => _text;
            set {
                _text = value;
                UpdateVisuals();
            }
        }
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

        private void UpdateVisuals()
        {

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
            var popup = UIManager.ShowPopup(_popupPrefab, DoneCallback);
            popup.GetComponentInChildren<UIPopupText>().text = text;
        }

        private void DoneCallback()
        {
            donePort.SendSignal();
        }
    }
}
