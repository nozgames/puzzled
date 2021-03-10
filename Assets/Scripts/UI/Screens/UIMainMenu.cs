using UnityEngine;
using System.IO;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Puzzled
{
    class UIMainMenu : UIScreen
    {
        public void OnPlayButton()
        {
            //UIManager.instance.ChoosePuzzlePack();
        }

        public void OnQuitButton()
        {
#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        public void OnStoryButton()
        {
            GameManager.LoadPuzzle(Path.Combine(Application.dataPath, "Puzzles", "Story", "main.puzzle"));
            GameManager.Play();
            UIManager.HideMenu();            
        }

        public void OnWorldButton()
        {
            UIManager.ShowWorldsMenu();
        }

        public void OnCreateButton()
        {
            UIPuzzleEditor.Initialize();
        }
    }
}
