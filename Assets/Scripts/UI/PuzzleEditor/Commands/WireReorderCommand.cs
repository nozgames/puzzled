using System.Collections.Generic;
using System.Diagnostics;

namespace Puzzled.Editor.Commands
{
    class WireReorderCommand : Command
    {
        private Wire wire;
        private int from;
        private int to;
        private List<Wire> wires;

        public WireReorderCommand (List<Wire> wires, int from, int to)
        {
            wire = wires[from];
            this.wires = wires;
            this.from = from;
            this.to = to;
        }

        protected override void OnExecute()
        {
            Debug.Assert(wires[from] == wire);
            wires.RemoveAt(from);
            wires.Insert(to, wire);
        }

        protected override void OnUndo()
        {
            Debug.Assert(wires[to] == wire);
            wires.RemoveAt(to);
            wires.Insert(from, wire);
        }
    }
}
