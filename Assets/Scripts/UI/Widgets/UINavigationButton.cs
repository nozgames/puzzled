using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Puzzled
{
    public class UINavigationButton : MonoBehaviour
    {
        [SerializeField] private Button _button;
        [SerializeField] private TMPro.TextMeshProUGUI _text;
        [SerializeField] private GameObject _gamepadIcon;

        public void RegisterButtonClickHandler(UnityAction call)
        {
            _button.onClick.AddListener(call);
        }

        public void SetState(bool showButton, string text)
        {
            gameObject.SetActive(showButton);
            _text.text = text;
        }
    }
}
