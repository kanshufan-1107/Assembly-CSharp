using System.Collections.Generic;

namespace Hearthstone.Store;

public interface IProductAvailabilityApi
{
	bool IgnoreProductTiming { get; }

	Dictionary<int, Network.ShopSale> Sales { get; }
}
