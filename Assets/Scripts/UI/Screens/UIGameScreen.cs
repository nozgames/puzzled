using UnityEngine;

namespace Puzzled
{
    class UIGameScreen : UIScreen
    {
        public void OnResumeButton()
        {
            UIManager.HideMenu();
        }

        public void OnRestartButton()
        {
            //GameManager._instance.LoadPuzzle(GameManager._instance.puzzle);
            UIManager.HideMenu();
        }

        public void OnQuitButton()
        {
            UIManager._instance.ShowMainMenu();
        }
    }
}
