using System;
using UnityEngine;

public class SecondaryTextureBlendController : MonoBehaviour
{
	[Serializable]
	public class RendererInfo
	{
		public Renderer Renderer;

		public int MaterialIndex;

		public string PropertyName;

		public float MinValue;

		public float MaxValue = 1f;

		[NonSerialized]
		public MaterialPropertyBlock PropertyBlock;
	}

	public RendererInfo[] Renderers;

	[Range(0f, 1f)]
	public float BlendAmount;

	private void Update()
	{
		UpdateSecondaryTextureBlendWeight();
	}

	private void UpdateSecondaryTextureBlendWeight()
	{
		if (Renderers == null)
		{
			return;
		}
		RendererInfo[] renderers = Renderers;
		foreach (RendererInfo info in renderers)
		{
			if (!(info.Renderer == null))
			{
				if (info.PropertyBlock == null)
				{
					info.PropertyBlock = new MaterialPropertyBlock();
				}
				int propertyId = Shader.PropertyToID(info.PropertyName);
				float blendAmount = Mathf.Lerp(info.MinValue, info.MaxValue, Mathf.Clamp01(BlendAmount));
				info.Renderer.GetPropertyBlock(info.PropertyBlock, info.MaterialIndex);
				info.PropertyBlock.SetFloat(propertyId, blendAmount);
				info.Renderer.SetPropertyBlock(info.PropertyBlock, info.MaterialIndex);
			}
		}
	}
}
