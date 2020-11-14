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
            GameManager.Instance.Restart();
            UIManager.instance.HideMenu();
        }

        public void OnQuitButton()
        {
            UIManager.instance.ShowMainMenu();
        }
    }
}
