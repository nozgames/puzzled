using NoZ;
using UnityEngine;
using UnityEngine.Rendering;

namespace Puzzled
{
    public class PostProcSepia : PostProcEffect
    {
        protected override void UpdatePostProc()
        {
            PostProcManager.sepia.blend.value = strengthFraction * blendScale;
        }
    }
}
