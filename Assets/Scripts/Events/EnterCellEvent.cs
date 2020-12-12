using NoZ;

namespace Puzzled
{
    public class EnterCellEvent : ActorEvent
    {
        /// <summary>
        /// Actor that is leaving the cell
        /// </summary>
        public Actor actor { get; private set; }

        /// <summary>
        /// Cell the actor came from
        /// </summary>
        public Cell cellFrom { get; private set; }

        public bool isPlayer => actor.gameObject.GetComponent<Player>() != null;

        public EnterCellEvent(Actor actor, Cell cellFrom)
        {
            this.actor = actor;
            this.cellFrom = cellFrom;
        }
    }
}
