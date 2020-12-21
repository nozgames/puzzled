using UnityEngine;
using UnityEngine.UI;

namespace Puzzled
{
    public class SpriteColor : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _renderer = null;
        [SerializeField] private Color _color = Color.white;
        [SerializeField] private Material _material = null;

        private void OnEnable()
        {
            _renderer.color = _color;

            if(_material != null)
                _renderer.sharedMaterial = _material;
        }
    }
}
