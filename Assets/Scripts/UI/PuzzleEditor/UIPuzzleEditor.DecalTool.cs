using UnityEngine;

namespace Puzzled
{
    public partial class UIPuzzleEditor
    {
        private void EnableDecalTool()
        {
            canvas.onLButtonDown = OnDecalToolLButtonDown;
            canvas.onLButtonDrag = OnDecalToolDrag;

            FilterPalette(typeof(UIDecalItem));
            palette.SetActive(true);
        }

        private void DisableDecalTool()
        {
        }

        private void OnDecalToolLButtonDown(Vector2 position) => DrawDecal(position);

        private void OnDecalToolDrag(Vector2 position, Vector2 delta) => DrawDecal(position);

        private void DrawDecal(Vector2 position)
        {
            if (null == drawTile)
                return;

            var cell = canvas.CanvasToCell(position);

            var tile = GetTile(cell);
            if (null == tile)
                return;

            var surface = tile.GetComponentInChildren<DecalSurface>();
            if (null == surface)
                return;

            surface.decal = _paletteList.GetItem(_paletteList.selected).GetComponent<UIDecalItem>().decal;
        }
    }
}
