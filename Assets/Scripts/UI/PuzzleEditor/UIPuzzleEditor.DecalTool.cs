using UnityEngine;

namespace Puzzled
{
    public partial class UIPuzzleEditor
    {
        private int _lastSelectedDecal = -1;

        private void EnableDecalTool()
        {
            canvas.onLButtonDown = OnDecalToolLButtonDown;
            canvas.onLButtonDrag = OnDecalToolDrag;

            var first = FilterPalette(typeof(UIDecalItem));
            palette.SetActive(true);
            _paletteList.Select(_lastSelectedDecal == -1 ? first : _lastSelectedDecal);
        }

        private void DisableDecalTool()
        {
            _lastSelectedDecal = _paletteList.selected;
        }

        private void OnDecalToolLButtonDown(Vector2 position) => DrawDecal(position);

        private void OnDecalToolDrag(Vector2 position, Vector2 delta) => DrawDecal(position);

        private void DrawDecal(Vector2 position)
        {
            var cell = canvas.CanvasToCell(position);

            var tile = GetTile(cell);
            if (null == tile)
                return;

            var property = tile.GetProperty("decal");
            if (null == property)
                return;

            var decal = _paletteList.GetItem(_paletteList.selected).GetComponent<UIDecalItem>().decal;
            if (property.GetValueDecal(tile) == decal)
                return;

            ExecuteCommand(new Editor.Commands.TileSetPropertyCommand(tile, "decal", decal));
        }
    }
}
