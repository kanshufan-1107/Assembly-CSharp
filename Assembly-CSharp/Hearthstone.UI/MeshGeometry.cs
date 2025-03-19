using UnityEngine;

namespace Hearthstone.UI;

[ExecuteAlways]
[AddComponentMenu("")]
public class MeshGeometry : Geometry
{
	[SerializeField]
	[CustomEditField(T = EditType.GAME_OBJECT)]
	private WeakAssetReference m_model;

	[SerializeField]
	private Vector3 m_rotation;

	protected override WeakAssetReference[] ModelReferences => new WeakAssetReference[1] { m_model };

	protected override void OnInstancesReady(PrefabInstance[] instances)
	{
		base.OnInstancesReady(instances);
		foreach (PrefabInstance inst in instances)
		{
			if (inst.Instance != null)
			{
				inst.Instance.transform.localRotation = Quaternion.Euler(m_rotation);
				inst.Instance.transform.localPosition = Vector3.zero;
				inst.Instance.transform.localScale = Vector3.one;
			}
		}
	}
}
