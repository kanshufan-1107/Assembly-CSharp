using UnityEngine;

public class UberTextRenderTextureTracker : IUberTextRenderTextureTracker
{
	public RenderTexture CreateNewTexture(int width, int height, int depth, RenderTextureFormat format = RenderTextureFormat.Default, RenderTextureReadWrite readWrite = RenderTextureReadWrite.Default)
	{
		return RenderTextureTracker.Get().CreateNewTexture(width, height, depth, format, readWrite);
	}

	public void DestroyRenderTexture(RenderTexture renderTexture)
	{
		RenderTextureTracker.Get().DestroyRenderTexture(renderTexture);
	}
}
