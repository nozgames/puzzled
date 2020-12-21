
using System.Collections.Generic;

namespace Puzzled.Editor.Commands
{
    public class GroupCommand : Command
    {
        private List<Command> commands = new List<Command>();

        public GroupCommand()
        {
        }

        public void Add(Command command) => commands.Add(command);

        protected override void OnExecute()
        {
            foreach (var command in commands)
                command.Execute();
        }

        protected override void OnRedo()
        {
            foreach (var command in commands)
                command.Redo();
        }

        protected override void OnUndo()
        {
            // Undo in reverse order
            for(int i=commands.Count-1; i>=0; i--)
                commands[i].Undo();
        }
    }
}
