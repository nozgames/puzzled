using UnityEngine;
using UnityEngine.UI;

namespace Puzzled
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
