using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Puzzled.UI
{
    class UIMainScreen : UIScreen
    {
        [SerializeField] private Button _playButton = null;
        [SerializeField] private Button _createButton = null;
        [SerializeField] private Button _quitButton = null;

        private void Awake()
        {
            _playButton.onClick.AddListener(() => {
                UIManager.ShowPlayScreen();
            });

            _createButton.onClick.AddListener(() => {
                UIManager.ShowCreateScreen();
            });

            _quitButton.onClick.AddListener(() => {
#if UNITY_EDITOR
                EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
            });
        }

        private void OnEnable()
        {
            _playButton.Select();
        }
    }
}
