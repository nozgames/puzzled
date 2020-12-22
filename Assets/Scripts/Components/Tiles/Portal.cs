using NoZ;

namespace Puzzled
{
    public class Portal : TileComponent
    {
        [Editable]
        public string puzzleName { get; set; }

        [Editable]
        public bool saveState { get; set; } = false;

        [ActorEventHandler]
        private void OnUse(UseEvent evt)
        {
            if (!saveState)
                GameManager.UnloadPuzzle();

            var p = GameManager.GetPuzzle(puzzleName);
            if (null != p)
                GameManager.puzzle = p;
            else
                GameManager.LoadPuzzle(System.IO.Path.Combine(UnityEngine.Application.dataPath, $"Puzzles/{puzzleName}.puzzle"));
        }
    }
}
