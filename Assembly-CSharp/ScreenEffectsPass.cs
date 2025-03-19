using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class ScreenEffectsPass : ScriptableRenderPass
{
	private ProfilingSampler m_profilingSampler;

	private string m_passTag;

	private LayerMask m_layerMask;

	private RenderTargetIdentifier m_cameraColorTarget;

	private RenderTargetIdentifier m_effectsMaskTexture;

	private ScreenEffectsRender m_screenEffectsRender;

	private RenderStateBlock m_depthStencilState;

	private Material m_opaqueOverrideMaterial;

	private Material m_glowMaterial;

	private int m_bloomBuffer1 = Shader.PropertyToID("_BloomBuffer1");

	private int m_bloomBuffer2 = Shader.PropertyToID("_BloomBuffer2");

	private int m_blurOffsetID = Shader.PropertyToID("_BlurOffset");

	private int m_mipLevelID = Shader.PropertyToID("_MipLevel");

	private int m_blurTexID = Shader.PropertyToID("_BlurTex");

	private RenderTargetIdentifier m_bloomBuffer1RTI;

	private RenderTargetIdentifier m_bloomBuffer2RTI;

	private List<ShaderTagId> m_opaqueShaderTags = new List<ShaderTagId>
	{
		new ShaderTagId("SRPDefaultUnlit"),
		new ShaderTagId("UniversalForward"),
		new ShaderTagId("LightweightForward")
	};

	private List<ShaderTagId> m_glowPrepassTag = new List<ShaderTagId>
	{
		new ShaderTagId("GlowPrepass")
	};

	private List<ShaderTagId> m_opaqueGlowShaderTags = new List<ShaderTagId>
	{
		new ShaderTagId("Glow")
	};

	private List<ShaderTagId> m_shaderTags = new List<ShaderTagId>
	{
		new ShaderTagId("Glow"),
		new ShaderTagId("GlowTransparent"),
		new ShaderTagId("GlowAdditive"),
		new ShaderTagId("GlowDissolveEdge"),
		new ShaderTagId("GlowCutoutDissolve")
	};

	public ScreenEffectsPass(string profilerTag, LayerMask layerMask, Material overrideMaterial, Material glowMaterial)
	{
		m_passTag = profilerTag;
		m_profilingSampler = new ProfilingSampler(profilerTag);
		m_layerMask = layerMask;
		m_opaqueOverrideMaterial = overrideMaterial;
		m_glowMaterial = glowMaterial;
	}

	public void Setup(RenderTargetIdentifier cameraColorTarget, ScreenEffectsRender screenEffectsRender)
	{
		m_cameraColorTarget = cameraColorTarget;
		m_screenEffectsRender = screenEffectsRender;
		m_effectsMaskTexture = new RenderTargetIdentifier(m_screenEffectsRender.m_MaskRenderTexture);
	}

	public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
	{
		if ((bool)m_screenEffectsRender.m_MaskRenderTexture)
		{
			ConfigureTarget(m_effectsMaskTexture);
			int maskWidth = m_screenEffectsRender.m_MaskRenderTexture.width;
			int maskHeight = m_screenEffectsRender.m_MaskRenderTexture.height;
			cmd.GetTemporaryRT(m_bloomBuffer1, maskWidth, maskHeight, 0, FilterMode.Bilinear);
			cmd.GetTemporaryRT(m_bloomBuffer2, maskWidth, maskHeight, 0, FilterMode.Bilinear);
			m_bloomBuffer1RTI = new RenderTargetIdentifier(m_bloomBuffer1);
			m_bloomBuffer2RTI = new RenderTargetIdentifier(m_bloomBuffer2);
		}
	}

	public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
	{
		if ((bool)m_screenEffectsRender.m_MaskRenderTexture)
		{
			CommandBuffer cmd = CommandBufferPool.Get(m_passTag);
			cmd.ClearRenderTarget(clearDepth: true, clearColor: true, Color.black);
			context.ExecuteCommandBuffer(cmd);
			cmd.Clear();
			FilteringSettings filter = new FilteringSettings(RenderQueueRange.opaque, LayerMask.GetMask("Default", "CardRaycast"));
			SortingCriteria sortingCriteria = renderingData.cameraData.defaultOpaqueSortFlags;
			DrawingSettings drawingSettings = CreateDrawingSettings(m_opaqueShaderTags, ref renderingData, sortingCriteria);
			drawingSettings.overrideMaterial = m_opaqueOverrideMaterial;
			drawingSettings.overrideMaterialPassIndex = 5;
			context.DrawRenderers(renderingData.cullResults, ref drawingSettings, ref filter, ref m_depthStencilState);
			FilteringSettings filter2 = new FilteringSettings(RenderQueueRange.opaque, LayerMask.GetMask("ScreenEffects"));
			SortingCriteria sortingCriteria2 = renderingData.cameraData.defaultOpaqueSortFlags;
			DrawingSettings drawingSettings2 = CreateDrawingSettings(m_glowPrepassTag, ref renderingData, sortingCriteria2);
			context.DrawRenderers(renderingData.cullResults, ref drawingSettings2, ref filter2, ref m_depthStencilState);
			drawingSettings2 = CreateDrawingSettings(m_opaqueGlowShaderTags, ref renderingData, sortingCriteria2);
			context.DrawRenderers(renderingData.cullResults, ref drawingSettings2, ref filter2, ref m_depthStencilState);
			FilteringSettings filter3 = new FilteringSettings(RenderQueueRange.transparent, m_layerMask);
			SortingCriteria sortingCriteria3 = SortingCriteria.CommonTransparent;
			DrawingSettings drawingSettings3 = CreateDrawingSettings(m_glowPrepassTag, ref renderingData, sortingCriteria3);
			context.DrawRenderers(renderingData.cullResults, ref drawingSettings3, ref filter3, ref m_depthStencilState);
			drawingSettings3 = CreateDrawingSettings(m_shaderTags, ref renderingData, sortingCriteria3);
			context.DrawRenderers(renderingData.cullResults, ref drawingSettings3, ref filter3, ref m_depthStencilState);
			cmd.SetGlobalFloat(m_blurOffsetID, 1f);
			cmd.SetGlobalInt(m_mipLevelID, 2);
			Blit(cmd, m_screenEffectsRender.m_MaskRenderTexture, m_bloomBuffer1, m_glowMaterial);
			cmd.SetGlobalFloat(m_blurOffsetID, 2f);
			cmd.SetGlobalInt(m_mipLevelID, 0);
			Blit(cmd, m_bloomBuffer1, m_bloomBuffer2, m_glowMaterial);
			if (!m_screenEffectsRender.m_Debug)
			{
				Blit(cmd, m_cameraColorTarget, m_bloomBuffer1RTI);
				cmd.SetGlobalTexture(m_blurTexID, m_bloomBuffer2);
				Blit(cmd, m_bloomBuffer1RTI, m_cameraColorTarget, m_glowMaterial, 1);
			}
			else
			{
				Blit(cmd, m_bloomBuffer2RTI, m_cameraColorTarget, m_glowMaterial, 2);
			}
			context.ExecuteCommandBuffer(cmd);
			CommandBufferPool.Release(cmd);
		}
	}

	public override void FrameCleanup(CommandBuffer cmd)
	{
		if ((bool)m_screenEffectsRender.m_MaskRenderTexture)
		{
			cmd.ReleaseTemporaryRT(m_bloomBuffer1);
			cmd.ReleaseTemporaryRT(m_bloomBuffer2);
		}
	}
}
