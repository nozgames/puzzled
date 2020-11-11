using UnityEngine;
using NoZ;

namespace Puzzled
{
    class QueryHasItemEvent : ActorEvent
    {
        public ItemType item { get; private set; }

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

        public QueryHasItemEvent(ItemType item)
        {
            result = false;
        }
    }
}
