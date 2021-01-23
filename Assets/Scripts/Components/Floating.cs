using UnityEngine;
using NoZ;

namespace Puzzled
{
    class Floating : MonoBehaviour
    {
        [SerializeField] private GameObject _target = null;
        [SerializeField] private float minHeight = 0;
        [SerializeField] private float maxHeight = 4;        

        private void OnEnable()
        {
            Tween.Move(new Vector3(0, minHeight, 0), new Vector3(0, maxHeight, 0), true)
                .Duration(1.0f)
                .PingPong()
                .EaseInOutCubic()
                .Loop()
                .Start(_target != null ? _target : gameObject);
        }

        private void OnDisable()
        {
            var target = _target != null ? _target : gameObject;
            Tween.Stop(target);
            target.transform.localPosition = new Vector3(0, minHeight, 0);
        }
    }
}
