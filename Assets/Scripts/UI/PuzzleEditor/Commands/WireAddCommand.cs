using UnityEngine;

namespace Puzzled.Editor.Commands
{
    class WireAddCommand : Command
    {
        private Tile from;
        private Tile to;
        private int fromIndex;
        private int toIndex;
        private Wire wire;
        private Transform parent;

        public WireAddCommand(Tile from, Tile to)
        {
            this.from = from;
            this.to = to;
        }

        protected override void OnUndo()
        {
            wire.from.tile.outputs.RemoveAt(fromIndex);
            wire.to.tile.inputs.RemoveAt(toIndex);
            wire.transform.SetParent(UIPuzzleEditor.deletedObjects);
        }

        protected override void OnExecute()
        {
            Debug.Assert(wire == null);
            wire = GameManager.InstantiateWire(from, to);
            wire.visible = true;
            parent = wire.transform.parent;
            fromIndex = wire.from.tile.GetOutputIndex(wire);
            toIndex = wire.to.tile.GetInputIndex(wire);
            UIPuzzleEditor.selectedWire = wire;
        }

        protected override void OnRedo()
        {
            wire.from.tile.outputs.Insert(fromIndex, wire);
            wire.to.tile.inputs.Insert(toIndex, wire);
            wire.transform.SetParent(parent);
        }

        protected override void OnDestroy()
        {
            // Destroy the wire if the command was undone
            if (!isExecuted)
                UnityEngine.Object.Destroy(wire.gameObject);
        }
    }
}
