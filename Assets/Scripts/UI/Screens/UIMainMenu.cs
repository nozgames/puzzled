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
            UIManager.instance.ChoosePuzzlePack();
        }

        public void OnQuitButton()
        {
#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        public void OnEditorButton()
        {
            UIManager.instance.EditPuzzle();
        }
    }
}
