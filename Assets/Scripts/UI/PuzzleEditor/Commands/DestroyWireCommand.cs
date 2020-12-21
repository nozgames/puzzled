using UnityEngine;

namespace Puzzled.Editor.Commands
{
    class DestroyWireCommand : ICommand
    {
        private int fromIndex;
        private int toIndex;
        private Wire wire;
        private Transform parent;
        private bool selected;
        private Tile selection;

        public DestroyWireCommand(Wire wire)
        {
            selected = wire.selected;
            parent = wire.transform.parent;
            fromIndex = wire.from.tile.GetOutputIndex(wire);
            toIndex = wire.to.tile.GetInputIndex(wire);
            selection = UIPuzzleEditor.selection;
            this.wire = wire;
        }

        public void Undo()
        {
            wire.from.tile.outputs.Insert(fromIndex, wire);
            wire.to.tile.inputs.Insert(toIndex, wire);
            wire.transform.SetParent(parent);
            UIPuzzleEditor.RefreshInspector();
            UIPuzzleEditor.selection = selection;
            wire.selected = selected;
        }

        public void Redo()
        {
            wire.from.tile.outputs.RemoveAt(fromIndex);
            wire.to.tile.inputs.RemoveAt(toIndex);
            wire.transform.SetParent(UIPuzzleEditor.deletedObjects);
            UIPuzzleEditor.RefreshInspector();
            UIPuzzleEditor.selection = selection;
            wire.selected = false;
        }

        public void Destroy()
        {
            UnityEngine.Object.Destroy(wire.gameObject);
        }
    }
}
