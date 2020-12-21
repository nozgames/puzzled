
namespace Puzzled
{
    public interface ICommand
    {
        void Undo();
        void Redo();
        void Destroy();
    }
}
