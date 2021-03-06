﻿using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Puzzled.Editor.Commands;

namespace Puzzled.Editor
{
    public partial class UIPuzzleEditor
    {
        private const int MaxUndo = 256;

        private List<Command> _undo = new List<Command>();
        private List<Command> _redo = new List<Command>();
        private Dictionary<int, Transform> _trash = new Dictionary<int, Transform>();

        private Command.EditorState editorState => new Command.EditorState {
            selectedTiles = selectedTiles,
            selectedWire = selectedWire,
            mode = mode,
            inspectorState = _selectedTiles.Select(t => { UpdateInspectorState(t); return t.editor.inspectorState; }).ToArray()
        };

        /// <summary>
        /// Set the editor state 
        /// </summary>
        /// <param name="editorState">New editor state</param>
        private void SetEditorState(Command.EditorState editorState)
        {
            mode = editorState.mode;

            // Clear selection so we can overwrite inspector states
            ClearSelection();

            // If there are selected tiles then first update the inspector state to the saved value
            if(editorState.selectedTiles != null && editorState.selectedTiles.Length > 0)
            {
                for(int i=0; i < editorState.selectedTiles.Length; i++)
                    editorState.selectedTiles[i].editor.inspectorState = editorState.inspectorState[i];

                SelectTiles(editorState.selectedTiles);
                SelectWire(editorState.selectedWire);
            }

            UpdateCursor(true);
        }

        /// <summary>
        /// Execute a new command
        /// </summary>
        /// <param name="command">Command to execute</param>
        public static void ExecuteCommand (Command command, bool combine=false, Action<Command> callback = null)
        {
            if (command == null)
                return;

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

            // Save editor undo state prior to executing the command
            command.editorStateUndo = instance.editorState;

            command.Execute();

            callback?.Invoke(command);

            // Save editor redo state after executing the command
            command.editorStateRedo = instance.editorState;

            // Mark the puzzle as modified
            instance.puzzle.isModified = true;

            instance.RefreshInspectorInternal();
            
            // Add a star to the end of the puzzle name
            if (!instance.puzzleName.text.EndsWith("*"))
                instance.puzzleName.text = instance.puzzleName.text + "*";

            LightmapManager.Render();
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

            SetEditorState(command.editorStateUndo);

            UpdateUndoButtons();

            _puzzle.isModified = true;

            LightmapManager.Render();
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

            SetEditorState(command.editorStateRedo);

            UpdateUndoButtons();


            _puzzle.isModified = true;

            LightmapManager.Render();
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
            if (instance._trash.ContainsKey(gameObject.GetInstanceID()))
            {
                Debug.LogError($"Object '{gameObject.name} is already in the trash");
                return;
            }

            // Automatically de-select any tiles going to the trash
            var tile = gameObject.GetComponent<Tile>();
            if (tile != null && tile.editor.isSelected)
                instance.RemoveSelection(tile);

            instance._trash[gameObject.GetInstanceID()] = gameObject.transform.parent;
            gameObject.transform.SetParent(instance._puzzle.trash);
        }

        public static void RestoreFromTrash(GameObject gameObject)
        {
            if(!instance._trash.TryGetValue(gameObject.GetInstanceID(), out var parent))
            {
                Debug.LogError($"Could not restore object '{gameObject.name} from trash");
                return;
            }

            instance._trash.Remove(gameObject.GetInstanceID());
            gameObject.transform.SetParent(parent);
        }

        public static bool IsInTrash(GameObject gameObject) =>
            gameObject.transform.parent == instance._puzzle.trash;
    }
}
