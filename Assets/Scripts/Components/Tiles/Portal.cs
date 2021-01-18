using UnityEngine;
using NoZ;
using System;

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

            var path = System.IO.Path.Combine(UnityEngine.Application.dataPath, $"Puzzles/{puzzle.worldName}/{puzzleName}.puzzle");
            var p = GameManager.GetPuzzleFromPath(path);
            if (null != p)
                GameManager.puzzle = p;
            else if (System.IO.File.Exists(path))
            {
                try
                {
                    GameManager.LoadPuzzle(path);
                }
                catch(Exception e)
                {
                    Debug.LogException(e);
                }                
            }
        }
    }
}
