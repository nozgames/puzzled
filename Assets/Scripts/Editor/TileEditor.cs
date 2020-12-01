using UnityEngine;
using UnityEditor;

namespace Puzzled
{
    [CustomEditor(typeof(Tile))]
    public class TileEditor : Editor
    {
        public void OnEnable()
        {
            var tile = (Tile)target;
            if (string.IsNullOrEmpty(tile.guid))
            {
                tile.guid = System.Guid.NewGuid().ToString();
                EditorUtility.SetDirty(tile.gameObject);
            }
        }

        public override void OnInspectorGUI()
        {
            GUI.enabled = false;
            EditorGUILayout.TextField("GUID", ((Tile)target).guid);
            GUI.enabled = true;
            base.OnInspectorGUI();
        }
    }
}
