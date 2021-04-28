using UnityEngine;

namespace Puzzled
{
    class UIPauseScreen : UIScreen
    {
        public void OnResumeButton()
        {
            UIManager.HideMenu();
        }

        public void OnQuitButton()
        {
            GameManager.Stop();
            GameManager.UnloadPuzzle();
            UIManager.ShowMainScreen();
        }
    }
}
