using UnityEngine;

namespace Puzzled
{
    public class UIOptionEditor : MonoBehaviour
    {
        [SerializeField] private TMPro.TextMeshProUGUI labelText = null;

        private TileEditorInfo.EditableProperty _target;

        public TileEditorInfo.EditableProperty target {
            get => _target;
            set {
                _target = value;

                if (_target == null)
                    return;

                labelText.text = _target.property.Name;

                OnTargetChanged(target);
            }
        }

        protected virtual void OnTargetChanged(TileEditorInfo.EditableProperty target)
        {
        }
    }
}
