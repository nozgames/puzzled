using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Puzzled
{
    public class UIActionButtonImage : MonoBehaviour
    {
        [SerializeField] private Sprite _gamepadSprite = null;
        [SerializeField] private Sprite _keyboardSprite = null;
        [SerializeField] private Image _image = null;

        private void OnEnable()
        {
            if (GameManager.isUsingGamepad)
                _image.sprite = _gamepadSprite;
            else
                _image.sprite = _keyboardSprite;
        }
    }
}

