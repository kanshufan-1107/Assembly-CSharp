using System;
using System.Collections.Generic;
using UnityEngine;

public class ManaFilterTabManager : MonoBehaviour
{
	public ManaFilterTab m_singleManaFilterPrefab;

	public ManaFilterTab m_dynamicManaFilterPrefab;

	public MultiSliceElement m_manaCrystalContainer;

	private bool m_tabsActive;

	private List<ManaFilterTab> m_tabs = new List<ManaFilterTab>();

	private HashSet<int> m_currentFilterExactValues = new HashSet<int>();

	private int? m_currentFilterMinValue;

	private int? m_currentFilterMaxValue;

	private bool m_currentFilterIsEven;

	private bool m_currentFilterIsOdd;

	public bool IsFilterActive
	{
		get
		{
			if (!IsFilterOnExactValues && !IsFilterOddOrEvenValues)
			{
				return IsFilterRange;
			}
			return true;
		}
	}

	public bool IsFilterOnExactValues => m_currentFilterExactValues.Count > 0;

	public bool IsFilterOddOrEvenValues
	{
		get
		{
			if (!IsFilterEvenValues)
			{
				return IsFilterOddValues;
			}
			return true;
		}
	}

	public bool IsFilterEvenValues => m_currentFilterIsEven;

	public bool IsFilterOddValues => m_currentFilterIsOdd;

	public bool IsFilterRange
	{
		get
		{
			if (!m_currentFilterMinValue.HasValue)
			{
				return m_currentFilterMaxValue.HasValue;
			}
			return true;
		}
	}

	public bool Enabled
	{
		get
		{
			foreach (ManaFilterTab tab in m_tabs)
			{
				if (!tab.IsEnabled())
				{
					return false;
				}
			}
			return true;
		}
		set
		{
			foreach (ManaFilterTab tab in m_tabs)
			{
				tab.SetEnabled(value);
				ManaFilterTab.FilterState tabFilterState = ManaFilterTab.FilterState.DISABLED;
				if (tab.IsEnabled() && m_tabsActive)
				{
					tabFilterState = GetTabFilterState(tab.GetManaID());
				}
				tab.SetFilterState(tabFilterState);
				if (tab.m_costText != null)
				{
					tab.m_costText.gameObject.SetActive(value);
				}
			}
		}
	}

	public event Action<bool> OnFilterCleared;

	public event Action<int, bool> OnManaValueActivated;

	public void ClearFilter(bool transitionPage = true)
	{
		UpdateCurrentFilterToSingleValue(-1, transitionPage);
	}

	public bool IsManaValueActive(int manaValue)
	{
		if (m_currentFilterExactValues.Contains(manaValue))
		{
			return true;
		}
		if (IsFilterRange)
		{
			if (m_currentFilterMinValue.HasValue && manaValue < m_currentFilterMinValue.Value)
			{
				return false;
			}
			if (m_currentFilterMaxValue.HasValue && manaValue > m_currentFilterMaxValue.Value)
			{
				return false;
			}
			return true;
		}
		if (IsFilterEvenValues)
		{
			return manaValue % 2 == 0;
		}
		if (IsFilterOddValues)
		{
			return manaValue % 2 == 1;
		}
		return false;
	}

	public void SetFilter_Range(int minCost, int maxCost)
	{
		if (!m_currentFilterMinValue.HasValue || m_currentFilterMinValue.Value != minCost || !m_currentFilterMaxValue.HasValue || m_currentFilterMaxValue.Value != maxCost)
		{
			SoundManager.Get().LoadAndPlay("mana_crystal_refresh.prefab:ea5c456dd852f904e9828db66636f54d");
		}
		m_currentFilterExactValues.Clear();
		m_currentFilterMinValue = minCost;
		m_currentFilterMaxValue = maxCost;
		m_currentFilterIsEven = false;
		m_currentFilterIsOdd = false;
		UpdateFilterStates();
	}

	public void SetFilter_EvenOdd(bool isOdd)
	{
		if (!IsFilterOddOrEvenValues || isOdd != IsFilterOddValues)
		{
			SoundManager.Get().LoadAndPlay("mana_crystal_refresh.prefab:ea5c456dd852f904e9828db66636f54d");
		}
		m_currentFilterExactValues.Clear();
		m_currentFilterMinValue = (m_currentFilterMaxValue = null);
		m_currentFilterIsEven = !isOdd;
		m_currentFilterIsOdd = isOdd;
		UpdateFilterStates();
	}

	public void SetUpTabs()
	{
		for (int i = 0; i <= 6; i++)
		{
			CreateNewTab(m_singleManaFilterPrefab, i);
		}
		CreateNewTab(m_dynamicManaFilterPrefab, 7);
		m_manaCrystalContainer.UpdateSlices();
	}

	public void ActivateTabs(bool active)
	{
		m_tabsActive = active;
		UpdateFilterStates();
		if (active)
		{
			m_manaCrystalContainer.UpdateSlices();
		}
	}

	private void CreateNewTab(ManaFilterTab tabPrefab, int index)
	{
		ManaFilterTab newTab = (ManaFilterTab)GameUtils.Instantiate(tabPrefab, m_manaCrystalContainer.gameObject);
		newTab.SetManaID(index);
		newTab.AddEventListener(UIEventType.RELEASE, OnTabPressed);
		newTab.AddEventListener(UIEventType.ROLLOVER, OnTabMousedOver);
		newTab.AddEventListener(UIEventType.ROLLOUT, OnTabMousedOut);
		newTab.SetFilterState(ManaFilterTab.FilterState.DISABLED);
		if (UniversalInputManager.Get().IsTouchMode())
		{
			newTab.SetReceiveReleaseWithoutMouseDown(receiveReleaseWithoutMouseDown: true);
		}
		m_tabs.Add(newTab);
		m_manaCrystalContainer.AddSlice(newTab.gameObject);
	}

	private void OnTabPressed(UIEvent e)
	{
		if (m_tabsActive)
		{
			ManaFilterTab clickedTab = (ManaFilterTab)e.GetElement();
			if (!UniversalInputManager.UsePhoneUI && !Options.Get().GetBool(Option.HAS_CLICKED_MANA_TAB, defaultVal: false) && UserAttentionManager.CanShowAttentionGrabber("ManaFilterTabManager.OnTabPressed:" + Option.HAS_CLICKED_MANA_TAB))
			{
				Options.Get().SetBool(Option.HAS_CLICKED_MANA_TAB, val: true);
				ShowManaTabHint(clickedTab);
			}
			if (IsManaValueActive(clickedTab.GetManaID()))
			{
				TelemetryManager.Client().SendManaFilterToggleOff();
				UpdateCurrentFilterToSingleValue(-1);
			}
			else
			{
				UpdateCurrentFilterToSingleValue(clickedTab.GetManaID());
			}
		}
	}

	private void OnTabMousedOver(UIEvent e)
	{
		if (m_tabsActive)
		{
			((ManaFilterTab)e.GetElement()).NotifyMousedOver();
		}
	}

	private void OnTabMousedOut(UIEvent e)
	{
		if (m_tabsActive)
		{
			((ManaFilterTab)e.GetElement()).NotifyMousedOut();
		}
	}

	private ManaFilterTab.FilterState GetTabFilterState(int manaValue)
	{
		if (!IsManaValueActive(manaValue))
		{
			return ManaFilterTab.FilterState.OFF;
		}
		return ManaFilterTab.FilterState.ON;
	}

	private void UpdateCurrentFilterToSingleValue(int manaValue, bool transitionPage = true)
	{
		int num;
		if (!m_currentFilterIsEven && !m_currentFilterIsOdd && m_currentFilterExactValues.Count == 1)
		{
			num = ((!m_currentFilterExactValues.Contains(manaValue)) ? 1 : 0);
			if (num == 0)
			{
				goto IL_0047;
			}
		}
		else
		{
			num = 1;
		}
		SoundManager.Get().LoadAndPlay("mana_crystal_refresh.prefab:ea5c456dd852f904e9828db66636f54d");
		goto IL_0047;
		IL_0047:
		m_currentFilterExactValues.Clear();
		if (manaValue != -1)
		{
			m_currentFilterExactValues.Add(manaValue);
		}
		m_currentFilterMinValue = (m_currentFilterMaxValue = null);
		m_currentFilterIsEven = false;
		m_currentFilterIsOdd = false;
		UpdateFilterStates();
		if (num == 0)
		{
			return;
		}
		if (manaValue == -1)
		{
			if (this.OnFilterCleared != null)
			{
				this.OnFilterCleared(transitionPage);
			}
		}
		else if (this.OnManaValueActivated != null)
		{
			this.OnManaValueActivated(manaValue, transitionPage);
		}
	}

	private void UpdateFilterStates()
	{
		foreach (ManaFilterTab tab in m_tabs)
		{
			ManaFilterTab.FilterState tabFilterState = ManaFilterTab.FilterState.DISABLED;
			if (tab.IsEnabled() && m_tabsActive)
			{
				tabFilterState = GetTabFilterState(tab.GetManaID());
			}
			tab.SetFilterState(tabFilterState);
		}
	}

	private void ShowManaTabHint(ManaFilterTab tabButton)
	{
		Notification notification = NotificationManager.Get().CreatePopupText(UserAttentionBlocker.NONE, tabButton.transform.position + new Vector3(0f, 0f, 7f), TutorialEntity.GetTextScale(), GameStrings.Get("GLUE_COLLECTION_MANAGER_MANA_TAB_FIRST_CLICK"));
		if (!(notification == null))
		{
			notification.ShowPopUpArrow(Notification.PopUpArrowDirection.Down);
			NotificationManager.Get().DestroyNotification(notification, 3f);
		}
	}
}
