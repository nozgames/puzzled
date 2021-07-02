using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.Universal.PostProcessing;

// Define the Volume Component for the custom post processing effect 
[System.Serializable, VolumeComponentMenu("Puzzled/Sepia")]
public class SepiaEffect : BlendableVolumeComponent
{
}

// Define the renderer for the custom post processing effect
[CustomPostProcess("Sepia", CustomPostProcessInjectionPoint.AfterPostProcess)]
public class SepiaEffectRenderer : CustomPostProcessRenderer
{
    // A variable to hold a reference to the corresponding volume component
    private SepiaEffect m_VolumeComponent;

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
        m_Material = CoreUtils.CreateEngineMaterial("Shader Graphs/Sepia");
    }

    // Called for each camera/injection point pair on each frame. Return true if the effect should be rendered for this camera.
    public override bool Setup(ref RenderingData renderingData, CustomPostProcessInjectionPoint injectionPoint)
    {
        // Get the current volume stack
        var stack = VolumeManager.instance.stack;
        // Get the corresponding volume component
        m_VolumeComponent = stack.GetComponent<SepiaEffect>();
        // if blend value > 0, then we need to render this effect. 
        return m_VolumeComponent.blend.value > 0;
    }

    // The actual rendering execution is done here
    public override void Render(CommandBuffer cmd, RenderTargetIdentifier source, RenderTargetIdentifier destination, ref RenderingData renderingData, CustomPostProcessInjectionPoint injectionPoint)
    {
        // set material properties
        if (m_Material != null)
        {
            m_Material.SetFloat(ShaderIDs.Blend, m_VolumeComponent.blend.value);
        }

        // Since we are using a shader graph, we cann't use CoreUtils.DrawFullScreen without modifying the vertex shader.
        // So we go with the easy route and use CommandBuffer.Blit instead. The same goes if you want to use legacy image effect shaders.
        // Note: don't forget to set pass to 0 (last argument in Blit) to make sure that extra passes are not drawn.
        cmd.Blit(source, destination, m_Material, 0);
        // 
        //       // set source texture
        //       cmd.SetGlobalTexture(ShaderIDs.Input, source);
        //       // draw a fullscreen triangle to the destination
        //       CoreUtils.DrawFullScreen(cmd, m_Material, destination);
    }
}
