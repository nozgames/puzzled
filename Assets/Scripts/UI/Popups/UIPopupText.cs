using UnityEngine;

namespace Puzzled
{
    public class UIPopupText : UIPopup
    {
        [SerializeField] private TMPro.TextMeshProUGUI _text = null;

        private string[] _pages;
        private int pageIndex = 0;

        public string[] pages
        { 
            get => _pages;
            set
            {
                pageIndex = 0;
                _pages = value;
                _text.text = _pages[pageIndex];
            }
        }

        public override void Use()
        {
            ++pageIndex;
            if (pageIndex >= pages.Length)
            {
                Close();
                return;
            }

            _text.text = _pages[pageIndex];
        }
    }
}
