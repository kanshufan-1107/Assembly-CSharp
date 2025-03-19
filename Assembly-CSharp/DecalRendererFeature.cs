using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class DecalRendererFeature : ScriptableRendererFeature
{
	public class CopyDepthForDecalsPass : ScriptableRenderPass
	{
		private Material m_copyDepthMaterial;

		public CopyDepthForDecalsPass()
		{
			base.renderPassEvent = RenderPassEvent.AfterRenderingTransparents;
			Shader copyDepthShader = Shader.Find("Hidden/Universal Render Pipeline/CopyDepth");
			if ((bool)copyDepthShader)
			{
				m_copyDepthMaterial = new Material(copyDepthShader);
			}
		}

		public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
		{
			RenderTextureDescriptor descriptor = cameraTextureDescriptor;
			descriptor.colorFormat = RenderTextureFormat.Depth;
			descriptor.depthBufferBits = 16;
			descriptor.msaaSamples = 1;
			descriptor.width /= 4;
			descriptor.height /= 4;
			cmd.GetTemporaryRT(s_tempDepth, descriptor);
			ConfigureTarget(s_tempDepth);
		}

		public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
		{
			if ((bool)m_copyDepthMaterial)
			{
				CommandBuffer cmd = CommandBufferPool.Get("Decal Depth Copy");
				RenderTextureDescriptor descriptor = renderingData.cameraData.cameraTargetDescriptor;
				int cameraSamples = descriptor.msaaSamples;
				CameraData cameraData = renderingData.cameraData;
				switch (cameraSamples)
				{
				case 8:
					cmd.DisableShaderKeyword("_DEPTH_MSAA_2");
					cmd.DisableShaderKeyword("_DEPTH_MSAA_4");
					cmd.EnableShaderKeyword("_DEPTH_MSAA_8");
					break;
				case 4:
					cmd.DisableShaderKeyword("_DEPTH_MSAA_2");
					cmd.EnableShaderKeyword("_DEPTH_MSAA_4");
					cmd.DisableShaderKeyword("_DEPTH_MSAA_8");
					break;
				case 2:
					cmd.EnableShaderKeyword("_DEPTH_MSAA_2");
					cmd.DisableShaderKeyword("_DEPTH_MSAA_4");
					cmd.DisableShaderKeyword("_DEPTH_MSAA_8");
					break;
				default:
					cmd.DisableShaderKeyword("_DEPTH_MSAA_2");
					cmd.DisableShaderKeyword("_DEPTH_MSAA_4");
					cmd.DisableShaderKeyword("_DEPTH_MSAA_8");
					break;
				}
				float flipSign = (cameraData.IsCameraProjectionMatrixFlipped() ? (-1f) : 1f);
				Vector4 scaleBias = ((flipSign < 0f) ? new Vector4(flipSign, 1f, -1f, 1f) : new Vector4(flipSign, 0f, 1f, 1f));
				cmd.SetGlobalVector(s_scaleBiasId, scaleBias);
				cmd.DrawMesh(RenderingUtils.fullscreenMesh, Matrix4x4.identity, m_copyDepthMaterial);
				context.ExecuteCommandBuffer(cmd);
				CommandBufferPool.Release(cmd);
			}
		}
	}

	public class DecalRendererPass : ScriptableRenderPass
	{
		public DecalRendererPass()
		{
			base.renderPassEvent = RenderPassEvent.AfterRenderingTransparents;
		}

		public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
		{
			ConfigureClear(ClearFlag.None, Color.black);
		}

		public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
		{
			CommandBuffer cmd = CommandBufferPool.Get("Decal Pass");
			foreach (DecalProjector decal in s_decals)
			{
				Material material = decal.Material;
				if (material != null)
				{
					cmd.DrawRenderer(decal.Renderer, material);
				}
			}
			cmd.ReleaseTemporaryRT(s_tempDepth);
			context.ExecuteCommandBuffer(cmd);
			cmd.Release();
		}
	}

	public static string s_tempDepthName;

	public static int s_tempDepth;

	public static int s_scaleBiasId;

	public static List<DecalProjector> s_decals;

	private DecalRendererPass m_pass;

	private CopyDepthForDecalsPass m_copyPass;

	static DecalRendererFeature()
	{
		s_tempDepthName = "QuarterResDecalDepthCopy";
		s_tempDepth = Shader.PropertyToID(s_tempDepthName);
		s_scaleBiasId = Shader.PropertyToID("_ScaleBiasRT");
		s_decals = new List<DecalProjector>();
	}

	public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
	{
		if (s_decals.Count > 0)
		{
			renderer.EnqueuePass(m_copyPass);
			renderer.EnqueuePass(m_pass);
		}
	}

	public override void Create()
	{
		m_pass = new DecalRendererPass();
		m_copyPass = new CopyDepthForDecalsPass();
	}
}
