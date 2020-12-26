namespace Puzzled.Editor.Commands
{
    public class TileDestroyCommand : Command
    {
        private Tile tile;
        private Cell cell;
        private GroupCommand children;

        public TileDestroyCommand(Tile tile)
        {
            this.tile = tile;
            cell = tile.cell;
        }

        protected override void OnExecute()
        {
            if (tile.inputCount + tile.outputCount > 0)
            {
                children = new GroupCommand();
                foreach (var input in tile.inputs)
                    children.Add(new WireDestroyCommand(input));

                foreach (var output in tile.outputs)
                    children.Add(new WireDestroyCommand(output));
            }

            children?.Execute();

            MoveToTrash();
        }

        protected override void OnUndo()
        {
            UIPuzzleEditor.RestoreFromTrash(tile.gameObject);
            tile.cell = cell;

            children?.Undo();
        }

        protected override void OnRedo()
        {
            children?.Redo();
            MoveToTrash();
        }

        private void MoveToTrash()
        {
            UIPuzzleEditor.MoveToTrash(tile.gameObject);
            tile.cell = Cell.invalid;
        }

        protected override void OnDestroy()
        {
            children?.Destroy();

            if (isExecuted)
            {
                UIPuzzleEditor.RestoreFromTrash(tile.gameObject);
                tile.Destroy();
            }
        }
    }
}
