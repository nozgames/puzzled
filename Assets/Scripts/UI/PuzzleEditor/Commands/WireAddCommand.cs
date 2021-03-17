using UnityEngine;

namespace Puzzled.Editor.Commands
{
    class WireAddCommand : Command
    {
        private Port from;
        private Port to;
        private int fromIndex;
        private int toIndex;
        private Wire wire;

        public Wire addedWire => wire;

        public WireAddCommand(Port from, Port to)
        {
            this.from = from;
            this.to = to;
        }

        public WireAddCommand(Wire wire)
        {
            this.wire = wire;
            this.from = wire.from.port;
            this.to = wire.to.port;
        }

        protected override void OnUndo()
        {
            from.wires.RemoveAt(fromIndex);
            to.wires.RemoveAt(toIndex);
            from.tile.Send(new StartEvent());
            to.tile.Send(new StartEvent());
            UIPuzzleEditor.MoveToTrash(wire.gameObject);
        }

        protected override void OnExecute()
        {
            if(wire == null)
                wire = puzzle.InstantiateWire(from, to);
            wire.visible = true;
            fromIndex = from.wires.IndexOf(wire);
            toIndex = to.wires.IndexOf(wire);
//            UIPuzzleEditor.selectedWire = wire;

            from.tile.Send(new StartEvent());
            to.tile.Send(new StartEvent());
        }

        protected override void OnRedo()
        {
            from.wires.Insert(fromIndex, wire);
            to.wires.Insert(toIndex, wire);
            UIPuzzleEditor.RestoreFromTrash(wire.gameObject);

            from.tile.Send(new StartEvent());
            to.tile.Send(new StartEvent());
        }

        protected override void OnDestroy()
        {
            // Destroy the wire if the command was undone
            if (!isExecuted)
                UnityEngine.Object.Destroy(wire.gameObject);
        }
    }
}
