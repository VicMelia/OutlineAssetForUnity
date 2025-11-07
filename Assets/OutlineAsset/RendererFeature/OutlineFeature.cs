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
    public static Material SharedOutlineMaterial { get; set; }
    public static OutlineFeatureSettings SharedSettings { get; private set; }

    /// <inheritdoc/>
    public override void Create()
    {
        m_ScriptablePass = new OutlineFeaturePass(settings);
        m_ScriptablePass.renderPassEvent = injectionPoint;
        SharedSettings = settings;

        if (material != null)
        {
            SharedOutlineMaterial = material;
        }
         
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        SharedOutlineMaterial = null;
        SharedSettings = null;
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
        [Tooltip("Layers excluded from outline effect")]
        public LayerMask excludedLayerMask = 0;
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

            using (var builder = renderGraph.AddRasterRenderPass<DepthData>("Depth Pass", out var passData))
            {
                passData.depth = outlineDepth;
                passData.cullingResults = frameData.Get<UniversalRenderingData>().cullResults;
                passData.cameraData = cameraData;
                passData.renderingData = renderingData;

                var mask = ~settings.excludedLayerMask.value;

                var rendererListDesc = new RendererListDesc(new ShaderTagId("DepthOnly"), passData.cullingResults, passData.cameraData.camera)
                {
                    sortingCriteria = SortingCriteria.CommonOpaque,
                    rendererConfiguration = PerObjectData.None,
                    renderQueueRange = RenderQueueRange.opaque,
                    layerMask = mask
                };

                var rendererList = renderGraph.CreateRendererList(rendererListDesc);
                builder.UseRendererList(rendererList);
                builder.SetRenderAttachmentDepth(passData.depth, AccessFlags.Write);
                builder.SetRenderFunc((DepthData data, RasterGraphContext ctx) =>
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
                passData.depth = outlineDepth;
                passData.normals = normals;
                passData.cullingResults = frameData.Get<UniversalRenderingData>().cullResults;
                passData.cameraData = cameraData;

                var mask = settings.excludedLayerMask.value;

                builder.UseTexture(passData.source, AccessFlags.Read);
                builder.UseTexture(passData.depth, AccessFlags.Read);
                builder.UseTexture(passData.normals, AccessFlags.Read);
                builder.SetRenderAttachment(passData.destination, 0);

                var rendererListDesc = new RendererListDesc(new ShaderTagId("UniversalForward"), passData.cullingResults, passData.cameraData.camera)
                {
                    sortingCriteria = SortingCriteria.CommonOpaque,
                    rendererConfiguration = PerObjectData.None,
                    renderQueueRange = RenderQueueRange.opaque,
                    layerMask = mask
                };

                var rendererList = renderGraph.CreateRendererList(rendererListDesc);
                builder.UseRendererList(rendererList);

                builder.SetRenderFunc((PassData data, RasterGraphContext ctx) =>
                {
                    data.material.SetTexture("_CameraDepthTexture", data.depth);
                    data.material.SetTexture("_CameraNormalsTexture", data.normals);

                    Blitter.BlitTexture(ctx.cmd, data.source, new Vector4(1, 1, 0, 0), data.material, 0);
                    ctx.cmd.DrawRendererList(rendererList);
                });
            }

            resourceData.cameraColor = destination;

            var descFinalDepth = renderGraph.GetTextureDesc(outlineDepth);
            descFinalDepth.name = "DepthFinalOutput";
            TextureHandle outputDepth = renderGraph.CreateTexture(descFinalDepth);

            using (var builder = renderGraph.AddRasterRenderPass<DepthData>("Depth Final Pass", out var passData))
            {
                passData.depth = outputDepth;
                passData.cullingResults = frameData.Get<UniversalRenderingData>().cullResults;
                passData.cameraData = cameraData;
                passData.renderingData = renderingData;

                var mask = ~settings.excludedLayerMask.value;

                var rendererListDesc = new RendererListDesc(new ShaderTagId("DepthOnly"), passData.cullingResults, passData.cameraData.camera)
                {
                    sortingCriteria = SortingCriteria.CommonOpaque,
                    rendererConfiguration = PerObjectData.None,
                    renderQueueRange = RenderQueueRange.opaque,
                    layerMask = mask
                };

                var rendererList = renderGraph.CreateRendererList(rendererListDesc);
                builder.UseRendererList(rendererList);
                builder.SetRenderAttachmentDepth(resourceData.cameraDepthTexture, AccessFlags.Write);
                builder.SetRenderFunc((DepthData data, RasterGraphContext ctx) =>
                {
                    ctx.cmd.DrawRendererList(rendererList);
                });
            }

        }

        class DepthData
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
            public CullingResults cullingResults;
            public UniversalCameraData cameraData;
        }
    }
}
