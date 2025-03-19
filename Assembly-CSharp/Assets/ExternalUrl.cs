using System;
using System.ComponentModel;
using Blizzard.T5.Core.Utils;

namespace Assets;

public static class ExternalUrl
{
	[Flags]
	public enum AssetFlags
	{
		[Description("none")]
		NONE = 0,
		[Description("dev_only")]
		DEV_ONLY = 1,
		[Description("not_packaged_in_client")]
		NOT_PACKAGED_IN_CLIENT = 2,
		[Description("force_do_not_localize")]
		FORCE_DO_NOT_LOCALIZE = 4
	}

	public enum Endpoint
	{
		ACCOUNT,
		ALERT,
		CREATION,
		LANDING,
		FIRESIDE_GATHERINGS,
		ACCOUNT_HEALUP,
		PRIVACY_POLICY,
		EULA,
		DATA_MANAGEMENT,
		SET_ROTATION,
		SYSTEM_REQUIREMENTS,
		RECRUIT_A_FRIEND,
		TERMS_OF_SALES,
		PURCHASE,
		ADD_PAYMENT,
		CVV,
		DUPLICATE_PURCHASE_ERROR,
		DUPLICATE_THIRDPARTY_PURCHASE,
		OUTSTANDING_PURCHASE,
		HEARTHSTONE_ON_IPAD,
		PASSWORD_RESET,
		CHECKOUT,
		CHECKOUT_NAVBAR,
		PAYMENT_INFO,
		GENERIC_PURCHASE_ERROR,
		CUSTOMER_SUPPORT,
		MOBILE_GAME_SERVER_CONNECTION,
		CHINA_RATINGS_WEBSITE,
		ACCOUNT_DELETION_SOFT_ACCOUNT,
		ACCOUNT_DELETION,
		PERSONALIZED_SHOP_OFFER_RULES,
		RANDOM_NAMES_TXT,
		DO_NOT_SELL_MORE_INFO,
		CN_PERSONAL_INFO,
		CN_SDK_INFO,
		CN_REGISTRATION_INFO,
		BLIZZARD_LICENSE_TERMS,
		APPLE_LICENSES_TERMS,
		GOOGLE_LICENSES_TERMS
	}

	public static AssetFlags ParseAssetFlagsValue(string value)
	{
		EnumUtils.TryGetEnum<AssetFlags>(value, out var e);
		return e;
	}

	public static Endpoint ParseEndpointValue(string value)
	{
		EnumUtils.TryGetEnum<Endpoint>(value, out var e);
		return e;
	}
}
