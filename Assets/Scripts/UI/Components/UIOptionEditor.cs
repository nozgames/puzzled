using System;
using UnityEngine;

namespace Puzzled
{
    public class UIOptionEditor : MonoBehaviour
    {
        [SerializeField] private TMPro.TextMeshProUGUI labelText = null;

        protected string label {
            get => labelText.text;
            set => labelText.text = value;
        }

        public event Action<object> onTargetChanged;

        private TilePropertyEditorTarget _target;

        public TilePropertyEditorTarget target {
            get => _target;
            set {
                _target = value;

                if (_target == null)
                    return;

                OnTargetChanged(target);
            }
        }

        protected virtual void OnTargetChanged(object target)
        {
            onTargetChanged?.Invoke(target);
        }
    }
}
