namespace Puzzled
{
    public class Sign : TileComponent
    {
        private string _text = "";

        [Editable]
        public string text {
            get => _text;
            set {
                _text = value;
                UpdateVisuals();
            }
        }

        private void UpdateVisuals()
        {

        }
    }
}
