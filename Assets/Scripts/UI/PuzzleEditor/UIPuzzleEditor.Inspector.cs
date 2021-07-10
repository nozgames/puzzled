using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Puzzled.Editor
{
    public partial class UIPuzzleEditor
    {
        [Header("Inspector")]
        [SerializeField] private GameObject _inspectorContent = null;
        [SerializeField] private GameObject _inspectorEmpty = null;
        [SerializeField] private GameObject _inspectorMultiple = null;
        [SerializeField] private GameObject _inspectorHeader = null;
        [SerializeField] private TMPro.TMP_InputField _inspectorTileName = null;
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

        private Tile _inspectorTile = null;

        /// <summary>
        /// Returns the tile that is currently being inspected
        /// </summary>
        public static Tile inspectorTile => instance._inspectorTile;

        private void UpdateInspectorState(Tile tile)
        {
            tile.inspectorState = inspector.GetComponentsInChildren<Editor.IInspectorStateProvider>(true).Select(p => (p.inspectorStateId, p.inspectorState)).ToArray();
        }

        public static void RefreshInspector() => instance.RefreshInspectorInternal();

        private void RefreshInspectorInternal()
        {
            // If a tile was being inspected then cleanup first
            if(_inspectorTile != null)
            {
                // Make sure the current edit box finishes
                if (UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject != null)
                    if (UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject.GetComponent<TMPro.TMP_InputField>() != null)
                        UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(null);

                UpdateInspectorState(_inspectorTile);

                _inspectorTile = null;
            }

            HideCameraEditor();

            _inspectorContent.transform.DetachAndDestroyChildren();
            _inspectorEmpty.SetActive(false);
            _inspectorMultiple.SetActive(false);
            _inspectorContent.SetActive(false);
            _inspectorHeader.SetActive(false);

            if (_selectedTiles.Count == 0)
            {
                _inspectorEmpty.SetActive(true);
                return;
            }

            if (_selectedTiles.Count > 1)
            {
                _inspectorMultiple.SetActive(true);
                return;
            }

            _inspectorContent.SetActive(true);
            _inspectorHeader.SetActive(true);

            _inspectorTile = _selectedTiles[0];
            _inspectorTileName.SetTextWithoutNotify(_inspectorTile.name);
            _inspectorTileType.text = $"<{_inspectorTile.info.displayName}>";
            _inspectorRotateButton.gameObject.SetActive(_inspectorTile.GetProperty("rotation") != null);
            _inspectorTilePreview.sprite = DatabaseManager.GetPreview(_inspectorTile.guid);

            // Create the custom editors
            if (_inspectorTile.info.customEditors != null)
                foreach (var customEditorPrefab in _inspectorTile.info.customEditors)
                    Instantiate(customEditorPrefab, _inspectorContent.transform).GetComponent<UIInspectorEditor>().tile = _inspectorTile;

            GameObject propertiesGroup = null;
            Transform propertiesGroupContent = null;
            foreach (var tileProperty in _inspectorTile.properties)
            {
                // Skip hidden properties
                if (_inspectorTile.IsPropertyHidden(tileProperty))
                    continue;

                var optionEditor = InstantiatePropertyEditor(_inspectorTile, tileProperty, _inspectorContent.transform);
                if (null == optionEditor)
                    continue;

                if (!optionEditor.isGrouped)
                {
                    if (null == propertiesGroup)
                    {
                        propertiesGroup = Instantiate(optionPropertiesPrefab, _inspectorContent.transform);
                        propertiesGroupContent = propertiesGroup.transform.Find("Content");
                    }

                    optionEditor.transform.SetParent(propertiesGroupContent);
                }

                optionEditor.target = new TilePropertyEditorTarget(_inspectorTile, tileProperty);
            }

            if (propertiesGroup != null)
                propertiesGroup.transform.SetAsFirstSibling();

            // Apply the saved inspector state
            if (_inspectorTile.inspectorState != null)
            {
                var providers = inspector.GetComponentsInChildren<Editor.IInspectorStateProvider>(true);
                foreach (var state in _inspectorTile.inspectorState)
                {
                    var provider = providers.FirstOrDefault(p => p.inspectorStateId == state.id);
                    if (provider != null)
                        provider.inspectorState = state.value;
                }
            }

            // Sort the priorities
            var priorities = new List<UIPriority>(_inspectorContent.transform.childCount);
            for (int i = 0; i < _inspectorContent.transform.childCount; i++)
            {
                var priority = _inspectorContent.transform.GetChild(i).GetComponent<UIPriority>();
                if (null == priority)
                    priority = _inspectorContent.transform.GetChild(i).gameObject.AddComponent<UIPriority>();

                priorities.Add(priority);
            }

            priorities = priorities.OrderByDescending(p => p.priority).ToList();
            for (int i = 0; i < priorities.Count; i++)
                priorities[i].transform.SetSiblingIndex(i);

            // If the tile is a camera then open the camera editor as well
            var gameCamera = _inspectorTile.GetComponent<GameCamera>();
            if (gameCamera != null)
                ShowCameraEditor(gameCamera);
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
            if (null == prefab)
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

                    case TilePropertyType.Cell:
                        prefab = cellEditorPrefab;
                        break;
                    case TilePropertyType.IntArray:
                        prefab = numberArrayEditorPrefab;
                        break;
                    case TilePropertyType.Bool:
                        prefab = boolEditorPrefab;
                        break;
                    case TilePropertyType.Background:
                        prefab = backgroundEditorPrefab;
                        break;
                    case TilePropertyType.Guid:
                        prefab = tileEditorPrefab;
                        break;
                    case TilePropertyType.Sound:
                        prefab = soundEditorPrefab;
                        break;
                    case TilePropertyType.Color:
                        prefab = colorEditorPrefab;
                        break;
                    case TilePropertyType.SoundArray:
                        prefab = soundArrayEditorPrefab;
                        break;
                    case TilePropertyType.Decal:
                        prefab = decalEditorPrefab;
                        break;
                    case TilePropertyType.DecalArray:
                        prefab = decalArrayEditorPrefab;
                        break;
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

        private void OnInspectorTileNameChanged(string name)
        {
            if (inspectorTile == null)
                return;

            name = name.Trim();

            // Name hasnt changed?
            if (name == inspectorTile.name)
                return;

            // Do not allow empty names, reset back to previous name
            if (string.IsNullOrWhiteSpace(name))
            {
                _inspectorTileName.SetTextWithoutNotify(inspectorTile.name);
                return;
            }

            ExecuteCommand(new Commands.TileRenameCommand(inspectorTile, name));
        }
    }
}
