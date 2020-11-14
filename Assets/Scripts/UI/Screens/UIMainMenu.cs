using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Puzzled
{
    class UIMainMenu : UIScreen
    {
        public void OnPlayButton()
        {
            GameManager.Instance.Restart();
            UIManager.instance.HideMenu();
        }

        public void OnQuitButton()
        {
#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
