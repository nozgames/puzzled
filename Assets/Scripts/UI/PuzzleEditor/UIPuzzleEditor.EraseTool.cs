﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Puzzled.Editor
{
    public partial class UIPuzzleEditor
    {
        [Flags]
        private enum EraseFlags
        {
            None = 0,
            KeepWallMount = 1
        }

        private Cell _lastEraseCell;
        private TileLayer _eraseLayer = TileLayer.Static;
        private bool _eraseLayerOnly;
        private bool _eraseStarted;

        private void EnableEraseTool()
        {
            _canvas.onLButtonDown = OnEraseToolLButtonDown;
            _canvas.onLButtonUp = OnEraseToolLButtonUp;
            _canvas.onLButtonDrag = OnEraseToolDrag;

            _getCursor = OnEraseGetCursor;
            _onKeyModifiers = OnModifiersChangedErase;

            _lastEraseCell = Cell.invalid;
        }

        private void DisableEraseTool()
        {
        }

        private void OnModifiersChangedErase(bool shift, bool ctrl, bool alt)
        {
        }

        private void OnEraseToolLButtonDown(Vector2 position)
        {
            var tile = GetTopMostTile(_cursorCell);
            _eraseLayerOnly = tile != null;
            _eraseLayer = _eraseLayerOnly ? tile.layer : TileLayer.InvisibleStatic;
            _eraseStarted = false;
            Erase(_cursorCell);
        }

        private void OnEraseToolLButtonUp(Vector2 position)
        {
            _lastEraseCell = Cell.invalid;
            _eraseLayerOnly = false;            
        }

        private void OnEraseToolDrag(Vector2 position, Vector2 delta) => Erase(_cursorCell);

        private void Erase(Cell cell)
        {
            if (_lastEraseCell == cell)
                return;

            _lastEraseCell = cell;

            var tile = _eraseLayerOnly ? GetTopMostTile(cell,_eraseLayer) : GetTopMostTile(cell);
            if (null == tile)
                return;

            if (_eraseLayerOnly && tile.layer != _eraseLayer)
                return;

            ExecuteCommand(Erase(tile), _eraseStarted);
            _eraseStarted = true;
        }

        private Commands.GroupCommand Erase(Tile[] tiles, Commands.GroupCommand group = null)
        {
            if (null == tiles || tiles.Length == 0)
                return null;

            if (null == group)
                group = new Commands.GroupCommand();

            // Ensure there are no duplicates
            var eraseTiles = tiles.Distinct().ToList();

            // Search through the tiles being erased and see if there are additional tiles that need to be erased as well
            foreach (var tile in eraseTiles)
            {
                // Add any wall mounts to walls being erased.
                if (tile.layer == TileLayer.Wall)
                {
                    var wall = tile.GetComponent<Wall>();
                    if (null != wall)
                        foreach (var mountedTile in wall.GetMountedTiles())
                        {
                            if (!eraseTiles.Contains(mountedTile))
                                eraseTiles.Add(mountedTile);
                        }
                }
            }

            foreach (var eraseTile in eraseTiles)
                Erase(eraseTile, group, EraseFlags.KeepWallMount);

            return group;
        }

        private Commands.GroupCommand Erase (Tile tile, Commands.GroupCommand group = null, EraseFlags flags = EraseFlags.None)
        {
            if (null == group)
                group = new Commands.GroupCommand();

            // If a wall layer is being erased and we were not told to keep the wall mounts then remove all wall mounts
            if(tile.layer == TileLayer.Wall && (flags & EraseFlags.KeepWallMount) != EraseFlags.KeepWallMount)
            {
                var wall = tile.GetComponent<Wall>();
                if (null != wall)
                    foreach (var mounted in wall.GetMountedTiles())
                        Erase(mounted, group);
            }

            // Destroy all wires connected to the tile
            foreach (var property in tile.properties)
                if (property.type == TilePropertyType.Port)
                {
                    var port = property.GetValue<Port>(tile);
                    //if (port.flow != PortFlow.Output)
                      //  continue;

                    foreach (var wire in port.wires)
                        group.Add(new Editor.Commands.WireDestroyCommand(wire));
                }

            group.Add(new Editor.Commands.TileDestroyCommand(tile));

            return group;
        }

        private CursorType OnEraseGetCursor(Cell arg) => CursorType.Crosshair;
    }
}
