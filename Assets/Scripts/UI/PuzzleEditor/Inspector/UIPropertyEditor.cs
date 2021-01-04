using System;
using UnityEngine;

namespace Puzzled
{
    public class UIPropertyEditor : MonoBehaviour
    {
        [SerializeField] private TMPro.TextMeshProUGUI labelText = null;
        [SerializeField] private bool _grouped = false;

        protected string label {
            get => labelText.text;
            set => labelText.text = value;
        }

        public bool isGrouped => _grouped;

        public event Action<TilePropertyEditorTarget> onTargetChanged;

        private TilePropertyEditorTarget _target;

        public TilePropertyEditorTarget target {
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
            onTargetChanged?.Invoke(target);
        }
    }
}
