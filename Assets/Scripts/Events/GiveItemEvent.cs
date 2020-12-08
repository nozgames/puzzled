using NoZ;
using UnityEngine;

namespace Puzzled
{
    public class GiveItemEvent : ActorEvent
    {
        /// <summary>
        /// Tile that is entering the cell
        /// </summary>
        public Item item { get; private set; }

        public GiveItemEvent(Item item)
        {
            this.item = item;
        }
    }
}
