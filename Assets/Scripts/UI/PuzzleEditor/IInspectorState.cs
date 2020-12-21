using UnityEngine;

namespace Puzzled.Editor
{
    public interface IInspectorState
    {
        void Apply(Transform inspector);
    }

    public interface IInspectorStateProvider
    {
        IInspectorState GetState();
    }
}
