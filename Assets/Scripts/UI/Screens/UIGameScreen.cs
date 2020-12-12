using UnityEngine;

namespace Puzzled
{
    class UIGameScreen : UIScreen
    {
        public void OnResumeButton()
        {
            UIManager.instance.HideMenu();
        }

        public void OnRestartButton()
        {
            //GameManager._instance.LoadPuzzle(GameManager._instance.puzzle);
            UIManager.instance.HideMenu();
        }

        public void OnQuitButton()
        {
            UIManager.instance.ShowMainMenu();
        }
    }
}
