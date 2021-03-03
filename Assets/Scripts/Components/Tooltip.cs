using NoZ;
using UnityEngine;

namespace Puzzled
{
    public enum TooltipDirection
    {
        Top,
        Bottom,
        Right,
        Left
    }

    class Tooltip : TileComponent
    {
        [SerializeField] private string _text = null;
        [SerializeField] private float _height = 0.0f;

        public float height => _height;

        [ActorEventHandler]
        private void OnQueryTooltipEvent (QueryTooltipEvent evt)
        {
            if (!string.IsNullOrEmpty(_text))
            {
                evt.IsHandled = true;
                evt.tooltip = _text;
                evt.height = _height;
            }
        }
    }
}
