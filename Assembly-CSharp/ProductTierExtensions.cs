using System;
using System.Collections.Generic;
using Hearthstone.DataModels;
using UnityEngine;

public static class ProductTierExtensions
{
	public static string GetDebugName(this ProductTierDataModel productTierDataModel)
	{
		string headerName = "Unknown";
		string layoutMapFlat = "Unknown";
		if (productTierDataModel != null)
		{
			if (!string.IsNullOrEmpty(productTierDataModel.Header))
			{
				headerName = productTierDataModel.Header;
			}
			if (productTierDataModel.LayoutMap != null)
			{
				layoutMapFlat = string.Join(',', productTierDataModel.LayoutMap);
			}
		}
		return $"Section: {headerName}, Size: {productTierDataModel.LayoutWidth}x{productTierDataModel.LayoutHeight}, Map: {layoutMapFlat}";
	}

	public static List<Tuple<int, int>> GetSlots(this ProductTierDataModel productTierDataModel)
	{
		if (productTierDataModel == null)
		{
			Log.Store.PrintError("Unable to update buttons for a null product tier");
			return null;
		}
		string tierName = productTierDataModel.GetDebugName();
		List<Tuple<int, int>> slots = new List<Tuple<int, int>>();
		foreach (string layout in productTierDataModel.LayoutMap)
		{
			string[] layoutSizeStr = layout.ToLower().Split("x");
			if (layoutSizeStr.Length != 2)
			{
				Log.Store.PrintError("Cannot parse shop slot layout " + layout + " in tier " + tierName);
				return null;
			}
			string widthStr = layoutSizeStr[0];
			if (!int.TryParse(widthStr, out var width))
			{
				Log.Store.PrintError("Cannot parse slot layout " + layout + " in tier " + tierName + " with width " + widthStr);
				return null;
			}
			if (width <= 0)
			{
				Log.Store.PrintError("Cannot use zero or negative width from slot layout " + layout + " in tier " + tierName);
				return null;
			}
			string heightStr = layoutSizeStr[1];
			if (!int.TryParse(heightStr, out var height))
			{
				Log.Store.PrintError("Cannot parse slot layout " + layout + " in tier " + tierName + " with height " + heightStr);
				return null;
			}
			if (height <= 0)
			{
				Log.Store.PrintError("Cannot use zero or negative height from slot layout " + layout + " in tier " + tierName);
				return null;
			}
			slots.Add(new Tuple<int, int>(width, height));
		}
		return slots;
	}

	public static LayoutMapping GetLayoutMapping(this ProductTierDataModel productTierDataModel, List<Tuple<int, int>> slots, int totalNumberOfProducts)
	{
		if (productTierDataModel == null)
		{
			Log.Store.PrintError("Unable to update buttons for a null product tier");
			return null;
		}
		int targetLayoutCount = Mathf.CeilToInt((float)totalNumberOfProducts / (float)slots.Count);
		targetLayoutCount = Mathf.Clamp(targetLayoutCount, 0, productTierDataModel.MaxLayoutCount);
		return new LayoutMapping(productTierDataModel.GetDebugName(), productTierDataModel.LayoutWidth, productTierDataModel.LayoutHeight, slots, targetLayoutCount);
	}
}
