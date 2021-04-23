using System;
using UnityEngine;

namespace Puzzled.Editor.Commands
{
    public abstract class Command
    {
        public (string name,object value)[] undoState { get; set; }
        public (string name,object value)[] redoState { get; set; }

        public UIPuzzleEditor.Mode mode { get; private set; }

        public Tile selectedTile { get; private set; }

        public Wire selectedWireUndo { get; set; }
        public Wire selectedWireRedo { get; set; }

        public bool isExecuted { get; private set; }

        /// <summary>
        /// Puzzle being edited
        /// </summary>
        public Puzzle puzzle => UIPuzzleEditor.instance.puzzle;

        public Command ()
        {
            selectedTile = UIPuzzleEditor.selectedTile;
            selectedWireUndo = UIPuzzleEditor.selectedWire;
            mode = UIPuzzleEditor.instance.mode;
        }

        public void Execute (Action<Command> callback = null)
        {
            isExecuted = true;            
            OnExecute();
        }

        public void Undo ()
        {
            OnUndo();
            isExecuted = false;
        }

        public void Redo()
        {
            isExecuted = true;
            OnRedo();
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
