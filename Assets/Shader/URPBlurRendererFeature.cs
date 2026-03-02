using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class URPBlurRendererFeature : ScriptableRendererFeature
{
    [System.Serializable]
    public class BlurSettings
    {
        public RenderPassEvent passEvent = RenderPassEvent.BeforeRenderingTransparents;
        [Range(1, 4)] public int downsample = 2;
        [Range(1, 6)] public int blurIterations = 2;
        [Range(0.2f, 3f)] public float blurSpread = 1.0f;
        public Material blurMaterial; // Uses Hidden/URP/KawaseBlur
        public string globalTextureName = "_UIBlurTexture";
    }

    class BlurPass : ScriptableRenderPass
    {
        private BlurSettings settings;
        private RTHandle source;
        private RTHandle rt1;
        private RTHandle rt2;

        private static readonly int BlurOffset = Shader.PropertyToID("_BlurOffset");
        private int globalTexId;

        public BlurPass(BlurSettings s)
        {
            settings = s;
            profilingSampler = new ProfilingSampler("URP UI Blur Pass");
            globalTexId = Shader.PropertyToID(settings.globalTextureName);
        }

        public void Setup(RTHandle cameraColorTargetHandle)
        {
            source = cameraColorTargetHandle;
        }

        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
            var desc = cameraTextureDescriptor;
            desc.depthBufferBits = 0;
            desc.msaaSamples = 1;
            desc.width /= settings.downsample;
            desc.height /= settings.downsample;

            RenderingUtils.ReAllocateIfNeeded(ref rt1, desc, FilterMode.Bilinear, TextureWrapMode.Clamp, name: "_UIBlurRT1");
            RenderingUtils.ReAllocateIfNeeded(ref rt2, desc, FilterMode.Bilinear, TextureWrapMode.Clamp, name: "_UIBlurRT2");
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (settings.blurMaterial == null) return;

            var cmd = CommandBufferPool.Get("URP UI Blur");

            // Copy camera color -> rt1 (downsampled copy)
            Blitter.BlitCameraTexture(cmd, source, rt1);

            // Kawase blur ping-pong
            for (int i = 0; i < settings.blurIterations; i++)
            {
                float offset = 1f + i * settings.blurSpread;
                settings.blurMaterial.SetFloat(BlurOffset, offset);

                Blitter.BlitCameraTexture(cmd, rt1, rt2, settings.blurMaterial, 0);
                Blitter.BlitCameraTexture(cmd, rt2, rt1, settings.blurMaterial, 0);
            }

            // Expose blurred texture globally for UI shader
            cmd.SetGlobalTexture(globalTexId, rt1);

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        public override void OnCameraCleanup(CommandBuffer cmd) { }
    }

    public BlurSettings settings = new BlurSettings();
    BlurPass pass;

    public override void Create()
    {
        pass = new BlurPass(settings)
        {
            renderPassEvent = settings.passEvent
        };
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (settings.blurMaterial == null) return;
        pass.Setup(renderer.cameraColorTargetHandle);
        renderer.EnqueuePass(pass);
    }
}