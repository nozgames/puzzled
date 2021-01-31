using System.Collections.Generic;
using UnityEngine;

namespace Puzzled.Editor.Commands
{
    class TileAddCommand : Command
    {
        private Tile prefab;
        private Tile tile;
        private Cell cell;

        public TileAddCommand(Tile prefab, Cell cell)
        {
            this.prefab = prefab;
            this.cell = cell;
        }

        protected override void OnExecute()
        {
            // Create the new tile
            tile = puzzle.InstantiateTile(prefab, cell);

            // Start again
            tile.Send(new StartEvent());
        }

        protected override void OnUndo()
        {
            // Move to the trash
            UIPuzzleEditor.MoveToTrash(tile.gameObject);

            // Unlink the tile
            tile.cell = Cell.invalid;

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
