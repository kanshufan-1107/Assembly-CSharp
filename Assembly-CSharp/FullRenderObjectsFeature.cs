using System;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class FullRenderObjectsFeature : ScriptableRendererFeature
{
	[Serializable]
	public class RenderObjectsSettings
	{
		public RenderPassEvent Event = RenderPassEvent.AfterRenderingTransparents;

		public bool ClearDepth;

		public FilterSettings FilterSettings = new FilterSettings();
	}

	[Serializable]
	public class FilterSettings
	{
		public LayerMask LayerMask;

		public FilterSettings()
		{
			LayerMask = 0;
		}
	}

	public RenderObjectsSettings Settings = new RenderObjectsSettings();

	private FullRenderObjectsPass m_renderObjectsPass;

	public override void Create()
	{
		if (Settings.Event < RenderPassEvent.BeforeRenderingPrePasses)
		{
			Settings.Event = RenderPassEvent.BeforeRenderingPrePasses;
		}
		m_renderObjectsPass = new FullRenderObjectsPass(Settings);
	}

	public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
	{
		renderer.EnqueuePass(m_renderObjectsPass);
	}
}
