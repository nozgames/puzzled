using NoZ;
using UnityEngine;

namespace Puzzled
{
    public class StaticCamera : GameCamera
    {
        public override void OnCameraStart(int transitionTime)
        {
            // Tell the camera manager to transition
            CameraManager.Transition(puzzle.grid.CellToWorld(tile.cell + offset), zoomLevel, background, transitionTime);
        }
    }
}
