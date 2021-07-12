using UnityEngine;

namespace Puzzled
{
    public static class TransformExtensions 
    {
        public static void DestroyChildren (this Transform transform)
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
                UnityEngine.Object.Destroy(transform.GetChild(i).gameObject);
        }

        public static void DestroyChildrenPooled(this Transform transform)
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
                transform.GetChild(i).gameObject.DestroyPooled();
        }

        public static void DetachAndDestroyChildren(this Transform transform)
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                var go = transform.GetChild(i).gameObject;
                go.transform.SetParent(null);
                UnityEngine.Object.Destroy(go);
            }
        }

        public static void DetachAndDestroyChildrenPooled(this Transform transform)
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
                transform.GetChild(i).gameObject.DestroyPooled();
        }

        public static void DisableChildren(this Transform transform)
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
                transform.GetChild(i).gameObject.SetActive(false);
        }
    }
}
