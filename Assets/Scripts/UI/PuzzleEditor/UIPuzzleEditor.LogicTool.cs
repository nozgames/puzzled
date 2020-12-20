using System;
using UnityEngine;

namespace Puzzled
{
    public partial class UIPuzzleEditor
    {
        private WireMesh dragWire = null;
        private bool logicCycleSelection = false;

        private void EnableLogicTool()
        {
            canvas.onLButtonDown = OnLogicLButtonDown;
            canvas.onLButtonUp = OnLogicLButtonUp;
            canvas.onLButtonDragBegin = OnLogicLButtonDragBegin;
            canvas.onLButtonDrag = OnLogicLButtonDrag;
            canvas.onLButtonDragEnd = OnLogicLButtonDragEnd;

            inspector.SetActive(true);

            logicCycleSelection = false;
        }

        private void DisableLogicTool()
        {
            SelectTile(null);
            GameManager.ShowWires(false);
        }

        private void OnLogicLButtonDown(Vector2 position)
        {
            // Ensure the cell being dragged is the selected cell
            var cell = canvas.CanvasToCell(position);
            if (selection == null || selection.cell != cell)
            {
                SelectTile(GetTile(cell, TileLayer.Logic));
                logicCycleSelection = false;
            }
            else
                logicCycleSelection = true;
        }

        private void OnLogicLButtonUp(Vector2 position)
        {
            if (null != dragWire || selection==null || !logicCycleSelection)
                return;

            var cell = canvas.CanvasToCell(position);
            if (selection.cell != cell)
                return;

            var tile = GetTile(cell, (selection != null && selection.info.layer != TileLayer.Floor && selection.cell == cell) ? ((TileLayer)selection.info.layer - 1) : TileLayer.Logic);
            if (null == tile && selection != null)
                tile = GetTile(cell, TileLayer.Logic);

            if (tile != null)
                SelectTile(tile);
            else
                SelectTile(null);
        }

        private void OnLogicLButtonDragBegin(Vector2 position)
        {
            var cell = canvas.CanvasToCell(position);
            if (selection == null || !selection.info.allowWireOutputs)
                return;

            dragWire = Instantiate(dragWirePrefab).GetComponent<WireMesh>();
            dragWire.transform.position = TileGrid.CellToWorld(selection.cell);
            dragWire.target = selection.cell;
        }

        private void OnLogicLButtonDrag(Vector2 position, Vector2 delta)
        {
            if (null == dragWire)
                return;

            dragWire.target = canvas.CanvasToCell(position);
        }

        private void OnLogicLButtonDragEnd(Vector2 position)
        {
            if (null == dragWire)
                return;

            var cell = canvas.CanvasToCell(position);
            var target = GetTile(cell);

            // Find the first tile that accepts input
            while (target != null && !target.info.allowWireInputs && target.info.layer > TileLayer.Floor)
                target = GetTile(cell, (TileLayer)(target.info.layer - 1));

            if (target != null && target.info.allowWireInputs)
            {
                var wire = GameManager.InstantiateWire(selection, target);
                if (wire != null)
                    wire.visible = true;
            }

            UpdateInspector(selection);

            Destroy(dragWire.gameObject);
            dragWire = null;
        }

        private void SelectTile(Cell cell) => SelectTile(GetTile(cell));

        private void SelectTile(Tile tile)
        {
            if(selection != null)
            {
                // Deselect all input wires
                foreach (var input in selection.inputs)
                    input.selected = false;

                // Deselect all output wires
                foreach (var output in selection.outputs)
                    output.selected = false;
            }

            selection = tile;

            if (tile == null)
            {
                selectionRect.gameObject.SetActive(false);
                options.DetachAndDestroyChildren();
                inspectorContent.SetActive(false);

                GameManager.ShowWires(true);
            } 
            else
            {
                inspectorContent.SetActive(true);
                inspectorTileName.text = tile.info.displayName;
                inspectorTilePreview.texture = TileDatabase.GetPreview(tile.guid);
                SetSelectionRect(tile.cell, tile.cell);
                GameManager.HideWires();
                GameManager.ShowWires(tile);
                UpdateInspector(tile);
            }
        }
    }
}
