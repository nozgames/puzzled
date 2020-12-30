namespace Puzzled.Editor.Commands
{
    public class WireSetOptionCommand : Command
    {
        private int option;
        private int redoValue;
        private int undoValue;
        private bool input;
        private Wire wire;

        public WireSetOptionCommand(Wire wire, bool input, int option, int value)
        {
            this.option = option;
            this.input = input;
            this.wire = wire;
            redoValue = value;
            undoValue = input ? wire.to.GetOption(option) : wire.from.GetOption(option);
        }

        protected override void OnExecute()
        {
            if (input)
                wire.to.SetOption(option, redoValue);
            else
                wire.from.SetOption(option, redoValue);
        }

        protected override void OnUndo()
        {
            if (input)
                wire.to.SetOption(option, undoValue);
            else
                wire.from.SetOption(option, undoValue);
        }
    }
}
