using System.Linq;
using UnityEngine;

namespace Puzzled.Editor
{
    public class EditorTile : TileComponent
    {
        /// <summary>
        /// Index of the "Selected" layer
        /// </summary>
        private static int _selectedLayer = -1;

        private static int _selectedFloorLayer = -1;

        private static int _tileFloorLayer = -1;

        private static int _markId = 1;

        /// <summary>
        /// Tag used to ignore selection on a renderer
        /// </summary>
        private const string _ignoreSelectionTag = "IgnoreSelection";

        private static int _selectionCount = 0;

        private struct SelectionRenderer
        {
            public Renderer renderer;
            public int layer;
            public Color color;
        }

        /// <summary>
        /// Cached list of renderers affected by the selection
        /// </summary>
        private SelectionRenderer[] _renderers = null;

        /// <summary>
        /// True if the tile is selected in the editor
        /// </summary>
        private bool _selected;

        /// <summary>
        /// True if the tile should be highlighted in the editor
        /// </summary>
        private bool _highlighted;

        /// <summary>
        /// MarkId the last time this tile was marked
        /// </summary>
        private int _lastMarkIdA = 0;

        /// <summary>
        /// MarkId the last time this tile was marked
        /// </summary>
        private int _lastMarkIdB = 0;

        /// <summary>
        /// Get/Set the selected state of the tile within the editor
        /// </summary>
        public bool isSelected {
            get => _selected;
            set {
                if (_selected == value)
                    return;

                _selected = value;

                UpdateSelection();
            }
        }

        /// <summary>
        /// Get/Set the highlighted state of the tile within the editor
        /// </summary>
        public bool isHighlighted {
            get => _highlighted;
            set {
                if (_highlighted == value)
                    return;

                _highlighted = value;
                UpdateSelection();
            }
        }

        public bool isMarkedA {
            get => _markId == _lastMarkIdA;
            set {
                if (value)
                    _lastMarkIdA = _markId;
                else
                    _lastMarkIdA = -1;
            }
        }

        public bool isMarkedB {
            get => _markId == _lastMarkIdB;
            set {
                if (value)
                    _lastMarkIdB = _markId;
                else
                    _lastMarkIdB = -1;
            }
        }

        /// <summary>
        /// Returns true if the tile is in the trash
        /// </summary>
        public bool isInTrash => transform.parent == tile.puzzle.trash;

        /// <summary>
        /// Inspector state stored on the tile for the next time the tile is selected
        /// </summary>
        public (string id, object value)[] inspectorState { get; set; }

        private void Awake()
        {
            if (_selectedLayer == -1)
                _selectedLayer = LayerMask.NameToLayer("Selected");

            if (_selectedFloorLayer == -1)
                _selectedFloorLayer = LayerMask.NameToLayer("TileFloorSelected");

            if (_tileFloorLayer == -1)
                _tileFloorLayer = LayerMask.NameToLayer("TileFloor");
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            UpdateSelection();
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            Deselect();
        }

        private void UpdateSelection()
        {
            if (_selected || _highlighted)
                Select();
            else
                Deselect();
        }

        private void Select()
        {
            // Early out if already selected
            if (_renderers != null)
                return;

            // Get list of all renderers affected by the selection
            _renderers = GetComponentsInChildren<Renderer>()
                .Where(r => r.tag != _ignoreSelectionTag)
                .Select(r => {
                    return new SelectionRenderer {
                        renderer = r,
                        color = (r is SpriteRenderer spriteRenderer) ? spriteRenderer.color : Color.white,
                        layer = r.gameObject.layer
                    };
                })
                .ToArray();

            if(_renderers.Length == 0)
            {
                _renderers = null;
                return;
            }

            _selectionCount++;

            CameraManager.showSelection = true;

            // Swap the color and layer for all affected renderers
            foreach (var renderer in _renderers)
            {                
                if (renderer.renderer is SpriteRenderer spriteRenderer)
                    spriteRenderer.color = UIPuzzleEditor.selectionColor;
                else
                    renderer.renderer.gameObject.layer = renderer.renderer.gameObject.layer == 
                        _tileFloorLayer ? _selectedFloorLayer : _selectedLayer;
            }
        }

        private void Deselect()
        {
            // Make sure we were selected
            if (_renderers == null)
                return;

            _selectionCount--;
            Debug.Assert(_selectionCount >= 0);

            CameraManager.showSelection = _selectionCount > 0;

            foreach (var renderer in _renderers)
            {
                // Make sure the renderer was not destroyed
                if (renderer.renderer == null)
                    continue;

                if (renderer.renderer is SpriteRenderer spriteRenderer)
                    spriteRenderer.color = renderer.color;
                else
                    renderer.renderer.gameObject.layer = renderer.layer;
            }

            _renderers = null;
        }

        public static void BeginMark() => _markId++;
    }
}
