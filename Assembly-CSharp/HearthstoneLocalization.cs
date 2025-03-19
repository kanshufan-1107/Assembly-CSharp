using Blizzard.GameService.SDK.Client.Integration;
using Blizzard.T5.Configuration;
using Blizzard.T5.Core.Utils;
using Hearthstone;
using Hearthstone.DataModels;
using Hearthstone.Devices;
using Hearthstone.UI;
using UnityEngine;

public class HearthstoneLocalization
{
	public static readonly PlatformDependentValue<bool> LOCALE_FROM_OPTIONS = new PlatformDependentValue<bool>(PlatformCategory.OS)
	{
		iOS = true,
		Android = true,
		PC = false,
		Mac = false
	};

	public static void Initialize()
	{
		Locale? locale = null;
		if ((bool)LOCALE_FROM_OPTIONS && EnumUtils.TryGetEnum<Locale>(Options.Get().GetString(Option.LOCALE), out var localeFromName))
		{
			locale = localeFromName;
		}
		if (!locale.HasValue)
		{
			if (PlatformSettings.IsMobileRuntimeOS)
			{
				locale = DeviceLocaleHelper.GetBestGuessForLocale();
			}
			else if (PlatformSettings.IsSteam)
			{
				locale = ((!Application.isEditor) ? new Locale?(DeviceLocaleHelper.GetBestGuessForLocale()) : ((!EnumUtils.TryGetEnum<Locale>(Vars.Key("Localization.Locale").GetStr(Localization.DEFAULT_LOCALE_NAME), out var localeFromName2)) ? new Locale?(Locale.enUS) : new Locale?(localeFromName2)));
			}
			else
			{
				string localeName = null;
				if (HearthstoneApplication.IsPublic() && !PlatformSettings.IsSteam)
				{
					localeName = BattleNet.GetLaunchOption("LOCALE", encrypted: false);
				}
				if (string.IsNullOrEmpty(localeName))
				{
					localeName = Vars.Key("Localization.Locale").GetStr(Localization.DEFAULT_LOCALE_NAME);
				}
				if (HearthstoneApplication.IsInternal())
				{
					string nonBNetLocale = Vars.Key("Localization.OverrideBnetLocale").GetStr("");
					if (!string.IsNullOrEmpty(nonBNetLocale))
					{
						localeName = nonBNetLocale;
					}
				}
				locale = ((!EnumUtils.TryGetEnum<Locale>(localeName, out var localeFromName3)) ? new Locale?(Locale.enUS) : new Locale?(localeFromName3));
			}
			if ((bool)LOCALE_FROM_OPTIONS)
			{
				Options.Get().SetString(Option.LOCALE, locale.ToString());
			}
		}
		Localization.RegisterSetLocaleDoneCallback(OnSetLocalDone);
		Localization.SetLocale(locale.Value);
	}

	private static void OnSetLocalDone(Locale locale)
	{
		DataContext dataContext = GlobalDataContext.Get();
		IDataModel dataModel = null;
		if (dataContext.GetDataModel(153, out dataModel))
		{
			(dataModel as AccountDataModel).Language = Localization.GetLocale();
		}
	}
}
