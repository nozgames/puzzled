using NoZ;
using UnityEngine;

namespace Puzzled
{
    public class LeaveCellEvent : ActorEvent
    {
        /// <summary>
        /// Actor that is leaving the cell
        /// </summary>
        public Actor actor { get; private set; }

        /// <summary>
        /// Cell the actor is leaving to
        /// </summary>
        public Cell cellTo { get; private set; }

        public bool isPlayer => actor.gameObject.GetComponent<Player>() != null;

        public LeaveCellEvent (Actor actor, Cell cellTo)
        {
            this.actor = actor;
            this.cellTo = cellTo;
        }
    }
}
