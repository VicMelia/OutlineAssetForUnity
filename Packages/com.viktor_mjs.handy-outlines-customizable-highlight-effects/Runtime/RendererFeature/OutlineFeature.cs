using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;
using UnityEngine.Rendering.RendererUtils;

namespace HandyOutlines
{
    public class OutlineFeature : ScriptableRendererFeature
    {
        #region Feature variables
        private OutlineFeaturePass m_ScriptablePass;
        [SerializeField] OutlineFeatureSettings settings;
        public static Material SharedOutlineMaterial { get; set; }
        public static OutlineFeatureSettings SharedSettings { get; private set; }
        public RenderPassEvent injectionPoint = RenderPassEvent.BeforeRenderingPostProcessing;
        public Material material;
        #endregion

        #region Feature Setup
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

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (material == null) return;
            m_ScriptablePass.Setup(material);
            renderer.EnqueuePass(m_ScriptablePass);
        }
        #endregion

        #region Feature Settings
        [Serializable]
        public class OutlineFeatureSettings
        {
            public LayerMask excludedLayerMask = 0;
        }
        #endregion

        private class OutlineFeaturePass : ScriptableRenderPass
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

            public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
            {
                var resourceData = frameData.Get<UniversalResourceData>();
                var colorSource = resourceData.activeColorTexture;

                var cameraData = frameData.Get<UniversalCameraData>();
                var renderingData = frameData.Get<UniversalRenderingData>();

                #region First Pass (Depth Prepass)
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
                #endregion

                #region Second Pass (Normal Prepass)
                var normals = resourceData.cameraNormalsTexture;
                var descNormals = renderGraph.GetTextureDesc(normals);
                descDepth.name = "DepthOutput";
                TextureHandle outlineNormal = renderGraph.CreateTexture(descNormals);

                using (var builder = renderGraph.AddRasterRenderPass<NormalsData>("Normal Pass", out var passData))
                {
                    passData.normals = outlineNormal;
                    passData.cullingResults = frameData.Get<UniversalRenderingData>().cullResults;
                    passData.cameraData = cameraData;
                    passData.renderingData = renderingData;

                    var mask = ~settings.excludedLayerMask.value;

                    var rendererListDesc = new RendererListDesc(new ShaderTagId("DepthNormals"), passData.cullingResults, passData.cameraData.camera)
                    {
                        sortingCriteria = SortingCriteria.CommonOpaque,
                        rendererConfiguration = PerObjectData.None,
                        renderQueueRange = RenderQueueRange.opaque,
                        layerMask = mask
                    };

                    var rendererList = renderGraph.CreateRendererList(rendererListDesc);
                    builder.UseRendererList(rendererList);
                    builder.SetRenderAttachment(passData.normals, 0);
                    builder.SetRenderFunc((NormalsData data, RasterGraphContext ctx) =>
                    {
                        ctx.cmd.DrawRendererList(rendererList);
                    });
                }
                #endregion

                #region Third Pass (Outline Pass)
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
                    passData.normals = outlineNormal;
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
                #endregion 

                #region Fourth Pass (Depth rewritten)
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
                #endregion

            }

            private class DepthData
            {
                public TextureHandle depth;
                public CullingResults cullingResults;
                public UniversalCameraData cameraData;
                public UniversalRenderingData renderingData;
            }

            private class NormalsData
            {
                public TextureHandle normals;
                public CullingResults cullingResults;
                public UniversalCameraData cameraData;
                public UniversalRenderingData renderingData;
            }

            private class PassData
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
}