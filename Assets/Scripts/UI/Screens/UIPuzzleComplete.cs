using UnityEngine;
using UnityEngine.UI;

namespace Puzzled
{
    class UIPuzzleComplete : UIScreen
    {
        [SerializeField] private Button nextButton = null;

        private void OnEnable()
        {
            nextButton.interactable = false;
        }

        public void OnReplayButton()
        {
            GameManager.Instance.Restart();
            UIManager.instance.HideMenu();
        }

        public void OnMainMenuButton()
        {
            UIManager.instance.ShowMainMenu();
        }

        public void OnNextButton()
        {
            GameManager.Instance.Restart();
            UIManager.instance.HideMenu();
        }
    }
}
