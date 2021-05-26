using System.Collections;
using UnityEngine;

namespace Puzzled
{
    public class UIScreen : MonoBehaviour
    {
        private void OnEnable()
        {
            IEnumerator EndOfFrameCoroutine()
            {
                yield return new WaitForEndOfFrame();
                OnScreenActivated();
            }

            StartCoroutine(EndOfFrameCoroutine());
        }

        private void OnDisable()
        {
            OnScreenDeactivated();
        }

        virtual protected void OnScreenActivated()
        {
        }

        virtual protected void OnScreenDeactivated()
        {
        }

        virtual public void HandleUpInput()
        {
        }

        virtual public void HandleDownInput()
        {
        }

        virtual public void HandleLeftInput()
        {
        }

        virtual public void HandleRightInput()
        {
        }

        virtual public void HandleConfirmInput()
        {
        }

        virtual public void HandleCancelInput()
        {
        }

        virtual public void HandleMenuInput()
        {
        }

        virtual public void HandleOptionInput()
        {
        }
    }
}
