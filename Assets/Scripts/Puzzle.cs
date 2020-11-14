using UnityEngine;

namespace Puzzled
{
    [CreateAssetMenu(fileName="New Puzzle", menuName ="Puzzled/Puzzle")]
    public class Puzzle : ScriptableObject
    {
        public GameObject puzzlePrefab;

        [SerializeField]
        private struct Piece
        {
        }

        [SerializeField] private int width = 10;
        [SerializeField] private int height = 10;
        [SerializeField] private Theme theme = null;

        public Theme Theme {
            get => theme;
            set {
                theme = value;
            }
        }

        public int Width => width;

        public int Height => height;
    }
}
