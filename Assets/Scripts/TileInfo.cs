using System;
using UnityEngine;
using UnityEngine.Serialization;

using Puzzled.Editor;

namespace Puzzled
{
    [CreateAssetMenu(fileName = "New Tile Info", menuName = "Puzzled/Tile Info")]
    public class TileInfo : ScriptableObject
    {
        [SerializeField] private string _displayName = "";
        [SerializeField] private string description;

        [Tooltip("Layer the tile is linked to")]
        [SerializeField] private TileLayer _layer = TileLayer.Static;

        [Tooltip("Category the tile is in")]
        [SerializeField] private TileCategory _category = TileCategory.None;

        [Serializable]
        public class CustomPropertyEditor
        {
            public string name;
            public UIPropertyEditor prefab;
        }

        [Header("Preview")]
        public float previewPitch = 45.0f;
        public bool previewOrthographic = false;
        public float previewSize = 1.0f;

        [Header("Editor")]
        [Tooltip("True if this tile can be added to a puzzled more than once")]
        [SerializeField] private bool _allowMultiple = true;

        [Tooltip("True if a dynamic tile can be placed on top on top of this tile")]
        [SerializeField] private bool _allowDynamic = false;
        [Tooltip("True if the the tile should no not be visible in the tile palette")]
        [FormerlySerializedAs("_deprecated")]
        [SerializeField] private bool _hidden = false;

        [SerializeField] private UIInspectorEditor[] _customEditors = null;
        [SerializeField] private CustomPropertyEditor[] _customPropertyEditors = null;

        public bool allowMultiple => _allowMultiple;

        /// <summary>
        /// Returns true if a static tile allows a dynamic tile to be placed on top of it
        /// </summary>
        public bool allowDynamic => _allowDynamic;

        public bool isHidden => _hidden;

        public string displayName => string.IsNullOrEmpty(_displayName) ? name : _displayName;

        public UIInspectorEditor[] customEditors => _customEditors;

        public CustomPropertyEditor[] customPropertyEditors => _customPropertyEditors;
        
        public TileLayer layer => _layer;

        public TileCategory category => _category;

        /// <summary>
        /// Return the custom property editor for the given property
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public CustomPropertyEditor GetCustomPropertyEditor(TileProperty property)
        {
            if (null == _customPropertyEditors)
                return null;

            foreach (var editor in _customPropertyEditors)
                if (string.Compare(editor.name, property.name, true) == 0)
                    return editor;

            return null;
        }
    }
}

