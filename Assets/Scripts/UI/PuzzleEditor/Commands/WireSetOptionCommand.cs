namespace Puzzled.Editor.Commands
{
    public class WireSetOptionCommand : Command
    {
        private Wire.Connection connection;
        private int option;
        private int redoValue;
        private int undoValue;
        private bool input;

        public WireSetOptionCommand(Wire.Connection connection, int option, int value)
        {
            this.connection = connection;
            this.option = option;
            redoValue = value;
            undoValue = connection.GetOption(option);
        }

        protected override void OnExecute()
        {
            connection.SetOption(option, redoValue);
        }

        protected override void OnUndo()
        {
            connection.SetOption(option, undoValue);
        }
    }
}
