using UnityEngine;
using UnityEngine.UI;

namespace Puzzled
{
    class UIPuzzleComplete : UIScreen
    {
        [SerializeField] private Button nextButton = null;

        private void OnEnable()
        {
            var nextPuzzle = UIManager.instance.GetNextPuzzle();
            nextButton.interactable = nextPuzzle != null;
        }

        public void OnReplayButton()
        {
            GameManager.Instance.Restart(GameManager.Instance.puzzle);
            UIManager.instance.HideMenu();
        }

        public void OnMainMenuButton()
        {
            UIManager.instance.ShowMainMenu();
        }

        public void OnNextButton()
        {
            GameManager.Instance.Restart(UIManager.instance.GetNextPuzzle());
            UIManager.instance.HideMenu();
        }
    }
}
