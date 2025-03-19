using System.Collections.Generic;
using com.blizzard.commerce.Model;

public class CatalogNetworkPages
{
	private readonly Dictionary<string, CatalogNetworkPage> m_pages = new Dictionary<string, CatalogNetworkPage>();

	private Dictionary<string, Placement> m_placements = new Dictionary<string, Placement>();

	public IEnumerable<CatalogNetworkPage> Pages => m_pages.Values;

	public CatalogNetworkPage GetOrCreatePage(Placement placement)
	{
		string placementId = placement.placementId;
		if (!m_placements.TryGetValue(placementId, out var result))
		{
			CatalogNetworkPage page = new CatalogNetworkPage();
			m_pages[placement.page.pageId] = page;
			m_placements[placementId] = placement;
			return page;
		}
		return m_pages[result.page.pageId];
	}

	public bool Contains(string pageId)
	{
		return m_pages.ContainsKey(pageId);
	}
}
