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
        [SerializeField] private UIScreen choosePuzzlePack = null;
        [SerializeField] private UIChoosePuzzle choosePuzzle = null;

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
    }
}
