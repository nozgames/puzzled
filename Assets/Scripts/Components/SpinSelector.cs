using NoZ;
using UnityEngine;

namespace Puzzled
{
    class SpinSelector : PuzzledActorComponent
    {
        public int value { get; private set; }

        [Header("Visuals")]
        [SerializeField] private GameObject[] visualValues;

        [Header("Config")]
        [SerializeField] private uint target = 0;

        [ActorEventHandler]
        private void OnQueryUse(QueryUseEvent evt)
        {
            evt.result = true;
        }

        [ActorEventHandler]
        private void OnUse(UseEvent evt)
        {
            int numValues = visualValues.Length;

            int newValue = (value + 1) % numValues;
            SetValue(value);
        }

        private void SetValue(int newValue)
        {
            Debug.Assert(newValue < visualValues.Length);

            value = newValue;

            if (value == target)
                actor.ActivateWire();
            else
                actor.DeactivateWire();

            UpdateVisuals();
        }

        private void UpdateVisuals()
        {
            for (int i = 0; i < visualValues.Length; ++i)
                visualValues[value].SetActive(value == i);
        }
    }
}
