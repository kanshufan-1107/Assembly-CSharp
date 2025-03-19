using System.Collections.Generic;

public class CatalogNetworkPage
{
	public List<Network.ShopSection> Sections { get; } = new List<Network.ShopSection>();

	public Network.ShopSection GetSectionBySortOrder(uint sortOrder)
	{
		int i = 0;
		for (int iMax = Sections.Count; i < iMax; i++)
		{
			Network.ShopSection shopSection = Sections[i];
			if (shopSection.SortOrder == sortOrder)
			{
				return shopSection;
			}
		}
		return null;
	}

	public void AddSection(Network.ShopSection section)
	{
		Sections.Add(section);
	}

	public void Clear()
	{
		Sections.Clear();
	}
}
