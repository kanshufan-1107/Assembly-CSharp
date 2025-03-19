using System.Collections.Generic;
using Hearthstone.UI;
using UnityEngine;

public abstract class ShopBrowserElement : MonoBehaviour
{
	[SerializeField]
	[Header("Element Data")]
	private Widget m_widget;

	[SerializeField]
	private WidgetTransform m_widgetTransform;

	[Header("Element Layouts")]
	[SerializeField]
	private List<ShopBrowserElementLayout> m_layouts;

	private HashSet<string> m_suppressBoundChangedTriggerIds = new HashSet<string>();

	public bool IsElementEnabled { get; private set; }

	public Widget Widget => m_widget;

	public WidgetInstance Instance { get; private set; }

	public WidgetTransform WidgetTransform => m_widgetTransform;

	public Rect WidgetRect => m_widgetTransform.Rect;

	public Vector3 ElementLocalPosition
	{
		get
		{
			return base.transform.parent.localPosition;
		}
		set
		{
			base.transform.parent.localPosition = value;
		}
	}

	protected virtual void Awake()
	{
		Instance = GetComponentInParent<WidgetInstance>();
		m_widgetTransform.OnBoundsChanged += TriggerBoundsChanged;
		OnBoundsChanged();
	}

	public void SetEnabled(bool isEnabled)
	{
		IsElementEnabled = isEnabled;
		OnEnabledStateChanged(isEnabled);
	}

	public void SetHeight(float height)
	{
		SuppressBoundsChangedTrigger("SET_HEIGHT", isSuppressed: true);
		WidgetTransform.Bottom = (0f - height) / 2f;
		WidgetTransform.Top = height / 2f;
		SuppressBoundsChangedTrigger("SET_HEIGHT", isSuppressed: false);
	}

	public void SetWidth(float width)
	{
		SuppressBoundsChangedTrigger("SET_WIDTH", isSuppressed: true);
		WidgetTransform.Left = (0f - width) / 2f;
		WidgetTransform.Right = width / 2f;
		SuppressBoundsChangedTrigger("SET_WIDTH", isSuppressed: false);
	}

	public void SuppressBoundsChangedTrigger(string suppressId, bool isSuppressed)
	{
		if (isSuppressed)
		{
			m_suppressBoundChangedTriggerIds.Add(suppressId);
		}
		else if (m_suppressBoundChangedTriggerIds.Remove(suppressId) && m_suppressBoundChangedTriggerIds.Count == 0)
		{
			TriggerBoundsChanged();
		}
	}

	public void TriggerBoundsChanged()
	{
		if (m_suppressBoundChangedTriggerIds.Count <= 0)
		{
			OnBoundsChanged();
		}
	}

	protected abstract void OnEnabledStateChanged(bool isEnabled);

	protected virtual void OnBoundsChanged()
	{
		foreach (ShopBrowserElementLayout layout in m_layouts)
		{
			if (layout != null)
			{
				layout.UpdateLayout(m_widgetTransform);
			}
		}
	}
}
