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
                Destroy(content.GetChild(i).gameObject);

            foreach (var puzzle in puzzlePack.puzzles)
            {
                var go = Instantiate(puzzleButtonPrefab, content);
                go.GetComponent<UIPuzzleButton>().puzzle = puzzle;
            }
        }
    }
}
