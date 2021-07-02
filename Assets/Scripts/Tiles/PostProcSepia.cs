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
