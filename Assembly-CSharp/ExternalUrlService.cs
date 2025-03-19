using System;
using System.Collections.Generic;
using Assets;
using Blizzard.GameService.SDK.Client.Integration;
using Blizzard.T5.Jobs;
using Blizzard.T5.Services;
using Hearthstone;
using Hearthstone.Devices;

public class ExternalUrlService : IService
{
	private const string DefaultExternalUrl = "https://www.blizzard.com/";

	private const string DefaultExternalCNUrl = "https://www.blizzardgames.cn/";

	public IEnumerator<IAsyncJobResult> Initialize(ServiceLocator serviceLocator)
	{
		yield break;
	}

	public Type[] GetDependencies()
	{
		return new Type[1] { typeof(GameDbf) };
	}

	public void Shutdown()
	{
	}

	public static ExternalUrlService Get()
	{
		return ServiceManager.Get<ExternalUrlService>();
	}

	public string GetBreakingNewsLink()
	{
		return BuildUrl(ExternalUrl.Endpoint.ALERT, Localization.GetBnetLocaleName().ToLower());
	}

	public string GetPrivacyPolicyLink()
	{
		return BuildUrl(ExternalUrl.Endpoint.PRIVACY_POLICY);
	}

	public string GetDataManagementLink(string ssoToken)
	{
		return BuildUrl(ExternalUrl.Endpoint.DATA_MANAGEMENT, ssoToken);
	}

	public string GetSystemRequirementsLink()
	{
		return BuildUrl(ExternalUrl.Endpoint.SYSTEM_REQUIREMENTS);
	}

	public string GetRecruitAFriendLink()
	{
		return BuildUrl(ExternalUrl.Endpoint.RECRUIT_A_FRIEND);
	}

	public string GetTermsOfSaleLink()
	{
		return BuildUrl(ExternalUrl.Endpoint.TERMS_OF_SALES);
	}

	public string GetCVVLink()
	{
		return BuildUrl(ExternalUrl.Endpoint.CVV);
	}

	public string GetResetPasswordLink()
	{
		return BuildUrl(ExternalUrl.Endpoint.PASSWORD_RESET);
	}

	public string GetDuplicatePurchaseErrorLink()
	{
		return BuildUrl(ExternalUrl.Endpoint.DUPLICATE_PURCHASE_ERROR);
	}

	public string GetPaymentInfoLink()
	{
		return BuildUrl(ExternalUrl.Endpoint.PAYMENT_INFO);
	}

	public string GetGenericPurchaseErrorLink()
	{
		return BuildUrl(ExternalUrl.Endpoint.GENERIC_PURCHASE_ERROR);
	}

	public string GetAddPaymentLink()
	{
		return BuildUrl(ExternalUrl.Endpoint.ADD_PAYMENT);
	}

	public string GetMobileGameServerConnectionLink()
	{
		return BuildUrl(ExternalUrl.Endpoint.MOBILE_GAME_SERVER_CONNECTION);
	}

	public string GetChinaRatingsWebsiteLink()
	{
		return BuildUrl(ExternalUrl.Endpoint.CHINA_RATINGS_WEBSITE);
	}

	public string GetAccountDeletionLink()
	{
		return BuildUrl(ExternalUrl.Endpoint.ACCOUNT_DELETION);
	}

	public string GetSoftAccountDeletionLink(string token)
	{
		return BuildUrl(ExternalUrl.Endpoint.ACCOUNT_DELETION_SOFT_ACCOUNT, token);
	}

	public string GetPersonalizedShopRulesLink()
	{
		return BuildUrl(ExternalUrl.Endpoint.PERSONALIZED_SHOP_OFFER_RULES);
	}

	public string GetRandomNamesText()
	{
		return BuildUrl(ExternalUrl.Endpoint.RANDOM_NAMES_TXT);
	}

	public string GetDoNotSellMoreInfo()
	{
		return BuildUrl(ExternalUrl.Endpoint.DO_NOT_SELL_MORE_INFO);
	}

	public string GetPersonalInfoLink()
	{
		return BuildUrl(ExternalUrl.Endpoint.CN_PERSONAL_INFO);
	}

	public string GetSdkInfoLink()
	{
		return BuildUrl(ExternalUrl.Endpoint.CN_SDK_INFO);
	}

	public string GetRegistrationInfoLink()
	{
		return BuildUrl(ExternalUrl.Endpoint.CN_REGISTRATION_INFO);
	}

	public string GetBlizzardLicenseTerms()
	{
		return BuildUrl(ExternalUrl.Endpoint.BLIZZARD_LICENSE_TERMS);
	}

	public string GetAppleLicenseTerms()
	{
		return BuildUrl(ExternalUrl.Endpoint.APPLE_LICENSES_TERMS);
	}

	public string GetGoogleLicenseTerms()
	{
		return BuildUrl(ExternalUrl.Endpoint.GOOGLE_LICENSES_TERMS);
	}

	private static string BuildUrl(ExternalUrl.Endpoint endpoint, params string[] args)
	{
		string regionStr = GetRegionString();
		bool num = HearthstoneApplication.GetMobileEnvironment() == MobileEnv.DEVELOPMENT || HearthstoneApplication.IsInternal();
		ExternalUrlDbfRecord externalUrlRecord = null;
		if (num)
		{
			externalUrlRecord = GameDbf.ExternalUrl.GetRecord((ExternalUrlDbfRecord dbf) => dbf.AssetFlags == ExternalUrl.AssetFlags.DEV_ONLY && dbf.Endpoint == endpoint);
		}
		if (externalUrlRecord == null)
		{
			externalUrlRecord = GameDbf.ExternalUrl.GetRecord((ExternalUrlDbfRecord dbf) => dbf.Endpoint == endpoint);
		}
		if (externalUrlRecord == null)
		{
			Log.BattleNet.PrintError("No external URL found for endpoint {0}", endpoint.ToString());
			if (regionStr == "CN")
			{
				return "https://www.blizzardgames.cn/";
			}
			return "https://www.blizzard.com/";
		}
		RegionOverridesDbfRecord regionOverride = externalUrlRecord.RegionOverrides.Find((RegionOverridesDbfRecord x) => x.Region == regionStr);
		string unformattedUrl = ((regionOverride == null) ? externalUrlRecord.GlobalUrl : regionOverride.OverrideUrl);
		try
		{
			string formattedUrl = string.Format(unformattedUrl, args);
			Log.BattleNet.PrintDebug("Url for endpoint {0}: {1}", endpoint.ToString(), formattedUrl);
			return formattedUrl;
		}
		catch (Exception ex)
		{
			Log.BattleNet.PrintError(ex.ToString());
			Log.BattleNet.PrintError("Url for endpoint {0} could not be formatted, using unformatted URL instead: {1}", endpoint.ToString(), unformattedUrl);
			return unformattedUrl;
		}
	}

	public static string GetRegionString()
	{
		return (PlatformSettings.IsMobile() ? DeviceLocaleHelper.GetCurrentRegionId() : BattleNet.GetAccountRegion()) switch
		{
			BnetRegion.REGION_US => "US", 
			BnetRegion.REGION_EU => "EU", 
			BnetRegion.REGION_KR => "KR", 
			BnetRegion.REGION_CN => "CN", 
			_ => "US", 
		};
	}
}
