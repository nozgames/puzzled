using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.Universal.PostProcessing;
using Puzzled;

// Define the Volume Component for the custom post processing effect 
[System.Serializable, VolumeComponentMenu("Puzzled/Outline")]
public class OutlineEffect : BlendableVolumeComponent
{
}

// Define the renderer for the custom post processing effect
[CustomPostProcess("Outline", CustomPostProcessInjectionPoint.AfterPostProcess)]
public class OutlineEffectRenderer : BlendablePostProcessRenderer<OutlineEffect>
{
   // The postprocessing material
   private Material m_Material;

   // The ids of the shader variables
   static class ShaderIDs
   {
      internal readonly static int Input = Shader.PropertyToID("_MainTex");
      internal readonly static int Blend = Shader.PropertyToID("_Blend");
   }

   // By default, the effect is visible in the scene view, but we can change that here.
   public override bool visibleInSceneView => true;

   /// Specifies the input needed by this custom post process. Default is Color only.
   public override ScriptableRenderPassInput input => ScriptableRenderPassInput.Color;

   // Initialized is called only once before the first render call
   // so we use it to create our material
   public override void Initialize()
   {
      base.Initialize();
      m_Material = CoreUtils.CreateEngineMaterial("Shader Graphs/SelectedOutline");
   }

    public override bool Setup(ref RenderingData renderingData, CustomPostProcessInjectionPoint injectionPoint)
    {
        if (!CameraManager.showSelection)
            return false;

        return base.Setup(ref renderingData, injectionPoint);
    }

    public override void Render(CommandBuffer cmd, RenderTargetIdentifier source, RenderTargetIdentifier destination, ref RenderingData renderingData, CustomPostProcessInjectionPoint injectionPoint)
   {
      // set material properties
      if (m_Material != null)
      {
         m_Material.SetFloat(ShaderIDs.Blend, volumeComponent.blend.value);
      }

      cmd.Blit(source, destination, m_Material, 0);
   }
}
