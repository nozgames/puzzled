using UnityEngine;

namespace Puzzled
{
    class UIGameScreen : UIScreen
    {
        public void OnResumeButton()
        {
            UIManager.HideMenu();
        }

        public void OnQuitButton()
        {
            GameManager.Stop();
            GameManager.UnloadPuzzle();
            UIManager.ShowMainMenu();
        }
    }
}
