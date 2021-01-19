using UnityEngine;

namespace Puzzled
{
    class MaterialSwap : MonoBehaviour
    {
        [SerializeField] private Renderer _renderer = null;
        [SerializeField] private Material[] _materials = null;

        private void OnEnable()
        {
            if (_renderer != null)
                _renderer.sharedMaterials = _materials;
        }
    }
}
