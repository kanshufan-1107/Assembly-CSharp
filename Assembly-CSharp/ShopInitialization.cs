using System;
using System.Collections;
using Blizzard.GameService.SDK.Client.Integration;
using Blizzard.T5.Services;
using Hearthstone.Core;
using Hearthstone.Store;
using Hearthstone.Streaming;
using UnityEngine;

public static class ShopInitialization
{
	private static class ShopStatusMessage
	{
		public static readonly string InitialStatusMessage = "Waiting for Shop initial state from Hearthstone Server.";

		public static readonly string StartInitialization = "Got Shop initial state from Hearthstone server. Starting initialization...";

		public static readonly string CancellingInitializationCoroutine = "Cancelling initialization coroutine in progress";

		public static readonly string WaitForProductDataService = "Waiting for ProductDataService Initialization";

		public static readonly string StoringProductDataFromServer = "Storing product data from Hearthstone server";

		public static readonly string WaitForCSDKInitialization = "Waiting Commerce SDK service initialization (HearthstoneCheckout)";

		public static readonly string WaitForGuardianVars = "Waiting for guardian vars from server";

		public static readonly string WaitForCurrencyStatesFromNetCache = "Waiting to receive currency states from NetCache";

		public static readonly string ApplyDataFromServerToCSDK = "Applying data from Hearthstone Server to Commerce SDK";

		public static readonly string WaitForCSDKToLoadStorePages = "Waiting for Commerce SDK to load all requested storefront pages";

		public static readonly string WaitForCSDKToLoadProductData = "Waiting for Commerce SDK to load all product data";

		public static readonly string ReconcileServerAndCSDKProducts = "Reconciling Hearthstone Server and Commerce SDK products";

		public static readonly string WaitForPrerequisiteSystem = "Waiting for prerequisite system: {0}";

		public static readonly string CreateDataModels = "Creating data models";

		public static readonly string WaitForCurrencyManager = "Waiting Currency Manager service";

		public static readonly string WaitForGoldBalance = "Waiting for gold balance";

		public static readonly string WaitForGamemodeAvailabilityService = "Waiting for GamemodeAvailabilityService";

		public static readonly string WaitForConnectionToBattleNet = "Waiting for connection to BattleNet";

		public static readonly string WaitForVirtualCurrencyBalance = "Waiting for virtual currency balance: {0}";

		public static readonly string WaitForBoosterVirtualCurrencyBalance = "Waiting for booster virtual currency balance: {0}";

		public static readonly string WaitForNoticeMessageProcessing = "Waiting for Notice message processing";

		public static readonly string WaitForFriendlyChallenge = "Waiting for Friendly Challenge to be answered";

		public static readonly string WaitForSetRotationRollover = "Waiting for Set Rotation Rollover.";

		public static readonly string WaitForRequiredPlayerMigration = "Waiting for Required Player Migration.";

		public static readonly string WaitForDownload = "Waiting for Download.";

		public static readonly string ShopIsReady = " === Shop initialized and ready to be opened! === ";
	}

	private enum ShopStatusTelemetryMessage
	{
		BPAY_CONFIG_UNAVAILABLE,
		PRODUCT_DATA_SERVICE_NOT_INITIALIZED,
		CSDK_NOT_INITIALIZED,
		BPAY_RECONCILE_FAILURE,
		SHOP_FEATURE_DISABLED,
		NO_VALID_PRODUCTS,
		STALLED,
		NO_ERROR
	}

	private static DateTime s_initializeStartTime;

	private static DateTime s_lastStatusChangeTime;

	private static bool s_hasStatusStalled;

	private static IEnumerator s_coroutineEnumerator;

	private static readonly float StalledThresholdSeconds = 10f;

	public static ShopStatus Status { get; private set; } = ShopStatus.NotStarted;

	public static string StatusMessage { get; private set; } = ShopStatusMessage.InitialStatusMessage;

	public static void Reset()
	{
		if (s_coroutineEnumerator != null)
		{
			SetAndLogStatus("Shop was reset, canceling initialization in-progress");
			Processor.CancelCoroutine(s_coroutineEnumerator);
			s_coroutineEnumerator = null;
		}
		Status = ShopStatus.NotStarted;
		SetAndLogStatus("Shop was reset. " + ShopStatusMessage.InitialStatusMessage);
		StoreManager.Get().HandleShopAvailabilityChange();
	}

	public static void StartInitializing(Network.BattlePayConfig battlePayConfig)
	{
		s_initializeStartTime = DateTime.Now;
		Status = ShopStatus.Initializing;
		SetAndLogStatus(ShopStatusMessage.StartInitialization);
		if (s_coroutineEnumerator != null)
		{
			SetAndLogStatus(ShopStatusMessage.CancellingInitializationCoroutine);
			Processor.CancelCoroutine(s_coroutineEnumerator);
		}
		s_coroutineEnumerator = InitializeShopCoroutine(battlePayConfig);
		Processor.RunCoroutine(s_coroutineEnumerator);
	}

	private static IEnumerator InitializeShopCoroutine(Network.BattlePayConfig battlePayConfig)
	{
		if (!battlePayConfig.Available)
		{
			SetAndLogFailure("Hearthstone server responds that shop is unavailable");
			SendShopStatusTelemetry(ShopStatusTelemetryMessage.BPAY_CONFIG_UNAVAILABLE);
			yield break;
		}
		StoreManager storeManager = StoreManager.Get();
		SetAndLogStatus(ShopStatusMessage.WaitForProductDataService);
		while (!ServiceManager.IsAvailable<IProductDataService>())
		{
			if (ServiceManager.GetServiceState<IProductDataService>() == ServiceLocator.ServiceState.Error)
			{
				SetAndLogFailure("Failed to initialize ProductDataService");
				SendShopStatusTelemetry(ShopStatusTelemetryMessage.PRODUCT_DATA_SERVICE_NOT_INITIALIZED);
				yield break;
			}
			yield return null;
		}
		IProductDataService productDataService = ServiceManager.Get<IProductDataService>();
		SetAndLogStatus(ShopStatusMessage.StoringProductDataFromServer);
		SetProductDataFromHearthstoneServer(battlePayConfig, productDataService);
		SetAndLogStatus(ShopStatusMessage.WaitForCSDKInitialization);
		while (!ServiceManager.IsAvailable<HearthstoneCheckout>())
		{
			if (ServiceManager.GetServiceState<HearthstoneCheckout>() == ServiceLocator.ServiceState.Error)
			{
				SetAndLogFailure("Failed to initialize Commerce SDK service (HearthstoneCheckout)");
				SendShopStatusTelemetry(ShopStatusTelemetryMessage.CSDK_NOT_INITIALIZED);
				yield break;
			}
			yield return null;
		}
		HearthstoneCheckout csdkService = ServiceManager.Get<HearthstoneCheckout>();
		SetAndLogStatus(ShopStatusMessage.WaitForGuardianVars);
		NetCache.NetCacheFeatures netCacheFeatures;
		for (netCacheFeatures = NetCache.Get()?.GetNetObject<NetCache.NetCacheFeatures>(); netCacheFeatures == null; netCacheFeatures = NetCache.Get()?.GetNetObject<NetCache.NetCacheFeatures>())
		{
			yield return null;
		}
		if (!netCacheFeatures.Store.Store)
		{
			SetAndLogFailure("Shop is disabled by Features.Store guardian var");
			SendShopStatusTelemetry(ShopStatusTelemetryMessage.SHOP_FEATURE_DISABLED);
			yield break;
		}
		SetAndLogStatus(ShopStatusMessage.WaitForCurrencyStatesFromNetCache);
		while (!HasReceivedCurrencyState())
		{
			yield return null;
		}
		SetAndLogStatus(ShopStatusMessage.ApplyDataFromServerToCSDK);
		SetCommerceSdkDataFromHearthstoneServer(battlePayConfig, csdkService);
		SetAndLogStatus(ShopStatusMessage.WaitForCSDKToLoadStorePages);
		while (!productDataService.HasReceivedAllShopTypeSections())
		{
			yield return null;
		}
		SetAndLogStatus(ShopStatusMessage.WaitForCSDKToLoadProductData);
		while (!csdkService.ReceivedSdkProducts)
		{
			yield return null;
		}
		SetAndLogStatus(ShopStatusMessage.ReconcileServerAndCSDKProducts);
		foreach (ProductInfo item in productDataService.ReconcileHearthstoneServerAndCommerceSdkProducts())
		{
			item.StringLogSetStatus();
		}
		if (!storeManager.HaveProductsToSell())
		{
			SetAndLogFailure("No valid products available.");
			SendShopStatusTelemetry(ShopStatusTelemetryMessage.NO_VALID_PRODUCTS);
			yield break;
		}
		string previousReasonBlocked = null;
		string blockedReason;
		while (!CatalogUtils.CanUpdateProductStatus(out blockedReason))
		{
			if (blockedReason != previousReasonBlocked)
			{
				SetAndLogStatus(string.Format(ShopStatusMessage.WaitForPrerequisiteSystem, blockedReason));
				previousReasonBlocked = blockedReason;
			}
			yield return null;
		}
		SetAndLogStatus(ShopStatusMessage.CreateDataModels);
		productDataService.CreateDataModels();
		SetAndLogStatus(ShopStatusMessage.WaitForCurrencyManager);
		while (!ServiceManager.IsAvailable<CurrencyManager>())
		{
			if (ServiceManager.GetServiceState<HearthstoneCheckout>() == ServiceLocator.ServiceState.Error)
			{
				SetAndLogFailure("Failed to initialize Currency Manager");
				SendShopStatusTelemetry(ShopStatusTelemetryMessage.CSDK_NOT_INITIALIZED);
				yield break;
			}
			yield return null;
		}
		CurrencyManager currencyManager = ServiceManager.Get<CurrencyManager>();
		SetAndLogStatus(ShopStatusMessage.WaitForGoldBalance);
		while (!currencyManager.IsBalanceAvailable(CurrencyType.GOLD))
		{
			yield return null;
		}
		if (ShopUtils.IsVirtualCurrencyEnabled())
		{
			if (!BattleNet.IsConnected())
			{
				SetAndLogStatus(ShopStatusMessage.WaitForConnectionToBattleNet);
				while (!BattleNet.IsConnected())
				{
					yield return null;
				}
			}
			CurrencyType mainVcType;
			bool num = ShopUtils.TryGetMainVirtualCurrencyType(out mainVcType);
			CurrencyType boosterVcType;
			bool hasBoosterVc = ShopUtils.TryGetBoosterVirtualCurrencyType(out boosterVcType);
			if (num)
			{
				SetAndLogStatus(string.Format(ShopStatusMessage.WaitForVirtualCurrencyBalance, mainVcType));
				while (!currencyManager.IsBalanceAvailable(mainVcType))
				{
					yield return null;
				}
			}
			if (hasBoosterVc)
			{
				SetAndLogStatus(string.Format(ShopStatusMessage.WaitForBoosterVirtualCurrencyBalance, boosterVcType));
				while (!currencyManager.IsBalanceAvailable(boosterVcType))
				{
					yield return null;
				}
			}
		}
		SetAndLogStatus(ShopStatusMessage.WaitForGamemodeAvailabilityService);
		while (!ServiceManager.IsAvailable<IGamemodeAvailabilityService>())
		{
			if (ServiceManager.GetServiceState<IGamemodeAvailabilityService>() == ServiceLocator.ServiceState.Error)
			{
				SetAndLogFailure("Failed to initialize IGamemodeAvailabilityService");
				SendShopStatusTelemetry(ShopStatusTelemetryMessage.STALLED);
				yield break;
			}
			yield return null;
		}
		SetAndLogStatus(ShopStatusMessage.WaitForNoticeMessageProcessing);
		while (!storeManager.FirstNoticesProcessed)
		{
			yield return null;
		}
		bool loggedOnce = false;
		FriendChallengeMgr challengeManager = FriendChallengeMgr.Get();
		while (challengeManager != null && challengeManager.HasChallenge())
		{
			if (!loggedOnce)
			{
				SetAndLogStatus(ShopStatusMessage.WaitForFriendlyChallenge);
				loggedOnce = true;
			}
			yield return null;
		}
		loggedOnce = false;
		SetRotationManager setRotationManager = SetRotationManager.Get();
		while (setRotationManager != null && setRotationManager.CheckForSetRotationRollover())
		{
			if (!loggedOnce)
			{
				SetAndLogStatus(ShopStatusMessage.WaitForSetRotationRollover);
				loggedOnce = true;
			}
			yield return null;
		}
		loggedOnce = false;
		PlayerMigrationManager playerMigrationManager = PlayerMigrationManager.Get();
		while (playerMigrationManager != null && playerMigrationManager.CheckForPlayerMigrationRequired())
		{
			if (!loggedOnce)
			{
				SetAndLogStatus(ShopStatusMessage.WaitForRequiredPlayerMigration);
				loggedOnce = true;
			}
			yield return null;
		}
		SetAndLogStatus(ShopStatusMessage.WaitForDownload);
		while (GameDownloadManagerProvider.Get() == null || !GameDownloadManagerProvider.Get().IsReadyToPlay)
		{
			yield return null;
		}
		Status = ShopStatus.Ready;
		SetAndLogStatus(ShopStatusMessage.ShopIsReady);
		SendShopStatusTelemetry(ShopStatusTelemetryMessage.NO_ERROR);
		s_coroutineEnumerator = null;
		StoreManager.Get().HandleShopAvailabilityChange();
	}

	private static void SetAndLogStatus(string message)
	{
		StatusMessage = message;
		Log.Store.PrintInfo("ShopInitialization status: " + message);
		s_lastStatusChangeTime = DateTime.Now;
		s_hasStatusStalled = false;
	}

	private static void SetAndLogFailure(string message)
	{
		StatusMessage = message;
		Status = ShopStatus.Failed;
		Log.Store.PrintError("ShopInitialization failed: " + message);
		s_lastStatusChangeTime = DateTime.Now;
		s_hasStatusStalled = false;
		s_coroutineEnumerator = null;
	}

	private static void SendShopStatusTelemetry(ShopStatusTelemetryMessage message)
	{
		SendShopStatusTelemetry(message.ToString());
	}

	private static void SendShopStatusTelemetry(string message)
	{
		double elapsedSeconds = (DateTime.Now - s_initializeStartTime).TotalSeconds;
		TelemetryManager.Client().SendShopStatus(message, elapsedSeconds, Time.realtimeSinceStartup - SplashScreen.FadeOutCompleteTime);
	}

	public static void SendShopStatusTelemetryIfStalled()
	{
		if (Status == ShopStatus.Initializing && !s_hasStatusStalled && (DateTime.Now - s_lastStatusChangeTime).TotalSeconds > (double)StalledThresholdSeconds)
		{
			s_hasStatusStalled = true;
			SendShopStatusTelemetry(ShopStatusTelemetryMessage.STALLED.ToString() + StatusMessage);
		}
	}

	private static void SetCommerceSdkDataFromHearthstoneServer(Network.BattlePayConfig battlePayConfig, HearthstoneCheckout csdkService)
	{
		HearthstoneCheckout.OneStoreKey = battlePayConfig.CheckoutKrOnestoreKey;
		csdkService.SetCurrencyCode(battlePayConfig.Currency?.Code ?? string.Empty);
		csdkService.SetClientID(battlePayConfig.CommerceClientID);
	}

	private static void SetProductDataFromHearthstoneServer(Network.BattlePayConfig battlePayConfig, IProductDataService productDataService)
	{
		foreach (Network.Bundle bundle in battlePayConfig.Bundles)
		{
			productDataService.SetProduct(bundle);
		}
		productDataService.ShopPlacementIds.Clear();
		foreach (string shopPageId in battlePayConfig.ShopPageIds)
		{
			if (!string.IsNullOrEmpty(shopPageId))
			{
				productDataService.ShopPlacementIds.Add(shopPageId);
			}
		}
	}

	private static bool HasReceivedCurrencyState()
	{
		NetCache netCache = NetCache.Get();
		if (netCache != null && netCache.GetNetObject<NetCache.NetCacheArcaneDustBalance>() != null && netCache.GetNetObject<NetCache.NetCacheGoldBalance>() != null && netCache.GetNetObject<NetCache.NetCacheRenownBalance>() != null)
		{
			return netCache.GetNetObject<NetCache.NetCacheBattlegroundsTokenBalance>() != null;
		}
		return false;
	}
}
