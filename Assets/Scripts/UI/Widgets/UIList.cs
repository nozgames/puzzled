using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Puzzled.UI
{
    class UIList : Selectable
    {
        public int selected => GetSelectedIndex();

        public UIListItem selectedItem => GetItem(GetSelectedIndex());

        public int itemCount => transform.childCount;

        public event Action<int> onSelectionChanged;

        public event Action<int> onDoubleClickItem;

        public event Action<int,int> onReorderItem;

        private int GetSelectedIndex ()
        {
            for(int i=0; i<transform.childCount; i++)
            {
                if (transform.GetChild(i).GetComponent<UIListItem>().selected)
                    return i;
            }

            return -1;
        }

        public void SelectItem(int index)
        {
            if(index < 0)
            {
                ClearSelection();
                return;
            }

            SetSelection(Mathf.Min(index, transform.childCount - 1));
        }

        private void SetSelection(int index)
        {
            if (selected == index)
                return;

            if(index >=0)
                GetItem(index).selected = true;

            onSelectionChanged?.Invoke(index);
        }

        public void ClearSelection() => SetSelection(-1);

        public UIListItem GetItem(int index) => transform.GetChild(index).GetComponent<UIListItem>();

        public void OnDoubleClickItem(int index)
        {
            onDoubleClickItem?.Invoke(index);
        }

        public void OnReorderItem(int from, int to)
        {
            onReorderItem?.Invoke(from, to);
        }

        override public void OnMove(AxisEventData eventData)
        {
            int newSelection = Mathf.Clamp(selected + ((eventData.moveDir == MoveDirection.Down) ? 1 : -1), 0, itemCount - 1);
            SelectItem(newSelection);
        }
    }
}
