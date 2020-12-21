using UnityEngine;
using UnityEngine.UI;

namespace Puzzled
{
    public class ImageColor : MonoBehaviour
    {
        [SerializeField] private Image _image = null;
        [SerializeField] private Color _color = Color.white;

        private void OnEnable()
        {
            _image.color = _color;
        }
    }
}
