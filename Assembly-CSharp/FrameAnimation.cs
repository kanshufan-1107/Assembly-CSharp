using UnityEngine;

[ExecuteAlways]
public class FrameAnimation : MonoBehaviour
{
	public Vector2 tiles = Vector2.one;

	public float currentFrame;

	public Material material;

	private Vector4 scaleOffsetUV;

	private string scaleOffsetUVParametrName = "_MainTex_ST";

	private void Start()
	{
		scaleOffsetUV = new Vector4(1f / tiles.x, 1f / tiles.y);
	}

	private void Update()
	{
		if (!(material == null))
		{
			int currentFrameI = Mathf.FloorToInt(currentFrame);
			scaleOffsetUV.z = (float)currentFrameI % tiles.x;
			scaleOffsetUV.w = tiles.y - 1f - ((float)currentFrameI - scaleOffsetUV.z) / tiles.x % tiles.y;
			scaleOffsetUV.z *= scaleOffsetUV.x;
			scaleOffsetUV.w *= scaleOffsetUV.y;
			material.SetVector(scaleOffsetUVParametrName, scaleOffsetUV);
		}
	}
}
