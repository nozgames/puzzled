using UnityEngine;
using UnityEngine.UI;

namespace Puzzled
{
    class UICreateScreen : UIScreen
    {
        [SerializeField] private Button _closeButton = null;
        [SerializeField] private Button _tempButton = null;

        private void Awake()
        {
            _closeButton.onClick.AddListener(() => {
                UIManager.ShowMainScreen();
            });

            _tempButton.onClick.AddListener(() => {
                UIPuzzleEditor.Initialize();
            });
        }
    }
}
