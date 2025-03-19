using System.Collections.Generic;
using Blizzard.Commerce;
using Blizzard.T5.Core.Utils;
using Hearthstone.DataModels;
using Hearthstone.Store;

namespace Hearthstone.Commerce;

public static class ProductIssues
{
	private static readonly Dictionary<ProductId, List<string>> s_errors = new Dictionary<ProductId, List<string>>();

	private static readonly Dictionary<ProductId, List<string>> s_hidden = new Dictionary<ProductId, List<string>>();

	public static void LogError(ProductId productId, string format, params object[] args)
	{
		string message = GeneralUtils.SafeFormat(format, args);
		string logMessage = $"Invalid product {productId}: {message}";
		Log.Store.PrintWarning(logMessage);
	}

	public static void LogHidden(ProductId productId, string format, params object[] args)
	{
		string message = GeneralUtils.SafeFormat(format, args);
		string logMessage = $"Hiding product {productId}: {message}";
		Log.Store.PrintInfo(logMessage);
	}

	public static void LogError(ProductInfo netBundle, string format, params object[] args)
	{
		LogError(netBundle.Id, format, args);
	}

	public static void LogError(ProductDataModel product, string format, params object[] args)
	{
		LogError(ProductId.CreateFrom(product.PmtId), format, args);
	}

	public static void LogHidden(ProductDataModel product, string format, params object[] args)
	{
		LogHidden(ProductId.CreateFrom(product.PmtId), format, args);
	}
}
