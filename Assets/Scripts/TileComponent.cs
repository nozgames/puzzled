using NoZ;

namespace Puzzled
{
    public class TileComponent : ActorComponent
    {
        /// <summary>
        /// Tile the component is attached to
        /// </summary>
        public Tile tile => (Tile)base.actor;

        /// <summary>
        /// Puzzle the parent tile is attached to
        /// </summary>
        public Puzzle puzzle => tile.puzzle;

        /// <summary>
        /// True if the component is being edited
        /// </summary>
        public bool isEditing => puzzle.isEditing;

        /// <summary>
        /// True if the component is in the process of loading
        /// </summary>
        public bool isLoading => puzzle.isLoading;


        public bool SendToCell(ActorEvent evt, Cell cell, CellEventRouting routing = CellEventRouting.All) =>
            puzzle.grid.SendToCell(evt, cell, routing);

        protected void BeginBusy() => GameManager.busy++;

        protected void EndBusy() => GameManager.busy--;
    }
}
