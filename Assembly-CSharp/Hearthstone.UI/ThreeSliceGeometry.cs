using Blizzard.T5.AssetManager;
using Hearthstone.UI.Core;
using UnityEngine;

namespace Hearthstone.UI;

[RequireComponent(typeof(WidgetTransform))]
[ExecuteAlways]
[AddComponentMenu("")]
public class ThreeSliceGeometry : Geometry, IBoundsDependent
{
	[SerializeField]
	private WeakAssetReference m_left;

	[SerializeField]
	private WeakAssetReference m_middle;

	[SerializeField]
	private WeakAssetReference m_right;

	private WidgetTransform m_widgetTransform;

	private Rect m_leftRect;

	private Rect m_middleRect;

	private Rect m_rightRect;

	private GameObject m_leftInstance;

	private GameObject m_middleInstance;

	private GameObject m_rightInstance;

	private AssetHandle<GameObject> m_leftPrefab;

	private AssetHandle<GameObject> m_middlePrefab;

	private AssetHandle<GameObject> m_rightPrefab;

	protected override WeakAssetReference[] ModelReferences => new WeakAssetReference[3] { m_left, m_middle, m_right };

	public bool NeedsBounds => true;

	protected override void OnInitialize()
	{
		base.OnInitialize();
		m_widgetTransform = GetComponent<WidgetTransform>();
		UpdateSlices();
	}

	protected override void OnDestroy()
	{
		AssetHandle.SafeDispose(ref m_leftPrefab);
		AssetHandle.SafeDispose(ref m_middlePrefab);
		AssetHandle.SafeDispose(ref m_rightPrefab);
		base.OnDestroy();
	}

	protected void Update()
	{
		UpdateSlices();
	}

	protected override void OnInstancesReady(PrefabInstance[] instances)
	{
		base.OnInstancesReady(instances);
		m_leftInstance = instances[0].Instance;
		m_middleInstance = instances[1].Instance;
		m_rightInstance = instances[2].Instance;
		AssetHandle.Set(ref m_leftPrefab, instances[0].Prefab);
		AssetHandle.Set(ref m_middlePrefab, instances[1].Prefab);
		AssetHandle.Set(ref m_rightPrefab, instances[2].Prefab);
		CalculateRects();
	}

	private void CalculateRects()
	{
		m_leftRect = GetRectForInstance(m_leftPrefab);
		m_middleRect = GetRectForInstance(m_middlePrefab);
		m_rightRect = GetRectForInstance(m_rightPrefab);
	}

	private Rect GetRectForInstance(GameObject go)
	{
		if (go == null)
		{
			return Rect.zero;
		}
		Bounds result = default(Bounds);
		Renderer[] meshes = go.GetComponentsInChildren<Renderer>(includeInactive: true);
		for (int i = 0; i < meshes.Length; i++)
		{
			if (i == 0)
			{
				result = meshes[i].bounds;
			}
			else
			{
				result.Encapsulate(meshes[i].bounds);
			}
		}
		Rect r = new Rect(result.center, new Vector3(result.size.x, result.size.z, 0f));
		r.min = go.transform.worldToLocalMatrix.MultiplyPoint(r.min);
		r.max = go.transform.worldToLocalMatrix.MultiplyPoint(r.max);
		return r;
	}

	private void UpdateSlices()
	{
		if (!(m_leftInstance == null) && !(m_rightInstance == null) && !(m_middleInstance == null))
		{
			Vector3 p = m_leftInstance.transform.localPosition;
			p.x = m_widgetTransform.Rect.xMin - m_leftRect.xMin + m_leftRect.size.x * 0.5f;
			p.z = m_widgetTransform.Rect.center.y;
			m_leftInstance.transform.localPosition = p;
			Vector3 p2 = m_middleInstance.transform.localPosition;
			Vector3 s = m_middleInstance.transform.localScale;
			float lmax = m_widgetTransform.Rect.xMin + m_leftRect.size.x;
			float num = m_widgetTransform.Rect.xMax - m_rightRect.size.x;
			float width = m_middleRect.width;
			float overlapFactor = num - lmax - width;
			float requiredWidth = width + overlapFactor;
			s.x = requiredWidth / width;
			p2.x = m_widgetTransform.Rect.center.x - m_middleRect.x * s.x;
			p2.z = m_widgetTransform.Rect.center.y;
			m_middleInstance.transform.localPosition = p2;
			m_middleInstance.transform.localScale = s;
			Vector3 p3 = m_rightInstance.transform.localPosition;
			p3.x = m_widgetTransform.Rect.xMax - m_rightRect.xMin - m_rightRect.size.x * 0.5f;
			p3.z = m_widgetTransform.Rect.center.y;
			m_rightInstance.transform.localPosition = p3;
		}
	}
}
