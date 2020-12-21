using UnityEngine;

namespace Puzzled.Editor.Commands
{
    class WireDestroyCommand : Command
    {
        private int fromIndex;
        private int toIndex;
        private Wire wire;

        public WireDestroyCommand(Wire wire)
        {
            this.wire = wire;
        }

        protected override void OnExecute()
        {
            fromIndex = wire.from.tile.GetOutputIndex(wire);
            toIndex = wire.to.tile.GetInputIndex(wire);
            OnRedo();
        }

        protected override void OnRedo()
        {
            if (wire.selected)
                UIPuzzleEditor.selectedWire = null;

            wire.from.tile.outputs.RemoveAt(fromIndex);
            wire.to.tile.inputs.RemoveAt(toIndex);
            UIPuzzleEditor.MoveToTrash(wire.gameObject);
        }

        protected override void OnUndo()
        {
            wire.from.tile.outputs.Insert(fromIndex, wire);
            wire.to.tile.inputs.Insert(toIndex, wire);
            UIPuzzleEditor.RestoreFromTrash(wire.gameObject);
        }

        protected override void OnDestroy()
        {
            // If the command was exeucted then the wire still exists in the deleted section, so destroy it now
            if (isExecuted)
            {
                UIPuzzleEditor.RestoreFromTrash(wire.gameObject);
                Object.Destroy(wire.gameObject);
            }
        }
    }
}
