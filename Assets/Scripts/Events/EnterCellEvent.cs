using NoZ;
using UnityEngine;

namespace Puzzled
{
    public class EnterCellEvent : ActorEvent
    {
        /// <summary>
        /// Tile that is entering the cell
        /// </summary>
        public Tile tile { get; private set; }

        public bool isPlayer => tile.gameObject.GetComponent<Player>() != null;

        public EnterCellEvent(Tile tile)
        {
            this.tile = tile;
        }
    }
}
