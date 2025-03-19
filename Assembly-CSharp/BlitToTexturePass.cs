using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class BlitToTexturePass : ScriptableRenderPass
{
	private const string ProfilerTag = "BlitToTexture";

	private readonly Material m_blitMaterial;

	private static readonly int s_offsetAndScale = Shader.PropertyToID("uvOffsetAndScale");

	private static readonly int s_uvRotateMtx = Shader.PropertyToID("uvRotateMtx");

	private readonly List<BlitToTextureService.Request> m_requests = new List<BlitToTextureService.Request>();

	private readonly BlitToTextureRenderLatePass m_blitToTextureRenderLatePass;

	public RenderTargetIdentifier CameraColorTarget;

	public BlitToTexturePass(BlitToTextureRenderLatePass blitToTextureRenderLatePass)
	{
		m_blitToTextureRenderLatePass = blitToTextureRenderLatePass;
		Shader blitToTextureShader = Shader.Find("Hidden/HS_URP/BlitToTexture");
		if ((bool)blitToTextureShader)
		{
			m_blitMaterial = new Material(blitToTextureShader);
		}
	}

	public void EnqueueRequest(BlitToTextureService.Request request)
	{
		m_requests.Add(request);
	}

	public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
	{
		if (!m_blitMaterial)
		{
			return;
		}
		CommandBuffer cmd = CommandBufferPool.Get("BlitToTexture");
		Camera camera = renderingData.cameraData.camera;
		foreach (BlitToTextureService.Request request in m_requests)
		{
			Vector3 viewportOffset = camera.ScreenToViewportPoint(request.Offset);
			Vector3 viewportSize = camera.ScreenToViewportPoint(request.Size);
			m_blitMaterial.SetVector(s_offsetAndScale, new Vector4(viewportOffset.x, viewportOffset.y, viewportSize.x, viewportSize.y));
			float radRotation = (float)Math.PI / 180f * request.RotationDeg;
			Vector4 rotateMtx = new Vector4(Mathf.Cos(radRotation), 0f - Mathf.Sin(radRotation), Mathf.Sin(radRotation), Mathf.Cos(radRotation));
			m_blitMaterial.SetVector(s_uvRotateMtx, rotateMtx);
			Blit(cmd, CameraColorTarget, request.TargetTexture, m_blitMaterial);
			if (request.DrawAfterRenderer != null)
			{
				m_blitToTextureRenderLatePass.EnqueueRenderer(request.DrawAfterRenderer);
			}
		}
		m_requests.Clear();
		context.ExecuteCommandBuffer(cmd);
		CommandBufferPool.Release(cmd);
	}
}
