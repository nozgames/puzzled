
using System.Collections.Generic;
using System.Diagnostics;

namespace Puzzled.Editor.Commands
{
    class ReorderWireCommand : ICommand
    {
        private Tile selection;
        private Wire wire;
        private int from;
        private int to;
        private List<Wire> wires;

        public ReorderWireCommand (List<Wire> wires, int from, int to)
        {
            selection = UIPuzzleEditor.selection;
            wire = wires[from];
            this.wires = wires;
            this.from = from;
            this.to = to;
        }

        public void Redo()
        {
            Debug.Assert(wires[from] == wire);
            wires.RemoveAt(from);
            wires.Insert(to, wire);
            UIPuzzleEditor.selection = selection;
            wire.selected = true;
        }

        public void Undo()
        {
            Debug.Assert(wires[to] == wire);
            wires.RemoveAt(to);
            wires.Insert(from, wire);
            UIPuzzleEditor.selection = selection;
            wire.selected = true;
        }

        public void Destroy()
        {
        }
    }
}
