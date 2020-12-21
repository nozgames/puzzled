using UnityEngine;

namespace Puzzled.Editor.Commands
{
    public abstract class Command
    {
        private Tile selectedTile;
        private UIPuzzleEditor.Mode mode;

        public bool isExecuted { get; private set; }

        public Command ()
        {
            selectedTile = UIPuzzleEditor.selectedTile;
            mode = UIPuzzleEditor.instance.mode;
        }

        public void Execute ()
        {
            isExecuted = true;
            OnExecute();
            UIPuzzleEditor.instance.mode = mode;
            UIPuzzleEditor.selectedTile = selectedTile;
        }

        public void Undo ()
        {
            OnUndo();
            isExecuted = false;
            UIPuzzleEditor.instance.mode = mode;
            UIPuzzleEditor.selectedTile = selectedTile;
        }

        public void Redo()
        {
            isExecuted = true;
            OnRedo();
            UIPuzzleEditor.instance.mode = mode;
            UIPuzzleEditor.selectedTile = selectedTile;
        }

        public void Destroy()
        {
            OnDestroy();
        }

        protected abstract void OnExecute();

        protected abstract void OnUndo();

        protected virtual void OnRedo() => OnExecute();

        protected virtual void OnDestroy() { }
    }
}
