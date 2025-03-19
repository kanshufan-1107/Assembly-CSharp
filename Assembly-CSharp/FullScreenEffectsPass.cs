using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class FullScreenEffectsPass : ScriptableRenderPass
{
	private string m_ProfilerTag;

	private FullScreenEffectsFeature.Settings m_settings;

	private FullScreenEffects m_fullScreenEffects;

	private int m_blur1 = Shader.PropertyToID("_Blur1");

	private RenderTargetIdentifier m_blur1Id;

	private int m_blur2 = Shader.PropertyToID("_Blur2");

	private RenderTargetIdentifier m_blur2Id;

	private int m_desaturationTexture = Shader.PropertyToID("_DesaturationTexture");

	private RenderTargetIdentifier m_desaturationTextureID;

	private int m_amountID = Shader.PropertyToID("_Amount");

	private int m_brightnessID = Shader.PropertyToID("_Brightness");

	private int m_desaturationID = Shader.PropertyToID("_Desaturation");

	private int m_maskTexID = Shader.PropertyToID("_MaskTex");

	private int m_colorID = Shader.PropertyToID("_Color");

	private int m_blurOffsetID = Shader.PropertyToID("_BlurOffset");

	private int m_blendTexID = Shader.PropertyToID("_BlendTex");

	private const int BLUR_BUFFER_SIZE = 512;

	private const float BLUR_SECOND_PASS_REDUCTION = 0.5f;

	private const float BLUR_PASS_1_OFFSET = 1f;

	private const float BLUR_PASS_2_OFFSET = 0.4f;

	private const float BLUR_PASS_3_OFFSET = -0.2f;

	private Camera m_currentCachedCamera;

	private readonly Dictionary<Camera, RenderTexture> m_renderTextures = new Dictionary<Camera, RenderTexture>();

	private RenderTexture m_activeRenderTexture;

	public void Setup(string profilerTag, FullScreenEffectsFeature.Settings settings, FullScreenEffects fullScreenEffects, Camera currentCamera)
	{
		m_ProfilerTag = profilerTag;
		m_settings = settings;
		m_fullScreenEffects = fullScreenEffects;
		m_currentCachedCamera = currentCamera;
	}

	private void CalcTextureSize(int currentWidth, int currentHeight, out float outWidth, out float outHeight)
	{
		float width = currentWidth;
		float height = currentHeight;
		if (width > height)
		{
			outWidth = 512f;
			outHeight = 512f * (height / width);
		}
		else
		{
			outWidth = 512f * (width / height);
			outHeight = 512f;
		}
	}

	public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
	{
		if (m_fullScreenEffects.DisablePreFullscreenRenderFeatures)
		{
			m_renderTextures.TryGetValue(m_currentCachedCamera, out var cachedRenderResult);
			if (cachedRenderResult != null && !m_fullScreenEffects.UsingCachedResult && (cachedRenderResult.width != cameraTextureDescriptor.width || cachedRenderResult.height != cameraTextureDescriptor.height))
			{
				cachedRenderResult.Release();
				cachedRenderResult = null;
			}
			if (cachedRenderResult == null)
			{
				cachedRenderResult = new RenderTexture(cameraTextureDescriptor.width, cameraTextureDescriptor.height, 0);
				m_renderTextures[m_currentCachedCamera] = cachedRenderResult;
			}
		}
		else
		{
			if (m_renderTextures.TryGetValue(m_currentCachedCamera, out var cachedRenderResult2))
			{
				cachedRenderResult2.Release();
				m_renderTextures.Remove(m_currentCachedCamera);
			}
			m_fullScreenEffects.UsingCachedResult = false;
		}
		if (m_fullScreenEffects.BlurEnabled)
		{
			float width = cameraTextureDescriptor.width;
			float height = cameraTextureDescriptor.height;
			CalcTextureSize(cameraTextureDescriptor.width, cameraTextureDescriptor.height, out width, out height);
			cmd.GetTemporaryRT(m_blur1, (int)width, (int)height, 0, FilterMode.Bilinear, RenderTextureFormat.ARGB32);
			m_blur1Id = new RenderTargetIdentifier(m_blur1);
			cmd.GetTemporaryRT(m_blur2, (int)(width * 0.5f), (int)(height * 0.5f), 0, FilterMode.Bilinear, RenderTextureFormat.ARGB32);
			m_blur2Id = new RenderTargetIdentifier(m_blur2);
			m_settings.m_blurMaterial.SetFloat(m_brightnessID, 1f);
		}
		if (m_fullScreenEffects.DesaturationEnabled)
		{
			if (m_fullScreenEffects.BlurEnabled)
			{
				float desaturation = 1f - m_fullScreenEffects.Desaturation;
				m_settings.m_desaturationMaterial.SetFloat(m_desaturationID, desaturation * desaturation * desaturation);
				m_desaturationTextureID = new RenderTargetIdentifier(m_blur1);
			}
			else
			{
				m_settings.m_desaturationMaterial.SetFloat(m_desaturationID, 1f - m_fullScreenEffects.Desaturation);
				cmd.GetTemporaryRT(m_desaturationTexture, cameraTextureDescriptor.width, cameraTextureDescriptor.height, 0, FilterMode.Bilinear, RenderTextureFormat.R8);
				m_desaturationTextureID = new RenderTargetIdentifier(m_desaturationTexture);
			}
		}
		if (m_fullScreenEffects.VignettingEnable)
		{
			float intensity = m_fullScreenEffects.VignettingIntensity;
			if (m_fullScreenEffects.BlurEnabled)
			{
				intensity = 1f - intensity;
				intensity = intensity * intensity * intensity;
				intensity = 1f - intensity;
			}
			m_settings.m_vignettingMaterial.SetFloat(m_amountID, intensity);
		}
		if (m_fullScreenEffects.BlendToColorEnable)
		{
			m_settings.m_blendToColorMaterial.SetFloat(m_amountID, m_fullScreenEffects.BlendToColorAmount);
			m_settings.m_blendToColorMaterial.SetColor(m_colorID, m_fullScreenEffects.BlendColor);
		}
		ConfigureTarget(m_settings.cameraColorTarget, m_settings.cameraDepthTargetFunc());
	}

	public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
	{
		CommandBuffer cmd = CommandBufferPool.Get(m_ProfilerTag);
		cmd.ClearRenderTarget(clearDepth: true, clearColor: false, Color.black);
		context.ExecuteCommandBuffer(cmd);
		cmd.Clear();
		m_renderTextures.TryGetValue(m_currentCachedCamera, out var cachedRenderResult);
		if (m_fullScreenEffects.DisablePreFullscreenRenderFeatures && m_fullScreenEffects.UsingCachedResult && cachedRenderResult != null)
		{
			Blit(cmd, cachedRenderResult, m_settings.cameraColorTarget);
		}
		else
		{
			if (m_fullScreenEffects.DesaturationEnabled)
			{
				Blit(cmd, m_settings.cameraColorTarget, m_desaturationTextureID, m_settings.m_desaturationMaterial);
				Blit(cmd, m_desaturationTextureID, m_settings.cameraColorTarget, m_settings.m_desaturationMaterial, 1);
			}
			if (m_fullScreenEffects.VignettingEnable)
			{
				Blit(cmd, m_fullScreenEffects.m_VignettingMask, m_settings.cameraColorTarget, m_settings.m_vignettingMaterial);
			}
			if (m_fullScreenEffects.BlurEnabled)
			{
				cmd.SetGlobalFloat(m_blurOffsetID, 1f);
				Blit(cmd, m_settings.cameraColorTarget, m_blur1Id, m_settings.m_blurMaterial);
				cmd.SetGlobalFloat(m_blurOffsetID, 0.4f);
				Blit(cmd, m_blur1Id, m_blur2Id, m_settings.m_blurMaterial);
				cmd.SetGlobalFloat(m_blurOffsetID, -0.2f);
				if (m_fullScreenEffects.BlurBlend >= 1f)
				{
					Blit(cmd, m_blur2Id, m_settings.cameraColorTarget, m_settings.m_blurMaterial);
				}
				else
				{
					Blit(cmd, m_blur2Id, m_blur1Id, m_settings.m_blurMaterial);
					cmd.SetGlobalFloat(m_amountID, m_fullScreenEffects.BlurBlend);
					Blit(cmd, m_blur1Id, m_settings.cameraColorTarget, m_settings.m_blurBlendMaterial);
				}
			}
			if (m_fullScreenEffects.BlendToColorEnable)
			{
				Blit(cmd, m_fullScreenEffects.m_VignettingMask, m_settings.cameraColorTarget, m_settings.m_blendToColorMaterial);
			}
			if (m_fullScreenEffects.DisablePreFullscreenRenderFeatures && cachedRenderResult != null)
			{
				Blit(cmd, m_settings.cameraColorTarget, cachedRenderResult);
				m_fullScreenEffects.UsingCachedResult = true;
			}
			else if (cachedRenderResult != null)
			{
				cachedRenderResult.Release();
				cachedRenderResult = null;
				m_renderTextures.Remove(m_currentCachedCamera);
			}
		}
		context.ExecuteCommandBuffer(cmd);
		CommandBufferPool.Release(cmd);
	}

	public override void FrameCleanup(CommandBuffer cmd)
	{
		if (m_fullScreenEffects.BlurEnabled)
		{
			cmd.ReleaseTemporaryRT(m_blur1);
			cmd.ReleaseTemporaryRT(m_blur2);
		}
		else if (m_fullScreenEffects.DesaturationEnabled)
		{
			cmd.ReleaseTemporaryRT(m_desaturationTexture);
		}
	}
}
