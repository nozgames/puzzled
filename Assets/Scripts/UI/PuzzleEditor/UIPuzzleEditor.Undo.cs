using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Puzzled
{
    public partial class UIPuzzleEditor
    {
        [Header("Undo")]
        [SerializeField] private Transform _deletedObjects = null;
        [SerializeField] private Button _undoButton = null;
        [SerializeField] private Button _redoButton = null;

        public static Transform deletedObjects => instance._deletedObjects;

        private List<ICommand> undo = new List<ICommand>();
        private List<ICommand> redo = new List<ICommand>();

        public static void ExecuteCommand (ICommand command)
        {
            foreach (var redoCommand in instance.redo)
                redoCommand.Destroy();

            instance.redo.Clear();
            instance.undo.Add(command);
            command.Redo();
            instance.UpdateUndoButtons();
        }

        public void Undo()
        {
            if (undo.Count == 0)
                return;

            var command = undo[undo.Count - 1];
            undo.RemoveAt(undo.Count - 1);
            redo.Add(command);
            command.Undo();
            UpdateUndoButtons();
        }

        public void Redo()
        {
            if (redo.Count == 0)
                return;

            var command = redo[redo.Count - 1];
            redo.RemoveAt(redo.Count - 1);
            undo.Add(command);
            command.Redo();
            UpdateUndoButtons();
        }

        private void UpdateUndoButtons()
        {
            _undoButton.interactable = undo.Count > 0;
            _redoButton.interactable = redo.Count > 0;
        }

        private void ClearUndo()
        {
            undo.Clear();
            redo.Clear();
            UpdateUndoButtons();
        }
    }
}
