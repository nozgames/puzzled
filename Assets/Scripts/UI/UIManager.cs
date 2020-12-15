﻿using UnityEngine;

namespace Puzzled
{
    public class UIManager : MonoBehaviour
    {
        [Header("General")]
        [SerializeField] private UIScreen startScreen = null;
        [SerializeField] private Transform popups = null;
        [SerializeField] private Transform popupCentered = null;

        [Header("Screens")]
        [SerializeField] private UIScreen mainMenu = null;
        [SerializeField] private UIScreen ingameScreen = null;
        [SerializeField] private UIScreen puzzleComplete = null;
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
        
        public void HideMenu ()
        {
            if (null == activeScreen)
                return;

            activeScreen.gameObject.SetActive(false);
        }

        public void EditPuzzle ()
        {
            if (activeScreen != null)
                activeScreen.gameObject.SetActive(false);

            activeScreen = puzzleEditor;
            activeScreen.gameObject.SetActive(true);
        }

        public static UIPopup ShowPopup(UIPopup popupPrefab)
        {
            instance.popups.gameObject.SetActive(true);
            return Instantiate(popupPrefab, instance.popupCentered).GetComponent<UIPopup>();
        }

        public static void ClosePopup()
        {
            instance.popups.gameObject.SetActive(false);
            instance.popupCentered.DetachAndDestroyChildren();
        }
    }
}
