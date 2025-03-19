using Blizzard.T5.MaterialService.Extensions;
using UnityEngine;

public class TiledBackground : MonoBehaviour
{
	private Renderer m_renderer;

	private Material m_material;

	public float Depth;

	private Renderer TiledRenderer
	{
		get
		{
			if (m_renderer == null)
			{
				m_renderer = GetComponent<Renderer>();
			}
			return m_renderer;
		}
	}

	private Material TiledMaterial
	{
		get
		{
			if (m_material == null && TiledRenderer != null)
			{
				m_material = TiledRenderer.GetMaterial();
			}
			return m_material;
		}
	}

	public Vector2 Offset
	{
		get
		{
			if (TiledMaterial == null)
			{
				return Vector2.zero;
			}
			Vector3 mainTextureOffset = TiledMaterial.mainTextureOffset;
			Vector3 mainTextureScale = TiledMaterial.mainTextureScale;
			return new Vector2(mainTextureOffset.x / mainTextureScale.x, mainTextureOffset.y / mainTextureScale.y);
		}
		set
		{
			if (!(TiledMaterial == null))
			{
				Vector3 mainTextureScale = TiledMaterial.mainTextureScale;
				TiledMaterial.mainTextureOffset = new Vector2(mainTextureScale.x * value.x, mainTextureScale.y * value.y);
			}
		}
	}

	private void Awake()
	{
		if (TiledMaterial == null)
		{
			Debug.LogError("TiledBackground requires the mesh renderer and for it to have a material!");
			Object.Destroy(this);
		}
	}

	public void SetBounds(Bounds bounds)
	{
		if (TiledRenderer == null)
		{
			Debug.LogError("TiledBackground.SetBounds - no renderer was found on this game object!");
			return;
		}
		base.transform.localScale = Vector3.one;
		Bounds renderBounds = TiledRenderer.bounds;
		Vector3 min = renderBounds.min;
		Vector3 max = renderBounds.max;
		if (base.transform.parent != null)
		{
			min = base.transform.parent.InverseTransformPoint(min);
			max = base.transform.parent.InverseTransformPoint(max);
		}
		Vector3 size = VectorUtils.Abs(max - min);
		Vector3 scale = new Vector3((size.x > 0f) ? (bounds.size.x / size.x) : 0f, (size.y > 0f) ? (bounds.size.y / size.y) : 0f, (size.z > 0f) ? (bounds.size.z / size.z) : 0f);
		base.transform.localScale = scale;
		base.transform.localPosition = bounds.center + new Vector3(0f, 0f, 0f - Depth);
		if (TiledMaterial == null)
		{
			Debug.LogError("TiledBackground.SetBounds - no material was found on this component!");
		}
		else
		{
			TiledMaterial.mainTextureScale = scale;
		}
	}
}
