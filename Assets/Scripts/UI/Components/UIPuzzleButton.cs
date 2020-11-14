using System;
using UnityEngine;
using UnityEngine.UI;

namespace Puzzled
{
    public class UIPuzzleButton : MonoBehaviour
    {
        [SerializeField] private TMPro.TextMeshProUGUI name = null;

        private Puzzle _puzzle = null;

        public Puzzle puzzle {
            get => _puzzle;
            set {
                _puzzle = value;
                if (null == _puzzle)
                    return;

                name.text = puzzle.name;
            }
        }

        private void Awake()
        {
            GetComponent<Button>().onClick.AddListener(OnClick);
        }

        private void OnClick()
        {
            GameManager.Instance.Restart(puzzle);
            UIManager.instance.HideMenu();
        }
    }
}
