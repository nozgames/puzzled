using System.Collections.Generic;
using UnityEngine;

namespace Puzzled.UI
{
    public class UIRadioGroup : MonoBehaviour
    {
        private List<UIRadio> _toggles = new List<UIRadio>();

        public void RegisterToggle(UIRadio toggle)
        {
            if (toggle == null || _toggles.Contains(toggle))
                return;

            _toggles.Add(toggle);
        }

        public void UnregisterToggle (UIRadio toggle)
        {
            if (toggle == null)
                return;

            _toggles.Remove(toggle);
        }

        public void NotifyToggleOn (UIRadio toggleOn)
        {
            foreach (var toggle in _toggles)
                if (toggle != toggleOn)
                    toggle.isOn = false;
        }
    }
}
