using UnityEngine;
using NoZ;

namespace Puzzled
{
    public class Sign : TileComponent
    {
        public bool isUsable = true;

        [SerializeField] private UIPopup _popupPrefab = null;

        private string _text = "";

        /// <summary>
        /// Import use port to activate sign
        /// </summary>
        [Editable]
        [Port(PortFlow.Input, PortType.Signal, legacy = true, signalEvent = typeof(UseSignal))]
        private Port usePort { get; set; }

        [Editable(multiline = true)]
        public string text {
            get => _text;
            set {
                _text = value;
                UpdateVisuals();
            }
        }

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
            var popup = UIManager.ShowPopup(_popupPrefab);
            popup.GetComponentInChildren<UIPopupText>().text = text;
        }
    }
}
