using UnityEngine;

using Puzzled.Editor;

namespace Puzzled
{
    public partial class UIPuzzleEditor
    {
        [Header("DecalTool")]
        [SerializeField] private UIDecalPalette _decalPalette = null;

        private bool _allowDecalDrag = false;

        private void EnableDecalTool()
        {
            canvas.onLButtonDown = OnDecalToolLButtonDown;
            canvas.onLButtonDrag = OnDecalToolDrag;

            _getCursor = OnDecalGetCursor;

            _decalPalette.gameObject.SetActive(true);
        }

        private void DisableDecalTool()
        {
            _decalPalette.gameObject.SetActive(false);
        }

        private CursorType OnDecalGetCursor(Cell cell)
        {
            if (cell == Cell.invalid)
                return CursorType.Arrow;

            if (KeyboardManager.isAltPressed)
                return CursorType.EyeDropper;

            var decalSurface = DecalSurface.FromCell(instance._puzzle, cell);
            if (null == decalSurface)
                return CursorType.ArrowWithNot;

            if(KeyboardManager.isCtrlPressed)
            {
                if (decalSurface.decal == Decal.none)
                    return CursorType.ArrowWithNot;

                return CursorType.ArrowWithMinus;
            }

            if (decalSurface.tile.layer == TileLayer.Floor && instance._puzzle.grid.CellToTile(cell, TileLayer.Static) != null)
                return CursorType.ArrowWithNot;

            return CursorType.ArrowWithPlus;
        }

        private void OnDecalToolLButtonDown(Vector2 position) => DrawDecal(position);

        private void OnDecalToolDrag(Vector2 position, Vector2 delta)
        {
            if (!_allowDecalDrag)
                return;

            DrawDecal(position);
        }

        private void DrawDecal(Vector2 position)
        {
            var cell = canvas.CanvasToCell(position);

            _allowDecalDrag = !KeyboardManager.isAltPressed;
            if (!_allowDecalDrag)
            {
                var surface = DecalSurface.FromCell(puzzle, cell);
                if (surface != null)
                    _decalPalette.selected = surface.decal;
                return;
            }

            SetDecal(cell, KeyboardManager.isCtrlPressed ? Decal.none : _decalPalette.selected);
        }

        public static bool SetDecal(Cell cell, Decal decal)
        {
            var surface = DecalSurface.FromCell(instance.puzzle, cell);
            if (surface == null)
                return false;

            if (surface.tile.layer == TileLayer.Floor && instance._puzzle.grid.CellToTile(cell, TileLayer.Static) != null)
                return false;

            if (surface.decal == decal && surface.decal.flags == decal.flags)
                return true;

            var command = new Editor.Commands.GroupCommand();
            command.Add(new Editor.Commands.TileSetPropertyCommand(surface.tile, "decal", decal));

            // If the decal is being erased then we need to also destroy any wires.
            if (decal == Decal.none)
                foreach (var wire in surface.decalPowerPort.wires)
                    command.Add(new Editor.Commands.WireDestroyCommand(wire));

            ExecuteCommand(command);

            return true;
        }
    }
}
