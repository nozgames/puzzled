using UnityEngine;
using NoZ;

namespace Puzzled
{
    public class Sign : TileComponent
    {
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

        private void UpdateVisuals()
        {

        }

        [ActorEventHandler]
        private void OnUse(UseEvent evt)
        {
            evt.IsHandled = true;
            var popup = UIManager.ShowPopup(_popupPrefab);
            popup.GetComponentInChildren<UIPopupText>().text = text;
        }
    }
}
