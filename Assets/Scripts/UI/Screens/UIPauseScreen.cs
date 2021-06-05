using UnityEngine;
using UnityEngine.UI;

namespace Puzzled.UI
{
    class UIPauseScreen : UIScreen
    {
        [SerializeField] private Button _resumeButton = null;
        //[SerializeField] private Button _optionsButton = null;
        [SerializeField] private Button _quitButton = null;

        private void Quit()
        {
            GameManager.Stop();
            GameManager.UnloadPuzzle();
            UIManager.ReturnToPlayWorldScreen();
        }

        private void Unpause()
        {
            UIManager.HideMenu();
        }

        private void Awake()
        {
            _resumeButton.onClick.AddListener(Unpause);
            //_optionsButton.onClick.AddListener();
            _quitButton.onClick.AddListener(Quit);
        }

        override protected void OnScreenActivated()
        {
            _resumeButton.Select();
            GameManager.busy++;
        }

        override protected void OnScreenDeactivated()
        {
            //_quitButton.Select(); // FIXME: why do we need to do this?
            GameManager.busy--;
        }

        override public void HandleCancelInput()
        {
            Unpause();
        }

        override public void HandleMenuInput()
        {
            Unpause();
        }
    }
}
