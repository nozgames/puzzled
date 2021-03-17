using UnityEngine;

namespace Puzzled.Editor.Commands
{
    class TileAddCommand : Command
    {
        private Tile prefab;
        private Tile tile;
        private Cell cell;

        public TileAddCommand(Tile prefab, Cell cell, Tile tile = null)
        {
            this.prefab = prefab;
            this.cell = cell;
            this.tile = tile;
        }

        protected override void OnExecute()
        {
            // Create the new tile
            if(tile == null)
                tile = puzzle.InstantiateTile(prefab, cell);

            // Start again
            tile.Send(new StartEvent());
        }

        protected override void OnUndo()
        {
            // Unlink the tile
            tile.cell = Cell.invalid;

            // Move to the trash
            UIPuzzleEditor.MoveToTrash(tile.gameObject);
        }

        protected override void OnRedo()
        {
            // Restore from trash
            UIPuzzleEditor.RestoreFromTrash(tile.gameObject);

            // Move the tile to the cell
            tile.cell = cell;

            // Start again
            tile.Send(new StartEvent());
        }

        protected override void OnDestroy()
        {
            if (!isExecuted)
                tile.Destroy();
        }
    }
}
