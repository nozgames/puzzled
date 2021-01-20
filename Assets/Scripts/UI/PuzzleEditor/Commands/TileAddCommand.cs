using UnityEngine;

namespace Puzzled.Editor.Commands
{
    class TileAddCommand : Command
    {
        private Tile prefab;
        private Tile tile;
        private Cell cell;
        private GroupCommand children;

        public TileAddCommand(Tile prefab, Cell cell)
        {
            this.prefab = prefab;
            this.cell = cell;
        }

        private void AddChild(Command child)
        {
            if (null == children)
                children = new GroupCommand();

            children.Add(child);
        }

        protected override void OnExecute()
        {
            // Remove what is already in that slot
            // TODO: if it is just a variant we should be able to swap it and reapply the connections and properties
            var existing = puzzle.grid.CellToTile(cell, prefab.info.layer);            
            if(null != existing)
                AddChild(new TileDestroyCommand(existing));

            // Destroy all other instances of this tile regardless of variant
            if (!prefab.info.allowMultiple)
            {
                existing = puzzle.grid.GetLinkedTile(prefab.info);
                if (null != existing)
                    AddChild(new TileDestroyCommand(existing));
            }

            // Create the new tile
            tile = puzzle.InstantiateTile(prefab, cell);
            if (null == tile)
                return;

            // If the incoming tile has a decal surface then transfer the decal and decal power to the
            // new surface
            var tileSurface = DecalSurface.FromCell(puzzle, cell, TileLayer.Static);
            if (tileSurface != null)
            {
                var floorSurface = DecalSurface.FromCell(puzzle, cell, TileLayer.Floor);
                if (null != floorSurface && floorSurface.decal != Decal.none)
                {
                    tileSurface.decal = floorSurface.decal;
                    AddChild(new TileSetPropertyCommand(floorSurface.tile, "decal", Decal.none));

                    foreach(var wire in floorSurface.decalPowerPort.wires)
                    {
                        AddChild(new WireAddCommand(wire.from.port, tileSurface.decalPowerPort));
                        AddChild(new WireDestroyCommand(wire));
                    }
                }
            }

            children?.Execute();

            // Ensure the tile is started when created
            tile.Send(new StartEvent());
        }

        protected override void OnUndo()
        {
            // Move to the trash
            UIPuzzleEditor.MoveToTrash(tile.gameObject);

            // Unlink the tile
            tile.cell = Cell.invalid;

            children?.Undo();
        }

        protected override void OnRedo()
        {
            children?.Redo();

            // Restore from trash
            UIPuzzleEditor.RestoreFromTrash(tile.gameObject);

            // Move the tile to the cell
            tile.cell = cell;

            // Start again
            tile.Send(new StartEvent());
        }

        protected override void OnDestroy()
        {
            children?.Destroy();

            if (!isExecuted)
                tile.Destroy();
        }
    }
}
