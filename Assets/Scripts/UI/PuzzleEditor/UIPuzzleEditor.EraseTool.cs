using UnityEngine;

namespace Puzzled
{
    public partial class UIPuzzleEditor
    {
        private Cell _lastEraseCell;

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

        private void OnEraseToolLButtonDown(Vector2 position) => Erase(canvas.CanvasToCell(position));

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

            // TODO: respect visible layers?
            if (eraseToolAllLayers.isOn)
                TileGrid.UnlinkTiles(cell, true);
            else
                TileGrid.UnlinkTile(GetTile(cell), true);
        }
    }
}
