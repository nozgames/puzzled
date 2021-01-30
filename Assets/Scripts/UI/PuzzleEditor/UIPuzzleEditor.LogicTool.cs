using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Puzzled.UI;

namespace Puzzled
{
    public partial class UIPuzzleEditor
    {
        [Header("Inspector")]
        [SerializeField] private GameObject _inspectorContent = null;
        [SerializeField] private GameObject _inspectorEmpty = null;
        [SerializeField] private GameObject _inspectorHeader = null;
        [SerializeField] private TMPro.TMP_InputField inspectorTileName = null;
        [SerializeField] private TMPro.TextMeshProUGUI _inspectorTileType = null;
        [SerializeField] private RawImage _inspectorTilePreview = null;
        [SerializeField] private UIPropertyEditor backgroundEditorPrefab = null;
        [SerializeField] private UIPropertyEditor boolEditorPrefab = null;
        [SerializeField] private UIPropertyEditor decalEditorPrefab = null;
        [SerializeField] private UIPropertyEditor decalArrayEditorPrefab = null;
        [SerializeField] private UIPropertyEditor numberEditorPrefab = null;
        [SerializeField] private UIPropertyEditor numberRangeEditorPrefab = null;
        [SerializeField] private UIPropertyEditor cellEditorPrefab = null;
        [SerializeField] private UIPropertyEditor numberArrayEditorPrefab = null;
        [SerializeField] private UIPropertyEditor portEditorPrefab = null;
        [SerializeField] private UIPropertyEditor portEmptyEditorPrefab = null;
        [SerializeField] private UIPropertyEditor stringEditorPrefab = null;
        [SerializeField] private UIPropertyEditor stringMultilineEditorPrefab = null;
        [SerializeField] private UIPropertyEditor stringArrayEditorPrefab = null;
        [SerializeField] private UIPropertyEditor multilineStringArrayEditorPrefab = null;
        [SerializeField] private UIPropertyEditor soundEditorPrefab = null;
        [SerializeField] private UIPropertyEditor tileEditorPrefab = null;
        [SerializeField] private GameObject optionPropertiesPrefab = null;
        [SerializeField] private UIRadio _inspectorFlip = null;
        [SerializeField] private UIRadio _inspectorRotate = null;

        private WireVisuals dragWire = null;
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

        private void OnInspectorTileNameChanged(string name)
        {
            if (_selectedTile == null)
                return;

            name = name.Trim();

            if (string.IsNullOrWhiteSpace(name))
            {
                inspectorTileName.SetTextWithoutNotify(_selectedTile.name);
                return;
            }

            if (name == _selectedTile.name)
                return;

            ExecuteCommand(new Editor.Commands.TileRenameCommand(_selectedTile, name));
        }

        private void OnLogicLButtonDown(Vector2 position)
        {
            // Ensure the cell being dragged is the selected cell
            var cell = canvas.CanvasToCell(position);

            _allowLogicDrag = false;

            // QuickConnect mode
            if (_selectedTile != null && KeyboardManager.isShiftPressed)
            {
                Connect(_selectedTile, cell);
                return;
            }

            if (_selectedTile != null && KeyboardManager.isCtrlPressed)
            {
                Disconnect(_selectedTile, cell);
                return;
            }

            // Handle no selection or selecting a new tile
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

            dragWire = Instantiate(dragWirePrefab, puzzle.transform).GetComponent<WireVisuals>();
            dragWire.portTypeFrom = PortType.Power;
            dragWire.portTypeTo = PortType.Power;
            dragWire.selected = true;
            dragWire.transform.position = puzzle.grid.CellToWorld(_selectedTile.cell);
            dragWire.target = puzzle.grid.CellToWorld(_selectedTile.cell);

            UpdateCursor();
        }

        private void OnLogicLButtonDrag(Vector2 position, Vector2 delta)
        {
            if (null == dragWire)
                return;

            dragWire.target = puzzle.grid.CellToWorld(canvas.CanvasToCell(position));
        }

        private void OnLogicLButtonDragEnd(Vector2 position)
        {
            if (null == dragWire)
                return;

            // Stop dragging
            Destroy(dragWire.gameObject);
            dragWire = null;

            // Connect to the cell
            Connect(_selectedTile, canvas.CanvasToCell(position));

            UpdateCursor();
        }

        private void Connect(Tile tile, Cell cell)
        {
            var group = new Editor.Commands.GroupCommand();

            if (!tile.CanConnectTo(cell))
                return;

            // Get all in the given cell that we can connect to
            var tiles = puzzle.grid.GetLinkedTiles(cell, cell).Where(t => tile.CanConnectTo(t, false)).ToArray();
            if (tiles.Length == 0)
                return;

            if(tiles.Length == 1)
            {
                ChoosePort(tile, tiles[0], (from, to) => {
                    ExecuteCommand(new Editor.Commands.WireAddCommand(from, to), false, (cmd) => {
                        selectedWire = (cmd as Editor.Commands.WireAddCommand).addedWire;
                    });
                });
            }
            else
            {
                ChooseTileConnection(tiles, (target) => {
                    ChoosePort(tile, target, (from, to) => {
                        ExecuteCommand(new Editor.Commands.WireAddCommand(from, to));
                    });
                });
            }
        }

        private void Disconnect(Tile tile, Cell cell)
        {
            var group = new Editor.Commands.GroupCommand();
            var outputs = tile.GetPorts(PortFlow.Output);
            foreach(var output in outputs)
                foreach(var wire in output.wires)
                {
                    var connection = wire.GetOppositeConnection(output);
                    if(connection.cell != cell)
                        continue;

                    group.Add(new Editor.Commands.WireDestroyCommand(wire));
                }

            if(group.hasCommands)
                ExecuteCommand(group);

            UpdateCursor();
        }

        private void SelectTile(Cell cell) => SelectTile(GetTile(cell));

        private void UpdateInspectorState(Tile tile)
        {
            tile.inspectorState = inspector.GetComponentsInChildren<Editor.IInspectorStateProvider>().Select(p => p.GetState()).ToArray();
        }

        private void SetWiresDark (Tile tile, bool dark)
        {
            if (null == tile || null == tile.properties)
                return;

            foreach (var property in tile.properties)
                if (property.type == TilePropertyType.Port)
                    foreach (var wire in property.GetValue<Port>(tile).wires)
                        wire.visuals.highlight = !dark;
        }

        private void SelectTile(Tile tile)
        {
            // Save the inspector state
            if (_selectedTile != null)
            {
                // Make sure the current edit box finishes before we clear the selected tile
                if(UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject == inspectorTileName)
                    UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(null);
                SetWiresDark(_selectedTile, false);
                UpdateInspectorState(_selectedTile);
            }

            _selectedTile = tile;

            if (tile == null)
            {
                selectionGizmo.gameObject.SetActive(false);
                _inspectorContent.transform.DetachAndDestroyChildren();
                _inspectorContent.SetActive(false);
                _inspectorHeader.SetActive(false);
                _inspectorEmpty.SetActive(true);
                _cameraBoundsGizmo.gameObject.SetActive(false);

                // Show all wires when no tile is selected
                _puzzle.ShowWires();
            } 
            else
            {
                _inspectorEmpty.SetActive(false);
                _inspectorContent.SetActive(true);
                _inspectorHeader.SetActive(true);
                inspectorTileName.SetTextWithoutNotify(tile.name);
                _inspectorTileType.text = $"<{_selectedTile.info.displayName}>";

                var rotated = _selectedTile.GetProperty("rotated");
                _inspectorRotate.gameObject.SetActive(rotated != null);
                _inspectorRotate.isOn = (rotated != null && rotated.GetValue<bool>(_selectedTile));

                var flipped = _selectedTile.GetProperty("flipped");
                _inspectorFlip.gameObject.SetActive(flipped != null);
                _inspectorFlip.isOn = (flipped != null && flipped.GetValue<bool>(_selectedTile));

                ShowCameraBounds(_selectedTile.GetComponent<GameCamera>());

                _inspectorTilePreview.texture = TileDatabase.GetPreview(tile.guid);
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
                _selectedWire.visuals.selected = false;

            _selectedWire = wire;

            if (_selectedWire != null)
                _selectedWire.visuals.selected = true;

            onSelectedWireChanged?.Invoke(_selectedWire);
        }


        public static void RefreshInspector() => instance.RefreshInspectorInternal();

        private void RefreshInspectorInternal()
        {
            var tile = _selectedTile;
            _inspectorContent.transform.DetachAndDestroyChildren();

            GameObject propertiesGroup = null;
            Transform propertiesGroupContent = null;
            foreach (var tileProperty in tile.properties)
            {
                // Skip hidden properties
                if (tile.IsPropertyHidden(tileProperty))
                    continue;

                var optionEditor = InstantiatePropertyEditor(tile, tileProperty, _inspectorContent.transform);
                if (null == optionEditor)
                    continue;

                if(!optionEditor.isGrouped)
                {
                    if (null == propertiesGroup)
                    {
                        propertiesGroup = Instantiate(optionPropertiesPrefab, _inspectorContent.transform);
                        propertiesGroupContent = propertiesGroup.transform.Find("Content");
                    }

                    optionEditor.transform.SetParent(propertiesGroupContent);
                }

                optionEditor.target = new TilePropertyEditorTarget(tile, tileProperty);
            }

            if (propertiesGroup != null)
                propertiesGroup.transform.SetAsFirstSibling();

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
        private UIPropertyEditor InstantiatePropertyEditor(Tile tile, TileProperty property, Transform parent)
        {
            var prefab = tile.info.GetCustomPropertyEditor(property)?.prefab ?? null;
            if(null == prefab)
                switch (property.type)
                {
                    case TilePropertyType.String:
                        prefab = property.editable.multiline ? stringMultilineEditorPrefab : stringEditorPrefab;
                        break;

                    case TilePropertyType.StringArray:
                        prefab = property.editable.multiline ? multilineStringArrayEditorPrefab : stringArrayEditorPrefab;
                        break;

                    case TilePropertyType.Int:
                        if (property.editable.range.x != property.editable.range.y)
                            prefab = numberRangeEditorPrefab;
                        else
                            prefab = numberEditorPrefab; 
                        break;


                    case TilePropertyType.Cell: prefab = cellEditorPrefab; break;
                    case TilePropertyType.IntArray: prefab = numberArrayEditorPrefab; break;
                    case TilePropertyType.Bool: prefab = boolEditorPrefab; break;
                    case TilePropertyType.Background: prefab = backgroundEditorPrefab; break;
                    case TilePropertyType.Guid: prefab = tileEditorPrefab; break;
                    case TilePropertyType.Sound: prefab = soundEditorPrefab; break;
                    case TilePropertyType.Decal: prefab = decalEditorPrefab; break;
                    case TilePropertyType.DecalArray: prefab = decalArrayEditorPrefab; break;
                    case TilePropertyType.Port:
                        if (property.GetValue<Port>(tile).wires.Count == 0)
                            prefab = portEmptyEditorPrefab; 
                        else
                            prefab = portEditorPrefab; 
                        break;

                    default:
                        return null;
                }

            return Instantiate(prefab.gameObject, parent).GetComponent<UIPropertyEditor>();
        }

        private void OnLogicKey(KeyCode keyCode)
        {
            switch (keyCode)
            {
                case KeyCode.Delete:
                    if (selectedTile != null)
                        ExecuteCommand(Erase(selectedTile));
                    break;

                case KeyCode.F:
                    if(_selectedTile != null)
                        Center(_selectedTile.cell, CameraManager.state.zoomLevel);
                    break;
            }
        }

        private CursorType OnLogicGetCursor(Cell cell)
        {
            // When shift is pressed it means "QuickConnect" mode
            if ((KeyboardManager.isShiftPressed && selectedTile != null) || dragWire != null)
            {
                var canConnect = selectedTile.CanConnectTo(cell, false);
                if (dragWire)
                    dragWire.highlight = canConnect;

                return canConnect ? CursorType.ArrowWithPlus : CursorType.ArrowWithNot;
            }

            // When ctrl is pressed it means "QuickDisconnect" mode
            if (KeyboardManager.isCtrlPressed && selectedTile != null)
                return selectedTile.IsConnectedTo(cell) ? CursorType.ArrowWithMinus : CursorType.ArrowWithNot;

            return CursorType.Arrow;
        }
    }
}
