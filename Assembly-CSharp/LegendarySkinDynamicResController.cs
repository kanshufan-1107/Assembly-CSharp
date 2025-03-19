using System;
using System.Collections.Generic;
using Blizzard.T5.MaterialService.Extensions;
using UnityEngine;

public class LegendarySkinDynamicResController : MonoBehaviour
{
	public enum SizeResult
	{
		Invalid,
		Bounded,
		MaxSize
	}

	private LegendarySkin m_skin;

	private Vector2 m_originalScale = new Vector2(1f, 1f);

	private Vector2 m_originalOffset = new Vector2(0f, 0f);

	[NonSerialized]
	public Renderer Renderer;

	[NonSerialized]
	public int MaterialIdx;

	[SerializeField]
	public bool ForceFullResolution;

	public LegendarySkin Skin
	{
		get
		{
			return m_skin;
		}
		set
		{
			if (m_skin != null)
			{
				m_skin.RemoveDynamicResController(this);
			}
			m_skin = value;
			if (m_skin != null && base.isActiveAndEnabled)
			{
				m_skin.AddDynamicResController(this);
			}
		}
	}

	public void CacheMaterialProperties(Material material)
	{
		if (material != null)
		{
			m_originalScale = material.mainTextureScale;
			m_originalOffset = material.mainTextureOffset;
		}
	}

	private void OnEnable()
	{
		if (m_skin != null)
		{
			m_skin.AddDynamicResController(this);
		}
	}

	private void OnDisable()
	{
		if (m_skin != null)
		{
			m_skin.RemoveDynamicResController(this);
		}
	}

	public SizeResult GetSize(IEnumerable<Camera> cameras, out float size)
	{
		if (ForceFullResolution)
		{
			size = float.MaxValue;
			return SizeResult.MaxSize;
		}
		if (Renderer == null)
		{
			size = 0f;
			return SizeResult.Invalid;
		}
		float pixelsHigh = 0f;
		SizeResult result = SizeResult.Invalid;
		int renderLayer = 1 << Renderer.gameObject.layer;
		Bounds b = Renderer.bounds;
		foreach (Camera camera in cameras)
		{
			if ((camera.cullingMask & renderLayer) != 0 && camera.isActiveAndEnabled)
			{
				Vector3 proj = camera.WorldToScreenPoint(new Vector3(b.min.x, b.min.y, b.min.z));
				Vector2 min = proj;
				Vector2 lhs = min;
				proj = camera.WorldToScreenPoint(new Vector3(b.max.x, b.min.y, b.min.z));
				min = Vector2.Min(min, proj);
				Vector2 lhs2 = Vector2.Max(lhs, proj);
				proj = camera.WorldToScreenPoint(new Vector3(b.min.x, b.max.y, b.min.z));
				min = Vector2.Min(min, proj);
				Vector2 lhs3 = Vector2.Max(lhs2, proj);
				proj = camera.WorldToScreenPoint(new Vector3(b.max.x, b.max.y, b.min.z));
				min = Vector2.Min(min, proj);
				Vector2 lhs4 = Vector2.Max(lhs3, proj);
				proj = camera.WorldToScreenPoint(new Vector3(b.min.x, b.min.y, b.max.z));
				min = Vector2.Min(min, proj);
				Vector2 lhs5 = Vector2.Max(lhs4, proj);
				proj = camera.WorldToScreenPoint(new Vector3(b.max.x, b.min.y, b.max.z));
				min = Vector2.Min(min, proj);
				Vector2 lhs6 = Vector2.Max(lhs5, proj);
				proj = camera.WorldToScreenPoint(new Vector3(b.min.x, b.max.y, b.max.z));
				min = Vector2.Min(min, proj);
				Vector2 lhs7 = Vector2.Max(lhs6, proj);
				proj = camera.WorldToScreenPoint(new Vector3(b.max.x, b.max.y, b.max.z));
				min = Vector2.Min(min, proj);
				Vector2 vector = Vector2.Max(lhs7, proj) - min;
				float widthPixels = Mathf.Abs(vector.x) / m_originalScale.x;
				float heightPixels = Mathf.Abs(vector.y) / m_originalScale.y;
				pixelsHigh = Mathf.Max(pixelsHigh, widthPixels, heightPixels);
				result = SizeResult.Bounded;
			}
		}
		size = pixelsHigh;
		return result;
	}

	public void UpdateMaterial(float dynamicResolution)
	{
		if (Renderer != null)
		{
			Material material = Renderer.GetSharedMaterial(MaterialIdx);
			if (material != null)
			{
				material.mainTextureScale = m_originalScale * dynamicResolution;
				material.mainTextureOffset = m_originalOffset * dynamicResolution;
				material.SetVector("_DynamicResolutionScale", new Vector2(dynamicResolution, 1f / dynamicResolution));
			}
		}
	}
}
