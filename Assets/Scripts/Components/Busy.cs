namespace Puzzled
{
    class Busy
    {
        private bool _enabled = false;

        public bool enabled {
            get => _enabled;
            set {
                if (_enabled == value)
                    return;

                if (value)
                    GameManager.busy++;
                else
                    GameManager.busy--;

                _enabled = value;
            }
        }
    }
}
