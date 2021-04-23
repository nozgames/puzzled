using UnityEngine;

using Puzzled.Editor;

namespace Puzzled
{
    public partial class UIPuzzleEditor
    {
        [Header("DecalTool")]
        [SerializeField] private UIDecalPalette _decalPalette = null;

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

            var decalSurfaces = DecalSurface.FromCell(instance._puzzle, cell);
            if (null == decalSurfaces || decalSurfaces.Length > 1)
                return CursorType.ArrowWithNot;

            if(KeyboardManager.isCtrlPressed)
            {
                if (decalSurfaces[0].decal == Decal.none)
                    return CursorType.ArrowWithNot;

                return CursorType.ArrowWithMinus;
            }

            // Do not allow decals to be set on the floor tile if there is a static tile in the same cell
            if (decalSurfaces[0].tile.layer == TileLayer.Floor && instance._puzzle.grid.CellToTile(cell, TileLayer.Static) != null)
                return CursorType.ArrowWithNot;

            // Do not allow decals to be set on a wall tile if there is a wall static in the same cell
            if (decalSurfaces[0].tile.layer == TileLayer.Wall && instance._puzzle.grid.CellToTile(cell, TileLayer.WallStatic) != null)
                return CursorType.ArrowWithNot;

            return CursorType.ArrowWithPlus;
        }

        private void OnDecalToolLButtonDown(Vector2 position) => DrawDecal(position);

        private void OnDecalToolDrag(Vector2 position, Vector2 delta) => DrawDecal(position);

        private void DrawDecal(Vector2 position) =>
            SetDecal(canvas.CanvasToCell(position), KeyboardManager.isCtrlPressed ? Decal.none : _decalPalette.selected);

        /// <summary>
        /// Set a decal on a tile using undo/redo
        /// </summary>
        /// <param name="tile">Tile to set decal on</param>
        /// <param name="tileProperty">Tile property to set</param>
        /// <param name="decal">Decal to set</param>
        /// <returns>True if the decal was set</returns>
        public static bool SetDecal(Tile tile, TileProperty tileProperty, Decal decal)
        {
            // If the decal is an deep match then there is nothing to do
            if (decal.Equals(tileProperty.GetValue<Decal>(tile), true))
                return true;

            var command = new Editor.Commands.GroupCommand();
            command.Add(new Editor.Commands.TileSetPropertyCommand(tile, tileProperty.name, decal));

            // If the decal is being cleared then check to see if there is a decal light and if 
            // so remove all wires from it if this was the last decal.
            if (decal == Decal.none)
            {
                var decalLight = tile.GetComponent<DecalLight>();
                if(decalLight != null && decalLight.decalCount < 2)
                {
                    foreach (var wire in decalLight.decalPowerPort.wires)
                        command.Add(new Editor.Commands.WireDestroyCommand(wire));
                }
            }

            ExecuteCommand(command);

            return true;
        }

        /// <summary>
        /// Set the top most tile with a "decal" property to the given decal
        /// </summary>
        /// <param name="cell"></param>
        /// <param name="decal"></param>
        /// <returns></returns>
        public static bool SetDecal(Cell cell, Decal decal)
        {
            var tile = instance.GetTopMostTileWithProperty(cell, "decal");
            if (null == tile)
                return false;

            var tileProperty = tile.GetProperty("decal");
            if (null == tileProperty)
                return false;

            return SetDecal(tile, tileProperty, decal);
        }
    }
}
