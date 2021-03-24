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
            if (UIPuzzleEditor.IsInTrash(wire.gameObject))
                return;

            fromIndex = wire.from.port.wires.IndexOf(wire);
            toIndex = wire.to.port.wires.IndexOf(wire);
            Debug.Assert(fromIndex >= 0);
            Debug.Assert(toIndex >= 0);
            OnRedo();
            wire.to.tile.Send(new StartEvent());
        }

        protected override void OnRedo()
        {
            if (UIPuzzleEditor.IsInTrash(wire.gameObject))
                return;

            wire.from.port.wires.RemoveAt(fromIndex);
            wire.to.port.wires.RemoveAt(toIndex);
            wire.from.tile.Send(new StartEvent());
            wire.to.tile.Send(new StartEvent());
            UIPuzzleEditor.MoveToTrash(wire.gameObject);
            wire.to.tile.Send(new StartEvent());
        }

        protected override void OnUndo()
        {
            if (!UIPuzzleEditor.IsInTrash(wire.gameObject))
                return;

            UIPuzzleEditor.RestoreFromTrash(wire.gameObject);
            wire.from.port.wires.Insert(fromIndex, wire);
            wire.to.port.wires.Insert(toIndex, wire);
            wire.from.tile.Send(new StartEvent());
            wire.to.tile.Send(new StartEvent());
        }

        protected override void OnDestroy()
        {
            // If the command was exeucted then the wire still exists in the deleted section, so destroy it now
            if (isExecuted && UIPuzzleEditor.IsInTrash(wire.gameObject))
            {
                UIPuzzleEditor.RestoreFromTrash(wire.gameObject);
                Object.Destroy(wire.gameObject);
            }
        }
    }
}
