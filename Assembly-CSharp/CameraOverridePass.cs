using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class CameraOverridePass : CustomViewPass
{
	[Flags]
	public enum OverrideFlags
	{
		Scissor = 1,
		ProjectionMatrix = 2,
		RenderLayerMask = 4,
		ViewMatrix = 8
	}

	public readonly string passName;

	public LayerMask layerMask;

	public RenderStateBlock depthStencilState;

	private static readonly List<ShaderTagId> s_ShaderTagIdList = new List<ShaderTagId>
	{
		new ShaderTagId("SRPDefaultUnlit"),
		new ShaderTagId("UniversalForward"),
		new ShaderTagId("LightweightForward")
	};

	private ProfilingSampler m_ProfilingSampler;

	public OverrideFlags toOverride { get; private set; }

	public uint renderLayerMaskOverride { get; private set; }

	public Matrix4x4 projectionOverride { get; private set; }

	public Matrix4x4 viewMatrixOverride { get; private set; }

	public Rect scissorOverride { get; private set; }

	public CameraOverridePass(string name, LayerMask layers)
	{
		passName = name;
		layerMask = layers;
		m_ProfilingSampler = new ProfilingSampler(name);
		base.renderPassEvent = RenderPassEvent.AfterRenderingTransparents;
	}

	public void OverrideRenderLayerMask(uint renderLayerMask)
	{
		renderLayerMaskOverride = renderLayerMask;
		toOverride |= OverrideFlags.RenderLayerMask;
	}

	public void OverrideProjectionMatrix(Matrix4x4 projectionMtx)
	{
		projectionOverride = projectionMtx;
		toOverride |= OverrideFlags.ProjectionMatrix;
	}

	public void OverrideViewMatrix(Matrix4x4 viewMtx)
	{
		viewMatrixOverride = viewMtx;
		toOverride |= OverrideFlags.ViewMatrix;
	}

	public void OverrideScissor(Rect scissor)
	{
		scissorOverride = scissor;
		toOverride |= OverrideFlags.Scissor;
	}

	public void ClearOverrides(OverrideFlags toClear)
	{
		toOverride &= ~toClear;
	}

	public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
	{
		ref CameraData cameraData = ref renderingData.cameraData;
		CommandBuffer cmd = CommandBufferPool.Get(passName);
		using (new ProfilingScope(cmd, m_ProfilingSampler))
		{
			if (toOverride.HasFlag(OverrideFlags.ProjectionMatrix | OverrideFlags.ViewMatrix))
			{
				Matrix4x4 projectionMatrix = cameraData.GetGPUProjectionMatrix();
				Matrix4x4 viewMatrix = cameraData.GetViewMatrix();
				if (toOverride.HasFlag(OverrideFlags.ProjectionMatrix))
				{
					projectionMatrix = GL.GetGPUProjectionMatrix(projectionOverride, cameraData.IsCameraProjectionMatrixFlipped());
				}
				if (toOverride.HasFlag(OverrideFlags.ViewMatrix))
				{
					viewMatrix = viewMatrixOverride;
				}
				RenderingUtils.SetViewAndProjectionMatrices(cmd, viewMatrix, projectionMatrix, setInverseMatrices: false);
			}
			if (toOverride.HasFlag(OverrideFlags.Scissor))
			{
				cmd.EnableScissorRect(scissorOverride);
			}
			context.ExecuteCommandBuffer(cmd);
			cmd.Clear();
			FilteringSettings filter = new FilteringSettings(RenderQueueRange.opaque, layerMask);
			if (toOverride.HasFlag(OverrideFlags.RenderLayerMask))
			{
				filter.renderingLayerMask = renderLayerMaskOverride;
			}
			SortingCriteria sortingCriteria = renderingData.cameraData.defaultOpaqueSortFlags;
			DrawingSettings drawingSettings = CreateDrawingSettings(s_ShaderTagIdList, ref renderingData, sortingCriteria);
			context.DrawRenderers(renderingData.cullResults, ref drawingSettings, ref filter, ref depthStencilState);
			FilteringSettings filter2 = new FilteringSettings(RenderQueueRange.transparent, layerMask);
			if (toOverride.HasFlag(OverrideFlags.RenderLayerMask))
			{
				filter2.renderingLayerMask = renderLayerMaskOverride;
			}
			SortingCriteria sortingCriteria2 = SortingCriteria.CommonTransparent;
			DrawingSettings drawingSettings2 = CreateDrawingSettings(s_ShaderTagIdList, ref renderingData, sortingCriteria2);
			context.DrawRenderers(renderingData.cullResults, ref drawingSettings2, ref filter2, ref depthStencilState);
			if (toOverride.HasFlag(OverrideFlags.ProjectionMatrix | OverrideFlags.ViewMatrix))
			{
				RenderingUtils.SetViewAndProjectionMatrices(cmd, cameraData.GetViewMatrix(), cameraData.GetGPUProjectionMatrix(), setInverseMatrices: false);
			}
			if (toOverride.HasFlag(OverrideFlags.Scissor))
			{
				cmd.DisableScissorRect();
			}
		}
		context.ExecuteCommandBuffer(cmd);
		CommandBufferPool.Release(cmd);
	}
}
