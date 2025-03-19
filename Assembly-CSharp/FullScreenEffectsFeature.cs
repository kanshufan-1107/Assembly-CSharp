using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class FullScreenEffectsFeature : ScriptableRendererFeature
{
	[Serializable]
	public class Settings
	{
		public RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingTransparents;

		public Material m_blurMaterial;

		public Material m_blurBlendMaterial;

		public Material m_desaturationMaterial;

		public Material m_vignettingMaterial;

		public Material m_blendToColorMaterial;

		public RenderTargetIdentifier cameraColorTarget;

		public Func<RenderTargetIdentifier> cameraDepthTargetFunc;
	}

	private class ClearDepthPass : ScriptableRenderPass
	{
		public ClearDepthPass()
		{
			base.renderPassEvent = RenderPassEvent.AfterRenderingTransparents;
		}

		public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
		{
			ConfigureClear(ClearFlag.Depth, Color.black);
		}

		public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
		{
		}
	}

	private FullScreenEffectsPass m_FullScreenEffectsPass;

	private ClearDepthPass m_ClearDepthPass;

	public Settings settings = new Settings();

	private Camera m_currentCamera;

	private FullScreenEffects m_fullScreenEffects;

	public override void Create()
	{
		m_FullScreenEffectsPass = new FullScreenEffectsPass();
		m_ClearDepthPass = new ClearDepthPass();
		m_FullScreenEffectsPass.renderPassEvent = settings.renderPassEvent;
	}

	public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
	{
		renderer.EnqueuePass(m_FullScreenEffectsPass);
		if (IsHuaweiDevice())
		{
			renderer.EnqueuePass(m_ClearDepthPass);
		}
	}

	public override void SetupRenderPasses(ScriptableRenderer renderer, in RenderingData renderingData)
	{
		if (m_fullScreenEffects == null || m_currentCamera != renderingData.cameraData.camera)
		{
			m_currentCamera = renderingData.cameraData.camera;
			m_fullScreenEffects = m_currentCamera.GetComponent<FullScreenEffects>();
		}
		settings.cameraColorTarget = renderer.cameraColorTargetHandle;
		settings.cameraDepthTargetFunc = () => renderer.cameraDepthTargetHandle;
		m_FullScreenEffectsPass.Setup("Full Screen Effects Pass", settings, m_fullScreenEffects, m_currentCamera);
	}

	private bool IsHuaweiDevice()
	{
		if (SystemInfo.deviceModel.Contains("huawei", StringComparison.OrdinalIgnoreCase))
		{
			return true;
		}
		return false;
	}
}
