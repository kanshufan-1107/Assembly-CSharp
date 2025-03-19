using System.Collections.Generic;
using Blizzard.T5.MaterialService.Extensions;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class BlitToTextureRenderLatePass : ScriptableRenderPass
{
	private const string ProfilerTag = "BlitToTextureRenderLate";

	private readonly List<Renderer> m_requests = new List<Renderer>();

	public RenderTargetIdentifier CameraColorTarget;

	public void EnqueueRenderer(Renderer renderer)
	{
		m_requests.Add(renderer);
	}

	public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
	{
		ConfigureTarget(CameraColorTarget);
	}

	public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
	{
		CommandBuffer cmd = CommandBufferPool.Get("BlitToTextureRenderLate");
		foreach (Renderer renderer in m_requests)
		{
			foreach (Material material in renderer.GetMaterials())
			{
				cmd.DrawRenderer(renderer, material);
			}
		}
		m_requests.Clear();
		context.ExecuteCommandBuffer(cmd);
		CommandBufferPool.Release(cmd);
	}
}
