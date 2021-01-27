using NoZ;
using UnityEngine;

namespace Puzzled
{
    public class FollowCamera : GameCamera
    {
        public override void OnCameraStop()
        {
            CameraManager.StopFollow();
        }

        public override void OnCameraStart(int transitionTime)
        {
            CameraManager.Follow(puzzle.player, zoomLevel, background, transitionTime);
        }
    }
}
