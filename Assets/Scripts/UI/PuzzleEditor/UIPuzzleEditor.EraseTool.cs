using System;
using UnityEngine;

namespace Puzzled
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
            canvas.onLButtonDown = OnEraseToolLButtonDown;
            canvas.onLButtonUp = OnEraseToolLButtonUp;
            canvas.onLButtonDrag = OnEraseToolDrag;

            _getCursor = OnEraseGetCursor;

            eraseToolOptions.SetActive(true);

            _lastEraseCell = Cell.invalid;
        }

        private void DisableEraseTool()
        {
            eraseToolOptions.SetActive(false);
        }

        private void OnModifiersChangedErase(bool shift, bool ctrl, bool alt)
        {
        }

        private void OnEraseToolLButtonDown(Vector2 position)
        {
            var tile = GetTile(_cursorCell);
            _eraseLayerOnly = !eraseToolAllLayers.isOn && tile != null;
            _eraseLayer = _eraseLayerOnly ? tile.layer : TileLayer.Logic;
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

            if (eraseToolAllLayers.isOn)
            {
                for(int layer = (int)TileLayer.Logic; layer >= (int)TileLayer.Floor; layer --)
                {
                    var tile = GetTile(cell, (TileLayer)layer);
                    if (null == tile)
                        continue;

                    ExecuteCommand(Erase(tile), _eraseStarted);
                    _eraseStarted = true;
                }
            } 
            else
            {
                var tile = _eraseLayerOnly ? GetTile(cell,_eraseLayer) : GetTile(cell);
                if (null == tile)
                    return;

                if (_eraseLayerOnly && tile.layer != _eraseLayer)
                    return;

                ExecuteCommand(Erase(tile), _eraseStarted);
                _eraseStarted = true;
            }
        }

        private Editor.Commands.GroupCommand Erase (Tile tile, Editor.Commands.GroupCommand group = null, EraseFlags flags = EraseFlags.None)
        {
            if (null == group)
                group = new Editor.Commands.GroupCommand();

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
                    foreach (var wire in property.GetValue<Port>(tile).wires)
                        group.Add(new Editor.Commands.WireDestroyCommand(wire));

            group.Add(new Editor.Commands.TileDestroyCommand(tile));

            return group;
        }

        private CursorType OnEraseGetCursor(Cell arg) => CursorType.Crosshair;
    }
}
