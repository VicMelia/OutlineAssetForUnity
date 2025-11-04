using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;
using UnityEngine.Rendering.RendererUtils;

public class OutlineFeature : ScriptableRendererFeature
{
    [SerializeField] OutlineFeatureSettings settings;
    OutlineFeaturePass m_ScriptablePass;

    public RenderPassEvent injectionPoint = RenderPassEvent.BeforeRenderingPostProcessing;
    public Material material;
    public static Material SharedOutlineMaterial { get; private set; }

    /// <inheritdoc/>
    public override void Create()
    {
        m_ScriptablePass = new OutlineFeaturePass(settings);

        // Configures where the render pass should be injected.
        m_ScriptablePass.renderPassEvent = injectionPoint;

        if (material != null)
            SharedOutlineMaterial = material;

        // You can request URP color texture and depth buffer as inputs by uncommenting the line below,
        // URP will ensure copies of these resources are available for sampling before executing the render pass.
        // Only uncomment it if necessary, it will have a performance impact, especially on mobiles and other TBDR GPUs where it will break render passes.
        //m_ScriptablePass.ConfigureInput(ScriptableRenderPassInput.Color | ScriptableRenderPassInput.Depth);

        // You can request URP to render to an intermediate texture by uncommenting the line below.
        // Use this option for passes that do not support rendering directly to the backbuffer.
        // Only uncomment it if necessary, it will have a performance impact, especially on mobiles and other TBDR GPUs where it will break render passes.
        //m_ScriptablePass.requiresIntermediateTexture = true;
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        SharedOutlineMaterial = null;
    }

    // Here you can inject one or multiple render passes in the renderer.
    // This method is called when setting up the renderer once per-camera.
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (material == null) return;
        m_ScriptablePass.Setup(material);
        renderer.EnqueuePass(m_ScriptablePass);
    }

    // Use this class to pass around settings from the feature to the pass
    [Serializable]
    public class OutlineFeatureSettings
    {

    }

    class OutlineFeaturePass : ScriptableRenderPass
    {
        private readonly OutlineFeatureSettings settings;
        private Material m_Material;

        public OutlineFeaturePass(OutlineFeatureSettings settings)
        {
            this.settings = settings;
        }

        public void Setup(Material material)
        {
            m_Material = material;
            requiresIntermediateTexture = true;
        }

        //Mtodo oficial para Unity 6 con Render Graph
        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            var resourceData = frameData.Get<UniversalResourceData>();
            var colorSource = resourceData.activeColorTexture;
            var normals = resourceData.cameraNormalsTexture;

            var cameraData = frameData.Get<UniversalCameraData>();
            var renderingData = frameData.Get<UniversalRenderingData>();
            //if (!cameraData.requiresDepthTexture) return;
            //if (cameraData.renderType != CameraRenderType.Base) return;
            var depth = resourceData.cameraDepthTexture;


            var descDepth = renderGraph.GetTextureDesc(depth);
            descDepth.name = "DepthOutput";
            TextureHandle outlineDepth = renderGraph.CreateTexture(descDepth);

            using (var builder = renderGraph.AddRasterRenderPass<DepthPrepassData>("Depth Pass", out var passData))
            {
                passData.depth = outlineDepth;
                passData.cullingResults = frameData.Get<UniversalRenderingData>().cullResults;
                passData.cameraData = cameraData;
                passData.renderingData = renderingData;

                var blockLayer = LayerMask.NameToLayer("Block");
                var mask = ~(1 << blockLayer);

                var rendererListDesc = new RendererListDesc(new ShaderTagId("DepthOnly"), passData.cullingResults, passData.cameraData.camera)
                {
                    sortingCriteria = SortingCriteria.CommonOpaque,
                    rendererConfiguration = PerObjectData.None,
                    renderQueueRange = RenderQueueRange.opaque,
                    layerMask = mask
                };

                var rendererList = renderGraph.CreateRendererList(rendererListDesc);
                builder.UseRendererList(rendererList);

                //builder.UseTexture(passData.depth, AccessFlags.Write);
                builder.SetRenderAttachmentDepth(passData.depth, AccessFlags.Write);
                builder.SetRenderFunc((DepthPrepassData data, RasterGraphContext ctx) =>
                {

                    ctx.cmd.DrawRendererList(rendererList);


                });
            }

            var desc = renderGraph.GetTextureDesc(colorSource);
            desc.name = "OutlineOutput";
            desc.clearBuffer = false;

            TextureHandle destination = renderGraph.CreateTexture(desc);

            using (var builder = renderGraph.AddRasterRenderPass<PassData>("Outline Pass", out var passData))
            {
                passData.source = colorSource;
                passData.destination = destination;
                passData.material = m_Material;
                passData.depth = depth;
                passData.normals = normals;

                builder.UseTexture(passData.source, AccessFlags.Read);
                builder.UseTexture(passData.depth, AccessFlags.Read);
                
                builder.UseTexture(passData.normals, AccessFlags.Read);
                builder.SetRenderAttachment(passData.destination, 0);

                builder.SetRenderFunc((PassData data, RasterGraphContext ctx) =>
                {
                    data.material.SetTexture("_CameraDepthTexture", data.depth);
                    data.material.SetTexture("_CameraNormalsTexture", data.normals);

                    Blitter.BlitTexture(ctx.cmd, data.source, new Vector4(1, 1, 0, 0), data.material, 0);
                });
            }

            resourceData.cameraColor = destination;
        }

        class DepthPrepassData
        {
            public TextureHandle depth;
            public CullingResults cullingResults;
            public UniversalCameraData cameraData;
            public UniversalRenderingData renderingData;
        }

        class PassData
        {
            public TextureHandle source;
            public TextureHandle destination;
            public TextureHandle depth;
            public TextureHandle normals;
            public Material material;
        }
    }
}
