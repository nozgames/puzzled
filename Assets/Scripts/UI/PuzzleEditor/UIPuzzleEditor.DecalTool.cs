﻿using UnityEngine;

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

            if(KeyboardManager.isAltPressed)
                return CursorType.EyeDropper;

            var tile = GetTile(cell);
            if (null == tile)
                return CursorType.ArrowWithNot;

            var property = tile.GetProperty("decal");
            if (null == property)
                return CursorType.ArrowWithNot;

            var decal = _decalPalette.selected;
            if(null == decal || decal.sprite == null || KeyboardManager.isCtrlPressed)
                return CursorType.ArrowWithMinus;

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
            if(!_allowDecalDrag)
            {
                var surface = DecalSurface.FromCell(puzzle, cell);
                if (surface != null)
                    _decalPalette.selected = surface.decal;
                return;
            }

            var tile = GetTile(cell);
            if (null == tile)
                return;

            var property = tile.GetProperty("decal");
            if (null == property)
                return;

            var decal = KeyboardManager.isCtrlPressed ? Decal.none : _decalPalette.selected;
            if (property.GetValue<Decal>(tile) == decal)
                return;

            ExecuteCommand(new Editor.Commands.TileSetPropertyCommand(tile, "decal", decal));
        }
    }
}
