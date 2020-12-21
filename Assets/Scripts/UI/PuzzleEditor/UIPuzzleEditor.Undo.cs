﻿using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using Puzzled.Editor.Commands;

namespace Puzzled
{
    public partial class UIPuzzleEditor
    {
        private const int MaxUndo = 256;

        [Header("Undo")]
        [SerializeField] private Transform _deletedObjects = null;
        [SerializeField] private Button _undoButton = null;
        [SerializeField] private Button _redoButton = null;

        public static Transform deletedObjects => instance._deletedObjects;

        private List<Command> _undo = new List<Command>();
        private List<Command> _redo = new List<Command>();
        private Dictionary<int, Transform> _trash = new Dictionary<int, Transform>();

        /// <summary>
        /// Execute a new command
        /// </summary>
        /// <param name="command">Command to execute</param>
        public static void ExecuteCommand (Command command, bool combine=false)
        {
            if (combine && instance._undo.Count > 0)
            {
                // Is there already a group command in the undo queue?
                var group = instance._undo[instance._undo.Count - 1] as GroupCommand;
                if (null == group)
                {
                    group = new GroupCommand();
                    group.Add(instance._undo[instance._undo.Count - 1]);
                    instance._undo[instance._undo.Count - 1] = group;
                }

                group.Add(command);
            } 
            else
            {
                // Clear the redo buffer 
                foreach (var redoCommand in instance._redo)
                    redoCommand.Destroy();

                instance._redo.Clear();

                // Shrink the undo stack to ensure its within maximums
                while (instance._undo.Count >= MaxUndo)
                    instance._undo.RemoveAt(0);

                instance._undo.Add(command);
                instance.UpdateUndoButtons();
            }

            command.Execute();
        }

        /// <summary>
        /// Undo the last command
        /// </summary>
        public void Undo()
        {
            if (_undo.Count == 0)
                return;

            var command = _undo[_undo.Count - 1];
            _undo.RemoveAt(_undo.Count - 1);
            _redo.Add(command);
            command.Undo();
            UpdateUndoButtons();
        }

        /// <summary>
        /// Redo the last command that was undone
        /// </summary>
        public void Redo()
        {
            if (_redo.Count == 0)
                return;

            var command = _redo[_redo.Count - 1];
            _redo.RemoveAt(_redo.Count - 1);
            _undo.Add(command);
            command.Redo();
            UpdateUndoButtons();
        }

        /// <summary>
        /// Update the interatable state of the undo and redo buttons 
        /// </summary>
        private void UpdateUndoButtons()
        {
            _undoButton.interactable = _undo.Count > 0;
            _redoButton.interactable = _redo.Count > 0;
        }

        /// <summary>
        /// Clear the undo buffer and destroy all commands in the buffer
        /// </summary>
        private void ClearUndo()
        {
            // Destroy all undo commands
            foreach (var undoCommand in instance._undo)
                undoCommand.Destroy();

            // Destroy all redo commands
            foreach (var redoCommand in instance._redo)
                redoCommand.Destroy();

            _undo.Clear();
            _redo.Clear();
            UpdateUndoButtons();
        }

        public static void MoveToTrash (GameObject gameObject)
        {
            instance._trash[gameObject.GetInstanceID()] = gameObject.transform.parent;
            gameObject.transform.SetParent(deletedObjects);
        }

        public static void RestoreFromTrash(GameObject gameObject)
        {
            var parent = instance._trash[gameObject.GetInstanceID()];
            if (parent == null)
                return;

            instance._trash.Remove(gameObject.GetInstanceID());
            gameObject.transform.SetParent(parent);
        }        
    }
}
