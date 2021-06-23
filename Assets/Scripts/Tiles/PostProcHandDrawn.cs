using NoZ;
using UnityEngine;
using UnityEngine.Rendering;

namespace Puzzled
{
    public class PostProcHandDrawn : PostProcEffect
    {
        protected override void UpdatePostProc()
        {
            PostProcManager.handDrawn.blend.value = strengthFraction * blendScale;
        }
    }
}
