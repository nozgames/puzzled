using UnityEngine;

using Puzzled.Editor;
using System;
using System.Collections.Generic;

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
            if (!canvas.isMouseOver)
                return CursorType.Arrow;

            if (KeyboardManager.isAltPressed)
                return CursorType.EyeDropper;

            var tile = _tilePalette.selected;

            // Dont allow drawing with a tile that is on a hidden layer
            if (!layerToggles[(int)tile.info.layer].isOn)
                return CursorType.Not;

            // Static objects cannot be placed on floor objects.
            if (tile.info.layer == TileLayer.Dynamic)
            {
                var staticTile = _puzzle.grid.CellToTile(cell, TileLayer.Static);
                if (staticTile != null && !staticTile.info.allowDynamic)
                    return CursorType.Not;
            }

            // Dont allow drawing of a static tile that does not allow dynamics on top of a dynamic
            if (tile.info.layer == TileLayer.Static && !tile.info.allowDynamic)
                if (null != _puzzle.grid.CellToTile(cell, TileLayer.Dynamic))
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

            if (UIManager.cursor != CursorType.Crosshair)
                return;
  
            var tile = _tilePalette.selected;

            // Dont draw if the same tile is already there.  This will prevent
            // accidental removal of connections and properties
            var existing = _puzzle.grid.CellToTile(cell, tile.info.layer);
            if (existing != null && existing.guid == tile.guid)
                return;

            Draw(cell, tile);
        }

        private void Draw (Cell cell, Tile prefab)
        {
            var command = new Editor.Commands.GroupCommand();

            // Remove what is already in that slot
            // TODO: if it is just a variant we should be able to swap it and reapply the connections and properties
            var existing = puzzle.grid.CellToTile(cell, prefab.info.layer);
            if (null != existing)
                Erase(existing, command);

            // Destroy all other instances of this tile regardless of variant
            if (!prefab.info.allowMultiple)
            {
                existing = puzzle.grid.GetLinkedTile(prefab.info);
                if (null != existing)
                    Erase(existing, command);
            }

            List<Port> prefabPowerPorts = null;
            Decal decal = Decal.none;
            if (prefab.info.layer == TileLayer.Static)
            {
                // If there is a floor with a decal we need to remove it
                var floorSurface = DecalSurface.FromCell(puzzle, cell, TileLayer.Floor);
                var tileHasDecal = DecalSurface.FromTile(prefab) != null;
                if (null != floorSurface && floorSurface.decal != Decal.none)
                {
                    if (tileHasDecal)
                        decal = floorSurface.decal;

                    command.Add(new Editor.Commands.TileSetPropertyCommand(floorSurface.tile, "decal", Decal.none));

                    prefabPowerPorts = new List<Port>();
                    foreach (var wire in floorSurface.decalPowerPort.wires)
                    {
                        prefabPowerPorts.Add(wire.from.port);
                        command.Add(new Editor.Commands.WireDestroyCommand(wire));
                    }
                }
            }

            command.Add(new Editor.Commands.TileAddCommand(prefab, cell));
            ExecuteCommand(command);

            // See if we can move an old decal to the new tile
            if(decal != Decal.none)
            {
                // Get the new tile
                var tile = puzzle.grid.CellToTile(cell, prefab.info.layer);
                if (null == tile)
                    return;

                var tileDecalSurface = DecalSurface.FromTile(tile);
                if (null == tileDecalSurface)
                    return;

                command = new Editor.Commands.GroupCommand();
                command.Add(new Editor.Commands.TileSetPropertyCommand(tile, "decal", decal));

                if (prefabPowerPorts != null)
                    foreach (var port in prefabPowerPorts)
                        command.Add(new Editor.Commands.WireAddCommand(port, tileDecalSurface.decalPowerPort));

                ExecuteCommand(command, true);
            }
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
