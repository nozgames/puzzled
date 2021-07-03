using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Puzzled.UI;
using Puzzled.Editor;
using System.Collections.Generic;

namespace Puzzled.Editor
{
    public partial class UIPuzzleEditor
    {
        [Header("Inspector")]
        [SerializeField] private GameObject _inspectorContent = null;
        [SerializeField] private GameObject _inspectorEmpty = null;
        [SerializeField] private GameObject _inspectorHeader = null;
        [SerializeField] private TMPro.TMP_InputField inspectorTileName = null;
        [SerializeField] private TMPro.TextMeshProUGUI _inspectorTileType = null;
        [SerializeField] private Image _inspectorTilePreview = null;
        [SerializeField] private UIPropertyEditor backgroundEditorPrefab = null;
        [SerializeField] private UIPropertyEditor boolEditorPrefab = null;
        [SerializeField] private UIPropertyEditor decalEditorPrefab = null;
        [SerializeField] private UIPropertyEditor decalArrayEditorPrefab = null;
        [SerializeField] private UIPropertyEditor soundArrayEditorPrefab = null;
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
        [SerializeField] private UIPropertyEditor colorEditorPrefab = null;
        [SerializeField] private UIPropertyEditor tileEditorPrefab = null;
        [SerializeField] private GameObject optionPropertiesPrefab = null;
        [SerializeField] private Button _inspectorRotateButton = null;

        private WireVisuals dragWire = null;
        private bool logicCycleSelection = false;
        private bool _allowLogicDrag = false;

        public static Action<Wire> onSelectedWireChanged;

        private void EnableLogicTool()
        {
            _canvas.onLButtonDown = OnLogicLButtonDown;
            _canvas.onLButtonUp = OnLogicLButtonUp;
            _canvas.onLButtonDragBegin = OnLogicLButtonDragBegin;
            _canvas.onLButtonDrag = OnLogicLButtonDrag;
            _canvas.onLButtonDragEnd = OnLogicLButtonDragEnd;

            _onKey = OnLogicKey;
            _getCursor = OnLogicGetCursor;

            inspector.SetActive(true);

            logicCycleSelection = false;

            // Show all wires when logic tool is enabled
            if(selectedTile == null)
                _puzzle.ShowWires(true);
        }

        private void DisableLogicTool()
        {            
        }

        private void OnInspectorTileNameChanged(string name)
        {
            if (selectedTile == null)
                return;

            name = name.Trim();

            if (string.IsNullOrWhiteSpace(name))
            {
                inspectorTileName.SetTextWithoutNotify(selectedTile.name);
                return;
            }

            if (name == selectedTile.name)
                return;

            ExecuteCommand(new Editor.Commands.TileRenameCommand(selectedTile, name));
        }

        private void OnLogicLButtonDown(Vector2 position)
        {
            // Ensure the cell being dragged is the selected cell
            var cell = _cursorCell;

            _allowLogicDrag = false;

            // QuickConnect mode
            if (selectedTile != null && KeyboardManager.isShiftPressed)
            {
                Connect(selectedTile, cell);
                return;
            }

            if (selectedTile != null && KeyboardManager.isCtrlPressed)
            {
                Disconnect(selectedTile, cell);
                return;
            }

            // Handle no selection or selecting a new tile
            if (selectedTile == null || !_puzzle.grid.CellContainsWorldPoint(selectedTile.cell, _cursorWorld))
            {
                SelectTile(GetTopMostTile(cell, TileLayer.InvisibleStatic));
                logicCycleSelection = false;
            }
            else
                logicCycleSelection = true;

            _allowLogicDrag = selectedTile != null && selectedTile.hasOutputs;
        }

        private void OnLogicLButtonUp(Vector2 position)
        {
            if (null != dragWire || selectedTile==null || !logicCycleSelection)
                return;

            SelectNextTileUnderCursor();
        }

        private void OnLogicLButtonDragBegin(Vector2 position)
        {
            if (!_allowLogicDrag)
                return;

            if (selectedTile == null || !selectedTile.hasOutputs)
                return;

            dragWire = Instantiate(dragWirePrefab, puzzle.transform).GetComponent<WireVisuals>();
            dragWire.portTypeFrom = PortType.Power;
            dragWire.portTypeTo = PortType.Power;
            dragWire.selected = true;
            dragWire.transform.position = puzzle.grid.CellToWorldBounds(selectedTile.cell).center;
            dragWire.target = puzzle.grid.CellToWorldBounds(selectedTile.cell).center;

            UpdateCursor();
        }

        private void OnLogicLButtonDrag(Vector2 position, Vector2 delta)
        {
            if (null == dragWire)
                return;

            dragWire.target = puzzle.grid.CellToWorldBounds(_cursorCell).center;
        }

        private void OnLogicLButtonDragEnd(Vector2 position)
        {
            if (null == dragWire)
                return;

            // Stop dragging
            Destroy(dragWire.gameObject);
            dragWire = null;

            // Connect to the cell
            Connect(selectedTile, _cursorCell);

            UpdateCursor();
        }

        private void Connect(Tile tile, Cell cell)
        {
            var group = new Editor.Commands.GroupCommand();

            if (!tile.CanConnectTo(cell))
                return;

            // Get all in the given cell that we can connect to
            var tiles = puzzle.grid.GetTiles(cell).Where(t => tile.CanConnectTo(t, false)).ToArray();
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


        private void UpdateInspectorState(Tile tile)
        {
            tile.inspectorState = inspector.GetComponentsInChildren<Editor.IInspectorStateProvider>(true).Select(p => (p.inspectorStateId, p.inspectorState)).ToArray();
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


        public static void RefreshInspector() => instance.RefreshInspectorInternal();

        private void RefreshInspectorInternal()
        {
            var tile = selectedTile;
            _inspectorContent.transform.DetachAndDestroyChildren();

            // Create the custom editos
            if(tile.info.customEditors != null)
                foreach (var customEditorPrefab in tile.info.customEditors)
                    Instantiate(customEditorPrefab, _inspectorContent.transform).GetComponent<UIInspectorEditor>().tile = tile;

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
            if (selectedTile.inspectorState != null)
            {
                var providers = inspector.GetComponentsInChildren<Editor.IInspectorStateProvider>(true);
                foreach (var state in selectedTile.inspectorState)
                {
                    var provider = providers.FirstOrDefault(p => p.inspectorStateId == state.id);
                    if (provider != null)
                        provider.inspectorState = state.value;
                }
            }

            // Sort the priorities
            var priorities = new List<UIPriority>(_inspectorContent.transform.childCount);
            for(int i=0; i< _inspectorContent.transform.childCount; i++)
            {
                var priority = _inspectorContent.transform.GetChild(i).GetComponent<UIPriority>();
                if(null == priority)
                    priority = _inspectorContent.transform.GetChild(i).gameObject.AddComponent<UIPriority>();

                priorities.Add(priority);
            }

            priorities = priorities.OrderByDescending(p => p.priority).ToList();
            for(int i=0; i<priorities.Count; i++)
                priorities[i].transform.SetSiblingIndex(i);
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
                    case TilePropertyType.Color: prefab = colorEditorPrefab; break;
                    case TilePropertyType.SoundArray: prefab = soundArrayEditorPrefab; break;
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
                    if(selectedTile != null)
                        Center(selectedTile.cell, _cameraZoom);
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
