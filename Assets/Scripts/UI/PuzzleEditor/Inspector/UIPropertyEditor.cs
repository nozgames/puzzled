using System;
using UnityEngine;

namespace Puzzled.Editor
{
    public interface IPropertyEditorTarget
    {
        string id { get; }

        string name { get; }

        string placeholder { get; }

        Vector2Int range { get; }

        void SetValue(object value, bool commit=true);

        object GetValue();

        public T GetValue<T>();
    }

    public class UIPropertyEditor : MonoBehaviour
    {
        [SerializeField] private TMPro.TextMeshProUGUI labelText = null;
        [SerializeField] private bool _grouped = false;

        public virtual bool isHidden => false;

        protected virtual string label => _target?.name ?? "";

        public bool isGrouped => _grouped;

        public event Action<IPropertyEditorTarget> onTargetChanged;

        private IPropertyEditorTarget _target;

        public IPropertyEditorTarget target {
            get => _target;
            set {
                _target = value;

                if (_target == null)
                    return;

                OnTargetChanged();
            }
        }

        protected virtual void OnTargetChanged()
        {
            UpdateLabel();
            onTargetChanged?.Invoke(target);
        }

        protected void UpdateLabel ()
        {
            labelText.text = label;
        }
    }
}
