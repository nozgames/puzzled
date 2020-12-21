using UnityEditor;

namespace Puzzled.Editor.Commands
{
    class AddSequenceStepCommand : Command
    {
        private string stepName;
        private Tile tile;

        public AddSequenceStepCommand(Tile tile, string name)
        {
            this.tile = tile;
            this.stepName = name;
        }

        protected override void OnExecute()
        {
            //_steps.Add("New Step");
            //_tile.SetProperty("steps", _steps.ToArray());
        }

        protected override void OnUndo()
        {            
        }
    }
}
