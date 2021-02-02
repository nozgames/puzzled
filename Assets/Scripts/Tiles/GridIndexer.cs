using NoZ;
using UnityEngine;

namespace Puzzled
{
    class GridIndexer : TileComponent
    {
        private int _row = 1;
        private int _column = 1;

        [Editable]
        public int rowCount { get; private set; } = 3;

        [Editable]
        public int columnCount { get; private set; } = 3;

        [Editable]
        public int initialRow { get; private set; } = 1;

        [Editable]
        public int initialColumn { get; private set; } = 1;

        [Editable]
        [Port(PortFlow.Output, PortType.Number, legacy = true)]
        public Port valueOutPort { get; set; }

        [Editable]
        [Port(PortFlow.Input, PortType.Signal, signalEvent = typeof(LeftSignal))]
        public Port leftPort { get; set; }

        [Editable]
        [Port(PortFlow.Input, PortType.Signal, legacy = true, signalEvent = typeof(UpSignal))]
        public Port upPort { get; set; }

        [Editable]
        [Port(PortFlow.Input, PortType.Signal, signalEvent = typeof(RightSignal))]
        public Port rightPort { get; set; }

        [Editable]
        [Port(PortFlow.Input, PortType.Signal, signalEvent = typeof(DownSignal))]
        public Port downPort { get; set; }

        [Editable]
        [Port(PortFlow.Input, PortType.Signal, signalEvent = typeof(ResetSignal))]
        public Port resetPort { get; set; }

        private int GetIndex(int row, int column)
        {
            return row * columnCount + column;
        }

        [ActorEventHandler]
        private void OnReset(ResetSignal evt)
        {
            HandleReset();
        }

        [ActorEventHandler]
        private void OnUp(UpSignal evt)
        {
            --_row;
            if (_row < 0)
                _row = rowCount - 1;

            SendValue();
        }

        [ActorEventHandler]
        private void OnDown(DownSignal evt)
        {
            ++_row;
            if (_row >= (rowCount))
                _row = 0;

            SendValue();
        }

        [ActorEventHandler]
        private void OnLeft(LeftSignal evt)
        {
            --_column;
            if (_column < 0)
                _column = columnCount - 1;

            SendValue();
        }

        [ActorEventHandler]
        private void OnRight(RightSignal evt)
        {
            ++_column;
            if (_column >= columnCount)
                _column = 0;

            SendValue();
        }

        [ActorEventHandler]
        private void OnStart(StartEvent evt)
        {
            HandleReset();
        }

        private void HandleReset()
        {
            _row = Mathf.Clamp(initialRow - 1, 0, rowCount - 1);
            _column = Mathf.Clamp(initialColumn - 1, 0, columnCount - 1);

            SendValue();
        }

        private void SendValue() => valueOutPort.SendValue(GetIndex(_row, _column) + 1, true);
    }
}
