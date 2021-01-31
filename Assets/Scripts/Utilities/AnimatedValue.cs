using UnityEngine;

namespace Puzzled
{
    public class AnimatedValue<T> 
    {
        public T from;
        public T to;
        public T value;

        public void Set(T value)
        {
            from = to = this.value = value;
        }

        public void Set(T from, T to)
        {
            this.from = value = from;
            this.to = to;
        }
    }

    public static class AnimatedValue
    {
        public static void Animate (this AnimatedValue<float> transition, float t)
        {
            transition.value = Mathf.Lerp(transition.from, transition.to, t);
        }

        public static void Animate(this AnimatedValue<Color> transition, float t)
        {
            transition.value = Color.Lerp(transition.from, transition.to, t);
        }

        public static void Animate(this AnimatedValue<Vector3> transition, float t)
        {
            transition.value = Vector3.Lerp(transition.from, transition.to, t);
        }
    }
}
