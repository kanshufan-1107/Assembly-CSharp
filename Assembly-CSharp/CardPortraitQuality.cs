public class CardPortraitQuality
{
	public const int NOT_LOADED = 0;

	public const int LOW = 1;

	public const int MEDIUM = 2;

	public const int HIGH = 3;

	public int TextureQuality;

	public TAG_PREMIUM PremiumType;

	public CardPortraitQuality(int quality, TAG_PREMIUM premiumType)
	{
		TextureQuality = quality;
		PremiumType = premiumType;
	}

	public static CardPortraitQuality GetUnloaded()
	{
		return new CardPortraitQuality(0, TAG_PREMIUM.NORMAL);
	}

	public static CardPortraitQuality GetDefault()
	{
		return new CardPortraitQuality(3, TAG_PREMIUM.SIGNATURE);
	}

	public static CardPortraitQuality GetFromDef(CardDef def)
	{
		if (!(def == null))
		{
			return def.GetPortraitQuality();
		}
		return GetDefault();
	}

	public override string ToString()
	{
		return "(" + TextureQuality + ", " + PremiumType.ToString() + ")";
	}
}
