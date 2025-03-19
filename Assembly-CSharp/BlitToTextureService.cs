using UnityEngine;

public static class BlitToTextureService
{
	public class Request
	{
		public Vector2 Offset;

		public Vector2 Size;

		public RenderTexture TargetTexture;

		public Renderer DrawAfterRenderer;

		public float RotationDeg;
	}

	public static void AddPersistentRequest(Request request)
	{
		BlitToTextureFeature.Get().AddPersistentRequest(request);
	}

	public static void RemovePersistentRequest(Request request)
	{
		BlitToTextureFeature.Get().RemovePersistentRequest(request);
	}
}
