using UnityEngine;

namespace Puzzled.Editor
{
    public class CameraBoundsGizmo : MonoBehaviour
    {
        [SerializeField] private Transform _handleTL = null;
        [SerializeField] private Transform _handleTR = null;
        [SerializeField] private Transform _handleBL = null;
        [SerializeField] private Transform _handleBR = null;
        [SerializeField] private LineRenderer _lineL = null;
        [SerializeField] private LineRenderer _lineT = null;
        [SerializeField] private LineRenderer _lineR = null;
        [SerializeField] private LineRenderer _lineB = null;

        [SerializeField] private Camera _camera = null;

        private static Vector3 GetPointAtHeight(Ray ray, float height) =>
            ray.origin + (((ray.origin.y - height) / -ray.direction.y) * ray.direction);

        private void OnEnable()
        {
            UpdateTransforms();
        }

        public void UpdateTransforms()
        { 
            var raytr = GetPointAtHeight(_camera.ViewportPointToRay(new Vector3(1, 1, 0)), 0);
            var raytl = GetPointAtHeight(_camera.ViewportPointToRay(new Vector3(0, 1, 0)), 0);
            var raybl = GetPointAtHeight(_camera.ViewportPointToRay(new Vector3(0, 0, 0)), 0);
            var raybr = GetPointAtHeight(_camera.ViewportPointToRay(new Vector3(1, 0, 0)), 0);

            _lineL.positionCount = 2;
            _lineL.SetPosition(0, raytl);
            _lineL.SetPosition(1, raybl);

            _lineR.positionCount = 2;
            _lineR.SetPosition(0, raytr);
            _lineR.SetPosition(1, raybr);

            _lineB.positionCount = 2;
            _lineB.SetPosition(0, raybl);
            _lineB.SetPosition(1, raybr);

            _lineT.positionCount = 2;
            _lineT.SetPosition(0, raytl);
            _lineT.SetPosition(1, raytr);

            _handleBL.transform.position = raybl;
            _handleBR.transform.position = raybr;
            _handleTL.transform.position = raytl;
            _handleTR.transform.position = raytr;
        }
    }
}
