using UnityEngine;
using NoZ;

namespace Puzzled
{
    public abstract class QueryEvent : ActorEvent
    {
        public Tile source { get; private set; }

        public Cell offset { get; private set; }

        public Cell targetCell => source.cell + offset;

        public bool hasResult => !_noresult;

        private bool _noresult = true;
        private bool _result = false;

        public bool result {
            get => _result;
            set {
                if (value && !_noresult)
                    return;

                _noresult = false;
                _result = value;
            }
        }

        protected QueryEvent(Tile source, Cell offset)
        {
            this.source = source;
            this.offset = offset;
            _noresult = true;
            _result = false;
        }
    }
}
