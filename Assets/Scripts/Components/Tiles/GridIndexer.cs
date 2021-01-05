using NoZ;

namespace Puzzled
{
    class GridIndexer : TileComponent
    {
        private int _index = 0;

        [Editable]
        public int rowCount { get; private set; } = 3;

        [Editable]
        public int columnCount { get; private set; } = 3;

        [Editable]
        [Port(PortFlow.Output, PortType.Number, legacy = true)]
        public Port valueOutPort { get; set; }

        [Editable]
        [Port(PortFlow.Input, PortType.Signal, legacy = true, signalEvent = typeof(UpSignal))]
        public Port upPort { get; set; }

        [Editable]
        [Port(PortFlow.Input, PortType.Signal, signalEvent = typeof(DownSignal))]
        public Port downPort { get; set; }

        [Editable]
        [Port(PortFlow.Input, PortType.Signal, signalEvent = typeof(LeftSignal))]
        public Port leftPort { get; set; }

        [Editable]
        [Port(PortFlow.Input, PortType.Signal, signalEvent = typeof(RightSignal))]
        public Port rightPort { get; set; }

        private int GetRow()
        {
            return (_index / columnCount);
        }

        private int GetColumn()
        {
            return (_index % columnCount);
        }

        private void SetRowAndColumn(int row, int column)
        {
            _index = row * columnCount + column;
        }

        [ActorEventHandler]
        private void OnUp(UpSignal evt)
        {
            int row = GetRow();
            int column = GetColumn();

            --row;
            if (row < 0)
                row = rowCount - 1;

            SetRowAndColumn(row, column);

            SendValue();
        }

        [ActorEventHandler]
        private void OnDown(DownSignal evt)
        {
            int row = GetRow();
            int column = GetColumn();
            
            ++row;
            if (row >= (rowCount))
                row = 0;

            SetRowAndColumn(row, column);

            SendValue();
        }

        [ActorEventHandler]
        private void OnLeft(LeftSignal evt)
        {
            int row = GetRow();
            int column = GetColumn();
            
            --column;
            if (column < 0)
                column = columnCount - 1;

            SetRowAndColumn(row, column);

            SendValue();
        }

        [ActorEventHandler]
        private void OnRight(RightSignal evt)
        {
            int row = GetRow();
            int column = GetColumn();

            ++column;
            if (column >= columnCount)
                column = 0;

            SetRowAndColumn(row, column);

            SendValue();
        }

        [ActorEventHandler]
        private void OnStart(StartEvent evt) => SendValue();

        private void SendValue() => valueOutPort.SendValue(_index + 1);
    }
}
