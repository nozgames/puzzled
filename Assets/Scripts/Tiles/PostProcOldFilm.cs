namespace Puzzled
{
    public class PostProcOldFilm : PostProcEffect
    {
        protected override void UpdatePostProc()
        {
            PostProcManager.SetBlend(PostProcManager.oldFilm, strengthFraction * blendScale);
        }
    }
}
