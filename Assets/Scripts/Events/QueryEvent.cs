using UnityEngine;
using NoZ;

namespace Puzzled
{
    public abstract class QueryEvent : ActorEvent
    {
        public PuzzledActor source { get; private set; }

        public Vector2Int offset { get; private set; }

        public Vector2Int targetCell => source.Cell + offset;

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

        protected QueryEvent(PuzzledActor source, Vector2Int offset)
        {
            this.source = source;
            this.offset = offset;
            _noresult = true;
            _result = false;
        }
    }
}
