using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Blizzard.Commerce;
using Blizzard.GameService.SDK.Client.Integration;
using Blizzard.T5.Configuration;
using Blizzard.T5.Jobs;
using Blizzard.T5.Logging;
using Blizzard.T5.Services;
using com.blizzard.commerce.Model;
using Hearthstone;
using Hearthstone.Core;
using Hearthstone.Core.Deeplinking;
using Hearthstone.Login;
using Hearthstone.Store;
using PegasusUtil;
using UnityEngine;

public class HearthstoneCheckout : blz_commerce_log_hook, ISceneEventObserver, IVirtualCurrencyEventObserver, IPurchaseEventObserver, ICatalogEventObserver, IHasUpdate, IService, IDeeplinkCallback
{
	public delegate void VirtualCurrencyBalanceCallback(VirtualCurrencyBalanceResult vcBalanceResult);

	public delegate void PersonalizedShopResponseCallback(IList<Placement> placements, RpcError error);

	public readonly struct VirtualCurrencyBalanceResult
	{
		public readonly bool isSuccess;

		public readonly string errorMessage;

		public readonly string currencyCode;

		public readonly long balance;

		public VirtualCurrencyBalanceResult(bool isSuccess, string errorMessage, string currencyCode, long balance)
		{
			this.isSuccess = isSuccess;
			this.errorMessage = errorMessage;
			this.currencyCode = currencyCode;
			this.balance = balance;
		}
	}

	private class WaitForIdle : IJobDependency, IAsyncJobResult
	{
		public bool IsReady()
		{
			return CommerceWrapper.Instance.IsIdle;
		}
	}

	private readonly struct VirtualCurrencyRequest
	{
		public readonly string currencyCode;

		public readonly VirtualCurrencyBalanceCallback callback;

		public VirtualCurrencyRequest(string currencyCode, VirtualCurrencyBalanceCallback callback)
		{
			this.currencyCode = currencyCode;
			this.callback = callback;
		}
	}

	private class WaitForClientInitializationResponse : IJobDependency, IAsyncJobResult
	{
		private readonly HearthstoneCheckout m_hearthstoneCheckout;

		private readonly float m_timeoutTimestamp;

		public WaitForClientInitializationResponse(HearthstoneCheckout hearthstoneCheckout, float timeoutDuration)
		{
			m_hearthstoneCheckout = hearthstoneCheckout;
			m_timeoutTimestamp = Time.realtimeSinceStartup + timeoutDuration;
		}

		public bool IsReady()
		{
			if (m_hearthstoneCheckout.m_ssoTokenState == State.Initializing)
			{
				return m_timeoutTimestamp <= Time.realtimeSinceStartup;
			}
			return true;
		}
	}

	private const string kTitleCode = "WTCG";

	private const string kHearthstoneCheckoutPrefab = "HearthstoneCheckout.prefab:da1b8fa18876ab5468bd2aa04a3f2539";

	private const int kInitializationRetryCount = 3;

	private const float kResolutionUpdateIntervalSeconds = 1f;

	private const int kMaxAttempts = 10;

	private const int kSteamTimeoutSeconds = 60;

	private const int kMaxSteamTimeoutSeconds = 180;

	private const float FAIL_WAIT_TIME = 0.5f;

	private const float MAX_FAIL_WAIT_TIME = 5f;

	private const float kInProgressBackgroundableDelay = 10f;

	private const byte kBYes = 1;

	private const byte kBNo = 0;

	private const string kCommerceSDKScheme = "blizzard";

	private const int kStoreId = 6;

	private const int kSteamEcosystem = 9;

	private const int kMaxProductLoadCount = 300;

	private static readonly Dictionary<string, string> kBNETCurrencyConversions = new Dictionary<string, string>
	{
		{ "CPT", "CBT" },
		{ "TPT", "TWD" },
		{ "GEL", "EUR" }
	};

	private static Type[] m_dependencies = null;

	private State m_csdkState;

	private State m_ssoTokenState;

	private State m_checkoutUiState;

	private State m_loadPageState;

	private State m_loadProducts;

	private State m_TransactionState;

	private PurchaseHandle m_purchaseHandle;

	private HearthstoneCheckoutUI m_checkoutUI;

	private bool m_closeRequested;

	private TransactionData m_currentTransaction;

	private Vector2 m_screenResolution;

	private float m_elapsedTimeSinceResolutionCheck;

	private float m_elapsedTimeSinceShown;

	private DateTime m_transactionStart = DateTime.Now;

	private int m_retriesRemaining = 3;

	private string m_currencyCode;

	private readonly List<VirtualCurrencyRequest> m_virtualCurrencyRequests = new List<VirtualCurrencyRequest>();

	private string m_clientID;

	private bool _shouldCallCSDKUpdate;

	private Queue<Action> _isOpenCallbacks = new Queue<Action>();

	private bool? m_overrideEndpointToProduction;

	private EventListenerObserverImpl commerceObserverImpl;

	private float m_loadProductsStartTime;

	private static string s_oneStoreKey;

	private Network m_network;

	private long m_lastTimeStamp;

	private string m_steamCurrencyCode;

	private AccountInitializationValues m_accountInfo;

	private SystemInitializationValues m_systemInfo;

	private bool m_canMakeSteamPurchase;

	internal bool m_arePurchasesBlocked;

	internal bool m_isPendingBlocked;

	[CompilerGenerated]
	private bool _003CIsSystemEnabled_003Ek__BackingField;

	public bool ReceivedSdkProducts
	{
		get
		{
			if (m_loadProducts == State.Finished)
			{
				return m_loadPageState == State.Finished;
			}
			return false;
		}
	}

	public static string OneStoreKey
	{
		set
		{
		}
	}

	public HearthstoneCheckoutUI CheckoutUi => m_checkoutUI;

	public bool CheckoutIsReady
	{
		get
		{
			if (m_checkoutUiState != State.Startup)
			{
				return m_checkoutUiState == State.InProgress;
			}
			return true;
		}
	}

	private bool IsSystemEnabled
	{
		[CompilerGenerated]
		set
		{
			_003CIsSystemEnabled_003Ek__BackingField = value;
		}
	}

	public bool HasProductCatalog
	{
		get
		{
			if (ServiceManager.TryGet<IProductDataService>(out var dataService))
			{
				return dataService.ProductCatalog != null;
			}
			return false;
		}
	}

	public bool HasClientID => m_clientID != null;

	public bool HasCurrencyCode
	{
		get
		{
			if (m_currencyCode != null)
			{
				if (PlatformSettings.IsSteam)
				{
					return m_steamCurrencyCode != null;
				}
				return true;
			}
			return false;
		}
	}

	public bool IsIdle
	{
		get
		{
			if (m_csdkState == State.Finished)
			{
				return m_TransactionState == State.Ready;
			}
			return false;
		}
	}

	public bool IsInProgress
	{
		get
		{
			if (m_csdkState == State.Finished)
			{
				return IsTransactionInProgress();
			}
			return false;
		}
	}

	public bool IsUIShown
	{
		get
		{
			if (m_checkoutUI != null)
			{
				return m_checkoutUI.IsShown();
			}
			return false;
		}
	}

	public float ShownTime => m_elapsedTimeSinceShown;

	public bool ShouldBlockInput
	{
		get
		{
			if (m_checkoutUI != null)
			{
				return m_checkoutUI.IsShown();
			}
			return false;
		}
	}

	public bool CanMakeRealMoneyPurchase
	{
		get
		{
			if (!m_isPendingBlocked && !m_canMakeSteamPurchase)
			{
				return !m_arePurchasesBlocked;
			}
			return false;
		}
	}

	public IEnumerator<IAsyncJobResult> Initialize(ServiceLocator serviceLocator)
	{
		InitializeNetworkServices(serviceLocator.Get<Network>());
		if (Vars.Key("Commerce.OverrideEndpointToProduction").HasValue)
		{
			m_overrideEndpointToProduction = Vars.Key("Commerce.OverrideEndpointToProduction").GetBool(def: true);
		}
		string libraryError = null;
		Exception error = null;
		DateTime lastTry = DateTime.UtcNow + TimeSpan.FromSeconds(5.0);
		ushort tryCount = 0;
		do
		{
			if (error != null)
			{
				Log.Store.PrintWarning("[HearthstoneCheckout.Initialize] Create SDK error: (" + error.Message + "). Retrying...");
				error = null;
			}
			tryCount++;
			try
			{
				if (CommerceWrapper.Instance.IsUninit)
				{
					CommerceWrapper.Instance.Dispose();
					error = new Exception("Unknown");
				}
			}
			catch (Exception ex)
			{
				error = ex;
			}
			if (error != null)
			{
				DateTime nextTry = DateTime.UtcNow + TimeSpan.FromSeconds(0.5);
				while (DateTime.UtcNow < nextTry)
				{
					yield return null;
				}
			}
		}
		while (error != null && DateTime.UtcNow < lastTry);
		if (error != null)
		{
			m_csdkState = State.Unavailable;
			Log.Store.PrintError("[HearthstoneCheckout.Initialize] Create SDK Total Failure: (" + error.Message + ").");
			Exception innerException = error.InnerException;
			int maxInnerRun = 10;
			while (innerException != null && maxInnerRun > 0)
			{
				Log.Store.PrintError("[HearthstoneCheckout.Initialize] Inner exception: (" + innerException.ToString() + ").");
				innerException = innerException.InnerException;
				maxInnerRun--;
			}
			TelemetryManager.Client().SendBlizzardCheckoutInitializationResult(success: false, "Commerce SDK not Valid.", error.Message);
			Log.Store.PrintError($"[HearthstoneCheckout.Initialize] Tried {tryCount} times.");
			Log.Store.PrintError("[HearthstoneCheckout.Initialize] " + error.StackTrace + ".");
			yield return new JobFailedResult(error.Message);
		}
		try
		{
			commerceObserverImpl = new EventListenerObserverImpl(this);
			commerceObserverImpl.AddSceneObserver(this);
			commerceObserverImpl.AddCatalogObserver(this);
			commerceObserverImpl.AddPurchaseObserver(this);
			commerceObserverImpl.AddVirtualCurrencyObserver(this);
		}
		catch (Exception ex2)
		{
			m_csdkState = State.Unavailable;
			libraryError = $"Failed to initialize HearthstoneCheckout: {ex2}";
			TelemetryManager.Client().SendBlizzardCheckoutInitializationResult(success: false, "Checkout Library Exception.", ex2.ToString());
		}
		if (!string.IsNullOrEmpty(libraryError))
		{
			TelemetryManager.Client().SendBlizzardCheckoutInitializationResult(success: false, "Commerce SDK Interface error.", libraryError);
			yield return new JobFailedResult(libraryError);
		}
		HearthstoneApplication hearthstoneApplication = HearthstoneApplication.Get();
		if (hearthstoneApplication != null)
		{
			hearthstoneApplication.WillReset += OnReset;
		}
		if (!(DeeplinkService.Get()?.RegisterDeeplinkHandler("blizzard", this) ?? false))
		{
			Log.Store.PrintError("Unable register with deeplink service");
		}
		JobDefinition loadCheckoutUIJob = new JobDefinition("HearthstoneCheckout.LoadCheckoutUI", Job_CreateCSDK(), JobFlags.StartImmediately, new WaitForGameDownloadManagerState());
		Processor.QueueJob(loadCheckoutUIJob);
		Processor.QueueJob(new JobDefinition("HearthstoneCheckout.InitializeCheckoutClient", Job_InitializeCheckoutClient(), loadCheckoutUIJob.CreateDependency(), new WaitForCheckoutConfiguration()));
	}

	public Type[] GetDependencies()
	{
		if (m_dependencies == null)
		{
			m_dependencies = new Type[4]
			{
				typeof(Network),
				typeof(LoginManager),
				typeof(IAssetLoader),
				typeof(ILoginService)
			};
		}
		return m_dependencies;
	}

	public void Update()
	{
		if (m_checkoutUI != null && m_checkoutUI.HasCheckoutMesh && CheckoutIsReady)
		{
			m_elapsedTimeSinceResolutionCheck += Time.deltaTime;
			if (m_elapsedTimeSinceResolutionCheck > 1f)
			{
				ScreenResolutionUpdate();
				m_elapsedTimeSinceResolutionCheck = 0f;
			}
		}
		if (IsUIShown)
		{
			m_elapsedTimeSinceShown += Time.deltaTime;
		}
		if (m_closeRequested)
		{
			if ((m_currentTransaction == null || !m_currentTransaction.IsVCPurchase) && !CommerceWrapper.Instance.SendBrowserCloseEvent())
			{
				Log.Store.PrintWarning("[HearthstoneCheckout.Update] SendBrowserCloseEvent failed");
			}
			if (!IsTransactionInProgress())
			{
				ClearTransaction();
			}
			else if (IsUIShown)
			{
				m_checkoutUI.Hide();
				m_checkoutUiState = State.Ready;
			}
			m_closeRequested = false;
			StoreManager.Get()?.HandleCommerceCloseEvent();
		}
		else
		{
			if (_shouldCallCSDKUpdate && CommerceWrapper.Instance.IsIdle && !CommerceWrapper.Instance.Update())
			{
				Log.Store.PrintWarning("[HearthstoneCheckout.Update] Update failed");
			}
			if (m_TransactionState == State.InProgress && (float)(DateTime.Now - m_transactionStart).Seconds >= 10f)
			{
				m_TransactionState = State.InProgress_Backgroundable;
			}
		}
	}

	public List<string> GetCurrencyCodes()
	{
		if (m_currencyCode == "CPT")
		{
			return new List<string> { "XSA", "XSB" };
		}
		if (!PlatformSettings.IsSteam)
		{
			return null;
		}
		if (m_steamCurrencyCode == null || m_currencyCode == null)
		{
			Log.Store.PrintError("Unexpected null currency codes: Steam " + m_steamCurrencyCode + "; BNET " + m_currencyCode);
		}
		if (string.IsNullOrEmpty(m_currencyCode) || !kBNETCurrencyConversions.TryGetValue(m_currencyCode, out var bnetCurrencyCode))
		{
			bnetCurrencyCode = m_currencyCode;
		}
		if (string.IsNullOrEmpty(bnetCurrencyCode) && string.IsNullOrEmpty(m_steamCurrencyCode))
		{
			return null;
		}
		if (string.IsNullOrEmpty(m_steamCurrencyCode) || bnetCurrencyCode == m_steamCurrencyCode)
		{
			return new List<string> { bnetCurrencyCode };
		}
		if (string.IsNullOrEmpty(bnetCurrencyCode))
		{
			return new List<string> { m_steamCurrencyCode };
		}
		return new List<string> { m_steamCurrencyCode, bnetCurrencyCode };
	}

	public IEnumerator<IAsyncJobResult> LoadProducts(List<uint> productIds)
	{
		if (BattleNet.GetCurrentRegion() == BnetRegion.REGION_UNINITIALIZED)
		{
			Log.Store.PrintError("[HearthstoneCheckout.LoadProducts] Tried to load products without a valid region!");
			yield return new JobFailedResult("Tried to load products without a valid region!");
		}
		List<string> currencyCodes = GetCurrencyCodes();
		bool ranSucceeded = false;
		m_loadProducts = State.Initializing;
		_shouldCallCSDKUpdate = false;
		yield return new WaitForIdle();
		m_loadProductsStartTime = Time.realtimeSinceStartup;
		int remainingRuns = 10;
		do
		{
			if (!CommerceWrapper.Instance.LoadProducts(currencyCodes, 300u, productIds))
			{
				Log.Store.PrintWarning("[HearthstoneCheckout.LoadProducts] LoadProducts failed");
				if (!CommerceWrapper.Instance.Update())
				{
					Log.Store.PrintWarning("[HearthstoneCheckout.LoadProducts] Update failed");
				}
			}
			else
			{
				ranSucceeded = true;
				remainingRuns = 0;
			}
			yield return null;
			remainingRuns--;
		}
		while (remainingRuns > 0);
		_shouldCallCSDKUpdate = true;
		if (!ranSucceeded)
		{
			yield return new JobFailedResult("LoadProducts failed");
		}
	}

	public void Shutdown()
	{
		Log.Store.PrintDebug("[HearthstoneCheckout.Shutdown]");
		battlenet_commerce.blz_commerce_unregister_log(IntPtr.Zero);
		DeeplinkService.Get()?.UnregisterDeeplinkHandler("blizzard", this);
		if (ServiceManager.TryGet<IProductDataService>(out var dataService))
		{
			dataService.ResetProducts();
		}
		m_loadProducts = State.Uninit;
		m_loadPageState = State.Uninit;
		m_csdkState = State.Uninit;
		m_TransactionState = State.Uninit;
		m_currentTransaction = null;
		_shouldCallCSDKUpdate = true;
		m_virtualCurrencyRequests.Clear();
		_isOpenCallbacks.Clear();
		DestroyCheckoutUI();
		DisposeCurrentCheckoutClient();
		DisposeListeners();
		HearthstoneApplication hearthstoneApplication = HearthstoneApplication.Get();
		if (hearthstoneApplication != null)
		{
			hearthstoneApplication.WillReset -= OnReset;
		}
	}

	public void ShowCheckout(ProductId productID, string currencyCode, uint quantity)
	{
		Processor.QueueJob("ShowCheckout", Job_ShowCheckout(productID, currencyCode, quantity)).AddJobFinishedEventListener(OnPurchaseJobFinished);
	}

	public void PurchaseWithVirtualCurrency(ProductId productID, string currencyCode, uint quantity)
	{
		Processor.QueueJob("PurchaseWithVirtualCurrency", Job_PurchaseWithVirtualCurrency(productID, currencyCode, quantity)).AddJobFinishedEventListener(OnPurchaseJobFinished);
	}

	public bool RegisterReadyCallback(Action callback)
	{
		if (IsAvailable())
		{
			callback();
			return false;
		}
		_isOpenCallbacks.Enqueue(callback);
		return true;
	}

	public IEnumerator GetVirtualCurrencyBalance(string currencyCode, VirtualCurrencyBalanceCallback callback, Action<bool> resultCallback)
	{
		bool successful = false;
		if (!CommerceWrapper.Instance.IsValid)
		{
			Log.Store.PrintError("[HearthstoneCheckout.GetVirtualCurrencyBalance] Cannot get virtual currency balance because the checkout client isn't initialized.");
		}
		_shouldCallCSDKUpdate = false;
		int waitCount = 100;
		while (waitCount > 0)
		{
			if (!CommerceWrapper.Instance.IsIdle)
			{
				waitCount = 0;
			}
			waitCount--;
			yield return null;
		}
		if (CommerceWrapper.Instance.GetVCBalance(currencyCode))
		{
			m_virtualCurrencyRequests.Add(new VirtualCurrencyRequest(currencyCode, callback));
			successful = true;
		}
		_shouldCallCSDKUpdate = true;
		if (!successful)
		{
			Log.Store.PrintWarning("[HearthstoneCheckout.GetVirtualCurrencyBalance] GetVCBalance failed");
		}
		resultCallback(successful);
	}

	public bool IsAvailable()
	{
		if (m_csdkState == State.Finished && m_checkoutUiState != State.Initializing)
		{
			return m_checkoutUiState != State.Uninit;
		}
		return false;
	}

	public bool IsClientCreationInProgress()
	{
		if (m_csdkState != State.Initializing && m_csdkState != 0 && m_checkoutUiState != State.Initializing)
		{
			return m_checkoutUiState == State.Uninit;
		}
		return true;
	}

	public void LoadPersonalizedShopData(List<string> placementIds, PersonalizedShopResponseCallback callback)
	{
		Processor.QueueJob("HearthstoneCheckout.GetPersonalizedShopData", GetPersonalizedShopData(placementIds, callback));
	}

	private IEnumerator<IAsyncJobResult> GetPersonalizedShopData(List<string> placementIds, PersonalizedShopResponseCallback callback)
	{
		if (callback == null)
		{
			Log.Store.PrintError("[HearthstoneCheckout.GetPersonalizedShopData] Callback cannot be null.");
			yield return new JobFailedResult("Callback cannot be null.");
		}
		if (!Network.IsLoggedIn())
		{
			Log.Store.PrintError("[HearthstoneCheckout.GetPersonalizedShopData] Cannot get personalized shop data because the user is off-line.");
			yield return new JobFailedResult("Cannot get personalized shop data because the user is off-line.");
		}
		m_loadPageState = State.Initializing;
		_shouldCallCSDKUpdate = false;
		int runCount = 100;
		while (runCount > 0)
		{
			if (!CommerceWrapper.Instance.IsIdle)
			{
				runCount = 0;
			}
			runCount--;
			yield return null;
		}
		GetCurrentPagesRequest request = new GetCurrentPagesRequest
		{
			gameServiceRegionId = m_accountInfo.Region,
			placementIds = placementIds,
			storeId = m_systemInfo.StoreId,
			locale = m_accountInfo.Locale
		};
		if (!CommerceWrapper.Instance.LoadPersonalizedShop(request))
		{
			_shouldCallCSDKUpdate = true;
			Log.Store.PrintError("[HearthstoneCheckout.GetPersonalizedShopData] LoadPersonalizedShop failed");
			yield return new JobFailedResult("[HearthstoneCheckout.GetPersonalizedShopData] LoadPersonalizedShop failed.");
		}
		_shouldCallCSDKUpdate = true;
	}

	public void RequestClose()
	{
		switch (m_TransactionState)
		{
		case State.Ready:
			((ISceneEventObserver)this).OnCancel();
			break;
		case State.InProgress_Backgroundable:
		case State.Finished:
			SignalCloseNextFrame();
			break;
		default:
			Log.Store.PrintWarning("[HearthstoneCheckout.RequestClose] HearthstoneCheckout received a request close when it should already be closed.  Attempting to close again...");
			SignalCloseNextFrame();
			break;
		case State.InProgress:
			break;
		}
	}

	public void CancelCurrentTransaction()
	{
		string transactionId = m_currentTransaction?.TransactionID;
		if (!string.IsNullOrEmpty(transactionId))
		{
			CommerceWrapper.Instance.CancelPurchase(transactionId);
			if (!IsIdle)
			{
				((IPurchaseEventObserver)this).OnCancel(m_currentTransaction);
			}
		}
	}

	public void SetClientID(string clientID)
	{
		m_clientID = clientID;
	}

	public void SetCurrencyCode(string currencyCode)
	{
		m_currencyCode = currencyCode;
	}

	public void ProcessDeeplink(string url)
	{
		Log.Store.PrintDebug("[HearthstoneCheckout] processing deeplink '" + url + "'");
		if (string.IsNullOrEmpty(url) || m_currentTransaction == null || !IsTransactionInProgress())
		{
			Log.Store.Print("invalid state for deep link {0}", url);
			return;
		}
		url = url.ToLower();
		if (url.EndsWith("nativepurchase"))
		{
			GenerateSSOToken generateToken = new GenerateSSOToken();
			string externalTransactionID = GenerateExternalTransactionID();
			IProductDataService dataService;
			ProductInfo product;
			string productClaimToken = ((!ServiceManager.TryGet<IProductDataService>(out dataService) || !dataService.TryGetProduct(m_currentTransaction.ProductID, out product)) ? string.Empty : product.ProductClaimToken);
			m_purchaseHandle = CommerceWrapper.Instance.PurchaseCheckout(m_currentTransaction.ProductID, m_currentTransaction.CurrencyCode, generateToken.Token, externalTransactionID, productClaimToken, useNativeFlow: true);
		}
		else if (url.EndsWith("canceledpurchase"))
		{
			((IPurchaseEventObserver)this).OnCancel(m_currentTransaction);
		}
		else if (url.EndsWith("finishedpurchase"))
		{
			((IPurchaseEventObserver)this).OnSuccessful(m_currentTransaction);
		}
		Log.Store.Print("Deep link recieved {0}", url);
	}

	private void OnHickoryPurchaseResponse()
	{
		if (!PlatformSettings.IsSteam)
		{
			Log.Store.PrintWarning("[HearthStoneCheckout.OnHickoryPurchaseResponse] Cannot run api outside of Hickory");
			return;
		}
		SteamPurchaseResponse packet = m_network.GetSteamPurchaseResponse();
		if (!IsTransactionInProgress())
		{
			Log.Store.PrintWarning("[HearthstoneCheckout.OnHickoryPurchaseResponse] Is not in a transaction");
			return;
		}
		if (packet == null)
		{
			Log.Store.PrintError("[HearthstoneCheckout.OnHickoryPurchaseResponse] Received null packet");
			return;
		}
		if (packet.Success)
		{
			Log.Store.PrintDebug("[HearthstoneCheckout.OnHickoryPurchaseResponse] Previous Operation Has Succeeded");
			return;
		}
		Log.Store.PrintError("[HearthstoneCheckout.OnHickoryPurchaseResponse] Previous Operation Has Failed");
		string transactionId = (packet.HasTransactionId ? packet.TransactionId.ToString() : null);
		TransactionData transaction = CalculateTransactionData(transactionId, "HICKORY400");
		((IPurchaseEventObserver)this).OnFailure(transaction);
	}

	private void OnGetHickoryUserInfoResponse()
	{
		if (!PlatformSettings.IsSteam)
		{
			Log.Store.PrintWarning("[HearthStoneCheckout.OnGetHickoryUserInfoResponse] Cannot run api outside of Hickory");
			return;
		}
		GetSteamUserInfoResponse getSteamUserInfoResponse = m_network.GetGetSteamUserInfoResponse();
		ProcessGetHickoryUserInfoResponse(getSteamUserInfoResponse);
		m_network.RemoveNetHandler(GetSteamUserInfoResponse.PacketID.ID, OnGetHickoryUserInfoResponse);
	}

	internal void ProcessGetHickoryUserInfoResponse(GetSteamUserInfoResponse getSteamUserInfoResponse)
	{
		if (!PlatformSettings.IsSteam)
		{
			Log.Store.PrintError("[HearthstoneCheckout.ProcessGetHickoryUserInfoResponse] Outside of Hickory");
			return;
		}
		if (getSteamUserInfoResponse == null)
		{
			Log.Store.PrintError("[HearthstoneCheckout.ProcessGetHickoryUserInfoResponse] Packet empty");
			return;
		}
		if (getSteamUserInfoResponse.HasSteamCurrencyCode)
		{
			m_steamCurrencyCode = getSteamUserInfoResponse.SteamCurrencyCode;
			Log.Store.PrintDebug("[HearthstoneCheckout.ProcessGetHickoryUserInfoResponse] Set currency code to {0}", m_steamCurrencyCode);
		}
		else
		{
			m_steamCurrencyCode = string.Empty;
			Log.Store.PrintWarning("[HearthstoneCheckout.ProcessGetHickoryUserInfoResponse] Currency code not set");
		}
		m_canMakeSteamPurchase = getSteamUserInfoResponse.SteamPurchasesAllowed;
		if (getSteamUserInfoResponse.HasUserStatus)
		{
			m_arePurchasesBlocked = getSteamUserInfoResponse.UserStatus.HasBlockedPurchase;
			m_isPendingBlocked = getSteamUserInfoResponse.UserStatus.HasPendingPurchase;
		}
	}

	private void OnExternalPurchaseUpdate()
	{
		if (!PlatformSettings.IsSteam)
		{
			Log.Store.PrintWarning("[HearthStoneCheckout.OnExternalPurchaseUpdate] Cannot run api outside of Hickory");
			return;
		}
		ExternalPurchaseUpdate packet = m_network.GetExternalPurchaseUpdate();
		ProcessExternalPurchaseUpdate(packet);
	}

	private void ProcessTransactionPurchaseUpdate(ExternalPurchaseUpdate packet)
	{
		if (!PlatformSettings.IsSteam)
		{
			Log.Store.PrintWarning("[HearthStoneCheckout.ProcessTransactionPurchaseUpdate] Cannot run api outside of Hickory");
			return;
		}
		if (!IsTransactionInProgress())
		{
			Log.Store.PrintError("[HearthstoneCheckout.ProcessTransactionPurchaseUpdate] No current transaction");
			return;
		}
		if (packet == null || !packet.HasPurchaseData)
		{
			Log.Store.PrintError("[HearthstoneCheckout.ProcessTransactionPurchaseUpdate] Received null packet");
			return;
		}
		if (packet.HasUserStatus)
		{
			m_isPendingBlocked = packet.UserStatus.HasPendingPurchase;
			m_arePurchasesBlocked = packet.UserStatus.HasBlockedPurchase;
		}
		if (string.IsNullOrEmpty(m_currentTransaction.TransactionID))
		{
			if (packet.PurchaseData.HasOrderId && packet.PurchaseData.PurchaseStatus == ExternalPurchaseData.ExternalPurchaseStatus.PURCHASE_STATUS_PENDING)
			{
				Log.Store.PrintDebug($"[HearthstoneCheckout.ProcessTransactionPurchaseUpdate] Setting order id to {packet.PurchaseData.OrderId}");
				m_currentTransaction.TransactionID = packet.PurchaseData.OrderId.ToString();
			}
			else if (packet.PurchaseData.PurchaseStatus == ExternalPurchaseData.ExternalPurchaseStatus.PURCHASE_STATUS_FAILED)
			{
				Log.Store.PrintDebug("[HearthstoneCheckout.ProcessTransactionPurchaseUpdate] Received purchase failure");
				m_currentTransaction.ErrorCodes = "HICKORY400";
				((IPurchaseEventObserver)this).OnFailure(m_currentTransaction);
			}
			else
			{
				Log.Store.PrintDebug($"[HearthstoneCheckout.ProcessTransactionPurchaseUpdate] Unknown purchase status {packet.PurchaseData.PurchaseStatus} for order {packet.PurchaseData.OrderId}");
			}
			return;
		}
		if (m_currentTransaction.TransactionID != packet.PurchaseData.OrderId.ToString())
		{
			Log.Store.Print(Blizzard.T5.Logging.LogLevel.Debug, verbose: true, $"[HearthstoneCheckout.ProcessTransactionPurchaseUpdate] Recieved update for {packet.PurchaseData.OrderId} instead of {m_currentTransaction.TransactionID}");
			return;
		}
		switch (packet.PurchaseData.PurchaseStatus)
		{
		case ExternalPurchaseData.ExternalPurchaseStatus.PURCHASE_STATUS_COMPLETE:
			Log.Store.PrintDebug("[HearthstoneCheckout.ProcessTransactionPurchaseUpdate] Transaction Complete");
			((IPurchaseEventObserver)this).OnSuccessful(m_currentTransaction);
			break;
		case ExternalPurchaseData.ExternalPurchaseStatus.PURCHASE_STATUS_FAILED:
			Log.Store.PrintDebug("[HearthstoneCheckout.ProcessTransactionPurchaseUpdate] Transaction Failed");
			m_currentTransaction.ErrorCodes = "HICKORY400";
			((IPurchaseEventObserver)this).OnFailure(m_currentTransaction);
			break;
		case ExternalPurchaseData.ExternalPurchaseStatus.PURCHASE_STATUS_UNKNOWN:
			Log.Store.PrintWarning($"[HearthstoneCheckout.ProcessTransactionPurchaseUpdate] Received unknown purchase status {packet.PurchaseData.PurchaseStatus} for order {packet.PurchaseData.OrderId}");
			break;
		default:
			Log.Store.Print(Blizzard.T5.Logging.LogLevel.Debug, verbose: true, $"[HearthstoneCheckout.ProcessTransactionPurchaseUpdate] Recieved update {packet.PurchaseData.PurchaseStatus} for {packet.PurchaseData.OrderId}");
			break;
		}
	}

	internal void ProcessExternalPurchaseUpdate(ExternalPurchaseUpdate packet)
	{
		if (!PlatformSettings.IsSteam)
		{
			Log.Store.PrintWarning("[HearthStoneCheckout.ProcessExternalPurchaseUpdate] Cannot run api outside of Hickory");
			return;
		}
		if (!IsTransactionInProgress())
		{
			Log.Store.PrintWarning("[HearthstoneCheckout.ProcessExternalPurchaseUpdate] Is not in a transaction");
			return;
		}
		if (packet == null)
		{
			Log.Store.PrintError("[HearthstoneCheckout.ProcessExternalPurchaseUpdate] Received null packet");
			return;
		}
		if (!packet.HasTimestamp || packet.Timestamp < m_lastTimeStamp)
		{
			Log.Store.Print(Blizzard.T5.Logging.LogLevel.Debug, verbose: true, "[HearthstoneCheckout.ProcessExternalPurchaseUpdate] Time stamp error");
			return;
		}
		m_lastTimeStamp = packet.Timestamp;
		if (!packet.HasPurchaseData || (!packet.PurchaseData.HasOrderId && !packet.PurchaseData.HasPurchaseStatus))
		{
			Log.Store.PrintWarning("[HearthstoneCheckout.ProcessExternalPurchaseUpdate] Received empty purchase status");
		}
		else
		{
			ProcessTransactionPurchaseUpdate(packet);
		}
	}

	private void InitializeNetworkServices(Network network)
	{
		if (network == null)
		{
			Log.Store.PrintError("[HearthstoneCheckout.InitializeNetworkServices] Network service is null");
			return;
		}
		m_network = network;
		m_network.RegisterNetHandler(InitialClientState.PacketID.ID, OnInitialClientState);
		if (PlatformSettings.IsSteam)
		{
			m_network.RegisterNetHandler(SteamPurchaseResponse.PacketID.ID, OnHickoryPurchaseResponse);
			m_network.RegisterNetHandler(ExternalPurchaseUpdate.PacketID.ID, OnExternalPurchaseUpdate);
			m_network.RegisterNetHandler(GetSteamUserInfoResponse.PacketID.ID, OnGetHickoryUserInfoResponse);
			m_network.RequestSteamUserInfo();
		}
	}

	private TransactionData CalculateTransactionData(string transactionId, string errorCode)
	{
		string currencyCode = GetCurrencyCodes()?.FirstOrDefault();
		TransactionData transaction = ((m_currentTransaction != null) ? m_currentTransaction : new TransactionData(ProductId.InvalidProduct, currencyCode, 1u, isVCPurchase: false));
		if (!string.IsNullOrEmpty(transactionId))
		{
			transaction.TransactionID = transactionId;
		}
		transaction.ErrorCodes = errorCode;
		return transaction;
	}

	private void ResetPurchaseState()
	{
		m_TransactionState = State.Ready;
		if (m_checkoutUI != null && m_checkoutUI.IsShown())
		{
			m_checkoutUI.Hide();
			m_checkoutUiState = State.Ready;
		}
		m_currentTransaction = null;
		OnTransactionProcessCompleted();
	}

	private void OnReset()
	{
		if (ServiceManager.TryGet<IProductDataService>(out var dataService))
		{
			dataService.ResetProducts();
		}
		IsSystemEnabled = false;
		m_loadProducts = State.Uninit;
		m_loadPageState = State.Uninit;
		m_csdkState = State.Initializing;
		m_TransactionState = State.Initializing;
		m_currentTransaction = null;
		m_clientID = null;
		_shouldCallCSDKUpdate = false;
		_isOpenCallbacks.Clear();
		m_virtualCurrencyRequests.Clear();
		DestroyCheckoutUI();
		DisposeCurrentCheckoutClient();
		DisposeListeners();
		string libraryError = "";
		try
		{
			commerceObserverImpl = new EventListenerObserverImpl(this);
			commerceObserverImpl.AddSceneObserver(this);
			commerceObserverImpl.AddCatalogObserver(this);
			commerceObserverImpl.AddPurchaseObserver(this);
			commerceObserverImpl.AddVirtualCurrencyObserver(this);
		}
		catch (Exception ex)
		{
			m_csdkState = State.Unavailable;
			libraryError = $"Failed to initialize HearthstoneCheckout: {ex}";
			TelemetryManager.Client().SendBlizzardCheckoutInitializationResult(success: false, "Checkout Library Exception.", ex.ToString());
		}
		if (!string.IsNullOrEmpty(libraryError))
		{
			TelemetryManager.Client().SendBlizzardCheckoutInitializationResult(success: false, "Commerce SDK Interface error.", libraryError);
			return;
		}
		JobDefinition loadCheckoutUIJob = new JobDefinition("HearthstoneCheckout.LoadCheckoutUI", Job_CreateCSDK(), JobFlags.StartImmediately, new WaitForGameDownloadManagerState());
		Processor.QueueJob(loadCheckoutUIJob);
		Processor.QueueJob(new JobDefinition("HearthstoneCheckout.InitializeCheckoutClient", Job_InitializeCheckoutClient(), loadCheckoutUIJob.CreateDependency(), new WaitForCheckoutConfiguration()));
	}

	private void OnInitialClientState()
	{
		InitialClientState packet = m_network.GetInitialClientState();
		if (packet != null && packet.HasGuardianVars)
		{
			IsSystemEnabled = packet.GuardianVars.ProductsFromCommerceEnabled;
		}
	}

	private string GetTitleVersionString()
	{
		string version = "32.0";
		string revision = "0";
		int build = 217964;
		string platform = GetPlatformString();
		return $"{version}.{revision}.{build}-{platform}";
	}

	private static string GetPlatformString()
	{
		return PlatformSettings.RuntimeOS switch
		{
			OSCategory.PC => "Windows", 
			OSCategory.Mac => "MacOS", 
			OSCategory.Android => GetAndroidPlatformString(), 
			OSCategory.iOS => GetIOSPlatformString(), 
			_ => "UnknownOS", 
		};
	}

	private static string GetAndroidPlatformString()
	{
		return AndroidDeviceSettings.Get().GetAndroidStore() switch
		{
			AndroidStore.GOOGLE => "Google", 
			AndroidStore.AMAZON => "Amazon", 
			AndroidStore.BLIZZARD => "AndroidBattlenet", 
			AndroidStore.HUAWEI => "Huawei", 
			AndroidStore.DASHEN => "Dashen", 
			AndroidStore.ONE_STORE => "OneStore", 
			_ => "UnkownAndroid", 
		};
	}

	private static string GetIOSPlatformString()
	{
		if (PlatformSettings.LocaleVariant != LocaleVariant.China)
		{
			return "iOS";
		}
		return "iOSCN";
	}

	private string GenerateExternalTransactionID()
	{
		if (PlatformSettings.IsSteam)
		{
			return string.Empty;
		}
		if (!CommerceWrapper.Instance.IsValid)
		{
			Log.Store.PrintError("[HearthstoneCheckout.GenerateExternalTransactionID] Checkout Client must exists to generate an external transaction ID.");
			return null;
		}
		BnetRegion region = BattleNet.GetAccountRegion();
		if ((uint)(region - 1) > 4u)
		{
			region = BnetRegion.REGION_PTR;
		}
		return CommerceWrapper.Instance.GenerateTransactionID(14, (int)region);
	}

	private static string GetBrowserPath()
	{
		if (Application.platform == RuntimePlatform.OSXPlayer)
		{
			return Application.dataPath + "/../../BlizzardBrowser";
		}
		if ((Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer) && !Application.isEditor)
		{
			return Application.dataPath;
		}
		CommerceConfig config = CommerceConfig.RetrieveConfig();
		if (config == null)
		{
			Debug.LogErrorFormat("Could not retrieve the commerce config file! We can not load the checkout window appropriately and sales will not be possible.");
			return Application.dataPath;
		}
		return config.RuntimeDataPath;
	}

	private void ScreenResolutionUpdate()
	{
		if (m_checkoutUI != null && m_checkoutUI.IsShown() && (m_screenResolution.x != (float)Screen.width || m_screenResolution.y != (float)Screen.height))
		{
			m_checkoutUI.DetermineBrowserSize();
			Log.Store.PrintDebug("Browser Width: {0}\nBrowser Height: {1}", m_checkoutUI.BrowserWidth, m_checkoutUI.BrowserHeight);
			if (!CommerceWrapper.Instance.SendResizeEvent(m_checkoutUI.BrowserWidth, m_checkoutUI.BrowserHeight))
			{
				Log.Store.PrintWarning("[HearthstoneCheckout.ScreenResolutionUpdate] Unable to send resize event");
			}
			m_screenResolution.x = Screen.width;
			m_screenResolution.y = Screen.height;
		}
	}

	private void UpdateTransactionData(TransactionData response)
	{
		if (m_currentTransaction == null)
		{
			Log.Store.PrintDebug($"[UpdateTransactionData] Unable to update transaction, no transaction set response:{response}");
		}
		else if (response != null && response.ErrorCodes != null)
		{
			m_currentTransaction.ErrorCodes = response.ErrorCodes;
		}
	}

	private void LogPurchaseResponse(string tag, TransactionData data)
	{
		Log.Store.PrintDebug("{0} Status - {1}", tag, data?.Status);
		if (!string.IsNullOrEmpty(data?.ErrorCodes))
		{
			Log.Store.PrintError("[HearthstoneCheckout] CHECKOUT ERROR: {0}", data.ErrorCodes);
		}
	}

	private void SignalCloseNextFrame()
	{
		m_closeRequested = true;
	}

	private void ClearTransaction()
	{
		if (IsUIShown)
		{
			m_checkoutUI.Hide();
			m_checkoutUiState = State.Ready;
		}
		m_closeRequested = false;
		m_currentTransaction = null;
		m_TransactionState = State.Ready;
		_shouldCallCSDKUpdate = true;
	}

	private void DestroyCheckoutUI()
	{
		if (m_checkoutUI != null && m_checkoutUI.gameObject != null)
		{
			UnityEngine.Object.Destroy(m_checkoutUI.gameObject);
			m_checkoutUI = null;
			m_checkoutUiState = State.Uninit;
		}
	}

	private void OnTransactionProcessCompleted()
	{
		if (!IsUIShown)
		{
			SignalCloseNextFrame();
		}
	}

	private void OnPurchaseJobFinished(JobDefinition job, bool success)
	{
		if (!success && m_TransactionState == State.Ready)
		{
			StoreManager.Get()?.HandleCommerceSubmitFailure();
			RequestClose();
		}
	}

	private void OnOutsideClick()
	{
		if (StoreManager.Get().CanTapOutConfirmationUI())
		{
			m_TransactionState = State.Finished;
			m_currentTransaction = null;
			RequestClose();
		}
	}

	private void DisposeCurrentCheckoutClient()
	{
		if (m_purchaseHandle != null)
		{
			m_purchaseHandle.Dispose();
			m_purchaseHandle = null;
		}
	}

	private void DisposeListeners()
	{
		commerceObserverImpl.RemoveSceneObserver(this);
		commerceObserverImpl.RemoveCatalogObserver(this);
		commerceObserverImpl.RemovePurchaseObserver(this);
		commerceObserverImpl.RemoveVirtualCurrencyObserver(this);
		CommerceWrapper.Instance.Dispose();
	}

	private bool IsTransactionInProgress()
	{
		if (m_currentTransaction != null)
		{
			if (m_TransactionState != State.InProgress)
			{
				return m_TransactionState == State.InProgress_Backgroundable;
			}
			return true;
		}
		return false;
	}

	private static Ecosystems GetEcosystem()
	{
		if (PlatformSettings.IsSteam)
		{
			return (Ecosystems)9;
		}
		switch (Application.platform)
		{
		case RuntimePlatform.IPhonePlayer:
			return Ecosystems.IOS_VALUE;
		case RuntimePlatform.Android:
			switch (AndroidDeviceSettings.Get().GetAndroidStore())
			{
			case AndroidStore.GOOGLE:
			case AndroidStore.HUAWEI:
				return Ecosystems.ANDROID_VALUE;
			case AndroidStore.AMAZON:
				return Ecosystems.AMAZON_VALUE;
			case AndroidStore.BLIZZARD:
			case AndroidStore.DASHEN:
				Log.Store.PrintWarning("[HearthstoneCheckout.GetEcosystem] returning Ecosystems.BATTLE_NET_VALUE");
				return Ecosystems.BATTLE_NET_VALUE;
			case AndroidStore.ONE_STORE:
				return Ecosystems.ONE_STORE_VALUE;
			default:
				return Ecosystems.UNKNOWN_ECOSYSTEM_VALUE;
			}
		case RuntimePlatform.OSXEditor:
		case RuntimePlatform.OSXPlayer:
		case RuntimePlatform.WindowsPlayer:
		case RuntimePlatform.WindowsEditor:
		case RuntimePlatform.LinuxPlayer:
		case RuntimePlatform.LinuxEditor:
			return Ecosystems.BATTLE_NET_VALUE;
		default:
			return Ecosystems.UNKNOWN_ECOSYSTEM_VALUE;
		}
	}

	private void HandleCommerceError()
	{
		if (m_currentTransaction != null && !string.IsNullOrEmpty(m_currentTransaction.ErrorCodes) && m_currentTransaction.ErrorCodes.Contains("4097"))
		{
			Log.Store.Print("[HearthstoneCheckout.HandleCommerceError] Failed to purchase product, calling ResumeCheckout" + $" product id: {m_currentTransaction.ProductID}," + " transaction id: " + m_currentTransaction.TransactionID + ", error: " + m_currentTransaction.ErrorCodes);
			Processor.RunCoroutine(ResumeCheckout());
		}
	}

	private IEnumerator ResumeCheckout()
	{
		yield return new WaitUntil(() => CommerceWrapper.Instance.HasLoadedCatalog && CommerceWrapper.Instance.IsIdle);
		if (!CommerceWrapper.Instance.ResumeCommerceAPI())
		{
			Log.Store.PrintWarning("[HearthstoneCheckout.ProductsLoaded] ResumeCommerceAPI failed.");
		}
		else
		{
			Log.Store.Print("[HearthstoneCheckout.ResumeCheckout] ResumeCommerceAPI called.");
		}
	}

	private IEnumerator<IAsyncJobResult> Job_ShowCheckout(ProductId productID, string currencyCode, uint quantity)
	{
		if (!CommerceWrapper.Instance.IsValid)
		{
			yield return new JobFailedResult("[HearthstoneCheckout.ShowCheckout] Cannot show checkout because the checkout client isn't available.");
		}
		if (m_checkoutUI == null)
		{
			yield return new JobFailedResult("[HearthstoneCheckout.ShowCheckout] Cannot show checkout because the UI isn't loaded.");
		}
		if (!ServiceManager.TryGet<IProductDataService>(out var dataService))
		{
			yield return new JobFailedResult("[HearthstoneCheckout.ShowCheckout] Cannot show checkout because data service is not available.");
		}
		m_checkoutUiState = State.Startup;
		Log.Store.PrintDebug("[HearthstoneCheckout.ShowCheckout] Started");
		m_elapsedTimeSinceResolutionCheck = 0f;
		m_elapsedTimeSinceShown = 0f;
		m_checkoutUI.GenerateMeshes();
		yield return new WaitForLogin();
		GenerateSSOToken generateToken = new GenerateSSOToken();
		yield return generateToken;
		m_currentTransaction = new TransactionData(productID, currencyCode, quantity, isVCPurchase: false)
		{
			TransactionID = GenerateExternalTransactionID()
		};
		_shouldCallCSDKUpdate = false;
		yield return new WaitForIdle();
		m_TransactionState = State.InProgress;
		ProductInfo product;
		string productClaimToken = ((!dataService.TryGetProduct(m_currentTransaction.ProductID, out product)) ? string.Empty : product.ProductClaimToken);
		bool forceNativeFlow = NetCache.Get().GetNetObject<NetCache.NetCacheFeatures>().BattlenetBillingFlowDisableOverride;
		m_purchaseHandle = CommerceWrapper.Instance.PurchaseCheckout(m_currentTransaction.ProductID, m_currentTransaction.CurrencyCode, generateToken.Token, m_currentTransaction.TransactionID, productClaimToken, forceNativeFlow);
		_shouldCallCSDKUpdate = true;
		if (!PlatformSettings.IsSteam && m_purchaseHandle == null)
		{
			Log.Store.PrintError("[HearthstoneCheckout.ShowCheckout] Failed to obtain purchase handle.");
			yield return new JobFailedResult("[HearthstoneCheckout.ShowCheckout] Failed to obtain purchase handle.");
		}
		Log.Store.PrintDebug("[HearthstoneCheckout.ShowCheckout]Purchase was successfully initiated.");
	}

	private IEnumerator<IAsyncJobResult> Job_PurchaseWithVirtualCurrency(ProductId productID, string currencyCode, uint quantity)
	{
		m_TransactionState = State.Startup;
		if (!CommerceWrapper.Instance.HasLoadedCatalog)
		{
			m_TransactionState = State.Ready;
			yield return new JobFailedResult("[HearthstoneCheckout.PurchaseWithVirtualCurrency] Cannot initiate purchase because catalog has not loaded.");
		}
		GenerateSSOToken generateSSOToken = new GenerateSSOToken();
		yield return generateSSOToken;
		if (!generateSSOToken.HasToken)
		{
			m_TransactionState = State.Ready;
			yield return new JobFailedResult("[HearthstoneCheckout.PurchaseWithVirtualCurrency] Cannot show checkout because it didn't receive an SSO token.");
		}
		yield return new WaitForLogin();
		m_TransactionState = State.InProgress;
		m_currentTransaction = new TransactionData(productID, currencyCode, quantity, isVCPurchase: true)
		{
			TransactionID = GenerateExternalTransactionID()
		};
		_shouldCallCSDKUpdate = false;
		yield return new WaitForIdle();
		IProductDataService dataService;
		ProductInfo product;
		string productClaimsToken = ((!ServiceManager.TryGet<IProductDataService>(out dataService) || !dataService.TryGetProduct(m_currentTransaction.ProductID, out product)) ? string.Empty : product.ProductClaimToken);
		PlaceOrderWithVCRequest purchaseRequest = new PlaceOrderWithVCRequest
		{
			currencyCode = m_currentTransaction.CurrencyCode,
			externalTransactionId = m_currentTransaction.TransactionID,
			gameServiceRegionId = (int)BattleNet.GetCurrentRegion(),
			productId = (int)m_currentTransaction.ProductID.Value,
			quantity = (int)m_currentTransaction.Quantity,
			ecosystem = (int)GetEcosystem(),
			pointOfSaleId = 1465140039u,
			productClaimsToken = productClaimsToken
		};
		if (!CommerceWrapper.Instance.PurchaseWithVC(purchaseRequest))
		{
			_shouldCallCSDKUpdate = true;
			Log.Store.PrintWarning("[HearthstoneCheckout.Job_PurchaseWithVirtualCurrency] PurchaseVC failed.");
			yield return new JobFailedResult("[HearthstoneCheckout.PurchaseWithVirtualCurrency] Purchase with VC failed from CSDK");
		}
		_shouldCallCSDKUpdate = true;
	}

	private IEnumerator<IAsyncJobResult> Job_CreateCSDK()
	{
		if (m_checkoutUI != null)
		{
			Log.Store.PrintError("[HearthstoneCheckout.Job_CreateCSDK] Checkout UI already exists!  Please destroy the existing UI before creating a new one.");
			yield break;
		}
		while (!Network.IsLoggedIn())
		{
			yield return null;
		}
		m_checkoutUiState = State.Initializing;
		InstantiatePrefab loadCheckoutUI = new InstantiatePrefab("HearthstoneCheckout.prefab:da1b8fa18876ab5468bd2aa04a3f2539");
		yield return loadCheckoutUI;
		m_checkoutUI = loadCheckoutUI.InstantiatedPrefab.GetComponent<HearthstoneCheckoutUI>();
		loadCheckoutUI.InstantiatedPrefab.AddComponent<HSDontDestroyOnLoad>();
		m_checkoutUI.Hide();
		m_checkoutUI.DetermineBrowserSize();
		m_checkoutUiState = State.Ready;
		while (string.IsNullOrEmpty(m_clientID))
		{
			yield return null;
		}
		GenerateSSOToken generateSSOToken = new GenerateSSOToken();
		yield return generateSSOToken;
		if (!generateSSOToken.HasToken)
		{
			TelemetryManager.Client().SendBlizzardCheckoutInitializationResult(success: false, "CommerceWrapper.InitListener Failed", "SSO Token failed");
			yield return new JobFailedResult("[HearthstoneCheckout.Job_CreateCSDK] Cannot show checkout because it didn't receive an SSO token.");
		}
		m_checkoutUI.AddOutsideClickListener(OnOutsideClick);
		Vec2D browserSize = new Vec2D(CheckoutUi.BrowserWidth, CheckoutUi.BrowserHeight);
		CommerceWrapper.Instance.VerboseLogging = false;
		Blizzard.Commerce.Environment httpEnvironment = (HearthstoneApplication.IsPublic() ? Blizzard.Commerce.Environment.PROD : Blizzard.Commerce.Environment.QA);
		CommerceWrapper.Instance.Runner = Processor.RunCoroutine;
		RequestedModules modules = RequestedModules.CATALOG | RequestedModules.VIRTUAL_CURRENCY;
		if (!PlatformSettings.IsSteam)
		{
			modules |= RequestedModules.CHECKOUT;
		}
		modules |= RequestedModules.HTTP | RequestedModules.SCENE;
		m_accountInfo = new AccountInitializationValues
		{
			Region = (int)BattleNet.GetCurrentRegion(),
			Locale = Localization.GetBnetLocaleName(),
			AccountId = string.Empty
		};
		m_systemInfo = new SystemInitializationValues
		{
			TitleCode = "WTCG",
			TitleVersion = GetTitleVersionString(),
			ClientId = m_clientID,
			IsProduction = HearthstoneApplication.IsPublic(),
			OverrideProduction = (m_overrideEndpointToProduction ?? HearthstoneApplication.IsPublic()),
			DeviceId = SystemInfo.deviceUniqueIdentifier,
			BrowserPath = GetBrowserPath(),
			CheckoutURL = $"https://nydus-qa.web.blizzard.net/Bnet/{Localization.GetLocaleName()}/client/checkout",
			MaxBrowserSize = new Vec2D(Screen.width, Screen.height),
			LogDir = Log.LogsPath,
			Ecosystem = GetEcosystem(),
			StoreId = 6,
			IsLegacyStyle = true,
			HttpEnvironment = httpEnvironment,
			BrowserEnvironment = httpEnvironment
		};
		Log.Store.Print($"[HearthstoneCheckout.Job_CreateCSDK] Platform: {Application.platform}");
		Log.Store.Print("[HearthstoneCheckout.Job_CreateCSDK] App Data Path: " + Application.dataPath);
		Log.Store.Print("[HearthstoneCheckout.Job_CreateCSDK] Browser Path: " + m_systemInfo.BrowserPath);
		Log.Store.Print("[HearthstoneCheckout.Job_CreateCSDK] TitleVersionString: " + m_systemInfo.TitleVersion);
		if (!CommerceWrapper.Instance.InitListener(commerceObserverImpl, generateSSOToken, modules, m_systemInfo, m_accountInfo, browserSize, this))
		{
			TelemetryManager.Client().SendBlizzardCheckoutInitializationResult(success: false, "CommerceWrapper.InitListener Failed", "CSDK error");
			Log.Store.PrintError("[HearthstoneCheckout.Job_CreateCSDK]: CommerceWrapper.InitListener Failed");
			yield return new JobFailedResult("[HearthstoneCheckout.Job_CreateCSDK] The commerce SDK failed to initialize internally!");
		}
		Log.Store.PrintDebug("[HearthstoneCheckout.Job_CreateCSDK] CSDK is now Ready");
		m_csdkState = State.Finished;
		m_ssoTokenState = State.Finished;
		m_checkoutUI?.HandleCommerceReadyEvent();
		TelemetryManager.Client().SendBlizzardCheckoutIsReady(ShownTime, isReady: true);
		while (!StoreManager.Get().BattlePayAvailable)
		{
			yield return null;
		}
		_shouldCallCSDKUpdate = true;
		ServiceManager.Get<IProductDataService>().RefreshShopPageRequests();
	}

	private IEnumerator<IAsyncJobResult> Job_GenerateSSOToken()
	{
		m_ssoTokenState = State.Initializing;
		GenerateSSOToken generateSSOToken = new GenerateSSOToken();
		yield return generateSSOToken;
		if (!generateSSOToken.HasToken)
		{
			m_ssoTokenState = State.Unavailable;
			yield return new JobFailedResult("[HearthstoneCheckout.CreateCheckoutClient] Didn't receive a SSO token from request.");
		}
		else
		{
			m_ssoTokenState = State.Finished;
		}
		yield return new WaitForLogin();
		if (m_ssoTokenState == State.Unavailable)
		{
			m_ssoTokenState = State.Initializing;
		}
		Log.Store.PrintDebug("[HearthstoneCheckout.CreateCheckoutClient] Scene Checkout was successfully created.");
	}

	private IEnumerator<IAsyncJobResult> Job_InitializeCheckoutClient()
	{
		m_retriesRemaining = 3;
		bool success = false;
		while (!success && m_retriesRemaining-- > 0)
		{
			Log.Store.PrintDebug("[HearthstoneCheckout.InitializeCheckoutClient] Creating client");
			yield return new JobDefinition("HearthstoneCheckout.InitializeCheckoutClient", Job_GenerateSSOToken(), new WaitForLogin());
			Log.Store.PrintDebug("[HearthstoneCheckout.InitializeCheckoutClient] Client response: {0}", m_ssoTokenState);
			if (m_ssoTokenState == State.Initializing)
			{
				yield return new WaitForClientInitializationResponse(this, 60f);
			}
			switch (m_ssoTokenState)
			{
			case State.Initializing:
				Log.Store.PrintError("[HearthstoneCheckout.InitializeCheckoutClient] Client timed out");
				TelemetryManager.Client().SendBlizzardCheckoutInitializationResult(success: false, "Checkout Client Token Initialization Timeout", $"Attempt {3 - m_retriesRemaining} of {3}");
				break;
			case State.Finished:
				Log.Store.PrintDebug("[HearthstoneCheckout.InitializeCheckoutClient] Client Token initialized");
				TelemetryManager.Client().SendBlizzardCheckoutInitializationResult(success: true, "", "");
				success = true;
				break;
			case State.Unavailable:
				Log.Store.PrintError("[HearthstoneCheckout.InitializeCheckoutClient] Client Token failed");
				TelemetryManager.Client().SendBlizzardCheckoutInitializationResult(success: false, "Checkout Client Initialization Unsuccessful", "");
				break;
			default:
				Log.Store.PrintError("[HearthstoneCheckout.InitializeCheckoutClient] Unrecognized initialization response: {0}", m_ssoTokenState);
				break;
			}
		}
		if (success)
		{
			m_TransactionState = State.Ready;
			yield break;
		}
		m_TransactionState = State.Unavailable;
		m_csdkState = State.Unavailable;
		yield return new JobFailedResult("[HearthstoneCheckout.InitializeCheckoutClient] Failed to initialize checkout client.");
	}

	void ISceneEventObserver.OnReady()
	{
		Log.Store.PrintDebug("[HearthstoneCheckout.OnReady] Showing checkout UI");
		m_checkoutUI.InitiateCheckout(this);
		m_checkoutUI.Show();
		m_checkoutUiState = State.InProgress;
	}

	void ISceneEventObserver.OnDisconnect()
	{
		Log.Store.PrintDebug("[HearthstoneCheckout.OnDisconnect]");
		m_checkoutUiState = State.Ready;
		SignalCloseNextFrame();
	}

	void ISceneEventObserver.OnCancel()
	{
		Log.Store.PrintDebug("[HearthstoneCheckout.OnCancel]");
		SignalCloseNextFrame();
		StoreManager.Get()?.HandleCommerceCancelEvent();
		ResetPurchaseState();
		TelemetryManager.Client().SendBlizzardCheckoutIsReady(ShownTime, isReady: false);
	}

	void ISceneEventObserver.OnWindowResize(int sizeX, int sizeY)
	{
		Log.Store.PrintDebug("[HearthstoneCheckout.OnWindowResized] (x:{0}, y:{1})", sizeX, sizeY);
		if (m_checkoutUI != null)
		{
			m_checkoutUI.ResizeTexture(sizeX, sizeY);
		}
	}

	void ISceneEventObserver.OnBufferUpdate(byte[] data)
	{
		Log.Store.PrintDebug("[HearthstoneCheckout.OnBufferUpdate]");
		if (m_checkoutUI != null)
		{
			m_checkoutUI.UpdateTexture(data);
		}
	}

	void ISceneEventObserver.OnWindowResizeRequested(int requestX, int requestY)
	{
		Log.Store.PrintDebug("[HearthstoneCheckout.OnWindowResizeRequested] Requested Size (x: {0}, y:{1})", requestX, requestY);
	}

	void ISceneEventObserver.OnWindowCloseRequest()
	{
		Log.Store.PrintDebug("[HearthstoneCheckout.OnWindowCloseRequested]");
		SignalCloseNextFrame();
	}

	void ISceneEventObserver.OnCursorChanged()
	{
	}

	void ISceneEventObserver.OnExternalLink(string url)
	{
		Log.Store.PrintDebug("[HearthstoneCheckout.OnExternalLink] URL: {0}", url);
		Application.OpenURL(url);
	}

	void ISceneEventObserver.OnImeCompositionRangeChanged(int from, int to)
	{
	}

	void ISceneEventObserver.OnImeStateChanged()
	{
	}

	void ISceneEventObserver.OnImeCompositionCanceled()
	{
	}

	void ISceneEventObserver.OnImeTextSelectionChanged(string text, int offset, int from, int to)
	{
	}

	void ISceneEventObserver.OnImeTextBoundsChanged(bool isAnchorRect, Rect2D rect)
	{
	}

	void ISceneEventObserver.OnScenePageLoad(string url, long resultCode, string loadType, string browserType)
	{
	}

	void IPurchaseEventObserver.OnCancel(TransactionData data)
	{
		Log.Store.PrintInfo("[HearthstoneCheckout.IPurchaseEventObserver.OnCancel]");
		((ISceneEventObserver)this).OnCancel();
		StoreManager.Get()?.HandleCommerceCancelEvent();
		ResetPurchaseState();
		TelemetryManager.Client().SendBlizzardCheckoutIsReady(ShownTime, isReady: false);
	}

	void IPurchaseEventObserver.OnFailure(TransactionData data)
	{
		if (m_TransactionState != State.InProgress_Backgroundable && m_TransactionState != State.InProgress)
		{
			LogPurchaseResponse("[HearthstoneCheckout.OnOrderFailure: Canceled Before Response]", data);
			return;
		}
		LogPurchaseResponse("[HearthstoneCheckout.OnFailure]", data);
		UpdateTransactionData(data);
		StoreManager.Get()?.HandleCommerceOrderFailure(m_currentTransaction);
		HandleCommerceError();
		ResetPurchaseState();
	}

	void IPurchaseEventObserver.OnSuccessful(TransactionData data)
	{
		if (m_TransactionState != State.InProgress_Backgroundable && m_TransactionState != State.InProgress)
		{
			LogPurchaseResponse("[HearthstoneCheckout.OnOrderComplete: Canceled Before Response]", data);
			return;
		}
		LogPurchaseResponse("[HearthstoneCheckout.OnSuccessful]", data);
		UpdateTransactionData(data);
		StoreManager.Get()?.HandleCommerceOrderComplete(m_currentTransaction);
		ResetPurchaseState();
	}

	void IPurchaseEventObserver.OnPending(TransactionData data, bool isCancelable)
	{
		if (m_TransactionState != State.InProgress_Backgroundable && m_TransactionState != State.InProgress)
		{
			LogPurchaseResponse("[HearthstoneCheckout.OnPending: Canceled Before Response]", data);
			return;
		}
		LogPurchaseResponse("[HearthstoneCheckout.OnPending]", data);
		if (!isCancelable || m_TransactionState != State.InProgress_Backgroundable)
		{
			m_TransactionState = State.InProgress;
		}
		m_transactionStart = DateTime.Now;
		UpdateTransactionData(data);
		StoreManager.Get()?.HandleCommerceOrderPending(m_currentTransaction);
	}

	void IVirtualCurrencyEventObserver.OnPurchaseEvent(bool isError, State state, string errorCode)
	{
		State nextState = m_TransactionState;
		if (isError)
		{
			m_TransactionState = State.Ready;
			Log.Store.PrintError("[HearthstoneCheckout.OnVirtualCurrencyResponse] Http error occurred: {0}", errorCode);
			StoreManager.Get()?.HandleCommerceOrderFailure(m_currentTransaction);
			return;
		}
		switch (state)
		{
		case State.InProgress:
			nextState = State.InProgress;
			break;
		case State.Finished:
			nextState = State.Finished;
			break;
		default:
			isError = true;
			nextState = State.Finished;
			m_currentTransaction.ErrorCodes = errorCode;
			Log.Store.PrintError("[HearthstoneCheckout.OnVirtualCurrencyResponse] OrderWithVCRequest failed: {0}", errorCode);
			break;
		}
		if (!IsTransactionInProgress())
		{
			Log.Store.PrintDebug("[HearthstoneCheckout.OnVirtualCurrencyResponse: Canceled Before Response] Status: {0}, Response: {1}", state, errorCode);
		}
		else
		{
			if (m_TransactionState == nextState)
			{
				return;
			}
			m_TransactionState = nextState;
			if (m_TransactionState == State.Finished)
			{
				if (!isError)
				{
					StoreManager.Get()?.HandleCommerceOrderComplete(m_currentTransaction);
				}
				else
				{
					StoreManager.Get()?.HandleCommerceOrderFailure(m_currentTransaction);
				}
				OnTransactionProcessCompleted();
				m_TransactionState = State.Ready;
			}
		}
	}

	void IVirtualCurrencyEventObserver.OnGetBalance(bool isError, string errorCode, CurrencyBalance balance)
	{
		if (isError)
		{
			Log.Store.PrintError("[HearthstoneCheckout.OnGetBalance]There was an error with the virtual currency 'GetBalance' call! (Http Result Status: {0}", errorCode);
			return;
		}
		Log.Store.PrintInfo("[HearthstoneCheckout.OnGetBalance] Received balance response.  Currency - {0}   Balance - {1}", balance.CurrencyCode, balance.Balance);
		if (m_virtualCurrencyRequests.Count <= 0)
		{
			return;
		}
		VirtualCurrencyBalanceResult result = new VirtualCurrencyBalanceResult(balance.IsOk, errorCode, balance.CurrencyCode, (long)balance.Balance);
		int i = 0;
		while (i < m_virtualCurrencyRequests.Count)
		{
			if (m_virtualCurrencyRequests[i].currencyCode == balance.CurrencyCode)
			{
				m_virtualCurrencyRequests[i].callback?.Invoke(result);
				m_virtualCurrencyRequests.RemoveAt(i);
			}
			else
			{
				i++;
			}
		}
	}

	void ICatalogEventObserver.ProductsLoaded(IList<Product> products, float deserializeDuration)
	{
		if (!ServiceManager.TryGet<IProductDataService>(out var dataService))
		{
			return;
		}
		float loadProductsDuration = Time.realtimeSinceStartup - m_loadProductsStartTime;
		TelemetryManager.Client().SendLoadProducts(loadProductsDuration, deserializeDuration);
		if (products == null)
		{
			m_loadProducts = State.Unavailable;
			Log.Store.PrintError("Received a product from server that was not defined!");
			return;
		}
		foreach (Product curProduct in products)
		{
			if (!dataService.SetProduct(curProduct))
			{
				Log.Store.PrintError($"The product received had invalid product ID ({curProduct.productId}).");
			}
		}
		m_loadProducts = State.Finished;
		_shouldCallCSDKUpdate = false;
		Processor.RunCoroutine(ResumeCheckout());
		_shouldCallCSDKUpdate = true;
		Processor.QueueJob("HearthstoneCheckout.InvokeIsOpenCallbacks", InvokeIsOpenCallbacks(), JobFlags.StartImmediately);
	}

	private IEnumerator<IAsyncJobResult> InvokeIsOpenCallbacks()
	{
		StoreManager manager;
		do
		{
			manager = StoreManager.Get();
		}
		while (manager == null);
		while (!manager.IsOpen())
		{
			yield return null;
		}
		while (_isOpenCallbacks.Count() > 0)
		{
			_isOpenCallbacks.Dequeue()();
			yield return null;
		}
	}

	void ICatalogEventObserver.PersonalizedShopReceived(IList<Placement> placements, RpcError error)
	{
		Log.Store.PrintInfo("[HearthstoneCheckout.OnGetPersonalizedShopEvent] Received shop personalization data.");
		try
		{
			ServiceManager.Get<IProductDataService>().SetPersonalizedShopData(placements, error);
			m_loadPageState = State.Finished;
		}
		catch (Exception ex)
		{
			Log.Store.PrintError(ex.Message);
			m_loadPageState = State.Unavailable;
		}
	}

	public override void OnLogEvent(IntPtr owner, CommerceLogLevel logLevel, string subsystem, string message)
	{
		Blizzard.T5.Logging.LogLevel level = ConvertCommerceLogToILoggerLevel(logLevel);
		Log.Store.Print(level, logLevel == CommerceLogLevel.NOISE, "[COMMMERCE(" + subsystem + ")] " + message);
	}

	private static Blizzard.T5.Logging.LogLevel ConvertCommerceLogToILoggerLevel(CommerceLogLevel logLevel)
	{
		return logLevel switch
		{
			CommerceLogLevel.FATAL => Blizzard.T5.Logging.LogLevel.Exception, 
			CommerceLogLevel.ERROR => Blizzard.T5.Logging.LogLevel.Error, 
			CommerceLogLevel.WARNING => Blizzard.T5.Logging.LogLevel.Warning, 
			CommerceLogLevel.INFO => Blizzard.T5.Logging.LogLevel.Info, 
			_ => Blizzard.T5.Logging.LogLevel.Debug, 
		};
	}
}
