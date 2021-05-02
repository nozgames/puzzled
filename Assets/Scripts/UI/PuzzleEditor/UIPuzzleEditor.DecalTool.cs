using System.Linq;
using UnityEngine;

using Puzzled.Editor;

namespace Puzzled.Editor
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

#if false
            if (KeyboardManager.isAltPressed)
                return CursorType.EyeDropper;
#endif

            if(instance.GetTopMostTileWithPropertyType(cell, TilePropertyType.Decal) == null)
                return CursorType.ArrowWithNot;

            if(KeyboardManager.isCtrlPressed)
                return CursorType.ArrowWithMinus;

#if false
            // Do not allow decals to be set on the floor tile if there is a static tile in the same cell
            if (decalSurfaces[0].tile.layer == TileLayer.Floor && instance._puzzle.grid.CellToTile(cell, TileLayer.Static) != null)
                return CursorType.ArrowWithNot;

            // Do not allow decals to be set on a wall tile if there is a wall static in the same cell
            if (decalSurfaces[0].tile.layer == TileLayer.Wall && instance._puzzle.grid.CellToTile(cell, TileLayer.WallStatic) != null)
                return CursorType.ArrowWithNot;
#endif

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
        private static void SetDecal(Tile tile, TileProperty tileProperty, Decal decal, Editor.Commands.GroupCommand command)
        {
            // If the decal is an deep match then there is nothing to do
            if (decal.Equals(tileProperty.GetValue<Decal>(tile), true))
                return;

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
        }

        /// <summary>
        /// Set the top most tile with a "decal" property to the given decal
        /// </summary>
        /// <param name="cell"></param>
        /// <param name="decal"></param>
        /// <returns></returns>
        public static bool SetDecal(Cell cell, Decal decal)
        {
            var tile = instance.GetTopMostTileWithPropertyType(cell, TilePropertyType.Decal);
            if (null == tile)
                return false;

            var command = new Editor.Commands.GroupCommand();
            foreach(var property in tile.properties.Where(p => p.type == TilePropertyType.Decal))
                SetDecal(tile, property, decal, command);

            if (!command.hasCommands)
                return false;

            ExecuteCommand(command);

            return true;
        }
    }
}
