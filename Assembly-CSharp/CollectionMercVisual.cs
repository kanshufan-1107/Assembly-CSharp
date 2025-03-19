using System.Collections.Generic;
using Hearthstone.UI;
using UnityEngine;

[CustomEditClass]
public class CollectionMercVisual : PegUIElement, IDraggableCollectionVisual
{
	private float m_wiggleIntensity;

	private Quaternion m_originalButtonRotation;

	private int m_positionIndex;

	private Listable m_contentsController;

	private WidgetInstance m_widgetInstance;

	private DeckTrayMercListContent m_listContentController;

	private Transform m_cachedTransform;

	protected override void Awake()
	{
		base.Awake();
		m_cachedTransform = base.transform;
	}

	private void Start()
	{
		m_contentsController = base.gameObject.GetComponentInParentsOnly<Listable>();
		m_widgetInstance = base.gameObject.GetComponentInParentsOnly<WidgetInstance>();
		m_listContentController = base.gameObject.GetComponentInParentsOnly<DeckTrayMercListContent>();
	}

	private void OnEnable()
	{
		m_originalButtonRotation = m_cachedTransform.localRotation;
		SetPositionIndexFromWidgetItems();
	}

	private void Update()
	{
		float wiggleStartTime = m_listContentController.m_rearrangeStartStopTweenDuration;
		float wiggleStopTime = m_listContentController.m_rearrangeStartStopTweenDuration;
		float wiggleFrequency = m_listContentController.m_rearrangeWiggleFrequency;
		float wiggleMaxAmplitude = m_listContentController.m_rearrangeWiggleAmplitude;
		Vector3 wiggleAxis = m_listContentController.m_rearrangeWiggleAxis;
		bool num = m_listContentController.IsReorderingAllowed && m_listContentController.DraggingDeckBox != null && m_listContentController.DraggingDeckBox != this;
		bool wasWiggling = m_wiggleIntensity > 0f;
		if (num)
		{
			m_wiggleIntensity = Mathf.Clamp01(m_wiggleIntensity + Time.deltaTime / wiggleStartTime);
		}
		else
		{
			m_wiggleIntensity = Mathf.Clamp01(m_wiggleIntensity - Time.deltaTime / wiggleStopTime);
		}
		bool isWiggling = m_wiggleIntensity > 0f;
		if (wasWiggling || isWiggling)
		{
			float wiggleOffset = wiggleMaxAmplitude * m_wiggleIntensity * Mathf.Cos((float)m_positionIndex + Time.time * wiggleFrequency);
			m_cachedTransform.localRotation = Quaternion.AngleAxis(wiggleOffset, wiggleAxis) * m_originalButtonRotation;
		}
	}

	protected override void OnHold()
	{
		if (!(m_listContentController == null) && !m_listContentController.IsTouchDragging)
		{
			m_listContentController.StartDragToReorder(this);
		}
	}

	public void OnStopDragToReorder()
	{
		SetPositionIndexFromWidgetItems();
	}

	private void SetPositionIndexFromWidgetItems()
	{
		if (!m_contentsController)
		{
			return;
		}
		m_positionIndex = 0;
		using IEnumerator<WidgetInstance> enumerator = m_contentsController.WidgetItems.GetEnumerator();
		while (enumerator.MoveNext() && !(enumerator.Current == m_widgetInstance))
		{
			m_positionIndex++;
		}
	}
}
