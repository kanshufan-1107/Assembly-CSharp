using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class FullRenderObjectsPass : ScriptableRenderPass
{
	private FilteringSettings m_filteringSettings;

	private int m_layerMask;

	private bool clearDepth;

	private List<ShaderTagId> m_shaderTagIdList = new List<ShaderTagId>
	{
		new ShaderTagId("SRPDefaultUnlit"),
		new ShaderTagId("UniversalForward"),
		new ShaderTagId("LightweightForward")
	};

	public FullRenderObjectsPass(FullRenderObjectsFeature.RenderObjectsSettings settings)
	{
		base.renderPassEvent = settings.Event;
		m_layerMask = settings.FilterSettings.LayerMask;
		clearDepth = settings.ClearDepth;
	}

	private void RenderOpaques(ScriptableRenderContext context, ref RenderingData renderingData)
	{
		FilteringSettings filteringSettings = new FilteringSettings(RenderQueueRange.opaque, m_layerMask);
		SortingCriteria sortingCriteria = renderingData.cameraData.defaultOpaqueSortFlags;
		DrawingSettings drawingSettings = CreateDrawingSettings(m_shaderTagIdList, ref renderingData, sortingCriteria);
		context.DrawRenderers(renderingData.cullResults, ref drawingSettings, ref filteringSettings);
	}

	private void RenderTransparents(ScriptableRenderContext context, ref RenderingData renderingData)
	{
		FilteringSettings filteringSettings = new FilteringSettings(RenderQueueRange.transparent, m_layerMask);
		SortingCriteria sortingCriteria = SortingCriteria.CommonTransparent;
		DrawingSettings drawingSettings = CreateDrawingSettings(m_shaderTagIdList, ref renderingData, sortingCriteria);
		context.DrawRenderers(renderingData.cullResults, ref drawingSettings, ref filteringSettings);
	}

	public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
	{
		if (clearDepth)
		{
			ConfigureClear(ClearFlag.Depth, Color.black);
		}
	}

	public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
	{
		RenderOpaques(context, ref renderingData);
		RenderTransparents(context, ref renderingData);
	}
}
