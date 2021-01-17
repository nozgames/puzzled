namespace Puzzled.Editor.Commands
{
    public class TileDestroyCommand : Command
    {
        private Tile tile;
        private Cell cell;
        private GroupCommand children = null;

        public TileDestroyCommand(Tile tile)
        {
            this.tile = tile;
            cell = tile.cell;
        }

        private void AddChild(Command command)
        {
            if (null == children)
                children = new GroupCommand();

            children.Add(command);
        }

        protected override void OnExecute()
        {
            // Destroy all wires connected as well
            foreach(var property in tile.properties)
                if(property.type == TilePropertyType.Port)
                    foreach(var wire in property.GetValue<Port>(tile).wires)
                        AddChild(new WireDestroyCommand(wire));

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
            tile.cell = Cell.invalid;
            UIPuzzleEditor.MoveToTrash(tile.gameObject);
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
