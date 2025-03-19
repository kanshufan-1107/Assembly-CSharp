using UnityEngine;

public class LayerUtils
{
	public static void SetLayer(GameObject go, int layer, int? ignoredLayer = null)
	{
		if (go == null)
		{
			return;
		}
		if (!ignoredLayer.HasValue || go.layer != ignoredLayer.Value)
		{
			go.layer = layer;
		}
		foreach (Transform item in go.transform)
		{
			SetLayer(item.gameObject, layer, ignoredLayer);
		}
	}

	public static void SetLayer(Component c, int layer)
	{
		if (!(c == null) && !(c.gameObject == null))
		{
			SetLayer(c.gameObject, layer, null);
		}
	}

	public static void SetLayer(GameObject go, GameLayer layer)
	{
		if (!(go == null))
		{
			SetLayer(go, (int)layer, null);
		}
	}

	public static void SetLayer(Component c, GameLayer layer)
	{
		if (!(c == null) && !(c.gameObject == null))
		{
			SetLayer(c.gameObject, (int)layer, null);
		}
	}

	public static void ReplaceLayer(GameObject parentObject, GameLayer newLayer, GameLayer oldLayer)
	{
		if (parentObject == null)
		{
			return;
		}
		if (parentObject.layer == (int)oldLayer)
		{
			parentObject.layer = (int)newLayer;
		}
		foreach (Transform item in parentObject.transform)
		{
			ReplaceLayer(item.gameObject, newLayer, oldLayer);
		}
	}
}
