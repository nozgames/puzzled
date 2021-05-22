using UnityEngine;

namespace Puzzled
{
    public static class GameObjectExtensions
    {
        public static void SetChildLayers (this GameObject gameObject, int layer, int mask = int.MaxValue)
        {
            if((gameObject.layer & mask) != 0 || gameObject.layer == 0)
                gameObject.layer = layer;

            for (int i = gameObject.transform.childCount - 1; i >= 0; i--)
                gameObject.transform.GetChild(i).gameObject.SetChildLayers(layer, mask);
        }
    }
}
