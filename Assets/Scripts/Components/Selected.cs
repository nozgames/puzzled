using System.Linq;
using Puzzled.Editor;
using UnityEngine;

namespace Puzzled
{
    public class Selected : MonoBehaviour
    {
        private Renderer[] _renderers = null;
        private int[] _layers = null;
        private Color[] _oldColors = null;

        private static int _selectedLayer = -1;
        private const string _ignoreSelectionTag = "IgnoreSelection";

        private void OnEnable()
        {
            if (_selectedLayer == -1)
                _selectedLayer = LayerMask.NameToLayer("Selected");

            _renderers = GetComponentsInChildren<Renderer>().Where(r => r.tag != _ignoreSelectionTag).ToArray();
            _layers = new int[_renderers.Length];
            _oldColors = new Color[_renderers.Length];
            for (int i=0; i<_renderers.Length; i++)
            {
                _layers[i] = _renderers[i].gameObject.layer;

                var renderer = _renderers[i];
                if (renderer is SpriteRenderer spriteRenderer)
                {
                    _oldColors[i] = spriteRenderer.color;
                    spriteRenderer.color = UIPuzzleEditor.selectionColor;
                } else
                    renderer.gameObject.layer = _selectedLayer;
            }
        }

        private void OnDisable()
        {
            if(_renderers != null && _layers != null)
            {
                for(int i=0; i < _layers.Length; i++)
                {
                    var renderer = _renderers[i];
                    if (renderer is SpriteRenderer spriteRenderer)
                        spriteRenderer.color = _oldColors[i];
                    else if(_renderers[i] != null)
                        _renderers[i].gameObject.layer = _layers[i];
                }
            }
        }
    }
}
