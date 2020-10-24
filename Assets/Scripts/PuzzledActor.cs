using UnityEngine;
using NoZ;

namespace Puzzled
{
    public class PuzzledActor : Actor
    {
        /// <summary>
        /// Cell the actor is current in
        /// </summary>
        public Vector2Int Cell {
            get => GameManager.Instance.GetActorCell(this);
            set => GameManager.Instance.SetActorCell(this, value);
        }
    }
}
