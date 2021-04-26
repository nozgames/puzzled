using UnityEngine;

using Puzzled.Editor;
using System;
using System.Linq;
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
            if (!IsLayerVisible(tile.layer))
                return CursorType.Not;

            // Static objects cannot be placed on floor objects.
            if (tile.layer == TileLayer.Dynamic)
            {
                var staticTile = _puzzle.grid.CellToTile(cell, TileLayer.Static);
                if (staticTile != null && !staticTile.info.allowDynamic)
                    return CursorType.Not;
            }

            // Dont allow drawing of a static tile that does not allow dynamics on top of a dynamic
            if (tile.layer == TileLayer.Static && !tile.info.allowDynamic)
                if (null != _puzzle.grid.CellToTile(cell, TileLayer.Dynamic))
                    return CursorType.Not;

            // Special case to prevent wall mounted objects from being placed where there are no walls
            // or on walls that do not allow wall mounted objects.
            if (tile.layer == TileLayer.WallStatic)
            {
                var wall = _puzzle.grid.CellToComponent<Wall>(cell.ConvertTo(CellCoordinateSystem.SharedEdge), TileLayer.Wall);
                if (null == wall || !wall.allowsWallMounts)
                    return CursorType.Not;
            }

            return CursorType.Crosshair;
        }
            
        private void OnDrawToolLButtonDown(Vector2 position) => Draw(false);

        private void OnDrawToolDrag(Vector2 position, Vector2 delta) => Draw(true);

        private void Draw (bool group)
        {
            if (null == _tilePalette.selected)
                return;

            // Dont allow drawing with a tile that is hidden
            if (!IsLayerVisible(_tilePalette.selected.layer))
                return;

            if (KeyboardManager.isAltPressed)
            {
                EyeDropper(_cursorCell, !group);
                return;
            }

            if (UIManager.cursor != CursorType.Crosshair)
                return;
  
            Draw(_cursorCell, _tilePalette.selected, group);
        }

        private void Draw (Cell cell, Tile prefab, bool group = false)
        {
            if (!puzzle.grid.IsValid(cell))
                return;

            var command = new Editor.Commands.GroupCommand();

            // Dont draw if the same exact tile is already there.  This prevents accidental removal 
            // of connections and properties
            var existing = puzzle.grid.CellToTile(cell, prefab.layer);
            if (existing != null && existing.guid == prefab.guid)
                return;

            // If this prefab does not allow multiples then destroy all other instances within the puzzle 
            if (!prefab.info.allowMultiple)
            {
                existing = puzzle.grid.GetTiles().FirstOrDefault(t => t.info == prefab.info);
                if (null != existing)
                    Erase(existing, command);
            } 
            else if (existing != null)
            {
                var eraseFlags = EraseFlags.None;
                if (existing.layer == TileLayer.WallStatic)
                {
                    var wall = prefab.GetComponent<Wall>();
                    if (null != wall && wall.allowsWallMounts)
                        eraseFlags |= EraseFlags.KeepWallMount;
                }

                // Remove what is already in that slot
                // TODO: if it is just a variant we should be able to swap it and reapply the connections and properties
                Erase(existing, command, eraseFlags);
            }

            var tile = puzzle.InstantiateTile(prefab.guid, Cell.invalid);
            command.Add(new Editor.Commands.TileAddCommand(prefab, cell, tile));

            // Copy properties and wires?
            if (existing)
            {
                foreach (var property in existing.properties)
                {
                    var otherProperty = tile.GetProperty(property.name);
                    if (otherProperty == null || otherProperty.type != property.type || !property.editable.serialized)
                        continue;

                    if (property.type != TilePropertyType.Port)
                        tile.SetPropertyValue(property.name, property.GetValue(existing));
                    else
                    {
                        var port = property.GetValue<Port>(existing);
                        var otherPort = otherProperty.GetValue<Port>(tile);
                        for (int wireIndex = 0; wireIndex < port.wireCount; wireIndex++)
                        {
                            var wire = port.GetWire(wireIndex);
                            var otherWire = port.flow == PortFlow.Output ?
                                puzzle.InstantiateWire(otherPort, wire.to.port) :
                                puzzle.InstantiateWire(wire.from.port, otherPort);

                            if (wire.from.hasOptions)
                                for (int optionIndex = 0; optionIndex < wire.from.options.Length; optionIndex++)
                                    otherWire.from.SetOption(optionIndex, wire.from.GetOption(optionIndex));

                            if (wire.to.hasOptions)
                                for (int optionIndex = 0; optionIndex < wire.to.options.Length; optionIndex++)
                                    otherWire.to.SetOption(optionIndex, wire.to.GetOption(optionIndex));
                        }
                    }
                }
            }

            command.Add(new Editor.Commands.TileMoveCommand(tile, cell));

            ExecuteCommand(command, group);
        }

        private void EyeDropper(Cell cell, bool cycle)
        {
            var selected = _tilePalette.selected;
            var existing = GetTopMostTile(cell, (selected == null || !cycle) ? TileLayer.Logic : selected.layer);
            if (cycle && existing != null && selected != null && existing.guid == selected.guid)
            {
                if (existing.layer != TileLayer.Floor)
                    existing = GetTopMostTile(cell, (TileLayer)(existing.layer - 1));
                else
                    existing = null;
            }

            if (null == existing)
            {
                existing = GetTopMostTile(cell, TileLayer.Logic);
                if (null == existing)
                    return;
            }

            _tilePalette.selected = DatabaseManager.GetTile(existing.guid);
        }
    }
}
