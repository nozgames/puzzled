using UnityEngine;

using Puzzled.Editor;
using System;

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

            _getCursor = OnDrawGetCursor;

            _tilePalette.gameObject.SetActive(true);
        }

        private void DisableDrawTool()
        {
            _tilePalette.gameObject.SetActive(false);
        }

        private CursorType OnDrawGetCursor(Cell cell)
        {
            if (KeyboardManager.isAltPressed)
                return CursorType.EyeDropper;

            if (!layerToggles[(int)_tilePalette.selected.info.layer].isOn)
                return CursorType.Not;

            return CursorType.Crosshair;
        }
            
        private void OnDrawToolLButtonDown(Vector2 position) => Draw(position, false);

        private void OnDrawToolDrag(Vector2 position, Vector2 delta) => Draw(position, true);


        private void Draw (Vector2 position, bool group)
        {
            if (null == _tilePalette.selected)
                return;

            // Dont allow drawing with a tile that is hidden
            if (!layerToggles[(int)_tilePalette.selected.info.layer].isOn)
                return;

            var cell = canvas.CanvasToCell(position);

            if (KeyboardManager.isAltPressed)
            {
                EyeDropper(cell, !group);
                return;
            }
  
            var tile = _tilePalette.selected;

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

        private void EyeDropper(Cell cell, bool cycle)
        {
            var selected = _tilePalette.selected;
            var existing = GetTile(cell, (selected == null || !cycle) ? TileLayer.Logic : selected.info.layer);
            if (cycle && existing != null && selected != null && existing.guid == selected.guid)
            {
                if (existing.info.layer != TileLayer.Floor)
                    existing = GetTile(cell, (TileLayer)(existing.info.layer - 1));
                else
                    existing = null;
            }

            if (null == existing)
            {
                existing = GetTile(cell, TileLayer.Logic);
                if (null == existing)
                    return;
            }

            _tilePalette.selected = TileDatabase.GetTile(existing.guid);
        }
    }
}
