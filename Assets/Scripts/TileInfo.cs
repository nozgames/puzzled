using System;
using UnityEngine;

namespace Puzzled
{
    [CreateAssetMenu(fileName = "New Tile Info", menuName = "Puzzled/Tile Info")]
    public class TileInfo : ScriptableObject
    {
        [SerializeField] private string _displayName = "";
        [SerializeField] private string description;

        [Tooltip("True if this tile can be added to a puzzled more than once")]
        [SerializeField] private bool _allowMultiple = true;

        [Tooltip("True if a dynamic tile can be placed on top on top of this tile")]
        [SerializeField] private bool _allowDynamic = false;
        
        [Tooltip("Layer the tile is linked to")]
        [SerializeField] private TileLayer _layer = TileLayer.Static;

        [Tooltip("True if the the tile should no longer be used")]
        [SerializeField] private bool _deprecated = false;

        [Serializable]
        public struct CustomOptionEditor
        {
            public GameObject prefab;
            public int order;
        }

        [Header("Editor")]
        [SerializeField] private GameObject _inputsPrefab = null;
        [SerializeField] private GameObject _outputsPrefab = null;
        [SerializeField] private CustomOptionEditor[] _customEditorPrefabs = null;

        public bool allowMultiple => _allowMultiple;

        /// <summary>
        /// Returns true if a static tile allows a dynamic tile to be placed on top of it
        /// </summary>
        public bool allowDynamic => _allowDynamic;

        public bool isDeprecated => _deprecated;

        public string displayName => string.IsNullOrEmpty(_displayName) ? name : _displayName;

        public CustomOptionEditor[] customOptionEditors => _customEditorPrefabs;
        public GameObject inputsPrefab => _inputsPrefab;
        public GameObject outputsPrefab => _outputsPrefab;

        public TileLayer layer => _layer;        
    }
}

