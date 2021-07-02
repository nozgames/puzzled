namespace Puzzled
{
    public class PostProcSepia : PostProcEffect
    {
        protected override void UpdatePostProc()
        {
            PostProcManager.SetBlend(PostProcManager.sepia, strengthFraction * blendScale);
        }
    }
}
