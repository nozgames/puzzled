using System.Linq;

namespace Puzzled.Editor.Commands
{
    class TileMoveCommand : Command
    {
        private Cell _offset;
        private Tile[] _tiles;
        private Cell[] _tileCells;
        private Cell _from;
        private Cell _to;
        private Cell _size;

        public TileMoveCommand(Tile[] tiles, Cell from, Cell to, Cell size)
        {
            _tiles = tiles;
            _offset = to - from;
            _from = from;
            _to = to;
            _size = size;
            _tileCells = tiles.Select(t => t.cell).ToArray();
        }

        protected override void OnExecute()
        {
            foreach (var tile in _tiles)
                puzzle.grid.UnlinkTile(tile);

            for (int i = 0; i < _tiles.Length; i++)
                _tiles[i].cell = _tileCells[i] + _offset;

            var startEvent = new StartEvent();
            foreach (var tile in _tiles)
                tile.Send(startEvent);

            UIPuzzleEditor.instance.SetSelectionRect(_to, _to + _size);
        }

        protected override void OnUndo()
        {
            foreach (var tile in _tiles)
                puzzle.grid.UnlinkTile(tile);

            for (int i = 0; i < _tiles.Length; i++)
                _tiles[i].cell = _tileCells[i];

            var startEvent = new StartEvent();
            foreach (var tile in _tiles)
                tile.Send(startEvent);

            UIPuzzleEditor.instance.SetSelectionRect(_from, _from + _size);
        }
    }
}
