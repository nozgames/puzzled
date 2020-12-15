using UnityEngine;

namespace Puzzled
{
    public class UIPopup : MonoBehaviour
    {
        public void Close()
        {
            UIManager.ClosePopup();
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
