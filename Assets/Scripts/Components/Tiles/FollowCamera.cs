using NoZ;
using UnityEngine;

namespace Puzzled
{
    public class FollowCamera : GameCamera
    {
        public override Vector3 target => 
            puzzle.player != null ? 
                puzzle.grid.CellToWorld(puzzle.player.tile.cell) + new Vector3(offset.x * 0.25f, offset.y * 0.25f) : 
                base.target;

        public override void OnCameraStop()
        {
            CameraManager.StopFollow();
        }

        public override void OnCameraStart(int transitionTime)
        {
            if(isEditing)
                CameraManager.Transition(puzzle.player.tile.transform.position, pitch, zoomLevel, background, transitionTime);
            else
                CameraManager.Follow(puzzle.player.tile.transform, pitch, zoomLevel, background, transitionTime);
        }
    }
}
