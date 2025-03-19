using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class FreezeFramePass : ScriptableRenderPass
{
	private string m_ProfilerTag;

	private FreezeFrame m_FreezeFrame;

	private RenderTargetIdentifier m_CameraColorTarget;

	public void Setup(string profilerTag, RenderTargetIdentifier cameraColorTarget, FreezeFrame freezeFrame)
	{
		m_ProfilerTag = profilerTag;
		m_CameraColorTarget = cameraColorTarget;
		m_FreezeFrame = freezeFrame;
	}

	public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
	{
		if (m_FreezeFrame.m_CaptureFrozenImage && !m_FreezeFrame.m_FrozenState)
		{
			CommandBuffer cmd = CommandBufferPool.Get(m_ProfilerTag);
			cmd.Blit(m_CameraColorTarget, m_FreezeFrame.m_FrozenScreenTexture);
			context.ExecuteCommandBuffer(cmd);
			CommandBufferPool.Release(cmd);
			m_FreezeFrame.m_CaptureFrozenImage = false;
			m_FreezeFrame.m_FrozenState = true;
			m_FreezeFrame.m_DeactivateFrameCount = 0;
		}
		if (m_FreezeFrame.m_FrozenState)
		{
			CommandBuffer cmd2 = CommandBufferPool.Get(m_ProfilerTag);
			Blit(cmd2, m_FreezeFrame.m_FrozenScreenTexture, m_CameraColorTarget);
			context.ExecuteCommandBuffer(cmd2);
			CommandBufferPool.Release(cmd2);
			m_FreezeFrame.m_DeactivateFrameCount = 0;
		}
	}
}
