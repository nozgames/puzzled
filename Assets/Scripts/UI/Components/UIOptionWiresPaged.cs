using UnityEngine;

namespace Puzzled
{
    public class UIOptionWiresPaged : UIOptionWires
    {
        [SerializeField] private TMPro.TextMeshProUGUI _pageText = null;

        private int _currentPage = 0;
        private int _pageCount = 0;

        public void PreviousPage ()
        {
            _currentPage = Mathf.Clamp(_currentPage - 1, 0, _pageCount - 1);            
            _pageText.text = (_currentPage + 1).ToString();
            stateBit = _currentPage;
            OnTargetChanged(target);
        }

        public void NextPage()
        {
            _currentPage = Mathf.Clamp(_currentPage + 1, 0, _pageCount - 1);
            _pageText.text = (_currentPage + 1).ToString();
            stateBit = _currentPage;
            OnTargetChanged(target);
        }

        public void DeletePage()
        {
            var tile = (Tile)target;
            _pageCount = Mathf.Max(_pageCount - 1, 1);
            tile.SetProperty("pageCount", _pageCount);
            _currentPage = Mathf.Clamp(_currentPage, 0, _pageCount - 1);
            _pageText.text = (_currentPage + 1).ToString();
            stateBit = _currentPage;
            OnTargetChanged(target);
        }

        public void AddPage()
        {
            var tile = (Tile)target;
            _pageCount = Mathf.Max(_pageCount + 1, 1);
            _currentPage = _pageCount - 1;
            _pageText.text = (_currentPage + 1).ToString();
            tile.SetProperty("pageCount", _pageCount);
            stateBit = _currentPage;
            OnTargetChanged(target);
        }

        protected override void OnTargetChanged(object target)
        {
            base.OnTargetChanged(target);

            var tile = (Tile)target;
            _pageCount = Mathf.Max(tile.GetPropertyInt("pageCount"), 1);
        }
    }
}
