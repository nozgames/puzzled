using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Puzzled
{
    [CustomEditor(typeof(Theme))]
    public class ThemeEditor : Editor
    {
        private Theme theme;
        private TileType[] sorted;

        private void OnEnable()
        {
            theme = (Theme)target;

            sorted = ((TileType[])Enum.GetValues(typeof(TileType))).OrderBy(id => id.ToString()).ToArray();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            var changed = false;

            foreach(var id in sorted)
            {
                EditorGUI.BeginChangeCheck();
                var prefab = (GameObject)EditorGUILayout.ObjectField(id.ToString(), theme.GetPrefab(id), typeof(GameObject), false);
                if (EditorGUI.EndChangeCheck())
                {
                    theme.SetPrefab(id, prefab);
                    changed = true;
                }
            }

            if (changed)
                serializedObject.ApplyModifiedProperties();
        }
    }
}
