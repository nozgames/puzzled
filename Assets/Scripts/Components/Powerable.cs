using UnityEngine;

namespace Puzzled
{
    public class Powerable : TileComponent
    {
        [Editable]
        [Port(PortFlow.Input, PortType.Power, legacy = true)]
        public Port powerInPort { get; set; }

        public bool hasPower => powerInPort.wireCount == 0 || powerInPort.hasPower;
    }
}
