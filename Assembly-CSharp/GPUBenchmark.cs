using UnityEngine;
using UnityEngine.Rendering;

public class GPUBenchmark : IBenchmark
{
	private CommandBuffer m_commandBuffer;

	private int m_noiseId = Shader.PropertyToID("_NoiseTex");

	private int m_blurId1 = Shader.PropertyToID("_BlurTex0");

	private int m_blurId2 = Shader.PropertyToID("_BlurTex1");

	private RenderTargetIdentifier m_noiseRTid;

	private RenderTargetIdentifier m_blurRTid1;

	private RenderTargetIdentifier m_blurRTid2;

	private Material m_noiseRTMaterial;

	private Material m_blurRTMaterial;

	public override bool Setup()
	{
		Shader noiseShader = Shader.Find("Custom/Noise");
		Shader blurShader = Shader.Find("Custom/FullScreen/Blur");
		if (noiseShader == null || blurShader == null)
		{
			Debug.LogError("Could not find shaders for GPU benchmark. Noise shader is " + ((noiseShader == null) ? "null" : "valid") + ", blur shader is " + ((blurShader == null) ? "null" : "valid"));
			return false;
		}
		m_commandBuffer = new CommandBuffer();
		m_commandBuffer.name = "GPU Benchmark";
		m_noiseRTMaterial = new Material(noiseShader);
		m_blurRTMaterial = new Material(blurShader);
		m_commandBuffer.GetTemporaryRT(m_noiseId, 512, 512, 0, FilterMode.Trilinear);
		m_commandBuffer.GetTemporaryRT(m_blurId1, 512, 512, 0, FilterMode.Trilinear);
		m_commandBuffer.GetTemporaryRT(m_blurId2, 512, 512, 0, FilterMode.Trilinear);
		m_noiseRTid = new RenderTargetIdentifier(m_noiseId);
		m_blurRTid1 = new RenderTargetIdentifier(m_blurId1);
		m_blurRTid2 = new RenderTargetIdentifier(m_blurId2);
		return true;
	}

	public override void Run()
	{
		m_commandBuffer.SetRenderTarget(m_noiseRTid);
		m_commandBuffer.Blit(null, m_noiseRTid, m_noiseRTMaterial);
		m_commandBuffer.SetRenderTarget(m_blurRTid1);
		m_commandBuffer.Blit(m_noiseRTid, m_blurRTid1, m_blurRTMaterial);
		m_commandBuffer.SetRenderTarget(m_blurRTid2);
		m_commandBuffer.Blit(m_blurRTid1, m_blurRTid2, m_blurRTMaterial);
		Graphics.ExecuteCommandBuffer(m_commandBuffer);
	}

	public override void Cleanup()
	{
		m_commandBuffer.Clear();
		m_commandBuffer.Dispose();
		m_noiseRTMaterial = null;
		m_blurRTMaterial = null;
	}
}
