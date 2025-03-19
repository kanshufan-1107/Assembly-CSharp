using System.Linq;
using UnityEngine;

public class CustomSelectiveMeshBoundsOverride : MonoBehaviour
{
	public Renderer[] m_meshesToExcludeFromBounds;

	public Bounds ComputeBounds(bool includeInactive)
	{
		if (m_meshesToExcludeFromBounds == null || m_meshesToExcludeFromBounds.Length == 0)
		{
			return TransformUtil.GetBoundsOfChildren(base.gameObject, includeInactive);
		}
		Renderer[] childRenderers = base.gameObject.GetComponentsInChildren<Renderer>(includeInactive);
		if (childRenderers.Length == 0)
		{
			return new Bounds(base.transform.position, Vector3.zero);
		}
		Bounds compositeBounds = childRenderers[0].bounds;
		for (int i = 1; i < childRenderers.Length; i++)
		{
			Renderer r = childRenderers[i];
			if (!(r == null) && !m_meshesToExcludeFromBounds.Contains(r))
			{
				Bounds bounds = r.bounds;
				Vector3 maxPoint = Vector3.Max(bounds.max, compositeBounds.max);
				Vector3 minPoint = Vector3.Min(bounds.min, compositeBounds.min);
				compositeBounds.SetMinMax(minPoint, maxPoint);
			}
		}
		return compositeBounds;
	}
}
