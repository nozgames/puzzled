using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

using Object = UnityEngine.Object;

namespace Puzzled
{
    [InitializeOnLoad]
    static class ResourceInspectorGUI
    {
        static ResourceInspectorGUI()
        {
            UnityEditor.Editor.finishedDefaultHeaderGUI -= OnPostHeaderGUI;
            UnityEditor.Editor.finishedDefaultHeaderGUI += OnPostHeaderGUI;
        }

        static private T GetTarget<T> (Object target, bool importer = false) where T : Object
        {
            if (!importer)
                return target as T;

            var assetImporter = target as AssetImporter;
            if (null == assetImporter)
                return null;

            var assetType = AssetDatabase.GetMainAssetTypeAtPath(assetImporter.assetPath);
            if (assetType != typeof(T))
                return AssetDatabase.LoadAllAssetsAtPath(assetImporter.assetPath).FirstOrDefault(a => a is T) as T;

            return AssetDatabase.LoadAssetAtPath<T>(assetImporter.assetPath);
        }

        static void OnPostHeaderGUI(UnityEditor.Editor editor)
        {
            if (editor.target == null)
                return;

            if (ResourceCheckbox(editor, TileDatabase.GetInstance(), true, (a) => a != null && a.GetComponent<Tile>() != null))
                return;

            if (ResourceCheckbox(editor, BackgroundDatabase.GetInstance()))
                return;

            if (ResourceCheckbox(editor, SoundDatabase.GetInstance(), true))
                return;

            if (ResourceCheckbox(editor, DecalDatabase.GetInstance(), true))
                return;
        }

        public static void DrawUILine(Color color, int thickness = 2, int padding = 10)
        {
            Rect r = EditorGUILayout.GetControlRect(GUILayout.Height(padding + thickness));
            r.height = thickness;
            r.y += padding / 2;
            r.x -= 2;
            r.width += 6;
            EditorGUI.DrawRect(r, color);
        }

        private static bool ResourceCheckbox<TAsset> (UnityEditor.Editor editor, ResourceDatabase<TAsset> database, bool importer = false, Func<TAsset,bool> filter = null) where TAsset : Object
        {
            if (filter == null)
                filter = (a) => a != null;

            var target = GetTarget<TAsset>(editor.target, importer);
            if (null == target)
                return false;

            if (!filter(target))
                return false;

            var targets = editor.targets.Select(t => GetTarget<TAsset>(t, importer)).Where(t => t != null && filter(t)).ToArray();
            var value = database.Contains(targets[0]);
            var multi = false;
            for(int i=1; i< targets.Length; i++)
                if(database.Contains(targets[i]) != value)
                {
                    multi = true;
                    break;
                }

            DrawUILine(Color.black, 1);
//            EditorGUI.indentLevel++;
            EditorGUI.BeginChangeCheck();
            EditorGUI.showMixedValue = multi;
            var result = EditorGUILayout.ToggleLeft($"{database.GetType().Name} Resource", value);
            EditorGUI.showMixedValue = false;
  //          EditorGUI.indentLevel--;

            if (EditorGUI.EndChangeCheck())
            {
                if (result)
                {
                    foreach (var asset in targets)
                        database.Add(asset);
                }
                else
                {
                    foreach (var asset in targets)
                        database.Remove(asset);
                }
            }

            return true;
        }
    }
}
