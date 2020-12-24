using UnityEngine;

using Puzzled.Editor;

namespace Puzzled
{
    public partial class UIPuzzleEditor
    {
        [Header("DrawTool")]
        [SerializeField] private UITilePalette _tilePalette = null;

        private void EnableDrawTool()
        {
            canvas.onLButtonDown = OnDrawToolLButtonDown;
            canvas.onLButtonDrag = OnDrawToolDrag;

            _tilePalette.gameObject.SetActive(true);
        }

        private void DisableDrawTool()
        {
            _tilePalette.gameObject.SetActive(false);
        }

        private void OnDrawToolLButtonDown(Vector2 position) => Draw(position, false);

        private void OnDrawToolDrag(Vector2 position, Vector2 delta) => Draw(position, true);


        private void Draw (Vector2 position, bool group)
        {
            if (null == _tilePalette.selected)
                return;

            var tile = _tilePalette.selected;
            var cell = canvas.CanvasToCell(position);

            // Dont draw if the same tile is already there.  This will prevent
            // accidental removal of connections and properties
            var existing = _puzzle.grid.CellToTile(cell, tile.info.layer);
            if (existing != null && existing.guid == tile.guid)
                return;

            // Static objects cannot be placed on floor objects.
            if (tile.info.layer == TileLayer.Dynamic)
            {
                var staticTile = _puzzle.grid.CellToTile(cell, TileLayer.Static);
                if (staticTile != null && !staticTile.info.allowDynamic)
                    return;
            }

            ExecuteCommand(new Editor.Commands.TileAddCommand(tile, cell), group);
        }
    }
}
