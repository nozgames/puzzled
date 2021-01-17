using System;
using UnityEngine;
using UnityEngine.UI;

namespace Puzzled.Editor
{
    public class UIPortSelector : MonoBehaviour
    {
        [SerializeField] private RawImage _tilePreview = null; 
        [SerializeField] private UIPortSelectorPort[] _ports = null;

        private Action<Port,Port> _callback;

        private void Awake()
        {
            foreach (var port in _ports)
                port.button.onClick.AddListener(() => {
                    OnClickPort(port);
                });
        }

        private void OnClickPort(UIPortSelectorPort port)
        {
            _callback?.Invoke(port.from, port.to);
        }

        public void Open(Tile tileFrom, Tile tileTo, Action<Port, Port> callback)
        {
            _callback = callback;

            _tilePreview.texture = TileDatabase.GetPreview(tileTo);

            foreach (var port in _ports)
                port.gameObject.SetActive(false);

            var powerOut = (Port)null;
            var signalOut = (Port)null;
            var numberOut = (Port)null;
            foreach (var property in tileFrom.properties)
                if (property.type == TilePropertyType.Port && property.port.flow == PortFlow.Output)
                {
                    if (property.port.type == PortType.Number)
                        numberOut = property.GetValue<Port>(tileFrom);
                    else if (property.port.type == PortType.Signal)
                        signalOut = property.GetValue<Port>(tileFrom);
                    else if (property.port.type == PortType.Power)
                        powerOut = property.GetValue<Port>(tileFrom);
                }

            var portCount = 0;
            foreach (var property in tileTo.properties)
                if (property.type == TilePropertyType.Port && property.port.flow == PortFlow.Input)
                {
                    Port portOut = null;

                    switch (property.port.type)
                    {
                        case PortType.Number:
                            portOut = numberOut;
                            break;

                        case PortType.Power:
                            portOut = powerOut;
                            break;

                        case PortType.Signal:
                            portOut = signalOut ?? powerOut;
                            break;

                        default:
                            throw new NotImplementedException();
                    }

                    if(null == portOut)
                        continue;

                    _ports[portCount].from = portOut;
                    _ports[portCount].to = property.GetValue<Port>(tileTo);
                    _ports[portCount].gameObject.SetActive(true);
                    portCount++;
                }

            if(portCount == 0)
            {
                // TODO: handle error
                return;
            }

            if(portCount == 1)
            {
                _callback?.Invoke(_ports[0].from, _ports[0].to);
                return;
            }
        }
    }
}
