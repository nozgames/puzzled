using UnityEngine;

namespace Puzzled
{
    public class UIOptionEditor : MonoBehaviour
    {
        [SerializeField] private TMPro.TextMeshProUGUI labelText = null;

        public string label {
            get => labelText.text;
            set {
                labelText.text = value;
            }
        }
    }
}
