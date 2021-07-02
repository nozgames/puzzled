using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.Universal.PostProcessing;

namespace Puzzled
{
    public class BlendableVolumeComponent : VolumeComponent
    {
        [Tooltip("Controls the blending of the component.")]
        public ClampedFloatParameter blend = new ClampedFloatParameter(0, 0, 1);
    }

    public abstract class BlendablePostProcessRenderer<T> : CustomPostProcessRenderer where T : BlendableVolumeComponent
    {
        protected T volumeComponent { get; private set; }

        public override bool Setup(ref RenderingData renderingData, CustomPostProcessInjectionPoint injectionPoint)
        {
            if(volumeComponent == null)
                volumeComponent = VolumeManager.instance.stack.GetComponent<T>();

            return volumeComponent != null && volumeComponent.blend.value > 0 && !PostProcManager.disableAll;
        }
    }
}
