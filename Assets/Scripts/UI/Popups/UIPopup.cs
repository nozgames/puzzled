using UnityEngine;
using System;

namespace Puzzled
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
    }
}
