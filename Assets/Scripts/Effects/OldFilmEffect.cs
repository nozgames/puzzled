using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.Universal.PostProcessing;
using Puzzled;

// Define the Volume Component for the custom post processing effect 
[System.Serializable, VolumeComponentMenu("Puzzled/OldFilm")]
public class OldFilmEffect : BlendableVolumeComponent
{
}

// Define the renderer for the custom post processing effect
[CustomPostProcess("OldFilm", CustomPostProcessInjectionPoint.AfterPostProcess)]
public class OldFilmEffectRenderer : BlendablePostProcessRenderer<OldFilmEffect>
{
    // The postprocessing material (you can define as many as you like)
    private Material m_Material;

    // The ids of the shader variables
    static class ShaderIDs
    {
        internal readonly static int Input = Shader.PropertyToID("_MainTex");
        internal readonly static int Blend = Shader.PropertyToID("_Blend");
    }

    // By default, the effect is visible in the scene view, but we can change that here.
    public override bool visibleInSceneView => true;

    // Initialized is called only once before the first render call
    // so we use it to create our material
    public override void Initialize()
    {
        base.Initialize();
        m_Material = CoreUtils.CreateEngineMaterial("Shaders/OldFilm");
    }

    public override void Render(CommandBuffer cmd, RenderTargetIdentifier source, RenderTargetIdentifier destination, ref RenderingData renderingData, CustomPostProcessInjectionPoint injectionPoint)
    {
        // set material properties
        if (m_Material != null)
        {
            m_Material.SetFloat(ShaderIDs.Blend, volumeComponent.blend.value);
        }
        // set source texture
        cmd.SetGlobalTexture(ShaderIDs.Input, source);
        // draw a fullscreen triangle to the destination
        CoreUtils.DrawFullScreen(cmd, m_Material, destination);
    }
}
