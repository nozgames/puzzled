﻿using NoZ;
using UnityEngine;
using UnityEngine.Rendering;

namespace Puzzled
{
    public class PostProcBlackAndWhite : PostProcEffect
    {
        protected override void UpdatePostProc()
        {
            PostProcManager.blackAndWhite.blend.value = strengthFraction * blendScale;
        }
    }
}
