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
            fromIndex = wire.from.port.wires.IndexOf(wire);
            toIndex = wire.to.port.wires.IndexOf(wire);
            Debug.Assert(fromIndex >= 0);
            Debug.Assert(toIndex >= 0);
            OnRedo();
        }

        protected override void OnRedo()
        {
            wire.from.port.wires.RemoveAt(fromIndex);
            wire.to.port.wires.RemoveAt(toIndex);
            wire.from.tile.Send(new StartEvent());
            wire.to.tile.Send(new StartEvent());
            UIPuzzleEditor.MoveToTrash(wire.gameObject);
        }

        protected override void OnUndo()
        {
            UIPuzzleEditor.RestoreFromTrash(wire.gameObject);
            wire.from.port.wires.Insert(fromIndex, wire);
            wire.to.port.wires.Insert(toIndex, wire);
            wire.from.tile.Send(new StartEvent());
            wire.to.tile.Send(new StartEvent());
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
