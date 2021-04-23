using UnityEngine;

namespace Puzzled.Editor
{
    public interface IInspectorStateProvider
    {
        /// <summary>
        /// Identifier of the provider
        /// </summary>
        string inspectorStateId { get; }

        /// <summary>
        /// Get/Set the inspector state
        /// </summary>
        object inspectorState { get; set; }
    }
}
