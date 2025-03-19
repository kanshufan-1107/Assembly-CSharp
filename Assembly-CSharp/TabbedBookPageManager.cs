using System;
using System.Collections;
using System.Collections.Generic;
using Blizzard.T5.Core;
using UnityEngine;

public abstract class TabbedBookPageManager : BookPageManager
{
	public GameObject m_tabContainer;

	public BookTab m_tabPrefab;

	public float m_spaceBetweenTabs;

	protected BookTab m_currentTab;

	protected List<BookTab> m_allTabs = new List<BookTab>();

	protected Map<BookTab, bool> m_tabVisibility = new Map<BookTab, bool>();

	protected bool m_tabsAreAnimating;

	public event Action OnVisibleTabsUpdated;

	protected override void Start()
	{
		SetUpBookTabs();
		base.Start();
	}

	public void UpdateVisibleTabs()
	{
		this.OnVisibleTabsUpdated?.Invoke();
		if (!SceneMgr.Get().IsInLettuceMode() && (bool)UniversalInputManager.UsePhoneUI)
		{
			return;
		}
		bool visibleTabsChanged = false;
		foreach (BookTab tab in m_allTabs)
		{
			bool num = m_tabVisibility[tab];
			bool tabIsVisible = ShouldShowTab(tab);
			if (num != tabIsVisible)
			{
				visibleTabsChanged = true;
				m_tabVisibility[tab] = tabIsVisible;
			}
		}
		if (visibleTabsChanged)
		{
			PositionBookTabs(animate: true);
		}
	}

	protected abstract bool ShouldShowTab(BookTab tab);

	protected abstract void SetUpBookTabs();

	protected abstract void PositionBookTabs(bool animate);

	protected void DeselectCurrentTab()
	{
		if (!(m_currentTab == null))
		{
			m_currentTab.SetSelected(selected: false);
			m_currentTab.SetLargeTab(large: false);
			m_currentTab = null;
		}
	}

	protected virtual void OnTabOver(UIEvent e)
	{
		BookTab tab = e.GetElement() as BookTab;
		if (!(tab == null))
		{
			tab.SetGlowActive(active: true);
		}
	}

	protected void OnTabOut(UIEvent e)
	{
		BookTab tab = e.GetElement() as BookTab;
		if (!(tab == null))
		{
			tab.SetGlowActive(active: false);
			_ = tab as CollectionClassTab == null;
		}
	}

	protected void OnTabOver_Touch(UIEvent e)
	{
		if (UniversalInputManager.Get().IsTouchMode())
		{
			(e.GetElement() as BookTab).SetLargeTab(large: true);
		}
	}

	protected void OnTabOut_Touch(UIEvent e)
	{
		if (UniversalInputManager.Get().IsTouchMode())
		{
			BookTab tab = e.GetElement() as BookTab;
			if (tab != m_currentTab)
			{
				tab.SetLargeTab(large: false);
			}
		}
	}

	protected override void TransitionPage(object callbackData)
	{
		base.TransitionPage(callbackData);
		UpdateVisibleTabs();
	}

	protected override void HandleTouchModeChanged()
	{
		base.HandleTouchModeChanged();
		foreach (BookTab allTab in m_allTabs)
		{
			allTab.SetReceiveReleaseWithoutMouseDown(UniversalInputManager.Get().IsTouchMode());
		}
	}

	protected void PositionFixedTab(bool showTab, BookTab tab, Vector3 originalPos, bool animate)
	{
		if (!showTab)
		{
			originalPos.z -= 0.5f;
		}
		tab.SetTargetVisibility(showTab);
		tab.SetTargetLocalPosition(originalPos);
		if (animate)
		{
			tab.AnimateToTargetPosition(0.4f, iTween.EaseType.easeOutQuad);
			return;
		}
		tab.SetIsVisible(tab.ShouldBeVisible());
		tab.transform.localPosition = originalPos;
	}

	protected IEnumerator SelectTabWhenReady(BookTab tab)
	{
		while (m_tabsAreAnimating)
		{
			yield return 0;
		}
		if (!(m_currentTab != tab))
		{
			tab.SetSelected(selected: true);
			tab.SetLargeTab(large: true);
		}
	}
}
