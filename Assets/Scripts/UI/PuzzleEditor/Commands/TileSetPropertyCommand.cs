namespace Puzzled.Editor.Commands
{
    public class TileSetPropertyCommand : Command
    {
        private Tile tile;
        private string propertyName;
        private object propertyValue;
        private object undoValue;

        public TileSetPropertyCommand (Tile tile, string name, object value)
        {
            this.tile = tile;
            this.propertyName = name;
            this.propertyValue = value;
        }

        protected override void OnExecute()
        {
            undoValue = tile.GetProperty(propertyName).GetValue(tile);
            tile.GetProperty(propertyName).SetValue(tile, propertyValue);
        }

        protected override void OnUndo()
        {
            tile.GetProperty(propertyName).SetValue(tile, undoValue);
        }

        protected override void OnRedo()
        {
            tile.GetProperty(propertyName).SetValue(tile, propertyValue);
        }
    }
}
