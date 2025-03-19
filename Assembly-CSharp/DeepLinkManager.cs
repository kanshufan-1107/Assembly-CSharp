using System;
using System.Collections;
using System.Collections.Generic;
using Assets;
using Blizzard.T5.Configuration;
using Blizzard.T5.Core.Utils;
using Blizzard.T5.Services;
using Hearthstone;
using Hearthstone.Core;
using Hearthstone.Core.Deeplinking;
using Hearthstone.Core.Streaming;
using Hearthstone.InGameMessage;
using Hearthstone.Progression;
using Hearthstone.Store;
using Hearthstone.Streaming;
using PegasusShared;
using PegasusUtil;
using UnityEngine;

public class DeepLinkManager
{
	public enum DeepLinkSource
	{
		NONE,
		PUSH_NOTIFICATION,
		COMMAND_LINE_ARGUMENTS,
		INNKEEPERS_SPECIAL,
		IN_GAME_MESSAGE,
		QUEST,
		LOCKED_HERO_TOOLTIP,
		RAILROAD_MANAGER,
		TAVERN_GUIDE,
		DECK_PICKER
	}

	private enum CommandLineVerbs
	{
		NONE,
		OPEN_MODE,
		RUN_CHEATS
	}

	private class DeeplinkListener : IDeeplinkCallback
	{
		void IDeeplinkCallback.ProcessDeeplink(string url)
		{
			TryExecuteDeepLink(url, fromUnpause: true, 0);
		}
	}

	private static Dictionary<string, SceneMgr.Mode> modeMapping = new Dictionary<string, SceneMgr.Mode>
	{
		{
			"hub",
			SceneMgr.Mode.HUB
		},
		{
			"play",
			SceneMgr.Mode.TOURNAMENT
		},
		{
			"ranked",
			SceneMgr.Mode.TOURNAMENT
		},
		{
			"adventure",
			SceneMgr.Mode.ADVENTURE
		},
		{
			"arena",
			SceneMgr.Mode.DRAFT
		},
		{
			"tb",
			SceneMgr.Mode.TAVERN_BRAWL
		},
		{
			"tavernbrawl",
			SceneMgr.Mode.TAVERN_BRAWL
		},
		{
			"packopening",
			SceneMgr.Mode.PACKOPENING
		},
		{
			"cm",
			SceneMgr.Mode.COLLECTIONMANAGER
		},
		{
			"collectionmanager",
			SceneMgr.Mode.COLLECTIONMANAGER
		},
		{
			"credits",
			SceneMgr.Mode.CREDITS
		},
		{
			"store",
			SceneMgr.Mode.HUB
		},
		{
			"raf",
			SceneMgr.Mode.HUB
		},
		{
			"recruitafriend",
			SceneMgr.Mode.HUB
		},
		{
			"battlegrounds",
			SceneMgr.Mode.BACON
		},
		{
			"gamemode",
			SceneMgr.Mode.GAME_MODE
		},
		{
			"lettuce",
			SceneMgr.Mode.LETTUCE_VILLAGE
		},
		{
			"mercstore",
			SceneMgr.Mode.LETTUCE_VILLAGE
		},
		{
			"journal",
			SceneMgr.Mode.HUB
		},
		{
			"practice",
			SceneMgr.Mode.ADVENTURE
		}
	};

	private static bool s_hasProcessedStartupDeepLink;

	private static bool s_hasStartedListening;

	private static string s_lastProcessedDeeplink;

	private static DeeplinkListener s_deeplinkListener = new DeeplinkListener();

	public static void StartListeningForDeepLinks()
	{
		if (!s_hasStartedListening)
		{
			Log.DeepLink.PrintDebug("[DeepLinkManager] Starting listening for application deeplinks");
			s_hasStartedListening = true;
			DeeplinkService.Get().RegisterDeeplinkHandler("hearthstone", s_deeplinkListener);
			string currentDeeplink = DeeplinkService.Get().GetCurrentDeeplinkUrl();
			if (!string.IsNullOrEmpty(currentDeeplink) && currentDeeplink.StartsWith("hearthstone://", StringComparison.OrdinalIgnoreCase) && currentDeeplink != s_lastProcessedDeeplink)
			{
				TryExecuteDeepLink(currentDeeplink, fromUnpause: true, 0);
			}
		}
	}

	public static void StopListeningForDeepLinks()
	{
		if (s_hasStartedListening)
		{
			Log.DeepLink.PrintDebug("[DeepLinkManager] Stopped listening for application deeplinks");
			s_hasStartedListening = false;
			DeeplinkService.Get().UnregisterDeeplinkHandler("hearthstone", s_deeplinkListener);
		}
	}

	public static void TryExecuteStartupDeepLinkPostLogin()
	{
		if (!s_hasProcessedStartupDeepLink)
		{
			Log.DeepLink.PrintDebug("[DeepLinkManager] Executing launch deeplink on post-login");
			TryExecuteDeepLink(DeeplinkService.Get().GetStartupDeeplinkUrl(), fromUnpause: false, 0);
			s_hasProcessedStartupDeepLink = true;
		}
	}

	private static void TryExecuteDeepLink(string deeplinkUrl, bool fromUnpause, int questId)
	{
		Log.DeepLink.PrintDebug("[DeepLinkManager] Executing deeplink '" + deeplinkUrl + "'");
		s_lastProcessedDeeplink = deeplinkUrl;
		deeplinkUrl = deeplinkUrl ?? string.Empty;
		DeepLinkSource source = DeepLinkSource.NONE;
		string[] deepLinkArgs = null;
		string[] cheatsArgs = null;
		if (string.IsNullOrEmpty(deeplinkUrl) || !deeplinkUrl.StartsWith("hearthstone://", StringComparison.OrdinalIgnoreCase))
		{
			Log.DeepLink.PrintInfo("DeepLinkManager didn't execute start up deeplinks as it wasn't valid. Value=" + deeplinkUrl);
		}
		else
		{
			int queryIndex = deeplinkUrl.LastIndexOf('?');
			if (queryIndex > 0)
			{
				deeplinkUrl = deeplinkUrl.Substring(0, queryIndex);
			}
			deepLinkArgs = deeplinkUrl.Substring("hearthstone://".Length).Split('/');
		}
		if (deepLinkArgs != null && deepLinkArgs.Length != 0 && deepLinkArgs[0] != string.Empty)
		{
			source = DeepLinkSource.PUSH_NOTIFICATION;
		}
		else
		{
			string[] commandLineArgs = HearthstoneApplication.CommandLineArgs;
			int startIndex = -1;
			for (int i = 0; i < commandLineArgs.Length; i++)
			{
				string text = commandLineArgs[i];
				CommandLineVerbs cmd = ((text == "--mode") ? CommandLineVerbs.OPEN_MODE : ((text == "--runcheats") ? CommandLineVerbs.RUN_CHEATS : CommandLineVerbs.NONE));
				if (cmd != 0)
				{
					startIndex = ++i;
					for (; i < commandLineArgs.Length && !commandLineArgs[i].StartsWith("-"); i++)
					{
					}
					string[] args = commandLineArgs.Slice(startIndex, i);
					switch (cmd)
					{
					case CommandLineVerbs.OPEN_MODE:
						deepLinkArgs = args;
						source = DeepLinkSource.COMMAND_LINE_ARGUMENTS;
						break;
					case CommandLineVerbs.RUN_CHEATS:
						cheatsArgs = args;
						break;
					}
				}
			}
		}
		string dlStr = ((deepLinkArgs != null) ? string.Join(" ", deepLinkArgs) : "null");
		Log.DeepLink.PrintDebug(string.Format("[{0}] Trying to execute deeplink '{1}' from source '{2}' (unpause:{3})", "DeepLinkManager", dlStr, source, fromUnpause));
		if (SceneMgr.Get() == null || SetRotationManager.Get() == null)
		{
			Log.DeepLink.PrintError("[DeepLinkManager] Ignoring a deeplink that executed too early (Some systems are not readyt). " + $"SceneManager ready: {SceneMgr.Get() != null}, " + $"SetRotationManager ready: {SetRotationManager.Get() != null}, ");
			return;
		}
		if (deepLinkArgs == null || deepLinkArgs.Length == 0)
		{
			if (!fromUnpause && SceneMgr.Get().GetMode() == SceneMgr.Mode.LOGIN)
			{
				SceneMgr.Get().SetNextMode(SceneMgr.Mode.HUB);
			}
		}
		else if (SetRotationManager.Get().ShouldShowSetRotationIntro())
		{
			SceneMgr.Get().SetNextMode(SceneMgr.Mode.HUB);
			TelemetryManager.Client().SendDeepLinkExecuted(string.Join(" ", deepLinkArgs), source.ToString(), completed: false, questId);
		}
		else if (!InternalExecuteDeepLink(deepLinkArgs, source, fromUnpause, questId))
		{
			Log.DeepLink.PrintDebug("[DeepLinkManager] Failed to execute deeplink with args '" + dlStr + "', going to the hub instead");
			if (!fromUnpause && SceneMgr.Get().GetMode() == SceneMgr.Mode.LOGIN)
			{
				SceneMgr.Get().SetNextMode(SceneMgr.Mode.HUB);
			}
			TelemetryManager.Client().SendDeepLinkExecuted(string.Join(" ", deepLinkArgs), source.ToString(), completed: false, questId);
		}
		ExecuteCheats(cheatsArgs);
	}

	public static bool TryParseUri(string uri, out string[] deepLinkArgs)
	{
		if (!uri.StartsWith("hearthstone://"))
		{
			deepLinkArgs = null;
			return false;
		}
		deepLinkArgs = uri.Substring("hearthstone://".Length).Split('/');
		return true;
	}

	public static bool ExecuteDeepLink(string[] deepLink, DeepLinkSource source, int questId)
	{
		return InternalExecuteDeepLink(deepLink, source, fromUnpause: false, questId);
	}

	private static bool InternalExecuteDeepLink(string[] deepLink, DeepLinkSource source, bool fromUnpause, int questId)
	{
		if (deepLink == null || deepLink.Length == 0)
		{
			Log.DeepLink.PrintDebug("[DeepLinkManager] Skipping deeplink execution - no args were extracted from deeplink.");
			return false;
		}
		for (int i = 0; i < deepLink.Length; i++)
		{
			if (!string.IsNullOrEmpty(deepLink[i]))
			{
				deepLink[i] = deepLink[i].TrimEnd();
			}
		}
		Action modeDelegate = null;
		string mode = Vars.Key("Debug.OpenMode").GetStr(string.Empty);
		if (string.IsNullOrEmpty(mode) || s_hasProcessedStartupDeepLink)
		{
			mode = deepLink[0];
		}
		switch (mode)
		{
		case "hub":
		case "play":
		case "packopening":
		case "credits":
			modeDelegate = ShowSceneMode(deepLink, source, questId);
			break;
		case "cm":
		case "collectionmanager":
			modeDelegate = ShowCollectionManager(deepLink, source, questId);
			break;
		case "ranked":
			modeDelegate = ShowRankedMode(deepLink, source, questId);
			break;
		case "journal":
			modeDelegate = ShowJournal(deepLink, source, questId);
			break;
		case "arena":
			modeDelegate = ShowArena(deepLink, source, questId);
			break;
		case "adventure":
			modeDelegate = ShowAdventure(deepLink, source, questId);
			break;
		case "tb":
		case "tavernbrawl":
			modeDelegate = ShowTavernBrawl(deepLink, source, questId);
			break;
		case "store":
			modeDelegate = ShowStore(deepLink, source, questId);
			break;
		case "raf":
		case "recruitafriend":
			modeDelegate = ShowRecruitAFriend(deepLink, source, questId);
			break;
		case "battlegrounds":
			modeDelegate = ShowBattlegrounds(deepLink, source, questId);
			break;
		case "gamemode":
			modeDelegate = ShowGameMode(deepLink, source, questId);
			break;
		case "lettuce":
		case "mercenaries":
			modeDelegate = ShowLettuce(deepLink, source, questId);
			break;
		case "mercstore":
			modeDelegate = ShowMercstore(deepLink, source, questId);
			break;
		case "practice":
			modeDelegate = ShowPracticeMode(deepLink, source, questId);
			break;
		default:
			return false;
		}
		if (modeDelegate != null)
		{
			GoToMode(modeDelegate, fromUnpause);
		}
		return true;
	}

	private static void ExecuteCheats(string[] cheatsArgs)
	{
		string cheats = Vars.Key("Debug.RunCheats").GetStr(string.Empty);
		if (string.IsNullOrEmpty(cheats) && cheatsArgs != null)
		{
			cheats = string.Join(" ", cheatsArgs);
		}
		if (!string.IsNullOrEmpty(cheats))
		{
			Processor.RunCoroutine(RunCheatCommands(cheats.Split(';')));
		}
	}

	private static IEnumerator RunCheatCommands(string[] cheats)
	{
		foreach (string cheat in cheats)
		{
			CheatMgr.Get()?.ProcessCheat(cheat);
			yield return new WaitForSecondsRealtime(0.5f);
		}
	}

	private static void GoToMode(Action modeDelegate, bool fromUnpause)
	{
		if (!fromUnpause && SceneMgr.Get().GetMode() == SceneMgr.Mode.LOGIN)
		{
			SceneMgr.Get().SetNextMode(SceneMgr.Mode.HUB);
			SceneMgr.Get().RegisterSceneLoadedEvent(OnSceneLoaded, modeDelegate);
		}
		else if (!SceneMgr.Get().IsTransitioning())
		{
			modeDelegate();
		}
		else
		{
			SceneMgr.Get().RegisterSceneLoadedEvent(OnSceneLoaded, modeDelegate);
		}
	}

	private static Action ShowSceneMode(string[] deepLink, DeepLinkSource source, int questId)
	{
		return delegate
		{
			SceneMgr.Get().SetNextMode(modeMapping[deepLink[0]]);
			TelemetryManager.Client().SendDeepLinkExecuted(string.Join(" ", deepLink), source.ToString(), completed: true, questId);
		};
	}

	private static Action ShowCollectionManager(string[] deepLink, DeepLinkSource source, int questId)
	{
		return delegate
		{
			SceneMgr.Get().SetNextMode(SceneMgr.Mode.COLLECTIONMANAGER);
			bool flag = false;
			if (deepLink.Length > 1)
			{
				string cardId = deepLink[1];
				if (deepLink.Length > 2 && deepLink[2].ToLowerInvariant() == "crafting")
				{
					flag = true;
				}
				CollectionManagerJobs.OpenToCardPageWhenReady(cardId, TAG_PREMIUM.NORMAL, flag, flag);
			}
			TelemetryManager.Client().SendDeepLinkExecuted(string.Join(" ", deepLink), source.ToString(), completed: true, questId);
		};
	}

	private static Action ShowRankedMode(string[] deepLink, DeepLinkSource source, int questId)
	{
		FormatType formatType = FormatType.FT_STANDARD;
		if (deepLink.Length > 1 && !string.IsNullOrEmpty(deepLink[1]) && !EnumUtils.TryGetEnum<FormatType>(deepLink[1], StringComparison.OrdinalIgnoreCase, out formatType))
		{
			return null;
		}
		return delegate
		{
			SceneMgr.Get().SetNextMode(SceneMgr.Mode.TOURNAMENT);
			Options.SetFormatType(formatType);
			Options.SetInRankedPlayMode(inRankedPlayMode: true);
			TelemetryManager.Client().SendDeepLinkExecuted(string.Join(" ", deepLink), source.ToString(), completed: true, questId);
		};
	}

	private static Action ShowPracticeMode(string[] deepLink, DeepLinkSource source, int questId)
	{
		return delegate
		{
			if (GameDownloadManagerProvider.Get().IsModuleReadyToPlay(DownloadTags.Content.Adventure))
			{
				SceneMgr.Get().SetNextMode(SceneMgr.Mode.ADVENTURE);
				AdventureConfig.Get().SetSelectedAdventureMode(AdventureDbId.PRACTICE, AdventureModeDbId.LINEAR);
				TelemetryManager.Client().SendDeepLinkExecuted(string.Join(" ", deepLink), source.ToString(), completed: true, questId);
			}
			else
			{
				TelemetryManager.Client().SendDeepLinkExecuted(string.Join(" ", deepLink), source.ToString(), completed: false, questId);
			}
		};
	}

	private static Action ShowJournal(string[] deepLink, DeepLinkSource source, int questId)
	{
		JournalTrayDisplay.JournalTab journalTab = JournalTrayDisplay.JournalTab.Unknown;
		if (deepLink.Length > 1 && !string.IsNullOrEmpty(deepLink[1]))
		{
			if (!EnumUtils.TryGetEnum<JournalTrayDisplay.JournalTab>(deepLink[1], StringComparison.OrdinalIgnoreCase, out journalTab))
			{
				return null;
			}
			return delegate
			{
				JournalButton journalButton = Box.Get().GetJournalButton();
				if (journalButton != null)
				{
					if (!JournalPopup.s_isShowing)
					{
						JournalTrayDisplay.SetActiveTabForTrackType(Global.RewardTrackType.GLOBAL, journalTab);
						journalButton.ShowJournal();
						TelemetryManager.Client().SendDeepLinkExecuted(string.Join(" ", deepLink), source.ToString(), completed: true, questId);
					}
					else
					{
						JournalTrayDisplay journalTrayDisplay = journalButton.GetJournalTrayDisplay();
						if (journalTrayDisplay != null)
						{
							journalTrayDisplay.ForceChangeActiveTabViaDeepLink(journalTab);
							TelemetryManager.Client().SendDeepLinkExecuted(string.Join(" ", deepLink), source.ToString(), completed: true, questId);
						}
					}
				}
			};
		}
		return null;
	}

	private static Action ShowStore(string[] deepLink, DeepLinkSource source, int questId)
	{
		long pmtProductId = 0L;
		if (deepLink.Length > 1)
		{
			long.TryParse(deepLink[1], out pmtProductId);
			if (deepLink.Length > 2 && deepLink[2].ToLowerInvariant() != "pmt")
			{
				StorePackId storePackId = default(StorePackId);
				GetBoosterAndStorePackTypeFromGameAction(deepLink, out storePackId.Id, out storePackId.Type);
				ProductType productType = StorePackId.GetProductTypeFromStorePackType(storePackId);
				int productData = GameUtils.GetProductDataFromStorePackId(storePackId);
				pmtProductId = ServiceManager.Get<IProductDataService>().GetDeeplinkProductId(productType, productData);
			}
		}
		return delegate
		{
			ProductPageJobs.OpenToProductPageWhenReady(pmtProductId);
			TelemetryManager.Client().SendDeepLinkExecuted(string.Join(" ", deepLink), source.ToString(), completed: true, questId);
		};
	}

	private static Action ShowAdventure(string[] deepLink, DeepLinkSource source, int questId)
	{
		AdventureDbId adventureId = AdventureDbId.INVALID;
		AdventureModeDbId adventureModeId = AdventureModeDbId.LINEAR;
		if (deepLink.Length > 1)
		{
			string adventureAction = deepLink[1];
			adventureId = EnumUtils.SafeParse(adventureAction, AdventureDbId.INVALID, ignoreCase: true);
			adventureModeId = AdventureModeDbId.LINEAR;
			if (deepLink.Length > 2)
			{
				string adventureModeAction = deepLink[2];
				adventureModeId = EnumUtils.SafeParse(adventureModeAction, AdventureModeDbId.LINEAR, ignoreCase: true);
			}
		}
		return delegate
		{
			if (GameDownloadManagerProvider.Get().IsModuleReadyToPlay(DownloadTags.Content.Adventure))
			{
				SceneMgr.Get().SetNextMode(SceneMgr.Mode.ADVENTURE);
				if (adventureId != 0)
				{
					AdventureConfig.Get().SetSelectedAdventureMode(adventureId, adventureModeId);
				}
				TelemetryManager.Client().SendDeepLinkExecuted(string.Join(" ", deepLink), source.ToString(), completed: true, questId);
			}
			else
			{
				TelemetryManager.Client().SendDeepLinkExecuted(string.Join(" ", deepLink), source.ToString(), completed: false, questId);
			}
		};
	}

	private static Action ShowTavernBrawl(string[] deepLink, DeepLinkSource source, int questId)
	{
		return delegate
		{
			if (!TavernBrawlManager.Get().HasUnlockedTavernBrawl(BrawlType.BRAWL_TYPE_TAVERN_BRAWL) || !TavernBrawlManager.Get().IsTavernBrawlActive(BrawlType.BRAWL_TYPE_TAVERN_BRAWL))
			{
				TelemetryManager.Client().SendDeepLinkExecuted(string.Join(" ", deepLink), source.ToString(), completed: false, questId);
			}
			else
			{
				SceneMgr.Get().SetNextMode(SceneMgr.Mode.TAVERN_BRAWL);
				TelemetryManager.Client().SendDeepLinkExecuted(string.Join(" ", deepLink), source.ToString(), completed: true, questId);
			}
		};
	}

	private static Action ShowArena(string[] deepLink, DeepLinkSource source, int questId)
	{
		return delegate
		{
			if (GameModeUtils.HasUnlockedMode(Global.UnlockableGameMode.ARENA) && HealthyGamingMgr.Get().isArenaEnabled())
			{
				AchieveManager.Get().NotifyOfClick(Achievement.ClickTriggerType.BUTTON_ARENA);
				SceneMgr.Get().SetNextMode(SceneMgr.Mode.DRAFT);
				TelemetryManager.Client().SendDeepLinkExecuted(string.Join(" ", deepLink), source.ToString(), completed: true, questId);
			}
			else
			{
				TelemetryManager.Client().SendDeepLinkExecuted(string.Join(" ", deepLink), source.ToString(), completed: false, questId);
			}
		};
	}

	private static Action ShowRecruitAFriend(string[] deepLink, DeepLinkSource source, int questId)
	{
		return delegate
		{
			TelemetryManager.Client().SendDeepLinkExecuted(string.Join(" ", deepLink), source.ToString(), completed: true, questId);
			RAFManager.Get().ShowRAFFrame();
		};
	}

	private static Action ShowBattlegrounds(string[] deepLink, DeepLinkSource source, int questId)
	{
		return delegate
		{
			if (GameDownloadManagerProvider.Get().IsModuleReadyToPlay(DownloadTags.Content.Bgs))
			{
				if (deepLink.Length > 1 && deepLink[1].ToLowerInvariant() == "duos")
				{
					BaconLobbyMgr.Get().SetBattlegroundsGameMode("duos");
				}
				TelemetryManager.Client().SendDeepLinkExecuted(string.Join(" ", deepLink), source.ToString(), completed: true, questId);
				SceneMgr.Get().SetNextMode(SceneMgr.Mode.BACON);
			}
			else
			{
				TelemetryManager.Client().SendDeepLinkExecuted(string.Join(" ", deepLink), source.ToString(), completed: false, questId);
			}
		};
	}

	private static Action ShowGameMode(string[] deepLink, DeepLinkSource source, int questId)
	{
		return delegate
		{
			TelemetryManager.Client().SendDeepLinkExecuted(string.Join(" ", deepLink), source.ToString(), completed: true, questId);
			SceneMgr.Get().SetNextMode(SceneMgr.Mode.GAME_MODE);
		};
	}

	private static Action ShowLettuce(string[] deepLink, DeepLinkSource source, int questId)
	{
		return delegate
		{
			if (GameDownloadManagerProvider.Get().IsModuleReadyToPlay(DownloadTags.Content.Merc))
			{
				TelemetryManager.Client().SendDeepLinkExecuted(string.Join(" ", deepLink), source.ToString(), completed: true, questId);
				SceneMgr.Get().SetNextMode(SceneMgr.Mode.LETTUCE_VILLAGE);
			}
			else
			{
				TelemetryManager.Client().SendDeepLinkExecuted(string.Join(" ", deepLink), source.ToString(), completed: false, questId);
			}
		};
	}

	private static Action ShowMercstore(string[] deepLink, DeepLinkSource source, int questId)
	{
		if (!MercenaryMessageUtils.HasCompletedMercenaryVillageShopTutorial() || !GameDownloadManagerProvider.Get().IsModuleReadyToPlay(DownloadTags.Content.Merc))
		{
			return ShowStore(deepLink, source, questId);
		}
		long pmtProductId = 0L;
		if (deepLink.Length > 1)
		{
			long.TryParse(deepLink[1], out pmtProductId);
			if (deepLink.Length > 2 && deepLink[2].ToLowerInvariant() != "pmt")
			{
				StorePackId storePackId = default(StorePackId);
				GetBoosterAndStorePackTypeFromGameAction(deepLink, out storePackId.Id, out storePackId.Type);
				ProductType productType = StorePackId.GetProductTypeFromStorePackType(storePackId);
				int productData = GameUtils.GetProductDataFromStorePackId(storePackId);
				pmtProductId = ServiceManager.Get<IProductDataService>().GetDeeplinkProductId(productType, productData);
			}
		}
		Action goToStoreDelegate = delegate
		{
			ProductPageJobs.OpenToProductPageWhenReady(pmtProductId);
			TelemetryManager.Client().SendDeepLinkExecuted(string.Join(" ", deepLink), source.ToString(), completed: true, questId);
		};
		if (SceneMgr.Get().GetMode() == SceneMgr.Mode.LETTUCE_VILLAGE)
		{
			return goToStoreDelegate;
		}
		return delegate
		{
			SceneMgr.Get().SetNextMode(SceneMgr.Mode.LETTUCE_VILLAGE);
			SceneMgr.Get().RegisterSceneLoadedEvent(OnMercVillageSceneLoaded, goToStoreDelegate);
		};
	}

	private static void OnSceneLoaded(SceneMgr.Mode mode, PegasusScene scene, object userData)
	{
		if (mode == SceneMgr.Mode.HUB)
		{
			((Action)userData)();
			SceneMgr.Get().UnregisterSceneLoadedEvent(OnSceneLoaded, userData);
		}
	}

	private static void OnMercVillageSceneLoaded(SceneMgr.Mode mode, PegasusScene scene, object userData)
	{
		if (mode == SceneMgr.Mode.LETTUCE_VILLAGE)
		{
			((Action)userData)();
			SceneMgr.Get().UnregisterSceneLoadedEvent(OnSceneLoaded, userData);
		}
	}

	public static void GetBoosterAndStorePackTypeFromGameAction(string[] actionTokens, out int boosterId, out StorePackType storePackType)
	{
		string storeAction = actionTokens[1];
		storePackType = StorePackType.BOOSTER;
		if (actionTokens.Length > 2)
		{
			storePackType = EnumUtils.SafeParse(actionTokens[2], StorePackType.BOOSTER, ignoreCase: true);
		}
		boosterId = (int)EnumUtils.SafeParse(storeAction, BoosterDbId.INVALID, ignoreCase: true);
	}
}
