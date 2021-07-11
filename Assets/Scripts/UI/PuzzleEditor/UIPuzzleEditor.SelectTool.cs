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
        private Tile _cursorHighlight = null;
        private Vector2 _selectionStart;
        private Vector3 _selectionStartWorld;
        private List<Tile> _savedSelection = new List<Tile>();

        public static Action<Wire> onSelectedWireChanged;

        private void EnableSelectTool()
        {
            _canvas.onLButtonDown = OnSelectLButtonDown;
            _canvas.onLButtonUp = OnSelectLButtonUp;
            _canvas.onLButtonDragBegin = OnSelectLButtonDragBegin;
            _canvas.onLButtonDrag = OnSelectLButtonDrag;
            _canvas.onLButtonDragEnd = OnSelectLButtonDragEnd;
            _canvas.onExit = OnSelectToolExitCanvas;

            _onKey = OnSelectToolKey;
            _onKeyModifiers = OnSelectToolKeyModifiers;
            _getCursor = OnSelectToolGetCursor;

            inspector.SetActive(true);

            _puzzle.ShowWires(!hasSelection);
        }

        private void DisableSelectTool()
        {
            HilightTile(null);
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
            // Initial box selection
            _selectionStart = position;
            _selectionStartWorld = _cursorWorld;
            _selectionRect.offsetMin = position;
            _selectionRect.offsetMax = position;
        }

        private void OnSelectLButtonUp (Vector2 position)
        {
            if (_dragWire != null || _selectionRect.gameObject.activeSelf)
                return;

            var hit = _cursorTiles;
            if (hit.Count == 0)
            {
                ClearSelection();
                return;
            }

            var hitTile = hit.Last();
            var selectedIndex = _selectedTiles.Count > 0 ? hit.IndexOf(_selectedTiles[0]) : -1;

            // When shift is held use multi-select logic
            if (KeyboardManager.isShiftPressed)
            {
                if (hitTile.isSelected)
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
                SelectTile(hit[(selectedIndex - 1 + hit.Count) % hit.Count]);
            }
            // Single select logic
            else
            {
                SelectTile(hitTile);
            }

            UpdateCursor();
        }

        private List<Tile> _boxSelectionTiles = new List<Tile>();
        private List<Tile> _boxSelectionTilesOutside = new List<Tile>();
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
            _boxSelectionTilesOutside.Clear();

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
                var collider = _boxSelectionColliders[i];
                _boxSelectionColliders[i] = null;
                var tile = collider.GetComponentInParent<Tile>();
                if (null == tile)
                    continue;

                var outside = false;
                for(int planeIndex = 0; !outside && planeIndex < _boxSelectionPlanes.Length; planeIndex++)
                {
                    var plane = _boxSelectionPlanes[planeIndex];
                    var colliderBounds = collider.bounds;
                    var min = colliderBounds.min;
                    var max = colliderBounds.max;

                    outside = !plane.GetSide(new Vector3(min.x, 0, min.z)) ||
                              !plane.GetSide(new Vector3(min.x, 0, max.z)) ||
                              !plane.GetSide(new Vector3(max.x, 0, min.z)) ||
                              !plane.GetSide(new Vector3(max.x, 0, max.z));
                }

                if(outside)
                {
                    _boxSelectionTilesOutside.Add(tile);
                    outTiles.Remove(tile);
                    continue;
                }

                if(!outTiles.Contains(tile))
                    outTiles.Add(tile);
            }

            return outTiles.Count > 0;
        }

        private void UpdateBoxSelection(Vector2 position)
        {
            // Update the selection rect
            _selectionRect.offsetMin = Vector3.Min(_selectionStart, position);
            _selectionRect.offsetMax = Vector3.Max(_selectionStart, position);

            UpdateBoxSelection(KeyboardManager.isShiftPressed);
        }

        private void UpdateBoxSelection(bool additive=false)
        { 
            // Clear the current selection
            ClearSelection();

            _boxSelectionTiles.Clear();

            // Find all tiles within the selection rectangle
            var rect = _selectionRect.rect;
            rect.position = _selectionRect.offsetMin;
            ScreenRectToTiles(rect, _boxSelectionTiles);

            // If shift is held then we need to merge the saved selection with the new selection
            if (additive)
                foreach (var savedTile in _savedSelection)
                    if (!_boxSelectionTiles.Contains(savedTile))
                        _boxSelectionTiles.Add(savedTile);

            // Select all of the tiles
            foreach (var tile in _boxSelectionTiles)
                AddSelection(tile);
        }

        private void OnSelectLButtonDragBegin(Vector2 position)
        {
            // Drag a wire from the wire gizmo
            if(IsOverWireGizmo(_selectionStartWorld))
            {
                // Start dragging a wire
                _dragWire = Instantiate(dragWirePrefab, puzzle.transform).GetComponent<WireVisuals>();
                _dragWire.portTypeFrom = PortType.Power;
                _dragWire.portTypeTo = PortType.Power;
                _dragWire.selected = true;
                _dragWire.target = _dragWire.transform.position = puzzle.grid.CellToWorldBounds(_inspectorTile.cell).center;

                UpdateCursor();

                return;
            }

            // TODO: if near outside edge of selection allow rotate (single tile only for now)
            // TODO: if dragged from on top of a selected tile then perform a move
            // TODO: if dragged from the wire manipulator then start a wire drag

            // Save the selection for shift box drag operations
            _savedSelection.Clear();
            _savedSelection.AddRange(_selectedTiles);

            // Show the box selection 
            _selectionRect.gameObject.SetActive(true);

            // Start with the new box selection
            UpdateBoxSelection(position);
        }

        private void OnSelectLButtonDrag(Vector2 position, Vector2 delta)
        {
            // Update wire drag
            if (null != _dragWire)
            {
                _dragWire.target = _cursorWorld; 
                return;
            }

            // Update box selection
            if (_selectionRect.gameObject.activeSelf)
            {
                UpdateBoxSelection(position);
                return;
            }
        }

        private void OnSelectLButtonDragEnd(Vector2 position)
        {
            // End Box selection
            if(_selectionRect.gameObject.activeSelf)
            {
                UpdateBoxSelection(position);
                RefreshInspectorInternal();
                _selectionRect.gameObject.SetActive(false);
                _savedSelection.Clear();
                _boxSelectionTiles.Clear();
                _boxSelectionTilesOutside.Clear();
                return;
            }

            // End Wire drag
            if (null != _dragWire)
            {
                // Stop dragging
                Destroy(_dragWire.gameObject);
                _dragWire = null;

                // Connect to the cursor tiles
                Connect(_inspectorTile, _cursorTiles.Where(t => _inspectorTile.CanConnectTo(t, false)).ToArray());

                UpdateCursor();
                return;
            }

            // TODO: end move
        }

        private void Connect(Tile tile, Tile[] tiles)
        {
            if (tiles.Length == 0)
                return;

            if (tiles.Length == 1)
            {
                ChoosePort(tile, tiles[0], (from, to) => {
                    ExecuteCommand(new Commands.WireAddCommand(from, to), false, (cmd) => {
                        selectedWire = (cmd as Commands.WireAddCommand).addedWire;
                    });
                });
            } else
            {
                ChooseTileConnection(tiles, (target) => {
                    ChoosePort(tile, target, (from, to) => {
                        ExecuteCommand(new Commands.WireAddCommand(from, to));
                    });
                });
            }
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
                    ExecuteCommand(Erase(_selectedTiles.ToArray()));
                    break;

                case KeyCode.F:
                    if(hasSelection)
                        Center(selectedTiles, _cameraZoom);
                    break;
            }
        }

        private void OnSelectToolKeyModifiers(bool shift, bool ctrl, bool alt)
        {
            // Update box selection whenever modifiers change if it is active
            if (_selectionRect.gameObject.activeSelf)
                UpdateBoxSelection(shift);
        }

        /// <summary>
        /// Returns true if the cursor is over the wire gizmo
        /// </summary>
        private bool IsOverWireGizmo(Vector3 position) => _wireGizmo.activeSelf && (position - _wireGizmo.transform.position).magnitude <= _wireGizmo.transform.localScale.x * 0.5f;

        /// <summary>
        /// Set the higlight tile
        /// </summary>
        private void HilightTile(Tile highlight)
        {
            if (highlight == _cursorHighlight)
                return;

            if (_cursorHighlight != null)
                _cursorHighlight.editor.isHighlighted = false;

            _cursorHighlight = highlight;

            if (_cursorHighlight != null)
                _cursorHighlight.editor.isHighlighted = true;
        }

        private CursorType OnSelectToolGetCursor(Cell cell)
        {
            // Show arrow for box selection
            if (_selectionRect.gameObject.activeSelf)
            {
                HilightTile(null);
                return CursorType.Arrow;
            }

            // If the wire gizmo is active then hit test it
            if (_dragWire == null && IsOverWireGizmo(_cursorWorld))
            {
                // TODO: wire crosshair?
                HilightTile(null);
                return CursorType.ArrowWithPlus;
            }

            _cursorTiles = RaycastTiles(_cursorRay);

            HilightTile(_cursorTiles.LastOrDefault());

            // If dragging a wire..
            if (_dragWire != null)
            {
                // Can the inspector tile connect to any of the cursor tiles?
                foreach (var cursorTile in _cursorTiles)
                    if (_inspectorTile.CanConnectTo(cursorTile, false))
                        return CursorType.ArrowWithPlus;

                return CursorType.ArrowWithNot;
            }

            // Move cursor when over a tile that is already selected
            if (_cursorTiles.Any(t => t.isSelected))
                return CursorType.ArrowWithMove;

            return CursorType.Arrow;
        }

        private void OnSelectToolExitCanvas()
        {            
            HilightTile(null);
        }

    }
}
