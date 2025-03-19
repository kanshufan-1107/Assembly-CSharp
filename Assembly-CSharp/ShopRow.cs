using System.Collections.Generic;
using Hearthstone.DataModels;
using Hearthstone.UI;

public class ShopRow
{
	public List<ShopBrowserButtonDataModel> BrowserButtons { get; } = new List<ShopBrowserButtonDataModel>();

	public bool IsEnabled { get; set; }

	public void SetData(DataModelList<ShopBrowserButtonDataModel> browserButtons, int maxItems, ref int buttonIndex)
	{
		IsEnabled = false;
		if (browserButtons == null)
		{
			return;
		}
		int i = 0;
		while (i < maxItems && buttonIndex < browserButtons.Count)
		{
			ShopBrowserButtonDataModel button = browserButtons[buttonIndex];
			BrowserButtons.Add(button);
			if (ShopUtils.ShouldDisplayButton(button))
			{
				IsEnabled = true;
			}
			i++;
			buttonIndex++;
		}
	}
}
