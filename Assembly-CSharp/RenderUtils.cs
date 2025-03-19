using Blizzard.T5.Core;
using Blizzard.T5.MaterialService.Extensions;
using UnityEngine;

public static class RenderUtils
{
	public static readonly int MinRenderQueueValue = 1;

	public static readonly int MaxRenderQueueValue = 5000;

	public static void SetAlpha(GameObject go, float alpha)
	{
		SetAlpha(go, alpha, includeInactive: false);
	}

	public static void SetAlpha(GameObject go, float alpha, bool includeInactive)
	{
		Renderer[] componentsInChildren = go.GetComponentsInChildren<Renderer>(includeInactive);
		foreach (Renderer r in componentsInChildren)
		{
			foreach (Material m in r.GetMaterials())
			{
				if (m.HasProperty("_Color"))
				{
					Color color = m.color;
					color.a = alpha;
					m.color = color;
				}
				else if (m.HasProperty("_TintColor"))
				{
					Color color2 = m.GetColor("_TintColor");
					color2.a = alpha;
					m.SetColor("_TintColor", color2);
				}
			}
			if (r.TryGetComponent<Light>(out var light))
			{
				Color color3 = light.color;
				color3.a = alpha;
				light.color = color3;
			}
		}
		UberText[] componentsInChildren2 = go.GetComponentsInChildren<UberText>(includeInactive);
		foreach (UberText obj in componentsInChildren2)
		{
			Color c = obj.TextColor;
			obj.TextColor = new Color(c.r, c.g, c.b, alpha);
		}
	}

	public static int ClampRenderQueueValue(int value)
	{
		return Mathf.Clamp(value, MinRenderQueueValue, MaxRenderQueueValue);
	}

	public static void SetInvisibleRenderer(Renderer renderer, bool show, ref Map<Renderer, int> originalLayers)
	{
		if (originalLayers == null)
		{
			originalLayers = new Map<Renderer, int>();
		}
		if (renderer != null)
		{
			GameObject gameObject = renderer.gameObject;
			int originalLayer = gameObject.layer;
			int overrideLayer = originalLayer;
			if (show && originalLayer == 28 && !originalLayers.TryGetValue(renderer, out overrideLayer))
			{
				overrideLayer = originalLayer;
			}
			if (!show && originalLayer != 28)
			{
				originalLayers[renderer] = originalLayer;
				overrideLayer = 28;
			}
			gameObject.layer = overrideLayer;
		}
	}

	public static void SetRenderQueue(GameObject go, int renderQueue, bool includeInactive = false)
	{
		Renderer[] componentsInChildren = go.GetComponentsInChildren<Renderer>(includeInactive);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			Material material = componentsInChildren[i].GetMaterial();
			if (!(material == null))
			{
				material.renderQueue = renderQueue;
			}
		}
	}

	public static void EnableRenderers(GameObject go, bool enable)
	{
		EnableRenderers(go, enable, includeInactive: false);
	}

	public static void EnableRenderers(GameObject go, bool enable, bool includeInactive)
	{
		Renderer[] renderers = go.GetComponentsInChildren<Renderer>(includeInactive);
		if (renderers != null)
		{
			Renderer[] array = renderers;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].enabled = enable;
			}
		}
	}

	public static void EnableColliders(GameObject go, bool enable)
	{
		Collider[] colliders = go.GetComponentsInChildren<Collider>();
		if (colliders != null)
		{
			Collider[] array = colliders;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].enabled = enable;
			}
		}
	}

	public static void EnableRenderersAndColliders(GameObject go, bool enable)
	{
		Collider c = go.GetComponent<Collider>();
		if (c != null)
		{
			c.enabled = enable;
		}
		Renderer r = go.GetComponent<Renderer>();
		if (r != null)
		{
			r.enabled = enable;
		}
		foreach (Transform item in go.transform)
		{
			EnableRenderersAndColliders(item.gameObject, enable);
		}
	}
}
