using UnityEngine;
using NoZ;

class CameraFollow : MonoBehaviour
{
#if false
    public Transform Target;
    public float FollowSpeed = 1.0f;

    public Transform LimitLeft = null;
    public Transform LimitRight = null;
    public Transform LimitTop = null;
    public Transform LimitBottom = null;

    private Camera _camera;

    private void Start()
    {
        Player.PlayerSpawnEvent.Subscribe(OnPlayerSpawn);
        _camera = GetComponent<Camera>();
    }

    private void OnPlayerSpawn(Player player)
    {
        Target = player.transform;

        transform.position = Target.position;
    }

    private void FixedUpdate()
    {
        var delta = (transform.position - Target.position);
        var mag = delta.sqrMagnitude * Time.deltaTime * FollowSpeed;
        if(mag > delta.magnitude)
            transform.position = Target.position;
        else
            transform.position += delta.normalized * -mag;

        var bounds = _camera.OrthoBounds();
        if (LimitLeft != null && bounds.x < LimitLeft.position.x)
            transform.position += new Vector3(LimitLeft.position.x - bounds.x, 0, 0);
        if (LimitBottom != null && bounds.y < LimitBottom.position.y)
            transform.position += new Vector3(0, LimitBottom.position.y - bounds.y, 0);
        if (LimitRight != null && bounds.x + bounds.width > LimitRight.position.x)
            transform.position -= new Vector3(bounds.x+bounds.width-LimitRight.position.x, 0, 0);
        if (LimitTop != null && bounds.y + bounds.height > LimitTop.position.y)
            transform.position -= new Vector3(0, bounds.y+bounds.height-LimitTop.position.y, 0);
    }
#endif
}
