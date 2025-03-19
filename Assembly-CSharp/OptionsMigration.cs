using Blizzard.T5.Core;
using UnityEngine;

public static class OptionsMigration
{
	private delegate bool UpgradeCallback(int startingVersion);

	private static readonly Map<int, UpgradeCallback> s_clientUpgradeCallbacks = new Map<int, UpgradeCallback>
	{
		{ 2, UpgradeClientOptions_V3 },
		{ 3, UpgradeClientOptions_V4 }
	};

	private static readonly Map<int, UpgradeCallback> s_serverUpgradeCallbacks = new Map<int, UpgradeCallback>
	{
		{ 2, UpgradeServerOptions_V3 },
		{ 3, UpgradeServerOptions_V4 },
		{ 4, UpgradeServerOptions_V5 },
		{ 5, UpgradeServerOptions_V6 }
	};

	public static bool UpgradeClientOptions()
	{
		int optionsVersion = Options.Get().GetInt(Option.CLIENT_OPTIONS_VERSION);
		int startingVersion = optionsVersion;
		if (!Options.Get().HasOption(Option.CLIENT_OPTIONS_VERSION))
		{
			if (!UpgradeClientOptions_V2())
			{
				return false;
			}
			optionsVersion = 2;
		}
		for (; optionsVersion < 4; optionsVersion++)
		{
			if (!s_clientUpgradeCallbacks.TryGetValue(optionsVersion, out var callback))
			{
				Error.AddDevFatal("OptionsMigration.UpgradeClientOptions() - Current version is {0} and there is no function to upgrade to {1}. Latest is {2}.", optionsVersion, optionsVersion + 1, 4);
				return false;
			}
			if (!callback(startingVersion))
			{
				return false;
			}
		}
		return true;
	}

	private static bool UpgradeClientOptions_V2()
	{
		Options.Get().SetInt(Option.CLIENT_OPTIONS_VERSION, 2);
		return Options.Get().GetInt(Option.CLIENT_OPTIONS_VERSION) == 2;
	}

	private static bool UpgradeClientOptions_V3(int startingVersion)
	{
		Options.Get().SetInt(Option.CLIENT_OPTIONS_VERSION, 3);
		float newMusicVolume = Mathf.Clamp(Options.Get().GetFloat(Option.MUSIC_VOLUME), 0f, 0.5f);
		newMusicVolume /= 0.5f;
		Options.Get().SetFloat(Option.MUSIC_VOLUME, newMusicVolume);
		return Options.Get().GetInt(Option.CLIENT_OPTIONS_VERSION) == 3;
	}

	private static bool UpgradeClientOptions_V4(int startingVersion)
	{
		Options.Get().SetInt(Option.CLIENT_OPTIONS_VERSION, 4);
		float newMasterVolume = Mathf.Pow(Options.Get().GetFloat(Option.SOUND_VOLUME), 0.5714286f);
		Options.Get().SetFloat(Option.SOUND_VOLUME, newMasterVolume);
		return Options.Get().GetInt(Option.CLIENT_OPTIONS_VERSION) == 4;
	}

	public static bool UpgradeServerOptions()
	{
		int optionsVersion = Options.Get().GetInt(Option.SERVER_OPTIONS_VERSION);
		int startingVersion = optionsVersion;
		if (!Options.Get().HasOption(Option.SERVER_OPTIONS_VERSION))
		{
			if (!UpgradeServerOptions_V2())
			{
				return false;
			}
			optionsVersion = 2;
		}
		for (; optionsVersion < 6; optionsVersion++)
		{
			if (!s_serverUpgradeCallbacks.TryGetValue(optionsVersion, out var callback))
			{
				Error.AddDevFatal("OptionsMigration.UpgradeServerOptions() - Current version is {0} and there is no function to upgrade to {1}. Latest is {2}.", optionsVersion, optionsVersion + 1, 6);
				return false;
			}
			if (!callback(startingVersion))
			{
				return false;
			}
		}
		return true;
	}

	private static bool UpgradeServerOptions_V2()
	{
		Options.Get().SetInt(Option.SERVER_OPTIONS_VERSION, 2);
		return Options.Get().GetInt(Option.SERVER_OPTIONS_VERSION) == 2;
	}

	private static bool UpgradeServerOptions_V3(int startingVersion)
	{
		if (startingVersion != 0)
		{
			bool hasUnlockedAllHeroes = false;
			if (AchieveManager.Get() != null && AchieveManager.Get().IsReady())
			{
				hasUnlockedAllHeroes = GameModeUtils.HasUnlockedAllDefaultHeroes();
			}
			else if (Options.Get().GetBool(Option.HAS_SEEN_EXPERT_AI_UNLOCK))
			{
				hasUnlockedAllHeroes = true;
			}
			if (hasUnlockedAllHeroes)
			{
				Options.Get().SetBool(Option.HAS_SEEN_UNLOCK_ALL_HEROES_TRANSITION, val: true);
				Options.Get().SetBool(Option.HAS_CLICKED_COLLECTION_BUTTON_FOR_NEW_CARD, val: true);
				Options.Get().SetBool(Option.HAS_CLICKED_COLLECTION_BUTTON_FOR_NEW_DECK, val: true);
			}
			if (Options.Get().GetBool(Option.HAS_STARTED_A_DECK))
			{
				Options.Get().SetBool(Option.HAS_REMOVED_CARD_FROM_DECK, val: true);
			}
		}
		Options.Get().SetInt(Option.SERVER_OPTIONS_VERSION, 3);
		return Options.Get().GetInt(Option.SERVER_OPTIONS_VERSION) == 3;
	}

	private static bool UpgradeServerOptions_V4(int startingVersion)
	{
		if (SetRotationManager.HasSeenStandardModeTutorial())
		{
			Options.Get().SetBool(Option.HAS_SEEN_SET_FILTER_TUTORIAL, val: true);
		}
		Options.Get().SetInt(Option.SERVER_OPTIONS_VERSION, 4);
		return Options.Get().GetInt(Option.SERVER_OPTIONS_VERSION) == 4;
	}

	private static bool UpgradeServerOptions_V5(int startingVersion)
	{
		Options.Get().DeleteOption(Option.WHIZBANG_POPUP_COUNTER);
		Options.Get().SetInt(Option.SERVER_OPTIONS_VERSION, 5);
		return Options.Get().GetInt(Option.SERVER_OPTIONS_VERSION) == 5;
	}

	private static bool UpgradeServerOptions_V6(int startingVersion)
	{
		Options.Get().DeleteOption(Option.FORMAT_TYPE);
		Options.Get().SetInt(Option.SERVER_OPTIONS_VERSION, 6);
		return Options.Get().GetInt(Option.SERVER_OPTIONS_VERSION) == 6;
	}
}
