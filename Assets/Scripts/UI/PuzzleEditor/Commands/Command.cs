using System;
using UnityEngine;

namespace Puzzled.Editor.Commands
{
    public abstract class Command
    {        
        public class EditorState
        {
            public Tile[] selectedTiles;
            public Wire selectedWire;
            public UIPuzzleEditor.Mode mode;
            public (string name, object value)[][] inspectorState;
        };

        public EditorState editorStateUndo;
        public EditorState editorStateRedo;

        public bool isExecuted { get; private set; }

        /// <summary>
        /// Puzzle being edited
        /// </summary>
        public Puzzle puzzle => UIPuzzleEditor.instance.puzzle;

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
