using UnityEngine;

namespace Puzzled
{
    public class UIManager : MonoBehaviour
    {
        [Header("General")]
        [SerializeField] private UIScreen startScreen = null;

        [Header("Screens")]
        [SerializeField] private UIScreen mainMenu = null;
        [SerializeField] private UIScreen ingameScreen = null;
        [SerializeField] private UIScreen puzzleComplete = null;
        [SerializeField] private UIChoosePack choosePuzzlePack = null;
        [SerializeField] private UIChoosePuzzle choosePuzzle = null;
        [SerializeField] private UIPuzzleEditor puzzleEditor = null;

        public static UIManager instance { get; private set; }

        public UIScreen activeScreen { get; private set; }

        private void Awake()
        {
            instance = this;

            foreach (var screen in GetComponentsInChildren<UIScreen>(true))
                screen.gameObject.SetActive(false);

            startScreen.gameObject.SetActive(true);
            activeScreen = startScreen;
        }

        private void OnDestroy()
        {
            instance = null;
        }

        public void ShowMainMenu()
        {
            if (activeScreen != null)
                activeScreen.gameObject.SetActive(false);

            activeScreen = mainMenu;
            activeScreen.gameObject.SetActive(true);
        }

        public void ShowIngame ()
        {
            if (activeScreen != null)
                activeScreen.gameObject.SetActive(false);

            activeScreen = ingameScreen;
            activeScreen.gameObject.SetActive(true);
        }

        public void ShowPuzzleComplete()
        {
            if(activeScreen == puzzleEditor)
            {
                puzzleEditor.OnStopButton();
                return;
            }

            if (activeScreen != null)
                activeScreen.gameObject.SetActive(false);

            activeScreen = puzzleComplete;
            activeScreen.gameObject.SetActive(true);            
        }

        public void ChoosePuzzlePack ()
        {
            if (activeScreen != null)
                activeScreen.gameObject.SetActive(false);

            activeScreen = choosePuzzlePack;
            activeScreen.gameObject.SetActive(true);
        }

        public void ChoosePuzzle (PuzzlePack pack)
        {
            if (activeScreen != null)
                activeScreen.gameObject.SetActive(false);

            choosePuzzle.puzzlePack = pack;
            activeScreen = choosePuzzle;
            activeScreen.gameObject.SetActive(true);
        }

        public void HideMenu ()
        {
            if (null == activeScreen)
                return;

            activeScreen.gameObject.SetActive(false);
        }

        public Puzzle GetNextPuzzle ()
        {
            var pack = choosePuzzle.puzzlePack;
            if (null == pack)
                return null;

            var current = GameManager.Instance.puzzle;
            if (null == current)
                return null;

            for(int i=0; i<pack.puzzles.Length - 1; i++)
                if(pack.puzzles[i] == current)
                    return pack.puzzles[i + 1];

            return null;
        }

        public void EditPuzzle ()
        {
            if (activeScreen != null)
                activeScreen.gameObject.SetActive(false);

            activeScreen = puzzleEditor;
            activeScreen.gameObject.SetActive(true);
        }
    }
}
