using System.Collections;
using UnityEngine;

namespace Puzzled
{
    public class UIScreen : MonoBehaviour
    {
        virtual public bool showNavigationBar => true;
        virtual public bool showConfirmButton => false;
        virtual public bool showCancelButton => false;
        virtual public bool showOptionButton => false;
        virtual public string confirmButtonText => "Select";
        virtual public string cancelButtonText => "Back";
        virtual public string optionButtonText => "Option";

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
