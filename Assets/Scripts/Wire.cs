using UnityEngine;

namespace Puzzled
{
    public class Wire : MonoBehaviour
    {
        [SerializeField] private LineRenderer line = null;

        public Tile input { get; set; }
        public Tile output { get; set; }
        
        public bool visible {
            get => line.enabled;
            set {
                line.enabled = value;
                if (line.enabled)
                    UpdateLine();
            }
        }

        private void OnEnable()
        {
            output.Send(new ActivateWireEvent(this));

            if(visible)
                UpdateLine();
        }

        private void OnDisable()
        {
            output.Send(new DeactivateWireEvent(this));
        }

        private void OnDestroy()
        {
            if(input != null)
                input.outputs.Remove(this);

            if(output != null)
                output.inputs.Remove(this);
        }

        public void UpdateLine()
        {
            transform.position = input.transform.position;

            line.positionCount = 2;
            line.SetPosition(0, Vector3.zero);
            line.SetPosition(1, output.transform.position - input.transform.position);
        }
    }
}
