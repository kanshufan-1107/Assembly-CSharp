using System.Collections.Generic;
using UnityEngine.Rendering.Universal;

public class BlitToTextureFeature : ScriptableRendererFeature
{
	private static BlitToTextureFeature s_instance;

	private BlitToTexturePass m_pass;

	private BlitToTextureRenderLatePass m_renderLatePass;

	private readonly List<BlitToTextureService.Request> m_persistentRequests = new List<BlitToTextureService.Request>();

	public static BlitToTextureFeature Get()
	{
		return s_instance;
	}

	public override void Create()
	{
		s_instance = this;
		m_renderLatePass = new BlitToTextureRenderLatePass
		{
			renderPassEvent = RenderPassEvent.AfterRenderingTransparents
		};
		m_pass = new BlitToTexturePass(m_renderLatePass)
		{
			renderPassEvent = RenderPassEvent.AfterRenderingTransparents
		};
	}

	public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
	{
		foreach (BlitToTextureService.Request request in m_persistentRequests)
		{
			EnqueueRequest(request);
		}
		renderer.EnqueuePass(m_pass);
		renderer.EnqueuePass(m_renderLatePass);
	}

	public override void SetupRenderPasses(ScriptableRenderer renderer, in RenderingData renderingData)
	{
		m_pass.CameraColorTarget = renderer.cameraColorTargetHandle;
		m_renderLatePass.CameraColorTarget = renderer.cameraColorTargetHandle;
	}

	public void EnqueueRequest(BlitToTextureService.Request request)
	{
		m_pass.EnqueueRequest(request);
	}

	public void AddPersistentRequest(BlitToTextureService.Request request)
	{
		m_persistentRequests.Add(request);
	}

	public void RemovePersistentRequest(BlitToTextureService.Request request)
	{
		m_persistentRequests.Remove(request);
	}
}
