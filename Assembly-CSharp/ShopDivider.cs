using System.Collections.Generic;
using Hearthstone.UI.Core;
using UnityEngine;

public class ShopDivider : ShopBrowserElement, IPopupRendering
{
	[Header("Divider Data")]
	[SerializeField]
	private UIBScrollableItem m_scrollableItem;

	private IPopupRoot m_popupRoot;

	private HashSet<IPopupRendering> m_popupRenderingComponents = new HashSet<IPopupRendering>();

	protected override void OnEnabledStateChanged(bool isEnabled)
	{
	}

	protected override void OnBoundsChanged()
	{
		base.OnBoundsChanged();
		if (m_scrollableItem != null)
		{
			m_scrollableItem.m_offset.x = base.WidgetRect.x + base.WidgetRect.width / 2f;
			m_scrollableItem.m_offset.z = base.WidgetRect.y + base.WidgetRect.height / 2f;
			m_scrollableItem.m_size.x = base.WidgetRect.width;
			m_scrollableItem.m_size.z = base.WidgetRect.height;
		}
	}

	void IPopupRendering.EnablePopupRendering(IPopupRoot popupRoot)
	{
		popupRoot.ApplyPopupRendering(base.transform, m_popupRenderingComponents);
	}

	void IPopupRendering.DisablePopupRendering()
	{
		if (m_popupRoot != null)
		{
			m_popupRoot.CleanupPopupRendering(m_popupRenderingComponents);
			m_popupRoot = null;
		}
	}

	bool IPopupRendering.HandlesChildPropagation()
	{
		return true;
	}
}
