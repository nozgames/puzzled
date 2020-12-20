using System;
using UnityEngine;

namespace Puzzled
{
    class UIList : MonoBehaviour
    {
        public int selected { get; private set; } = -1;

        public int itemCount => transform.childCount;

        public event Action<int> onSelectionChanged;

        private void OnTransformChildrenChanged()
        {
            if (selected >= transform.childCount)
                selected = transform.childCount - 1;
        }

        public void Select(int index)
        {
            if (index < 0 || index >= transform.childCount)
                return;

            SetSelection(index);
        }

        private void SetSelection(int index)
        {
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
    }
}
