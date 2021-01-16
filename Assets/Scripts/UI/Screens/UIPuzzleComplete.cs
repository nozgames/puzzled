using UnityEngine;
using UnityEngine.UI;

namespace Puzzled
{
    class UIPuzzleComplete : UIScreen
    {
        //[SerializeField] private Button nextButton = null;

        private void OnEnable()
        {
            //var nextPuzzle = UIManager.instance.GetNextPuzzle();
            //nextButton.interactable = nextPuzzle != null;
        }

        public void OnReplayButton()
        {
            //GameManager._instance.LoadPuzzle(GameManager._instance.puzzle);
            UIManager.HideMenu();
        }

        public void OnMainMenuButton()
        {
            UIManager._instance.ShowMainMenu();
        }

        public void OnNextButton()
        {
            //GameManager._instance.LoadPuzzle(UIManager.instance.GetNextPuzzle());
            UIManager.HideMenu();
        }
    }
}
