using PegasusUtil;

public struct StorePackId
{
	public StorePackType Type;

	public int Id;

	public override bool Equals(object obj)
	{
		if (((StorePackId)obj).Type == Type)
		{
			return ((StorePackId)obj).Id == Id;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return Type.GetHashCode() ^ Id;
	}

	public static ProductType GetProductTypeFromStorePackType(StorePackId storePackId)
	{
		if (storePackId.Type == StorePackType.BOOSTER)
		{
			if (!GameUtils.IsHiddenLicenseBundleBooster(storePackId))
			{
				return ProductType.PRODUCT_TYPE_BOOSTER;
			}
			return ProductType.PRODUCT_TYPE_HIDDEN_LICENSE;
		}
		return ProductType.PRODUCT_TYPE_UNKNOWN;
	}
}
