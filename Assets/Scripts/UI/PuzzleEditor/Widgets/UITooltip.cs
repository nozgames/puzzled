using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Puzzled.Editor
{
    public enum TooltipDirection
    {
        Top,
        Bottom,
        Right,
        Left
    }

    public class UITooltip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private TooltipDirection _direction = TooltipDirection.Bottom;
        [Multiline]
        [SerializeField] private string _text = "";

        public void OnPointerEnter(PointerEventData eventData)
        {
            UIPuzzleEditor.ShowTooltip(GetComponent<RectTransform>(), _text, _direction);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            UIPuzzleEditor.HideTooltip();
        }
    }
}
