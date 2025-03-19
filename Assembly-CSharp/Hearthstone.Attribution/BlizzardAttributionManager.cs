using System.Collections.Generic;
using System.Globalization;
using Blizzard.Telemetry;
using Blizzard.Telemetry.WTCG.Client;
using Hearthstone.Attribution.AppsFlyer;
using Hearthstone.Attribution.Util;
using Hearthstone.Core;
using HearthstoneTelemetry;
using PegasusShared;
using UnityEngine;

namespace Hearthstone.Attribution;

public sealed class BlizzardAttributionManager
{
	private static BlizzardAttributionManager s_instance;

	private HsFirebase m_firebase;

	private readonly ITelemetryClient m_telemetryClient;

	private readonly CachedServerFlag m_optOutFlag;

	private BlizzardAttributionManager()
	{
		m_telemetryClient = TelemetryManager.Client();
		m_optOutFlag = new CachedServerFlag(Options.Get(), Option.OPTOUT_DATA_SHARING, Option.OPTOUT_DATA_SHARING_CACHED);
	}

	public static BlizzardAttributionManager Get()
	{
		if (s_instance == null)
		{
			s_instance = new BlizzardAttributionManager();
		}
		return s_instance;
	}

	public void Initialize()
	{
		Log.AdTracking.PrintInfo("Initializing Attribution");
		HsAppsFlyer.Initialize(60, m_optOutFlag.Value);
		m_firebase = new HsFirebase(Log.AdTracking, m_optOutFlag.Value);
		if (m_firebase.IsEnabled)
		{
			Log.AdTracking.PrintInfo("Firebase is enabled for this platform");
		}
		else
		{
			Log.AdTracking.PrintInfo("Firebase is disabled for this platform");
		}
	}

	public void OnConnected()
	{
		m_optOutFlag.OnServerOptionAvailable();
	}

	public void OptOutOfDataSharing()
	{
		HsAppsFlyer.OptOutOfSharing();
		m_firebase.OptOutOfSharing();
		m_optOutFlag.Value = true;
		Log.AdTracking.PrintInfo("Opting out of data sharing");
	}

	public void SetCustomerUserId(string id)
	{
		HsAppsFlyer.SetCustomerUserId(id);
		m_firebase.SetCustomerUserId(id);
	}

	public void SendEvent_Install()
	{
		if (PlatformSettings.RuntimeOS == OSCategory.iOS)
		{
			Processor.RunCoroutine(AppleSearchAds.RequestAsync(OnIOSInstallCompleted));
			return;
		}
		if (PlatformSettings.RuntimeOS == OSCategory.Android)
		{
			Processor.RunCoroutine(InstallReferrer.RequestCoroutine(OnAndroidInstallCompleted));
			return;
		}
		SendEvent(new AttributionInstall
		{
			ApplicationId = TelemetryManager.GetApplicationId(),
			BundleId = Application.identifier,
			FirstInstallDate = AppLaunchTracker.FirstInstallTimeMilliseconds,
			DeviceType = SystemInfo.deviceModel
		});
	}

	public void SendEvent_Launch()
	{
		SendEvent(new AttributionLaunch
		{
			ApplicationId = TelemetryManager.GetApplicationId(),
			BundleId = Application.identifier,
			FirstInstallDate = AppLaunchTracker.FirstInstallTimeMilliseconds,
			DeviceType = SystemInfo.deviceModel,
			Counter = AppLaunchTracker.LaunchCount
		});
	}

	public void SendEvent_Login()
	{
		SendEvent(new AttributionLogin
		{
			ApplicationId = TelemetryManager.GetApplicationId(),
			BundleId = Application.identifier,
			FirstInstallDate = AppLaunchTracker.FirstInstallTimeMilliseconds,
			DeviceType = SystemInfo.deviceModel
		});
		SendEventToExternalTrackers("login", new Dictionary<string, string>());
	}

	public void SendEvent_FirstLogin()
	{
		SendEvent(new AttributionFirstLogin
		{
			ApplicationId = TelemetryManager.GetApplicationId(),
			BundleId = Application.identifier,
			FirstInstallDate = AppLaunchTracker.FirstInstallTimeMilliseconds,
			DeviceType = SystemInfo.deviceModel
		});
		SendEventToExternalTrackers("first_login", new Dictionary<string, string>());
	}

	public void SendEvent_Registration()
	{
		SendEvent(new AttributionRegistration
		{
			ApplicationId = TelemetryManager.GetApplicationId(),
			BundleId = Application.identifier,
			FirstInstallDate = AppLaunchTracker.FirstInstallTimeMilliseconds,
			DeviceType = SystemInfo.deviceModel
		});
		SendEventToExternalTrackers("registration", new Dictionary<string, string>());
	}

	public void SendEvent_HeadlessAccountCreated(Context messasgeContext = null)
	{
		SendEvent(new AttributionHeadlessAccountCreated
		{
			ApplicationId = TelemetryManager.GetApplicationId(),
			BundleId = Application.identifier,
			FirstInstallDate = AppLaunchTracker.FirstInstallTimeMilliseconds,
			DeviceType = SystemInfo.deviceModel
		}, messasgeContext);
		SendEventToExternalTrackers("headless_account_created", new Dictionary<string, string>());
	}

	public void SendEvent_HeadlessAccountHealedUp(string temporaryGameAccountId)
	{
		SendEvent(new AttributionHeadlessAccountHealedUp
		{
			ApplicationId = TelemetryManager.GetApplicationId(),
			BundleId = Application.identifier,
			FirstInstallDate = AppLaunchTracker.FirstInstallTimeMilliseconds,
			DeviceType = SystemInfo.deviceModel,
			TemporaryGameAccountId = temporaryGameAccountId
		});
		SendEventToExternalTrackers("headless_account_healed_up", new Dictionary<string, string> { { "temporary_game_account_id", temporaryGameAccountId } });
	}

	public void SendEvent_ContentUnlocked(string contentId)
	{
		SendEvent(new AttributionContentUnlocked
		{
			ApplicationId = TelemetryManager.GetApplicationId(),
			BundleId = Application.identifier,
			FirstInstallDate = AppLaunchTracker.FirstInstallTimeMilliseconds,
			DeviceType = SystemInfo.deviceModel,
			ContentId = contentId
		});
		SendEventToExternalTrackers("content_unlocked", new Dictionary<string, string> { { "content_id", contentId } });
	}

	public void SendEvent_ScenarioResult(int scenarioId, string result, int bossId)
	{
		SendEvent(new AttributionScenarioResult
		{
			ApplicationId = TelemetryManager.GetApplicationId(),
			BundleId = Application.identifier,
			FirstInstallDate = AppLaunchTracker.FirstInstallTimeMilliseconds,
			DeviceType = SystemInfo.deviceModel,
			ScenarioId = scenarioId,
			Result = result,
			BossId = bossId
		});
		SendEventToExternalTrackers("scenario_result", new Dictionary<string, string>
		{
			{
				"scenario_id",
				scenarioId.ToString(CultureInfo.InvariantCulture)
			},
			{ "result", result },
			{
				"boss_id",
				bossId.ToString(CultureInfo.InvariantCulture)
			}
		});
	}

	public void SendEvent_GameRoundEnd(string gameMode, string result, PegasusShared.FormatType formatType)
	{
		SendEvent(new AttributionGameRoundEnd
		{
			ApplicationId = TelemetryManager.GetApplicationId(),
			BundleId = Application.identifier,
			FirstInstallDate = AppLaunchTracker.FirstInstallTimeMilliseconds,
			DeviceType = SystemInfo.deviceModel,
			GameMode = gameMode,
			Result = result,
			FormatType = (Blizzard.Telemetry.WTCG.Client.FormatType)formatType
		});
		SendEventToExternalTrackers("game_end", new Dictionary<string, string>
		{
			{ "gameMode", gameMode },
			{
				"format_type",
				formatType.ToString()
			},
			{ "result", result }
		});
	}

	public void SendEvent_GameRoundStart(string gameMode, PegasusShared.FormatType formatType)
	{
		SendEvent(new AttributionGameRoundStart
		{
			ApplicationId = TelemetryManager.GetApplicationId(),
			BundleId = Application.identifier,
			FirstInstallDate = AppLaunchTracker.FirstInstallTimeMilliseconds,
			DeviceType = SystemInfo.deviceModel,
			GameMode = gameMode,
			FormatType = (Blizzard.Telemetry.WTCG.Client.FormatType)formatType
		});
		SendEventToExternalTrackers("game_start", new Dictionary<string, string>
		{
			{ "gameMode", gameMode },
			{
				"format_type",
				formatType.ToString()
			}
		});
		if (PlatformSettings.IsMobile())
		{
			CheckIfFirstGameAfterTutorial();
		}
	}

	public void SendEvent_Purchase(string productId, string transactionId, int quantity, string currencyCode, bool isVirtualCurrency, float amount)
	{
		if (string.IsNullOrEmpty(productId))
		{
			Log.AdTracking.PrintWarning("Missing productId on Purchase event.");
			return;
		}
		if (string.IsNullOrEmpty(currencyCode))
		{
			Log.AdTracking.PrintWarning("Missing currency on Purchase event for " + productId + ".");
			return;
		}
		AttributionPurchase.PaymentInfo payment = new AttributionPurchase.PaymentInfo
		{
			CurrencyCode = currencyCode,
			IsVirtualCurrency = isVirtualCurrency,
			Amount = amount
		};
		List<AttributionPurchase.PaymentInfo> payments = new List<AttributionPurchase.PaymentInfo> { payment };
		SendEvent(new AttributionPurchase
		{
			ApplicationId = TelemetryManager.GetApplicationId(),
			BundleId = Application.identifier,
			FirstInstallDate = AppLaunchTracker.FirstInstallTimeMilliseconds,
			DeviceType = SystemInfo.deviceModel,
			PurchaseType = productId,
			TransactionId = transactionId,
			Quantity = quantity,
			Payments = payments
		});
		if (currencyCode.ToUpper() == "TPT")
		{
			currencyCode = "TWD";
		}
		else if (currencyCode.ToUpper() == "CPT")
		{
			currencyCode = "CNY";
		}
		SendEventToExternalTrackers("af_purchase", new Dictionary<string, string>
		{
			{ "af_content_type", productId },
			{ "af_receipt_id", transactionId },
			{
				"af_quantity",
				quantity.ToString(CultureInfo.InvariantCulture)
			},
			{ "af_currency", currencyCode },
			{
				"af_revenue",
				amount.ToString(CultureInfo.InvariantCulture)
			}
		});
	}

	public void SendEvent_VirtualCurrencyTransaction(int amount, string currencyName)
	{
		SendEvent(new AttributionVirtualCurrencyTransaction
		{
			ApplicationId = TelemetryManager.GetApplicationId(),
			BundleId = Application.identifier,
			FirstInstallDate = AppLaunchTracker.FirstInstallTimeMilliseconds,
			DeviceType = SystemInfo.deviceModel,
			Amount = amount,
			Currency = currencyName
		});
	}

	public void SendEvent_BoxAfterTutorial(string tutorialCompleted)
	{
		if (HasNotSentExternalTrackerEvent(Option.AF_FIRST_BOX_AFTER_TUTORIAL))
		{
			Options.Get().SetBool(Option.AF_FIRST_BOX_AFTER_TUTORIAL, val: true);
			SendEventToExternalTrackers("first_box_after_tutorial", new Dictionary<string, string> { { "tutorial_completed", tutorialCompleted } });
		}
	}

	public void SendEvent_FirstShopVisit()
	{
		if (HasNotSentExternalTrackerEvent(Option.AF_FIRST_SHOP_VISIT))
		{
			bool firstShopVisitAfterTutorial = false;
			string mode = string.Empty;
			if (SceneMgr.Get().GetMode() == SceneMgr.Mode.LETTUCE_VILLAGE && GameUtils.IsMercenariesVillageTutorialComplete())
			{
				mode = "mercenaries";
				firstShopVisitAfterTutorial = true;
			}
			else if (SceneMgr.Get().GetMode() == SceneMgr.Mode.HUB)
			{
				mode = "box";
				firstShopVisitAfterTutorial = true;
			}
			if (firstShopVisitAfterTutorial)
			{
				Options.Get().SetBool(Option.AF_FIRST_SHOP_VISIT, val: true);
				SendEventToExternalTrackers("first_shop_visit", new Dictionary<string, string> { { "mode", mode } });
			}
		}
	}

	public void SendEvent_PackOpen(int packOpeningId)
	{
		if (HasNotSentExternalTrackerEvent(Option.AF_FIRST_PACK_OPENED))
		{
			bool firstPackOpened = false;
			_ = string.Empty;
			bool packMercenaries = GameUtils.IsMercenariesVillageTutorialComplete() && BoosterPackUtils.GetBoosterOpenedCount(629) > 0;
			if (SceneMgr.Get().GetMode() == SceneMgr.Mode.LETTUCE_PACK_OPENING && packMercenaries)
			{
				firstPackOpened = true;
			}
			else if (SceneMgr.Get().GetMode() == SceneMgr.Mode.PACKOPENING)
			{
				firstPackOpened = true;
			}
			if (firstPackOpened)
			{
				Options.Get().SetBool(Option.AF_FIRST_PACK_OPENED, val: true);
				SendEventToExternalTrackers("first_pack_opening", new Dictionary<string, string> { 
				{
					"pack_id",
					packOpeningId.ToString()
				} });
			}
		}
	}

	public void SendEvent_RewardTrackUpdate(int level)
	{
		if (HasNotSentExternalTrackerEvent(Option.AF_REWARD_TRACK_EVENT, level == 2))
		{
			Options.Get().SetBool(Option.AF_REWARD_TRACK_EVENT, val: true);
			SendEventToExternalTrackers("reward_track", new Dictionary<string, string> { 
			{
				"level",
				level.ToString()
			} });
		}
	}

	public void OnIOSInstallCompleted(bool success, string jsonString, int errorCode, string errorMessage)
	{
		AttributionInstall installEvent = new AttributionInstall
		{
			ApplicationId = TelemetryManager.GetApplicationId(),
			BundleId = Application.identifier,
			FirstInstallDate = AppLaunchTracker.FirstInstallTimeMilliseconds,
			DeviceType = SystemInfo.deviceModel
		};
		if (success)
		{
			installEvent.AppleSearchAdsJson = jsonString;
		}
		else
		{
			installEvent.AppleSearchAdsErrorCode = errorCode;
		}
		SendEvent(installEvent);
	}

	public void OnAndroidInstallCompleted(int responseCode, string referrerUrl)
	{
		AttributionInstall installEvent = new AttributionInstall
		{
			ApplicationId = TelemetryManager.GetApplicationId(),
			BundleId = Application.identifier,
			FirstInstallDate = AppLaunchTracker.FirstInstallTimeMilliseconds,
			DeviceType = SystemInfo.deviceModel
		};
		if (responseCode < 0)
		{
			Log.Telemetry.Print("Error while requesting the referrer URL.");
		}
		else
		{
			installEvent.Referrer = referrerUrl;
		}
		SendEvent(installEvent);
	}

	private void OnInstallTelemetryMessageReceivedByIngest(long messageId)
	{
		if (messageId == 0L)
		{
			Log.Telemetry.PrintError("There was an error sending the 'Attirbution_Install' telemetry event to ingest. Received ({0}) identifier back from telemetry SDK.", messageId);
		}
		else
		{
			AppLaunchTracker.IsInstallReported = true;
		}
	}

	private void SendEvent(IProtoBuf eventMessage, Context messageContext = null)
	{
		if (PlatformSettings.OS == OSCategory.PC || PlatformSettings.OS == OSCategory.Mac)
		{
			if (eventMessage.GetType().Name.Equals("AttributionInstall"))
			{
				AppLaunchTracker.IsInstallReported = true;
			}
			return;
		}
		MessageOptions messageOptions = new MessageOptions
		{
			IdentifierInfoExtensionEnabled = true
		};
		if (messageContext != null)
		{
			messageOptions.Context = messageContext;
		}
		long messageId = m_telemetryClient.EnqueueMessage(eventMessage, messageOptions);
		if (eventMessage.GetType().Name.Equals("AttributionInstall"))
		{
			TelemetryManager.RegisterMessageSentCallback(messageId, OnInstallTelemetryMessageReceivedByIngest);
			TelemetryManager.Flush();
		}
	}

	private void SendEventToExternalTrackers(string eventName, Dictionary<string, string> eventParams)
	{
		HsAppsFlyer.SendEvent(eventName, eventParams);
		m_firebase.SendEvent(eventName, eventParams);
	}

	private void CheckIfFirstGameAfterTutorial()
	{
		bool firstGameAfterTutorial = false;
		string mode = string.Empty;
		bool gameTraditional = GameUtils.IsTraditionalTutorialComplete() && GameMgr.Get().IsPlay();
		bool gameBattlegrounds = GameUtils.IsBattleGroundsTutorialComplete() && GameMgr.Get().IsBattlegrounds();
		bool gameMercenaries = GameUtils.IsMercenariesVillageTutorialComplete() && GameMgr.Get().IsMercenaries();
		if (HasNotSentExternalTrackerEvent(Option.AF_FIRST_NON_TUTORIAL_GAME_START_TRADITIONAL, gameTraditional))
		{
			Options.Get().SetBool(Option.AF_FIRST_NON_TUTORIAL_GAME_START_TRADITIONAL, val: true);
			mode = "traditional";
			firstGameAfterTutorial = true;
		}
		else if (HasNotSentExternalTrackerEvent(Option.AF_FIRST_NON_TUTORIAL_GAME_START_BATTLEGROUNDS, gameBattlegrounds))
		{
			Options.Get().SetBool(Option.AF_FIRST_NON_TUTORIAL_GAME_START_BATTLEGROUNDS, val: true);
			mode = "battlegrounds";
			firstGameAfterTutorial = true;
		}
		else if (HasNotSentExternalTrackerEvent(Option.AF_FIRST_NON_TUTORIAL_GAME_START_MERCENARIES, gameMercenaries))
		{
			Options.Get().SetBool(Option.AF_FIRST_NON_TUTORIAL_GAME_START_MERCENARIES, val: true);
			mode = "mercenaries";
			firstGameAfterTutorial = true;
		}
		if (firstGameAfterTutorial)
		{
			SendEventToExternalTrackers("first_game_after_tutorial", new Dictionary<string, string> { { "mode", mode } });
		}
	}

	private bool HasNotSentExternalTrackerEvent(Option option, bool additionalCheck = true)
	{
		if (PlatformSettings.IsMobile())
		{
			return !Options.Get().GetBool(option) && additionalCheck;
		}
		return false;
	}

	public void StopCollection()
	{
		HsAppsFlyer.Stop(isSdkStopped: true);
		m_firebase.SetEnabledCollection(enabled: false);
	}

	public void ResumeCollection()
	{
		HsAppsFlyer.Stop(isSdkStopped: false);
		m_firebase.SetEnabledCollection(m_optOutFlag.Value);
	}
}
