using UnityEngine;

namespace Puzzled
{
    public partial class UIPuzzleEditor
    {
        private void EnableDrawTool()
        {
            canvas.onLButtonDown = OnDrawToolLButtonDown;
            canvas.onLButtonDrag = OnDrawToolDrag;
        }

        private void DisableDrawTool()
        {
        }

        private void OnDrawToolLButtonDown(Vector2 position) => Draw(position);

        private void OnDrawToolDrag(Vector2 position, Vector2 delta) => Draw(position);

        private void Draw (Vector2 position)
        {
            if (null == drawTile)
                return;

            var cell = canvas.CanvasToCell(position);

            // Dont draw if the same tile is already there.  This will prevent
            // accidental removal of connections and properties
            var existing = TileGrid.CellToTile(cell, drawTile.info.layer);
            if (existing != null && existing.guid == drawTile.guid)
                return;

            // Static objects cannot be placed on floor objects.
            if (drawTile.info.layer == TileLayer.Dynamic)
            {
                var staticTile = TileGrid.CellToTile(cell, TileLayer.Static);
                if (staticTile != null && !staticTile.info.allowDynamic)
                    return;
            }

            // Remove what is already in that slot
            // TODO: if it is just a variant we should be able to swap it and reapply the connections and properties
            TileGrid.UnlinkTile(cell, drawTile.info.layer, true);

            // Destroy all other instances of this tile regardless of variant
            if (!drawTile.info.allowMultiple)
                TileGrid.UnlinkTile(TileGrid.GetLinkedTile(drawTile.info), true);

            // Create the new tile
            GameManager.InstantiateTile(drawTile, cell);
        }
    }
}
