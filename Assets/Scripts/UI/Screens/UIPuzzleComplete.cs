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
            GameManager.Instance.LoadPuzzle(GameManager.Instance.puzzle);
            UIManager.instance.HideMenu();
        }

        public void OnMainMenuButton()
        {
            UIManager.instance.ShowMainMenu();
        }

        public void OnNextButton()
        {
            GameManager.Instance.LoadPuzzle(UIManager.instance.GetNextPuzzle());
            UIManager.instance.HideMenu();
        }
    }
}
