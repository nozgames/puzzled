﻿using UnityEngine;

namespace Puzzled
{
    public partial class UIPuzzleEditor
    {
        private void EnableDrawTool()
        {
            canvas.onLButtonDown = OnDrawToolLButtonDown;
            canvas.onLButtonDrag = OnDrawToolDrag;
            
            FilterPalette(typeof(UITileItem));
            palette.SetActive(true);
        }

        private void DisableDrawTool()
        {
        }

        private void OnDrawToolLButtonDown(Vector2 position) => Draw(position, false);

        private void OnDrawToolDrag(Vector2 position, Vector2 delta) => Draw(position, true);


        private void Draw (Vector2 position, bool group)
        {
            if (null == drawTile)
                return;

            var cell = canvas.CanvasToCell(position);

            // Dont draw if the same tile is already there.  This will prevent
            // accidental removal of connections and properties
            var existing = _puzzle.grid.CellToTile(cell, drawTile.info.layer);
            if (existing != null && existing.guid == drawTile.guid)
                return;

            // Static objects cannot be placed on floor objects.
            if (drawTile.info.layer == TileLayer.Dynamic)
            {
                var staticTile = _puzzle.grid.CellToTile(cell, TileLayer.Static);
                if (staticTile != null && !staticTile.info.allowDynamic)
                    return;
            }

            ExecuteCommand(new Editor.Commands.TileAddCommand(drawTile, cell), group);
        }
    }
}
