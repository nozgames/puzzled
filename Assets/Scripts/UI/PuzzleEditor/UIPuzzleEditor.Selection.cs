using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Puzzled.Editor
{
    public partial class UIPuzzleEditor
    {
        [Header("Selection")]
        [SerializeField] private Color _selectionColor = Color.red;

        public static Color selectionColor => instance._selectionColor;

        private List<Tile> _selectedTiles = new List<Tile>();
        private Wire _selectedWire = null;
        private List<Tile> _boxSelectionTiles = new List<Tile>();
        private List<Tile> _boxSelectionTilesOutside = new List<Tile>();
        private Plane[] _boxSelectionPlanes = new Plane[4];
        private Collider[] _boxSelectionColliders = new Collider[128];

        /// <summary>
        /// True if there is at least one tile selected
        /// </summary>
        public bool hasSelection => _selectedTiles.Count > 0;

        /// <summary>
        /// Creates a copy of the selected tiles array and returns it
        /// </summary>
        public static Tile[] selectedTiles {
            get => instance._selectedTiles.ToArray();
            set => instance.SelectTiles(value);
        }

        /// <summary>
        /// Get/Set the selected wire.
        /// </summary>
        public static Wire selectedWire {
            get => instance._selectedWire;
            set => instance.SelectWire(value);
        }

        public void ClearSelection()
        {
            if (_selectedWire != null)
                SelectWire(null);

            while (_selectedTiles.Count > 0)
                RemoveSelection(_selectedTiles[0]);

            UpdateWireVisibility();

            RefreshInspectorInternal();

            UpdateCursor();
        }

        /// <summary>
        /// Select the top most tile in the given cell
        /// </summary>
        /// <param name="cell"></param>
        public static void SelectTile(Cell cell) => SelectTile(instance.GetTopMostTile(cell));

        /// <summary>
        /// Select the givens tiles
        /// </summary>
        /// <param name="tiles">Tiles to select</param>
        private void SelectTiles(Tile[] tiles)
        {
            ClearSelection();

            if (null == tiles || tiles.Length == 0)
                return;

            foreach (var tile in tiles)
                AddSelection(tile);

            RefreshInspectorInternal();
        }

        /// <summary>
        /// Add a tile to the current selection
        /// </summary>
        /// <param name="tile">Tile to select</param>
        private void AddSelection (Tile tile)
        {
            if (tile == null)
            {
                Debug.LogError("cannot add 'null' tile to selection");
                return;
            }

            if (_selectedTiles.Contains(tile))
                return;

            _selectedTiles.Add(tile);

            tile.editor.isSelected = true;

            tile.ShowGizmos(true);

            UpdateWireVisibility();

            SelectWire(null);
        }

        /// <summary>
        /// Remove a tile from the current selection
        /// </summary>
        /// <param name="tile">Tile to remove</param>
        private void RemoveSelection (Tile tile)
        {
            if (!_selectedTiles.Contains(tile))
                return;

            SetWiresDark(tile, false);

            tile.editor.isSelected = false;

            _selectedTiles.Remove(tile);

            tile.ShowGizmos(false);

            SelectWire(null);

            UpdateWireVisibility();
        }

        public static void DeselectTile (Tile tile)
        {
            instance.RemoveSelection(tile);
            instance.RefreshInspectorInternal();
        }

        /// <summary>
        /// Select a single tile
        /// </summary>
        public static void SelectTile(Tile tile)
        {
            instance.ClearSelection();
            instance.AddSelection(tile);            
            instance.RefreshInspectorInternal();
        }

        /// <summary>
        /// Select the given wire
        /// </summary>
        /// <param name="wire">Wire to select</param>
        private void SelectWire(Wire wire)
        {
            // Make sure one of the two tiles from the wire is selected, if not select the input
            if (wire != null && !wire.from.tile.isSelected && !wire.to.tile.isSelected)
                SelectTile(wire.from.tile);

            // Unselect the current wire selection
            if (_selectedWire != null)
                _selectedWire.visuals.selected = false;

            _selectedWire = wire;

            if (_selectedWire != null)
                _selectedWire.visuals.selected = true;

            onSelectedWireChanged?.Invoke(_selectedWire);
        }

        /// <summary>
        /// Returns a list of tiles that overlap or are contained within the given screen rectangle
        /// </summary>
        /// <param name="rect">Screen rectangle</param>
        /// <param name="outTiles">Tiles that overlap the screen rectangle</param>
        /// <returns>True if any tiles were found</returns>
        private bool CanvasRectToTiles (Rect rect, List<Tile> outTiles)
        {
            _boxSelectionTilesOutside.Clear();

            // Calculate the rays at the four corners of the rect
            var ray1 = _canvas.CanvasToRay(rect.min);
            var ray2 = _canvas.CanvasToRay(new Vector2(rect.min.x, rect.max.y));
            var ray3 = _canvas.CanvasToRay(rect.max);
            var ray4 = _canvas.CanvasToRay(new Vector2(rect.max.x, rect.min.y));

            // Find the intersection point of the rays with the ground plane
            var ground = new Plane(Vector3.up, Vector3.zero);
            if (!ground.Raycast(ray1, out float enter1))
                return false;
            if (!ground.Raycast(ray2, out float enter2))
                return false;
            if (!ground.Raycast(ray3, out float enter3))
                return false;
            if (!ground.Raycast(ray4, out float enter4))
                return false;

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
            while (overlapCount == _boxSelectionColliders.Length)
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
            for (int i = 0; i < overlapCount; i++)
            {
                var collider = _boxSelectionColliders[i];
                _boxSelectionColliders[i] = null;
                var tile = collider.GetComponentInParent<Tile>();
                if (null == tile)
                    continue;

                var outside = false;
                for (int planeIndex = 0; !outside && planeIndex < _boxSelectionPlanes.Length; planeIndex++)
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

                if (outside)
                {
                    _boxSelectionTilesOutside.Add(tile);
                    outTiles.Remove(tile);
                    continue;
                }

                if (!outTiles.Contains(tile))
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

        private void UpdateBoxSelection(bool additive = false)
        {
            // Clear the current selection
            ClearSelection();

            _boxSelectionTiles.Clear();

            // Find all tiles within the selection rectangle
            var rect = _selectionRect.rect;
            rect.position = _selectionRect.offsetMin;
            CanvasRectToTiles(rect, _boxSelectionTiles);

            // If shift is held then we need to merge the saved selection with the new selection
            if (additive)
                foreach (var savedTile in _savedSelection)
                    if (!_boxSelectionTiles.Contains(savedTile))
                        _boxSelectionTiles.Add(savedTile);

            // Select all of the tiles
            foreach (var tile in _boxSelectionTiles)
                AddSelection(tile);
        }

        /// <summary>
        ///  Update the visibility of all wires based on the current selection
        /// </summary>
        private void UpdateWireVisibility ()
        {
            if(_mode != Mode.Select)
            {
                _puzzle.ShowWires(false);
                return;
            }

            if(!hasSelection)
            {
                _puzzle.ShowWires(true);
                return;
            }

            _puzzle.ShowWires(false);
            foreach(var tile in _selectedTiles)
                _puzzle.ShowWires(tile, true);
        }
    }
}
