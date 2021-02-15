namespace Puzzled.Editor.Commands
{
    /// <summary>
    /// Command used to move a tile to a new cell
    /// </summary>
    class TileMoveCommand : Command
    {
        private Tile _tile;
        private Cell _redoCell;
        private Cell _undoCell;

        public TileMoveCommand(Tile tile, Cell cell)
        {
            _tile = tile;
            _undoCell = tile.cell;
            _redoCell = cell;
        }

        public TileMoveCommand(Tile tile, Cell cell, Cell undo)
        {
            _tile = tile;
            _undoCell = undo;
            _redoCell = cell;
        }

        protected override void OnExecute() => OnRedo();

        protected override void OnUndo()
        {
            if (_undoCell == _redoCell)
                return;

            _tile.cell = _undoCell;
        }

        protected override void OnRedo()
        {
            if (_undoCell == _redoCell)
                return;

            _tile.cell = _redoCell;
        }
    }
}
