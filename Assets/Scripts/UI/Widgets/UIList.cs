﻿using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Puzzled.UI
{
    class UIList : Selectable
    {
        [SerializeField] private ScrollRect _scrollRect = null;

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
                var listItem = transform.GetChild(i).GetComponent<UIListItem>();
                if (listItem != null && listItem.selected)
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
            var old = selected;
            if (old == index)
                return;

            if (index >= 0)
                GetItem(index).selected = true;
            else if (old >= 0)
                GetItem(old).selected = false;                
        }

        public void OnSelectionChanged ()
        {
            onSelectionChanged?.Invoke(selected);
        }

        public void ClearSelection() => SetSelection(-1);

        public UIListItem GetItem(int index) => index < 0 ? null : transform.GetChild(index).GetComponent<UIListItem>();

        public void OnDoubleClickItem(int index)
        {
            onDoubleClickItem?.Invoke(index);
        }

        public void OnReorderItem(int from, int to)
        {
            onReorderItem?.Invoke(from, to);
        }

        public override void OnMove(AxisEventData eventData)
        {            
            var dir = ((eventData.moveDir == MoveDirection.Down) ? 1 : -1);
            for (int newSelection = selected + dir; newSelection >= 0 && newSelection < transform.childCount; newSelection += dir)
            { 
                if(GetItem(newSelection).interactable)
                {
                    SelectItem(newSelection);
                    ScrollTo(newSelection);
                    return;
                }
            }
        }

        public void ScrollTo(int itemIndex)
        {
            if (itemIndex < 0 || itemIndex >= itemCount)
                return;

            if (null != _scrollRect)
                _scrollRect.ScrollTo(GetItem(itemIndex).GetComponent<RectTransform>());
        }
    }
}
