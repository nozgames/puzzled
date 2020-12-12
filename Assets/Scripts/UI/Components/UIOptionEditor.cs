using System.Text;
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

        private object _target;

        public object target {
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
        }
    }
}
