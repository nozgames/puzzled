using System;
using UnityEngine;
using NoZ;

namespace Puzzled
{
    public static class TweenAnimations
    {
        private static bool ArcMove (Tween tween, float t)
        {
            ((GameObject)tween.TargetObject).transform.position =
                    ((Vector3)tween.Param1 * (1.0f - t)) +
                    ((Vector3)tween.Param2) * t +
                    Vector3.up * Mathf.Sin(t * Mathf.PI) * tween.Param1.w;
            return true;
        }

        public static Tween ArcMove (Vector3 from, Vector3 to, float height)
        {
            var param1 = (Vector4)from;
            var param2 = (Vector4)to;
            param1.w = height;
            return Tween.Custom(ArcMove, param1, param2);
        }
    }
}
