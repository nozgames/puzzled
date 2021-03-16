using NoZ;

namespace Puzzled
{
    class LogicSpinner : Spinner
    {
        [Editable]
        public int valueCount { get; private set; }

        override protected int maxValues => valueCount;
    }
}
