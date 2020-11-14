using UnityEngine;

namespace Puzzled
{
    [CreateAssetMenu(fileName = "New Puzzle Pack", menuName = "Puzzled/Puzzle Pack")]
    public class PuzzlePack : ScriptableObject
    {
        public Puzzle[] puzzles;
    }
}

