using System;
using UnityEngine;

namespace Puzzled.UI
{
    class UIList : MonoBehaviour
    {
        public int selected { get; private set; } = -1;

        public UIListItem selectedItem => selected == -1 ? null : GetItem(selected);

        public int itemCount => transform.childCount;

        public event Action<int> onSelectionChanged;

        public event Action<int> onDoubleClickItem;

        public event Action<int,int> onReorderItem;

        private void OnTransformChildrenChanged()
        {
            if (selected >= transform.childCount)
                selected = transform.childCount - 1;
        }

        public void Select(int index)
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

            var old = selected;
            selected = index;

            if (old >= 0)
                GetItem(old).selected = false;

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
    }
}
