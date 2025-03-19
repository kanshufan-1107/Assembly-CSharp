using System;
using System.Collections;
using System.Collections.Generic;
using Blizzard.T5.Services;
using Blizzard.Telemetry.WTCG.Client;
using Hearthstone.DataModels;
using Hearthstone.Store;
using Hearthstone.UI;
using UnityEngine;

public class ShopTabController : MonoBehaviour
{
	public delegate void OnTabChangedDelegate(int tabIndex, int subTabIndex);

	private struct TabData
	{
		public ShopTabDataModel ParentTabData;

		public List<ShopTabDataModel> SubTabData;
	}

	[SerializeField]
	private Widget m_widget;

	[SerializeField]
	private List<AsyncReference> m_tabRefs;

	private List<ShopTab> m_tabs = new List<ShopTab>();

	[SerializeField]
	private List<AsyncReference> m_subTabRefs;

	private List<ShopSubTab> m_subTabs = new List<ShopSubTab>();

	private List<TabData> m_tabDataModels = new List<TabData>();

	private int m_pendingTabIndex = -1;

	private int m_pendingSubTabIndex = -1;

	private int m_vcPageTabIndex = -1;

	private ShopDataModel m_shopDataModel;

	private Coroutine m_sendShopVisitTelemetryCoroutine;

	private ShopVisit m_currentShopVisitData;

	private const string UPDATE_TAB_VISUALS_EVENT = "UPDATE_TAB_VISUALS";

	private const string UPDATE_TAB_VISUALS_IMMEDIATE_EVENT = "UPDATE_TAB_VISUALS_IMMEDIATE";

	private const string CODE_REFRESH_TAB_DATA_EVENT = "CODE_REFRESH_TAB_DATA";

	private const string CODE_REFRESH_SUBTAB_DATA_EVENT = "CODE_REFRESH_SUBTAB_DATA";

	private const string CODE_UPDATE_CURRENT_TAB_EVENT = "CODE_UPDATE_CURRENT_TAB";

	private const string CODE_UPDATE_CURRENT_SUBTAB_EVENT = "CODE_UPDATE_CURRENT_SUBTAB";

	private const string CODE_TRIGGER_TAB_CHANGED_EVENT = "CODE_TRIGGER_TAB_CHANGED";

	private const string CODE_REQUEST_SUBTAB_BAR_EVENT = "CODE_REQUEST_SUBTAB_BAR";

	private const string RunestoneTabId = "runestones";

	public int ActiveTabIndex { get; private set; } = -1;

	public int ActiveSubTabIndex { get; private set; } = -1;

	private event OnTabChangedDelegate OnTabsChangedEvent;

	private void Awake()
	{
		m_widget.RegisterEventListener(HandleEvent);
		GlobalDataContext.Get().GetDataModel(24, out var dataModel);
		m_shopDataModel = (ShopDataModel)dataModel;
		foreach (AsyncReference tabRef in m_tabRefs)
		{
			if (tabRef == null)
			{
				Log.Store.PrintError("Null tab ref - Delete any removed tabs from list");
				continue;
			}
			m_tabs.Add(null);
			tabRef.RegisterReadyListener(delegate(Widget w)
			{
				SetupTab(w, m_tabRefs.IndexOf(tabRef));
			});
		}
		foreach (AsyncReference subTabRef in m_subTabRefs)
		{
			if (subTabRef == null)
			{
				Log.Store.PrintError("Null sub tab ref - Delete any removed sub tabs from list");
				continue;
			}
			m_subTabs.Add(null);
			subTabRef.RegisterReadyListener(delegate(Widget w)
			{
				SetupSubTab(w, m_subTabRefs.IndexOf(subTabRef));
			});
		}
	}

	private void Start()
	{
		Shop shop = Shop.Get();
		if (shop != null)
		{
			shop.OnCloseCompleted += OnShopClosed;
		}
	}

	private void OnDestroy()
	{
		if (m_widget != null)
		{
			m_widget.RemoveEventListener(HandleEvent);
		}
		Shop shop = Shop.Get();
		if (shop != null)
		{
			shop.OnCloseCompleted -= OnShopClosed;
		}
	}

	public void SetTabData(ShopDataModel shopDataModel)
	{
		m_tabDataModels.Clear();
		foreach (ShopPageDataModel page in shopDataModel.Pages)
		{
			if (page.Tab == null)
			{
				continue;
			}
			TabData tabData = default(TabData);
			tabData.ParentTabData = page.Tab;
			tabData.SubTabData = new List<ShopTabDataModel>();
			TabData tabData2 = tabData;
			foreach (ShopSubPageDataModel subPage in page.ShopSubPages)
			{
				tabData2.SubTabData.Add(subPage.Tab);
			}
			m_tabDataModels.Add(tabData2);
		}
		if (shopDataModel.VirtualCurrency != null)
		{
			ShopTabDataModel tabModel = new ShopTabDataModel
			{
				Id = "runestones",
				Icon = "RS",
				Name = "GLUE_SHOP_RUNESTONES",
				Locked = false
			};
			TabData tabData = default(TabData);
			tabData.ParentTabData = tabModel;
			tabData.SubTabData = new List<ShopTabDataModel> { tabModel };
			TabData rsTabData = tabData;
			m_tabDataModels.Add(rsTabData);
		}
		RebindTabData();
		RebindSubTabData();
	}

	public int GetNumberOfTabs()
	{
		return m_tabDataModels.Count;
	}

	public int GetNumberOfActiveSubTabs()
	{
		return GetNumberOfSubTabs(ActiveTabIndex);
	}

	public int GetNumberOfSubTabs(int tabIndex)
	{
		if (tabIndex < 0 || tabIndex >= m_tabDataModels.Count)
		{
			return 0;
		}
		return m_tabDataModels[tabIndex].SubTabData.Count;
	}

	public void ResetTabKnowledge()
	{
		ActiveTabIndex = -1;
		m_pendingTabIndex = -1;
		ActiveSubTabIndex = -1;
		m_pendingSubTabIndex = -1;
	}

	public void SelectTab(string tabId, string subTabId = null, bool instant = false)
	{
		if (string.IsNullOrEmpty(tabId))
		{
			Log.Store.PrintError("Cannot select Shop Tab with null or empty tab id");
			return;
		}
		int tabIndex = -1;
		int subTabIndex = -1;
		for (int i = 0; i < m_tabDataModels.Count; i++)
		{
			TabData tabData = m_tabDataModels[i];
			if (!(tabData.ParentTabData.Id == tabId))
			{
				continue;
			}
			tabIndex = i;
			if (string.IsNullOrEmpty(subTabId))
			{
				break;
			}
			for (int j = 0; j < tabData.SubTabData.Count; j++)
			{
				if (tabData.SubTabData[j].Id == subTabId)
				{
					subTabIndex = j;
					break;
				}
			}
			break;
		}
		if (tabIndex < 0)
		{
			Log.Store.PrintError("Cannot select Shop tab with id " + tabId + " as it cannot be found");
			return;
		}
		if (!string.IsNullOrEmpty(subTabId))
		{
			if (subTabIndex < 0)
			{
				Log.Store.PrintError("Cannot select Shop sub tab with id " + subTabId + " as it cannot be found. Defaulting to index 0");
				subTabIndex = 0;
			}
		}
		else
		{
			subTabIndex = 0;
		}
		SelectTab(tabIndex, subTabIndex, instant);
	}

	public void SelectTab(int tabIndex, int subTabIndex = 0, bool instant = false)
	{
		if (tabIndex < 0 || tabIndex >= m_tabs.Count)
		{
			Log.Store.PrintError($"Cannot select tab at index {tabIndex} - widgets out of range");
			return;
		}
		if (subTabIndex < 0)
		{
			Log.Store.PrintError("Cannot select sub tab with index less than 0. Defaulting to index 0");
			subTabIndex = 0;
		}
		if (tabIndex == ActiveTabIndex)
		{
			return;
		}
		if (m_tabs[tabIndex].CurrentData.Id == "runestones")
		{
			m_vcPageTabIndex = tabIndex;
			Shop.Get().ProductPageController.OpenVirtualCurrencyPurchase();
			ProductPageController productPageController = Shop.Get().ProductPageController;
			productPageController.OnProductPageClosed = (Action<ProductPage>)Delegate.Combine(productPageController.OnProductPageClosed, new Action<ProductPage>(OnProductPageClosed));
			if (ActiveTabIndex >= 0 && ActiveTabIndex < m_tabs.Count)
			{
				m_tabs[ActiveTabIndex].Deselect();
			}
			if (m_vcPageTabIndex >= 0 && m_vcPageTabIndex < m_tabs.Count)
			{
				m_tabs[m_vcPageTabIndex].Select();
			}
			return;
		}
		MarkSubTabsAsDisplayed();
		m_pendingTabIndex = tabIndex;
		ActiveTabIndex = -1;
		m_pendingSubTabIndex = subTabIndex;
		ActiveSubTabIndex = -1;
		if (instant)
		{
			UpdatePendingTabIndex();
			RebindSubTabData();
			UpdatePendingSubTabIndex();
			TriggerTabChangedEvent();
			m_widget.TriggerEvent("UPDATE_TAB_VISUALS_IMMEDIATE");
		}
		else
		{
			m_widget.TriggerEvent("UPDATE_TAB_VISUALS");
		}
		SendShopTabTelemetry();
	}

	public void SelectSubTab(int subTabIndex)
	{
		if (subTabIndex < 0 || subTabIndex >= m_subTabs.Count)
		{
			Log.Store.PrintError($"Cannot select tab at index {subTabIndex} - out of range");
			return;
		}
		MarkTabAsDisplayed();
		m_pendingSubTabIndex = subTabIndex;
		ActiveSubTabIndex = -1;
		UpdatePendingSubTabIndex();
		TriggerTabChangedEvent();
		SendShopTabTelemetry();
	}

	public void RegisterTabChangedListener(OnTabChangedDelegate del)
	{
		OnTabsChangedEvent -= del;
		OnTabsChangedEvent += del;
	}

	public void UnregisterTabChangedListener(OnTabChangedDelegate del)
	{
		OnTabsChangedEvent -= del;
	}

	public bool TryGetActiveMainTabDataModel(out ShopTabDataModel tabDataModel)
	{
		tabDataModel = null;
		if (ActiveTabIndex < 0 || ActiveTabIndex >= m_tabDataModels.Count)
		{
			return false;
		}
		tabDataModel = m_tabDataModels[ActiveTabIndex].ParentTabData;
		return tabDataModel != null;
	}

	public bool TryGetActiveSubTabDataModel(out ShopTabDataModel subTabDataModel)
	{
		subTabDataModel = null;
		if (ActiveTabIndex < 0 || ActiveTabIndex >= m_tabDataModels.Count)
		{
			return false;
		}
		TabData parentTabData = m_tabDataModels[ActiveTabIndex];
		if (ActiveSubTabIndex < 0 || ActiveSubTabIndex >= parentTabData.SubTabData.Count)
		{
			return false;
		}
		subTabDataModel = parentTabData.SubTabData[ActiveSubTabIndex];
		return subTabDataModel != null;
	}

	private void HandleEvent(string e)
	{
		switch (e)
		{
		default:
			_ = e == "CODE_REQUEST_SUBTAB_BAR";
			break;
		case "CODE_REFRESH_TAB_DATA":
			RebindTabData();
			break;
		case "CODE_REFRESH_SUBTAB_DATA":
			RebindSubTabData();
			break;
		case "CODE_UPDATE_CURRENT_TAB":
			UpdatePendingTabIndex();
			break;
		case "CODE_UPDATE_CURRENT_SUBTAB":
			UpdatePendingSubTabIndex();
			break;
		case "CODE_TRIGGER_TAB_CHANGED":
			TriggerTabChangedEvent();
			break;
		}
	}

	private void SetupTab(Widget w, int tabIndex)
	{
		if (w == null)
		{
			Log.Store.PrintError("Null widget loaded for shop tab");
			return;
		}
		ShopTab shopTab = w.GetComponentInChildren<ShopTab>(includeInactive: true);
		if (shopTab == null)
		{
			Log.Store.PrintError("Null shop tab for widget");
			return;
		}
		shopTab.RegisterTabClickListener(delegate
		{
			SelectTab(tabIndex);
		});
		m_tabs[tabIndex] = shopTab;
		if (tabIndex >= 0 && tabIndex < m_tabDataModels.Count)
		{
			shopTab.Show(m_tabDataModels[tabIndex].ParentTabData);
			if (ActiveTabIndex == tabIndex)
			{
				shopTab.Select();
			}
			else
			{
				shopTab.Deselect();
			}
		}
		else
		{
			shopTab.Hide();
		}
	}

	private void SetupSubTab(Widget w, int subTabIndex)
	{
		if (w == null)
		{
			Log.Store.PrintError("Null widget loaded for shop tab");
			return;
		}
		ShopSubTab shopTab = w.GetComponentInChildren<ShopSubTab>(includeInactive: true);
		if (shopTab == null)
		{
			Log.Store.PrintError("Null shop tab for widget");
			return;
		}
		shopTab.RegisterTabClickListener(delegate
		{
			SelectSubTab(subTabIndex);
		});
		m_subTabs[subTabIndex] = shopTab;
		if (ActiveTabIndex >= 0 && ActiveTabIndex < m_tabDataModels.Count && subTabIndex >= 0 && subTabIndex < m_tabDataModels[ActiveTabIndex].SubTabData.Count)
		{
			shopTab.Show(m_tabDataModels[ActiveTabIndex].SubTabData[subTabIndex]);
			if (ActiveSubTabIndex == subTabIndex)
			{
				shopTab.Select();
			}
			else
			{
				shopTab.Deselect();
			}
		}
		else
		{
			shopTab.Hide();
		}
	}

	private void RebindTabData()
	{
		if (m_tabs.Count < m_tabDataModels.Count)
		{
			Log.Store.PrintError($"Tab Controller does not have enough tabs to handle the number request - {m_tabDataModels.Count}. Some data will be lost");
		}
		for (int i = 0; i < m_tabs.Count; i++)
		{
			ShopTab tab = m_tabs[i];
			if (tab != null)
			{
				if (i < m_tabDataModels.Count)
				{
					tab.Show(m_tabDataModels[i].ParentTabData);
				}
				else
				{
					tab.Hide();
				}
			}
		}
	}

	private void RebindSubTabData()
	{
		if (ActiveTabIndex < 0 || ActiveTabIndex >= m_tabDataModels.Count)
		{
			return;
		}
		TabData tabData = m_tabDataModels[ActiveTabIndex];
		if (m_subTabs.Count < tabData.SubTabData.Count)
		{
			Log.Store.PrintError($"Tab Controller does not have enough sub tabs to handle the number request - {tabData.SubTabData.Count}. Some data will be lost");
		}
		for (int i = 0; i < m_subTabs.Count; i++)
		{
			ShopSubTab tab = m_subTabs[i];
			if (tab != null)
			{
				if (i < tabData.SubTabData.Count)
				{
					tab.Show(tabData.SubTabData[i]);
				}
				else
				{
					tab.Hide();
				}
			}
		}
		int activeSubTabs = GetNumberOfActiveSubTabs();
		int totalSubTabs = tabData.SubTabData.Count;
		if (ServiceManager.TryGet<IProductDataService>(out var pds) && pds != null && pds.TryGetSubTabs(tabData.ParentTabData.Id, out var subTabs) && subTabs != null)
		{
			totalSubTabs = subTabs.Count;
		}
		m_shopDataModel.ShowSubTabBar = activeSubTabs >= 1 && totalSubTabs > 1;
	}

	private void UpdatePendingTabIndex()
	{
		ActiveTabIndex = m_pendingTabIndex;
		m_pendingTabIndex = -1;
		for (int i = 0; i < m_tabs.Count; i++)
		{
			ShopTab tab = m_tabs[i];
			if (tab != null)
			{
				if (i == ActiveTabIndex)
				{
					tab.Select();
				}
				else
				{
					tab.Deselect();
				}
			}
		}
	}

	private void UpdatePendingSubTabIndex()
	{
		ActiveSubTabIndex = m_pendingSubTabIndex;
		m_pendingSubTabIndex = -1;
		for (int i = 0; i < m_subTabs.Count; i++)
		{
			ShopSubTab tab = m_subTabs[i];
			if (tab != null)
			{
				if (i == ActiveSubTabIndex)
				{
					tab.Select();
				}
				else
				{
					tab.Deselect();
				}
			}
		}
	}

	private void MarkSubTabsAsDisplayed()
	{
		if (!ServiceManager.TryGet<IProductDataService>(out var productService) || ActiveTabIndex < 0 || ActiveTabIndex >= m_tabDataModels.Count)
		{
			return;
		}
		foreach (ShopTabDataModel subTab in m_tabDataModels[ActiveTabIndex].SubTabData)
		{
			productService.MarkTabAsDisplayed(m_tabDataModels[ActiveTabIndex].ParentTabData, subTab);
		}
	}

	private void MarkTabAsDisplayed()
	{
		if (ServiceManager.TryGet<IProductDataService>(out var productService) && ActiveTabIndex >= 0 && ActiveTabIndex < m_tabDataModels.Count && ActiveSubTabIndex >= 0 && ActiveSubTabIndex < m_tabDataModels[ActiveTabIndex].SubTabData.Count)
		{
			productService.MarkTabAsDisplayed(m_tabDataModels[ActiveTabIndex].ParentTabData, m_tabDataModels[ActiveTabIndex].SubTabData[ActiveSubTabIndex]);
		}
	}

	private void TriggerTabChangedEvent()
	{
		this.OnTabsChangedEvent?.Invoke(ActiveTabIndex, ActiveSubTabIndex);
	}

	private void OnProductPageClosed(ProductPage page)
	{
		if (!(Shop.Get() == null) && Shop.Get().IsOpen() && (bool)page == Shop.Get().ProductPageController.IsVCPage(page))
		{
			ProductPageController productPageController = Shop.Get().ProductPageController;
			productPageController.OnProductPageClosed = (Action<ProductPage>)Delegate.Remove(productPageController.OnProductPageClosed, new Action<ProductPage>(OnProductPageClosed));
			if (ActiveTabIndex >= 0 && ActiveTabIndex < m_tabs.Count)
			{
				m_tabs[ActiveTabIndex].Select();
			}
			if (m_vcPageTabIndex >= 0 && m_vcPageTabIndex < m_tabs.Count)
			{
				m_tabs[m_vcPageTabIndex].Deselect();
			}
			m_vcPageTabIndex = -1;
		}
	}

	private void OnShopClosed()
	{
		MarkTabAsDisplayed();
		MarkSubTabsAsDisplayed();
	}

	private void SendShopTabTelemetry()
	{
		Shop shop = Shop.Get();
		if (!(shop == null))
		{
			if (m_sendShopVisitTelemetryCoroutine != null)
			{
				StopCoroutine(m_sendShopVisitTelemetryCoroutine);
				SendShopVisitTelemetry(m_currentShopVisitData, 0f);
			}
			m_currentShopVisitData = shop.GetShopVisitTelemetryData();
			m_sendShopVisitTelemetryCoroutine = StartCoroutine(SendShopVisitTelemetryCoroutine());
		}
	}

	private void SendShopVisitTelemetry(ShopVisit data, float time)
	{
		if (data != null)
		{
			TelemetryManager.Client().SendShopVisit(data.Cards, data.StoreType, data.ShopTab, data.ShopSubTab, time);
		}
	}

	private IEnumerator SendShopVisitTelemetryCoroutine()
	{
		Shop shop = Shop.Get();
		if (shop == null)
		{
			yield break;
		}
		float startTime = Time.time;
		FrameTimer timer = new FrameTimer();
		timer.StartRecording();
		while (true)
		{
			if (shop == null)
			{
				yield break;
			}
			if ((shop.IsBrowserReady && !shop.IsBrowserDirty) || Time.time - startTime >= 20f)
			{
				break;
			}
			yield return null;
		}
		if (!shop.IsBrowserReady)
		{
			yield return null;
		}
		timer.StopRecording();
		SendShopVisitTelemetry(shop.GetShopVisitTelemetryData(), timer.TimeTaken);
		m_sendShopVisitTelemetryCoroutine = null;
	}
}
