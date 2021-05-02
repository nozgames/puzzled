using UnityEngine;
using UnityEngine.UI;

namespace Puzzled.UI
{
    class UIPlayScreen : UIScreen
    {
        [SerializeField] private Button _closeButton = null;

        private void Awake()
        {
            _closeButton.onClick.AddListener(() => {
                UIManager.ShowMainScreen();
            });
        }
    }
}
