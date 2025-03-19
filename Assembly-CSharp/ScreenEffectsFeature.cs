using System;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class ScreenEffectsFeature : ScriptableRendererFeature
{
	[Serializable]
	public class ScreenEffectsSettings
	{
		public Material m_opaqueOverrideMaterial;

		public Material m_glowMaterial;
	}

	public ScreenEffectsSettings m_settings = new ScreenEffectsSettings();

	private ScreenEffectsPass m_ScreenEffectsPass;

	private Camera m_camera;

	private ScreenEffectsRender m_screenEffectsRender;

	public override void Create()
	{
		LayerMask layerMask = (1 << LayerMask.NameToLayer("Default")) | (1 << LayerMask.NameToLayer("ScreenEffects"));
		m_ScreenEffectsPass = new ScreenEffectsPass("ScreenEffectsPass", layerMask, m_settings.m_opaqueOverrideMaterial, m_settings.m_glowMaterial);
		m_ScreenEffectsPass.renderPassEvent = RenderPassEvent.AfterRenderingTransparents;
	}

	public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
	{
		renderer.EnqueuePass(m_ScreenEffectsPass);
	}

	public override void SetupRenderPasses(ScriptableRenderer renderer, in RenderingData renderingData)
	{
		if (m_screenEffectsRender == null || m_camera != renderingData.cameraData.camera)
		{
			m_camera = renderingData.cameraData.camera;
			m_screenEffectsRender = m_camera.GetComponent<ScreenEffectsRender>();
		}
		m_ScreenEffectsPass.Setup(renderer.cameraColorTargetHandle, m_screenEffectsRender);
	}
}
