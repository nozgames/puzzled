using NoZ;

namespace Puzzled
{
    class QueryTooltipEvent : ActorEvent
    {
        /// <summary>
        /// Tooltip string
        /// </summary>
        public string tooltip { get; set; } = null;

        /// <summary>
        /// Height offset for tooltip
        /// </summary>
        public float height { get; set; } = 0.0f;

        public Player player { get; private set; }

        public QueryTooltipEvent(Player player)
        {
            this.player = player;
        }
    }
}
