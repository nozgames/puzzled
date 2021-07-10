using UnityEngine;

namespace Puzzled
{
    class MeshSortingOrder : MonoBehaviour
    {
        [SerializeField] private int sortingOrder = 0;
        [SerializeField] private string sortingLayerName;

        private void Awake()
        {
            var renderer = GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.sortingOrder = sortingOrder;
                renderer.sortingLayerName = sortingLayerName;
            }
        }
    }
}
