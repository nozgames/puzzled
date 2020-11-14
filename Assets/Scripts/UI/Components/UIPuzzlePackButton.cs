using System;
using UnityEngine;
using UnityEngine.UI;

namespace Puzzled
{
    public class UIPuzzlePackButton : MonoBehaviour
    {
        [SerializeField] private TMPro.TextMeshProUGUI name = null;

        private PuzzlePack _puzzlePack;

        public PuzzlePack puzzlePack {
            get => _puzzlePack;
            set {
                _puzzlePack = value;

                if (value == null)
                    return;

                name.text = _puzzlePack.name;
            }
        }

        private void Awake()
        {
            GetComponent<Button>().onClick.AddListener(OnClick);
        }

        private void OnClick()
        {
            UIManager.instance.ChoosePuzzle(_puzzlePack);
        }
    }
}
