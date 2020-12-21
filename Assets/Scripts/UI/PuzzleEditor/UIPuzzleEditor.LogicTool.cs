﻿using System;
using System.Linq;
using UnityEngine;

namespace Puzzled
{
    public partial class UIPuzzleEditor
    {
        private WireMesh dragWire = null;
        private bool logicCycleSelection = false;
        private Tile _selectedTile = null;
        private Wire _selectedWire = null;

        public static Tile selectedTile {
            get => instance._selectedTile;
            set => instance.SelectTile(value);
        }

        public static Wire selectedWire {
            get => instance._selectedWire;
            set => instance.SelectWire(value);
        }

        public static Action<Wire> onSelectedWireChanged;

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
            if (_selectedTile == null || _selectedTile.cell != cell)
            {
                SelectTile(GetTile(cell, TileLayer.Logic));
                logicCycleSelection = false;
            }
            else
                logicCycleSelection = true;
        }

        private void OnLogicLButtonUp(Vector2 position)
        {
            if (null != dragWire || _selectedTile==null || !logicCycleSelection)
                return;

            var cell = canvas.CanvasToCell(position);
            if (_selectedTile.cell != cell)
                return;

            var tile = GetTile(cell, (_selectedTile != null && _selectedTile.info.layer != TileLayer.Floor && _selectedTile.cell == cell) ? ((TileLayer)_selectedTile.info.layer - 1) : TileLayer.Logic);
            if (null == tile && _selectedTile != null)
                tile = GetTile(cell, TileLayer.Logic);

            if (tile != null)
                SelectTile(tile);
            else
                SelectTile(null);
        }

        private void OnLogicLButtonDragBegin(Vector2 position)
        {
            var cell = canvas.CanvasToCell(position);
            if (_selectedTile == null || !_selectedTile.info.allowWireOutputs)
                return;

            dragWire = Instantiate(dragWirePrefab).GetComponent<WireMesh>();
            dragWire.transform.position = TileGrid.CellToWorld(_selectedTile.cell);
            dragWire.target = _selectedTile.cell;
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
            if (target == selectedTile)
                target = null;

            // Find the first tile that accepts input
            while (target != null && !target.info.allowWireInputs && target.info.layer > TileLayer.Floor)
                target = GetTile(cell, (TileLayer)(target.info.layer - 1));

            if (target != null && target.info.allowWireInputs)
                ExecuteCommand(new Editor.Commands.WireAddCommand(_selectedTile, target));

            RefreshInspectorInternal();

            Destroy(dragWire.gameObject);
            dragWire = null;
        }

        private void SelectTile(Cell cell) => SelectTile(GetTile(cell));

        private void SelectTile(Tile tile)
        {
            // Save the inspector state
            if(_selectedTile != null)
                _selectedTile.inspectorState = inspector.GetComponentsInChildren<Editor.IInspectorStateProvider>().Select(p => p.GetState()).ToArray();

            _selectedTile = tile;

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
                RefreshInspectorInternal();
            }

            // Clear wire selection if the selected wire does not connect to the newly selected tile
            if (selectedWire != null && selectedWire.from.tile != tile && selectedWire.to.tile != tile)
                SelectWire(null);
        }

        /// <summary>
        /// Select the given wire
        /// </summary>
        /// <param name="wire">Wire to select</param>
        private void SelectWire(Wire wire)
        {
            // Make sure one of the two tiles from the wire is selected, if not select the input
            if (wire != null && _selectedTile != wire.from.tile && _selectedTile != wire.to.tile)
                SelectTile(wire.from.tile);

            if (_selectedWire != null)
                _selectedWire.selected = false;

            _selectedWire = wire;

            if (_selectedWire != null)
                _selectedWire.selected = true;

            onSelectedWireChanged?.Invoke(_selectedWire);
        }


        public static void RefreshInspector() => instance.RefreshInspectorInternal();

        private void RefreshInspectorInternal()
        {
            var tile = _selectedTile;
            options.DetachAndDestroyChildren();

            if (tile.info.allowWireInputs)
                Instantiate(tile.info.inputsPrefab != null ? tile.info.inputsPrefab : optionInputsPrefab.gameObject, options).GetComponentInChildren<UIOptionEditor>().target = tile;

            if (tile.info.allowWireOutputs)
                Instantiate(tile.info.outputsPrefab != null ? tile.info.outputsPrefab : optionOutputsPrefab.gameObject, options).GetComponentInChildren<UIOptionEditor>().target = tile;

            if (tile.info.customOptionEditors != null)
                foreach (var editor in tile.info.customOptionEditors)
                    Instantiate(editor.prefab, options).GetComponent<UIOptionEditor>().target = tile;

            Transform properties = null;
            foreach (var tileProperty in tile.properties)
            {
                // Skip hidden properties
                if (tileProperty.editable.hidden)
                    continue;

                if (properties == null)
                    properties = Instantiate(optionPropertiesPrefab, options).transform.Find("Content");

                var optionEditor = InstantiateOptionEditor(tileProperty.property.PropertyType, properties);
                if (null == optionEditor)
                    continue;

                optionEditor.target = new TilePropertyOption(tile, tileProperty);
            }

            // Apply the saved inspector state
            if (_selectedTile.inspectorState != null)
                foreach (var state in _selectedTile.inspectorState)
                    state.Apply(inspector.transform);
        }
    }
}
