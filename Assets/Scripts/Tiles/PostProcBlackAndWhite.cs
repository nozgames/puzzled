namespace Puzzled
{
    public class PostProcBlackAndWhite : PostProcEffect
    {
        protected override void UpdatePostProc()
        {
            PostProcManager.SetBlend(PostProcManager.blackAndWhite, strengthFraction * blendScale);
        }
    }
}
