using System;
using System.Collections.Generic;
using UnityEngine;

public class Hero_03ac_EyeTintingAnimation : StateDrivenAnimation
{
	[Serializable]
	public class Tint
	{
		public string Channel;

		public Color[] TintColors;

		public float ScrollRate;

		public bool Evaluate(float time, out Color color)
		{
			if (TintColors != null && TintColors.Length != 0)
			{
				int numColors = TintColors.Length;
				time *= ScrollRate;
				int index = Mathf.FloorToInt(time);
				float t = time - (float)index;
				t = t * t * (3f - 2f * t);
				int s0 = index % numColors;
				int s1 = (index + 1) % numColors;
				color = Color.Lerp(TintColors[s0], TintColors[s1], t);
				return true;
			}
			color = Color.magenta;
			return false;
		}
	}

	[Header("Tinting")]
	public Tint[] Tints;

	[Header("Eye Material")]
	public Renderer Renderer;

	public int MaterialIndex;

	private MaterialPropertyBlock m_propertyBlock;

	private Color m_baseColor = Color.white;

	private float m_scrollTimer;

	private static readonly int s_tintColorID = Shader.PropertyToID("_TintColor");

	protected override void Awake()
	{
		base.Awake();
		m_propertyBlock = new MaterialPropertyBlock();
		GetBaseColorFromRenderer();
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		m_scrollTimer = 0f;
	}

	protected override void UpdateAnimation(in Dictionary<string, float> channelValues)
	{
		m_scrollTimer += Time.deltaTime;
		if (!(Renderer != null))
		{
			return;
		}
		Color finalTintColor = m_baseColor;
		if (Tints != null)
		{
			Tint[] tints = Tints;
			foreach (Tint tint in tints)
			{
				if (channelValues.TryGetValue(tint.Channel, out var weight) && tint.Evaluate(m_scrollTimer, out var tintColor))
				{
					finalTintColor = Color.Lerp(finalTintColor, tintColor, weight);
				}
			}
		}
		Renderer.GetPropertyBlock(m_propertyBlock, MaterialIndex);
		m_propertyBlock.SetColor(s_tintColorID, finalTintColor);
		Renderer.SetPropertyBlock(m_propertyBlock, MaterialIndex);
	}

	private void GetBaseColorFromRenderer()
	{
		if (!(Renderer != null))
		{
			return;
		}
		List<Material> sharedMaterials = new List<Material>();
		Renderer.GetSharedMaterials(sharedMaterials);
		if (MaterialIndex >= 0 && MaterialIndex < sharedMaterials.Count)
		{
			Material eyeMaterial = sharedMaterials[MaterialIndex];
			if (eyeMaterial.HasProperty(s_tintColorID))
			{
				m_baseColor = eyeMaterial.GetColor(s_tintColorID);
			}
		}
	}
}
