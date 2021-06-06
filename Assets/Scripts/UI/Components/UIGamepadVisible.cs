using System;
using UnityEngine;

namespace Puzzled.UI
{
    /// <summary>
    /// Sets the visiblity state of the given targets based on whether or not the gamepad is available
    /// </summary>
    class UIGamepadVisible : MonoBehaviour
    {
        [Tooltip("Set to true to invert the logic and hide when gamepad is available")]
        [SerializeField] private bool _inverted = false;

        [SerializeField] private GameObject[] _targets = null;


        private void OnEnable()
        {
            OnGamepadChanged(GameManager.isUsingGamepad);

            GameManager.onGamepadChanged += OnGamepadChanged;
        }

        private void OnDisable()
        {
            GameManager.onGamepadChanged -= OnGamepadChanged;
        }

        private void OnGamepadChanged(bool isGamepad)
        {
            var active = isGamepad == !_inverted;
            if (null == _targets || _targets.Length == 0)
            {
                gameObject.SetActive(active);
            }
            else
            {
                foreach (var target in _targets)
                    gameObject.SetActive(active);
            }
        }
    }
}
