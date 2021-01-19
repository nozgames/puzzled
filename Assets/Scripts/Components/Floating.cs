using UnityEngine;
using NoZ;

namespace Puzzled
{
    class Floating : MonoBehaviour
    {
        [SerializeField] private float minHeight = 0;
        [SerializeField] private float maxHeight = 4;

        private void OnEnable()
        {
            Tween.Move(new Vector3(0, minHeight, 0), new Vector3(0, maxHeight, 0), true)
                .Duration(1.0f)
                .PingPong()
                .EaseInOutCubic()
                .Loop()
                .Start(gameObject);
        }

        private void OnDisable()
        {
            Tween.Stop(gameObject);
        }
    }
}
