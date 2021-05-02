using UnityEngine;

namespace Puzzled.UI
{
    public class UIPopupText : UIPopup
    {
        [SerializeField] private TMPro.TextMeshProUGUI _text = null;
        [SerializeField] private Editor.UIDecalPreview _decalImage = null;

        private string[] _pages;
        private int pageIndex = 0;

        public Decal decal {
            get => _decalImage.decal;
            set {
                _decalImage.decal = value;
                _decalImage.gameObject.SetActive(_decalImage.decal != Decal.none);                
            }
        }

        public string[] pages
        { 
            get => _pages;
            set
            {
                pageIndex = 0;
                _pages = value;
                if (_pages == null)
                    return;

                _text.text = _pages[pageIndex];
            }
        }

        public override void Use()
        {
            if (pages == null)
            {
                Close();
                return;
            }

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
