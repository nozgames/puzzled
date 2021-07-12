using UnityEngine;
using System.Linq;
using Puzzled.UI;
using Puzzled.Editor.Commands;
using System;

namespace Puzzled.Editor
{
    public partial class UIPuzzleEditor
    {
        [Header("DrawTool")]
        [SerializeField] private UITilePalette _tilePalette = null;

        private Cell _drawCommandCell = Cell.invalid;
        private Command _drawCommand = null;
        private Tile _drawTile = null;

        private void EnableDrawTool()
        {
            _canvas.onLButtonDown = OnDrawToolLButtonDown;
            _canvas.onLButtonUp = OnDrawToolLButtonUp;
            _canvas.onLButtonDrag = OnDrawToolDrag;
            _canvas.onLButtonDragBegin = OnDrawToolDragBegin;
            _canvas.onExit = OnDrawToolPointerExit;

            _getCursor = OnDrawGetCursor;

            _tilePalette.gameObject.SetActive(true);

            _drawCommandCell = Cell.invalid;
        }

        private void DisableDrawTool()
        {
            _tilePalette.gameObject.SetActive(false);

            CancelDrawCommand();
        }

        private CursorType OnDrawGetCursor(Cell cell)
        {
            var tile = _tilePalette.selected;

            // Dont allow drawing with a tile that is on a hidden layer
            if (!IsLayerVisible(tile.layer))
                return CursorType.Not;

            // Dynamic objects can only be placed on top of a static if the static allows it (aka pressure plate)
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

            // If the cell change then update the draw command
            if(!_canvas.isDragging && _drawCommandCell != _cursorCell)
            {
                CancelDrawCommand();

                _drawCommandCell = _cursorCell;

                _drawCommand = CreateDrawCommand(_cursorCell, tile);
                if (_drawCommand != null)
                {
                    _drawCommand.Execute();
                    LightmapManager.Render();
                }
            }

            return CursorType.Crosshair;
        }

        private void OnDrawToolLButtonDown(Vector2 position)
        {
            CancelDrawCommand();
            Draw(false);
        }

        private void OnDrawToolLButtonUp(Vector2 position)
        {
            if(_drawTile != null && !_drawTile.info.allowMultiple)
            {
                mode = Mode.Select;
                SelectTile(_drawTile);
            }
            _drawTile = null;
        }

        private void OnDrawToolDragBegin (Vector2 position)
        {
            var group = _drawCommand != null;
            Debug.Log(group);
            CancelDrawCommand();
            Draw(group);
        }

        private void OnDrawToolDrag(Vector2 position, Vector2 delta) => Draw(true);

        private void OnDrawToolPointerExit() => CancelDrawCommand();

        private void CancelDrawCommand()
        {
            if (_drawCommand == null)
                return;

            _drawCommand.Undo();
            _drawCommand.Destroy();
            _drawCommand = null;

            _drawCommandCell = Cell.invalid;
        }

        private Command CreateDrawCommand (Cell cell, Tile prefab)
        {
            if (!puzzle.grid.IsValid(cell))
                return null;

            // Dont draw if the same exact tile is already there.  This prevents accidental removal 
            // of connections and properties
            var existing = puzzle.grid.CellToTile(cell, prefab.layer);
            if (existing != null && existing.guid == prefab.guid)
                return null;

            var command = new GroupCommand();

            // If this prefab does not allow multiples then destroy all other instances within the puzzle 
            if (!prefab.info.allowMultiple)
            {
                var multipleInstance = puzzle.grid.GetTile(prefab.info);
                if (null != multipleInstance)
                    Erase(multipleInstance, command);

                if (multipleInstance == existing)
                    existing = null;
            }

            if (existing != null)
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
            command.Add(new TileAddCommand(prefab, cell, tile));
            _drawTile = tile;

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

            command.Add(new TileMoveCommand(tile, cell));

            return command;
        }

        private void Draw(bool group)
        {
            if (null == _tilePalette.selected)
                return;

            // Dont allow drawing with a tile that is hidden
            if (!IsLayerVisible(_tilePalette.selected.layer))
                return;

            if (UIManager.cursor != CursorType.Crosshair)
                return;

            ExecuteCommand(CreateDrawCommand(_cursorCell, _tilePalette.selected), group);
        }
    }
}
