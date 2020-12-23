﻿using UnityEngine;

namespace Puzzled
{
    public partial class UIPuzzleEditor
    {
        private Cell _lastEraseCell;
        private TileLayer _eraseLayer = TileLayer.Static;
        private bool _eraseLayerOnly;
        private bool _eraseStarted;

        private void EnableEraseTool()
        {
            canvas.onLButtonDown = OnEraseToolLButtonDown;
            canvas.onLButtonUp = OnEraseToolLButtonUp;
            canvas.onLButtonDrag = OnEraseToolDrag;

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
            var cell = canvas.CanvasToCell(position);
            var tile = GetTile(cell);
            _eraseLayerOnly = !eraseToolAllLayers.isOn && tile != null;
            _eraseLayer = tile != null ? tile.info.layer : TileLayer.Logic;
            _eraseStarted = false;

            Erase(cell);
        }

        private void OnEraseToolLButtonUp(Vector2 position)
        {
            _lastEraseCell = Cell.invalid;
        }

        private void OnEraseToolDrag(Vector2 position, Vector2 delta) => Erase(canvas.CanvasToCell(position));

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

                    ExecuteCommand(new Editor.Commands.TileDestroyCommand(tile), _eraseStarted);
                    _eraseStarted = true;
                }
            } 
            else
            {
                var tile = _eraseLayerOnly ? GetTile(cell,_eraseLayer) : GetTile(cell);
                if (null == tile)
                    return;

                if (_eraseLayerOnly && tile.info.layer != _eraseLayer)
                    return;

                ExecuteCommand(new Editor.Commands.TileDestroyCommand(tile), _eraseStarted);
            }
        }
    }
}
