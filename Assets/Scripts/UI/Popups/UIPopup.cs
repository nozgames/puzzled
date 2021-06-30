using UnityEngine;
using System;

namespace Puzzled.UI
{
    public class UIPopup : MonoBehaviour
    {
        public Action doneCallback;

        public virtual void Use()
        {
        }

        public void Close()
        {
            UIManager.ClosePopup();
            Action callback = doneCallback;
            doneCallback = null;
            callback?.Invoke();           
        }

        private void OnEnable()
        {
            GameManager.busy++;
        }

        private void OnDisable()
        {
            GameManager.busy--;
        }

        public virtual void HandleConfirmInput ()
        {
        }

        public virtual void HandleCancelInput()
        {
        }
    }
}
