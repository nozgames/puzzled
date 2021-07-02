namespace Puzzled
{
    public class PostProcOldFilm : PostProcEffect
    {
        protected override void UpdatePostProc()
        {
            PostProcManager.oldFilm.blend.value = strengthFraction * blendScale;
        }
    }
}
