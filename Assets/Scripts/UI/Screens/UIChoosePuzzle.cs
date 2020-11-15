using UnityEngine;

namespace Puzzled
{
    public class UIChoosePuzzle : UIScreen
    {
        [SerializeField] private Transform content = null;
        [SerializeField] private GameObject puzzleButtonPrefab = null;

        public PuzzlePack puzzlePack { get; set; }

        private void OnEnable()
        {
            for (int i = content.childCount - 1; i >= 0; i--)
            {
                var go = content.GetChild(i).gameObject;
                go.transform.SetParent(null);
                Destroy(go);
            }

            foreach (var puzzle in puzzlePack.puzzles)
            {
                var go = Instantiate(puzzleButtonPrefab, content);
                go.GetComponent<UIPuzzleButton>().puzzle = puzzle;
            }
        }

        public void OnBackButton()
        {
            UIManager.instance.ChoosePuzzlePack();
        }
    }
}
