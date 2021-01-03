
namespace Puzzled.Editor.Commands
{
    class TileRenameCommand : Command
    {
        private Tile _tile;
        private string _undoName;
        private string _redoName;

        public TileRenameCommand (Tile tile, string name)
        {
            _tile = tile;
            _redoName = name;
            _undoName = tile.name;
        }

        protected override void OnExecute()
        {
            _tile.name = _redoName;
        }

        protected override void OnUndo()
        {
            _tile.name = _undoName;
        }
    }
}
