using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Puzzled
{
    public class UIActionButtonImage : MonoBehaviour
    {
        [SerializeField] private InputActionReference _action = null;
        [SerializeField] private Sprite _gamepadSprite = null;
        [SerializeField] private Sprite _keyboardSprite = null;
        [SerializeField] private Image _image = null;
        [SerializeField] private Button _button = null;

        private float _openedTime;

        private void OnEnable()
        {
            _action.action.performed += OnActionButton;

            _openedTime = Time.time;

            if (GameManager.isUsingGamepad)
                _image.sprite = _gamepadSprite;
            else
                _image.sprite = _keyboardSprite;
        }

        private void OnDisable()
        {
            _action.action.performed -= OnActionButton;
        }

        private void OnActionButton(InputAction.CallbackContext ctx)
        {
            if (Time.time - _openedTime < 0.5)
                return;

            if(ctx.ReadValueAsButton())
                _button.onClick?.Invoke();
        }
    }
}

