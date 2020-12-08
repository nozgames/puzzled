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

        private static StringBuilder _nicifyBuilder = new StringBuilder();

        protected string NicifyName(string name)
        {
            _nicifyBuilder.Clear();
            _nicifyBuilder.Capacity = name.Length * 2;
            for (int i = 0; i < name.Length; i++)
            {
                if (char.IsUpper(name[i]))
                {
                    _nicifyBuilder.Append(' ');
                    _nicifyBuilder.Append(name[i]);
                } 
                else
                {
                    if (i==0)
                        _nicifyBuilder.Append(char.ToUpper(name[i]));
                    else
                        _nicifyBuilder.Append(name[i]);
                }
            }

            return _nicifyBuilder.ToString();
        }
    }
}
