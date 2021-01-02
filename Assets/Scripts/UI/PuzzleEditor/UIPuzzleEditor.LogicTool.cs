using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Puzzled
{
    public partial class UIPuzzleEditor
    {
        [Header("Inspector")]
        [SerializeField] private GameObject inspectorContent = null;
        [SerializeField] private TMPro.TMP_InputField inspectorTileName = null;
        [SerializeField] private RawImage inspectorTilePreview = null;
        [SerializeField] private UIOptionEditor optionPrefabInt = null;
        [SerializeField] private UIOptionEditor optionPrefabBool = null;
        [SerializeField] private UIOptionEditor optionPrefabBackground = null;
        [SerializeField] private UIOptionEditor optionPrefabDecal = null;
        [SerializeField] private UIOptionEditor optionPrefabDecalArray = null;
        [SerializeField] private UIOptionEditor optionPrefabIntArray = null;
        [SerializeField] private UIOptionEditor optionPrefabString = null;
        [SerializeField] private UIOptionEditor optionPrefabStringMultiline = null;
        [SerializeField] private UIOptionEditor optionPrefabTile = null;
        [SerializeField] private UIOptionEditor optionPrefabPort = null;
        [SerializeField] private GameObject optionPropertiesPrefab = null;

        private WireMesh dragWire = null;
        private bool logicCycleSelection = false;
        private Tile _selectedTile = null;
        private Wire _selectedWire = null;
        private bool _allowLogicDrag = false;

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

            _onKey = OnLogicKey;
            _getCursor = OnLogicGetCursor;

            inspector.SetActive(true);

            logicCycleSelection = false;

            // Show all wires when logic tool is enabled
            _puzzle.ShowWires(true);
        }

        private void DisableLogicTool()
        {
            SelectTile(null);

            // Hide all wires when logic tool is hidden
            _puzzle.ShowWires(false);
        }

        private void OnLogicLButtonDown(Vector2 position)
        {
            // Ensure the cell being dragged is the selected cell
            var cell = canvas.CanvasToCell(position);

            var group = new Editor.Commands.GroupCommand();
            if (_selectedTile != null && KeyboardManager.isShiftPressed)
            {
                _allowLogicDrag = false;

                if (_selectedTile.cell == cell || !_selectedTile.hasOutputs)
                    return;

                // Destroy all all linked wires
                foreach(var property in _selectedTile.properties)
                    if(property.type == TilePropertyType.Port)
                    {
                        var port = property.GetValue<Port>(selectedTile);
                        foreach(var wire in port.wires)
                        {
                            var connection = wire.GetOppositeConnection(port);
                            if (connection.cell == cell && layerToggles[(int)connection.tile.info.layer].isOn)
                                group.Add(new Editor.Commands.WireDestroyCommand(wire));
                        }
                    }

                if(!group.hasCommands)
                {
                    var target = GetTile(cell, GetTileFlag.AllowInputs);
                    if(null != target)
                        group.Add(new Editor.Commands.WireAddCommand(selectedTile.GetLegacyPort(PortFlow.Output), target.GetLegacyPort(PortFlow.Input)));
                }

                if (group.hasCommands)
                    ExecuteCommand(group);

                UpdateCursor();

                return;
            }

            if (_selectedTile == null || _selectedTile.cell != cell)
            {
                SelectTile(GetTile(cell, TileLayer.Logic));
                logicCycleSelection = false;
            }
            else
                logicCycleSelection = true;

            _allowLogicDrag = selectedTile != null && _selectedTile.hasOutputs;
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
            if (!_allowLogicDrag)
                return;

            var cell = canvas.CanvasToCell(position);
            if (_selectedTile == null || !_selectedTile.hasOutputs)
                return;

            dragWire = Instantiate(dragWirePrefab).GetComponent<WireMesh>();
            dragWire.state = WireVisualState.Selected;
            dragWire.transform.position = puzzle.grid.CellToWorld(_selectedTile.cell);
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
            while (target != null && !target.hasInputs && target.info.layer > TileLayer.Floor)
                target = GetTile(cell, (TileLayer)(target.info.layer - 1));

            if (target != null && target.hasInputs)
                ExecuteCommand(new Editor.Commands.WireAddCommand(_selectedTile.GetLegacyPort(PortFlow.Output), target.GetLegacyPort(PortFlow.Input)));

            RefreshInspectorInternal();

            Destroy(dragWire.gameObject);
            dragWire = null;
        }

        private void SelectTile(Cell cell) => SelectTile(GetTile(cell));

        private void UpdateInspectorState(Tile tile)
        {
            tile.inspectorState = inspector.GetComponentsInChildren<Editor.IInspectorStateProvider>().Select(p => p.GetState()).ToArray();
        }

        private void SetWiresDark (Tile tile, bool dark)
        {
            foreach (var property in tile.properties)
                if (property.type == TilePropertyType.Port)
                    foreach (var wire in property.GetValue<Port>(tile).wires)
                        wire.dark = dark;
        }

        private void SelectTile(Tile tile)
        {
            if (tile == null && _selectedTile == null)
                return;

            // Save the inspector state
            if (_selectedTile != null)
            {
                UpdateInspectorState(_selectedTile);
                SetWiresDark(tile, false);
            }

            _selectedTile = tile;

            if (tile == null)
            {
                selectionRect.gameObject.SetActive(false);
                options.DetachAndDestroyChildren();
                inspectorContent.SetActive(false);

                // Show all wires when no tile is selected
                _puzzle.ShowWires();
            } 
            else
            {
                inspectorContent.SetActive(true);
                inspectorTileName.text = tile.info.displayName;
                inspectorTilePreview.texture = TileDatabase.GetPreview(tile.guid);
                SetSelectionRect(tile.cell, tile.cell);

                // Hide all wires in case they were all visible previously and show the selected tiles wires
                _puzzle.HideWires();
                _puzzle.ShowWires(tile);
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

            Transform properties = null;
            foreach (var tileProperty in tile.properties)
            {
                // Skip hidden properties
                if (tileProperty.editable.hidden)
                    continue;

                if (properties == null)
                    properties = Instantiate(optionPropertiesPrefab, options).transform.Find("Content");

                var optionEditor = InstantiatePropertyEditor(tile, tileProperty, properties);
                if (null == optionEditor)
                    continue;

                optionEditor.target = new TilePropertyEditorTarget(tile, tileProperty);
            }

            // Apply the saved inspector state
            if (_selectedTile.inspectorState != null)
                foreach (var state in _selectedTile.inspectorState)
                    state.Apply(inspector.transform);
        }

        /// <summary>
        /// Instantiate a property editor for the given property
        /// </summary>
        /// <param name="tile">Tile that owns the property</param>
        /// <param name="property">Property</param>
        /// <param name="parent">Parent transform to instantiate in to</param>
        /// <returns>The instantiated editor</returns>
        private UIOptionEditor InstantiatePropertyEditor(Tile tile, TileProperty property, Transform parent)
        {
            var prefab = tile.info.GetCustomPropertyEditor(property)?.prefab ?? null;
            if(null == prefab)
                switch (property.type)
                {
                    case TilePropertyType.String:
                        prefab = property.editable.multiline ? optionPrefabStringMultiline : optionPrefabString;
                        break;

                    case TilePropertyType.Int:
                        prefab = optionPrefabInt;
                        break;

                    case TilePropertyType.Bool:
                        prefab = optionPrefabBool;
                        break;

                    case TilePropertyType.Background:
                        prefab = optionPrefabBackground;
                        break;

                    // TODO: change to tile
                    case TilePropertyType.Guid:
                        prefab = optionPrefabTile;
                        break;

                    case TilePropertyType.Decal:
                        prefab = optionPrefabDecal;
                        break;

                    case TilePropertyType.DecalArray:
                        prefab = optionPrefabDecalArray;
                        break;

                    case TilePropertyType.IntArray:
                        prefab = optionPrefabIntArray;
                        break;

                    default:
                        return null;
                }

            return Instantiate(prefab.gameObject, parent).GetComponent<UIOptionEditor>();
        }

        private void OnLogicKey(KeyCode keyCode)
        {
            switch (keyCode)
            {
                case KeyCode.Delete:
                    if(selectedTile != null)
                        ExecuteCommand(new Editor.Commands.TileDestroyCommand(selectedTile));
                    break;
            }
        }

        private CursorType OnLogicGetCursor(Cell cell)
        {
            if (KeyboardManager.isShiftPressed && selectedTile != null)
            {
                if (cell == selectedTile.cell || !selectedTile.hasOutputs)
                    return CursorType.ArrowWithNot;

                foreach (var property in _selectedTile.properties)
                    if (property.type == TilePropertyType.Port)
                    {
                        var port = property.GetValue<Port>(selectedTile);
                        foreach (var wire in port.wires)
                        {
                            var connection = wire.GetOppositeConnection(port);
                            if (connection.cell == cell && layerToggles[(int)connection.tile.info.layer].isOn)
                                return CursorType.ArrowWithMinus;
                        }
                    }

                return GetTile(cell, GetTileFlag.AllowInputs) != null ? CursorType.ArrowWithPlus : CursorType.ArrowWithNot;
            }

            return CursorType.Arrow;
        }

    }
}
