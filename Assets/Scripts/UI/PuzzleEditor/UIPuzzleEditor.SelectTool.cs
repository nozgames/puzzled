using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Puzzled.Editor
{
    public partial class UIPuzzleEditor
    {
        [Header("SelectTool")]
        [SerializeField] private RectTransform _selectionRect = null;

        private WireVisuals _dragWire = null;
        private List<Tile> _cursorTiles = null;
        private Tile _pendingSelection = null;
        private Vector2 _selectionStart;

        public static Action<Wire> onSelectedWireChanged;

        private void EnableSelectTool()
        {
            _canvas.onLButtonDown = OnSelectLButtonDown;
            _canvas.onLButtonUp = OnSelectLButtonUp;
            _canvas.onLButtonDragBegin = OnSelectLButtonDragBegin;
            _canvas.onLButtonDrag = OnSelectLButtonDrag;
            _canvas.onLButtonDragEnd = OnSelectLButtonDragEnd;

            _onKey = OnSelectToolKey;
            _getCursor = OnSelectToolGetCursor;

            inspector.SetActive(true);

            _puzzle.ShowWires(!hasSelection);
        }

        private void DisableSelectTool()
        {
            _cursorTiles = null;
        }

        private List<Tile> RaycastTiles (Ray ray) => 
            Physics.RaycastAll(_cursorRay, 100.0f, CameraManager.camera.cullingMask)
                .OrderByDescending(h => h.distance)
                .Select(h => h.collider.GetComponentInParent<Tile>())
                .Where(t => t != null)
                .ToList();

        private void OnSelectLButtonDown (Vector2 position)
        {
            _selectionStart = position;
            _selectionRect.offsetMin = position;
            _selectionRect.offsetMax = position;

#if false

            // TODO: did it hit a manipulator such as wire drag or move ?
            // TODO: box selection

            var hit = _cursorTiles;
            if (hit.Count == 0)
                return;

            var hitTile = hit.Last();
            var selectedIndex = _selectedTiles.Count > 0 ? hit.IndexOf(_selectedTiles[0]) : -1;

            // When shift is held use multi-select logic
            if (KeyboardManager.isShiftPressed)
            {
                if (IsSelected(hitTile))
                    RemoveSelection(hitTile);
                else
                    AddSelection(hitTile);

                RefreshInspectorInternal();
            }
            // If there is only one selected tile and that tile was hit then we need to cycle
            // selection to the next lowest tile.
            else if (_selectedTiles.Count == 1 && selectedIndex != -1)
            {
                // Selected the same tile so just ignore it
                if (hit.Count == 1)
                    return;

                // Set the pending selection to the tile under the current one selected.  This pending
                // selection will be realized if no drag occurs and the mouse is released.
                _pendingSelection = hit[(selectedIndex - 1 + hit.Count) % hit.Count];
            }
            // Single select logic
            else
            { 
                SelectTile(hitTile);
            }
#endif
        }

        private void OnSelectLButtonUp (Vector2 position)
        {
            if(_pendingSelection != null)
            {
                SelectTile(_pendingSelection);
                _pendingSelection = null;
            }
        }

        private List<Tile> _boxSelectionTiles = new List<Tile>();
        private Plane[] _boxSelectionPlanes = new Plane[4];
        private Collider[] _boxSelectionColliders = new Collider[128];

        /// <summary>
        /// Returns a list of tiles that overlap or are contained within the given screen rectangle
        /// </summary>
        /// <param name="rect">Screen rectangle</param>
        /// <param name="outTiles">Tiles that overlap the screen rectangle</param>
        /// <returns>True if any tiles were found</returns>
        private bool ScreenRectToTiles(Rect rect, List<Tile> outTiles)
        {
            outTiles.Clear();

            // Calculate the rays at the four corners of the rect
            var ray1 = CameraManager.camera.ScreenPointToRay(rect.min);
            var ray2 = CameraManager.camera.ScreenPointToRay(new Vector2(rect.min.x, rect.max.y));
            var ray3 = CameraManager.camera.ScreenPointToRay(rect.max);
            var ray4 = CameraManager.camera.ScreenPointToRay(new Vector2(rect.max.x, rect.min.y));

            // Find the intersection point of the rays with the ground plane
            var ground = new Plane(Vector3.up, Vector3.zero);
            if (!ground.Raycast(ray1, out float enter1)) return false;
            if (!ground.Raycast(ray2, out float enter2)) return false;
            if (!ground.Raycast(ray3, out float enter3)) return false;
            if (!ground.Raycast(ray4, out float enter4)) return false;

            // Find the world coordinates of the ray intersections
            var world1 = ray1.origin + ray1.direction * enter1;
            var world2 = ray2.origin + ray2.direction * enter2;
            var world3 = ray3.origin + ray3.direction * enter3;
            var world4 = ray4.origin + ray4.direction * enter4;

            // Find all colliders within an AABB that wraps the box selection world coordinates
            var bounds = new Bounds(world1, Vector3.zero);
            bounds.Encapsulate(world2);
            bounds.Encapsulate(world3);
            bounds.Encapsulate(world4);
            bounds.Encapsulate(world1 + Vector3.up * 10.0f);

            var overlapCount = Physics.OverlapBoxNonAlloc(bounds.center, bounds.extents, _boxSelectionColliders, Quaternion.identity, CameraManager.camera.cullingMask);
            while(overlapCount == _boxSelectionColliders.Length)
            {
                _boxSelectionColliders = new Collider[_boxSelectionColliders.Length * 2];
                overlapCount = Physics.OverlapBoxNonAlloc(bounds.center, bounds.extents, _boxSelectionColliders, Quaternion.identity, CameraManager.camera.cullingMask);
            }

            // Create planes for each side of the box selection
            _boxSelectionPlanes[0] = new Plane(-Vector3.Cross((world2 - world1).normalized, Vector3.up), world1);
            _boxSelectionPlanes[1] = new Plane(-Vector3.Cross((world3 - world2).normalized, Vector3.up), world2);
            _boxSelectionPlanes[2] = new Plane(-Vector3.Cross((world4 - world3).normalized, Vector3.up), world3);
            _boxSelectionPlanes[3] = new Plane(-Vector3.Cross((world1 - world4).normalized, Vector3.up), world4);

            // Test each collider that was overlapped and see if it is inside the calculate planes
            for(int i=0; i<overlapCount; i++)
            {
                var tile = _boxSelectionColliders[i].GetComponentInParent<Tile>();
                if (null == tile)
                    continue;

                if (outTiles.Contains(tile))
                    continue;

                if (GeometryUtility.TestPlanesAABB(_boxSelectionPlanes, _boxSelectionColliders[i].bounds))
                    outTiles.Add(tile);
            }

            return outTiles.Count > 0;
        }

        private void UpdateBoxSelection(Vector2 position)
        {
            // Update the selection rect
            _selectionRect.offsetMin = Vector3.Min(_selectionStart, position);
            _selectionRect.offsetMax = Vector3.Max(_selectionStart, position);

            // Ensure it is visible
            _selectionRect.gameObject.SetActive(true);

            var rect = _selectionRect.rect;
            rect.position = _selectionRect.offsetMin;
            
            ClearSelection();
            if (!ScreenRectToTiles(rect, _boxSelectionTiles))
                return;

            foreach (var tile in _boxSelectionTiles)
                AddSelection(tile);
        }

        private void OnSelectLButtonDragBegin(Vector2 position)
        {
            // TODO: if dragged from on top of a selected tile then perform a move
            // TODO: if dragged from the wire manipulator then start a wire drag

            // Once a box selection starts we just clear the current selection
            ClearSelection();

            UpdateBoxSelection(position);
            


            // Do not allow dragging wires if the selection has more than one tile
            if (_selectedTiles.Count != 1)
                return;

            // Do not allow dragging wires if there is no output on the selected tile
            var tile = _selectedTiles[0];
            if (!tile.hasOutputs)
                return;

            // Since a drag is starting we cancel any pending tile selection
            _pendingSelection = null;

            // Start dragging a wire
            _dragWire = Instantiate(dragWirePrefab, puzzle.transform).GetComponent<WireVisuals>();
            _dragWire.portTypeFrom = PortType.Power;
            _dragWire.portTypeTo = PortType.Power;
            _dragWire.selected = true;
            _dragWire.transform.position = puzzle.grid.CellToWorldBounds(tile.cell).center;
            _dragWire.target = puzzle.grid.CellToWorldBounds(tile.cell).center;

            UpdateCursor();
        }

        private void OnSelectLButtonDrag(Vector2 position, Vector2 delta)
        {
            UpdateBoxSelection(position);


            // TODO: UPDATE the selected items in the selection rect

            if (null == _dragWire)
                return;

            _dragWire.target = puzzle.grid.CellToWorldBounds(_cursorCell).center;
        }

        private void OnSelectLButtonDragEnd(Vector2 position)
        {
            _selectionRect.gameObject.SetActive(false);

            if (null == _dragWire)
                return;

            // Stop dragging
            Destroy(_dragWire.gameObject);
            _dragWire = null;

            // Connect to the cell
            Connect(_selectedTiles[0], _cursorCell);

            UpdateCursor();
        }

        private void Connect(Tile tile, Cell cell)
        {
            var group = new Commands.GroupCommand();

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

        private void SetWiresDark (Tile tile, bool dark)
        {
            if (null == tile || null == tile.properties)
                return;

            foreach (var property in tile.properties)
                if (property.type == TilePropertyType.Port)
                    foreach (var wire in property.GetValue<Port>(tile).wires)
                        wire.visuals.highlight = !dark;
        }

        private void OnSelectToolKey(KeyCode keyCode)
        {
            switch (keyCode)
            {
                case KeyCode.Delete:
                    if (_selectedTiles.Count == 0)
                        return;

                    var command = new Commands.GroupCommand();
                    foreach (var tile in _selectedTiles)
                        Erase(tile, command);

                    ExecuteCommand(command);
                    break;

                case KeyCode.F:
                    if(hasSelection)
                        Center(selectedTiles, _cameraZoom);
                    break;
            }
        }

        private CursorType OnSelectToolGetCursor(Cell cell)
        {
            _cursorTiles = RaycastTiles(_cursorRay);

            // If dragging a wire..
            if (_dragWire != null)
            {
                // TODO: if over a tile that can be connected to then plus icon else not icon
            }

            return CursorType.Arrow;
        }
    }
}
