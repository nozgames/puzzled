using UnityEngine;

namespace Puzzled
{
    public static class GameObjectExtensions
    {
        public static void SetChildLayers (this GameObject gameObject, int layer)
        {
            gameObject.layer = layer;
            for (int i = gameObject.transform.childCount - 1; i >= 0; i--)
                gameObject.transform.GetChild(i).gameObject.SetChildLayers(layer);
        }
    }
}
