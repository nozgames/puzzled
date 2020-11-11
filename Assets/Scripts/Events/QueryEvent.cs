using UnityEngine;
using NoZ;

namespace Puzzled
{
    public abstract class QueryEvent : ActorEvent
    {
        public Actor source { get; private set; }

        public Vector2Int offset { get; private set; }

        public Vector2Int targetCell { get; private set; }

        private bool _result = false;

        public bool result {
            get => _result;
            set {
                _result = value;
                if (_result)
                    IsHandled = true;
            }
        }

        protected QueryEvent(Actor source, Vector2Int offset)
        {
            this.source = source;
            this.offset = offset;
            _result = false;
        }
    }
}
