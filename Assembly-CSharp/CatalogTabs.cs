using System.Collections.Generic;
using Hearthstone.DataModels;

public class CatalogTabs
{
	private class CatalogTab
	{
		private Dictionary<string, CatalogSubTab> m_subTabs = new Dictionary<string, CatalogSubTab>();

		public ShopTabData TabData { get; private set; }

		public CatalogTab(ShopTabData tabData)
		{
			TabData = tabData;
		}

		public bool HasSubTab(string tabId)
		{
			return m_subTabs.ContainsKey(tabId);
		}

		public void AddSubTab(ShopTabData subTabData)
		{
			if (subTabData == null || string.IsNullOrEmpty(subTabData.Id))
			{
				Log.Store.PrintError("Cannot add sub tab - null tab data");
			}
			else if (HasSubTab(subTabData.Id))
			{
				Log.Store.PrintError("Cannot add sub tab - duplicate ID: " + subTabData.Id);
			}
			else
			{
				m_subTabs.Add(subTabData.Id, new CatalogSubTab(subTabData));
			}
		}

		public IEnumerable<CatalogSubTab> GetAllSubTabs()
		{
			return m_subTabs.Values;
		}

		public bool TryGetSubTab(string subTabId, out CatalogSubTab subTab)
		{
			return m_subTabs.TryGetValue(subTabId, out subTab);
		}
	}

	private class CatalogSubTab
	{
		private readonly List<ProductTierDataModel> m_tiers = new List<ProductTierDataModel>();

		public ShopTabData TabData { get; private set; }

		public CatalogSubTab(ShopTabData tabData)
		{
			TabData = tabData;
		}

		public void AddTier(ProductTierDataModel tier)
		{
			if (tier == null)
			{
				Log.Store.PrintError("Cannot add tier - null tier data model");
			}
			else
			{
				m_tiers.Add(tier);
			}
		}

		public List<ProductTierDataModel> GetTiers()
		{
			return m_tiers;
		}
	}

	private Dictionary<string, CatalogTab> m_tabs = new Dictionary<string, CatalogTab>();

	public bool HasTab(string tabId)
	{
		return m_tabs.ContainsKey(tabId);
	}

	public void AddTab(ShopTabData tabData)
	{
		if (tabData == null || string.IsNullOrEmpty(tabData.Id))
		{
			Log.Store.PrintError("Cannot add tab - null tab data");
		}
		else if (HasTab(tabData.Id))
		{
			Log.Store.PrintError("Cannot add tab - duplicate ID: " + tabData.Id);
		}
		else
		{
			m_tabs.Add(tabData.Id, new CatalogTab(tabData));
		}
	}

	public void AddSubTab(string parentTabId, ShopTabData subTabData)
	{
		CatalogTab tab;
		if (string.IsNullOrEmpty(parentTabId) || subTabData == null)
		{
			Log.Store.PrintError("Cannot add sub tab -  null tab data");
		}
		else if (!TryGetTab(parentTabId, out tab))
		{
			Log.Store.PrintError("Cannot add sub tab - no parent tab: " + parentTabId);
		}
		else
		{
			tab.AddSubTab(subTabData);
		}
	}

	public void AddTier(string parentTabId, string parentSubTabId, ProductTierDataModel tier)
	{
		CatalogTab tab;
		CatalogSubTab subTab;
		if (string.IsNullOrEmpty(parentTabId) || string.IsNullOrEmpty(parentSubTabId))
		{
			Log.Store.PrintError("Cannot add tier -  null tab data");
		}
		else if (!TryGetTab(parentTabId, out tab))
		{
			Log.Store.PrintError("Cannot add tier - no parent tab: " + parentTabId);
		}
		else if (!tab.TryGetSubTab(parentSubTabId, out subTab))
		{
			Log.Store.PrintError("Cannot add tier - no parent sub tab: " + parentSubTabId);
		}
		else
		{
			subTab.AddTier(tier);
		}
	}

	public bool TryGetTabData(string tabId, out ShopTabData tabData)
	{
		tabData = null;
		if (!TryGetTab(tabId, out var tab))
		{
			return false;
		}
		tabData = tab.TabData;
		return true;
	}

	public List<ShopTabData> GetAllTabData()
	{
		List<ShopTabData> allTabData = new List<ShopTabData>();
		foreach (CatalogTab tab in m_tabs.Values)
		{
			allTabData.Add(tab.TabData);
		}
		return allTabData;
	}

	public bool TryGetSubTabData(string parentTabId, string subTabId, out ShopTabData tabData)
	{
		tabData = null;
		if (!TryGetTab(parentTabId, out var tab))
		{
			return false;
		}
		if (!tab.TryGetSubTab(subTabId, out var subTab))
		{
			return false;
		}
		tabData = subTab.TabData;
		return true;
	}

	public bool TryGetAllSubTabData(string parentTabId, out List<ShopTabData> subTabs)
	{
		subTabs = new List<ShopTabData>();
		if (!TryGetTab(parentTabId, out var tab))
		{
			return false;
		}
		foreach (CatalogSubTab subTab in tab.GetAllSubTabs())
		{
			subTabs.Add(subTab.TabData);
		}
		return true;
	}

	public List<ProductTierDataModel> GetTiers()
	{
		List<ProductTierDataModel> tiers = new List<ProductTierDataModel>();
		foreach (CatalogTab value in m_tabs.Values)
		{
			foreach (CatalogSubTab subTab in value.GetAllSubTabs())
			{
				tiers.AddRange(subTab.GetTiers());
			}
		}
		return tiers;
	}

	public List<ProductTierDataModel> GetTiers(string tabId)
	{
		List<ProductTierDataModel> tiers = new List<ProductTierDataModel>();
		if (!TryGetTab(tabId, out var tab))
		{
			Log.Store.PrintError("Cannot get tiers - no tab: " + tabId);
			return tiers;
		}
		foreach (CatalogSubTab subTab in tab.GetAllSubTabs())
		{
			tiers.AddRange(subTab.GetTiers());
		}
		return tiers;
	}

	public List<ProductTierDataModel> GetTiers(string parentTabId, string subTabId)
	{
		List<ProductTierDataModel> tiers = new List<ProductTierDataModel>();
		if (!TryGetTab(parentTabId, out var tab))
		{
			Log.Store.PrintError("Cannot get tiers - no tab: " + parentTabId);
			return tiers;
		}
		if (!tab.TryGetSubTab(subTabId, out var subTab))
		{
			Log.Store.PrintError("Cannot get tiers - no sub tab: " + subTabId);
			return tiers;
		}
		tiers.AddRange(subTab.GetTiers());
		return tiers;
	}

	public bool HasTiers()
	{
		foreach (CatalogTab value in m_tabs.Values)
		{
			foreach (CatalogSubTab allSubTab in value.GetAllSubTabs())
			{
				if (allSubTab.GetTiers().Count > 0)
				{
					return true;
				}
			}
		}
		return false;
	}

	public void Clear()
	{
		m_tabs.Clear();
	}

	private bool TryGetTab(string tabId, out CatalogTab tab)
	{
		return m_tabs.TryGetValue(tabId, out tab);
	}
}
