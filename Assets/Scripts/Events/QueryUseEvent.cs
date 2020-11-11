using UnityEngine;
using NoZ;

namespace Puzzled
{
    class QueryUseEvent : ActorEvent
    {
        public Actor user { get; private set; }

        private bool _result = false;

        public bool result
        {
            get => _result;
            set
            {
                _result = value;
                if (_result)
                    IsHandled = true;
            }
        }

        public QueryUseEvent(Actor user)
        {
            this.user = user;
            result = false;
        }
    }
}
