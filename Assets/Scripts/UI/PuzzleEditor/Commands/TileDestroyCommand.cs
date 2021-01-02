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
            // Destroy all wires connected as well
            foreach(var property in tile.properties)
                if(property.type == TilePropertyType.Port)
                    foreach(var wire in property.GetValue<Port>(tile).wires)
                        children.Add(new WireDestroyCommand(wire));

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
