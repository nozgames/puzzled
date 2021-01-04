using UnityEngine;

namespace Puzzled.Editor
{
    class UIChoosePuzzlePopupItem : UIListItem
    {
        [SerializeField] private TMPro.TextMeshProUGUI _text = null;

        public string data { get; set; }

        public string text {
            get => _text.text;
            set => _text.text = value;
        }
    }
}
