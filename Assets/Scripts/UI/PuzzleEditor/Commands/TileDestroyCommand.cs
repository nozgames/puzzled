namespace Puzzled.Editor.Commands
{
    public class TileDestroyCommand : Command
    {
        private Tile tile;
        private Cell cell;

        public TileDestroyCommand(Tile tile)
        {
            this.tile = tile;
            cell = tile.cell;
        }

        protected override void OnExecute()
        {
            MoveToTrash();
        }

        protected override void OnUndo()
        {
            UIPuzzleEditor.RestoreFromTrash(tile.gameObject);
            tile.cell = cell;
        }

        protected override void OnRedo()
        {
            MoveToTrash();
        }

        private void MoveToTrash()
        {
            tile.cell = Cell.invalid;
            UIPuzzleEditor.MoveToTrash(tile.gameObject);
        }

        protected override void OnDestroy()
        {
            if (isExecuted)
            {
                UIPuzzleEditor.RestoreFromTrash(tile.gameObject);
                tile.Destroy();
            }
        }
    }
}
