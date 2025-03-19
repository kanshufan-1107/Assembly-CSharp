using UnityEngine;
using UnityEngine.Rendering.Universal;

public class FreezeFrameFeature : ScriptableRendererFeature
{
	private FreezeFramePass m_freezeFramePass = new FreezeFramePass();

	private Camera m_camera;

	private FreezeFrame m_freezeFrame;

	public override void Create()
	{
		m_freezeFramePass.renderPassEvent = RenderPassEvent.AfterRenderingTransparents;
	}

	public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
	{
		renderer.EnqueuePass(m_freezeFramePass);
	}

	public override void SetupRenderPasses(ScriptableRenderer renderer, in RenderingData renderingData)
	{
		if (m_freezeFrame == null || m_camera != renderingData.cameraData.camera)
		{
			m_camera = renderingData.cameraData.camera;
			m_freezeFrame = m_camera.GetComponent<FreezeFrame>();
		}
		m_freezeFramePass.Setup("Freeze Frame Pass", renderer.cameraColorTargetHandle, m_freezeFrame);
	}
}
