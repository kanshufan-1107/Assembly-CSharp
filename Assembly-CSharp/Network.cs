using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using Blizzard.BlizzardErrorMobile;
using Blizzard.Commerce;
using Blizzard.GameService.Protocol.V2.Client;
using Blizzard.GameService.SDK.Client.Integration;
using Blizzard.T5.Configuration;
using Blizzard.T5.Core;
using Blizzard.T5.Core.Utils;
using Blizzard.T5.Jobs;
using Blizzard.T5.Logging;
using Blizzard.T5.Network;
using Blizzard.T5.Services;
using Blizzard.Telemetry.WTCG.Client;
using BobNetProto;
using Hearthstone;
using Hearthstone.BreakingNews;
using Hearthstone.Commerce;
using Hearthstone.Devices;
using Hearthstone.Login;
using Hearthstone.Networking.BattleNet;
using Hearthstone.Streaming;
using Hearthstone.Util;
using HSCachedDeckCompletion;
using Networking;
using PegasusGame;
using PegasusLettuce;
using PegasusLuckyDraw;
using PegasusShared;
using PegasusUtil;
using Shared.Scripts.Game.Shop.Product;
using Shared.Scripts.Util.ValueTypes;
using UnityEngine;

public class Network : IHasUpdate, IService
{
	private class HSClientInterface : ClientInterface
	{
		private string s_tempCachePath = Application.temporaryCachePath;

		public string GetVersion()
		{
			return Network.GetVersion();
		}

		public string GetUserAgent()
		{
			string userAgent = "Hearthstone/";
			userAgent = userAgent + "31.6." + 216423 + " (";
			userAgent = ((PlatformSettings.OS == OSCategory.iOS) ? (userAgent + "iOS;") : ((PlatformSettings.OS == OSCategory.Android) ? (userAgent + "Android;") : ((PlatformSettings.OS == OSCategory.PC) ? (userAgent + "PC;") : ((PlatformSettings.OS != OSCategory.Mac) ? (userAgent + "UNKNOWN;") : (userAgent + "Mac;")))));
			userAgent = userAgent + CleanUserAgentString(SystemInfo.deviceModel) + ";" + SystemInfo.deviceType.ToString() + ";" + CleanUserAgentString(SystemInfo.deviceUniqueIdentifier) + ";" + SystemInfo.graphicsDeviceID + ";" + CleanUserAgentString(SystemInfo.graphicsDeviceName) + ";" + CleanUserAgentString(SystemInfo.graphicsDeviceVendor) + ";" + SystemInfo.graphicsDeviceVendorID + ";" + CleanUserAgentString(SystemInfo.graphicsDeviceVersion) + ";" + SystemInfo.graphicsMemorySize + ";" + SystemInfo.graphicsShaderLevel + ";" + SystemInfo.npotSupport.ToString() + ";" + CleanUserAgentString(SystemInfo.operatingSystem) + ";" + SystemInfo.processorCount + ";" + CleanUserAgentString(SystemInfo.processorType) + ";" + SystemInfo.supportedRenderTargetCount + ";" + SystemInfo.supports3DTextures + ";" + SystemInfo.supportsAccelerometer + ";" + SystemInfo.supportsComputeShaders + ";" + SystemInfo.supportsGyroscope + ";" + SystemInfo.supportsImageEffects + ";" + SystemInfo.supportsInstancing + ";" + SystemInfo.supportsRenderTextures + ";" + SystemInfo.supportsRenderToCubemap + ";" + SystemInfo.supportsShadows + ";" + SystemInfo.supportsSparseTextures + ";" + SystemInfo.supportsStencil + ";" + SystemInfo.supportsVibration + ";" + SystemInfo.systemMemorySize + ";" + SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.ARGBHalf) + ";" + SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.ARGB4444) + ";" + SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.Depth) + ";" + SystemInfo.graphicsDeviceVersion.StartsWith("Metal") + ";" + Screen.currentResolution.width + ";" + Screen.currentResolution.height + ";" + Screen.dpi + ";";
			userAgent = ((!PlatformSettings.IsMobile()) ? (userAgent + "Desktop;") : ((!UniversalInputManager.UsePhoneUI) ? (userAgent + "Tablet;") : (userAgent + "Phone;")));
			userAgent += Application.genuine;
			userAgent += ") Battle.net/CSharp";
			Log.Net.Print(userAgent);
			return userAgent;
		}

		public int GetApplicationVersion()
		{
			return 216423;
		}

		private string CleanUserAgentString(string data)
		{
			return Regex.Replace(data, "[^a-zA-Z0-9_.]+", "_");
		}

		public string GetBasePersistentDataPath()
		{
			return PlatformFilePaths.PersistentDataPath;
		}

		public string GetTemporaryCachePath()
		{
			return s_tempCachePath;
		}

		public bool GetDisableConnectionMetering()
		{
			return Vars.Key("Aurora.DisableConnectionMetering").GetBool(def: false);
		}

		public Blizzard.GameService.SDK.Client.Integration.MobileEnv GetMobileEnvironment()
		{
			if (HearthstoneApplication.GetMobileEnvironment() == MobileEnv.PRODUCTION)
			{
				return Blizzard.GameService.SDK.Client.Integration.MobileEnv.PRODUCTION;
			}
			return Blizzard.GameService.SDK.Client.Integration.MobileEnv.DEVELOPMENT;
		}

		public string GetAuroraVersionName()
		{
			return 216423.ToString();
		}

		public string GetLocaleName()
		{
			return Localization.GetLocaleName();
		}

		public string GetPlatformName()
		{
			return "Win";
		}

		public RuntimeEnvironment GetRuntimeEnvironment()
		{
			return RuntimeEnvironment.Mono;
		}

		public int GetDataVersion()
		{
			return GameDbf.GetDataVersion();
		}

		public string GetAuthPlatform()
		{
			if (PlatformSettings.IsSteam)
			{
				return "STEA";
			}
			return "";
		}
	}

	public class ConnectErrorParams : ErrorParams
	{
		public float m_creationTime;

		public ConnectErrorParams()
		{
			m_creationTime = Time.realtimeSinceStartup;
		}
	}

	private class RequestContext
	{
		public float m_waitUntil;

		public int m_pendingResponseId;

		public int m_requestId;

		public int m_requestSubId;

		public TimeoutHandler m_timeoutHandler;

		public RequestContext(int pendingResponseId, int requestId, int requestSubId, TimeoutHandler timeoutHandler)
		{
			m_waitUntil = Time.realtimeSinceStartup + GetMaxDeferredWait();
			m_pendingResponseId = pendingResponseId;
			m_requestId = requestId;
			m_requestSubId = requestSubId;
			m_timeoutHandler = timeoutHandler;
		}
	}

	private class BnetErrorListener : EventListener<BnetErrorCallback>
	{
		public bool Fire(BnetErrorInfo info)
		{
			return m_callback(info, m_userData);
		}
	}

	public delegate void NetHandler();

	public delegate void ThrottledPacketListener(int packetID, long retryMillis);

	public delegate void TimeoutHandler(int pendingResponseId, int requestId, int requestSubId);

	public delegate void BnetEventHandler(BnetEvent[] updates);

	public delegate void FriendsHandler(FriendsUpdate[] updates);

	public delegate void WhisperHandler(BnetWhisper[] whispers);

	public delegate void PresenceHandler(PresenceUpdate[] updates);

	public delegate void ShutdownHandler(int minutes);

	public delegate bool BnetErrorCallback(BnetErrorInfo info, object userData);

	private struct NetworkState
	{
		public BattleNetLogSource LogSource { get; set; }

		public BnetGameType FindingBnetGameType { get; set; }

		public float LastCall { get; set; }

		public float LastCallReport { get; set; }

		public int LastCallFrame { get; set; }

		public FriendsHandler CurrentFriendsHandler { get; set; }

		public WhisperHandler CurrentWhisperHandler { get; set; }

		public PresenceHandler CurrentPresenceHandler { get; set; }

		public ShutdownHandler CurrentShutdownHandler { get; set; }

		public Map<BnetFeature, List<BnetErrorListener>> FeatureBnetErrorListeners { get; set; }

		public List<BnetErrorListener> GlobalBnetErrorListeners { get; set; }

		public FindGameResult LastFindGameParameters { get; set; }

		public ConnectToGameServer LastConnectToGameServerInfo { get; set; }

		public GameServerInfo LastGameServerInfo { get; set; }

		public string DelayedError { get; set; }

		public float TimeBeforeAllowReset { get; set; }

		public List<ClientStateNotification> QueuedClientStateNotifications { get; set; }

		public BnetGameAccountId CachedGameAccountId { get; set; }

		public BnetRegion CachedRegion { get; set; }

		public int CurrentCreateDeckRequestId { get; set; }

		public HashSet<int> InTransitOfflineCreateDeckRequestIds { get; set; }

		public HashSet<long> DeckIdsWaitingToDiffAgainstOfflineCache { get; set; }

		public int CurrentCreateTeamRequestId { get; set; }

		public Map<int, TimeoutHandler> NetTimeoutHandlers { get; set; }

		public void SetDefaults()
		{
			LogSource = new BattleNetLogSource("Network");
			FindingBnetGameType = BnetGameType.BGT_UNKNOWN;
			LastCall = Time.realtimeSinceStartup;
			LastCallReport = Time.realtimeSinceStartup;
			LastCallFrame = 0;
			FeatureBnetErrorListeners = new Map<BnetFeature, List<BnetErrorListener>>();
			GlobalBnetErrorListeners = new List<BnetErrorListener>();
			QueuedClientStateNotifications = new List<ClientStateNotification>();
			InTransitOfflineCreateDeckRequestIds = new HashSet<int>();
			DeckIdsWaitingToDiffAgainstOfflineCache = new HashSet<long>();
			NetTimeoutHandlers = new Map<int, TimeoutHandler>();
		}
	}

	public class UnavailableReason
	{
		public string mainReason;

		public string subReason;

		public string extraData;
	}

	public delegate void QueueInfoHandler(QueueInfo queueInfo);

	public delegate void GameQueueHandler(QueueEvent queueEvent);

	public delegate void GameServerDisconnectEvent(BattleNetErrors errorCode);

	public enum DisconnectReason
	{
		Unload,
		GameState_Restart,
		EndGameScreen,
		GameState_Reconnect,
		DALA_Tavern,
		ULDA_Dungeon,
		ULDA_Tavern,
		TB_BaconShop_Tutorial,
		TB_RoadToNR_Tavern,
		Network_Unreachable,
		DisconnectAfterFailedPings,
		GameCanceled,
		LeaveSpectator
	}

	public class QueueInfo
	{
		public int position;

		public long secondsTilEnd;

		public long stdev;
	}

	public class CanceledQuest
	{
		public int AchieveID { get; set; }

		public bool Canceled { get; set; }

		public long NextQuestCancelDate { get; set; }

		public CanceledQuest()
		{
			AchieveID = 0;
			Canceled = false;
			NextQuestCancelDate = 0L;
		}

		public override string ToString()
		{
			return $"[CanceledQuest AchieveID={AchieveID} Canceled={Canceled} NextQuestCancelDate={NextQuestCancelDate}]";
		}
	}

	public class TriggeredEvent
	{
		[CompilerGenerated]
		private int _003CEventID_003Ek__BackingField;

		[CompilerGenerated]
		private bool _003CSuccess_003Ek__BackingField;

		public int EventID
		{
			[CompilerGenerated]
			set
			{
				_003CEventID_003Ek__BackingField = value;
			}
		}

		public bool Success
		{
			[CompilerGenerated]
			set
			{
				_003CSuccess_003Ek__BackingField = value;
			}
		}

		public TriggeredEvent()
		{
			EventID = 0;
			Success = false;
		}
	}

	public class AdventureProgress
	{
		public int Wing { get; set; }

		public int Progress { get; set; }

		public int Ack { get; set; }

		public ulong Flags { get; set; }

		public AdventureProgress()
		{
			Wing = 0;
			Progress = 0;
			Ack = 0;
			Flags = 0uL;
		}
	}

	public class CardSaleResult
	{
		public enum SaleResult
		{
			GENERIC_FAILURE = 1,
			CARD_WAS_SOLD,
			CARD_WAS_BOUGHT,
			SOULBOUND,
			FAILED_WRONG_SELL_PRICE,
			FAILED_WRONG_BUY_PRICE,
			FAILED_NO_PERMISSION,
			FAILED_EVENT_NOT_ACTIVE,
			COUNT_MISMATCH,
			CARD_WAS_UPGRADED
		}

		public SaleResult Action { get; set; }

		public int AssetID { get; set; }

		public string AssetName { get; set; }

		public TAG_PREMIUM Premium { get; set; }

		public override string ToString()
		{
			return $"[CardSaleResult Action={Action} assetName={AssetName} premium={Premium}]";
		}
	}

	public class BeginDraft
	{
		public long DeckID { get; set; }

		public List<NetCache.CardDefinition> Heroes { get; }

		public int Wins { get; set; }

		public int MaxSlot { get; set; }

		public ArenaSession Session { get; set; }

		public DraftSlotType SlotType { get; set; }

		public List<DraftSlotType> UniqueSlotTypesForDraft { get; set; }

		public BeginDraft()
		{
			Heroes = new List<NetCache.CardDefinition>();
		}
	}

	public class DraftChoicesAndContents
	{
		public int Slot { get; set; }

		public List<NetCache.CardDefinition> Choices { get; }

		public NetCache.CardDefinition Hero { get; }

		public NetCache.CardDefinition HeroPower { get; }

		public DeckContents DeckInfo { get; }

		public int Wins { get; set; }

		public int Losses { get; set; }

		public RewardChest Chest { get; set; }

		public int MaxWins { get; set; }

		public int MaxSlot { get; set; }

		public ArenaSession Session { get; set; }

		public DraftSlotType SlotType { get; set; }

		public List<DraftSlotType> UniqueSlotTypesForDraft { get; }

		public DraftChoicesAndContents()
		{
			Choices = new List<NetCache.CardDefinition>();
			Hero = new NetCache.CardDefinition();
			HeroPower = new NetCache.CardDefinition();
			DeckInfo = new DeckContents();
			Chest = null;
			UniqueSlotTypesForDraft = new List<DraftSlotType>();
		}
	}

	public class DraftChosen
	{
		public NetCache.CardDefinition ChosenCard { get; set; }

		public List<NetCache.CardDefinition> NextChoices { get; set; }

		public DraftSlotType SlotType { get; set; }

		public DraftChosen()
		{
			ChosenCard = new NetCache.CardDefinition();
			NextChoices = new List<NetCache.CardDefinition>();
		}
	}

	public class RewardChest
	{
		public List<RewardData> Rewards { get; }

		public RewardChest()
		{
			Rewards = new List<RewardData>();
		}
	}

	public class DraftRetired
	{
		public long Deck { get; set; }

		public RewardChest Chest { get; set; }

		public DraftRetired()
		{
			Deck = 0L;
			Chest = new RewardChest();
		}
	}

	public class MassDisenchantResponse
	{
		public int Amount { get; set; }

		public MassDisenchantResponse()
		{
			Amount = 0;
		}
	}

	public class SetFavoriteHeroResponse
	{
		public bool Success;

		public TAG_CLASS HeroClass;

		public NetCache.CardDefinition Hero;

		public bool IsFavorite;
	}

	public class PurchaseErrorInfo
	{
		public enum ErrorType
		{
			UNKNOWN = -1,
			SUCCESS = 0,
			STILL_IN_PROGRESS = 1,
			INVALID_BNET = 2,
			SERVICE_NA = 3,
			PURCHASE_IN_PROGRESS = 4,
			DATABASE = 5,
			INVALID_QUANTITY = 6,
			DUPLICATE_LICENSE = 7,
			REQUEST_NOT_SENT = 8,
			NO_ACTIVE_BPAY = 9,
			FAILED_RISK = 10,
			CANCELED = 11,
			WAIT_MOP = 12,
			WAIT_CONFIRM = 13,
			WAIT_RISK = 14,
			PRODUCT_NA = 15,
			RISK_TIMEOUT = 16,
			PRODUCT_ALREADY_OWNED = 17,
			WAIT_THIRD_PARTY_RECEIPT = 18,
			PRODUCT_EVENT_HAS_ENDED = 19,
			BP_GENERIC_FAIL = 100,
			BP_INVALID_CC_EXPIRY = 101,
			BP_RISK_ERROR = 102,
			BP_NO_VALID_PAYMENT = 103,
			BP_PAYMENT_AUTH = 104,
			BP_PROVIDER_DENIED = 105,
			BP_PURCHASE_BAN = 106,
			BP_SPENDING_LIMIT = 107,
			BP_PARENTAL_CONTROL = 108,
			BP_THROTTLED = 109,
			BP_THIRD_PARTY_BAD_RECEIPT = 110,
			BP_THIRD_PARTY_RECEIPT_USED = 111,
			BP_PRODUCT_UNIQUENESS_VIOLATED = 112,
			BP_REGION_IS_DOWN = 113,
			E_BP_GENERIC_FAIL_RETRY_CONTACT_CS_IF_PERSISTS = 115,
			E_BP_CHALLENGE_ID_FAILED_VERIFICATION = 116
		}

		[CompilerGenerated]
		private string _003CPurchaseInProgressProductID_003Ek__BackingField;

		public ErrorType Error { get; set; }

		public string PurchaseInProgressProductID
		{
			[CompilerGenerated]
			set
			{
				_003CPurchaseInProgressProductID_003Ek__BackingField = value;
			}
		}

		public string ErrorCode { get; set; }

		public PurchaseErrorInfo()
		{
			Error = ErrorType.UNKNOWN;
			PurchaseInProgressProductID = string.Empty;
			ErrorCode = string.Empty;
		}
	}

	public class PurchaseCanceledResponse
	{
		public enum CancelResult
		{
			SUCCESS,
			NOT_ALLOWED,
			NOTHING_TO_CANCEL
		}

		public CancelResult Result { get; set; }

		public long TransactionID { get; set; }

		public long? PMTProductID { get; set; }

		public string CurrencyCode { get; set; }
	}

	public class BattlePayStatus
	{
		public enum PurchaseState
		{
			UNKNOWN = -1,
			READY,
			CHECK_RESULTS,
			ERROR
		}

		public PurchaseState State { get; set; }

		public long TransactionID { get; set; }

		public string ThirdPartyID { get; set; }

		public long? PMTProductID { get; set; }

		public PurchaseErrorInfo PurchaseError { get; set; }

		public bool BattlePayAvailable { get; set; }

		public string CurrencyCode { get; set; }

		public BattlePayProvider? Provider { get; set; }

		public BattlePayStatus()
		{
			State = PurchaseState.UNKNOWN;
			TransactionID = 0L;
			ThirdPartyID = string.Empty;
			PMTProductID = null;
			PurchaseError = new PurchaseErrorInfo();
			BattlePayAvailable = false;
			Provider = MoneyOrGTAPPTransaction.UNKNOWN_PROVIDER;
		}
	}

	public class BundleItem : Record
	{
		public ProductType ItemType { get; set; }

		public int ProductData { get; set; }

		public int Quantity { get; set; }

		public int BaseQuantity { get; set; }

		public AttributeSet Attributes { get; set; }

		public bool IsBlocking { get; set; }

		public bool HasShopAvailableDate { get; set; }

		public string ShopAvailableDate { get; set; }

		public BundleItem()
		{
			ItemType = ProductType.PRODUCT_TYPE_UNKNOWN;
			ProductData = 0;
			Quantity = 0;
			BaseQuantity = 0;
			Attributes = new AttributeSet();
			ShopAvailableDate = string.Empty;
		}

		protected override IEnumerable<object> GetEqualityComponents()
		{
			yield return ItemType;
			yield return ProductData;
			yield return Quantity;
			yield return BaseQuantity;
			yield return Attributes;
		}
	}

	public class Bundle : Record
	{
		public struct CostInfo
		{
			public long CurrentCost;

			public long OriginalCost;
		}

		public CostInfo? Cost { get; set; }

		public CostInfo? GtappGoldCost { get; set; }

		public CostInfo? VirtualCurrencyCost { get; set; }

		public string VirtualCurrencyCode { get; set; }

		public List<BundleItem> Items { get; set; }

		public string ProductEvent { get; set; }

		public bool IsPrePurchase { get; set; }

		public long? PMTProductID { get; set; }

		public DbfLocValue DisplayName { get; set; }

		public DbfLocValue DisplayDescription { get; set; }

		public AttributeSet Attributes { get; set; }

		public List<int> SaleIds { get; set; }

		public bool VisibleOnSalePeriodOnly { get; set; }

		public MobileShopType DisableRealMoneyShopFlags { get; set; }

		public Bundle()
		{
			Items = new List<BundleItem>();
			SaleIds = new List<int>();
		}

		protected override IEnumerable<object> GetEqualityComponents()
		{
			yield return Cost;
			foreach (BundleItem item in from item in Items
				orderby item.ItemType, item.ProductData
				select item)
			{
				yield return item;
			}
			yield return IsPrePurchase;
			yield return PMTProductID;
			foreach (int id in SaleIds)
			{
				yield return id;
			}
			yield return VisibleOnSalePeriodOnly;
		}
	}

	public class ShopSection
	{
		public string InternalName { get; set; }

		public DbfLocValue Label { get; set; }

		public uint SortOrder { get; set; }

		public List<Tuple<long, uint>> ProductOrder { get; set; }

		public AttributeSet Attributes { get; set; }
	}

	public class ShopSale : Record
	{
		public int SaleId { get; }

		public DateTime? StartUtc { get; }

		public DateTime? SoftEndUtc { get; }

		public DateTime? HardEndUtc { get; }

		protected override IEnumerable<object> GetEqualityComponents()
		{
			yield return SaleId;
			yield return StartUtc;
			yield return SoftEndUtc;
			yield return HardEndUtc;
		}
	}

	public class GoldCostBooster
	{
		public long? Cost { get; set; }

		public int ID { get; set; }

		public EventTimingType BuyWithGoldEvent { get; set; }

		public GoldCostBooster()
		{
			Cost = null;
			ID = 0;
			BuyWithGoldEvent = EventTimingType.UNKNOWN;
		}
	}

	public class BattlePayConfig
	{
		public bool Available { get; set; }

		public Currency Currency { get; set; }

		public List<Currency> Currencies { get; }

		public List<Bundle> Bundles { get; }

		public List<GoldCostBooster> GoldCostBoosters { get; }

		public long? GoldCostArena { get; set; }

		public int SecondsBeforeAutoCancel { get; set; }

		public string CommerceClientID { get; set; }

		public List<string> ShopPageIds { get; set; }

		public Map<long, Locale> CatalogLocaleToGameLocale { get; }

		public List<ShopSale> SaleList { get; set; }

		public bool IgnoreProductTiming { get; set; }

		public string CheckoutKrOnestoreKey { get; set; }

		public BattlePayConfig()
		{
			Available = false;
			Currencies = new List<Currency>();
			Bundles = new List<Bundle>();
			GoldCostBoosters = new List<GoldCostBooster>();
			GoldCostArena = null;
			SecondsBeforeAutoCancel = StoreManager.DEFAULT_SECONDS_BEFORE_AUTO_CANCEL;
			CommerceClientID = "df5787f96b2b46c49c66dd45bcb05490";
			ShopPageIds = new List<string>();
			CatalogLocaleToGameLocale = new Map<long, Locale>();
			SaleList = new List<ShopSale>();
			IgnoreProductTiming = false;
			CheckoutKrOnestoreKey = null;
		}
	}

	public class PurchaseViaGoldResponse
	{
		public enum ErrorType
		{
			UNKNOWN = -1,
			SUCCESS = 1,
			INSUFFICIENT_GOLD = 2,
			PRODUCT_NA = 3,
			FEATURE_NA = 4,
			INVALID_QUANTITY = 5
		}

		public ErrorType Error { get; set; }

		public long GoldUsed { get; set; }

		public PurchaseViaGoldResponse()
		{
			Error = ErrorType.UNKNOWN;
			GoldUsed = 0L;
		}
	}

	public class PurchaseMethod
	{
		public long TransactionID { get; set; }

		public long? PMTProductID { get; set; }

		public int Quantity { get; set; }

		public string CurrencyCode { get; set; }

		public string WalletName { get; set; }

		public bool UseEBalance { get; set; }

		public bool IsZeroCostLicense { get; set; }

		public string ChallengeID { get; set; }

		public string ChallengeURL { get; set; }

		public PurchaseErrorInfo PurchaseError { get; set; }

		public PurchaseMethod()
		{
			TransactionID = 0L;
			PMTProductID = null;
			Quantity = 0;
			CurrencyCode = string.Empty;
			WalletName = string.Empty;
			UseEBalance = false;
			IsZeroCostLicense = false;
			ChallengeID = string.Empty;
			ChallengeURL = string.Empty;
			PurchaseError = null;
		}
	}

	public class PurchaseResponse
	{
		public PurchaseErrorInfo PurchaseError { get; set; }

		public long TransactionID { get; set; }

		public long? PMTProductID { get; set; }

		public string ThirdPartyID { get; set; }

		public string CurrencyCode { get; set; }

		public PurchaseResponse()
		{
			PurchaseError = new PurchaseErrorInfo();
			TransactionID = 0L;
			PMTProductID = null;
			ThirdPartyID = string.Empty;
		}
	}

	public class CardBackResponse
	{
		public bool Success { get; set; }

		public bool IsFavorite { get; set; }

		public int CardBack { get; set; }

		public CardBackResponse()
		{
			Success = false;
			CardBack = 0;
		}
	}

	public class CosmeticCoinResponse
	{
		public bool Success { get; set; }

		public int Coin { get; set; }

		public bool IsFavorite { get; set; }

		public CosmeticCoinResponse()
		{
			Success = false;
			Coin = 1;
			IsFavorite = false;
		}
	}

	public class GameCancelInfo
	{
		public enum Reason
		{
			OPPONENT_TIMEOUT = 1,
			PLAYER_LOADING_TIMEOUT,
			PLAYER_LOADING_DISCONNECTED
		}

		public Reason CancelReason { get; set; }
	}

	public class Entity
	{
		public class Tag
		{
			public int Name { get; set; }

			public int Value { get; set; }
		}

		public class TagList
		{
			public int Name { get; set; }

			public List<int> Values { get; set; }
		}

		[CompilerGenerated]
		private List<TagList> _003CDefTagLists_003Ek__BackingField;

		public int ID { get; set; }

		public List<Tag> Tags { get; set; }

		public List<Tag> DefTags { get; set; }

		public List<TagList> TagLists { get; set; }

		public List<TagList> DefTagLists
		{
			[CompilerGenerated]
			set
			{
				_003CDefTagLists_003Ek__BackingField = value;
			}
		}

		public string CardID { get; set; }

		public Entity()
		{
			Tags = new List<Tag>();
			DefTags = new List<Tag>();
		}

		public static Entity CreateFromProto(PegasusGame.Entity src)
		{
			return new Entity
			{
				ID = src.Id,
				CardID = string.Empty,
				Tags = CreateTagsFromProto(src.Tags),
				TagLists = CreateTagListsFromProto(src.TagLists)
			};
		}

		public static Entity CreateFromProto(PowerHistoryEntity src)
		{
			return new Entity
			{
				ID = src.Entity,
				CardID = src.Name,
				Tags = CreateTagsFromProto(src.Tags),
				DefTags = CreateTagsFromProto(src.DefTags),
				TagLists = CreateTagListsFromProto(src.TagLists),
				DefTagLists = CreateTagListsFromProto(src.DefTagLists)
			};
		}

		public static List<Tag> CreateTagsFromProto(IList<PegasusGame.Tag> tagList)
		{
			List<Tag> result = new List<Tag>();
			for (int i = 0; i < tagList.Count; i++)
			{
				PegasusGame.Tag tag = tagList[i];
				result.Add(new Tag
				{
					Name = tag.Name,
					Value = tag.Value
				});
			}
			return result;
		}

		public static List<TagList> CreateTagListsFromProto(IList<PegasusGame.TagList> tagLists)
		{
			List<TagList> result = new List<TagList>();
			for (int i = 0; i < tagLists.Count; i++)
			{
				PegasusGame.TagList tagList = tagLists[i];
				result.Add(new TagList
				{
					Name = tagList.Name,
					Values = tagList.Values
				});
			}
			return result;
		}

		public override string ToString()
		{
			return $"id={ID} cardId={CardID} tags={Tags.Count}";
		}
	}

	public class Options
	{
		public class Option
		{
			public enum OptionType
			{
				PASS = 1,
				END_TURN,
				POWER
			}

			public class PlayErrorInfo
			{
				public PlayErrors.ErrorType PlayError { get; set; }

				public int? PlayErrorParam { get; set; }

				public PlayErrorInfo()
				{
					PlayError = PlayErrors.ErrorType.INVALID;
					PlayErrorParam = null;
				}

				public bool IsValid()
				{
					return PlayError == PlayErrors.ErrorType.NONE;
				}
			}

			public class TargetOption
			{
				public int ID { get; set; }

				public PlayErrorInfo PlayErrorInfo { get; set; }

				public TargetOption()
				{
					ID = 0;
					PlayErrorInfo = new PlayErrorInfo();
				}

				public void CopyFrom(TargetOption targetOption)
				{
					ID = targetOption.ID;
					PlayErrorInfo = targetOption.PlayErrorInfo;
				}

				public void CopyFrom(PegasusGame.TargetOption targetOption)
				{
					ID = targetOption.Id;
					PlayErrorInfo.PlayError = (PlayErrors.ErrorType)targetOption.PlayError;
					PlayErrorInfo.PlayErrorParam = (targetOption.HasPlayErrorParam ? new int?(targetOption.PlayErrorParam) : ((int?)null));
				}
			}

			public class SubOption
			{
				public int ID { get; set; }

				public List<TargetOption> Targets { get; set; }

				public PlayErrorInfo PlayErrorInfo { get; set; }

				public SubOption()
				{
					ID = 0;
					PlayErrorInfo = new PlayErrorInfo();
				}

				public bool IsDeckActionOption()
				{
					if (IsValidTarget(ID))
					{
						return Targets.Count == 1;
					}
					return false;
				}

				public bool IsValidTarget(int entityID)
				{
					if (Targets == null)
					{
						return false;
					}
					foreach (TargetOption target in Targets)
					{
						if (target.ID == entityID && target.PlayErrorInfo.IsValid())
						{
							return true;
						}
					}
					return false;
				}

				public PlayErrors.ErrorType GetErrorForTarget(int entityID)
				{
					if (Targets == null)
					{
						return PlayErrors.ErrorType.INVALID;
					}
					foreach (TargetOption target in Targets)
					{
						if (target.ID == entityID)
						{
							return target.PlayErrorInfo.PlayError;
						}
					}
					return PlayErrors.ErrorType.INVALID;
				}

				public int? GetErrorParamForTarget(int entityID)
				{
					if (Targets == null)
					{
						return null;
					}
					foreach (TargetOption target in Targets)
					{
						if (target.ID == entityID)
						{
							return target.PlayErrorInfo.PlayErrorParam;
						}
					}
					return null;
				}

				public bool HasValidTarget()
				{
					if (Targets == null)
					{
						return false;
					}
					foreach (TargetOption target in Targets)
					{
						if (target.PlayErrorInfo.IsValid())
						{
							return true;
						}
					}
					return false;
				}

				public void CopyFrom(SubOption subOption)
				{
					ID = subOption.ID;
					PlayErrorInfo = subOption.PlayErrorInfo;
					if (subOption.Targets == null)
					{
						Targets = null;
						return;
					}
					if (Targets == null)
					{
						Targets = new List<TargetOption>();
					}
					else
					{
						Targets.Clear();
					}
					for (int i = 0; i < subOption.Targets.Count; i++)
					{
						TargetOption targetOption = new TargetOption();
						targetOption.CopyFrom(subOption.Targets[i]);
						Targets.Add(targetOption);
					}
				}
			}

			public OptionType Type { get; set; }

			public SubOption Main { get; set; }

			public List<SubOption> Subs { get; set; }

			public Option()
			{
				Main = new SubOption();
				Subs = new List<SubOption>();
			}

			public SubOption GetSubOptionFromEntityID(int entityID)
			{
				foreach (SubOption sub in Subs)
				{
					if (sub.ID == entityID)
					{
						return sub;
					}
				}
				return null;
			}

			public bool HasValidSubOption()
			{
				foreach (SubOption sub in Subs)
				{
					if (sub.PlayErrorInfo.IsValid())
					{
						return true;
					}
				}
				return false;
			}

			public void CopyFrom(Option option)
			{
				Type = option.Type;
				if (Main == null)
				{
					Main = new SubOption();
				}
				Main.CopyFrom(option.Main);
				if (option.Subs == null)
				{
					Subs = null;
					return;
				}
				if (Subs == null)
				{
					Subs = new List<SubOption>();
				}
				else
				{
					Subs.Clear();
				}
				for (int i = 0; i < option.Subs.Count; i++)
				{
					SubOption subOption = new SubOption();
					subOption.CopyFrom(option.Subs[i]);
					Subs.Add(subOption);
				}
			}
		}

		public int ID { get; set; }

		public List<Option> List { get; set; }

		public Options()
		{
			List = new List<Option>();
		}

		public bool HasValidOption()
		{
			foreach (Option item in List)
			{
				if (item.Main.PlayErrorInfo.IsValid())
				{
					return true;
				}
			}
			return false;
		}

		public Option GetOptionFromEntityID(int entityID, bool wantDeckActionOption = false)
		{
			for (int i = 0; i < List.Count; i++)
			{
				bool isDeckActionOption = List[i].Main.IsDeckActionOption();
				if (List[i].Main.ID == entityID && isDeckActionOption == wantDeckActionOption)
				{
					return List[i];
				}
			}
			return null;
		}

		public void CopyFrom(Options options)
		{
			ID = options.ID;
			if (options.List == null)
			{
				List = null;
				return;
			}
			if (List != null)
			{
				List.Clear();
			}
			else
			{
				List = new List<Option>();
			}
			for (int i = 0; i < options.List.Count; i++)
			{
				Option option = new Option();
				option.CopyFrom(options.List[i]);
				List.Add(option);
			}
		}
	}

	public class EntityChoices
	{
		public int ID { get; set; }

		public CHOICE_TYPE ChoiceType { get; set; }

		public int CountMin { get; set; }

		public int CountMax { get; set; }

		public List<int> Entities { get; set; }

		public int Source { get; set; }

		public int PlayerId { get; set; }

		public bool HideChosen { get; set; }

		public bool IsSingleChoice()
		{
			if (CountMax == 0)
			{
				return true;
			}
			if (CountMin == 1)
			{
				return CountMax == 1;
			}
			return false;
		}
	}

	public class MulliganChooseOneTentativeSelection
	{
		public int EntityId { get; set; }

		public bool IsConfirmation { get; set; }

		public bool IsFromTeammate { get; set; }
	}

	public class EntitiesChosen
	{
		public int ID { get; set; }

		public List<int> Entities { get; set; }

		public int PlayerId { get; set; }

		public CHOICE_TYPE ChoiceType { get; set; }
	}

	public class GameSetup
	{
		public int Board { get; set; }

		public int BoardLayout { get; set; }

		public int BaconFavoriteBoardSkin { get; set; }

		public int BaconTeammateFavoriteBoardSkin { get; set; }

		public int MaxSecretZoneSizePerPlayer { get; set; }

		public int MaxSecretsPerPlayer { get; set; }

		public int MaxQuestsPerPlayer { get; set; }

		public int MaxFriendlyMinionsPerPlayer { get; set; }

		public uint DisconnectWhenStuckSeconds { get; set; }
	}

	public class UserUI
	{
		public class MouseInfo
		{
			[CompilerGenerated]
			private int _003CX_003Ek__BackingField;

			[CompilerGenerated]
			private int _003CY_003Ek__BackingField;

			public int OverCardID { get; set; }

			public int HeldCardID { get; set; }

			public int ArrowOriginID { get; set; }

			public int X
			{
				[CompilerGenerated]
				set
				{
					_003CX_003Ek__BackingField = value;
				}
			}

			public int Y
			{
				[CompilerGenerated]
				set
				{
					_003CY_003Ek__BackingField = value;
				}
			}

			public bool UseTeammatesGamestate { get; set; }
		}

		public class EmoteInfo
		{
			public int Emote { get; set; }

			public int BattlegroundsEmoteId { get; set; }
		}

		public class SelectionInfo
		{
			public int SelectedEntityID { get; set; }
		}

		public MouseInfo mouseInfo;

		public EmoteInfo emoteInfo;

		public SelectionInfo selectionInfo;

		public int? playerId;

		public bool fromTeammate;
	}

	public enum PowerType
	{
		FULL_ENTITY = 1,
		SHOW_ENTITY,
		HIDE_ENTITY,
		TAG_CHANGE,
		BLOCK_START,
		BLOCK_END,
		CREATE_GAME,
		META_DATA,
		CHANGE_ENTITY,
		RESET_GAME,
		SUB_SPELL_START,
		SUB_SPELL_END,
		VO_SPELL,
		CACHED_TAG_FOR_DORMANT_CHANGE,
		SHUFFLE_DECK,
		VO_BANTER,
		TAG_LIST_CHANGE
	}

	public class PowerHistory
	{
		public PowerType Type { get; }

		public PowerHistory(PowerType init)
		{
			Type = init;
		}

		public override string ToString()
		{
			return $"type={Type}";
		}
	}

	public class HistBlockStart : PowerHistory
	{
		public HistoryBlock.Type BlockType { get; set; }

		public List<int> Entities { get; set; }

		public int Target { get; set; }

		public int SubOption { get; set; }

		public List<string> EffectCardId { get; set; }

		public List<bool> IsEffectCardIdClientCached { get; set; }

		public int EffectIndex { get; set; }

		public int TriggerKeyword { get; set; }

		public bool ShowInHistory { get; set; }

		public bool IsDeferrable { get; set; }

		public bool IsBatchable { get; set; }

		public bool IsDeferBlocker { get; set; }

		public bool ForceShowBigCard { get; set; }

		public HistBlockStart(HistoryBlock.Type type)
			: base(PowerType.BLOCK_START)
		{
			BlockType = type;
		}

		public override string ToString()
		{
			return $"type={base.Type} blockType={BlockType} entity={Entities} target={Target} b={IsBatchable} d={IsDeferrable} xd={IsDeferBlocker} bigCard={ForceShowBigCard}";
		}
	}

	public class HistBlockEnd : PowerHistory
	{
		public HistBlockEnd()
			: base(PowerType.BLOCK_END)
		{
		}
	}

	public class HistCreateGame : PowerHistory
	{
		public class PlayerData
		{
			public int ID { get; set; }

			public BnetGameAccountId GameAccountId { get; set; }

			public Entity Player { get; set; }

			public int CardBackID { get; set; }

			public static PlayerData CreateFromProto(PegasusGame.Player src)
			{
				return new PlayerData
				{
					ID = src.Id,
					GameAccountId = BnetGameAccountId.CreateFromNet(src.GameAccountId),
					Player = Entity.CreateFromProto(src.Entity),
					CardBackID = src.CardBack
				};
			}

			public override string ToString()
			{
				return $"ID={ID} GameAccountId={GameAccountId} Player={Player} CardBackID={CardBackID}";
			}
		}

		public class SharedPlayerInfo
		{
			public int ID { get; set; }

			public BnetGameAccountId GameAccountId { get; set; }

			public static SharedPlayerInfo CreateFromProto(PegasusGame.SharedPlayerInfo src)
			{
				return new SharedPlayerInfo
				{
					ID = src.Id,
					GameAccountId = BnetGameAccountId.CreateFromNet(src.GameAccountId)
				};
			}

			public override string ToString()
			{
				return $"ID={ID} GameAccountId={GameAccountId}";
			}
		}

		public class ActionInfo
		{
			public int PlayerID { get; set; }

			public int SelectedEntityID { get; set; }

			public static ActionInfo CreateFromProto(PegasusGame.ActionInfo src)
			{
				return new ActionInfo
				{
					PlayerID = src.PlayerId,
					SelectedEntityID = src.SelectedEntityId
				};
			}

			public override string ToString()
			{
				return $"PlayerID={PlayerID} SelectedEntityID={SelectedEntityID}";
			}
		}

		public Entity Game { get; set; }

		public string Uuid { get; set; }

		public List<PlayerData> Players { get; set; }

		public List<SharedPlayerInfo> PlayerInfos { get; set; }

		public List<ActionInfo> ActionInfos { get; set; }

		public static HistCreateGame CreateFromProto(PowerHistoryCreateGame src)
		{
			HistCreateGame dst = new HistCreateGame
			{
				Uuid = src.GameUuid,
				Game = Entity.CreateFromProto(src.GameEntity)
			};
			if (src.Players != null)
			{
				dst.Players = new List<PlayerData>();
				foreach (PegasusGame.Player player in src.Players)
				{
					PlayerData dstPlayer = PlayerData.CreateFromProto(player);
					dst.Players.Add(dstPlayer);
				}
			}
			if (src.PlayerInfos == null)
			{
				return dst;
			}
			dst.PlayerInfos = new List<SharedPlayerInfo>();
			foreach (PegasusGame.SharedPlayerInfo playerInfo in src.PlayerInfos)
			{
				SharedPlayerInfo dstPlayerInfo = SharedPlayerInfo.CreateFromProto(playerInfo);
				dst.PlayerInfos.Add(dstPlayerInfo);
			}
			if (src.ActionInfos != null)
			{
				dst.ActionInfos = new List<ActionInfo>();
				for (int i = 0; i < src.ActionInfos.Count; i++)
				{
					ActionInfo dstActionInfo = ActionInfo.CreateFromProto(src.ActionInfos[i]);
					dst.ActionInfos.Add(dstActionInfo);
				}
			}
			return dst;
		}

		public HistCreateGame()
			: base(PowerType.CREATE_GAME)
		{
		}

		public override string ToString()
		{
			StringBuilder builder = new StringBuilder();
			builder.AppendFormat("game={0}", Game);
			if (Players == null)
			{
				builder.Append(" players=(null)");
			}
			else if (Players.Count == 0)
			{
				builder.Append(" players=0");
			}
			else
			{
				for (int i = 0; i < Players.Count; i++)
				{
					builder.AppendFormat(" players[{0}]=[{1}]", i, Players[i]);
				}
			}
			if (PlayerInfos == null)
			{
				builder.Append(" playerInfos=(null)");
			}
			else if (PlayerInfos.Count == 0)
			{
				builder.Append(" playerInfos=0");
			}
			else
			{
				for (int j = 0; j < PlayerInfos.Count; j++)
				{
					builder.AppendFormat(" playerInfos[{0}]=[{1}]", j, PlayerInfos[j]);
				}
			}
			if (ActionInfos == null)
			{
				builder.Append(" ActionInfos=(null)");
			}
			else
			{
				for (int k = 0; k < ActionInfos.Count; k++)
				{
					builder.AppendFormat(" ActionInfos[{0}]=[{1}]", k, ActionInfos[k]);
				}
			}
			return builder.ToString();
		}
	}

	public class HistResetGame : PowerHistory
	{
		public HistCreateGame CreateGame { get; set; }

		public HistResetGame()
			: base(PowerType.RESET_GAME)
		{
		}

		public override string ToString()
		{
			return $"type={base.Type}";
		}

		public static HistResetGame CreateFromProto(PowerHistoryResetGame proto)
		{
			return new HistResetGame
			{
				CreateGame = HistCreateGame.CreateFromProto(proto.CreateGame)
			};
		}
	}

	public class HistFullEntity : PowerHistory
	{
		public Entity Entity { get; set; }

		public HistFullEntity()
			: base(PowerType.FULL_ENTITY)
		{
		}

		public override string ToString()
		{
			return $"type={base.Type} entity=[{Entity}]";
		}
	}

	public class HistShowEntity : PowerHistory
	{
		public Entity Entity { get; set; }

		public HistShowEntity()
			: base(PowerType.SHOW_ENTITY)
		{
		}

		public override string ToString()
		{
			return $"type={base.Type} entity=[{Entity}]";
		}
	}

	public class HistHideEntity : PowerHistory
	{
		public int Entity { get; set; }

		public int Zone { get; set; }

		public HistHideEntity()
			: base(PowerType.HIDE_ENTITY)
		{
		}

		public override string ToString()
		{
			return $"type={base.Type} entity={Entity} zone={Zone}";
		}
	}

	public class HistChangeEntity : PowerHistory
	{
		public Entity Entity { get; set; }

		public HistChangeEntity()
			: base(PowerType.CHANGE_ENTITY)
		{
		}

		public override string ToString()
		{
			return $"type={base.Type} entity=[{Entity}]";
		}
	}

	public class HistTagChange : PowerHistory
	{
		public int Entity { get; set; }

		public int Tag { get; set; }

		public int Value { get; set; }

		public bool ChangeDef { get; set; }

		public HistTagChange()
			: base(PowerType.TAG_CHANGE)
		{
		}

		public override string ToString()
		{
			return $"type={base.Type} entity={Entity} tag={Tag} value={Value}";
		}
	}

	public class HistMetaData : PowerHistory
	{
		public HistoryMeta.Type MetaType { get; set; }

		public List<int> Info { get; }

		public int Data { get; set; }

		public List<int> AdditionalData { get; }

		public HistMetaData()
			: base(PowerType.META_DATA)
		{
			Info = new List<int>();
			AdditionalData = new List<int>();
		}

		public override string ToString()
		{
			return $"type={base.Type} metaType={MetaType} infoCount={Info.Count} data={Data}";
		}
	}

	public class HistSubSpellStart : PowerHistory
	{
		public string SpellPrefabGUID { get; set; }

		public int SourceEntityID { get; set; }

		public List<int> TargetEntityIDS { get; set; }

		public HistSubSpellStart()
			: base(PowerType.SUB_SPELL_START)
		{
		}

		public static HistSubSpellStart CreateFromProto(PowerHistorySubSpellStart proto)
		{
			return new HistSubSpellStart
			{
				SpellPrefabGUID = proto.SpellPrefabGuid,
				SourceEntityID = (proto.HasSourceEntityId ? proto.SourceEntityId : 0),
				TargetEntityIDS = proto.TargetEntityIds
			};
		}
	}

	public class HistSubSpellEnd : PowerHistory
	{
		public HistSubSpellEnd()
			: base(PowerType.SUB_SPELL_END)
		{
		}
	}

	public class HistVoSpell : PowerHistory
	{
		public string SpellPrefabGUID { get; set; }

		public int Speaker { get; set; }

		public bool Blocking { get; set; }

		public int AdditionalDelayMs { get; set; }

		public string BrassRingGUID { get; set; }

		public AudioSource m_audioSource { get; set; }

		public bool m_ableToLoad { get; set; }

		public HistVoSpell()
			: base(PowerType.VO_SPELL)
		{
		}

		public static HistVoSpell CreateFromProto(PowerHistoryVoTask proto)
		{
			return new HistVoSpell
			{
				SpellPrefabGUID = proto.SpellPrefabGuid,
				Speaker = proto.SpeakingEntity,
				Blocking = proto.Blocking,
				AdditionalDelayMs = proto.AdditionalDelayMs,
				BrassRingGUID = proto.BrassRingPrefabGuid
			};
		}
	}

	public class HistVoBanter : PowerHistory
	{
		public PowerHistoryVoBanter.ClientEmoteEvent EmoteEvent { get; private set; }

		public List<int> Teams { get; private set; } = new List<int>();

		public int Speaker { get; private set; }

		public HistVoBanter()
			: base(PowerType.VO_BANTER)
		{
		}

		public static HistVoBanter CreateFromProto(PowerHistoryVoBanter proto)
		{
			HistVoBanter voBanter = new HistVoBanter();
			voBanter.EmoteEvent = proto.EmoteEvent;
			if (proto.Teams.Count > 0)
			{
				voBanter.Teams = proto.Teams;
			}
			if (proto.HasSpeaker)
			{
				voBanter.Speaker = proto.Speaker;
			}
			return voBanter;
		}
	}

	public class HistCachedTagForDormantChange : PowerHistory
	{
		public int Entity { get; set; }

		public int Tag { get; set; }

		public int Value { get; set; }

		public HistCachedTagForDormantChange()
			: base(PowerType.CACHED_TAG_FOR_DORMANT_CHANGE)
		{
		}

		public static HistCachedTagForDormantChange CreateFromProto(PowerHistoryCachedTagForDormantChange proto)
		{
			return new HistCachedTagForDormantChange
			{
				Entity = proto.Entity,
				Tag = proto.Tag,
				Value = proto.Value
			};
		}

		public override string ToString()
		{
			return $"type={base.Type} entity={Entity} tag={Tag} value={Value}";
		}
	}

	public class HistTagListChange : PowerHistory
	{
		public int Entity { get; set; }

		public int Tag { get; set; }

		public List<int> Values { get; set; }

		public bool ChangeDef { get; set; }

		public HistTagListChange()
			: base(PowerType.TAG_LIST_CHANGE)
		{
		}

		public static HistTagListChange CreateFromProto(PowerHistoryTagListChange proto)
		{
			return new HistTagListChange
			{
				Entity = proto.Entity,
				Tag = proto.Tag,
				Values = proto.TagList,
				ChangeDef = proto.ChangeDef
			};
		}

		public override string ToString()
		{
			return $"type={base.Type} entity={Entity} tag={Tag} values={Values.ToString()} defChange={ChangeDef}";
		}
	}

	public class HistShuffleDeck : PowerHistory
	{
		public int PlayerID { get; set; }

		public HistShuffleDeck()
			: base(PowerType.SHUFFLE_DECK)
		{
		}

		public static HistShuffleDeck CreateFromProto(PowerHistoryShuffleDeck proto)
		{
			return new HistShuffleDeck
			{
				PlayerID = proto.PlayerId
			};
		}

		public override string ToString()
		{
			return $"type={base.Type} player_id={PlayerID}";
		}
	}

	public class CardUserData
	{
		public int DbId { get; set; }

		public int Count { get; set; }

		public TAG_PREMIUM Premium { get; set; }
	}

	public class SideboardCardUserData
	{
		public int LinkedCardDbId { get; set; }

		public CardUserData Card { get; set; }
	}

	public class DeckContents
	{
		public long Deck { get; set; }

		public List<CardUserData> Cards { get; }

		public List<SideboardCardUserData> SideboardCards { get; }

		public DeckContents()
		{
			Cards = new List<CardUserData>();
			SideboardCards = new List<SideboardCardUserData>();
		}

		public static DeckContents FromPacket(PegasusUtil.DeckContents packet)
		{
			DeckContents result = new DeckContents
			{
				Deck = packet.DeckId
			};
			foreach (DeckCardData card in packet.Cards)
			{
				CardUserData cardToInsert = new CardUserData
				{
					DbId = card.Def.Asset,
					Count = ((!card.HasQty) ? 1 : card.Qty),
					Premium = (card.Def.HasPremium ? ((TAG_PREMIUM)card.Def.Premium) : TAG_PREMIUM.NORMAL)
				};
				result.Cards.Add(cardToInsert);
			}
			foreach (SideBoardCardData sideboardCard in packet.SideboardCards)
			{
				SideboardCardUserData cardToInsert2 = new SideboardCardUserData
				{
					Card = new CardUserData
					{
						Count = sideboardCard.Qty,
						DbId = sideboardCard.Def.Asset,
						Premium = (TAG_PREMIUM)sideboardCard.Def.Premium
					},
					LinkedCardDbId = sideboardCard.LinkedCardDbId
				};
				result.SideboardCards.Add(cardToInsert2);
			}
			return result;
		}
	}

	public class DeckName
	{
		public long Deck { get; set; }

		public string Name { get; set; }

		public bool IsCensored { get; set; }
	}

	public class GenericResponse
	{
		public enum Result
		{
			RESULT_OK = 0,
			RESULT_REQUEST_IN_PROCESS = 1,
			RESULT_REQUEST_COMPLETE = 2,
			RESULT_UNKNOWN_ERROR = 100,
			RESULT_INTERNAL_ERROR = 101,
			RESULT_DB_ERROR = 102,
			RESULT_INVALID_REQUEST = 103,
			RESULT_LOGIN_LOAD = 104,
			RESULT_DATA_MIGRATION_OR_PLAYER_ID_ERROR = 105,
			RESULT_INTERNAL_RPC_ERROR = 106,
			RESULT_DATA_MIGRATION_REQUIRED = 107
		}

		public int RequestId { get; set; }

		public int RequestSubId { get; set; }

		public Result ResultCode { get; set; }

		public object GenericData { get; set; }
	}

	public class DBAction
	{
		public enum ActionType
		{
			UNKNOWN,
			GET_DECK,
			CREATE_DECK,
			RENAME_DECK,
			DELETE_DECK,
			SET_DECK,
			OPEN_BOOSTER,
			GAMES_INFO
		}

		public enum ResultType
		{
			UNKNOWN,
			SUCCESS,
			NOT_OWNED,
			CONSTRAINT
		}

		public ActionType Action { get; set; }

		public ResultType Result { get; set; }

		public long MetaData { get; set; }
	}

	public class TurnTimerInfo
	{
		public float Seconds { get; set; }

		public int Turn { get; set; }

		public bool Show { get; set; }
	}

	public class GameEnd
	{
		public List<NetCache.ProfileNotice> Notices { get; }

		public GameEnd()
		{
			Notices = new List<NetCache.ProfileNotice>();
		}
	}

	public class AccountLicenseAchieveResponse
	{
		public enum AchieveResult
		{
			INVALID_ACHIEVE = 1,
			NOT_ACTIVE,
			IN_PROGRESS,
			COMPLETE,
			STATUS_UNKNOWN
		}

		public int Achieve { get; set; }

		public AchieveResult Result { get; set; }
	}

	public class DebugConsoleResponse
	{
		public int Type { get; set; }

		public string Response { get; set; }

		public DebugConsoleResponse()
		{
			Response = "";
		}
	}

	public static string TutorialServer = "01";

	public const string CosmeticVersion = "31.6";

	public const string CosmeticRevision = "0";

	public const string VersionPostfix = "";

	private static readonly float PROCESS_WARNING = 15f;

	private static readonly float PROCESS_WARNING_REPORT_GAP = 1f;

	private const int MIN_DEFERRED_WAIT = 30;

	public const int SEND_DECK_DATA_NO_HERO_ASSET_CHANGE = -1;

	public const int SEND_DECK_DATA_NO_COSMETIC_COIN_CHANGE = -1;

	public const int SEND_DECK_DATA_NO_CARD_BACK_CHANGE = -1;

	private const float ERROR_HANDLING_DELAY = 0.4f;

	public static readonly PlatformDependentValue<bool> CONNECT_TO_AURORA_BY_DEFAULT = new PlatformDependentValue<bool>(PlatformCategory.OS)
	{
		PC = true,
		Mac = true,
		iOS = false,
		Android = false
	};

	private static readonly SortedDictionary<int, int> m_deferredMessageResponseMap = new SortedDictionary<int, int>
	{
		{ 305, 306 },
		{ 205, 307 },
		{ 314, 315 }
	};

	private static readonly SortedDictionary<int, int> m_deferredGetAccountInfoMessageResponseMap = new SortedDictionary<int, int>
	{
		{ 11, 233 },
		{ 18, 264 },
		{ 4, 232 },
		{ 2, 202 },
		{ 10, 231 },
		{ 15, 260 },
		{ 19, 271 },
		{ 8, 270 },
		{ 21, 283 },
		{ 7, 236 },
		{ 27, 318 },
		{ 28, 325 },
		{ 29, 608 },
		{ 30, 621 },
		{ 31, 626 },
		{ 32, 637 },
		{ 33, 642 },
		{ 34, 649 }
	};

	private IDispatcher m_dispatcherImpl;

	private Map<int, List<NetHandler>> m_netHandlers = new Map<int, List<NetHandler>>();

	[StatePrinter.IncludeState]
	private int m_numConnectionFailures;

	private ConnectAPI m_connectApi;

	[StatePrinter.IncludeState]
	private double m_timeInternetUnreachable;

	private AckCardSeen m_ackCardSeenPacket = new AckCardSeen();

	private AckBattlegroundsSkinsSeen m_ackBattlegroundsSkinsSeenPacket = new AckBattlegroundsSkinsSeen();

	private readonly List<ConnectErrorParams> m_errorList = new List<ConnectErrorParams>();

	private List<ThrottledPacketListener> m_throttledPacketListeners = new List<ThrottledPacketListener>();

	private List<RequestContext> m_inTransitRequests = new List<RequestContext>();

	private static float m_maxDeferredWait = 120f;

	[StatePrinter.IncludeState]
	private static bool s_shouldBeConnectedToAurora = CONNECT_TO_AURORA_BY_DEFAULT;

	[StatePrinter.IncludeState]
	private static bool s_running;

	private NetworkState m_state;

	private NetworkReachabilityManager m_networkReachabilityManager;

	private BreakingNews m_breakingNews;

	private List<BnetEvent> m_bnetEvents = new List<BnetEvent>();

	private List<BnetWhisper> m_bnetWhispers = new List<BnetWhisper>();

	private List<BnetNotification> m_bnetNotifications = new List<BnetNotification>();

	private List<FriendsUpdate> m_friendsUpdates = new List<FriendsUpdate>();

	private List<PresenceUpdate> m_presenceUpdates = new List<PresenceUpdate>();

	private List<BnetErrorInfo> m_bnetErrors = new List<BnetErrorInfo>();

	private List<BattleNetErrors> m_sentTelemErrors = new List<BattleNetErrors>(0);

	private List<BattleNetErrors> m_sentTelemNetworkErrors = new List<BattleNetErrors>(0);

	private List<int> ignorableErrorsSent = new List<int>();

	private const int DefaultMinimumSkinsSeenBeforeSend = 16;

	[CompilerGenerated]
	private string _003CGameServerIPv6_003Ek__BackingField;

	[CompilerGenerated]
	private string _003CGameServerIPv4_003Ek__BackingField;

	[CompilerGenerated]
	private bool _003CIsOSSupportIPv6_003Ek__BackingField = Socket.OSSupportsIPv6;

	private DispatchListener m_dispatcherListner;

	private RuntimeGamePacketMonitor m_runtimeGamePacketMonitor;

	private QueueInfoHandler m_queueInfoHandler;

	private GameQueueHandler m_gameQueueHandler;

	private uint m_gameServerKeepAliveFrequencySeconds;

	private uint m_gameServerKeepAliveRetry;

	private uint m_gameServerKeepAliveWaitForInternetSeconds;

	[StatePrinter.IncludeState]
	private bool m_gameConceded;

	[StatePrinter.IncludeState]
	private bool m_disconnectRequested;

	private const int USING_LOCAL_TEAM = 0;

	private const int USING_REMOTE_TEAM = 2;

	private static readonly Map<BnetRegion, string> RegionToTutorialName = new Map<BnetRegion, string>
	{
		{
			BnetRegion.REGION_US,
			"us-tutorial{0}.actual.battle.net"
		},
		{
			BnetRegion.REGION_EU,
			"eu-tutorial{0}.actual.battle.net"
		},
		{
			BnetRegion.REGION_KR,
			"kr-tutorial{0}.actual.battle.net"
		},
		{
			BnetRegion.REGION_CN,
			"cn-tutorial{0}.actual.battlenet.com.cn"
		}
	};

	private const bool ReconnectAfterFailedPings = true;

	private Blizzard.T5.Core.ILogger m_GameNetlogger;

	public IDispatcher QueueDispatcher => m_dispatcherImpl;

	public static string BranchName => string.Format("{0}.{1}{2}", "31.6", "0", "");

	private long FakeIdWaitingForResponse { get; set; }

	public DispatchListener NetworkDispatchListener => m_dispatcherListner;

	private static List<BattleNetErrors> GameServerDisconnectEvents { get; set; }

	private string GameServerIPv6
	{
		[CompilerGenerated]
		set
		{
			_003CGameServerIPv6_003Ek__BackingField = value;
		}
	}

	private string GameServerIPv4
	{
		[CompilerGenerated]
		set
		{
			_003CGameServerIPv4_003Ek__BackingField = value;
		}
	}

	private bool IsOSSupportIPv6
	{
		[CompilerGenerated]
		set
		{
			_003CIsOSSupportIPv6_003Ek__BackingField = value;
		}
	}

	public Blizzard.T5.Core.ILogger GameNetLogger
	{
		get
		{
			if (m_GameNetlogger == null)
			{
				LogInfo logInfo = new LogInfo();
				logInfo.m_name = "GameNetLogger";
				logInfo.m_defaultLevel = Blizzard.T5.Logging.LogLevel.Info;
				logInfo.m_minLevel = Blizzard.T5.Logging.LogLevel.Info;
				logInfo.m_consolePrinting = true;
				logInfo.m_filePrinting = true;
				logInfo.m_alwaysPrintErrors = true;
				m_GameNetlogger = LogSystem.Get().CreateLogger("GameNetLogger", logInfo);
			}
			return m_GameNetlogger;
		}
	}

	public event Action<BattleNetErrors> OnConnectedToBattleNet;

	public event Action<BattleNetErrors> OnDisconnectedFromBattleNet;

	public void MercenariesPlayerInfoRequest()
	{
		MercenariesPlayerInfoRequest request = new MercenariesPlayerInfoRequest();
		m_connectApi.MercenariesPlayerInfoRequest(request);
	}

	public MercenariesPlayerInfoResponse MercenariesPlayerInfoResponse()
	{
		return m_connectApi.MercenariesPlayerInfoResponse();
	}

	public void MercenariesCollectionRequest()
	{
		MercenariesCollectionRequest request = new MercenariesCollectionRequest();
		m_connectApi.MercenariesCollectionRequest(request);
	}

	public MercenariesCollectionResponse MercenariesCollectionResponse()
	{
		return m_connectApi.MercenariesCollectionResponse();
	}

	public MercenariesCollectionUpdate MercenariesCollectionUpdate()
	{
		return m_connectApi.MercenariesCollectionUpdate();
	}

	public MercenariesCurrencyUpdate MercenariesCurrencyUpdate()
	{
		return m_connectApi.MercenariesCurrencyUpdate();
	}

	public MercenariesExperienceUpdate MercenariesExperienceUpdate()
	{
		return m_connectApi.MercenariesExperienceUpdate();
	}

	public MercenariesRewardUpdate MercenariesRewardUpdate()
	{
		return m_connectApi.MercenariesRewardUpdate();
	}

	public void MercenariesTeamListRequest()
	{
		MercenariesTeamListRequest request = new MercenariesTeamListRequest();
		m_connectApi.MercenariesTeamListRequest(request);
	}

	public MercenariesTeamListResponse MercenariesTeamListResponse()
	{
		return m_connectApi.MercenariesTeamListResponse();
	}

	public void CreateMercenariesTeamRequest(string name, PegasusLettuce.LettuceTeam.Type type, out int? requestId)
	{
		if (!IsLoggedIn())
		{
			requestId = null;
			return;
		}
		requestId = GetNextCreateTeamRequestId();
		Log.Net.Print("Network.CreateMercenariesTeamRequest");
		uint sortOrder = 0u;
		List<LettuceTeam> teams = CollectionManager.Get().GetTeams();
		for (int i = 0; i < teams.Count; i++)
		{
			sortOrder = (uint)Mathf.Max(sortOrder, teams[i].SortOrder + 1);
		}
		CreateMercenariesTeamRequest request = new CreateMercenariesTeamRequest
		{
			Team = new PegasusLettuce.LettuceTeam
			{
				Name = name,
				Type_ = type,
				SortOrder = sortOrder
			}
		};
		request.RequestId = requestId.Value;
		m_connectApi.CreateMercenariesTeamRequest(request);
	}

	public CreateMercenariesTeamResponse CreateMercenariesTeamResponse()
	{
		return m_connectApi.CreateMercenariesTeamResponse();
	}

	public void UpdateMercenariesTeamRequest(LettuceTeam team)
	{
		UpdateMercenariesTeamRequest request = new UpdateMercenariesTeamRequest();
		if (team.Name == null)
		{
			Log.Net.PrintError("Network.UpdateMercenariesTeamRequest - Team name is null!");
			return;
		}
		request.Team = new PegasusLettuce.LettuceTeam
		{
			TeamId = team.ID,
			Name = team.Name,
			Type_ = team.TeamType,
			SortOrder = team.SortOrder,
			MercenaryList = new LettuceTeamMercenaryList()
		};
		foreach (LettuceMercenary mercenary in team.GetMercs())
		{
			LettuceMercenary.Loadout loadout = team.GetLoadout(mercenary);
			if (loadout == null || !loadout.IsValid())
			{
				Log.Net.PrintError($"Network.UpdateMercenariesTeamRequest - Loadout was null or invalid mercenary{mercenary.ID}!");
				continue;
			}
			int portraitId = LettuceMercenary.GetPortraitIdFromArtVariation(loadout.m_artVariationRecord.ID, loadout.m_artVariationPremium);
			LettuceTeamMercenary teamMerc = new LettuceTeamMercenary
			{
				MercenaryId = mercenary.ID,
				SelectedPortraitId = portraitId
			};
			if (loadout.m_equipmentRecord != null)
			{
				teamMerc.SelectedEquipmentId = loadout.m_equipmentRecord.ID;
			}
			request.Team.MercenaryList.Mercenaries.Add(teamMerc);
		}
		m_connectApi.UpdateMercenariesTeamRequest(request);
	}

	public UpdateMercenariesTeamResponse UpdateMercenariesTeamResponse()
	{
		return m_connectApi.UpdateMercenariesTeamResponse();
	}

	public void MercenariesTeamReorderRequest(LettuceTeam team)
	{
		MercenariesTeamReorderRequest request = new MercenariesTeamReorderRequest
		{
			TeamId = team.ID,
			SortOrder = team.SortOrder
		};
		m_connectApi.MercenariesTeamReorderRequest(request);
	}

	public void DeleteTeam(long teamId)
	{
		DeleteMercenariesTeamRequest request = new DeleteMercenariesTeamRequest
		{
			TeamId = teamId
		};
		m_connectApi.DeleteMercenariesTeamRequest(request);
	}

	public DeleteMercenariesTeamResponse DeleteMercenariesTeamResponse()
	{
		return m_connectApi.DeleteMercenariesTeamResponse();
	}

	public void UpdateMercenariesTeamNameRequest(LettuceTeam team)
	{
		if (team.Name == null)
		{
			Log.Net.PrintError("Network.UpdateMercenariesTeamNameRequest - Team name is null!");
			return;
		}
		UpdateMercenariesTeamNameRequest request = new UpdateMercenariesTeamNameRequest
		{
			TeamId = team.ID,
			TeamName = team.Name
		};
		m_connectApi.UpdateMercenariesTeamNameRequest(request);
	}

	public UpdateMercenariesTeamNameResponse UpdateMercenariesTeamNameResponse()
	{
		return m_connectApi.UpdateMercenariesTeamNameResponse();
	}

	private int GetNextCreateTeamRequestId()
	{
		return ++m_state.CurrentCreateTeamRequestId;
	}

	public void UpdateEquippedMercenaryEquipment(int mercenaryId, int? equipmentId)
	{
		UpdateEquippedMercenaryEquipmentRequest request = new UpdateEquippedMercenaryEquipmentRequest
		{
			MercenaryId = mercenaryId
		};
		if (equipmentId.HasValue)
		{
			request.EquipmentId = equipmentId.Value;
		}
		m_connectApi.UpdateEquippedMercenaryEquipmentRequest(request);
	}

	public UpdateEquippedMercenaryEquipmentResponse UpdateEquippedMercenaryEquipmentResponse()
	{
		return m_connectApi.UpdateEquippedMercenaryEquipmentResponse();
	}

	public void CraftMercenary(int mercenaryId)
	{
		CraftMercenaryRequest request = new CraftMercenaryRequest
		{
			MercenaryId = mercenaryId
		};
		m_connectApi.CraftMercenaryRequest(request);
	}

	public CraftMercenaryResponse CraftMercenaryResponse()
	{
		return m_connectApi.CraftMercenaryResponse();
	}

	public void UpgradeMercenaryAbility(int mercenaryId, int abilityId, uint desiredTier)
	{
		UpgradeMercenaryAbilityRequest request = new UpgradeMercenaryAbilityRequest
		{
			MercenaryId = mercenaryId,
			AbilityId = abilityId,
			DesiredTier = desiredTier
		};
		m_connectApi.UpgradeMercenaryAbilityRequest(request);
	}

	public UpgradeMercenaryAbilityResponse UpgradeMercenaryAbilityResponse()
	{
		return m_connectApi.UpgradeMercenaryAbilityResponse();
	}

	public CraftMercenaryEquipmentResponse CraftMercenaryEquipmentResponse()
	{
		return m_connectApi.CraftMercenaryEquipmentResponse();
	}

	public void UpgradeMercenaryEquipment(int mercenaryId, int equipmentId, uint desiredTier)
	{
		UpgradeMercenaryEquipmentRequest request = new UpgradeMercenaryEquipmentRequest
		{
			MercenaryId = mercenaryId,
			EquipmentId = equipmentId,
			DesiredTier = desiredTier
		};
		m_connectApi.UpgradeMercenaryEquipmentRequest(request);
	}

	public void UpdateEquippedMercenaryArtVariation(int mercenaryId, int artVariationId, TAG_PREMIUM premium)
	{
		int portraitId = LettuceMercenary.GetPortraitIdFromArtVariation(artVariationId, premium);
		if (portraitId != 0)
		{
			UpdateEquippedMercenaryPortraitRequest request = new UpdateEquippedMercenaryPortraitRequest
			{
				MercenaryId = mercenaryId,
				EquippedPortraitId = portraitId
			};
			m_connectApi.UpdateEquippedMercenaryPortraitRequest(request);
		}
	}

	public UpdateEquippedMercenaryPortraitResponse UpdateEquippedMercenaryPortraitResponse()
	{
		return m_connectApi.GetUpdateEquippedMercenaryPortraitResponse();
	}

	public UpgradeMercenaryEquipmentResponse UpgradeMercenaryEquipmentResponse()
	{
		return m_connectApi.UpgradeMercenaryEquipmentResponse();
	}

	public void OpenMercenariesPackRequest()
	{
		OpenMercenariesPackRequest request = new OpenMercenariesPackRequest();
		m_connectApi.OpenMercenariesPackRequest(request);
	}

	public OpenMercenariesPackResponse OpenMercenariesPackResponse()
	{
		return m_connectApi.OpenMercenariesPackResponse();
	}

	public MercenariesPvPRatingUpdate MercenariesPvPRatingUpdate()
	{
		return m_connectApi.MercenariesPvPRatingUpdate();
	}

	public MercenariesPvPWinUpdate MercenariesPvPWinUpdate()
	{
		return m_connectApi.MercenariesPvPWinUpdate();
	}

	public MercenariesPlayerBountyInfoUpdate MercenariesPlayerBountyInfoUpdate()
	{
		return m_connectApi.MercenariesPlayerBountyInfoUpdate();
	}

	public MercenariesTeamUpdate MercenariesTeamUpdate()
	{
		return m_connectApi.MercenariesTeamUpdate();
	}

	public void MercenariesTrainingAddRequest(int mercenaryID)
	{
		m_connectApi.MercenariesTrainingAddRequest(mercenaryID);
	}

	public void MercenariesTrainingRemoveRequest(int mercenaryID)
	{
		m_connectApi.MercenariesTrainingRemoveRequest(mercenaryID);
	}

	public void MercenariesTrainingCollectRequest(int mercenaryID)
	{
		m_connectApi.MercenariesTrainingCollectRequest(mercenaryID);
	}

	public MercenariesTrainingAddResponse MercenariesTrainingAddResponse()
	{
		return m_connectApi.MercenariesTrainingAddResponse();
	}

	public MercenariesTrainingRemoveResponse MercenariesTrainingRemoveResponse()
	{
		return m_connectApi.MercenariesTrainingRemoveResponse();
	}

	public MercenariesTrainingCollectResponse MercenariesTrainingCollectResponse()
	{
		return m_connectApi.MercenariesTrainingCollectResponse();
	}

	public void SendMercenariesDebugCommandRequest(MercenariesDebugCommandRequest request)
	{
		m_connectApi.RequestMercenaryDebugCommand(request);
	}

	public MercenariesDebugCommandResponse MercenariesDebugCommandResponse()
	{
		return m_connectApi.MercenariesDebugCommandResponse();
	}

	public void MercenariesMythicTreasureScalarsRequest()
	{
		MercenariesMythicTreasureScalarsRequest request = new MercenariesMythicTreasureScalarsRequest();
		m_connectApi.MercenariesMythicTreasureScalarsRequest(request);
	}

	public MercenariesMythicTreasureScalarsResponse MercenariesMythicTreasureScalarsResponse()
	{
		return m_connectApi.MercenariesMythicTreasureScalarsResponse();
	}

	public void MercenariesMythicTreasureScalarPurchaseRequest(MercenariesMythicTreasureScalarPurchaseRequest request)
	{
		m_connectApi.MercenariesMythicTreasureScalarPurchaseRequest(request);
	}

	public MercenariesMythicTreasureScalarPurchaseResponse MercenariesMythicTreasureScalarPurchaseResponse()
	{
		return m_connectApi.MercenariesMythicTreasureScalarPurchaseResponse();
	}

	public void MercenariesMythicUpgradeAbilityRequest(MercenariesMythicUpgradeAbilityRequest request)
	{
		m_connectApi.MercenariesMythicUpgradeAbilityRequest(request);
	}

	public MercenariesMythicUpgradeAbilityResponse MercenariesMythicUpgradeAbilityResponse()
	{
		return m_connectApi.MercenariesMythicUpgradeAbilityResponse();
	}

	public void MercenariesMythicUpgradeEquipmentRequest(MercenariesMythicUpgradeEquipmentRequest request)
	{
		m_connectApi.MercenariesMythicUpgradeEquipmentRequest(request);
	}

	public MercenariesMythicUpgradeEquipmentResponse MercenariesMythicUpgradeEquipmentResponse()
	{
		return m_connectApi.MercenariesMythicUpgradeEquipmentResponse();
	}

	public void MercenariesVillageStatusRequest()
	{
		MercenariesGetVillageRequest request = new MercenariesGetVillageRequest();
		m_connectApi.RequestMercenaryVillageStatus(request);
	}

	public MercenariesGetVillageResponse MercenariesVillageStatusResponse()
	{
		return m_connectApi.GetMercenaryVillageStatusResponse();
	}

	public void MercenariesVillageRefreshRequest()
	{
		MercenariesRefreshVillageRequest request = new MercenariesRefreshVillageRequest();
		m_connectApi.RequestMercenaryVillageRefresh(request);
	}

	public MercenariesRefreshVillageResponse MercenariesVillageRefreshResponse()
	{
		return m_connectApi.GetMercenaryVillageRefreshResponse();
	}

	public MercenariesVisitorStateUpdate MercenariesVisitorStateUpdate()
	{
		return m_connectApi.GetMercenaryVisitorStateUpdate();
	}

	public MercenariesBuildingStateUpdate MercenariesBuildingStateUpdate()
	{
		return m_connectApi.GetMercenaryBuildingStateUpdate();
	}

	public void UpgradeMercenaryBuilding(int buildingId, int requestedTierId)
	{
		MercenariesBuildingUpgradeRequest request = new MercenariesBuildingUpgradeRequest
		{
			BuildingId = buildingId,
			RequestedTier = requestedTierId
		};
		m_connectApi.RequestMercenaryBuildingUpgrade(request);
	}

	public MercenariesBuildingUpgradeResponse UpgradeMercenaryBuildingResponse()
	{
		return m_connectApi.GetMercenaryBuildingUpgradeResponse();
	}

	public void ClaimMercenaryTask(int taskId)
	{
		MercenariesClaimTaskRequest request = new MercenariesClaimTaskRequest
		{
			TaskId = taskId
		};
		m_connectApi.RequestMercenaryClaimTask(request);
	}

	public MercenariesClaimTaskResponse ClaimMercenaryTaskResponse()
	{
		return m_connectApi.GetMercenaryClaimTaskResponse();
	}

	public void DismissMercenaryTask(int visitorId)
	{
		MercenariesDismissTaskRequest request = new MercenariesDismissTaskRequest
		{
			VisitorId = visitorId
		};
		m_connectApi.RequestMercenaryDismissTask(request);
	}

	public MercenariesDismissTaskResponse DismissMercenaryTaskResponse()
	{
		return m_connectApi.GetMercenaryDismissTaskResponse();
	}

	public void AcknowledgeBounties(List<int> bountyIds)
	{
		MercenariesBountyAcknowledgeRequest request = new MercenariesBountyAcknowledgeRequest
		{
			BountyIds = bountyIds
		};
		m_connectApi.RequestMercenaryBountyAcknowledge(request);
	}

	public MercenariesBountyAcknowledgeResponse AcknowledgeBountiesResponse()
	{
		return m_connectApi.GetMercenaryBountyAcknowledgeResponse();
	}

	public void AcknowledgeMercenaryCollection(List<MercenaryAcknowledgeData> acknowledgeData)
	{
		MercenariesCollectionAcknowledgeRequest request = new MercenariesCollectionAcknowledgeRequest
		{
			Acknowledgments = acknowledgeData
		};
		m_connectApi.RequestMercenaryCollectionAcknowledge(request);
	}

	public MercenariesCollectionAcknowledgeResponse AcknowledgeMercenaryCollectionResponse()
	{
		return m_connectApi.GetMercenaryCollectionAcknowledgeResponse();
	}

	public void ConvertExcessCoinsToRenown(List<int> mercenaryIds)
	{
		MercenariesConvertExcessCoinsRequest request = new MercenariesConvertExcessCoinsRequest
		{
			MercenaryIds = mercenaryIds
		};
		m_connectApi.RequestConvertExcessCoinsToRenown(request);
	}

	public MercenariesConvertExcessCoinsResponse ConvertExcessCoinsToRenownResponse()
	{
		return m_connectApi.GetMercenariesConvertExcessCoinsResponse();
	}

	public void PurchaseRenownOffer(int renownOfferId)
	{
		MercenariesPurchaseRenownOfferRequest request = new MercenariesPurchaseRenownOfferRequest
		{
			RenownOfferId = renownOfferId
		};
		m_connectApi.RequestPurchaseRenownOffer(request);
	}

	public MercenariesPurchaseRenownOfferResponse PurchaseRenownOfferResponse()
	{
		return m_connectApi.GetPurchaseRenownOfferResponse();
	}

	public void DismissRenownOffer(int renownOfferId)
	{
		MercenariesDismissRenownOfferRequest request = new MercenariesDismissRenownOfferRequest
		{
			RenownOfferId = renownOfferId
		};
		m_connectApi.RequestDismissRenownOffer(request);
	}

	public MercenariesDismissRenownOfferResponse DismissRenownOfferResponse()
	{
		return m_connectApi.GetDismissRenownOfferResponse();
	}

	public MercenariesMythicBountyLevelUpdate MercenariesMythicBountyLevelUpdate()
	{
		return m_connectApi.GetMercenariesMythicBountyLevelUpdate();
	}

	public IEnumerator<IAsyncJobResult> Initialize(ServiceLocator serviceLocator)
	{
		if (PlatformSettings.IsMobileRuntimeOS)
		{
			yield return new WaitForDbfBundleReady();
		}
		m_networkReachabilityManager = ServiceManager.Get<NetworkReachabilityManager>();
		m_breakingNews = ServiceManager.Get<BreakingNews>();
		m_state.SetDefaults();
		if (PlatformSettings.s_isDeviceSupported)
		{
			HearthstoneApplication.Get().WillReset += WillReset;
			HearthstoneApplication.Get().Resetting += OnReset;
			s_running = true;
			CreateNewDispatcher();
			InitBattleNet(m_dispatcherImpl);
			RegisterNetHandler(SubscribeResponse.PacketID.ID, OnSubscribeResponse);
			RegisterNetHandler(ClientStateNotification.PacketID.ID, OnClientStateNotification);
			RegisterNetHandler(PegasusUtil.GenericResponse.PacketID.ID, OnGenericResponse);
			RegisterNetHandler(PegasusUtil.GetDeckContentsResponse.PacketID.ID, OnDeckContentsResponse);
			RegisterNetHandler(CollectionClientStateResponse.PacketID.ID, OnCollectionClientStateResponse);
			RegisterNetHandler(SecondaryClientStateResponse.PacketID.ID, OnSecondaryClientStateResponse);
			OnConnectedToBattleNet += OnConnectedToBattleNetCallback;
			OnDisconnectedFromBattleNet += OnDisconnectedFromBattleNetCallback;
			if (!CONNECT_TO_AURORA_BY_DEFAULT)
			{
				SetShouldBeConnectedToAurora(global::Options.Get().GetBool(Option.CONNECT_TO_AURORA));
			}
		}
	}

	public Type[] GetDependencies()
	{
		return new Type[3]
		{
			typeof(GameDbf),
			typeof(NetworkReachabilityManager),
			typeof(BreakingNews)
		};
	}

	public void Shutdown()
	{
		if (s_running)
		{
			NetCache.Get()?.DispatchClientOptionsToServer();
			PresenceMgr.Get()?.OnShutdown();
			if (IsLoggedIn())
			{
				CancelFindGame();
			}
			m_runtimeGamePacketMonitor.Dispose();
			UnregisterNetworkGameTelemetryEvents();
			ClearTransientBnetPresence();
			CloseAll();
			BattleNet.AppQuit();
			BnetRecentPlayerMgr.Get().Shutdown();
			BnetNearbyPlayerMgr.Get().Shutdown();
			s_running = false;
		}
	}

	private void ClearTransientBnetPresence()
	{
		BattleNet.SetPresenceBlob(17u, null);
		BattleNet.SetPresenceString(19u, string.Empty);
		BattleNet.SetPresenceString(20u, string.Empty);
		BattleNet.SetPresenceBlob(21u, null);
		BattleNet.SetPresenceBlob(23u, null);
		BattleNet.SetPresenceBlob(24u, null);
		BattleNet.SetPresenceBlob(25u, null);
		BattleNet.SetPresenceEntityId(26u, new BnetEntityId(0uL, 0uL));
		BattleNet.SetPresenceBool(1u, val: false);
	}

	private void WillReset()
	{
		NetCache.Get().DispatchClientOptionsToServer();
		NetCache.Get().Clear();
		m_state.DelayedError = null;
		m_state.TimeBeforeAllowReset = 0f;
		if (m_connectApi != null)
		{
			RemoveConnectApiConnectionListeners();
		}
	}

	public void OnReset()
	{
		m_state = default(NetworkState);
		m_state.SetDefaults();
		if (m_connectApi != null)
		{
			RegisterConnectApiConnectionListeners();
		}
		s_running = true;
		CloseAll();
	}

	public bool ResetForNewAuroraConnection()
	{
		Log.Offline.PrintDebug("Resetting for new Aurora Connection");
		NetCache.Get().ClearForNewAuroraConnection();
		m_state.QueuedClientStateNotifications.Clear();
		CloseAll();
		m_dispatcherImpl.ResetForNewConnection();
		m_inTransitRequests.Clear();
		bool resetOk = false;
		BattleNet.RequestCloseAurora();
		if (ShouldBeConnectedToAurora())
		{
			resetOk = BattleNet.Connect(CreateConnectionOptions(), HandleConnectionStatusResult);
			Log.Offline.PrintDebug("ResetForNewAuroraConnection: ResetOk={0}", resetOk);
		}
		if (resetOk || !ShouldBeConnectedToAurora())
		{
			BnetParty.SetDisconnectedFromBattleNet();
			m_connectApi.SetDisconnectedFromBattleNet();
			InitializeConnectApi(m_dispatcherImpl);
		}
		return resetOk;
	}

	private static void HandleConnectionStatusResult(Blizzard.GameService.SDK.Client.Integration.ConnectionStatus status)
	{
		Log.BattleNet.PrintInfo("Connection attempt result: " + status.ToString());
		BnetConnectionStatus bnetConnectionStatus = new BnetConnectionStatus();
		BnetConnectionStatus bnetConnectionStatus2 = bnetConnectionStatus;
		bnetConnectionStatus2.Result_ = status.Result switch
		{
			ConnectionResult.Success => BnetConnectionStatus.Result.Success, 
			ConnectionResult.Failure => BnetConnectionStatus.Result.Failure, 
			ConnectionResult.Cancelled => BnetConnectionStatus.Result.Cancelled, 
			_ => throw new ArgumentOutOfRangeException(), 
		};
		BnetConnectionStatus telem = bnetConnectionStatus;
		if (status.Result == ConnectionResult.Failure && status.FailureReason.HasValue)
		{
			bnetConnectionStatus = telem;
			bnetConnectionStatus.FailureReason_ = status.FailureReason.Value switch
			{
				ConnectionFailureReason.DnSLookupFailed => BnetConnectionStatus.FailureReason.DnSLookupFailed, 
				ConnectionFailureReason.RPCConnectionException => BnetConnectionStatus.FailureReason.RPCConnectionException, 
				ConnectionFailureReason.RPCConnectionFailure => BnetConnectionStatus.FailureReason.RPCConnectionFailure, 
				ConnectionFailureReason.LoginStartFailed => BnetConnectionStatus.FailureReason.LoginStartFailed, 
				ConnectionFailureReason.LoginCompleteFailed => BnetConnectionStatus.FailureReason.LoginCompleteFailed, 
				ConnectionFailureReason.SessionStartFailure => BnetConnectionStatus.FailureReason.SessionStartFailure, 
				ConnectionFailureReason.SessionCreationFailure => BnetConnectionStatus.FailureReason.SessionCreationFailure, 
				ConnectionFailureReason.UnknownException => BnetConnectionStatus.FailureReason.UnknownException, 
				_ => BnetConnectionStatus.FailureReason.UnknownException, 
			};
		}
		if (status.BnetErrorCode != 0)
		{
			telem.BnetErrorCode = (uint)status.BnetErrorCode;
			telem.BnetErrorCodeName = EnumUtils.GetString(status.BnetErrorCode);
		}
		TelemetryManager.Client()?.EnqueueMessage(telem);
	}

	private static ConnectionOptions CreateConnectionOptions()
	{
		string token = ServiceManager.Get<ILoginService>().GetCachedAuthTokenIfAvailable();
		ConnectionOptions result = default(ConnectionOptions);
		result.Host = GetTargetServer();
		result.Port = GetPort();
		result.UseSSL = true;
		result.AuthToken = token;
		return result;
	}

	public static Network Get()
	{
		return ServiceManager.Get<Network>();
	}

	public static float GetMaxDeferredWait()
	{
		return m_maxDeferredWait;
	}

	public static string ProductVersion()
	{
		return 31 + "." + 6 + "." + 0 + "." + 0;
	}

	private void CreateNewDispatcher()
	{
		m_dispatcherListner = new DispatchListener();
		m_runtimeGamePacketMonitor = new RuntimeGamePacketMonitor(m_dispatcherListner);
		BlizzardToUnityLogger gameLogger = new BlizzardToUnityLogger(GameNetLogger);
		m_dispatcherImpl = new QueueDispatcher(new RpcController(gameLogger), new ClientRequestManager(m_dispatcherListner), new PacketDecoderManager(registerDebugDecoders: true), m_dispatcherListner);
		RegisterNetworkGameTelemetryEvents();
	}

	private void RegisterNetworkGameTelemetryEvents()
	{
		DispatchListener dispatcherListner = m_dispatcherListner;
		dispatcherListner.OnGameServerConnect = (Action<BattleNetErrors>)Delegate.Combine(dispatcherListner.OnGameServerConnect, (Action<BattleNetErrors>)delegate(BattleNetErrors errors)
		{
			TelemetryManager.NetworkComponent.OnSendNetworkTelemetryEvents(TelemetryManagerComponentNetwork.NetworkTelemetryEvent.CONNECT, m_dispatcherImpl.CurrentGameServerEndPoint, 0f, errors);
		});
		DispatchListener dispatcherListner2 = m_dispatcherListner;
		dispatcherListner2.OnGameServerDisconnect = (Action<BattleNetErrors>)Delegate.Combine(dispatcherListner2.OnGameServerDisconnect, (Action<BattleNetErrors>)delegate(BattleNetErrors errors)
		{
			TelemetryManager.NetworkComponent.OnSendNetworkTelemetryEvents(TelemetryManagerComponentNetwork.NetworkTelemetryEvent.DISCONNECT, m_dispatcherImpl.CurrentGameServerEndPoint, 0f, errors);
		});
		DispatchListener dispatcherListner3 = m_dispatcherListner;
		dispatcherListner3.OnGamePacketReceived = (Action<PegasusPacket>)Delegate.Combine(dispatcherListner3.OnGamePacketReceived, (Action<PegasusPacket>)delegate(PegasusPacket packet)
		{
			TelemetryManager.NetworkComponent.OnSendNetworkTelemetryEvents(TelemetryManagerComponentNetwork.NetworkTelemetryEvent.RECEIVE, m_dispatcherImpl.CurrentGameServerEndPoint, packet.Size);
		});
		DispatchListener dispatcherListner4 = m_dispatcherListner;
		dispatcherListner4.OnGamePacketSent = (Action<PegasusPacket>)Delegate.Combine(dispatcherListner4.OnGamePacketSent, (Action<PegasusPacket>)delegate(PegasusPacket packet)
		{
			TelemetryManager.NetworkComponent.OnSendNetworkTelemetryEvents(TelemetryManagerComponentNetwork.NetworkTelemetryEvent.SEND, m_dispatcherImpl.CurrentGameServerEndPoint, packet.Size);
		});
		DispatchListener dispatcherListner5 = m_dispatcherListner;
		dispatcherListner5.OnGameServerPing = (Action<float>)Delegate.Combine(dispatcherListner5.OnGameServerPing, (Action<float>)delegate(float travelTime)
		{
			TelemetryManager.NetworkComponent.OnSendNetworkTelemetryEvents(TelemetryManagerComponentNetwork.NetworkTelemetryEvent.PING, m_dispatcherImpl.CurrentGameServerEndPoint, travelTime);
		});
	}

	private void UnregisterNetworkGameTelemetryEvents()
	{
		if (m_dispatcherListner != null && TelemetryManager.NetworkComponent != null)
		{
			DispatchListener dispatcherListner = m_dispatcherListner;
			dispatcherListner.OnGameServerConnect = (Action<BattleNetErrors>)Delegate.Remove(dispatcherListner.OnGameServerConnect, (Action<BattleNetErrors>)delegate(BattleNetErrors errors)
			{
				TelemetryManager.NetworkComponent.OnSendNetworkTelemetryEvents(TelemetryManagerComponentNetwork.NetworkTelemetryEvent.CONNECT, m_dispatcherImpl.CurrentGameServerEndPoint, 0f, errors);
			});
			DispatchListener dispatcherListner2 = m_dispatcherListner;
			dispatcherListner2.OnGameServerDisconnect = (Action<BattleNetErrors>)Delegate.Remove(dispatcherListner2.OnGameServerDisconnect, (Action<BattleNetErrors>)delegate(BattleNetErrors errors)
			{
				TelemetryManager.NetworkComponent.OnSendNetworkTelemetryEvents(TelemetryManagerComponentNetwork.NetworkTelemetryEvent.DISCONNECT, m_dispatcherImpl.CurrentGameServerEndPoint, 0f, errors);
			});
			DispatchListener dispatcherListner3 = m_dispatcherListner;
			dispatcherListner3.OnGamePacketReceived = (Action<PegasusPacket>)Delegate.Remove(dispatcherListner3.OnGamePacketReceived, (Action<PegasusPacket>)delegate(PegasusPacket packet)
			{
				TelemetryManager.NetworkComponent.OnSendNetworkTelemetryEvents(TelemetryManagerComponentNetwork.NetworkTelemetryEvent.RECEIVE, m_dispatcherImpl.CurrentGameServerEndPoint, packet.Size);
			});
			DispatchListener dispatcherListner4 = m_dispatcherListner;
			dispatcherListner4.OnGamePacketSent = (Action<PegasusPacket>)Delegate.Remove(dispatcherListner4.OnGamePacketSent, (Action<PegasusPacket>)delegate(PegasusPacket packet)
			{
				TelemetryManager.NetworkComponent.OnSendNetworkTelemetryEvents(TelemetryManagerComponentNetwork.NetworkTelemetryEvent.SEND, m_dispatcherImpl.CurrentGameServerEndPoint, packet.Size);
			});
			DispatchListener dispatcherListner5 = m_dispatcherListner;
			dispatcherListner5.OnGameServerPing = (Action<float>)Delegate.Remove(dispatcherListner5.OnGameServerPing, (Action<float>)delegate(float travelTime)
			{
				TelemetryManager.NetworkComponent.OnSendNetworkTelemetryEvents(TelemetryManagerComponentNetwork.NetworkTelemetryEvent.PING, m_dispatcherImpl.CurrentGameServerEndPoint, travelTime);
			});
		}
	}

	private void ProcessRequestTimeouts()
	{
		float now = Time.realtimeSinceStartup;
		for (int i = 0; i < m_inTransitRequests.Count; i++)
		{
			RequestContext rc = m_inTransitRequests[i];
			try
			{
				if (rc.m_timeoutHandler != null && rc.m_waitUntil < now)
				{
					Log.Net.PrintWarning($"Encountered timeout waiting for {rc.m_pendingResponseId} {rc.m_requestId} {rc.m_requestSubId}");
					rc.m_timeoutHandler(rc.m_pendingResponseId, rc.m_requestId, rc.m_requestSubId);
				}
			}
			catch (Exception ex)
			{
				Log.Net.PrintError("Exception reporting timeout: " + ex.Message);
				ExceptionReporter.Get()?.ReportCaughtException(ex);
			}
		}
		for (int i2 = m_inTransitRequests.Count - 1; i2 >= 0; i2--)
		{
			if (m_inTransitRequests[i2].m_waitUntil < now)
			{
				m_inTransitRequests.RemoveAt(i2);
			}
		}
	}

	public void AddPendingRequestTimeout(int requestId, int requestSubId)
	{
		if (!ShouldBeConnectedToAurora())
		{
			return;
		}
		int pendingResponseId = 0;
		if ((201 == requestId && m_deferredGetAccountInfoMessageResponseMap.TryGetValue(requestSubId, out pendingResponseId)) || m_deferredMessageResponseMap.TryGetValue(requestId, out pendingResponseId))
		{
			TimeoutHandler timeoutHandler = null;
			if (m_state.NetTimeoutHandlers.TryGetValue(pendingResponseId, out timeoutHandler))
			{
				m_inTransitRequests.Add(new RequestContext(pendingResponseId, requestId, requestSubId, timeoutHandler));
			}
			else
			{
				m_inTransitRequests.Add(new RequestContext(pendingResponseId, requestId, requestSubId, OnRequestTimeout));
			}
		}
	}

	private void RemovePendingRequestTimeout(int pendingResponseId)
	{
		m_inTransitRequests.RemoveAll((RequestContext pc) => pc.m_pendingResponseId == pendingResponseId);
	}

	private static void OnRequestTimeout(int pendingResponseId, int requestId, int requestSubId)
	{
		if (m_deferredMessageResponseMap.ContainsValue(pendingResponseId) || m_deferredGetAccountInfoMessageResponseMap.ContainsValue(pendingResponseId))
		{
			Debug.LogError($"OnRequestTimeout pending ID {pendingResponseId} {requestId} {requestSubId}");
			Get().GameNetLogger.Log(Blizzard.T5.Core.LogLevel.Debug, $"Networ.OnRequestTimeout() - pending ID {pendingResponseId} {requestId} {requestSubId}");
			FatalErrorMgr.Get().SetErrorCode("HS", "NT" + pendingResponseId, requestId.ToString(), requestSubId.ToString());
			TelemetryManager.Client().SendNetworkError(NetworkError.ErrorType.TIMEOUT_DEFERRED_RESPONSE, FatalErrorMgr.Get().GetFormattedErrorCode(), 0);
			Get().ShowBreakingNewsOrError("GLOBAL_ERROR_NETWORK_UNAVAILABLE_UNKNOWN");
		}
		else
		{
			Debug.LogError($"Unhandled OnRequestTimeout pending ID {pendingResponseId} {requestId} {requestSubId}");
			Get().GameNetLogger.Log(Blizzard.T5.Core.LogLevel.Debug, $"Networ.OnRequestTimeout() - Unhandled pending ID {pendingResponseId} {requestId} {requestSubId}");
			FatalErrorMgr.Get().SetErrorCode("HS", "NU" + pendingResponseId, requestId.ToString(), requestSubId.ToString());
			TelemetryManager.Client().SendNetworkError(NetworkError.ErrorType.TIMEOUT_NOT_DEFERRED_RESPONSE, FatalErrorMgr.Get().GetFormattedErrorCode(), 0);
			Get().ShowBreakingNewsOrError("GLOBAL_ERROR_NETWORK_UNAVAILABLE_UNKNOWN");
		}
	}

	private void OnGenericResponse()
	{
		GenericResponse genericResponse = GetGenericResponse();
		if (genericResponse == null)
		{
			Debug.LogError($"Login - GenericResponse parse error");
			return;
		}
		bool num = 201 == genericResponse.RequestId && m_deferredGetAccountInfoMessageResponseMap.ContainsKey(genericResponse.RequestSubId);
		bool isAnotherHandledResponse = m_deferredMessageResponseMap.ContainsKey(genericResponse.RequestId);
		if ((num || isAnotherHandledResponse) && GenericResponse.Result.RESULT_REQUEST_IN_PROCESS != genericResponse.ResultCode && GenericResponse.Result.RESULT_DATA_MIGRATION_REQUIRED != genericResponse.ResultCode)
		{
			Debug.LogError($"Unhandled resultCode {genericResponse.ResultCode} for requestId {genericResponse.RequestId}:{genericResponse.RequestSubId}");
			FatalErrorMgr.Get().SetErrorCode("HS", "NG" + genericResponse.ResultCode, genericResponse.RequestId.ToString(), genericResponse.RequestSubId.ToString());
			TelemetryManager.Client().SendNetworkError(NetworkError.ErrorType.REQUEST_ERROR, FatalErrorMgr.Get().GetFormattedErrorCode(), 0);
			ShowBreakingNewsOrError("GLOBAL_ERROR_NETWORK_UNAVAILABLE_UNKNOWN");
		}
	}

	public static bool IsRunning()
	{
		return s_running;
	}

	public double TimeSinceLastPong()
	{
		if (!IsConnectedToGameServer() || m_gameServerKeepAliveFrequencySeconds == 0 || m_connectApi.GetTimeLastPingSent() <= m_connectApi.GetTimeLastPingReceieved())
		{
			return 0.0;
		}
		return (double)Time.realtimeSinceStartup - m_connectApi.GetTimeLastPingReceieved();
	}

	private void OnSubscribeResponse()
	{
		SubscribeResponse packet = m_connectApi.GetSubscribeResponse();
		if (packet != null && packet.HasRequestMaxWaitSecs && packet.RequestMaxWaitSecs >= 30)
		{
			m_maxDeferredWait = packet.RequestMaxWaitSecs;
		}
	}

	private void OnClientStateNotification()
	{
		ClientStateNotification packet = m_connectApi.GetClientStateNotification();
		if (!NetCache.Get().HasReceivedInitialClientState)
		{
			m_state.QueuedClientStateNotifications.Add(packet);
			TelemetryManager.Client().SendInitialClientStateOutOfOrder(packet.HasAchievementNotifications ? packet.AchievementNotifications.AchievementNotifications_.Count : 0, packet.HasNoticeNotifications ? packet.NoticeNotifications.NoticeNotifications_.Count : 0, packet.HasCollectionModifications ? packet.CollectionModifications.CardModifications.Sum((CardModification m) => m.Quantity) : 0, packet.HasCurrencyState ? 1 : 0, packet.HasBoosterModifications ? packet.BoosterModifications.Modifications.Sum((BoosterInfo m) => m.Count) : 0, packet.HasHeroXp ? packet.HeroXp.XpInfos.Count : 0, packet.HasPlayerRecords ? packet.PlayerRecords.Records.Count : 0, packet.HasArenaSessionResponse ? 1 : 0, packet.HasCardBackModifications ? packet.CardBackModifications.CardBackModifications_.Count : 0);
		}
		else
		{
			ProcessClientStateNotification(packet);
		}
	}

	public static void ProcessClientStateNotification(ClientStateNotification packet)
	{
		if (packet.HasCurrencyState)
		{
			try
			{
				NetCache.Get().OnCurrencyState(packet.CurrencyState);
			}
			catch (Exception e)
			{
				ReportClientStateNotificationException(e, "Currency State");
			}
		}
		if (packet.HasCollectionModifications)
		{
			try
			{
				NetCache.Get().OnCollectionModification(packet);
			}
			catch (Exception e2)
			{
				ReportClientStateNotificationException(e2, "Collection Modifications");
			}
		}
		else
		{
			if (packet.HasAchievementNotifications)
			{
				try
				{
					AchieveManager.Get().OnAchievementNotifications(packet.AchievementNotifications.AchievementNotifications_);
				}
				catch (Exception e3)
				{
					ReportClientStateNotificationException(e3, "Achievement Notifications");
				}
			}
			if (packet.HasNoticeNotifications)
			{
				try
				{
					Get().OnNoticeNotifications(packet.NoticeNotifications);
				}
				catch (Exception e4)
				{
					ReportClientStateNotificationException(e4, "Notice Notifications");
				}
			}
			if (packet.HasBoosterModifications)
			{
				try
				{
					NetCache.Get().OnBoosterModifications(packet.BoosterModifications);
				}
				catch (Exception e5)
				{
					ReportClientStateNotificationException(e5, "Booster Modifications");
				}
			}
		}
		if (packet.HasBattlegroundsGuideSkinModifications)
		{
			try
			{
				NetCache.Get().OnBattlegroundsGuideSkinModifications(packet.BattlegroundsGuideSkinModifications);
			}
			catch (Exception e6)
			{
				ReportClientStateNotificationException(e6, "Battlegrounds Guide Skin Modifications");
			}
		}
		if (packet.HasBattlegroundsHeroSkinModifications)
		{
			try
			{
				NetCache.Get().OnBattlegroundsHeroSkinModifications(packet.BattlegroundsHeroSkinModifications);
			}
			catch (Exception e7)
			{
				ReportClientStateNotificationException(e7, "Battlegrounds Hero Skin Modifications");
			}
		}
		if (packet.HasBattlegroundsBoardSkinModifications)
		{
			try
			{
				NetCache.Get().OnBattlegroundsBoardSkinModifications(packet.BattlegroundsBoardSkinModifications);
			}
			catch (Exception e8)
			{
				ReportClientStateNotificationException(e8, "Battlegrounds Board Skin Modifications");
			}
		}
		if (packet.HasBattlegroundsFinisherModifications)
		{
			try
			{
				NetCache.Get().OnBattlegroundsFinisherModifications(packet.BattlegroundsFinisherModifications);
			}
			catch (Exception e9)
			{
				ReportClientStateNotificationException(e9, "battlegrounds Finisher Modifications");
			}
		}
		if (packet.HasBattlegroundsEmoteModifications)
		{
			try
			{
				NetCache.Get().OnBattlegroundsEmoteModifications(packet.BattlegroundsEmoteModifications);
			}
			catch (Exception e10)
			{
				ReportClientStateNotificationException(e10, "Battlegrounds Emote Modifications");
			}
		}
		if (packet.HasHeroXp)
		{
			try
			{
				NetCache.Get().OnHeroXP(packet.HeroXp);
			}
			catch (Exception e11)
			{
				ReportClientStateNotificationException(e11, "Hero XP");
			}
		}
		if (packet.HasPlayerRecords)
		{
			try
			{
				NetCache.Get().OnPlayerRecordsPacket(packet.PlayerRecords);
			}
			catch (Exception e12)
			{
				ReportClientStateNotificationException(e12, "Player Records");
			}
		}
		if (packet.HasArenaSessionResponse)
		{
			try
			{
				DraftManager.Get().OnArenaSessionResponsePacket(packet.ArenaSessionResponse);
			}
			catch (Exception e13)
			{
				ReportClientStateNotificationException(e13, "Arena Session Response");
			}
		}
		if (packet.HasCardBackModifications)
		{
			try
			{
				NetCache.Get().OnCardBackModifications(packet.CardBackModifications);
			}
			catch (Exception e14)
			{
				ReportClientStateNotificationException(e14, "Card Back Modifications");
			}
		}
		if (packet.HasPlayerDraftTickets)
		{
			try
			{
				NetCache.Get().OnPlayerDraftTickets(packet.PlayerDraftTickets);
			}
			catch (Exception e15)
			{
				ReportClientStateNotificationException(e15, "Player Draft Tickets");
			}
		}
	}

	private static void ReportClientStateNotificationException(Exception e, string clientStateType)
	{
		Log.Net.PrintError("Exception processing client state notification type: " + clientStateType + ", exception message: " + e.Message + " \n " + e.StackTrace);
		ExceptionReporter.Get().ReportCaughtException(e);
	}

	public void OnInitialClientStateProcessed()
	{
		List<ClientStateNotification> list = new List<ClientStateNotification>(m_state.QueuedClientStateNotifications);
		m_state.QueuedClientStateNotifications.Clear();
		foreach (ClientStateNotification packet in list)
		{
			try
			{
				ProcessClientStateNotification(packet);
			}
			catch (Exception e)
			{
				ReportClientStateNotificationException(e, "Initial Client State");
			}
		}
	}

	public void OnNoticeNotifications(NoticeNotifications packet)
	{
		List<ProfileNotice> noticeList = new List<ProfileNotice>();
		List<NetCache.ProfileNotice> result = new List<NetCache.ProfileNotice>();
		for (int i = 0; i < packet.NoticeNotifications_.Count; i++)
		{
			NoticeNotification notice = packet.NoticeNotifications_[i];
			noticeList.Add(notice.Notice);
		}
		HandleProfileNotices(noticeList, ref result);
		NetCache.Get().HandleIncomingProfileNotices(result, isInitialNoticeList: false);
	}

	public void UpdateCachedBnetValues()
	{
		m_state.CachedGameAccountId = BattleNet.GetMyGameAccountId();
		m_state.CachedRegion = BattleNet.GetCurrentRegion();
	}

	public void OverrideKeepAliveSeconds(uint value)
	{
		if (HearthstoneApplication.IsInternal())
		{
			m_gameServerKeepAliveFrequencySeconds = value;
		}
	}

	public BnetGameAccountId GetMyGameAccountId()
	{
		BnetGameAccountId gameAccountId = BattleNet.GetMyGameAccountId();
		if (gameAccountId.High == 0L && gameAccountId.Low == 0L)
		{
			return m_state.CachedGameAccountId;
		}
		return gameAccountId;
	}

	public BnetRegion GetCurrentRegion()
	{
		BnetRegion currentRegion = BattleNet.GetCurrentRegion();
		if (currentRegion == BnetRegion.REGION_UNINITIALIZED)
		{
			return m_state.CachedRegion;
		}
		return currentRegion;
	}

	private void InitializeConnectApi(IDispatcher dispatcher)
	{
		m_errorList.Clear();
		if (m_connectApi == null)
		{
			GameServerDisconnectEvents = new List<BattleNetErrors>();
			DebugConnectionManager debugConnectionManager = new DebugConnectionManager();
			m_connectApi = new ConnectAPI(dispatcher, debugConnectionManager);
			RegisterConnectApiConnectionListeners();
		}
		m_connectApi.SetGameStartState(GameStartState.Invalid);
	}

	public static void ApplicationPaused()
	{
		if (NetCache.Get() != null)
		{
			NetCache.Get().DispatchClientOptionsToServer();
		}
		if (ServiceManager.TryGet<Network>(out var network) && network.m_connectApi != null)
		{
			network.m_connectApi.ProcessUtilPackets();
		}
		BattleNet.ApplicationWasPaused();
	}

	public void CloseAll()
	{
		if (m_ackCardSeenPacket.CardDefs.Count != 0)
		{
			SendAckCardsSeen();
		}
		CheckForSendingBattlegroundsSkinsSeenPacket(1);
		if (m_connectApi != null)
		{
			m_connectApi.Close();
		}
	}

	public static void ApplicationUnpaused()
	{
		BattleNet.ApplicationWasUnpaused();
	}

	public void Update()
	{
		if (s_running)
		{
			try
			{
				ProcessRequestTimeouts();
			}
			catch (Exception ex)
			{
				Log.Net.PrintError("Exception processing timeouts: " + ex.Message);
				ExceptionReporter.Get().ReportCaughtException(ex);
			}
			try
			{
				ProcessNetworkReachability();
			}
			catch (Exception ex2)
			{
				Log.Net.PrintError("Exception processing network reachability: " + ex2.Message);
				ExceptionReporter.Get().ReportCaughtException(ex2);
			}
			try
			{
				ProcessConnectApiHeartbeat();
			}
			catch (Exception ex3)
			{
				Log.Net.PrintError("Exception processing api heartbeat: " + ex3.Message);
				ExceptionReporter.Get().ReportCaughtException(ex3);
			}
			try
			{
				StoreManager.Get().Heartbeat();
			}
			catch (Exception ex4)
			{
				Log.Net.PrintError("Exception processing store heartbeat: " + ex4.Message);
				ExceptionReporter.Get().ReportCaughtException(ex4);
			}
			float now = Time.realtimeSinceStartup;
			float gap = now - m_state.LastCall;
			if (!(gap < PROCESS_WARNING) && !(now - m_state.LastCallReport < PROCESS_WARNING_REPORT_GAP))
			{
				m_state.LastCallReport = now;
				string gapString = TimeUtils.GetDevElapsedTimeString(gap);
				Debug.LogWarning($"Network.ProcessNetwork not called for {gapString}");
			}
		}
	}

	private void ProcessConnectApiHeartbeat()
	{
		GetBattleNetPackets();
		int numErrors = m_errorList.Count;
		for (int i = 0; i < numErrors; i++)
		{
			ConnectErrorParams error = m_errorList[i];
			if (error == null)
			{
				Debug.LogError("null error! " + m_errorList.Count);
			}
			else if (Time.realtimeSinceStartup >= error.m_creationTime + 0.4f)
			{
				m_errorList.RemoveAt(i);
				i--;
				numErrors = m_errorList.Count;
				Error.AddFatal(error);
			}
		}
		if (m_connectApi != null)
		{
			if (m_connectApi.IsConnectedToGameServer())
			{
				UpdatePingPong();
			}
			m_connectApi.ProcessUtilPackets();
			if (m_connectApi.TryConnectDebugConsole())
			{
				m_connectApi.UpdateDebugConsole();
			}
		}
	}

	private void ProcessNetworkReachability()
	{
		if (!IsLoggedIn())
		{
			return;
		}
		if (!m_networkReachabilityManager.InternetAvailable_Cached)
		{
			if (IsInGame())
			{
				double currentTime = TimeUtils.GetElapsedTimeSinceEpoch(null).TotalSeconds;
				if (m_timeInternetUnreachable == 0.0)
				{
					m_timeInternetUnreachable = currentTime;
					return;
				}
				if (currentTime - m_timeInternetUnreachable < (double)m_gameServerKeepAliveWaitForInternetSeconds)
				{
					return;
				}
			}
			Log.Offline.PrintError("Network.ProcessInternetReachability(): Access to the Internet has been lost.");
			Error.AddFatal(FatalErrorReason.NO_INTERNET_ACCESS, "GLOBAL_ERROR_NETWORK_DISCONNECT");
			return;
		}
		if (m_timeInternetUnreachable != 0.0)
		{
			double currentTime2 = TimeUtils.GetElapsedTimeSinceEpoch(null).TotalSeconds;
			TelemetryManager.Client().SendNetworkUnreachableRecovered((int)(currentTime2 - m_timeInternetUnreachable));
			if (IsInGame())
			{
				DisconnectFromGameServer(DisconnectReason.Network_Unreachable);
			}
		}
		m_timeInternetUnreachable = 0.0;
	}

	public void AddErrorToList(ConnectErrorParams errorParams)
	{
		m_errorList.Add(errorParams);
	}

	public void SetShouldIgnorePong(bool value)
	{
		m_connectApi.SetShouldIgnorePong(value);
	}

	public void SetSpoofDisconnected(bool value)
	{
		m_connectApi.SetSpoofDisconnected(value);
	}

	private void GetBattleNetPackets()
	{
		UtilResponse response;
		while ((response = BattleNet.NextUtilPacket()) != null)
		{
			try
			{
				PegasusPacket packet = new PegasusPacket(response.type, response.bytes.Length, response.bytes);
				packet.Context = response.context;
				m_connectApi.DecodeAndProcessPacket(packet);
			}
			catch (Exception ex)
			{
				Log.Net.PrintError("Excpetion processsing packet " + response?.ToString() + ", Error: " + ex.Message + " , " + ex.StackTrace);
				ExceptionReporter.Get().ReportCaughtException(ex);
			}
		}
	}

	public void AppAbort()
	{
		if (s_running)
		{
			NetCache.Get().DispatchClientOptionsToServer();
			PresenceMgr.Get().OnShutdown();
			CancelFindGame();
			CloseAll();
			ClearTransientBnetPresence();
			BattleNet.AppQuit();
			BnetRecentPlayerMgr.Get().Shutdown();
			BnetNearbyPlayerMgr.Get().Shutdown();
			s_running = false;
		}
	}

	public void ResetConnectionFailureCount()
	{
		m_numConnectionFailures = 0;
	}

	public bool RegisterNetHandler(object enumId, NetHandler handler, TimeoutHandler timeoutHandler = null)
	{
		int id = (int)enumId;
		if (timeoutHandler != null)
		{
			if (m_state.NetTimeoutHandlers.ContainsKey(id))
			{
				return false;
			}
			m_state.NetTimeoutHandlers.Add(id, timeoutHandler);
		}
		if (m_netHandlers.TryGetValue(id, out var handlers))
		{
			if (handlers.Contains(handler))
			{
				return false;
			}
		}
		else
		{
			handlers = new List<NetHandler>();
			m_netHandlers.Add(id, handlers);
		}
		handlers.Add(handler);
		return true;
	}

	public bool RemoveNetHandler(object enumId, NetHandler handler)
	{
		int id = (int)enumId;
		if (m_netHandlers.TryGetValue(id, out var handlers) && handlers.Remove(handler))
		{
			return true;
		}
		return false;
	}

	public void RegisterThrottledPacketListener(ThrottledPacketListener listener)
	{
		if (!m_throttledPacketListeners.Contains(listener))
		{
			m_throttledPacketListeners.Add(listener);
		}
	}

	public bool FakeHandleType(Enum enumId)
	{
		int id = Convert.ToInt32(enumId);
		return FakeHandleType(id);
	}

	public bool FakeHandleType(int id)
	{
		if (!ShouldBeConnectedToAurora())
		{
			HandleType(id);
			return true;
		}
		return false;
	}

	private bool HandleType(int id)
	{
		RemovePendingRequestTimeout(id);
		if (!m_netHandlers.TryGetValue(id, out var handlers) || handlers.Count == 0)
		{
			if (!CanIgnoreUnhandledPacket(id))
			{
				Debug.LogError($"Network.HandleType() - Received packet {id}, but there are no handlers for it.");
			}
			return false;
		}
		NetHandler[] handlersCopy = handlers.ToArray();
		for (int i = 0; i < handlersCopy.Length; i++)
		{
			try
			{
				handlersCopy[i]();
			}
			catch (Exception ex)
			{
				Log.Net.PrintError($"Excpetion handling packet id {id}: {ex.Message}{ex.StackTrace}");
				ExceptionReporter.Get().ReportCaughtException(ex);
			}
		}
		return true;
	}

	private bool CanIgnoreUnhandledPacket(int id)
	{
		if (id == 15 || id == 116 || id == 254)
		{
			return true;
		}
		return false;
	}

	private bool ProcessUtilServer()
	{
		int id = m_connectApi.NextUtilPacketType();
		bool result = HandleType(id);
		m_connectApi.DropUtilPacket();
		return result;
	}

	private bool ProcessConsole()
	{
		int id = m_connectApi.NextDebugPacketType();
		bool result = HandleType(id);
		m_connectApi.DropDebugPacket();
		return result;
	}

	public void CraftingTransaction(CraftingPendingTransaction transaction, int expectedTransactionCost, int normalOwned, int goldenOwned, int signatureOwned, int diamondOwned)
	{
		PegasusShared.CardDef def = new PegasusShared.CardDef
		{
			Asset = GameUtils.TranslateCardIdToDbId(transaction.CardID),
			Premium = (int)transaction.Premium
		};
		m_connectApi.CraftingTransaction(transaction, def, expectedTransactionCost, normalOwned, goldenOwned, signatureOwned, diamondOwned);
	}

	public void SetClientOptions(SetOptions packet)
	{
		m_connectApi.SetClientOptions(packet);
	}

	public static ConnectionState BattleNetStatus()
	{
		return BattleNet.BattleNetStatus();
	}

	public static bool IsLoggedIn()
	{
		if (!BattleNet.IsInitialized())
		{
			return false;
		}
		return BattleNet.BattleNetStatus() == ConnectionState.Ready;
	}

	public bool HaveUnhandledPackets()
	{
		if (m_connectApi.HasUtilPackets())
		{
			return true;
		}
		if (m_connectApi.HasGamePackets())
		{
			return true;
		}
		if (m_connectApi.HasDebugPackets())
		{
			return true;
		}
		if (BattleNet.GetNotificationCount() > 0)
		{
			return true;
		}
		return false;
	}

	public void ProcessNetwork()
	{
		if (!s_running || m_state.LastCallFrame == Time.frameCount)
		{
			return;
		}
		m_state.LastCallFrame = Time.frameCount;
		m_state.LastCall = Time.realtimeSinceStartup;
		if (ShouldBeConnectedToAurora())
		{
			ProcessAurora();
		}
		else
		{
			ProcessDelayedError();
		}
		if (ProcessGameQueue())
		{
			return;
		}
		if (m_connectApi.HasGamePackets())
		{
			ProcessGameServer();
			return;
		}
		ProcessGameServerDisconnectEvents();
		if (m_connectApi.HasUtilPackets())
		{
			ProcessUtilServer();
		}
		else if (m_connectApi.HasDebugPackets())
		{
			ProcessConsole();
		}
		else
		{
			ProcessQueuePosition();
		}
	}

	public static void StartInitalBattleNetConnection()
	{
		Log.Net.PrintDebug("StartInitalBattleNetConnection");
		if (BattleNet.IsInitialized())
		{
			Log.Net.PrintDebug("Tried to connect to battle.net when already initialized");
			return;
		}
		if (BattleNet.Get() == null)
		{
			BattleNet.SetImpl(CreateBattleNetImplementation());
		}
		ConnectionOptions connectionOptions = CreateConnectionOptions();
		Log.Net.PrintDebug($"StartInitalBattleNetConnection: BattleNet.Connect with: {connectionOptions.Host}:{connectionOptions.Port} , Auth: {connectionOptions.AuthToken} ");
		BattleNet.Connect(connectionOptions, HandleConnectionStatusResult);
	}

	private void InitBattleNet(IDispatcher dispatcher)
	{
		if (!BattleNet.IsInitialized())
		{
			if (BattleNet.Get() == null)
			{
				BattleNet.SetImpl(CreateBattleNetImplementation());
			}
			AddBnetErrorListener(BnetFeature.Auth, OnBnetAuthError);
			InitializeConnectApi(dispatcher);
		}
	}

	private static IBattleNet CreateBattleNetImplementation()
	{
		ClientInterface ci = new HSClientInterface();
		LoggerInterface loggerInterface = BuildLoggerInterface();
		BattleNetCSharp battleNet = new BattleNetCSharp(ci, loggerInterface, TelemetryManager.NetworkComponent);
		Debug.LogFormat("*** BattleNet version: Product = {0}, Data = {1}", ci.GetVersion(), ci.GetDataVersion());
		return battleNet;
	}

	private static LoggerInterface BuildLoggerInterface()
	{
		Logger logger = LogSystem.Get().GetFullLogger("BattleNet");
		return BattleNetLoggerBuilder.BuildLoggerInterface(TelemetryManager.Client(), logger, new BnetErrorAdaptor());
	}

	private void OnConnectedToBattleNetCallback(BattleNetErrors error)
	{
		TelemetryManager.OnBattleNetConnect(BattleNet.GetEnvironment(), (int)BattleNet.GetPort(), error);
	}

	private void OnDisconnectedFromBattleNetCallback(BattleNetErrors error)
	{
		TelemetryManager.OnBattleNetDisconnect(BattleNet.GetEnvironment(), (int)BattleNet.GetPort(), error);
	}

	public static bool ShouldBeConnectedToAurora()
	{
		return s_shouldBeConnectedToAurora;
	}

	public static void SetShouldBeConnectedToAurora(bool shouldBeConnected)
	{
		s_shouldBeConnectedToAurora = shouldBeConnected;
	}

	public bool ShouldBeConnectedToAurora_NONSTATIC()
	{
		return s_shouldBeConnectedToAurora;
	}

	public void ProcessQueuePosition()
	{
		Blizzard.GameService.SDK.Client.Integration.QueueInfo bgsQueueInfo = default(Blizzard.GameService.SDK.Client.Integration.QueueInfo);
		BattleNet.GetQueueInfo(ref bgsQueueInfo);
		if (!bgsQueueInfo.changed || m_queueInfoHandler == null)
		{
			return;
		}
		QueueInfo queueInfo = new QueueInfo();
		queueInfo.position = bgsQueueInfo.position;
		queueInfo.secondsTilEnd = bgsQueueInfo.end;
		queueInfo.stdev = bgsQueueInfo.stdev;
		try
		{
			m_queueInfoHandler(queueInfo);
		}
		catch (Exception ex)
		{
			Log.Net.PrintError("Exception processing queue position: " + ex.Message);
			ExceptionReporter.Get().ReportCaughtException(ex);
		}
	}

	public void SetFriendsHandler(FriendsHandler handler)
	{
		m_state.CurrentFriendsHandler = handler;
	}

	public void SetWhisperHandler(WhisperHandler handler)
	{
		m_state.CurrentWhisperHandler = handler;
	}

	public void SetPresenceHandler(PresenceHandler handler)
	{
		m_state.CurrentPresenceHandler = handler;
	}

	public void SetShutdownHandler(ShutdownHandler handler)
	{
		m_state.CurrentShutdownHandler = handler;
	}

	public void AddBnetErrorListener(BnetFeature feature, BnetErrorCallback callback)
	{
		AddBnetErrorListener(feature, callback, null);
	}

	public void AddBnetErrorListener(BnetFeature feature, BnetErrorCallback callback, object userData)
	{
		BnetErrorListener listener = new BnetErrorListener();
		listener.SetCallback(callback);
		listener.SetUserData(userData);
		if (!m_state.FeatureBnetErrorListeners.TryGetValue(feature, out var listeners))
		{
			listeners = new List<BnetErrorListener>();
			m_state.FeatureBnetErrorListeners.Add(feature, listeners);
		}
		else if (listeners.Contains(listener))
		{
			return;
		}
		listeners.Add(listener);
	}

	public void AddBnetErrorListener(BnetErrorCallback callback)
	{
		AddBnetErrorListener(callback, null);
	}

	public void AddBnetErrorListener(BnetErrorCallback callback, object userData)
	{
		BnetErrorListener listener = new BnetErrorListener();
		listener.SetCallback(callback);
		listener.SetUserData(userData);
		if (!m_state.GlobalBnetErrorListeners.Contains(listener))
		{
			m_state.GlobalBnetErrorListeners.Add(listener);
		}
	}

	public bool RemoveBnetErrorListener(BnetFeature feature, BnetErrorCallback callback)
	{
		return RemoveBnetErrorListener(feature, callback, null);
	}

	public bool RemoveBnetErrorListener(BnetFeature feature, BnetErrorCallback callback, object userData)
	{
		if (!m_state.FeatureBnetErrorListeners.TryGetValue(feature, out var listeners))
		{
			return false;
		}
		BnetErrorListener listener = new BnetErrorListener();
		listener.SetCallback(callback);
		listener.SetUserData(userData);
		return listeners.Remove(listener);
	}

	public bool RemoveBnetErrorListener(BnetErrorCallback callback)
	{
		return RemoveBnetErrorListener(callback, null);
	}

	public bool RemoveBnetErrorListener(BnetErrorCallback callback, object userData)
	{
		BnetErrorListener listener = new BnetErrorListener();
		listener.SetCallback(callback);
		listener.SetUserData(userData);
		return m_state.GlobalBnetErrorListeners.Remove(listener);
	}

	public void SendUnsubcribeRequest(Unsubscribe packet, UtilSystemId systemChannel)
	{
		m_connectApi.SendUnsubscribeRequest(packet, systemChannel);
	}

	public void ProcessAurora()
	{
		ProcessBnetEvents();
		if (IsLoggedIn())
		{
			ProcessPresence();
			ProcessFriends();
			ProcessWhispers();
			ProcessParties();
			ProcessBroadcasts();
			ProcessNotifications();
			ProcessRecentPlayers();
			ProcessNearbyPlayers();
		}
		ProcessErrors();
	}

	private static void ProcessNearbyPlayers()
	{
		try
		{
			BnetNearbyPlayerMgr.Get().Update();
		}
		catch (Exception ex)
		{
			Log.Net.PrintError("Exception processing nearby players " + ex.Message);
			ExceptionReporter.Get().ReportCaughtException(ex);
		}
	}

	private static void ProcessRecentPlayers()
	{
		try
		{
			BnetRecentPlayerMgr.Get().Update();
		}
		catch (Exception ex)
		{
			Log.Net.PrintError("Exception processing recent players " + ex.Message);
			ExceptionReporter.Get().ReportCaughtException(ex);
		}
	}

	private void ProcessBnetEvents()
	{
		BattleNet.TakeBnetEvents(m_bnetEvents);
		foreach (BnetEvent bnetEvent in m_bnetEvents)
		{
			switch (bnetEvent.EventType)
			{
			case ConnectionState.Ready:
				SafeInvokeDelegatesBnetEvent(this.OnConnectedToBattleNet, bnetEvent.EventData);
				break;
			case ConnectionState.Disconnected:
				SafeInvokeDelegatesBnetEvent(this.OnDisconnectedFromBattleNet, bnetEvent.EventData);
				break;
			}
		}
		m_bnetEvents.Clear();
	}

	private void SafeInvokeDelegatesBnetEvent(MulticastDelegate multicastDelegate, BattleNetErrors arg)
	{
		Delegate[] invocationList = multicastDelegate.GetInvocationList();
		for (int i = 0; i < invocationList.Length; i++)
		{
			Action<BattleNetErrors> evt = (Action<BattleNetErrors>)invocationList[i];
			try
			{
				evt(arg);
			}
			catch (Exception ex)
			{
				Log.Net.PrintError("Exception invoking event " + ex.Message);
				ExceptionReporter.Get().ReportCaughtException(ex);
			}
		}
	}

	private void ProcessWhispers()
	{
		if (m_state.CurrentWhisperHandler == null)
		{
			return;
		}
		BattleNet.TakeWhispers(m_bnetWhispers);
		if (m_bnetWhispers.Count > 0)
		{
			try
			{
				m_state.CurrentWhisperHandler(m_bnetWhispers.ToArray());
			}
			catch (Exception ex)
			{
				Log.Net.PrintError("Exception processing whispers: " + ex.Message);
				ExceptionReporter.Get().ReportCaughtException(ex);
			}
			m_bnetWhispers.Clear();
		}
	}

	private void ProcessParties()
	{
		try
		{
			BnetParty.Process();
		}
		catch (Exception ex)
		{
			Log.Net.PrintError("Exception processing parties: " + ex.Message);
			ExceptionReporter.Get().ReportCaughtException(ex);
		}
	}

	private void ProcessBroadcasts()
	{
		int minutes = BattleNet.GetShutdownMinutes();
		if (minutes <= 0 || m_state.CurrentShutdownHandler == null)
		{
			return;
		}
		try
		{
			m_state.CurrentShutdownHandler(minutes);
		}
		catch (Exception ex)
		{
			Log.Net.PrintError("Exception processing broadcasts: " + ex.Message);
			ExceptionReporter.Get().ReportCaughtException(ex);
		}
	}

	private void ProcessNotifications()
	{
		BattleNet.TakeNotifications(ref m_bnetNotifications);
		foreach (BnetNotification notification in m_bnetNotifications)
		{
			try
			{
				ProcessUtilNotification(notification);
			}
			catch (Exception ex)
			{
				Log.Net.PrintError("Exception processing notifications: " + ex.Message);
				ExceptionReporter.Get().ReportCaughtException(ex);
			}
		}
		m_bnetNotifications.Clear();
	}

	private void ProcessUtilNotification(BnetNotification notification)
	{
		if (!(notification.NotificationType != "WTCG.UtilNotificationMessage"))
		{
			PegasusPacket packet = new PegasusPacket(notification.MessageType, 0, notification.MessageSize, notification.BlobMessage);
			m_connectApi.DecodeAndProcessPacket(packet);
		}
	}

	private void ProcessFriends()
	{
		if (m_state.CurrentFriendsHandler == null)
		{
			return;
		}
		BattleNet.TakeFriendsUpdates(m_friendsUpdates);
		if (m_friendsUpdates.Count > 0)
		{
			try
			{
				m_state.CurrentFriendsHandler(m_friendsUpdates.ToArray());
			}
			catch (Exception ex)
			{
				Log.Net.PrintError("Exception processing friends: " + ex.Message);
				ExceptionReporter.Get().ReportCaughtException(ex);
			}
			m_friendsUpdates.Clear();
		}
	}

	private void ProcessPresence()
	{
		if (m_state.CurrentPresenceHandler == null)
		{
			return;
		}
		BattleNet.TakePresence(m_presenceUpdates);
		if (m_presenceUpdates.Count > 0)
		{
			try
			{
				m_state.CurrentPresenceHandler(m_presenceUpdates.ToArray());
			}
			catch (Exception ex)
			{
				Log.Net.PrintError("Exception processing presence: " + ex.Message);
				ExceptionReporter.Get().ReportCaughtException(ex);
			}
			m_presenceUpdates.Clear();
		}
	}

	private void ProcessErrors()
	{
		ProcessDelayedError();
		if (m_connectApi.HasErrors())
		{
			BnetErrorInfo error = new BnetErrorInfo(BnetFeature.Games, BnetFeatureEvent.Games_OnClientRequest, BattleNetErrors.ERROR_GAME_UTILITY_SERVER_NO_SERVER);
			m_bnetErrors.Add(error);
		}
		else
		{
			BattleNet.TakeErrors(m_bnetErrors);
			if (m_bnetErrors.Count == 0)
			{
				return;
			}
		}
		for (int i = 0; i < m_bnetErrors.Count; i++)
		{
			BnetErrorInfo error2 = m_bnetErrors[i];
			BattleNetErrors errorCode = error2.GetError();
			if (errorCode == (BattleNetErrors)1003013u)
			{
				BattleNet.ClearErrors();
				HearthstoneApplication.Get().Reset();
				return;
			}
			string errorName = (HearthstoneApplication.IsPublic() ? "" : errorCode.ToString());
			if (!m_connectApi.HasErrors() && m_connectApi.ShouldIgnoreError(error2))
			{
				if (!HearthstoneApplication.IsPublic())
				{
					Log.BattleNet.PrintDebug("BattleNet/ConnectDLL generated error={0} {1} (can ignore)", (int)errorCode, errorName);
				}
			}
			else if (!FireErrorListeners(error2) && (m_connectApi.HasErrors() || !OnIgnorableBnetError(error2)))
			{
				OnFatalBnetError(error2);
			}
		}
		m_bnetErrors.Clear();
	}

	private bool FireErrorListeners(BnetErrorInfo info)
	{
		bool handled = false;
		if (m_state.FeatureBnetErrorListeners.TryGetValue(info.GetFeature(), out var featureListeners) && featureListeners.Count > 0)
		{
			BnetErrorListener[] featureListenersCopy = featureListeners.ToArray();
			for (int i = 0; i < featureListenersCopy.Length; i++)
			{
				try
				{
					handled = featureListenersCopy[i].Fire(info) || handled;
				}
				catch (Exception ex)
				{
					Log.Net.PrintError("Exception firing error listener " + ex.Message);
					ExceptionReporter.Get().ReportCaughtException(ex);
				}
			}
		}
		BnetErrorListener[] globalListenersCopy = m_state.GlobalBnetErrorListeners.ToArray();
		foreach (BnetErrorListener globalListener in globalListenersCopy)
		{
			try
			{
				handled = globalListener.Fire(info) || handled;
			}
			catch (Exception ex2)
			{
				Log.Net.PrintError("Exception firing error listener " + ex2.Message);
				ExceptionReporter.Get().ReportCaughtException(ex2);
			}
		}
		return handled;
	}

	public void ShowConnectionFailureError(string error)
	{
		ShowBreakingNewsOrError(error, DelayForConnectionFailures(m_numConnectionFailures++));
	}

	public void ShowBreakingNewsOrError(string error, float timeBeforeAllowReset = 0f)
	{
		GameNetLogger.Log(Blizzard.T5.Core.LogLevel.Debug, "Networ.ShowBreakingNewsOrError() - error: " + error);
		m_state.DelayedError = error;
		m_state.TimeBeforeAllowReset = timeBeforeAllowReset;
		Debug.LogError($"Setting delayed error for Error Message: {error} and prevent reset for {timeBeforeAllowReset} seconds");
		ProcessDelayedError();
	}

	private bool ProcessDelayedError()
	{
		if (m_state.DelayedError == null)
		{
			return false;
		}
		if (m_breakingNews.GetStatus() == BreakingNews.Status.Fetching)
		{
			return false;
		}
		ErrorParams parms = new ErrorParams();
		parms.m_delayBeforeNextReset = m_state.TimeBeforeAllowReset;
		string breakingNews = m_breakingNews.GetText();
		if (string.IsNullOrEmpty(breakingNews))
		{
			if (m_breakingNews.GetError() != null && m_state.DelayedError == "GLOBAL_ERROR_NETWORK_NO_GAME_SERVER")
			{
				parms.m_message = GameStrings.Format("GLOBAL_ERROR_NETWORK_NO_CONNECTION");
			}
			else if (HearthstoneApplication.IsInternal() && m_state.DelayedError == "GLOBAL_ERROR_UNKNOWN_ERROR")
			{
				parms.m_message = "Dev Message: Could not connect to Battle.net, and there was no breaking news to display. Maybe Battle.net is down?";
			}
			else
			{
				parms.m_message = GameStrings.Format(m_state.DelayedError);
				if (m_state.DelayedError == "GLOBAL_MOBILE_ERROR_GAMESERVER_CONNECT")
				{
					parms.m_reason = FatalErrorReason.MOBILE_GAME_SERVER_RPC_ERROR;
				}
			}
		}
		else
		{
			parms.m_message = GameStrings.Format("GLOBAL_MOBILE_ERROR_BREAKING_NEWS", breakingNews);
			parms.m_reason = FatalErrorReason.BREAKING_NEWS;
		}
		GameNetLogger.Log(Blizzard.T5.Core.LogLevel.Debug, $"Network.ProcessDelayedError() - reason: {parms.m_reason}, message: {parms.m_message}");
		GameNetLogger.Log(Blizzard.T5.Core.LogLevel.Error, GetNetworkGameStateStringForErrors());
		Error.AddFatal(parms);
		m_state.DelayedError = null;
		m_state.TimeBeforeAllowReset = 0f;
		return true;
	}

	public bool OnIgnorableBnetError(BnetErrorInfo info)
	{
		BattleNetErrors error = info.GetError();
		bool canBeIgnored = false;
		switch (error)
		{
		case BattleNetErrors.ERROR_OK:
			canBeIgnored = true;
			break;
		case BattleNetErrors.ERROR_GAME_UTILITY_SERVER_NO_SERVER:
			m_state.LogSource.LogError("Network.IgnoreBnetError() - error={0}", info);
			canBeIgnored = true;
			break;
		case BattleNetErrors.ERROR_TARGET_OFFLINE:
			canBeIgnored = true;
			break;
		case BattleNetErrors.ERROR_INVALID_TARGET_ID:
			canBeIgnored = info.GetFeature() == BnetFeature.Friends && info.GetFeatureEvent() == BnetFeatureEvent.Friends_OnSendInvitation;
			break;
		case BattleNetErrors.ERROR_API_NOT_READY:
			canBeIgnored = info.GetFeature() == BnetFeature.Presence;
			break;
		case BattleNetErrors.ERROR_FRIENDS_FRIENDSHIP_ALREADY_EXISTS:
		case BattleNetErrors.ERROR_FRIENDS_INVITATION_ALREADY_EXISTS:
		case BattleNetErrors.ERROR_FRIENDS_INVITEE_AT_MAX_FRIENDS:
		case BattleNetErrors.ERROR_FRIENDS_INVITER_AT_MAX_FRIENDS:
		case BattleNetErrors.ERROR_FRIENDS_INVITER_IS_BLOCKED_BY_INVITEE:
			canBeIgnored = true;
			break;
		case BattleNetErrors.ERROR_INVALID_ARGS:
		case BattleNetErrors.ERROR_REPORT_UNAVAILABLE:
			canBeIgnored = info.GetFeature() == BnetFeature.Report;
			break;
		}
		if (error != BattleNetErrors.ERROR_OK && canBeIgnored && !ignorableErrorsSent.Contains((int)error))
		{
			ignorableErrorsSent.Add((int)error);
			TelemetryManager.Client().SendIgnorableBattleNetError((int)error, error.ToString());
		}
		return canBeIgnored;
	}

	private void ReportUnknownNetErrorToTelem(BnetErrorInfo info)
	{
		BattleNetErrors error = info.GetError();
		if (!m_sentTelemNetworkErrors.Contains(error))
		{
			TelemetryManager.Client().SendNetworkError(NetworkError.ErrorType.OTHER_UNKNOWN, info.ToString(), (int)info.GetError());
			m_sentTelemNetworkErrors.Add(error);
		}
	}

	private void ReportErrorToTelemetry(BattleNetErrors error)
	{
		if (!m_sentTelemErrors.Contains(error))
		{
			TelemetryManager.Client().SendFatalBattleNetError((int)error, error.ToString());
			m_sentTelemErrors.Add(error);
		}
	}

	private float DelayForConnectionFailures(int numFailures)
	{
		float delayPerFailure = (float)(new System.Random().NextDouble() * 3.0) + 3.5f;
		return (float)Math.Min(numFailures, 3) * delayPerFailure;
	}

	private bool OnBnetAuthError(BnetErrorInfo info, object userData)
	{
		return false;
	}

	public static void AcceptFriendInvite(BnetInvitationId inviteid)
	{
		BattleNet.ManageFriendInvite(1, inviteid.GetVal());
	}

	public static void IgnoreFriendInvite(BnetInvitationId inviteid)
	{
		BattleNet.ManageFriendInvite(4, inviteid.GetVal());
	}

	private static void SendFriendInvite(string sender, string target, bool byEmail)
	{
		BattleNet.SendFriendInvite(sender, target, byEmail);
	}

	public static void SendFriendInviteByEmail(string sender, string target)
	{
		SendFriendInvite(sender, target, byEmail: true);
	}

	public static void SendFriendInviteByBattleTag(string sender, string target)
	{
		SendFriendInvite(sender, target, byEmail: false);
	}

	public static void RemoveFriend(BnetAccountId id)
	{
		BattleNet.RemoveFriend(id);
	}

	public static void SendWhisper(BnetAccountId account, string message)
	{
		BattleNet.SendWhisper(account, message);
	}

	public static string GetUsername()
	{
		string username = null;
		try
		{
			username = DeviceLocaleHelper.GetStoredUserName();
		}
		catch (Exception ex)
		{
			Debug.LogError("Exception while loading settings: " + ex.Message);
		}
		if (username == null)
		{
			username = Vars.Key("Aurora.Username").GetStr("NOT_PROVIDED_PLEASE_PROVIDE_VIA_CONFIG");
		}
		if (username != null && username.IndexOf("@") == -1)
		{
			username += "@blizzard.com";
		}
		return username;
	}

	public static string GetTargetServer()
	{
		return DeviceLocaleHelper.GetTargetServer();
	}

	public static uint GetPort()
	{
		return DeviceLocaleHelper.GetPort();
	}

	public static string GetVersion()
	{
		return DeviceLocaleHelper.GetVersion();
	}

	public void OnLoginStarted()
	{
		m_connectApi.OnLoginStarted();
	}

	public void DoLoginUpdate()
	{
		string referral = Vars.Key("Application.Referral").GetStr("none");
		if (referral.Equals("none"))
		{
			if (PlatformSettings.OS == OSCategory.PC || PlatformSettings.OS == OSCategory.Mac)
			{
				referral = "Battle.net";
			}
			else if (PlatformSettings.OS == OSCategory.iOS)
			{
				referral = "AppleAppStore";
			}
			else if (PlatformSettings.OS == OSCategory.Android)
			{
				switch (AndroidDeviceSettings.Get().GetAndroidStore())
				{
				case AndroidStore.GOOGLE:
					referral = "GooglePlay";
					break;
				case AndroidStore.AMAZON:
					referral = "AmazonAppStore";
					break;
				case AndroidStore.HUAWEI:
					referral = "HuaweiAppStore";
					break;
				case AndroidStore.DASHEN:
					referral = "Dashen";
					break;
				case AndroidStore.ONE_STORE:
					referral = "OneStore";
					break;
				case AndroidStore.BLIZZARD:
					if (PlatformSettings.LocaleVariant == LocaleVariant.China)
					{
						referral = "JV-Android";
					}
					break;
				}
			}
		}
		m_connectApi.DoLoginUpdate(referral);
	}

	public void OnStartupPacketSequenceComplete()
	{
		m_connectApi.OnStartupPacketSequenceComplete();
	}

	public void BattlegroundsPartyLeaderChangedFindingGameState(int newGameState)
	{
		if (Enum.IsDefined(typeof(BnetGameType), newGameState))
		{
			m_state.FindingBnetGameType = (BnetGameType)newGameState;
		}
	}

	private static bool RequiresScenarioIdAttribute(PegasusShared.GameType gameType)
	{
		if (gameType == PegasusShared.GameType.GT_VS_FRIEND)
		{
			return true;
		}
		if (GameUtils.IsTavernBrawlGameType(gameType))
		{
			return true;
		}
		return false;
	}

	private void ThrowDnsResolveError(string environment)
	{
		if (HearthstoneApplication.IsInternal())
		{
			Error.AddDevFatal("Environment " + environment + " could not be resolved! Please check your environment and Internet connection!");
		}
		else
		{
			Error.AddFatal(FatalErrorReason.DNS_RESOLVE, "GLOBAL_ERROR_NETWORK_NO_CONNECTION");
		}
	}

	public TeammatesEntities GetTeammateEntities()
	{
		return m_connectApi.GetTeammateEntities();
	}

	public TeammatesChooseEntities GetTeammatesChooseEntities()
	{
		return m_connectApi.GetTeammatesChooseEntities();
	}

	public TeammatesEntitiesChosen GetTeammatesEntitiesChosen()
	{
		return m_connectApi.GetTeammatesEntitiesChosen();
	}

	public TeammateConcede GetTeammateConcede()
	{
		return m_connectApi.GetTeammateConcede();
	}

	public EntityPinged GetEntityPinged()
	{
		return m_connectApi.GetEntityPinged();
	}

	public ReplaceBattlegroundMulliganHero ReplaceBattlegroundMulliganHero()
	{
		return m_connectApi.ReplaceBattlegroundMulliganHero();
	}

	public AchievementComplete GetAchievementComplete()
	{
		return m_connectApi.GetAchievementComplete();
	}

	public void SendFreeDeckChoice(int deckTemplateId, bool bypassTrialPeriod)
	{
		m_connectApi.SendFreeDeckChoice(deckTemplateId, bypassTrialPeriod);
	}

	public void ValidateAchieve(int achieveID)
	{
		Log.Achievements.Print("Validating achieve: " + achieveID);
		m_connectApi.ValidateAchieve(achieveID);
	}

	public ValidateAchieveResponse GetValidatedAchieve()
	{
		return m_connectApi.GetValidateAchieveResponse();
	}

	public void RequestCancelQuest(int achieveID)
	{
		m_connectApi.RequestCancelQuest(achieveID);
	}

	public CanceledQuest GetCanceledQuest()
	{
		CancelQuestResponse packet = m_connectApi.GetCanceledQuestResponse();
		if (packet == null)
		{
			return null;
		}
		return new CanceledQuest
		{
			AchieveID = packet.QuestId,
			Canceled = packet.Success,
			NextQuestCancelDate = (packet.HasNextQuestCancel ? TimeUtils.PegDateToFileTimeUtc(packet.NextQuestCancel) : 0)
		};
	}

	public TriggeredEvent GetTriggerEventResponse()
	{
		TriggerEventResponse packet = m_connectApi.GetTriggerEventResponse();
		if (packet == null)
		{
			return null;
		}
		return new TriggeredEvent
		{
			EventID = packet.EventId,
			Success = packet.Success
		};
	}

	public void RequestAdventureProgress()
	{
		m_connectApi.RequestAdventureProgress();
	}

	public List<AdventureProgress> GetAdventureProgressResponse()
	{
		AdventureProgressResponse packet = m_connectApi.GetAdventureProgressResponse();
		if (packet == null)
		{
			return null;
		}
		List<AdventureProgress> result = new List<AdventureProgress>();
		for (int i = 0; i < packet.List.Count; i++)
		{
			PegasusShared.AdventureProgress packetProgress = packet.List[i];
			AdventureProgress progress = new AdventureProgress();
			progress.Wing = packetProgress.WingId;
			progress.Progress = packetProgress.Progress;
			progress.Ack = packetProgress.Ack;
			progress.Flags = packetProgress.Flags_;
			result.Add(progress);
		}
		return result;
	}

	public BeginDraft GetBeginDraft()
	{
		DraftBeginning packet = m_connectApi.GetDraftBeginning();
		if (packet == null)
		{
			return null;
		}
		BeginDraft result = new BeginDraft();
		result.DeckID = packet.DeckId;
		for (int i = 0; i < packet.ChoiceList.Count; i++)
		{
			PegasusShared.CardDef choice = packet.ChoiceList[i];
			NetCache.CardDefinition resultChoice = new NetCache.CardDefinition
			{
				Name = GameUtils.TranslateDbIdToCardId(choice.Asset),
				Premium = (TAG_PREMIUM)choice.Premium
			};
			result.Heroes.Add(resultChoice);
		}
		result.Wins = (packet.HasCurrentSession ? packet.CurrentSession.Wins : 0);
		result.MaxSlot = packet.MaxSlot;
		if (packet.HasCurrentSession)
		{
			result.Session = packet.CurrentSession;
		}
		result.SlotType = packet.SlotType;
		result.UniqueSlotTypesForDraft = packet.UniqueSlotTypes;
		return result;
	}

	public DraftError GetDraftError()
	{
		return m_connectApi.DraftGetError();
	}

	public DraftChoicesAndContents GetDraftChoicesAndContents()
	{
		PegasusUtil.DraftChoicesAndContents packet = m_connectApi.GetDraftChoicesAndContents();
		if (packet == null)
		{
			return null;
		}
		DraftChoicesAndContents result = new DraftChoicesAndContents();
		result.DeckInfo.Deck = packet.DeckId;
		result.Slot = packet.Slot;
		result.Hero.Name = ((packet.HeroDef.Asset == 0) ? string.Empty : GameUtils.TranslateDbIdToCardId(packet.HeroDef.Asset));
		result.Hero.Premium = (TAG_PREMIUM)packet.HeroDef.Premium;
		result.Wins = packet.CurrentSession.Wins;
		result.Losses = packet.CurrentSession.Losses;
		result.MaxWins = (packet.HasMaxWins ? packet.MaxWins : int.MaxValue);
		result.MaxSlot = packet.MaxSlot;
		if (packet.HasCurrentSession)
		{
			result.Session = packet.CurrentSession;
		}
		if (packet.HasHeroPowerDef)
		{
			result.HeroPower.Name = ((packet.HeroPowerDef.Asset == 0) ? string.Empty : GameUtils.TranslateDbIdToCardId(packet.HeroPowerDef.Asset));
		}
		for (int i = 0; i < packet.ChoiceList.Count; i++)
		{
			PegasusShared.CardDef choice = packet.ChoiceList[i];
			if (choice.Asset != 0)
			{
				NetCache.CardDefinition resultChoice = new NetCache.CardDefinition
				{
					Name = GameUtils.TranslateDbIdToCardId(choice.Asset),
					Premium = (TAG_PREMIUM)choice.Premium
				};
				result.Choices.Add(resultChoice);
			}
		}
		for (int j = 0; j < packet.Cards.Count; j++)
		{
			DeckCardData card = packet.Cards[j];
			CardUserData cardToInsert = new CardUserData();
			cardToInsert.DbId = card.Def.Asset;
			cardToInsert.Count = ((!card.HasQty) ? 1 : card.Qty);
			cardToInsert.Premium = (card.Def.HasPremium ? ((TAG_PREMIUM)card.Def.Premium) : TAG_PREMIUM.NORMAL);
			result.DeckInfo.Cards.Add(cardToInsert);
		}
		result.Chest = (packet.HasChest ? ConvertRewardChest(packet.Chest) : null);
		result.SlotType = packet.SlotType;
		result.UniqueSlotTypesForDraft.AddRange(packet.UniqueSlotTypes);
		return result;
	}

	public DraftChosen GetDraftChosen()
	{
		PegasusUtil.DraftChosen packet = m_connectApi.GetDraftChosen();
		if (packet == null)
		{
			return null;
		}
		NetCache.CardDefinition chosenCard = new NetCache.CardDefinition
		{
			Name = GameUtils.TranslateDbIdToCardId(packet.Chosen.Asset),
			Premium = (TAG_PREMIUM)packet.Chosen.Premium
		};
		List<NetCache.CardDefinition> nextChoices = new List<NetCache.CardDefinition>();
		for (int i = 0; i < packet.NextChoiceList.Count; i++)
		{
			PegasusShared.CardDef choice = packet.NextChoiceList[i];
			NetCache.CardDefinition resultChoice = new NetCache.CardDefinition
			{
				Name = GameUtils.TranslateDbIdToCardId(choice.Asset),
				Premium = (TAG_PREMIUM)choice.Premium
			};
			nextChoices.Add(resultChoice);
		}
		return new DraftChosen
		{
			ChosenCard = chosenCard,
			NextChoices = nextChoices,
			SlotType = packet.SlotType
		};
	}

	public void MakeDraftChoice(long deckID, int slot, int index, int premium)
	{
		m_connectApi.DraftMakePick(deckID, slot, index, premium);
	}

	public void RequestDraftChoicesAndContents()
	{
		m_connectApi.RequestDraftChoicesAndContents();
	}

	public void SendArenaSessionRequest()
	{
		m_connectApi.SendArenaSessionRequest();
	}

	public ArenaSessionResponse GetArenaSessionResponse()
	{
		return m_connectApi.GetArenaSessionResponse();
	}

	public void DraftBegin()
	{
		m_connectApi.DraftBegin();
	}

	public void DraftRetire(long deckID, int slot, int seasonId)
	{
		m_connectApi.DraftRetire(deckID, slot, seasonId);
	}

	public DraftRetired GetRetiredDraft()
	{
		PegasusUtil.DraftRetired packet = m_connectApi.GetDraftRetired();
		if (packet == null)
		{
			return null;
		}
		return new DraftRetired
		{
			Deck = packet.DeckId,
			Chest = ConvertRewardChest(packet.Chest)
		};
	}

	public void AckDraftRewards(long deckID, int slot)
	{
		m_connectApi.DraftAckRewards(deckID, slot);
	}

	public long GetRewardsAckDraftID()
	{
		return m_connectApi.DraftRewardsAcked()?.DeckId ?? 0;
	}

	public void DraftRequestDisablePremiums()
	{
		m_connectApi.DraftRequestDisablePremiums();
	}

	public DraftChoicesAndContents GetDraftRemovePremiumsResponse()
	{
		DraftRemovePremiumsResponse packet = m_connectApi.GetDraftDisablePremiumsResponse();
		DraftChoicesAndContents result = new DraftChoicesAndContents();
		for (int i = 0; i < packet.ChoiceList.Count; i++)
		{
			PegasusShared.CardDef choice = packet.ChoiceList[i];
			if (choice.Asset != 0)
			{
				NetCache.CardDefinition resultChoice = new NetCache.CardDefinition
				{
					Name = GameUtils.TranslateDbIdToCardId(choice.Asset),
					Premium = (TAG_PREMIUM)choice.Premium
				};
				result.Choices.Add(resultChoice);
			}
		}
		for (int j = 0; j < packet.Cards.Count; j++)
		{
			DeckCardData card = packet.Cards[j];
			CardUserData cardToInsert = new CardUserData();
			cardToInsert.DbId = card.Def.Asset;
			cardToInsert.Count = ((!card.HasQty) ? 1 : card.Qty);
			cardToInsert.Premium = (card.Def.HasPremium ? ((TAG_PREMIUM)card.Def.Premium) : TAG_PREMIUM.NORMAL);
			result.DeckInfo.Cards.Add(cardToInsert);
		}
		return result;
	}

	public static RewardChest ConvertRewardChest(PegasusShared.RewardChest chest)
	{
		RewardChest result = new RewardChest();
		for (int i = 0; i < chest.Bag.Count; i++)
		{
			result.Rewards.Add(ConvertRewardBag(chest.Bag[i]));
		}
		return result;
	}

	public static RewardData ConvertRewardBag(RewardBag bag)
	{
		if (bag.HasRewardBooster)
		{
			return new BoosterPackRewardData(bag.RewardBooster.BoosterType, bag.RewardBooster.BoosterCount);
		}
		if (bag.HasRewardCard)
		{
			return new CardRewardData(GameUtils.TranslateDbIdToCardId(bag.RewardCard.Card.Asset), (TAG_PREMIUM)bag.RewardCard.Card.Premium, bag.RewardCard.Quantity);
		}
		if (bag.HasRewardDust)
		{
			return new ArcaneDustRewardData(bag.RewardDust.Amount);
		}
		if (bag.HasRewardGold)
		{
			return new GoldRewardData(bag.RewardGold.Amount);
		}
		if (bag.HasRewardCardBack)
		{
			return new CardBackRewardData(bag.RewardCardBack.CardBack);
		}
		if (bag.HasRewardArenaTicket)
		{
			return new ForgeTicketRewardData(bag.RewardArenaTicket.Quantity);
		}
		if (bag.HasRewardMercenariesCurrency)
		{
			return new MercenaryCoinRewardData(bag.RewardMercenariesCurrency.MercenaryId, (int)bag.RewardMercenariesCurrency.CurrencyDelta);
		}
		if (bag.HasRewardMercenariesExperience)
		{
			return new MercenaryExpRewardData(bag.RewardMercenariesExperience.MercenaryId, (int)bag.RewardMercenariesExperience.PreExp, (int)bag.RewardMercenariesExperience.PostExp, (int)bag.RewardMercenariesExperience.ExpDelta);
		}
		if (bag.HasRewardMercenariesEquipment)
		{
			return new MercenariesEquipmentRewardData(bag.RewardMercenariesEquipment.MercenaryId, bag.RewardMercenariesEquipment.EquipmentId, (int)bag.RewardMercenariesEquipment.EquipmentTier);
		}
		if (bag.HasRewardRandomMercenary)
		{
			return RewardUtils.CreateMercenaryOrKnockoutRewardData(bag.RewardRandomMercenary.MercenaryId, bag.RewardRandomMercenary.ArtVariationId, (TAG_PREMIUM)bag.RewardRandomMercenary.ArtVariationPremium, (int)bag.RewardRandomMercenary.CurrencyAmount);
		}
		if (bag.HasRewardRenown)
		{
			return new MercenaryRenownRewardData(bag.RewardRenown.Amount);
		}
		if (bag.HasRewardBattlegroundsToken)
		{
			return new BattlegroundsTokenRewardData(bag.RewardBattlegroundsToken.Amount);
		}
		Debug.LogError("Unrecognized reward bag reward");
		return null;
	}

	public void MassDisenchant()
	{
		m_connectApi.MassDisenchant();
	}

	public MassDisenchantResponse GetMassDisenchantResponse()
	{
		PegasusUtil.MassDisenchantResponse packet = m_connectApi.GetMassDisenchantResponse();
		if (packet == null)
		{
			return null;
		}
		if (packet.HasCollectionVersion)
		{
			NetCache.Get().AddExpectedCollectionModification(packet.CollectionVersion);
		}
		return new MassDisenchantResponse
		{
			Amount = packet.Amount
		};
	}

	public void SetFavoriteHero(TAG_CLASS heroClass, NetCache.CardDefinition hero, bool isFavorite)
	{
		PegasusShared.CardDef cardDef = new PegasusShared.CardDef
		{
			Asset = GameUtils.TranslateCardIdToDbId(hero.Name),
			Premium = (int)hero.Premium
		};
		if (IsLoggedIn())
		{
			m_connectApi.SetFavoriteHero((int)heroClass, cardDef, isFavorite);
			return;
		}
		OfflineDataCache.SetFavoriteHero((int)heroClass, cardDef, wasCalledOffline: true, isFavorite);
		NetCache.NetCacheFavoriteHeroes netCacheFavoriteHeroes = NetCache.Get().GetNetObject<NetCache.NetCacheFavoriteHeroes>();
		if (netCacheFavoriteHeroes != null)
		{
			if (isFavorite)
			{
				netCacheFavoriteHeroes.FavoriteHeroes.Add((heroClass, hero));
			}
			else
			{
				netCacheFavoriteHeroes.FavoriteHeroes.Remove((heroClass, hero));
			}
		}
	}

	public void SetTag(int tagID, int entityID, int tagValue)
	{
		SendDebugConsoleCommand($"settag {tagID} {entityID} {tagValue}");
	}

	public void SetTag(int tagID, string entityIdentifier, int tagValue)
	{
		SendDebugConsoleCommand($"settag {tagID} {entityIdentifier} {tagValue}");
	}

	public void PrintPersistentList(int entityID)
	{
		SendDebugConsoleCommand($"printpersistentlist {entityID}");
	}

	public void DebugScript(string powerGUID)
	{
		SendDebugConsoleCommand($"debugscript {powerGUID}");
	}

	public void DisableScriptDebug()
	{
		SendDebugConsoleCommand("disablescriptdebug");
	}

	public void DebugRopeTimer()
	{
		SendDebugConsoleCommand("debugropetimer");
	}

	public void DisableDebugRopeTimer()
	{
		SendDebugConsoleCommand("disabledebugropetimer");
	}

	public SetFavoriteHeroResponse GetSetFavoriteHeroResponse()
	{
		PegasusUtil.SetFavoriteHeroResponse packet = m_connectApi.GetSetFavoriteHeroResponse();
		if (packet == null)
		{
			return null;
		}
		SetFavoriteHeroResponse result = new SetFavoriteHeroResponse();
		result.Success = packet.Success;
		result.IsFavorite = packet.IsFavorite;
		if (packet.HasFavoriteHero)
		{
			if (!EnumUtils.TryCast<TAG_CLASS>(packet.FavoriteHero.ClassId, out result.HeroClass))
			{
				Debug.LogWarning($"Network.GetSetFavoriteHeroResponse() invalid class {packet.FavoriteHero.ClassId}");
			}
			if (!EnumUtils.TryCast<TAG_PREMIUM>(packet.FavoriteHero.Hero.Premium, out var heroPremium))
			{
				Debug.LogWarning($"Network.GetSetFavoriteHeroResponse() invalid heroPremium {packet.FavoriteHero.Hero.Premium}");
			}
			result.Hero = new NetCache.CardDefinition
			{
				Name = GameUtils.TranslateDbIdToCardId(packet.FavoriteHero.Hero.Asset),
				Premium = heroPremium
			};
		}
		return result;
	}

	public void RequestRecruitAFriendUrl()
	{
		m_connectApi.RequestRecruitAFriendUrl(GetPlatformBuilder());
	}

	public RecruitAFriendURLResponse GetRecruitAFriendUrlResponse()
	{
		return m_connectApi.GetRecruitAFriendUrlResponse();
	}

	public void RequestRecruitAFriendData()
	{
		m_connectApi.RequestRecruitAFriendData();
	}

	public RecruitAFriendDataResponse GetRecruitAFriendDataResponse()
	{
		return m_connectApi.GetRecruitAFriendDataResponse();
	}

	public void RequestProcessRecruitAFriend()
	{
		m_connectApi.RequestProcessRecruitAFriend();
	}

	public PurchaseCanceledResponse GetPurchaseCanceledResponse()
	{
		CancelPurchaseResponse packet = m_connectApi.GetCancelPurchaseResponse();
		if (packet == null)
		{
			return null;
		}
		PurchaseCanceledResponse result = new PurchaseCanceledResponse
		{
			TransactionID = (packet.HasTransactionId ? packet.TransactionId : 0),
			PMTProductID = (packet.HasPmtProductId ? new long?(packet.PmtProductId) : ((long?)null)),
			CurrencyCode = packet.CurrencyCode
		};
		switch (packet.Result)
		{
		case CancelPurchaseResponse.CancelResult.CR_SUCCESS:
			result.Result = PurchaseCanceledResponse.CancelResult.SUCCESS;
			break;
		case CancelPurchaseResponse.CancelResult.CR_NOT_ALLOWED:
			result.Result = PurchaseCanceledResponse.CancelResult.NOT_ALLOWED;
			break;
		case CancelPurchaseResponse.CancelResult.CR_NOTHING_TO_CANCEL:
			result.Result = PurchaseCanceledResponse.CancelResult.NOTHING_TO_CANCEL;
			break;
		}
		return result;
	}

	public BattlePayStatus GetBattlePayStatusResponse()
	{
		BattlePayStatusResponse packet = m_connectApi.GetBattlePayStatusResponse();
		if (packet == null)
		{
			return null;
		}
		BattlePayStatus result = new BattlePayStatus
		{
			State = (BattlePayStatus.PurchaseState)packet.Status,
			BattlePayAvailable = packet.BattlePayAvailable,
			CurrencyCode = packet.CurrencyCode
		};
		if (packet.HasTransactionId)
		{
			result.TransactionID = packet.TransactionId;
		}
		if (packet.HasPmtProductId)
		{
			result.PMTProductID = packet.PmtProductId;
		}
		if (packet.HasPurchaseError)
		{
			result.PurchaseError = ConvertPurchaseError(packet.PurchaseError);
		}
		if (packet.HasThirdPartyId)
		{
			result.ThirdPartyID = packet.ThirdPartyId;
		}
		if (packet.HasProvider)
		{
			result.Provider = packet.Provider;
		}
		return result;
	}

	private PurchaseErrorInfo ConvertPurchaseError(PurchaseError purchaseError)
	{
		PurchaseErrorInfo result = new PurchaseErrorInfo
		{
			Error = (PurchaseErrorInfo.ErrorType)purchaseError.Error_
		};
		if (purchaseError.HasPurchaseInProgress)
		{
			result.PurchaseInProgressProductID = purchaseError.PurchaseInProgress;
		}
		if (purchaseError.HasErrorCode)
		{
			result.ErrorCode = purchaseError.ErrorCode;
		}
		return result;
	}

	public BattlePayConfig GetBattlePayConfigResponse()
	{
		BattlePayConfigResponse packet = m_connectApi.GetBattlePayConfigResponse();
		if (packet == null)
		{
			return null;
		}
		BattlePayConfig result = new BattlePayConfig
		{
			Available = (!packet.HasUnavailable || !packet.Unavailable),
			SecondsBeforeAutoCancel = (packet.HasSecsBeforeAutoCancel ? packet.SecsBeforeAutoCancel : StoreManager.DEFAULT_SECONDS_BEFORE_AUTO_CANCEL)
		};
		if (packet.HasCheckoutKrOnestoreKey)
		{
			result.CheckoutKrOnestoreKey = packet.CheckoutKrOnestoreKey;
		}
		foreach (PegasusShared.Currency currency2 in packet.Currencies)
		{
			Currency currency = new Currency(currency2);
			result.Currencies.Add(currency);
			if (currency.Code == packet.DefaultCurrencyCode)
			{
				result.Currency = currency;
			}
		}
		foreach (PegasusUtil.Bundle bundle in packet.Bundles)
		{
			result.Bundles.Add(bundle.ToNetBundle(result.Currency));
		}
		foreach (PegasusUtil.GoldCostBooster goldCostBooster in packet.GoldCostBoosters)
		{
			GoldCostBooster resultGoldCostBooster = new GoldCostBooster
			{
				ID = goldCostBooster.PackType
			};
			if (goldCostBooster.Cost > 0)
			{
				resultGoldCostBooster.Cost = goldCostBooster.Cost;
			}
			else
			{
				resultGoldCostBooster.Cost = null;
			}
			if (goldCostBooster.HasBuyWithGoldEventName)
			{
				resultGoldCostBooster.BuyWithGoldEvent = EventTimingManager.Get().GetEventType(goldCostBooster.BuyWithGoldEventName);
			}
			result.GoldCostBoosters.Add(resultGoldCostBooster);
		}
		if (packet.HasGoldCostArena && packet.GoldCostArena > 0)
		{
			result.GoldCostArena = packet.GoldCostArena;
		}
		else
		{
			result.GoldCostArena = null;
		}
		if (packet.HasCheckoutOauthClientId && !string.IsNullOrEmpty(packet.CheckoutOauthClientId))
		{
			result.CommerceClientID = packet.CheckoutOauthClientId;
		}
		foreach (string shopPageId in packet.ShopPageIds)
		{
			if (!string.IsNullOrEmpty(shopPageId))
			{
				result.ShopPageIds.Add(shopPageId);
			}
		}
		if (packet.LocaleMap != null)
		{
			foreach (LocaleMapEntry mapEntry in packet.LocaleMap)
			{
				result.CatalogLocaleToGameLocale.Add(mapEntry.CatalogLocaleId, (Locale)mapEntry.GameLocaleId);
			}
		}
		foreach (Locale locale in Enum.GetValues(typeof(Locale)))
		{
			if (locale != Locale.UNKNOWN && !result.CatalogLocaleToGameLocale.ContainsValue(locale))
			{
				Log.Store.PrintError("BattlePayConfig includes no catalog locale ID mapping for {0}", locale.ToString());
			}
		}
		result.IgnoreProductTiming = packet.IgnoreProductTiming;
		return result;
	}

	public void PurchaseViaGold(int quantity, ProductType productItemType, int data)
	{
		if (!IsLoggedIn())
		{
			Log.All.PrintError("Client attempted to make a gold purchase while offline!");
		}
		else
		{
			m_connectApi.PurchaseViaGold(quantity, productItemType, data);
		}
	}

	public void GetPurchaseMethod(long? pmtProductId, int quantity, Currency currency)
	{
		m_connectApi.RequestPurchaseMethod(pmtProductId, quantity, currency.toProto(), SystemInfo.deviceUniqueIdentifier, GetPlatformBuilder());
	}

	public void ConfirmPurchase()
	{
		m_connectApi.ConfirmPurchase();
	}

	public void CancelBlizzardPurchase(bool isAutoCanceled, CancelPurchase.CancelReason? reason, string error)
	{
		m_connectApi.AbortBlizzardPurchase(SystemInfo.deviceUniqueIdentifier, isAutoCanceled, reason, error);
	}

	public PurchaseMethod GetPurchaseMethodResponse()
	{
		PegasusUtil.PurchaseMethod packet = m_connectApi.GetPurchaseMethodResponse();
		if (packet == null)
		{
			return null;
		}
		PurchaseMethod result = new PurchaseMethod();
		if (packet.HasTransactionId)
		{
			result.TransactionID = packet.TransactionId;
		}
		if (packet.HasPmtProductId)
		{
			result.PMTProductID = packet.PmtProductId;
		}
		if (packet.HasQuantity)
		{
			result.Quantity = packet.Quantity;
		}
		result.CurrencyCode = packet.CurrencyCode;
		if (packet.HasWalletName)
		{
			result.WalletName = packet.WalletName;
		}
		if (packet.HasUseEbalance)
		{
			result.UseEBalance = packet.UseEbalance;
		}
		result.IsZeroCostLicense = packet.HasIsZeroCostLicense && packet.IsZeroCostLicense;
		if (packet.HasChallengeId)
		{
			result.ChallengeID = packet.ChallengeId;
		}
		if (packet.HasChallengeUrl)
		{
			result.ChallengeURL = packet.ChallengeUrl;
		}
		if (packet.HasError)
		{
			result.PurchaseError = ConvertPurchaseError(packet.Error);
		}
		return result;
	}

	public PurchaseResponse GetPurchaseResponse()
	{
		PegasusUtil.PurchaseResponse packet = m_connectApi.GetPurchaseResponse();
		if (packet == null)
		{
			return null;
		}
		return new PurchaseResponse
		{
			PurchaseError = ConvertPurchaseError(packet.Error),
			TransactionID = (packet.HasTransactionId ? packet.TransactionId : 0),
			PMTProductID = (packet.HasPmtProductId ? new long?(packet.PmtProductId) : ((long?)null)),
			ThirdPartyID = (packet.HasThirdPartyId ? packet.ThirdPartyId : string.Empty),
			CurrencyCode = packet.CurrencyCode
		};
	}

	public PurchaseViaGoldResponse GetPurchaseWithGoldResponse()
	{
		PurchaseWithGoldResponse packet = m_connectApi.GetPurchaseWithGoldResponse();
		if (packet == null)
		{
			return null;
		}
		PurchaseViaGoldResponse result = new PurchaseViaGoldResponse
		{
			Error = (PurchaseViaGoldResponse.ErrorType)packet.Result
		};
		if (packet.HasGoldUsed)
		{
			result.GoldUsed = packet.GoldUsed;
		}
		return result;
	}

	public CardBackResponse GetCardBackResponse()
	{
		SetFavoriteCardBackResponse packet = m_connectApi.GetSetFavoriteCardBackResponse();
		if (packet == null)
		{
			return null;
		}
		return new CardBackResponse
		{
			Success = packet.Success,
			CardBack = packet.CardBack,
			IsFavorite = packet.IsFavorite
		};
	}

	public void SetFavoriteCardBack(int cardBack, bool isFavorite = true)
	{
		m_connectApi.SetFavoriteCardBack(cardBack, isFavorite);
		if (!IsLoggedIn())
		{
			OfflineDataCache.SetFavoriteCardBack(cardBack, isFavorite);
			if (NetCache.Get().GetNetObject<NetCache.NetCacheCardBacks>() != null)
			{
				NetCache.Get().ProcessNewFavoriteCardBack(cardBack);
			}
		}
	}

	public NetCache.NetCacheCardBacks GetCardBacks()
	{
		if (!ShouldBeConnectedToAurora())
		{
			return new NetCache.NetCacheCardBacks();
		}
		CardBacks packet = GetCardBacksPacket();
		if (packet == null)
		{
			return null;
		}
		NetCache.NetCacheCardBacks result = new NetCache.NetCacheCardBacks();
		for (int i = 0; i < packet.CardBacks_.Count; i++)
		{
			int cardBackId = packet.CardBacks_[i];
			result.CardBacks.Add(cardBackId);
		}
		for (int j = 0; j < packet.FavoriteCardBacks.Count; j++)
		{
			int cardBackId2 = packet.FavoriteCardBacks[j];
			result.FavoriteCardBacks.Add(cardBackId2);
		}
		return result;
	}

	public CardBacks GetCardBacksPacket()
	{
		if (!ShouldBeConnectedToAurora())
		{
			return null;
		}
		return m_connectApi.GetCardBacks();
	}

	public CosmeticCoinResponse GetCoinResponse()
	{
		SetFavoriteCosmeticCoinResponse packet = m_connectApi.GetSetFavoriteCosmeticCoinResponse();
		if (packet == null)
		{
			return null;
		}
		return new CosmeticCoinResponse
		{
			Success = packet.Success,
			Coin = packet.CoinId,
			IsFavorite = packet.IsFavorite
		};
	}

	public void SetFavoriteCosmeticCoin(ref OfflineDataCache.OfflineData data, int coin, bool isFavorite)
	{
		m_connectApi.SetFavoriteCosmeticCoin(coin, isFavorite);
		if (!IsLoggedIn())
		{
			OfflineDataCache.SetFavoriteCosmeticCoin(ref data, coin, isFavorite);
			if (NetCache.Get().GetNetObject<NetCache.NetCacheCoins>() != null)
			{
				NetCache.Get().ProcessNewFavoriteCoin(coin, isFavorite);
			}
		}
	}

	public NetCache.NetCacheCoins GetCoins()
	{
		if (!ShouldBeConnectedToAurora())
		{
			return new NetCache.NetCacheCoins();
		}
		CosmeticCoins packet = GetCoinsPacket();
		if (packet == null)
		{
			return null;
		}
		NetCache.NetCacheCoins result = new NetCache.NetCacheCoins();
		for (int i = 0; i < packet.FavoriteCoins.Count; i++)
		{
			int coin = packet.FavoriteCoins[i];
			result.FavoriteCoins.Add(coin);
		}
		for (int j = 0; j < packet.Coins.Count; j++)
		{
			int coin2 = packet.Coins[j];
			result.Coins.Add(coin2);
		}
		return result;
	}

	public CosmeticCoins GetCoinsPacket()
	{
		if (!ShouldBeConnectedToAurora())
		{
			return null;
		}
		return m_connectApi.GetCoins();
	}

	public CoinUpdate GetCoinUpdate()
	{
		return m_connectApi.GetCoinUpdate();
	}

	public CardValues GetCardValues()
	{
		if (!ShouldBeConnectedToAurora())
		{
			return null;
		}
		return m_connectApi.GetCardValues();
	}

	public void SetDebugRatingInfo(int ratingId)
	{
		if (ShouldBeConnectedToAurora())
		{
			DebugRatingInfoRequest request = new DebugRatingInfoRequest
			{
				RatingId = ratingId
			};
			m_connectApi.DebugRatingInfoRequest(request);
		}
	}

	public DebugRatingInfoResponse GetDebugRatingInfoResponse()
	{
		return m_connectApi.GetDebugRatingInfoResponse();
	}

	public InitialClientState GetInitialClientState()
	{
		if (!ShouldBeConnectedToAurora())
		{
			InitialClientState clientState = new InitialClientState();
			clientState.HasClientOptions = true;
			clientState.ClientOptions = new ClientOptions();
			clientState.HasCollection = true;
			clientState.Collection = new Collection();
			clientState.HasAchievements = true;
			clientState.Achievements = new Achieves();
			clientState.HasNotices = true;
			clientState.Notices = new ProfileNotices();
			clientState.HasGameCurrencyStates = true;
			clientState.GameCurrencyStates = new GameCurrencyStates();
			clientState.GameCurrencyStates.HasCurrencyVersion = true;
			clientState.GameCurrencyStates.CurrencyVersion = 0L;
			clientState.GameCurrencyStates.HasArcaneDustBalance = true;
			clientState.GameCurrencyStates.HasCappedGoldBalance = true;
			clientState.GameCurrencyStates.HasBonusGoldBalance = true;
			clientState.GameCurrencyStates.HasRenownBalance = true;
			clientState.HasBoosters = true;
			clientState.Boosters = new Boosters();
			if (clientState.Decks == null)
			{
				clientState.Decks = new List<DeckInfo>();
			}
			return clientState;
		}
		return NetCache.Get().InitialClientState;
	}

	public void OpenBooster(int id, int numPacks)
	{
		Log.Net.Print("Network.OpenBooster");
		m_connectApi.OpenBooster(id, numPacks);
	}

	public void CreateDeck(DeckType deckType, string name, int heroDatabaseAssetID, PegasusShared.FormatType formatType, long sortOrder, DeckSourceType sourceType, out int? requestId, string pastedDeckHash = null, int brawlLibraryItemId = 0)
	{
		if (!IsLoggedIn())
		{
			requestId = null;
			return;
		}
		requestId = GetNextCreateDeckRequestId();
		Log.Net.Print($"Network.CreateDeck hero={heroDatabaseAssetID}");
		m_connectApi.CreateDeck(deckType, name, heroDatabaseAssetID, formatType, sortOrder, sourceType, pastedDeckHash, brawlLibraryItemId, requestId);
	}

	private int GetNextCreateDeckRequestId()
	{
		return ++m_state.CurrentCreateDeckRequestId;
	}

	public void RenameDeck(long deck, string name, bool playerInitiated, DeckType deckType, DeckSourceType sourceType)
	{
		if (IsLoggedIn())
		{
			Log.Net.Print($"Network.RenameDeck {deck}");
			CollectionManager.Get().AddPendingDeckRename(deck, name, playerInitiated);
			m_connectApi.RenameDeck(deck, name, deckType, sourceType);
		}
		else
		{
			OfflineDataCache.RenameDeck(deck, name);
		}
	}

	public void SendDeckData(CollectionDeck.ChangeSource changeSource, int changeNumber, long deck, List<CardUserData> cards, List<SideboardCardUserData> sideboardCards, int newHeroAssetID, bool? newHeroOverridenStatus, int uiHeroOverrideAssetID, TAG_PREMIUM uiHeroOverridePremium, int? newCosmeticCoinID, bool? randomCoinUseFavorite, int? newCardBackID, PegasusShared.FormatType formatType, long sortOrder, bool? randomHeroUseFavorite, RuneType[] runeOrder, string pastedDeckHash = null)
	{
		DeckSetData packet = new DeckSetData
		{
			ChangeSource = (int)changeSource,
			ChangeNumber = changeNumber,
			Deck = deck,
			FormatType = formatType,
			TaggedStandard = (formatType == PegasusShared.FormatType.FT_STANDARD),
			SortOrder = sortOrder
		};
		for (int i = 0; i < cards.Count; i++)
		{
			CardUserData card = cards[i];
			DeckCardData cardPacket = new DeckCardData();
			PegasusShared.CardDef def = new PegasusShared.CardDef();
			def.Asset = card.DbId;
			if (card.Premium != 0)
			{
				def.Premium = (int)card.Premium;
			}
			cardPacket.Def = def;
			cardPacket.Qty = card.Count;
			packet.Cards.Add(cardPacket);
		}
		foreach (SideboardCardUserData sideboardCardData in sideboardCards)
		{
			SideBoardCardData sideBoardCard = new SideBoardCardData();
			PegasusShared.CardDef def2 = new PegasusShared.CardDef();
			CardUserData card2 = sideboardCardData.Card;
			def2.Asset = card2.DbId;
			if (card2.Premium != 0)
			{
				def2.Premium = (int)card2.Premium;
			}
			sideBoardCard.Def = def2;
			sideBoardCard.Qty = card2.Count;
			sideBoardCard.LinkedCardDbId = sideboardCardData.LinkedCardDbId;
			packet.SideboardCards.Add(sideBoardCard);
		}
		if (newHeroOverridenStatus.HasValue)
		{
			packet.HasHeroOverridden = true;
			packet.HeroOverridden = newHeroOverridenStatus.Value;
		}
		if (-1 != newHeroAssetID)
		{
			packet.Hero = newHeroAssetID;
		}
		if (-1 != uiHeroOverrideAssetID)
		{
			PegasusShared.CardDef heroDef = new PegasusShared.CardDef();
			heroDef.Asset = uiHeroOverrideAssetID;
			heroDef.Premium = (int)uiHeroOverridePremium;
			packet.UiHeroOverride = heroDef;
		}
		if (-1 != newCosmeticCoinID)
		{
			if (!newCosmeticCoinID.HasValue)
			{
				packet.HasCosmeticCoin = false;
				packet.RemovingCosmeticCoin = true;
			}
			else
			{
				packet.HasCosmeticCoin = true;
				packet.CosmeticCoin = newCosmeticCoinID.Value;
			}
		}
		if (randomCoinUseFavorite.HasValue)
		{
			packet.HasRandomCoinUseFavorite = true;
			packet.RandomCoinUseFavorite = randomCoinUseFavorite.Value;
		}
		if (-1 != newCardBackID)
		{
			if (!newCardBackID.HasValue)
			{
				packet.HasCardBack = false;
				packet.RemovingCardBack = true;
			}
			else
			{
				packet.HasCardBack = true;
				packet.CardBack = newCardBackID.Value;
			}
		}
		if (randomHeroUseFavorite.HasValue)
		{
			packet.HasRandomHeroUseFavorite = true;
			packet.RandomHeroUseFavorite = randomHeroUseFavorite.Value;
		}
		if (runeOrder != null && runeOrder.Length == DeckRule_DeathKnightRuneLimit.MaxRuneSlots)
		{
			packet.HasRune1 = true;
			packet.Rune1 = runeOrder[0];
			packet.HasRune2 = true;
			packet.Rune2 = runeOrder[1];
			packet.HasRune3 = true;
			packet.Rune3 = runeOrder[2];
		}
		if (!string.IsNullOrEmpty(pastedDeckHash))
		{
			packet.PastedDeckHash = pastedDeckHash;
		}
		if (IsLoggedIn())
		{
			m_connectApi.SendDeckData(packet);
			OfflineDataCache.ApplyDeckSetDataToOriginalDeck(packet);
			CollectionManager.Get().AddPendingDeckEdit(deck);
		}
		OfflineDataCache.ApplyDeckSetDataLocally(packet);
	}

	public void DeleteDeck(long deck, DeckType deckType)
	{
		OfflineDataCache.DeleteDeck(deck);
		if (IsLoggedIn())
		{
			Log.Net.Print($"Network.DeleteDeck {deck}");
			if (deck <= 0)
			{
				Log.Offline.PrintError("Network.DeleteDeck Error: Attempting to delete fake deck ID={0} on server.", deck);
			}
			else
			{
				m_connectApi.DeleteDeck(deck, deckType);
			}
		}
	}

	public void RequestDeckContents(params long[] deckIds)
	{
		if (IsLoggedIn())
		{
			Log.Net.Print("Network.GetDeckContents {0}", string.Join(", ", deckIds.Select((long id) => id.ToString()).ToArray()));
			m_connectApi.RequestDeckContents(deckIds);
		}
	}

	public void SetDeckTemplateSource(long deck, int templateID)
	{
		if (IsLoggedIn() && deck >= 0)
		{
			Log.Net.Print($"Network.SendDeckTemplateSource {deck}, {templateID}");
			m_connectApi.SendDeckTemplateSource(deck, templateID);
		}
	}

	public GetDeckContentsResponse GetDeckContentsResponse()
	{
		GetDeckContentsResponse response;
		if (IsLoggedIn())
		{
			response = m_connectApi.GetDeckContentsResponse();
		}
		else
		{
			response = new GetDeckContentsResponse
			{
				Decks = new List<PegasusUtil.DeckContents>()
			};
			response.Decks = OfflineDataCache.GetLocalDeckContentsFromCache();
		}
		return response;
	}

	public FreeDeckChoiceResponse GetFreeDeckChoiceResponse()
	{
		if (IsLoggedIn())
		{
			return m_connectApi.GetFreeDeckChoiceResponse();
		}
		return new FreeDeckChoiceResponse
		{
			Success = false
		};
	}

	public FreeDeckStateUpdate GetFreeDeckStateUpdate()
	{
		return m_connectApi.GetFreeDeckStateUpdate();
	}

	public static SmartDeckRequest GenerateSmartDeckRequestMessage(CollectionDeck deck)
	{
		List<SmartDeckCardData> packetDeckCards = new List<SmartDeckCardData>();
		Dictionary<long, SmartDeckCardData> assetToSmartCardData = new Dictionary<long, SmartDeckCardData>();
		foreach (CollectionDeckSlot slot in deck.GetSlots())
		{
			if (slot.Owned)
			{
				int cardAssetId = GameUtils.TranslateCardIdToDbId(slot.CardID);
				if (!assetToSmartCardData.ContainsKey(cardAssetId))
				{
					assetToSmartCardData.Add(cardAssetId, new SmartDeckCardData
					{
						Asset = cardAssetId
					});
				}
				assetToSmartCardData[cardAssetId].QtyGolden += slot.GetCount(TAG_PREMIUM.GOLDEN);
				assetToSmartCardData[cardAssetId].QtyNormal += slot.GetCount(TAG_PREMIUM.NORMAL);
			}
		}
		foreach (long assetId in assetToSmartCardData.Keys)
		{
			packetDeckCards.Add(assetToSmartCardData[assetId]);
		}
		HSCachedDeckCompletionRequest requestMessage = new HSCachedDeckCompletionRequest
		{
			HeroClass = (int)deck.GetClass(),
			InsertedCard = packetDeckCards,
			DeckId = deck.ID,
			FormatType = deck.FormatType,
			Rune1 = deck.GetRuneAtIndex(0),
			Rune2 = deck.GetRuneAtIndex(1),
			Rune3 = deck.GetRuneAtIndex(2)
		};
		return new SmartDeckRequest
		{
			RequestMessage = requestMessage
		};
	}

	public void RequestSmartDeckCompletion(CollectionDeck deck)
	{
		SmartDeckRequest packet = GenerateSmartDeckRequestMessage(deck);
		m_connectApi.SendSmartDeckRequest(packet);
	}

	public void RequestBaconRatingInfo()
	{
		m_connectApi.RequestBaconRatingInfo();
	}

	public void SendPVPDRSessionInfoRequest()
	{
		m_connectApi.SendPVPDRSessionInfoRequest();
	}

	public PVPDRSessionInfoResponse GetPVPDRSessionInfoResponse()
	{
		return m_connectApi.GetPVPDRSessionInfoResponse();
	}

	public void RequestLettuceMap(uint bountyId = 0u, List<LettuceMapPlayerData> playerDataList = null, BnetGameAccountId coopLeaderGameAccountId = null, int mythicLevel = -1)
	{
		LettuceMapRequest request = new LettuceMapRequest
		{
			LettuceBountyRecordId = bountyId
		};
		if (coopLeaderGameAccountId != null)
		{
			request.CoopMapOwnerId = BnetUtils.CreatePegasusBnetId(coopLeaderGameAccountId);
		}
		if (playerDataList != null)
		{
			request.PlayerData = playerDataList;
		}
		if (mythicLevel >= 0)
		{
			request.MythicLevel = (uint)mythicLevel;
		}
		m_connectApi.RequestLettuceMap(request);
	}

	public void ChooseLettuceMapNode(uint nodeId)
	{
		LettuceMapChooseNodeRequest request = new LettuceMapChooseNodeRequest
		{
			NodeId = nodeId
		};
		m_connectApi.ChooseLettuceMapNode(request);
	}

	public void RetireLettuceMap()
	{
		LettuceMapRetireRequest request = new LettuceMapRetireRequest();
		m_connectApi.RetireLettuceMap(request);
	}

	public void MakeMercenariesMapTreasureSelection(int mercenaryId, int optionIndex)
	{
		MercenariesMapTreasureSelectionRequest request = new MercenariesMapTreasureSelectionRequest();
		request.MercenaryId = mercenaryId;
		request.SelectedOptionIndex = optionIndex;
		m_connectApi.MakeMercenariesMapTreasureSelection(request);
	}

	public void MakeMercenariesMapVisitorSelection(int optionIndex)
	{
		MercenariesMapVisitorSelectionRequest request = new MercenariesMapVisitorSelectionRequest();
		request.SelectedOptionIndex = optionIndex;
		m_connectApi.MakeMercenariesMapVisitorSelection(request);
	}

	public void RequestLuckyDrawBoxState(int luckyDrawBoxId)
	{
		LuckyDrawBoxStateRequest request = new LuckyDrawBoxStateRequest();
		request.LuckyDrawBoxId = luckyDrawBoxId;
		m_connectApi.RequestLuckyDrawBoxState(request);
	}

	public LuckyDrawBoxStateResponse GetLuckyDrawBoxStateResponse()
	{
		return m_connectApi.GetLuckyDrawBoxStateResponse();
	}

	public void UseLuckyDrawHammer()
	{
		LuckyDrawUseHammerRequest request = new LuckyDrawUseHammerRequest();
		m_connectApi.UseLuckyDrawHammer(request);
	}

	public LuckyDrawUseHammerResponse GetUseLuckyDrawHammerResponse()
	{
		return m_connectApi.GetUseLuckyDrawHammerResponse();
	}

	public void AcknowledgeLuckyDrawHammers()
	{
		LuckyDrawAcknowledgeAllHammersRequest request = new LuckyDrawAcknowledgeAllHammersRequest();
		m_connectApi.AcknowledgeLuckyDrawHammers(request);
	}

	public void AcknowledgeLuckyDrawRewards()
	{
		LuckyDrawAcknowledgeAllRewardsRequest request = new LuckyDrawAcknowledgeAllRewardsRequest();
		m_connectApi.AcknowledgeLuckyDrawRewards(request);
	}

	public List<NetCache.BoosterCard> OpenedBooster(out bool isCatchup)
	{
		BoosterContent packet = m_connectApi.GetOpenedBooster();
		if (packet == null)
		{
			isCatchup = false;
			return null;
		}
		List<NetCache.BoosterCard> result = new List<NetCache.BoosterCard>();
		for (int i = 0; i < packet.List.Count; i++)
		{
			BoosterCard card = packet.List[i];
			NetCache.BoosterCard cardToInsert = new NetCache.BoosterCard();
			cardToInsert.Def.Name = GameUtils.TranslateDbIdToCardId(card.CardDef.Asset);
			cardToInsert.Def.Premium = (TAG_PREMIUM)card.CardDef.Premium;
			cardToInsert.Date = TimeUtils.PegDateToFileTimeUtc(card.InsertDate);
			result.Add(cardToInsert);
		}
		if (packet.HasCollectionVersion)
		{
			NetCache.Get().AddExpectedCollectionModification(packet.CollectionVersion);
		}
		isCatchup = packet.HasIsCatchupPack && packet.IsCatchupPack;
		return result;
	}

	public List<ExpansionCollectionStats> ExpansionCollectionStats()
	{
		return m_connectApi.GetOpenedBooster()?.ExpansionCollectionStats;
	}

	public bool GetMassPackOpeningEnabledFromResponsePacket(out bool value)
	{
		BoosterContent packet = m_connectApi.GetOpenedBooster();
		if (!packet.HasMassPackOpeningEnabled)
		{
			value = false;
			return false;
		}
		value = packet.MassPackOpeningEnabled;
		return true;
	}

	public bool GetNumPacksOpened(out int value)
	{
		BoosterContent packet = m_connectApi.GetOpenedBooster();
		if (!packet.HasPacksOpened)
		{
			value = 1;
			return false;
		}
		value = packet.PacksOpened;
		return true;
	}

	public DBAction GetDeckResponse()
	{
		return GetDbAction();
	}

	public DBAction GetDbAction()
	{
		PegasusUtil.DBAction packet = m_connectApi.GetDbAction();
		if (packet == null)
		{
			return null;
		}
		return new DBAction
		{
			Action = (DBAction.ActionType)packet.Action,
			Result = (DBAction.ResultType)packet.Result,
			MetaData = packet.MetaData
		};
	}

	public void ReconcileDeckContentsForChangedOfflineDecks(ref OfflineDataCache.OfflineData data, List<DeckInfo> remoteDecks, List<PegasusUtil.DeckContents> remoteContents, List<long> validDeckIds)
	{
		List<long> decksToReconcile = new List<long>();
		foreach (DeckInfo remoteDeckInfo in remoteDecks)
		{
			bool found = false;
			long remoteDeckInfoId = remoteDeckInfo.Id;
			foreach (long validDeckId in validDeckIds)
			{
				if (validDeckId == remoteDeckInfoId)
				{
					found = true;
					break;
				}
			}
			if (found)
			{
				continue;
			}
			DeckInfo originalDeckInfo = OfflineDataCache.GetDeckInfoFromDeckList(remoteDeckInfoId, data.OriginalDeckList);
			DeckInfo localDeckInfo = OfflineDataCache.GetDeckInfoFromDeckList(remoteDeckInfoId, data.LocalDeckList);
			if (localDeckInfo == null && originalDeckInfo != null)
			{
				NetCache.NetCacheFeatures guardianVars = NetCache.Get().GetNetObject<NetCache.NetCacheFeatures>();
				if (guardianVars != null && guardianVars.AllowOfflineClientDeckDeletion)
				{
					Get().DeleteDeck(remoteDeckInfoId, remoteDeckInfo.DeckType);
				}
			}
			else if (localDeckInfo == null && originalDeckInfo == null)
			{
				decksToReconcile.Add(remoteDeckInfoId);
			}
			else if (localDeckInfo != null && originalDeckInfo != null && remoteDeckInfo.LastModified != localDeckInfo.LastModified)
			{
				if (remoteDeckInfo.LastModified != originalDeckInfo.LastModified)
				{
					decksToReconcile.Add(remoteDeckInfoId);
				}
				else
				{
					decksToReconcile.Add(remoteDeckInfoId);
				}
			}
		}
		if (data.LocalDeckList != null)
		{
			foreach (DeckInfo localDeck in data.LocalDeckList)
			{
				long localDeckId = localDeck.Id;
				bool hasMatchingRemoteDeck = false;
				foreach (DeckInfo remoteDeck in remoteDecks)
				{
					if (remoteDeck.Id == localDeckId)
					{
						hasMatchingRemoteDeck = true;
						break;
					}
				}
				if (hasMatchingRemoteDeck)
				{
					continue;
				}
				bool hasMatchingOriginalDeck = false;
				foreach (DeckInfo originalDeck in data.OriginalDeckList)
				{
					if (originalDeck.Id == localDeckId)
					{
						hasMatchingOriginalDeck = true;
						break;
					}
				}
				if (hasMatchingOriginalDeck)
				{
					CollectionManager.Get().OnDeckDeletedWhileOffline(localDeckId);
				}
			}
		}
		if (decksToReconcile.Count > 0)
		{
			List<long> decksToRequest = new List<long>();
			foreach (long deck in decksToReconcile)
			{
				m_state.DeckIdsWaitingToDiffAgainstOfflineCache.Add(deck);
				DeckInfo deckHeader = null;
				foreach (DeckInfo item in remoteDecks)
				{
					if (item.Id == deck)
					{
						deckHeader = item;
						break;
					}
				}
				bool isPreconstructed = deckHeader != null && deckHeader.DeckType == DeckType.PRECON_DECK;
				bool hasRemoteContents = false;
				if (remoteContents != null)
				{
					foreach (PegasusUtil.DeckContents remoteContent in remoteContents)
					{
						if (remoteContent.DeckId == deck)
						{
							hasRemoteContents = true;
							break;
						}
					}
				}
				if (!isPreconstructed && !hasRemoteContents)
				{
					decksToRequest.Add(deck);
				}
			}
			if (decksToRequest.Count > 0)
			{
				RequestDeckContents(decksToRequest.ToArray());
			}
		}
		RegisterNetHandler(DeckCreated.PacketID.ID, OnDeckCreatedResponse_SendOfflineDeckSetData);
		CreateDeckFromOfflineDeckCache(ref data);
		if (remoteContents != null)
		{
			UpdateDecksFromContent(ref data, remoteContents);
		}
	}

	public NetCache.NetCacheDecks GetDeckHeaders()
	{
		NetCache.NetCacheDecks netCacheDecks = new NetCache.NetCacheDecks();
		if (!ShouldBeConnectedToAurora())
		{
			return netCacheDecks;
		}
		DeckList packet = m_connectApi.GetDeckHeaders();
		if (packet == null)
		{
			return null;
		}
		return GetDeckHeaders(packet.Decks);
	}

	public static NetCache.NetCacheDecks GetDeckHeaders(List<DeckInfo> deckHeaders)
	{
		NetCache.NetCacheDecks result = new NetCache.NetCacheDecks();
		if (deckHeaders == null)
		{
			return result;
		}
		for (int i = 0; i < deckHeaders.Count; i++)
		{
			result.Decks.Add(GetDeckHeaderFromDeckInfo(deckHeaders[i]));
		}
		return result;
	}

	private void OnDeckContentsResponse()
	{
		GetDeckContentsResponse response = GetDeckContentsResponse();
		OfflineDataCache.OfflineData data = OfflineDataCache.ReadOfflineDataFromFile();
		UpdateDecksFromContent(ref data, response.Decks);
		OfflineDataCache.WriteOfflineDataToFile(data);
	}

	private void CacheSecondaryClientState(SecondaryClientState secondaryState)
	{
		InitialClientState initialClientState = NetCache.Get().InitialClientState;
		initialClientState.Achievements = secondaryState.Achievements;
		initialClientState.ArenaSession = secondaryState.ArenaSession;
		initialClientState.Boosters = secondaryState.Boosters;
		initialClientState.ClientOptions = secondaryState.ClientOptions;
		initialClientState.DeckContents = secondaryState.DeckContents;
		initialClientState.Decks = secondaryState.Decks;
		initialClientState.DevTimeOffsetSeconds = secondaryState.DevTimeOffsetSeconds;
		initialClientState.DisconnectedGame = secondaryState.DisconnectedGame;
		initialClientState.DisplayBanner = secondaryState.DisplayBanner;
		initialClientState.GameCurrencyStates = secondaryState.GameCurrencyStates;
		initialClientState.GameSaveData = secondaryState.GameSaveData;
		initialClientState.GuardianVars = secondaryState.GuardianVars;
		initialClientState.MedalInfo = secondaryState.MedalInfo;
		initialClientState.Notices = secondaryState.Notices;
		initialClientState.PlayerDraftTickets = secondaryState.PlayerDraftTickets;
		initialClientState.PlayerExperiments = secondaryState.PlayerExperiments;
		initialClientState.PlayerProfileProgress = secondaryState.PlayerProfileProgress;
		initialClientState.ReturningPlayerInfo = secondaryState.ReturningPlayerInfo;
		initialClientState.SpecialEventTiming = secondaryState.SpecialEventTiming;
		initialClientState.TavernBrawlsList = secondaryState.TavernBrawlsList;
		initialClientState.ValidCachedDeckIds = secondaryState.ValidCachedDeckIds;
	}

	private void OnCollectionClientStateResponse()
	{
		CollectionClientStateResponse packet = m_connectApi.GetCollectionClientStateResponse();
		if (packet == null)
		{
			Debug.LogWarning("Network.OnCollectionClientStateResponse() server response not found");
			return;
		}
		InitialClientState initialClientState = new InitialClientState();
		NetCache.Get().InitialClientState = initialClientState;
		initialClientState.PlayerId = packet.PlayerId;
		initialClientState.Collection = packet.Collection;
		SecondaryClientState secondaryState = packet.SecondaryState;
		if (secondaryState == null)
		{
			RequestSecondaryClientState();
			return;
		}
		CacheSecondaryClientState(secondaryState);
		HandleInitialClientState();
	}

	private void OnSecondaryClientStateResponse()
	{
		SecondaryClientState secondaryClientState = m_connectApi.GetSecondaryClientStateResponse()?.SecondaryState;
		if (secondaryClientState == null)
		{
			Debug.LogWarning("Network.OnSecondaryClientStateResponse() state not found");
			return;
		}
		CacheSecondaryClientState(secondaryClientState);
		HandleInitialClientState();
	}

	private void HandleInitialClientState()
	{
		HandleType(207);
		NetCache.Get().InitialClientState = null;
	}

	private void UpdateDecksFromContent(ref OfflineDataCache.OfflineData data, List<PegasusUtil.DeckContents> decksContents)
	{
		List<DeckSetData> deckSetDataToSend = new List<DeckSetData>();
		List<RenameDeck> deckRenameToSend = new List<RenameDeck>();
		List<DeckInfo> currentDeckList = NetCache.Get().GetDeckListFromNetCache();
		foreach (PegasusUtil.DeckContents remoteDeckContents in decksContents)
		{
			if (m_state.DeckIdsWaitingToDiffAgainstOfflineCache.Contains(remoteDeckContents.DeckId))
			{
				m_state.DeckIdsWaitingToDiffAgainstOfflineCache.Remove(remoteDeckContents.DeckId);
				DiffRemoteDeckContentsAgainstOfflineDataCache(ref data, remoteDeckContents, currentDeckList, ref deckSetDataToSend, ref deckRenameToSend);
			}
			else
			{
				OfflineDataCache.CacheLocalAndOriginalDeckContents(ref data, remoteDeckContents, remoteDeckContents);
			}
		}
		List<long> decksToUpdate = new List<long>();
		foreach (DeckSetData deckSetData in deckSetDataToSend)
		{
			m_connectApi.SendDeckData(deckSetData);
			decksToUpdate.Add(deckSetData.Deck);
		}
		CollectionManager.Get().RegisterDecksToRequestContentsAfterDeckSetDataResponse(decksToUpdate);
		foreach (RenameDeck deckRename in deckRenameToSend)
		{
			m_connectApi.RenameDeck(deckRename.Deck, deckRename.Name, DeckType.NORMAL_DECK, DeckSourceType.DECK_SOURCE_TYPE_NORMAL);
		}
		OfflineDataCache.CacheLocalAndOriginalDeckList(ref data, currentDeckList, currentDeckList);
	}

	private void DiffRemoteDeckContentsAgainstOfflineDataCache(ref OfflineDataCache.OfflineData data, PegasusUtil.DeckContents remoteDeckContents, List<DeckInfo> currentNetCacheDeckList, ref List<DeckSetData> deckSetDataToSend, ref List<RenameDeck> deckRenameToSend)
	{
		DeckInfo localDeckInfo = OfflineDataCache.GetDeckInfoFromDeckList(remoteDeckContents.DeckId, data.LocalDeckList);
		PegasusUtil.DeckContents localDeckContents = OfflineDataCache.GetDeckContentsFromDeckContentsList(remoteDeckContents.DeckId, data.LocalDeckContents);
		DeckInfo netCacheDeckInfo = null;
		foreach (DeckInfo deck in currentNetCacheDeckList)
		{
			if (deck.Id == remoteDeckContents.DeckId)
			{
				netCacheDeckInfo = deck;
				break;
			}
		}
		if (netCacheDeckInfo == null)
		{
			return;
		}
		if (localDeckInfo != null && netCacheDeckInfo.LastModified < localDeckInfo.LastModified)
		{
			if (OfflineDataCache.GenerateDeckSetDataFromDiff(remoteDeckContents.DeckId, localDeckInfo, netCacheDeckInfo, localDeckContents, remoteDeckContents, out var deckSetData))
			{
				deckSetDataToSend.Add(deckSetData);
			}
			RenameDeck deckRename = OfflineDataCache.GenerateRenameDeckFromDiff(remoteDeckContents.DeckId, localDeckInfo, netCacheDeckInfo);
			if (deckRename != null && deckRename.Name != null)
			{
				deckRenameToSend.Add(deckRename);
			}
		}
		else
		{
			OfflineDataCache.CacheLocalAndOriginalDeckContents(ref data, remoteDeckContents, remoteDeckContents);
		}
	}

	private void CreateDeckFromOfflineDeckCache(ref OfflineDataCache.OfflineData data)
	{
		int startingIndex = 0;
		List<long> fakeDeckIds = OfflineDataCache.GetFakeDeckIds(data);
		if (fakeDeckIds.Contains(FakeIdWaitingForResponse))
		{
			startingIndex = fakeDeckIds.IndexOf(Get().FakeIdWaitingForResponse);
			startingIndex++;
		}
		DeckInfo deckInfo = null;
		for (int i = startingIndex; i < fakeDeckIds.Count; i++)
		{
			if (deckInfo != null)
			{
				break;
			}
			FakeIdWaitingForResponse = fakeDeckIds[i];
			deckInfo = OfflineDataCache.GetDeckInfoFromDeckList(FakeIdWaitingForResponse, data.LocalDeckList);
		}
		if (deckInfo == null)
		{
			RemoveNetHandler(DeckCreated.PacketID.ID, OnDeckCreatedResponse_SendOfflineDeckSetData);
			OnFinishedCreatingDecksFromOfflineDataCache(ref data);
			return;
		}
		CreateDeck(deckInfo.DeckType, deckInfo.Name, deckInfo.Hero, deckInfo.FormatType, deckInfo.SortOrder, deckInfo.SourceType, out var requestId, deckInfo.PastedDeckHash);
		if (requestId.HasValue)
		{
			m_state.InTransitOfflineCreateDeckRequestIds.Add(requestId.Value);
		}
	}

	private void OnFinishedCreatingDecksFromOfflineDataCache(ref OfflineDataCache.OfflineData data)
	{
		OfflineDataCache.ClearFakeDeckIds(ref data);
		OfflineDataCache.RemoveAllOldDecksContents(ref data);
		FakeIdWaitingForResponse = 0L;
	}

	private void OnDeckCreatedResponse_SendOfflineDeckSetData()
	{
		int? requestId;
		NetCache.DeckHeader createdDeck = GetCreatedDeck(out requestId);
		if (createdDeck != null && requestId.HasValue && m_state.InTransitOfflineCreateDeckRequestIds.Contains(requestId.Value))
		{
			m_state.InTransitOfflineCreateDeckRequestIds.Remove(requestId.Value);
			long fakeDeckId = Get().FakeIdWaitingForResponse;
			OfflineDataCache.OfflineData data = OfflineDataCache.ReadOfflineDataFromFile();
			if (OfflineDataCache.GenerateDeckSetDataFromDiff(fakeDeckId, data.LocalDeckList, data.OriginalDeckList, data.LocalDeckContents, data.OriginalDeckContents, out var packet))
			{
				packet.Deck = createdDeck.ID;
				CollectionManager.Get().RegisterDecksToRequestContentsAfterDeckSetDataResponse(new List<long> { createdDeck.ID });
				m_connectApi.SendDeckData(packet);
			}
			if (!OfflineDataCache.UpdateDeckWithNewId(fakeDeckId, createdDeck.ID))
			{
				Log.Offline.PrintDebug("OnDeckCreatedResponse_SendOfflineDeckSetData() - Deleting deck id={0} because it's fake id={1}  was not found in the offline cache.", createdDeck.ID, fakeDeckId);
				DeleteDeck(createdDeck.ID, createdDeck.Type);
			}
			else
			{
				CollectionManager.Get().UpdateDeckWithNewId(fakeDeckId, createdDeck.ID);
				CreateDeckFromOfflineDeckCache(ref data);
				OfflineDataCache.WriteOfflineDataToFile(data);
			}
		}
	}

	public static bool DeckNeedsName(ulong deckValidityFlags)
	{
		return (deckValidityFlags & 0x200) != 0;
	}

	public static bool AreDeckFlagsLocked(ulong deckValidityFlags)
	{
		return (deckValidityFlags & 0x400) != 0;
	}

	public NetCache.DeckHeader GetCreatedDeck(out int? requestId)
	{
		DeckCreated packet = m_connectApi.DeckCreated();
		if (packet == null)
		{
			requestId = null;
			return null;
		}
		NetCache.DeckHeader deckHeaderFromDeckInfo = GetDeckHeaderFromDeckInfo(packet.Info);
		requestId = packet.RequestId;
		return deckHeaderFromDeckInfo;
	}

	public static NetCache.DeckHeader GetDeckHeaderFromDeckInfo(DeckInfo deck)
	{
		NetCache.DeckHeader result = new NetCache.DeckHeader
		{
			ID = deck.Id,
			Name = deck.Name,
			Hero = GameUtils.TranslateDbIdToCardId(deck.Hero),
			HeroPower = GameUtils.GetHeroPowerCardIdFromHero(deck.Hero),
			RandomHeroUseFavorite = deck.RandomHeroUseFavorite,
			RandomCoinUseFavorite = deck.RandomCoinUseFavorite,
			Type = deck.DeckType,
			HeroOverridden = deck.HeroOverride,
			SeasonId = deck.SeasonId,
			BrawlLibraryItemId = deck.BrawlLibraryItemId,
			NeedsName = DeckNeedsName(deck.Validity),
			SortOrder = (deck.HasSortOrder ? deck.SortOrder : deck.Id),
			FormatType = deck.FormatType,
			Rune1 = deck.Rune1,
			Rune2 = deck.Rune2,
			Rune3 = deck.Rune3,
			Locked = AreDeckFlagsLocked(deck.Validity),
			SourceType = (deck.HasSourceType ? deck.SourceType : DeckSourceType.DECK_SOURCE_TYPE_UNKNOWN),
			UIHeroOverride = ((deck.HasUiHeroOverride && deck.UiHeroOverride != 0) ? GameUtils.TranslateDbIdToCardId(deck.UiHeroOverride) : string.Empty),
			UIHeroOverridePremium = (deck.HasUiHeroOverridePremium ? ((TAG_PREMIUM)deck.UiHeroOverridePremium) : TAG_PREMIUM.NORMAL)
		};
		if (deck.HasCosmeticCoin)
		{
			result.CosmeticCoin = deck.CosmeticCoin;
		}
		if (deck.HasCardBack)
		{
			result.CardBack = deck.CardBack;
		}
		if (deck.HasCreateDate)
		{
			result.CreateDate = TimeUtils.UnixTimeStampToDateTimeUtc(deck.CreateDate);
		}
		else
		{
			result.CreateDate = null;
		}
		if (deck.HasLastModified)
		{
			result.LastModified = TimeUtils.UnixTimeStampToDateTimeUtc(deck.LastModified);
		}
		else
		{
			result.LastModified = null;
		}
		return result;
	}

	public static DeckInfo GetDeckInfoFromDeckHeader(NetCache.DeckHeader deckHeader)
	{
		if (deckHeader == null)
		{
			return null;
		}
		DeckInfo result = new DeckInfo
		{
			Id = deckHeader.ID,
			Name = deckHeader.Name,
			Hero = GameUtils.TranslateCardIdToDbId(deckHeader.Hero),
			DeckType = deckHeader.Type,
			HeroOverride = deckHeader.HeroOverridden,
			BrawlLibraryItemId = deckHeader.BrawlLibraryItemId,
			SortOrder = deckHeader.SortOrder,
			SourceType = deckHeader.SourceType,
			FormatType = deckHeader.FormatType,
			RandomCoinUseFavorite = deckHeader.RandomCoinUseFavorite,
			RandomHeroUseFavorite = deckHeader.RandomHeroUseFavorite
		};
		if (deckHeader.CosmeticCoin.HasValue)
		{
			result.CosmeticCoin = deckHeader.CosmeticCoin.Value;
		}
		if (deckHeader.CardBack.HasValue)
		{
			result.CardBack = deckHeader.CardBack.Value;
		}
		if (deckHeader.SeasonId != 0)
		{
			result.SeasonId = deckHeader.SeasonId;
		}
		if (!string.IsNullOrEmpty(deckHeader.UIHeroOverride))
		{
			result.UiHeroOverride = GameUtils.TranslateCardIdToDbId(deckHeader.UIHeroOverride);
			result.UiHeroOverridePremium = (int)deckHeader.UIHeroOverridePremium;
		}
		if (deckHeader.CreateDate.HasValue)
		{
			result.CreateDate = (long)TimeUtils.DateTimeToUnixTimeStamp(deckHeader.CreateDate.Value);
		}
		if (deckHeader.LastModified.HasValue)
		{
			result.LastModified = (long)TimeUtils.DateTimeToUnixTimeStamp(deckHeader.LastModified.Value);
		}
		return result;
	}

	public long GetDeletedDeckID()
	{
		return m_connectApi.DeckDeleted()?.Deck ?? 0;
	}

	public DeckName GetRenamedDeck()
	{
		DeckRenamed packet = m_connectApi.DeckRenamed();
		if (packet == null)
		{
			return null;
		}
		return new DeckName
		{
			Deck = packet.Deck,
			Name = packet.Name,
			IsCensored = packet.IsNameCensored
		};
	}

	public GenericResponse GetGenericResponse()
	{
		if (!ShouldBeConnectedToAurora())
		{
			return new GenericResponse
			{
				RequestId = 0,
				RequestSubId = 1,
				ResultCode = GenericResponse.Result.RESULT_OK
			};
		}
		PegasusUtil.GenericResponse packet = m_connectApi.GetGenericResponse();
		if (packet == null)
		{
			return null;
		}
		return new GenericResponse
		{
			ResultCode = (GenericResponse.Result)packet.ResultCode,
			RequestId = packet.RequestId,
			RequestSubId = (packet.HasRequestSubId ? packet.RequestSubId : 0),
			GenericData = packet.GenericData
		};
	}

	public void RequestNetCacheObject(GetAccountInfo.Request request)
	{
		m_connectApi.RequestAccountInfoNetCacheObject(request);
	}

	public void RequestNetCacheObjectList(List<GetAccountInfo.Request> requestList, List<GenericRequest> genericRequests)
	{
		m_connectApi.RequestNetCacheObjectList(requestList, genericRequests);
	}

	public NetCache.NetCacheProfileProgress GetProfileProgress()
	{
		if (!ShouldBeConnectedToAurora())
		{
			return new NetCache.NetCacheProfileProgress
			{
				CampaignProgress = global::Options.Get().GetEnum<TutorialProgress>(Option.LOCAL_TUTORIAL_PROGRESS)
			};
		}
		ProfileProgress packet = m_connectApi.GetProfileProgress();
		if (packet == null)
		{
			return null;
		}
		return new NetCache.NetCacheProfileProgress
		{
			CampaignProgress = (TutorialProgress)packet.Progress,
			BestForgeWins = packet.BestForge,
			LastForgeDate = (packet.HasLastForge ? TimeUtils.PegDateToFileTimeUtc(packet.LastForge) : 0),
			TutorialComplete = packet.TutorialComplete
		};
	}

	public void SetProgress(long value)
	{
		m_connectApi.SetProgress(value);
	}

	public SetProgressResponse GetSetProgressResponse()
	{
		return m_connectApi.GetSetProgressResponse();
	}

	public void HandleProfileNotices(List<ProfileNotice> notices, ref List<NetCache.ProfileNotice> result)
	{
		for (int i = 0; i < notices.Count; i++)
		{
			ProfileNotice notice = notices[i];
			NetCache.ProfileNotice resultNotice = null;
			if (notice.HasMedal)
			{
				Map<ProfileNoticeMedal.MedalType, PegasusShared.FormatType> medalTypeMap = new Map<ProfileNoticeMedal.MedalType, PegasusShared.FormatType>
				{
					{
						ProfileNoticeMedal.MedalType.UNKNOWN_MEDAL,
						PegasusShared.FormatType.FT_UNKNOWN
					},
					{
						ProfileNoticeMedal.MedalType.WILD_MEDAL,
						PegasusShared.FormatType.FT_WILD
					},
					{
						ProfileNoticeMedal.MedalType.STANDARD_MEDAL,
						PegasusShared.FormatType.FT_STANDARD
					},
					{
						ProfileNoticeMedal.MedalType.CLASSIC_MEDAL,
						PegasusShared.FormatType.FT_CLASSIC
					},
					{
						ProfileNoticeMedal.MedalType.TWIST_MEDAL,
						PegasusShared.FormatType.FT_TWIST
					}
				};
				PegasusShared.FormatType formatType = PegasusShared.FormatType.FT_UNKNOWN;
				if (notice.Medal.HasMedalType_)
				{
					medalTypeMap.TryGetValue(notice.Medal.MedalType_, out formatType);
				}
				NetCache.ProfileNoticeMedal noticeMedal = new NetCache.ProfileNoticeMedal
				{
					LeagueId = notice.Medal.LeagueId,
					StarLevel = notice.Medal.StarLevel,
					LegendRank = (notice.Medal.HasLegendRank ? notice.Medal.LegendRank : 0),
					BestStarLevel = (notice.Medal.HasBestStarLevel ? notice.Medal.BestStarLevel : 0),
					FormatType = formatType,
					WasLimitedByBestEverStarLevel = (notice.Medal.HasWasLimitedByBestEverStarLevel && notice.Medal.WasLimitedByBestEverStarLevel)
				};
				if (notice.Medal.HasChest)
				{
					noticeMedal.Chest = ConvertRewardChest(notice.Medal.Chest);
				}
				resultNotice = noticeMedal;
			}
			else if (notice.HasRewardBooster)
			{
				resultNotice = new NetCache.ProfileNoticeRewardBooster
				{
					Id = notice.RewardBooster.BoosterType,
					Count = notice.RewardBooster.BoosterCount
				};
			}
			else if (notice.HasRewardCard)
			{
				resultNotice = new NetCache.ProfileNoticeRewardCard
				{
					CardID = GameUtils.TranslateDbIdToCardId(notice.RewardCard.Card.Asset),
					Premium = (notice.RewardCard.Card.HasPremium ? ((TAG_PREMIUM)notice.RewardCard.Card.Premium) : TAG_PREMIUM.NORMAL),
					Quantity = ((!notice.RewardCard.HasQuantity) ? 1 : notice.RewardCard.Quantity)
				};
			}
			else if (notice.HasPreconDeck)
			{
				resultNotice = new NetCache.ProfileNoticePreconDeck
				{
					DeckID = notice.PreconDeck.Deck,
					HeroAsset = notice.PreconDeck.Hero
				};
			}
			else if (notice.HasRewardDust)
			{
				resultNotice = new NetCache.ProfileNoticeRewardDust
				{
					Amount = notice.RewardDust.Amount
				};
			}
			else if (notice.HasRewardMount)
			{
				resultNotice = new NetCache.ProfileNoticeRewardMount
				{
					MountID = notice.RewardMount.MountId
				};
			}
			else if (notice.HasRewardForge)
			{
				resultNotice = new NetCache.ProfileNoticeRewardForge
				{
					Quantity = notice.RewardForge.Quantity
				};
			}
			else if (notice.HasRewardCurrency)
			{
				resultNotice = new NetCache.ProfileNoticeRewardCurrency
				{
					Amount = notice.RewardCurrency.Amount,
					CurrencyType = ((!notice.HasRewardCurrency) ? PegasusShared.CurrencyType.CURRENCY_TYPE_GOLD : notice.RewardCurrency.CurrencyType)
				};
			}
			else if (notice.HasPurchase)
			{
				resultNotice = new NetCache.ProfileNoticePurchase
				{
					PMTProductID = (notice.Purchase.HasPmtProductId ? new long?(notice.Purchase.PmtProductId) : ((long?)null)),
					Data = (notice.Purchase.HasData ? notice.Purchase.Data : 0),
					CurrencyCode = notice.Purchase.CurrencyCode
				};
			}
			else if (notice.HasRewardCardBack)
			{
				resultNotice = new NetCache.ProfileNoticeRewardCardBack
				{
					CardBackID = notice.RewardCardBack.CardBack
				};
			}
			else if (notice.HasBonusStars)
			{
				resultNotice = new NetCache.ProfileNoticeBonusStars
				{
					StarLevel = notice.BonusStars.StarLevel,
					Stars = notice.BonusStars.Stars
				};
			}
			else if (notice.HasDcGameResult)
			{
				if (!notice.DcGameResult.HasGameType)
				{
					Debug.LogError("Network.GetProfileNotices(): Missing GameType");
					continue;
				}
				if (!notice.DcGameResult.HasMissionId)
				{
					Debug.LogError("Network.GetProfileNotices(): Missing GameType");
					continue;
				}
				if (!notice.DcGameResult.HasGameResult_)
				{
					Debug.LogError("Network.GetProfileNotices(): Missing GameResult");
					continue;
				}
				NetCache.ProfileNoticeDisconnectedGame noticeDisconnectedGame = new NetCache.ProfileNoticeDisconnectedGame
				{
					GameType = notice.DcGameResult.GameType,
					FormatType = notice.DcGameResult.FormatType,
					MissionId = notice.DcGameResult.MissionId,
					GameResult = notice.DcGameResult.GameResult_
				};
				if (noticeDisconnectedGame.GameResult == ProfileNoticeDisconnectedGameResult.GameResult.GR_WINNER)
				{
					if (!notice.DcGameResult.HasYourResult || !notice.DcGameResult.HasOpponentResult)
					{
						Debug.LogError("Network.GetProfileNotices(): Missing PlayerResult");
						continue;
					}
					noticeDisconnectedGame.YourResult = notice.DcGameResult.YourResult;
					noticeDisconnectedGame.OpponentResult = notice.DcGameResult.OpponentResult;
				}
				resultNotice = noticeDisconnectedGame;
			}
			else if (notice.HasDcGameResultNew)
			{
				if (!notice.DcGameResultNew.HasGameType)
				{
					Debug.LogError("Network.GetProfileNotices(): Missing GameType");
					continue;
				}
				if (!notice.DcGameResultNew.HasMissionId)
				{
					Debug.LogError("Network.GetProfileNotices(): Missing GameType");
					continue;
				}
				if (!notice.DcGameResultNew.HasGameResult_)
				{
					Debug.LogError("Network.GetProfileNotices(): Missing GameResult");
					continue;
				}
				NetCache.ProfileNoticeDisconnectedGame noticeDisconnectedGame2 = new NetCache.ProfileNoticeDisconnectedGame
				{
					GameType = notice.DcGameResultNew.GameType,
					FormatType = notice.DcGameResultNew.FormatType,
					MissionId = notice.DcGameResultNew.MissionId,
					GameResult = (ProfileNoticeDisconnectedGameResult.GameResult)notice.DcGameResultNew.GameResult_
				};
				if (noticeDisconnectedGame2.GameResult == ProfileNoticeDisconnectedGameResult.GameResult.GR_WINNER)
				{
					if (!notice.DcGameResultNew.HasYourResult)
					{
						Debug.LogError("Network.GetProfileNotices(): Missing New PlayerResult");
					}
					noticeDisconnectedGame2.YourResult = (ProfileNoticeDisconnectedGameResult.PlayerResult)notice.DcGameResultNew.YourResult;
				}
				resultNotice = noticeDisconnectedGame2;
			}
			else if (notice.HasAdventureProgress)
			{
				NetCache.ProfileNoticeAdventureProgress noticeAdventureProgress = new NetCache.ProfileNoticeAdventureProgress
				{
					Wing = notice.AdventureProgress.WingId
				};
				switch ((NetCache.ProfileNotice.NoticeOrigin)notice.Origin)
				{
				case NetCache.ProfileNotice.NoticeOrigin.ADVENTURE_PROGRESS:
					noticeAdventureProgress.Progress = (int)(notice.HasOriginData ? notice.OriginData : 0);
					break;
				case NetCache.ProfileNotice.NoticeOrigin.ADVENTURE_FLAGS:
					noticeAdventureProgress.Flags = (ulong)(notice.HasOriginData ? notice.OriginData : 0);
					break;
				}
				resultNotice = noticeAdventureProgress;
			}
			else if (notice.HasLevelUp)
			{
				resultNotice = new NetCache.ProfileNoticeLevelUp
				{
					HeroClass = notice.LevelUp.HeroClass,
					NewLevel = notice.LevelUp.NewLevel,
					TotalLevel = notice.LevelUp.TotalLevel
				};
			}
			else if (notice.HasAccountLicense)
			{
				resultNotice = new NetCache.ProfileNoticeAcccountLicense
				{
					License = notice.AccountLicense.License,
					CasID = notice.AccountLicense.CasId
				};
			}
			else if (notice.HasTavernBrawlRewards)
			{
				resultNotice = new NetCache.ProfileNoticeTavernBrawlRewards
				{
					Chest = notice.TavernBrawlRewards.RewardChest,
					Wins = notice.TavernBrawlRewards.NumWins,
					Mode = (notice.TavernBrawlRewards.HasBrawlMode ? notice.TavernBrawlRewards.BrawlMode : TavernBrawlMode.TB_MODE_NORMAL)
				};
			}
			else if (notice.HasTavernBrawlTicket)
			{
				resultNotice = new NetCache.ProfileNoticeTavernBrawlTicket
				{
					TicketType = notice.TavernBrawlTicket.TicketType,
					Quantity = notice.TavernBrawlTicket.Quantity
				};
			}
			else if (notice.HasGenericRewardChest)
			{
				NetCache.ProfileNoticeGenericRewardChest noticeGenericRewardChest = new NetCache.ProfileNoticeGenericRewardChest();
				noticeGenericRewardChest.RewardChestAssetId = notice.GenericRewardChest.RewardChestAssetId;
				noticeGenericRewardChest.RewardChest = notice.GenericRewardChest.RewardChest;
				noticeGenericRewardChest.RewardChestByteSize = 0u;
				noticeGenericRewardChest.RewardChestHash = null;
				if (notice.GenericRewardChest.HasRewardChestByteSize)
				{
					noticeGenericRewardChest.RewardChestByteSize = notice.GenericRewardChest.RewardChestByteSize;
				}
				if (notice.GenericRewardChest.HasRewardChestHash)
				{
					noticeGenericRewardChest.RewardChestHash = notice.GenericRewardChest.RewardChestHash;
				}
				resultNotice = noticeGenericRewardChest;
			}
			else if (notice.HasLeaguePromotionRewards)
			{
				resultNotice = new NetCache.ProfileNoticeLeaguePromotionRewards
				{
					Chest = notice.LeaguePromotionRewards.RewardChest,
					LeagueId = notice.LeaguePromotionRewards.LeagueId
				};
			}
			else if (notice.HasDeckRemoved)
			{
				resultNotice = new NetCache.ProfileNoticeDeckRemoved
				{
					DeckID = notice.DeckRemoved.DeckId
				};
			}
			else if (notice.HasDeckGranted)
			{
				resultNotice = new NetCache.ProfileNoticeDeckGranted
				{
					DeckDbiID = notice.DeckGranted.DeckDbiId,
					ClassId = notice.DeckGranted.ClassId,
					PlayerDeckID = notice.DeckGranted.PlayerDeckId
				};
			}
			else if (notice.HasMiniSetGranted)
			{
				resultNotice = new NetCache.ProfileNoticeMiniSetGranted
				{
					MiniSetID = notice.MiniSetGranted.MiniSetId,
					Premium = notice.MiniSetGranted.Premium
				};
			}
			else if (notice.HasSellableDeckGranted)
			{
				resultNotice = new NetCache.ProfileNoticeSellableDeckGranted
				{
					SellableDeckID = notice.SellableDeckGranted.SellableDeckId,
					PlayerDeckID = notice.SellableDeckGranted.PlayerDeckId,
					Premium = (TAG_PREMIUM)notice.SellableDeckGranted.Premium
				};
			}
			else if (notice.HasBattlegroundsGuideSkinGranted)
			{
				int battlegroundsGuideSkinId = (int)notice.BattlegroundsGuideSkinGranted.BattlegroundsGuideSkinId;
				string skinCardIdTranslated = GameUtils.TranslateDbIdToCardId(GameDbf.BattlegroundsGuideSkin.GetRecord(battlegroundsGuideSkinId).SkinCardId);
				FixedRewardDbfRecord fixedRewardRecord = GameDbf.FixedReward.GetRecord((FixedRewardDbfRecord x) => x.BattlegroundsGuideSkinId == battlegroundsGuideSkinId);
				FixedRewardMapDbfRecord rewardMapRecord = ((fixedRewardRecord != null) ? GameDbf.FixedRewardMap.GetRecord((FixedRewardMapDbfRecord x) => x.RewardId == fixedRewardRecord.ID) : null);
				resultNotice = new NetCache.ProfileNoticeRewardBattlegroundsGuideSkin
				{
					CardID = skinCardIdTranslated,
					FixedRewardMapID = (rewardMapRecord?.ID ?? 0)
				};
			}
			else if (notice.HasBattlegroundsHeroSkinGranted)
			{
				int battlegroundsHeroSkinId = (int)notice.BattlegroundsHeroSkinGranted.BattlegroundsHeroSkinId;
				int skinCardId = GameDbf.BattlegroundsHeroSkin.GetRecord(battlegroundsHeroSkinId).SkinCardId;
				GameUtils.TranslateDbIdToCardId(skinCardId);
				FixedRewardDbfRecord fixedRewardRecord2 = GameDbf.FixedReward.GetRecord((FixedRewardDbfRecord x) => x.BattlegroundsHeroSkinId == battlegroundsHeroSkinId);
				FixedRewardMapDbfRecord rewardMapRecord2 = ((fixedRewardRecord2 != null) ? GameDbf.FixedRewardMap.GetRecord((FixedRewardMapDbfRecord x) => x.RewardId == fixedRewardRecord2.ID) : null);
				resultNotice = new NetCache.ProfileNoticeRewardBattlegroundsHeroSkin
				{
					CardID = GameUtils.TranslateDbIdToCardId(skinCardId),
					FixedRewardMapID = (rewardMapRecord2?.ID ?? 0)
				};
			}
			else if (notice.HasMercenariesRewards)
			{
				resultNotice = new NetCache.ProfileNoticeMercenariesRewards
				{
					RewardType = notice.MercenariesRewards.RewardType_,
					Chest = notice.MercenariesRewards.RewardChest
				};
			}
			else if (notice.HasMercenariesAbilityUnlock)
			{
				resultNotice = new NetCache.ProfileNoticeMercenariesAbilityUnlock
				{
					MercenaryId = notice.MercenariesAbilityUnlock.MercenaryId,
					AbilityId = notice.MercenariesAbilityUnlock.AbilityId
				};
			}
			else if (notice.HasMercenariesSeasonRoll)
			{
				resultNotice = new NetCache.ProfileNoticeMercenariesSeasonRoll
				{
					EndedSeasonId = notice.MercenariesSeasonRoll.EndedSeasonId,
					HighestSeasonRating = notice.MercenariesSeasonRoll.HighestSeasonRating
				};
			}
			else if (notice.HasMercenariesBoosterLicense)
			{
				resultNotice = new NetCache.ProfileNoticeMercenariesBoosterLicense
				{
					Count = notice.MercenariesBoosterLicense.Count
				};
			}
			else if (notice.HasMercenariesCurrencyLicense)
			{
				resultNotice = new NetCache.ProfileNoticeMercenariesCurrencyLicense
				{
					MercenaryId = notice.MercenariesCurrencyLicense.MercenaryId,
					CurrencyAmount = notice.MercenariesCurrencyLicense.CurrencyAmount
				};
			}
			else if (notice.HasMercenariesMercenaryLicense)
			{
				resultNotice = new NetCache.ProfileNoticeMercenariesMercenaryLicense
				{
					MercenaryId = notice.MercenariesMercenaryLicense.MercenaryId,
					ArtVariationId = notice.MercenariesMercenaryLicense.ArtVariationId,
					ArtVariationPremium = notice.MercenariesMercenaryLicense.ArtVariationPremium,
					CurrencyAmount = notice.MercenariesMercenaryLicense.CurrencyAmount
				};
			}
			else if (notice.HasMercenariesRandomRewardLicense)
			{
				ProfileNoticeMercenariesRandomRewardLicense serverNotice = notice.MercenariesRandomRewardLicense;
				NetCache.ProfileNoticeMercenariesRandomRewardLicense noticeMercenariesRandomRewardLicense = new NetCache.ProfileNoticeMercenariesRandomRewardLicense();
				noticeMercenariesRandomRewardLicense.MercenaryId = serverNotice.MercenaryId;
				noticeMercenariesRandomRewardLicense.ArtVariationId = serverNotice.ArtVariationId;
				noticeMercenariesRandomRewardLicense.ArtVariationPremium = serverNotice.ArtVariationPremium;
				noticeMercenariesRandomRewardLicense.CurrencyAmount = serverNotice.CurrencyAmount;
				if (serverNotice.HasArtVariationId && serverNotice.HasCurrencyAmount)
				{
					noticeMercenariesRandomRewardLicense.IsConvertedMercenary = true;
				}
				resultNotice = noticeMercenariesRandomRewardLicense;
			}
			else if (notice.HasMercenariesMercenaryFullyUpgraded)
			{
				resultNotice = new NetCache.ProfileNoticeMercenariesMercenaryFullyUpgraded
				{
					MercenaryId = notice.MercenariesMercenaryFullyUpgraded.MercenaryId
				};
			}
			else if (notice.HasMercenariesSeasonRewards)
			{
				resultNotice = new NetCache.ProfileNoticeMercenariesSeasonRewards
				{
					Chest = notice.MercenariesSeasonRewards.RewardChest,
					RewardAssetId = notice.MercenariesSeasonRewards.RewardAssetId
				};
			}
			else if (notice.HasMercenariesZoneUnlock)
			{
				resultNotice = new NetCache.ProfileNoticeMercenariesZoneUnlock
				{
					ZoneId = notice.MercenariesZoneUnlock.ZoneId
				};
			}
			else if (notice.HasBattlegroundsBoardSkinGranted)
			{
				long boardSkinID = notice.BattlegroundsBoardSkinGranted.BattlegroundsBoardSkinId;
				FixedRewardDbfRecord fixedRewardRecord3 = GameDbf.FixedReward.GetRecord((FixedRewardDbfRecord x) => x.BattlegroundsBoardSkinId == boardSkinID);
				FixedRewardMapDbfRecord rewardMapRecord3 = ((fixedRewardRecord3 != null) ? GameDbf.FixedRewardMap.GetRecord((FixedRewardMapDbfRecord x) => x.RewardId == fixedRewardRecord3.ID) : null);
				resultNotice = new NetCache.ProfileNoticeRewardBattlegroundsBoard
				{
					BoardSkinID = boardSkinID,
					FixedRewardMapID = (rewardMapRecord3?.ID ?? 0)
				};
			}
			else if (notice.HasBattlegroundsFinisherGranted)
			{
				long finisherID = notice.BattlegroundsFinisherGranted.BattlegroundsFinisherId;
				FixedRewardDbfRecord fixedRewardRecord4 = GameDbf.FixedReward.GetRecord((FixedRewardDbfRecord x) => x.BattlegroundsFinisherId == finisherID);
				FixedRewardMapDbfRecord rewardMapRecord4 = ((fixedRewardRecord4 != null) ? GameDbf.FixedRewardMap.GetRecord((FixedRewardMapDbfRecord x) => x.RewardId == fixedRewardRecord4.ID) : null);
				resultNotice = new NetCache.ProfileNoticeRewardBattlegroundsFinisher
				{
					FinisherID = finisherID,
					FixedRewardMapID = (rewardMapRecord4?.ID ?? 0)
				};
			}
			else if (notice.HasBattlegroundsEmoteGranted)
			{
				long emoteID = notice.BattlegroundsEmoteGranted.BattlegroundsEmoteId;
				FixedRewardDbfRecord fixedRewardRecord5 = GameDbf.FixedReward.GetRecord((FixedRewardDbfRecord x) => x.BattlegroundsEmoteId == emoteID);
				FixedRewardMapDbfRecord rewardMapRecord5 = ((fixedRewardRecord5 != null) ? GameDbf.FixedRewardMap.GetRecord((FixedRewardMapDbfRecord x) => x.RewardId == fixedRewardRecord5.ID) : null);
				resultNotice = new NetCache.ProfileNoticeRewardBattlegroundsEmote
				{
					EmoteID = emoteID,
					FixedRewardMapID = (rewardMapRecord5?.ID ?? 0)
				};
			}
			else if (notice.HasLuckyDrawReward)
			{
				resultNotice = new NetCache.ProfileNoticeLuckyDrawReward
				{
					LuckyDrawRewardId = notice.LuckyDrawReward.LuckyDrawRewardId,
					LuckyDrawOrigin = notice.LuckyDrawReward.LuckyDrawOrigin
				};
			}
			else if (notice.HasRedundantNdeReroll)
			{
				resultNotice = new NetCache.ProfileNoticeRedundantNDEReroll
				{
					CardID = GameUtils.TranslateDbIdToCardId(notice.RedundantNdeReroll.NdeCard.Asset),
					Premium = (TAG_PREMIUM)notice.RedundantNdeReroll.NdeCard.Premium,
					RerollPremiumOverride = (notice.RedundantNdeReroll.HasForcedRerollPremium ? ((TAG_PREMIUM)notice.RedundantNdeReroll.ForcedRerollPremium) : TAG_PREMIUM.MAX)
				};
			}
			else if (notice.HasRedundantNdeRerollResult)
			{
				resultNotice = new NetCache.ProfileNoticeRedundantNDERerollResult
				{
					RerolledCardID = notice.RedundantNdeRerollResult.NdeCard.Asset,
					GrantedCardID = notice.RedundantNdeRerollResult.GrantedCard.Asset,
					Premium = (TAG_PREMIUM)notice.RedundantNdeRerollResult.GrantedCard.Premium
				};
			}
			else
			{
				Debug.LogError("Network.GetProfileNotices(): Unrecognized profile notice");
			}
			if (resultNotice == null)
			{
				Debug.LogError("Network.GetProfileNotices(): Unhandled notice type! This notice will be lost!");
				continue;
			}
			resultNotice.NoticeID = notice.Entry;
			resultNotice.Origin = (NetCache.ProfileNotice.NoticeOrigin)notice.Origin;
			resultNotice.OriginData = (notice.HasOriginData ? notice.OriginData : 0);
			resultNotice.Date = TimeUtils.PegDateToFileTimeUtc(notice.When);
			result.Add(resultNotice);
		}
	}

	public NetCache.NetCacheMedalInfo GetMedalInfo()
	{
		if (!ShouldBeConnectedToAurora())
		{
			NetCache.NetCacheMedalInfo defaultMedalInfo = new NetCache.NetCacheMedalInfo();
			{
				foreach (PegasusShared.FormatType enumValue in Enum.GetValues(typeof(PegasusShared.FormatType)))
				{
					if (enumValue != 0)
					{
						MedalInfoData emptyInfoData = new MedalInfoData
						{
							FormatType = enumValue
						};
						defaultMedalInfo.MedalData.Add(enumValue, emptyInfoData);
					}
				}
				return defaultMedalInfo;
			}
		}
		MedalInfo packet = m_connectApi.GetMedalInfo();
		if (packet == null)
		{
			return null;
		}
		return new NetCache.NetCacheMedalInfo(packet);
	}

	public NetCache.NetCacheBaconRatingInfo GetBaconRatingInfo()
	{
		if (!ShouldBeConnectedToAurora())
		{
			return new NetCache.NetCacheBaconRatingInfo
			{
				Rating = 0
			};
		}
		ResponseWithRequest<BattlegroundsRatingInfoResponse, BattlegroundsRatingInfoRequest> response = m_connectApi.BattlegroundsRatingInfoResponse();
		if (response == null)
		{
			return null;
		}
		BattlegroundsRatingInfoResponse packet = response.Response;
		if (packet == null)
		{
			return null;
		}
		return new NetCache.NetCacheBaconRatingInfo
		{
			Rating = packet.PlayerInfo.Rating,
			DuosRating = packet.PlayerInfo.DuosRating
		};
	}

	public NetCache.NetCachePVPDRStatsInfo GetPVPDRStatsInfo()
	{
		if (!ShouldBeConnectedToAurora())
		{
			return new NetCache.NetCachePVPDRStatsInfo
			{
				Rating = 0,
				PaidRating = 0,
				HighWatermark = 0
			};
		}
		ResponseWithRequest<PVPDRStatsInfoResponse, PVPDRStatsInfoRequest> response = m_connectApi.PVPDRStatsInfoResponse();
		if (response == null)
		{
			return null;
		}
		PVPDRStatsInfoResponse packet = response.Response;
		if (packet == null)
		{
			return null;
		}
		return new NetCache.NetCachePVPDRStatsInfo
		{
			Rating = packet.Rating,
			PaidRating = packet.PaidRating,
			HighWatermark = packet.HighWatermark
		};
	}

	public GuardianVars GetGuardianVars()
	{
		if (!ShouldBeConnectedToAurora())
		{
			return new GuardianVars();
		}
		return m_connectApi.GetGuardianVars();
	}

	public PlayerRecords GetPlayerRecordsPacket()
	{
		if (!ShouldBeConnectedToAurora())
		{
			return new PlayerRecords();
		}
		return m_connectApi.GetPlayerRecords();
	}

	public static NetCache.NetCachePlayerRecords GetPlayerRecords(PlayerRecords packet)
	{
		if (packet == null)
		{
			return null;
		}
		NetCache.NetCachePlayerRecords result = new NetCache.NetCachePlayerRecords();
		for (int i = 0; i < packet.Records.Count; i++)
		{
			PlayerRecord record = packet.Records[i];
			result.Records.Add(new NetCache.PlayerRecord
			{
				RecordType = record.Type,
				Data = (record.HasData ? record.Data : 0),
				Wins = record.Wins,
				Losses = record.Losses,
				Ties = record.Ties
			});
		}
		return result;
	}

	public NetCache.NetCacheRewardProgress GetRewardProgress()
	{
		if (!ShouldBeConnectedToAurora())
		{
			return new NetCache.NetCacheRewardProgress();
		}
		RewardProgress packet = m_connectApi.GetRewardProgress();
		if (packet == null)
		{
			return null;
		}
		return new NetCache.NetCacheRewardProgress
		{
			Season = packet.SeasonNumber,
			SeasonEndDate = TimeUtils.PegDateToFileTimeUtc(packet.SeasonEnd),
			NextQuestCancelDate = TimeUtils.PegDateToFileTimeUtc(packet.NextQuestCancel)
		};
	}

	public NetCache.NetCacheGamesPlayed GetGamesInfo()
	{
		GamesInfo packet = m_connectApi.GetGamesInfo();
		if (packet == null)
		{
			return null;
		}
		return new NetCache.NetCacheGamesPlayed
		{
			GamesStarted = packet.GamesStarted,
			GamesWon = packet.GamesWon,
			GamesLost = packet.GamesLost
		};
	}

	public ClientStaticAssetsResponse GetClientStaticAssetsResponse()
	{
		if (!ShouldBeConnectedToAurora())
		{
			return new ClientStaticAssetsResponse();
		}
		return m_connectApi.GetClientStaticAssetsResponse();
	}

	public void RequestTavernBrawlInfo(BrawlType brawlType)
	{
		m_connectApi.RequestTavernBrawlInfo(brawlType);
	}

	public void RequestTavernBrawlPlayerRecord(BrawlType brawlType)
	{
		m_connectApi.RequestTavernBrawlPlayerRecord(brawlType);
	}

	public TavernBrawlInfo GetTavernBrawlInfo()
	{
		if (!ShouldBeConnectedToAurora())
		{
			return new TavernBrawlInfo();
		}
		return m_connectApi.GetTavernBrawlInfo();
	}

	public TavernBrawlRequestSessionBeginResponse GetTavernBrawlSessionBegin()
	{
		return m_connectApi.GetTavernBrawlSessionBeginResponse();
	}

	public void TavernBrawlRetire()
	{
		m_connectApi.TavernBrawlRetire();
	}

	public TavernBrawlRequestSessionRetireResponse GetTavernBrawlSessionRetired()
	{
		return m_connectApi.GetTavernBrawlSessionRetired();
	}

	public void RequestTavernBrawlSessionBegin()
	{
		m_connectApi.RequestTavernBrawlSessionBegin();
	}

	public void AckTavernBrawlSessionRewards()
	{
		m_connectApi.AckTavernBrawlSessionRewards();
	}

	public TavernBrawlPlayerRecord GetTavernBrawlRecord()
	{
		if (!ShouldBeConnectedToAurora())
		{
			return new TavernBrawlPlayerRecord();
		}
		return m_connectApi.GeTavernBrawlPlayerRecordResponse()?.Record;
	}

	public FavoriteHeroesResponse GetFavoriteHeroesResponse()
	{
		if (!ShouldBeConnectedToAurora())
		{
			return new FavoriteHeroesResponse();
		}
		return m_connectApi.GetFavoriteHeroesResponse();
	}

	public AccountLicensesInfoResponse GetAccountLicensesInfoResponse()
	{
		if (!ShouldBeConnectedToAurora())
		{
			return new AccountLicensesInfoResponse();
		}
		return m_connectApi.GetAccountLicensesInfoResponse();
	}

	public void RequestAccountLicensesUpdate()
	{
		m_connectApi.RequestAccountLicensesUpdate();
	}

	public UpdateAccountLicensesResponse GetUpdateAccountLicensesResponse()
	{
		return m_connectApi.GetUpdateAccountLicensesResponse();
	}

	public HeroXP GetHeroXP()
	{
		if (!ShouldBeConnectedToAurora())
		{
			return new HeroXP();
		}
		return m_connectApi.GetHeroXP();
	}

	public void AckNotice(long id)
	{
		if (NetCache.Get().RemoveNotice(id))
		{
			Log.Achievements.Print("acking notice: {0}", id);
			m_connectApi.AckNotice(id);
		}
	}

	public void AckAchieveProgress(int id, int ackProgress)
	{
		Log.Achievements.Print("AckAchieveProgress: Achieve={0} Progress={1}", id, ackProgress);
		m_connectApi.AckAchieveProgress(id, ackProgress);
	}

	public void AckQuest(int questId)
	{
		m_connectApi.AckQuest(questId);
	}

	public void CheckForNewQuests()
	{
		m_connectApi.CheckForNewQuests();
	}

	public void CheckForExpiredQuests()
	{
		m_connectApi.CheckForExpiredQuests();
	}

	public void RerollQuest(int questId)
	{
		m_connectApi.RerollQuest(questId);
	}

	public void AbandonQuest(int questId)
	{
		m_connectApi.AbandonQuest(questId);
	}

	public void AckAchievement(int achievementId)
	{
		m_connectApi.AckAchievement(achievementId);
	}

	public void ClaimAchievementReward(int achievementId, int chooseOneRewardId = 0)
	{
		m_connectApi.ClaimAchievementReward(achievementId, chooseOneRewardId);
	}

	public void AckRewardTrackReward(int rewardTrackId, int level, RewardTrackPaidType paidType)
	{
		m_connectApi.AckRewardTrackReward(rewardTrackId, level, paidType);
	}

	public void ClaimRewardTrackReward(int rewardTrackId, int level, RewardTrackPaidType paidType, int chooseOneRewardItemId)
	{
		m_connectApi.ClaimRewardTrackReward(rewardTrackId, level, paidType, chooseOneRewardItemId);
	}

	public void CheckForRewardTrackSeasonRoll()
	{
		m_connectApi.CheckForRewardTrackSeasonRoll();
	}

	public void SetActiveEventRewardTrack(int rewardTrackId)
	{
		m_connectApi.SetActiveEventRewardTrack(rewardTrackId);
	}

	public void CheckAccountLicenseAchieve(int achieveID)
	{
		m_connectApi.CheckAccountLicenseAchieve(achieveID);
	}

	public AccountLicenseAchieveResponse GetAccountLicenseAchieveResponse()
	{
		PegasusUtil.AccountLicenseAchieveResponse packet = m_connectApi.GetAccountLicenseAchieveResponse();
		if (packet == null)
		{
			return null;
		}
		return new AccountLicenseAchieveResponse
		{
			Achieve = packet.Achieve,
			Result = (AccountLicenseAchieveResponse.AchieveResult)packet.Result_
		};
	}

	public void RespondToRedundantNDEReroll(List<long> noticeIds, bool didReroll)
	{
		m_connectApi.RespondToRedundantNDEReroll(noticeIds, didReroll);
	}

	public void AckCardSeenBefore(int assetId, TAG_PREMIUM premium)
	{
		PegasusShared.CardDef def = new PegasusShared.CardDef
		{
			Asset = assetId
		};
		if (premium != 0)
		{
			def.Premium = (int)premium;
		}
		m_ackCardSeenPacket.CardDefs.Add(def);
		if (m_ackCardSeenPacket.CardDefs.Count > 15)
		{
			SendAckCardsSeen();
		}
	}

	public void AckWingProgress(int wingId, int ackId)
	{
		m_connectApi.AckWingProgress(wingId, ackId);
	}

	public void AcknowledgeBanner(int banner)
	{
		m_connectApi.AcknowledgeBanner(banner);
	}

	public void SendAckCardsSeen()
	{
		m_connectApi.AckCardSeen(m_ackCardSeenPacket);
		m_ackCardSeenPacket.CardDefs.Clear();
	}

	public void GetCustomizedCardHistoryRequest(uint maximumCount)
	{
		m_connectApi.GetCustomizedCardHistoryRequest(maximumCount);
	}

	public GetCustomizedCardHistoryResponse GetCustomizedCardHistoryResponse()
	{
		return m_connectApi.CustomizedCardHistoryResponse();
	}

	public void RequestSkipApprentice()
	{
		m_connectApi.RequestSkipApprentice();
	}

	public void UnlockBattlegroundsDuringApprentice()
	{
		m_connectApi.UnlockBattlegroundsDuringApprentice();
		GameUtils.MarkBattlegGroundsTutorialComplete();
		Box box = Box.Get();
		if (box != null)
		{
			box.EnableBattlegroundsButton();
		}
	}

	public void UpdateCustomizedCard(CustomizedCard customizedCard)
	{
		m_connectApi.UpdateCustomizedCard(customizedCard);
	}

	public SkipApprenticeResponse GetSkipApprenticeResponse()
	{
		return m_connectApi.GetSkipApprenticeResponse();
	}

	public SmartDeckResponse GetSmartDeckResponse()
	{
		return m_connectApi.GetSmartDeckResponse();
	}

	public PlayerQuestStateUpdate GetPlayerQuestStateUpdate()
	{
		return m_connectApi.GetPlayerQuestStateUpdate();
	}

	public PlayerQuestPoolStateUpdate GetPlayerQuestPoolStateUpdate()
	{
		return m_connectApi.GetPlayerQuestPoolStateUpdate();
	}

	public PlayerAchievementStateUpdate GetPlayerAchievementStateUpdate()
	{
		return m_connectApi.GetPlayerAchievementStateUpdate();
	}

	public PlayerRewardTrackStateUpdate GetPlayerRewardTrackStateUpdate()
	{
		return m_connectApi.GetPlayerRewardTrackStateUpdate();
	}

	public RerollQuestResponse GetRerollQuestResponse()
	{
		return m_connectApi.GetRerollQuestResponse();
	}

	public RewardTrackXpNotification GetRewardTrackXpNotification()
	{
		return m_connectApi.GetRewardTrackXpNotification();
	}

	public RewardTrackUnclaimedNotification GetRewardTrackUnclaimedNotification()
	{
		return m_connectApi.GetRewardTrackUnclaimedNotification();
	}

	public BattlegroundsHeroSkinsResponse GetBattlegroundsHeroSkinsResponse()
	{
		if (!ShouldBeConnectedToAurora())
		{
			return new BattlegroundsHeroSkinsResponse();
		}
		return m_connectApi.GetBattlegroundsHeroSkinsResponse();
	}

	public SetBattlegroundsFavoriteHeroSkinResponse GetSetBattlegroundsFavoriteHeroSkinResponse()
	{
		return m_connectApi.GetSetBattlegroundsFavoriteHeroSkinResponse();
	}

	public void UpdateFavoriteBattlegroundsHeroSkin(int baseHeroCardDbid, int battlegroundsHeroSkinId, bool favorite)
	{
		if (favorite)
		{
			SetBattlegroundsFavoriteHeroSkin(baseHeroCardDbid, battlegroundsHeroSkinId);
		}
		else
		{
			ClearBattlegroundsFavoriteHeroSkin(baseHeroCardDbid, battlegroundsHeroSkinId);
		}
	}

	private void SetBattlegroundsFavoriteHeroSkin(int baseHeroCardDbid, int battlegroundsHeroSkinId)
	{
		if (IsLoggedIn())
		{
			m_connectApi.SetBattlegroundsFavoriteHeroSkin(baseHeroCardDbid, battlegroundsHeroSkinId);
			return;
		}
		NetCache.NetCacheBattlegroundsHeroSkins netCacheBGHeroSkins = NetCache.Get().GetNetObject<NetCache.NetCacheBattlegroundsHeroSkins>();
		if (netCacheBGHeroSkins != null)
		{
			netCacheBGHeroSkins.BattlegroundsFavoriteHeroSkins.TryGetValue(baseHeroCardDbid, out var favoriteSkins);
			if (favoriteSkins == null)
			{
				favoriteSkins = new HashSet<int>();
				netCacheBGHeroSkins.BattlegroundsFavoriteHeroSkins[baseHeroCardDbid] = favoriteSkins;
			}
			favoriteSkins.Add(battlegroundsHeroSkinId);
		}
	}

	private void ClearBattlegroundsFavoriteHeroSkin(int baseHeroCardDbid, int battlegroundsHeroSkinId)
	{
		if (IsLoggedIn())
		{
			m_connectApi.ClearBattlegroundsFavoriteHeroSkin(baseHeroCardDbid, battlegroundsHeroSkinId);
			return;
		}
		NetCache.NetCacheBattlegroundsHeroSkins netCacheBGHeroSkins = NetCache.Get().GetNetObject<NetCache.NetCacheBattlegroundsHeroSkins>();
		if (netCacheBGHeroSkins != null)
		{
			netCacheBGHeroSkins.BattlegroundsFavoriteHeroSkins.TryGetValue(baseHeroCardDbid, out var favoriteSkins);
			favoriteSkins?.Remove(battlegroundsHeroSkinId);
		}
	}

	public ClearBattlegroundsFavoriteHeroSkinResponse GetClearBattlegroundsFavoriteHeroSkinResponse()
	{
		return m_connectApi.GetClearBattlegroundsFavoriteHeroSkinResponse();
	}

	public BattlegroundsGuideSkinsResponse GetBattlegroundsGuideSkinsResponse()
	{
		if (!ShouldBeConnectedToAurora())
		{
			return new BattlegroundsGuideSkinsResponse();
		}
		return m_connectApi.GetBattlegroundsGuideSkinsResponse();
	}

	public SetBattlegroundsFavoriteGuideSkinResponse GetSetBattlegroundsFavoriteGuideSkinResponse()
	{
		return m_connectApi.GetSetBattlegroundsFavoriteGuideSkinResponse();
	}

	public void SetBattlegroundsFavoriteGuideSkin(BattlegroundsGuideSkinId bgFavoriteGuideSkinID)
	{
		if (IsLoggedIn())
		{
			m_connectApi.SetBattlegroundsFavoriteGuideSkin(bgFavoriteGuideSkinID.ToValue());
			return;
		}
		NetCache.NetCacheBattlegroundsGuideSkins netCacheBGGuideSkins = NetCache.Get().GetNetObject<NetCache.NetCacheBattlegroundsGuideSkins>();
		if (netCacheBGGuideSkins != null)
		{
			netCacheBGGuideSkins.BattlegroundsFavoriteGuideSkin = bgFavoriteGuideSkinID;
		}
	}

	public void ClearBattlegroundsFavoriteGuideSkin()
	{
		if (IsLoggedIn())
		{
			m_connectApi.ClearBattlegroundsFavoriteGuideSkin();
			return;
		}
		NetCache.NetCacheBattlegroundsGuideSkins netCacheBGGuideSkins = NetCache.Get().GetNetObject<NetCache.NetCacheBattlegroundsGuideSkins>();
		if (netCacheBGGuideSkins != null)
		{
			netCacheBGGuideSkins.BattlegroundsFavoriteGuideSkin = null;
		}
	}

	public ClearBattlegroundsFavoriteGuideSkinResponse GetClearBattlegroundsFavoriteGuideSkinResponse()
	{
		return m_connectApi.GetClearBattlegroundsFavoriteGuideSkinResponse();
	}

	public bool TryAddSeenBattlegroundsHeroSkin(BattlegroundsHeroSkinId skinId)
	{
		NetCache.NetCacheBattlegroundsHeroSkins netCacheBattlegroundsHeroSkins = NetCache.Get().GetNetObject<NetCache.NetCacheBattlegroundsHeroSkins>();
		if (netCacheBattlegroundsHeroSkins == null || !netCacheBattlegroundsHeroSkins.UnseenSkinIds.Remove(skinId))
		{
			return false;
		}
		m_ackBattlegroundsSkinsSeenPacket.HeroSkins.Add(skinId.ToValue());
		CheckForSendingBattlegroundsSkinsSeenPacket();
		return true;
	}

	public bool TryAddSeenBattlegroundsGuideSkin(BattlegroundsGuideSkinId skinId)
	{
		NetCache.NetCacheBattlegroundsGuideSkins netCacheBattlegroundsGuideSkins = NetCache.Get().GetNetObject<NetCache.NetCacheBattlegroundsGuideSkins>();
		if (netCacheBattlegroundsGuideSkins == null || !netCacheBattlegroundsGuideSkins.UnseenSkinIds.Remove(skinId))
		{
			return false;
		}
		m_ackBattlegroundsSkinsSeenPacket.GuideSkins.Add(skinId.ToValue());
		CheckForSendingBattlegroundsSkinsSeenPacket();
		return true;
	}

	public bool TryAddSeenBattlegroundsBoardSkin(BattlegroundsBoardSkinId skinId)
	{
		NetCache.NetCacheBattlegroundsBoardSkins netCacheBattlegroundsBoardSkins = NetCache.Get().GetNetObject<NetCache.NetCacheBattlegroundsBoardSkins>();
		if (netCacheBattlegroundsBoardSkins == null || !netCacheBattlegroundsBoardSkins.UnseenSkinIds.Remove(skinId))
		{
			return false;
		}
		m_ackBattlegroundsSkinsSeenPacket.BoardSkins.Add(skinId.ToValue());
		CheckForSendingBattlegroundsSkinsSeenPacket();
		return true;
	}

	public bool TryAddSeenBattlegroundsFinisher(BattlegroundsFinisherId finisherId)
	{
		NetCache.NetCacheBattlegroundsFinishers netCacheBattlegroundsFinishers = NetCache.Get().GetNetObject<NetCache.NetCacheBattlegroundsFinishers>();
		if (netCacheBattlegroundsFinishers == null || !netCacheBattlegroundsFinishers.UnseenSkinIds.Remove(finisherId))
		{
			return false;
		}
		m_ackBattlegroundsSkinsSeenPacket.Finishers.Add(finisherId.ToValue());
		CheckForSendingBattlegroundsSkinsSeenPacket();
		return true;
	}

	public bool TryAddSeenBattlegroundsEmote(BattlegroundsEmoteId emoteId)
	{
		NetCache.NetCacheBattlegroundsEmotes netCacheBattlegroundsEmotes = NetCache.Get().GetNetObject<NetCache.NetCacheBattlegroundsEmotes>();
		if (netCacheBattlegroundsEmotes == null || !netCacheBattlegroundsEmotes.UnseenEmoteIds.Remove(emoteId))
		{
			return false;
		}
		m_ackBattlegroundsSkinsSeenPacket.Emotes.Add(emoteId.ToValue());
		CheckForSendingBattlegroundsSkinsSeenPacket();
		return true;
	}

	public void CheckForSendingBattlegroundsSkinsSeenPacket(int minToSend = 16)
	{
		if (m_ackBattlegroundsSkinsSeenPacket.HeroSkins.Count + m_ackBattlegroundsSkinsSeenPacket.GuideSkins.Count + m_ackBattlegroundsSkinsSeenPacket.BoardSkins.Count + m_ackBattlegroundsSkinsSeenPacket.Finishers.Count + m_ackBattlegroundsSkinsSeenPacket.Emotes.Count >= minToSend && IsLoggedIn())
		{
			m_connectApi.SendAckBattlegroundSkinsSeenPacket(m_ackBattlegroundsSkinsSeenPacket);
			m_ackBattlegroundsSkinsSeenPacket.HeroSkins.Clear();
			m_ackBattlegroundsSkinsSeenPacket.GuideSkins.Clear();
			m_ackBattlegroundsSkinsSeenPacket.BoardSkins.Clear();
			m_ackBattlegroundsSkinsSeenPacket.Finishers.Clear();
			m_ackBattlegroundsSkinsSeenPacket.Emotes.Clear();
		}
	}

	public BattlegroundsBoardSkinsResponse GetBattlegroundsBoardSkinsResponse()
	{
		if (!ShouldBeConnectedToAurora())
		{
			return new BattlegroundsBoardSkinsResponse();
		}
		return m_connectApi.GetBattlegroundsBoardSkinsResponse();
	}

	public SetBattlegroundsFavoriteBoardSkinResponse GetSetBattlegroundsFavoriteBoardSkinResponse()
	{
		return m_connectApi.GetSetBattlegroundsFavoriteBoardSkinResponse();
	}

	public void SetBattlegroundsFavoriteBoardSkin(BattlegroundsBoardSkinId bgFavoriteBoardSkinID)
	{
		if (IsLoggedIn())
		{
			m_connectApi.SetBattlegroundsFavoriteBoardSkin(bgFavoriteBoardSkinID.ToValue());
		}
		else
		{
			NetCache.Get().GetNetObject<NetCache.NetCacheBattlegroundsBoardSkins>()?.BattlegroundsFavoriteBoardSkins.Add(bgFavoriteBoardSkinID);
		}
	}

	public void ClearBattlegroundsFavoriteBoardSkin(BattlegroundsBoardSkinId bgFavoriteBoardSkinID)
	{
		if (IsLoggedIn())
		{
			m_connectApi.ClearBattlegroundsFavoriteBoardSkin(bgFavoriteBoardSkinID.ToValue());
		}
		else
		{
			NetCache.Get().GetNetObject<NetCache.NetCacheBattlegroundsBoardSkins>()?.BattlegroundsFavoriteBoardSkins.Remove(bgFavoriteBoardSkinID);
		}
	}

	public ClearBattlegroundsFavoriteBoardSkinResponse GetClearBattlegroundsFavoriteBoardSkinResponse()
	{
		return m_connectApi.GetClearBattlegroundsFavoriteBoardSkinResponse();
	}

	public BattlegroundsFinishersResponse GetBattlegroundsFinishersResponse()
	{
		if (!ShouldBeConnectedToAurora())
		{
			return new BattlegroundsFinishersResponse();
		}
		return m_connectApi.GetBattlegroundsFinishersResponse();
	}

	public SetBattlegroundsFavoriteFinisherResponse GetSetBattlegroundsFavoriteFinisherResponse()
	{
		return m_connectApi.GetSetBattlegroundsFavoriteFinisherResponse();
	}

	public void SetBattlegroundsFavoriteFinisher(BattlegroundsFinisherId bgFavoriteFinisherID)
	{
		if (IsLoggedIn())
		{
			m_connectApi.SetBattlegroundsFavoriteFinisher(bgFavoriteFinisherID.ToValue());
		}
		else
		{
			NetCache.Get().GetNetObject<NetCache.NetCacheBattlegroundsFinishers>()?.BattlegroundsFavoriteFinishers.Add(bgFavoriteFinisherID);
		}
	}

	public void ClearBattlegroundsFavoriteFinisher(BattlegroundsFinisherId bgFavoriteFinisherID)
	{
		if (IsLoggedIn())
		{
			m_connectApi.ClearBattlegroundsFavoriteFinisher(bgFavoriteFinisherID);
		}
		else
		{
			NetCache.Get().GetNetObject<NetCache.NetCacheBattlegroundsFinishers>()?.BattlegroundsFavoriteFinishers.Remove(bgFavoriteFinisherID);
		}
	}

	public ClearBattlegroundsFavoriteFinisherResponse GetClearBattlegroundsFavoriteFinisherResponse()
	{
		return m_connectApi.GetClearBattlegroundsFavoriteFinisherResponse();
	}

	public BattlegroundsEmotesResponse GetBattlegroundsEmotesResponse()
	{
		if (!ShouldBeConnectedToAurora())
		{
			return new BattlegroundsEmotesResponse();
		}
		return m_connectApi.GetBattlegroundsEmotesResponse();
	}

	public SetBattlegroundsEmoteLoadoutResponse GetSetBattlegroundsEmoteLoadoutResponse()
	{
		return m_connectApi.GetSetBattlegroundsEmoteLoadoutResponse();
	}

	public void SetBattlegroundsEmoteLoadout(Hearthstone.BattlegroundsEmoteLoadout loadout)
	{
		for (int i = 0; i < loadout.Emotes.Length; i++)
		{
		}
		if (IsLoggedIn())
		{
			m_connectApi.SetBattlegroundsEmoteLoadout(loadout);
			return;
		}
		NetCache.NetCacheBattlegroundsEmotes netCacheBGEmotes = NetCache.Get().GetNetObject<NetCache.NetCacheBattlegroundsEmotes>();
		if (netCacheBGEmotes != null)
		{
			netCacheBGEmotes.CurrentLoadout = loadout;
		}
	}

	public LettuceMapResponse GetLettuceMapResponse()
	{
		return m_connectApi.GetLettuceMapResponse();
	}

	public LettuceMapChooseNodeResponse GetLettuceMapChooseNodeResponse()
	{
		return m_connectApi.GetLettuceMapChooseNodeResponse();
	}

	public LettuceMapRetireResponse GetLettuceMapRetireResponse()
	{
		return m_connectApi.GetLettuceMapRetireResponse();
	}

	public MercenariesMapTreasureSelectionResponse GetMercenariesMapTreasureSelectionResponse()
	{
		return m_connectApi.GetMercenariesMapTreasureSelectionResponse();
	}

	public MercenariesMapVisitorSelectionResponse GetMercenariesMapVisitorSelectionResponse()
	{
		return m_connectApi.GetMercenariesMapVisitorSelectionResponse();
	}

	public void RequestGameSaveData(List<long> keys, int clientToken)
	{
		m_connectApi.RequestGameSaveData(keys, clientToken);
	}

	public GameSaveDataResponse GetGameSaveDataResponse()
	{
		return m_connectApi.GetGameSaveDataResponse();
	}

	public void SetGameSaveData(List<GameSaveDataUpdate> dataUpdates, int clientToken)
	{
		m_connectApi.SetGameSaveData(dataUpdates, clientToken);
	}

	public SetGameSaveDataResponse GetSetGameSaveDataResponse()
	{
		return m_connectApi.GetSetGameSaveDataResponse();
	}

	public GameSaveDataStateUpdate GetGameSaveDataStateUpdate()
	{
		return m_connectApi.GetGameSaveDataStateUpdate();
	}

	public CardSaleResult GetCardSaleResult()
	{
		BoughtSoldCard packet = m_connectApi.GetCardSaleResult();
		if (packet == null)
		{
			return null;
		}
		CardSaleResult result = new CardSaleResult
		{
			AssetID = packet.Def.Asset,
			AssetName = GameUtils.TranslateDbIdToCardId(packet.Def.Asset),
			Premium = (packet.Def.HasPremium ? ((TAG_PREMIUM)packet.Def.Premium) : TAG_PREMIUM.NORMAL),
			Action = (CardSaleResult.SaleResult)packet.Result_
		};
		if (packet.HasCollectionVersion)
		{
			NetCache.Get().AddExpectedCollectionModification(packet.CollectionVersion);
		}
		return result;
	}

	public void RequestCollectionClientState()
	{
		OfflineDataCache.OfflineData offlineData = OfflineDataCache.ReadOfflineDataFromFile();
		m_connectApi.RequestCollectionClientState(GetPlatformBuilder(), OfflineDataCache.GetCachedCollectionVersion(offlineData), OfflineDataCache.GetCachedDeckContentsTimes(offlineData), OfflineDataCache.GetCachedCollectionVersionLastModified(offlineData));
		TelemetryManager.Client().SendInitialDataRequested(GetVersion(), GameDbf.GetDataVersion());
	}

	public void RequestSecondaryClientState()
	{
		OfflineDataCache.OfflineData offlineData = OfflineDataCache.ReadOfflineDataFromFile();
		m_connectApi.RequestSecondaryClientState(GetPlatformBuilder(), OfflineDataCache.GetCachedDeckContentsTimes(offlineData));
	}

	public void LoginOk()
	{
		m_connectApi.OnLoginComplete();
	}

	public GetAssetResponse GetAssetResponse()
	{
		return m_connectApi.GetAssetResponse();
	}

	public void SendAssetRequest(int clientToken, List<AssetKey> requestKeys)
	{
		if (requestKeys != null && requestKeys.Count != 0)
		{
			m_connectApi.SendAssetRequest(clientToken, requestKeys);
		}
	}

	private Platform GetPlatformBuilder()
	{
		Platform platform = new Platform
		{
			Os = (int)PlatformSettings.OS,
			Screen = (int)PlatformSettings.Screen,
			Name = PlatformSettings.DeviceName,
			UniqueDeviceIdentifier = SystemInfo.deviceUniqueIdentifier
		};
		AndroidStore store = AndroidDeviceSettings.Get().GetAndroidStore();
		if (store != 0)
		{
			platform.Store = (int)store;
		}
		return platform;
	}

	public bool SendDebugConsoleCommand(string command)
	{
		if (!IsConnectedToGameServer())
		{
			Log.Net.Print($"Cannot send command '{command}' to server; no game server is active.");
			return false;
		}
		if (m_connectApi.AllowDebugConnections() && command != null)
		{
			m_connectApi.SendDebugConsoleCommand(command);
		}
		return true;
	}

	public void SendDebugConsoleResponse(int responseType, string message)
	{
		m_connectApi.SendDebugConsoleResponse(responseType, message);
	}

	public string GetDebugConsoleCommand()
	{
		DebugConsoleCommand packet = m_connectApi.GetDebugConsoleCommand();
		if (packet == null)
		{
			return string.Empty;
		}
		return packet.Command;
	}

	public DebugConsoleResponse GetDebugConsoleResponse()
	{
		BobNetProto.DebugConsoleResponse packet = m_connectApi.GetDebugConsoleResponse();
		if (packet == null)
		{
			return null;
		}
		return new DebugConsoleResponse
		{
			Type = (int)packet.ResponseType_,
			Response = packet.Response
		};
	}

	public void SendDebugCommandRequest(DebugCommandRequest packet)
	{
		m_connectApi.SendDebugCommandRequest(packet);
	}

	public DebugCommandResponse GetDebugCommandResponse()
	{
		return m_connectApi.GetDebugCommandResponse();
	}

	public void SendLocateCheatServerRequest()
	{
		m_connectApi.SendLocateCheatServerRequest();
	}

	public LocateCheatServerResponse GetLocateCheatServerResponse()
	{
		return m_connectApi.GetLocateCheatServerResponse();
	}

	public GameToConnectNotification GetGameToConnectNotification()
	{
		return m_connectApi.GetGameToConnectNotification();
	}

	public void GetServerTimeRequest()
	{
		m_connectApi.GetServerTimeRequest((long)TimeUtils.DateTimeToUnixTimeStamp(DateTime.Now));
	}

	public void ReportBlizzardCheckoutStatus(BlizzardCheckoutStatus status, TransactionData data = null)
	{
		m_connectApi.ReportBlizzardCheckoutStatus(status, data, (long)TimeUtils.DateTimeToUnixTimeStamp(DateTime.Now));
	}

	public ResponseWithRequest<GetServerTimeResponse, GetServerTimeRequest> GetServerTimeResponse()
	{
		return m_connectApi.GetServerTimeResponse();
	}

	public void SimulateUncleanDisconnectFromGameServer()
	{
		if (m_connectApi.IsConnectedToGameServer())
		{
			m_connectApi.DisconnectFromGameServer();
		}
	}

	public void SimulateReceivedPacketFromServer(PegasusPacket packet)
	{
		m_dispatcherImpl.NotifyUtilResponseReceived(packet);
	}

	public void ReportTokenFetchFailure()
	{
		ShowConnectionFailureError("GLOBAL_ERROR_NETWORK_LOGIN_FAILURE");
		ReconnectMgr.Get().FullResetRequired = true;
	}

	public string GetNetworkGameStateString()
	{
		return "";
	}

	public string GetNetworkGameStateStringForErrors()
	{
		return "*** Network Game Connection State ***  \n            " + StatePrinter.GetMemebersForDebugging(ReconnectMgr.Get()) + " \n            " + StatePrinter.GetMemebersForDebugging(GameMgr.Get()) + "\n            " + StatePrinter.GetMemebersForDebugging(Get()) + "\n            " + StatePrinter.GetMemebersForDebugging(m_dispatcherImpl) + "\n            ***************************";
	}

	public void GotoGameServer(GameServerInfo info, bool reconnecting)
	{
		if (info == null)
		{
			GameNetLogger.Log(Blizzard.T5.Core.LogLevel.Warning, "Network.GotoGameServe() - GameServerInfo is null. The game doesn't exist anymore?");
			return;
		}
		m_state.LastGameServerInfo = info;
		bool gameConnectionDisconnected = m_dispatcherImpl.GameConnectionState == Blizzard.T5.Network.ConnectionStatus.Disconnected;
		bool isRestoringGameState = ReconnectMgr.Get().IsRestoringGameStateFromDatabase();
		GameNetLogger.Log(Blizzard.T5.Core.LogLevel.Information, $"Network.GotoGameServe() - gameConnectionDisconnected {gameConnectionDisconnected}, IsRestoringGameState: {isRestoringGameState}");
		if (!gameConnectionDisconnected && !isRestoringGameState)
		{
			string message = $"Network.GotoGameServe() - was called when we're already waiting for a game to start. gameConnectionDisconnected {gameConnectionDisconnected}, IsRestoringGameState: {isRestoringGameState}";
			GameNetLogger.Log(Blizzard.T5.Core.LogLevel.Error, GetNetworkGameStateStringForErrors());
			Error.AddDevFatal(message);
			return;
		}
		string address = info.Address;
		uint port = Vars.Key("Application.GameServerPortOverride").GetUInt(info.Port);
		GameNetLogger.Log(Blizzard.T5.Core.LogLevel.Information, "Network.GotoGameServe() - address= " + address + ":" + port + ", game=" + info.GameHandle + ", client=" + info.ClientHandle + ", spectateKey=" + info.SpectatorPassword + " reconnecting=" + reconnecting);
		if (address != null)
		{
			if (string.IsNullOrEmpty(address) || port == 0 || (info.GameHandle == 0 && ShouldBeConnectedToAurora()))
			{
				GameNetLogger.Log(Blizzard.T5.Core.LogLevel.Warning, "Network.GotoGameServe() - ERROR in ServerInfo address= " + address + ":" + port + ",    game=" + info.GameHandle + ", client=" + info.ClientHandle + " reconnecting=" + reconnecting);
			}
			m_gameServerKeepAliveFrequencySeconds = 0u;
			m_gameServerKeepAliveRetry = 3u;
			m_gameConceded = false;
			m_disconnectRequested = false;
			m_connectApi.SetTimeLastPingSent(0.0);
			m_connectApi.SetTimeLastPingReceived(0.0);
			m_connectApi.SetPingsSinceLastPong(0);
			if (GameServerDisconnectEvents != null)
			{
				GameServerDisconnectEvents.Clear();
			}
			m_state.LastConnectToGameServerInfo = new ConnectToGameServer();
			m_state.LastConnectToGameServerInfo.TimeSpentMilliseconds = (long)TimeUtils.GetElapsedTimeSinceEpoch(null).TotalMilliseconds;
			m_state.LastConnectToGameServerInfo.GameSessionInfo = new Blizzard.Telemetry.WTCG.Client.GameSessionInfo();
			m_state.LastConnectToGameServerInfo.GameSessionInfo.GameServerIpAddress = info.Address;
			m_state.LastConnectToGameServerInfo.GameSessionInfo.GameServerPort = info.Port;
			m_state.LastConnectToGameServerInfo.GameSessionInfo.Version = info.Version;
			m_state.LastConnectToGameServerInfo.GameSessionInfo.GameHandle = info.GameHandle;
			m_state.LastConnectToGameServerInfo.GameSessionInfo.ScenarioId = GameMgr.Get().GetNextMissionId();
			m_state.LastConnectToGameServerInfo.GameSessionInfo.GameType = (Blizzard.Telemetry.WTCG.Client.GameType)GameMgr.Get().GetNextGameType();
			m_state.LastConnectToGameServerInfo.GameSessionInfo.FormatType = (Blizzard.Telemetry.WTCG.Client.FormatType)GameMgr.Get().GetNextFormatType();
			m_state.LastConnectToGameServerInfo.GameSessionInfo.IsReconnect = GameMgr.Get().IsNextReconnect();
			m_state.LastConnectToGameServerInfo.GameSessionInfo.IsSpectating = GameMgr.Get().IsNextSpectator();
			m_state.LastConnectToGameServerInfo.GameSessionInfo.ClientHandle = info.ClientHandle;
			if (GameMgr.Get().LastDeckId.HasValue)
			{
				m_state.LastConnectToGameServerInfo.GameSessionInfo.ClientDeckId = GameMgr.Get().LastDeckId.Value;
			}
			if (GameMgr.Get().LastHeroCardDbId.HasValue)
			{
				m_state.LastConnectToGameServerInfo.GameSessionInfo.ClientHeroCardId = GameMgr.Get().LastHeroCardDbId.Value;
			}
			m_connectApi.SetGameStartState((!reconnecting) ? GameStartState.InitialStart : GameStartState.Reconnecting);
			m_connectApi.GotoGameServer(address, port);
		}
	}

	public void SpectateSecondPlayer(GameServerInfo info)
	{
		info.SpectatorMode = true;
		if (!IsConnectedToGameServer())
		{
			GotoGameServer(info, reconnecting: false);
		}
		else
		{
			SendGameServerHandshake(info);
		}
	}

	public bool RetryGotoGameServer()
	{
		if (m_connectApi.GetGameStartState() == GameStartState.Invalid)
		{
			return false;
		}
		SendGameServerHandshake(m_state.LastGameServerInfo);
		return true;
	}

	public void DisconnectFromGameServer(DisconnectReason reason)
	{
		string trace = "";
		if (HearthstoneApplication.IsInternal())
		{
			trace = ", stacktrace:" + System.Environment.StackTrace;
		}
		GameNetLogger.Log(Blizzard.T5.Core.LogLevel.Information, $"Network.DisconnectFromGameServer() - Reason: {reason} {trace}");
		m_connectApi.SetGameStartState(GameStartState.Invalid);
		if (IsConnectedToGameServer())
		{
			m_disconnectRequested = true;
			m_connectApi.DisconnectFromGameServer();
		}
	}

	private void GoToNoAccountTutorialServer(int scenario)
	{
		GameServerInfo gameServer = new GameServerInfo();
		gameServer.Version = BattleNet.GetVersion();
		if (Vars.Key("GameServerOverride.Active").GetBool(def: false))
		{
			gameServer.Address = Vars.Key("GameServerOverride.Address").GetStr("");
			gameServer.Port = Vars.Key("GameServerOverride.Port").GetUInt(0u);
			gameServer.AuroraPassword = "";
		}
		else
		{
			BnetRegion region = DeviceLocaleHelper.GetCurrentRegionId();
			if (HearthstoneApplication.GetMobileEnvironment() == MobileEnv.PRODUCTION)
			{
				string regionString;
				try
				{
					regionString = RegionToTutorialName[region];
				}
				catch (KeyNotFoundException)
				{
					GameNetLogger.Log(Blizzard.T5.Core.LogLevel.Warning, "Network.GoToNoAccountTutorialServer() - No matching tutorial server name found for region " + region);
					regionString = "us";
				}
				gameServer.Address = string.Format(regionString, TutorialServer);
				gameServer.Port = 1119u;
			}
			else
			{
				DeviceLocaleData.ConnectionData connData = DeviceLocaleHelper.GetConnectionDataFromRegionId(region, isDev: true);
				gameServer.Port = connData.tutorialPort;
				gameServer.Address = (string.IsNullOrEmpty(connData.gameServerAddress) ? "10.130.126.28" : connData.gameServerAddress);
				gameServer.Version = connData.version;
			}
			GameNetLogger.Log(Blizzard.T5.Core.LogLevel.Information, $"Network.GoToNoAccountTutorialServer() - Connecting to account-free tutorial server for region {region}.  Address: {gameServer.Address}  Port: {gameServer.Port}  Version: {gameServer.Version}");
			gameServer.AuroraPassword = "";
		}
		gameServer.GameHandle = 0u;
		gameServer.ClientHandle = 0L;
		gameServer.Mission = scenario;
		gameServer.BrawlLibraryItemId = 0;
		ResolveAddressAndGotoGameServer(gameServer);
	}

	private void ResolveAddressAndGotoGameServer(GameServerInfo gameServer)
	{
		if (IPAddress.TryParse(gameServer.Address, out var ipAddress))
		{
			gameServer.Address = ipAddress.ToString();
			Get().GotoGameServer(gameServer, reconnecting: false);
			return;
		}
		try
		{
			IPHostEntry host = Dns.GetHostEntry(gameServer.Address);
			if (host.AddressList.Length != 0)
			{
				IPAddress ip = host.AddressList[0];
				gameServer.Address = ip.ToString();
				Get().GotoGameServer(gameServer, reconnecting: false);
				return;
			}
		}
		catch (Exception ex)
		{
			m_state.LogSource.LogError("Exception within ResolveAddressAndGotoGameServer: " + ex.Message);
			GameNetLogger.Log(Blizzard.T5.Core.LogLevel.Error, "Network.ResolveAddressAndGotoGameServer() - Exception: " + ex.Message);
			GameNetLogger.Log(Blizzard.T5.Core.LogLevel.Error, Get().GetNetworkGameStateStringForErrors());
		}
		ThrowDnsResolveError(gameServer.Address);
	}

	public bool WasDisconnectRequested()
	{
		return m_disconnectRequested;
	}

	public bool IsConnectedToGameServer()
	{
		return m_connectApi.IsConnectedToGameServer();
	}

	public Blizzard.T5.Network.ConnectionStatus GetGameServerConnectionState()
	{
		return m_dispatcherImpl.GameConnectionState;
	}

	public bool GameServerHasDisconnectEvents()
	{
		return GameServerDisconnectEvents.Count > 0;
	}

	public bool WasGameConceded()
	{
		return m_gameConceded;
	}

	public void Concede()
	{
		GameNetLogger.Log(Blizzard.T5.Core.LogLevel.Information, "Player conceded the game");
		m_gameConceded = true;
		m_connectApi.Concede();
	}

	public void AutoConcede()
	{
		if (IsConnectedToGameServer() && !WasGameConceded())
		{
			Concede();
		}
	}

	private void OnGameServerConnectEvent(BattleNetErrors error, SocketOperationResult socketResult)
	{
		GameNetLogger.Log(Blizzard.T5.Core.LogLevel.Information, "Network.OnGameServerConnectEvent() - Connected to game server with error code " + error);
		GameNetLogger.Log(Blizzard.T5.Core.LogLevel.Information, $"Network.OnGameServerConnectEvent() - Connected to game server with socket error: {socketResult.Error}, Message: {socketResult.Message}");
		if (m_state.LastConnectToGameServerInfo != null)
		{
			long timeSpentMilliseconds = (long)(TimeUtils.GetElapsedTimeSinceEpoch(null).TotalMilliseconds - (double)m_state.LastConnectToGameServerInfo.TimeSpentMilliseconds);
			m_state.LastConnectToGameServerInfo.ResultBnetCode = (uint)error;
			m_state.LastConnectToGameServerInfo.ResultBnetCodeString = error.ToString();
			m_state.LastConnectToGameServerInfo.TimeSpentMilliseconds = timeSpentMilliseconds;
			TelemetryManager.Client().SendConnectToGameServer(m_state.LastConnectToGameServerInfo);
			m_state.LastConnectToGameServerInfo = null;
		}
		GameServerInfo gameServerInfo = GetLastGameServerJoined();
		if (error == BattleNetErrors.ERROR_OK)
		{
			SendGameServerHandshake(m_state.LastGameServerInfo);
			TelemetryManager.Client().SendConnectSuccess("GAME", gameServerInfo?.Address, gameServerInfo?.Port);
			TelemetryManager.RegisterShutdownListener(SendDefaultDisconnectTelemetry);
			return;
		}
		TelemetryManager.Client().SendConnectFail("GAME", error.ToString(), gameServerInfo?.Address, gameServerInfo?.Port);
		GameStartState prevGameStartState = m_connectApi.GetGameStartState();
		m_connectApi.SetGameStartState(GameStartState.Invalid);
		if (ShouldBeConnectedToAurora())
		{
			if (prevGameStartState != GameStartState.Reconnecting)
			{
				if (error == BattleNetErrors.ERROR_RPC_PEER_UNKNOWN && NetworkReachabilityManager.OnCellular)
				{
					ShowBreakingNewsOrError("GLOBAL_MOBILE_ERROR_GAMESERVER_CONNECT");
				}
				else
				{
					ShowBreakingNewsOrError("GLOBAL_ERROR_NETWORK_NO_GAME_SERVER");
				}
				GameNetLogger.Log(Blizzard.T5.Core.LogLevel.Error, "Network.OnGameServerConnectEvent() - Failed to connect to game server with error " + error);
			}
		}
		else
		{
			ShowBreakingNewsOrError("GLOBAL_ERROR_NETWORK_NO_GAME_SERVER");
			GameNetLogger.Log(Blizzard.T5.Core.LogLevel.Error, "Network.OnGameServerConnectEvent() - Failed to connect to game server with error " + error);
		}
	}

	private void OnGameServerDisconnectEvent(BattleNetErrors error, SocketOperationResult socketResult)
	{
		GameNetLogger.Log(Blizzard.T5.Core.LogLevel.Information, $"Network.OnGameServerDisconnectEvent() - Disconnected from game server with BNet error: {error.ToString()} socket error: {socketResult.Error}, Message: {socketResult.Message}");
		TelemetryManager.UnregisterShutdownListener(SendDefaultDisconnectTelemetry);
		GameServerInfo gameServerInfo = GetLastGameServerJoined();
		TelemetryManager.Client().SendDisconnect("GAME", TelemetryUtil.GetReasonFromBnetError(error), (error == BattleNetErrors.ERROR_OK) ? null : error.ToString(), gameServerInfo?.Address, gameServerInfo?.Port);
		m_state.LastConnectToGameServerInfo = null;
		bool handled = false;
		if (error != 0)
		{
			switch (m_connectApi.GetGameStartState())
			{
			case GameStartState.Reconnecting:
				handled = true;
				break;
			case GameStartState.InitialStart:
				if (gameServerInfo == null || !gameServerInfo.SpectatorMode)
				{
					GameNetLogger.Log(Blizzard.T5.Core.LogLevel.Error, "Network.OnGameServerDisconnectEvent() - Disconnected from game server with error " + error);
					GameNetLogger.Log(Blizzard.T5.Core.LogLevel.Debug, Get().GetNetworkGameStateString());
					ConnectErrorParams parms = new ConnectErrorParams();
					parms.m_message = GameStrings.Format("GLOBAL_ERROR_NETWORK_DISCONNECT_GAME_SERVER");
					AddErrorToList(parms);
					handled = true;
				}
				break;
			}
		}
		if (!handled)
		{
			AddGameServerDisconnectEvent(error);
		}
	}

	private void OnIPv6ConversionEvent(string ipv6, string ipv4)
	{
		GameServerIPv6 = ipv6;
		GameServerIPv4 = ipv4;
		IsOSSupportIPv6 = Socket.OSSupportsIPv6;
	}

	public void OnFatalBnetError(BnetErrorInfo info)
	{
		BattleNetErrors error = info.GetError();
		m_state.LogSource.LogError("Network.OnFatalBnetError() - error={0}", info);
		GameNetLogger.Log(Blizzard.T5.Core.LogLevel.Error, "Network.OnFatalBnetError() - error={0}", info);
		ReportErrorToTelemetry(error);
		switch (error)
		{
		case BattleNetErrors.ERROR_DENIED:
			ShowConnectionFailureError("GLOBAL_ERROR_NETWORK_LOGIN_FAILURE");
			break;
		case BattleNetErrors.ERROR_RPC_QUOTA_EXCEEDED:
		{
			string errorMsg = "GLOBAL_ERROR_NETWORK_SPAM";
			Error.AddFatal(FatalErrorReason.BNET_NETWORK_SPAM, errorMsg);
			break;
		}
		case BattleNetErrors.ERROR_RPC_PEER_DISCONNECTED:
		{
			string errorMsg = "GLOBAL_ERROR_NETWORK_DISCONNECT";
			ShowConnectionFailureError(errorMsg);
			break;
		}
		case BattleNetErrors.ERROR_RPC_REQUEST_TIMED_OUT:
		{
			string errorMsg = "GLOBAL_ERROR_NETWORK_UTIL_TIMEOUT";
			ShowConnectionFailureError(errorMsg);
			break;
		}
		case BattleNetErrors.ERROR_SESSION_DUPLICATE:
			Error.AddFatal(FatalErrorReason.LOGIN_FROM_ANOTHER_DEVICE, "GLOBAL_ERROR_NETWORK_DUPLICATE_LOGIN");
			break;
		case BattleNetErrors.ERROR_SESSION_DISCONNECTED:
		{
			string errorMsg = "GLOBAL_ERROR_NETWORK_DISCONNECT";
			Error.AddFatal(FatalErrorReason.BNET_NETWORK_DISCONNECT, errorMsg);
			break;
		}
		case BattleNetErrors.ERROR_ADMIN_KICK:
		case BattleNetErrors.ERROR_SESSION_ADMIN_KICK:
			Error.AddFatal(FatalErrorReason.ADMIN_KICK_OR_BAN, "GLOBAL_ERROR_NETWORK_ADMIN_KICKED");
			break;
		case BattleNetErrors.ERROR_GAME_ACCOUNT_SUSPENDED:
			ServiceManager.Get<ILoginService>()?.ClearAuthentication();
			Error.AddFatal(FatalErrorReason.ADMIN_KICK_OR_BAN, "GLOBAL_ERROR_NETWORK_ACCOUNT_SUSPENDED");
			break;
		case BattleNetErrors.ERROR_GAME_ACCOUNT_BANNED:
		case BattleNetErrors.ERROR_BATTLENET_ACCOUNT_BANNED:
			ServiceManager.Get<ILoginService>()?.ClearAuthentication();
			Error.AddFatal(FatalErrorReason.ADMIN_KICK_OR_BAN, "GLOBAL_ERROR_NETWORK_ACCOUNT_BANNED");
			break;
		case BattleNetErrors.ERROR_PARENTAL_CONTROL_RESTRICTION:
			ServiceManager.Get<ILoginService>()?.ClearAuthentication();
			Error.AddFatal(FatalErrorReason.ADMIN_KICK_OR_BAN, "GLOBAL_ERROR_NETWORK_PARENTAL_CONTROLS");
			break;
		case BattleNetErrors.ERROR_PHONE_LOCK:
			Error.AddFatal(FatalErrorReason.BNET_PHONE_LOCK, "GLOBAL_ERROR_NETWORK_PHONE_LOCK");
			break;
		case BattleNetErrors.ERROR_LOGON_WEB_VERIFY_TIMEOUT:
			ShowConnectionFailureError("GLOBAL_MOBILE_ERROR_LOGON_WEB_TIMEOUT");
			break;
		case BattleNetErrors.ERROR_SERVER_IS_PRIVATE:
			TelemetryManager.Client().SendNetworkError(NetworkError.ErrorType.PRIVATE_SERVER, info.ToString(), 33);
			ShowConnectionFailureError("GLOBAL_ERROR_UNKNOWN_ERROR");
			Log.Net.PrintWarning("ERROR_SERVER_IS_PRIVATE - {0} connection failures.", m_numConnectionFailures);
			GameNetLogger.Log(Blizzard.T5.Core.LogLevel.Warning, "Network.OnFatalBnetError() - ERROR_SERVER_IS_PRIVATE - {0} connection failures.", m_numConnectionFailures);
			break;
		case BattleNetErrors.ERROR_RPC_PEER_UNAVAILABLE:
			TelemetryManager.Client().SendNetworkError(NetworkError.ErrorType.PEER_UNAVAILABLE, info.ToString(), 3004);
			ShowConnectionFailureError("GLOBAL_ERROR_UNKNOWN_ERROR");
			Log.Net.PrintWarning("ERROR_RPC_PEER_UNAVAILABLE - {0} connection failures.", m_numConnectionFailures);
			GameNetLogger.Log(Blizzard.T5.Core.LogLevel.Warning, "Network.OnFatalBnetError() - ERROR_RPC_PEER_UNAVAILABLE - {0} connection failures.", m_numConnectionFailures);
			break;
		case BattleNetErrors.ERROR_BAD_VERSION:
			if (PlatformSettings.IsMobile() && GameDownloadManagerProvider.Get() != null && !GameDownloadManagerProvider.Get().IsNewMobileVersionReleased)
			{
				Error.AddFatal(FatalErrorReason.UNAVAILABLE_NEW_VERSION, "GLOBAL_ERROR_NETWORK_UNAVAILABLE_NEW_VERSION");
			}
			else
			{
				Error.AddFatal(new ErrorParams
				{
					m_message = GameStrings.Format("GLOBAL_ERROR_NETWORK_UNAVAILABLE_UPGRADE"),
					m_redirectToStore = Error.HAS_APP_STORE,
					m_reason = FatalErrorReason.UNAVAILABLE_UPGRADE
				});
			}
			ReconnectMgr.Get().FullResetRequired = true;
			ReconnectMgr.Get().UpdateRequired = true;
			break;
		case BattleNetErrors.ERROR_SESSION_CAIS_PLAYTIME_EXCEEDED:
			ServiceManager.Get<ILoginService>()?.ClearAuthentication();
			Error.AddFatal(FatalErrorReason.ADMIN_KICK_OR_BAN, "GLOBAL_ERROR_NETWORK_ACCOUNT_PLAYTIME_EXCEEDED");
			break;
		case BattleNetErrors.ERROR_SESSION_CAIS_CURFEW:
			ServiceManager.Get<ILoginService>()?.ClearAuthentication();
			Error.AddFatal(FatalErrorReason.ADMIN_KICK_OR_BAN, "GLOBAL_ERROR_NETWORK_ACCOUNT_CURFEW_REACHED");
			break;
		case BattleNetErrors.ERROR_SESSION_INVALID_NID:
			ServiceManager.Get<ILoginService>()?.ClearAuthentication();
			Error.AddFatal(FatalErrorReason.ADMIN_KICK_OR_BAN, "GLOBAL_ERROR_NETWORK_ACCOUNT_INVALID_NID");
			break;
		default:
		{
			string message;
			if (HearthstoneApplication.IsInternal())
			{
				message = $"Unhandled Bnet Error: {info}";
			}
			else
			{
				Debug.LogError($"Unhandled Bnet Error: {info}");
				GameNetLogger.Log(Blizzard.T5.Core.LogLevel.Error, $"Network.OnFatalBnetError() - Unhandled Bnet Error: {info}");
				message = GameStrings.Format("GLOBAL_ERROR_UNKNOWN_ERROR");
			}
			ReportUnknownNetErrorToTelem(info);
			ShowConnectionFailureError(message);
			break;
		}
		}
	}

	private void RegisterConnectApiConnectionListeners()
	{
		m_connectApi.RegisterGameServerConnectEventListener(OnGameServerConnectEvent);
		m_connectApi.RegisterGameServerDisconnectEventListener(OnGameServerDisconnectEvent);
		m_connectApi.RegisterIPv6ConversionEventListener(OnIPv6ConversionEvent);
	}

	private void RemoveConnectApiConnectionListeners()
	{
		m_connectApi.RemoveGameServerConnectEventListener(OnGameServerConnectEvent);
		m_connectApi.RemoveGameServerDisconnectEventListener(OnGameServerDisconnectEvent);
		m_connectApi.RemoveIPv6ConversionEventListener(OnIPv6ConversionEvent);
	}

	private void AddGameServerDisconnectEvent(BattleNetErrors error)
	{
		GameNetLogger.Log(Blizzard.T5.Core.LogLevel.Debug, "Network.AddGameServerDisconnectEvent() - error: " + error);
		if (GameServerDisconnectEvents == null)
		{
			GameServerDisconnectEvents = new List<BattleNetErrors>();
		}
		GameServerDisconnectEvents.Add(error);
	}

	private void ProcessGameServerDisconnectEvents()
	{
		if (GameServerDisconnectEvents == null || GameServerDisconnectEvents.Count <= 0)
		{
			return;
		}
		BattleNetErrors[] errors = GameServerDisconnectEvents.ToArray();
		for (int i = 0; i < errors.Length; i++)
		{
			try
			{
				if (Gameplay.Get() != null)
				{
					Gameplay.Get().OnDisconnect(errors[i]);
				}
				else
				{
					GameMgr.Get().OnGameCanceled();
				}
			}
			catch (Exception ex)
			{
				GameNetLogger.Log(Blizzard.T5.Core.LogLevel.Error, "Network.ProcessGameServerDisconnectEvents() - Exception: " + ex.Message + ", " + ex.StackTrace);
				GameNetLogger.Log(Blizzard.T5.Core.LogLevel.Error, Get().GetNetworkGameStateStringForErrors());
				ExceptionReporter.Get().ReportCaughtException(ex);
			}
		}
		GameServerDisconnectEvents.Clear();
	}

	private void UpdatePingPong()
	{
		if (m_gameServerKeepAliveFrequencySeconds == 0)
		{
			return;
		}
		double currentTime = TimeUtils.GetElapsedTimeSinceEpoch(null).TotalSeconds;
		if (m_connectApi.IsConnectedToGameServer() && currentTime - m_connectApi.GetTimeLastPingSent() > (double)m_gameServerKeepAliveFrequencySeconds)
		{
			int pingsSincePong = m_connectApi.GetPingsSinceLastPong();
			if (m_connectApi.GetTimeLastPingSent() <= m_connectApi.GetTimeLastPingReceieved())
			{
				m_connectApi.SetTimeLastPingReceived(currentTime - 0.001);
			}
			m_connectApi.SetTimeLastPingSent(currentTime);
			m_connectApi.SendPing();
			if (pingsSincePong >= m_gameServerKeepAliveRetry)
			{
				DisconnectFromGameServer(DisconnectReason.DisconnectAfterFailedPings);
				SetShouldIgnorePong(value: false);
			}
			m_connectApi.SetPingsSinceLastPong(pingsSincePong + 1);
		}
	}

	public void RegisterGameQueueHandler(GameQueueHandler handler)
	{
		if (m_gameQueueHandler != null)
		{
			GameNetLogger.Log(Blizzard.T5.Core.LogLevel.Debug, "Network.RegisterGameQueueHandler() - handler {0} would bash game queue handler {1}", handler, m_gameQueueHandler);
		}
		else
		{
			m_gameQueueHandler = handler;
		}
	}

	public void RegisterQueueInfoHandler(QueueInfoHandler handler)
	{
		if (m_queueInfoHandler != null)
		{
			GameNetLogger.Log(Blizzard.T5.Core.LogLevel.Debug, "Network.RegisterQueueInfoHandler() - handler {0} would bash queue info handler {1}", handler, m_queueInfoHandler);
		}
		else
		{
			m_queueInfoHandler = handler;
		}
	}

	private bool ProcessGameQueue()
	{
		QueueEvent result = BattleNet.GetQueueEvent();
		if (result == null)
		{
			return false;
		}
		switch (result.EventType)
		{
		case QueueEvent.Type.QUEUE_LEAVE:
		case QueueEvent.Type.QUEUE_DELAY_ERROR:
		case QueueEvent.Type.QUEUE_AMM_ERROR:
		case QueueEvent.Type.QUEUE_CANCEL:
		case QueueEvent.Type.QUEUE_GAME_STARTED:
		case QueueEvent.Type.ABORT_CLIENT_DROPPED:
			m_state.FindingBnetGameType = BnetGameType.BGT_UNKNOWN;
			break;
		}
		if (m_gameQueueHandler == null)
		{
			GameNetLogger.Log(Blizzard.T5.Core.LogLevel.Warning, "Network.ProcessGameQueue() - m_gameQueueHandler is null,  event={0} server={1}:{2} gameHandle={3} clientHandle={4}", result.EventType, (result.GameServer == null) ? "null" : result.GameServer.Address, (result.GameServer != null) ? result.GameServer.Port : 0u, (result.GameServer != null) ? result.GameServer.GameHandle : 0u, (result.GameServer == null) ? 0 : result.GameServer.ClientHandle);
		}
		else
		{
			try
			{
				m_gameQueueHandler(result);
			}
			catch (Exception ex)
			{
				GameNetLogger.Log(Blizzard.T5.Core.LogLevel.Error, "Network.ProcessGameQueue() - Exception processing game queue " + ex.Message + ", " + ex.StackTrace);
				GameNetLogger.Log(Blizzard.T5.Core.LogLevel.Error, Get().GetNetworkGameStateStringForErrors());
				ExceptionReporter.Get().ReportCaughtException(ex);
			}
		}
		return true;
	}

	private bool ProcessGameServer()
	{
		int id = NextGamePacketType();
		bool result = HandleType(id);
		m_connectApi.DropGamePacket();
		return result;
	}

	public int NextGamePacketType()
	{
		return m_connectApi.NextGamePacketType();
	}

	public bool IsFindingGame()
	{
		return m_state.FindingBnetGameType != BnetGameType.BGT_UNKNOWN;
	}

	public BnetGameType GetFindingBnetGameType()
	{
		return m_state.FindingBnetGameType;
	}

	public static BnetGameType TranslateGameTypeToBnet(PegasusShared.GameType gameType, PegasusShared.FormatType formatType, int missionId)
	{
		switch (gameType)
		{
		case PegasusShared.GameType.GT_VS_AI:
			return BnetGameType.BGT_VS_AI;
		case PegasusShared.GameType.GT_VS_FRIEND:
			return BnetGameType.BGT_FRIENDS;
		case PegasusShared.GameType.GT_TUTORIAL:
			return BnetGameType.BGT_TUTORIAL;
		case PegasusShared.GameType.GT_RANKED:
		case PegasusShared.GameType.GT_CASUAL:
			return RankMgr.Get().GetBnetGameTypeForLeague(gameType == PegasusShared.GameType.GT_RANKED, formatType);
		case PegasusShared.GameType.GT_ARENA:
			return BnetGameType.BGT_ARENA;
		case PegasusShared.GameType.GT_TAVERNBRAWL:
			if (GameUtils.IsAIMission(missionId))
			{
				return BnetGameType.BGT_TAVERNBRAWL_1P_VERSUS_AI;
			}
			if (GameUtils.IsCoopMission(missionId))
			{
				return BnetGameType.BGT_TAVERNBRAWL_2P_COOP;
			}
			return BnetGameType.BGT_TAVERNBRAWL_PVP;
		case PegasusShared.GameType.GT_BATTLEGROUNDS:
			return BnetGameType.BGT_BATTLEGROUNDS;
		case PegasusShared.GameType.GT_BATTLEGROUNDS_FRIENDLY:
			return BnetGameType.BGT_BATTLEGROUNDS_FRIENDLY;
		case PegasusShared.GameType.GT_PVPDR:
			return BnetGameType.BGT_PVPDR;
		case PegasusShared.GameType.GT_PVPDR_PAID:
			return BnetGameType.BGT_PVPDR_PAID;
		case PegasusShared.GameType.GT_MERCENARIES_PVP:
			return BnetGameType.BGT_MERCENARIES_PVP;
		case PegasusShared.GameType.GT_MERCENARIES_PVE:
			return BnetGameType.BGT_MERCENARIES_PVE;
		case PegasusShared.GameType.GT_MERCENARIES_PVE_COOP:
			return BnetGameType.BGT_MERCENARIES_PVE_COOP;
		case PegasusShared.GameType.GT_BATTLEGROUNDS_PLAYER_VS_AI:
			return BnetGameType.BGT_BATTLEGROUNDS_PLAYER_VS_AI;
		case PegasusShared.GameType.GT_BATTLEGROUNDS_DUO:
			return BnetGameType.BGT_BATTLEGROUNDS_DUO;
		case PegasusShared.GameType.GT_BATTLEGROUNDS_DUO_VS_AI:
			return BnetGameType.BGT_BATTLEGROUNDS_DUO_VS_AI;
		case PegasusShared.GameType.GT_BATTLEGROUNDS_DUO_FRIENDLY:
			return BnetGameType.BGT_BATTLEGROUNDS_DUO_FRIENDLY;
		case PegasusShared.GameType.GT_BATTLEGROUNDS_DUO_1_PLAYER_VS_AI:
			return BnetGameType.BGT_BATTLEGROUNDS_DUO_1_PLAYER_VS_AI;
		default:
			Error.AddDevFatal("Network.TranslateGameTypeToBnet() - do not know how to translate {0}", gameType);
			return BnetGameType.BGT_UNKNOWN;
		}
	}

	public void FindGame(PegasusShared.GameType gameType, PegasusShared.FormatType formatType, int scenarioId, int brawlLibraryItemId, long deckId, string aiDeck, int heroCardDbId, int? seasonId, bool restoredSavedGameState, byte[] snapshot, int? lettuceMapNodeId, long lettuceTeamId, PegasusShared.GameType progFilterOverride = PegasusShared.GameType.GT_UNKNOWN, int deckTemplateId = 0)
	{
		if (gameType == PegasusShared.GameType.GT_VS_FRIEND)
		{
			Error.AddDevFatal("Network.FindGame - friendly challenges must call EnterFriendlyChallengeGame instead.");
			return;
		}
		BnetGameType bnetGameType = TranslateGameTypeToBnet(gameType, formatType, scenarioId);
		if (bnetGameType == BnetGameType.BGT_UNKNOWN)
		{
			Error.AddDevFatal($"FindGame: no bnetGameType for {gameType} {formatType}");
			return;
		}
		if (Cheats.Get().IsForcingApprenticeGameEnabled())
		{
			bnetGameType = BnetGameType.BGT_CASUAL_STANDARD_APPRENTICE;
		}
		m_state.FindingBnetGameType = bnetGameType;
		if (IsNoAccountTutorialGame(bnetGameType))
		{
			GoToNoAccountTutorialServer(scenarioId);
			return;
		}
		bool setScenarioIdAttr = RequiresScenarioIdAttribute(gameType) || formatType == PegasusShared.FormatType.FT_TWIST;
		byte[] requestGuid = Guid.NewGuid().ToByteArray();
		Log.BattleNet.PrintInfo("FindGame type={0} scenario={1} deck={2} aideck={3} setScenId={4} request_guid={5}", (int)bnetGameType, scenarioId, deckId, aiDeck, setScenarioIdAttr ? 1 : 0, (requestGuid == null) ? "null" : requestGuid.ToHexString());
		BnetMatchmakingPlayer player = new BnetMatchmakingPlayer(BnetGameAccountId.GetGameAccountHandle(BnetPresenceMgr.Get().GetMyGameAccountId()), BnetAttribute.CreateAttribute("type", (long)bnetGameType), BnetAttribute.CreateAttribute("scenario", scenarioId), BnetAttribute.CreateAttribute("brawl_library_item_id", brawlLibraryItemId), BnetAttribute.CreateAttribute("aideck", aiDeck ?? ""), BnetAttribute.CreateAttribute("request_guid", requestGuid), BnetAttribute.CreateAttribute("mercenaries_team", lettuceTeamId));
		if (deckTemplateId != 0)
		{
			player.AddAttributes(BnetAttribute.CreateAttribute("deck_template", deckTemplateId));
		}
		else
		{
			player.AddAttributes(BnetAttribute.CreateAttribute("deck", deckId));
		}
		if (!string.IsNullOrEmpty(Cheats.Get().GetPlayerTags()))
		{
			player.AddAttributes(BnetAttribute.CreateAttribute("cheat_player_tags", Cheats.Get().GetPlayerTags()));
		}
		Cheats.Get().ClearAllPlayerTags();
		CosmeticCoinManager.Get().FindCoinToUse(deckId, out var cosmeticCoinToUse, out var deckCosmeticCoin);
		if (cosmeticCoinToUse != deckCosmeticCoin)
		{
			player.AddAttributes(BnetAttribute.CreateAttribute("cosmetic_coin_id", cosmeticCoinToUse));
		}
		CardBackManager.Get().FindCardBackToUse(deckId, out var cardBackToUse, out var deckCardBack);
		if (cardBackToUse != deckCardBack)
		{
			player.AddAttributes(BnetAttribute.CreateAttribute("card_back_id", cardBackToUse));
		}
		if (GameUtils.IsBattlegroundsGameType(gameType))
		{
			NetCache.NetCacheBattlegroundsFinishers netCacheBGFinishers = NetCache.Get().GetNetObject<NetCache.NetCacheBattlegroundsFinishers>();
			if (netCacheBGFinishers != null && netCacheBGFinishers.BattlegroundsFavoriteFinishers.Count > 0)
			{
				int randomStrikeIndex = UnityEngine.Random.Range(0, netCacheBGFinishers.BattlegroundsFavoriteFinishers.Count);
				BattlegroundsFinisherId battlegroundsStrikeId = netCacheBGFinishers.BattlegroundsFavoriteFinishers.ToList()[randomStrikeIndex];
				if (battlegroundsStrikeId.IsValid())
				{
					player.AddAttributes(BnetAttribute.CreateAttribute("battlegrounds_strike_id", battlegroundsStrikeId.ToValue()));
				}
			}
			NetCache.NetCacheBattlegroundsBoardSkins netCacheBGBoardSkins = NetCache.Get().GetNetObject<NetCache.NetCacheBattlegroundsBoardSkins>();
			if (netCacheBGBoardSkins != null && netCacheBGBoardSkins.BattlegroundsFavoriteBoardSkins.Count > 0)
			{
				int randomBoardSkinIndex = UnityEngine.Random.Range(0, netCacheBGBoardSkins.BattlegroundsFavoriteBoardSkins.Count);
				BattlegroundsBoardSkinId battlegroundsBoardSkinId = netCacheBGBoardSkins.BattlegroundsFavoriteBoardSkins.ToList()[randomBoardSkinIndex];
				if (battlegroundsBoardSkinId.IsValid())
				{
					player.AddAttributes(BnetAttribute.CreateAttribute("battlegrounds_board_skin_id", battlegroundsBoardSkinId.ToValue()));
				}
			}
		}
		if (heroCardDbId != 0)
		{
			player.AddAttributes(BnetAttribute.CreateAttribute("hero_card_id", heroCardDbId));
		}
		else if (deckId != 0L || deckTemplateId > 0)
		{
			CollectionDeck deck = CollectionManager.Get().GetDeck(deckId);
			if (deck == null)
			{
				deck = FreeDeckMgr.Get().GetLoanerDeckFromDeckTemplateId(deckTemplateId);
			}
			if (deck != null && !deck.HeroOverridden)
			{
				int randomHeroCardId = CollectionManager.Get().GetRandomHeroIdOwnedByPlayer(deck.GetClass(), deck.RandomHeroUseFavorite, null);
				player.AddAttributes(BnetAttribute.CreateAttribute("random_hero_card_id", randomHeroCardId));
			}
		}
		if (seasonId.HasValue)
		{
			player.AddAttributes(BnetAttribute.CreateAttribute("season_id", seasonId.Value));
		}
		List<Blizzard.GameService.Protocol.V2.Client.Attribute> matchmakerAttributesFilter = BnetAttribute.CreateAttributeCollection(BnetAttribute.CreateAttribute("GameType", (long)bnetGameType));
		if (setScenarioIdAttr)
		{
			matchmakerAttributesFilter.Add(BnetAttribute.CreateAttribute("ScenarioId", scenarioId));
		}
		List<Blizzard.GameService.Protocol.V2.Client.Attribute> gameAttributes = BnetAttribute.CreateAttributeCollection(BnetAttribute.CreateAttribute("type", (long)bnetGameType), BnetAttribute.CreateAttribute("scenario", scenarioId), BnetAttribute.CreateAttribute("brawl_library_item_id", brawlLibraryItemId), BnetAttribute.CreateAttribute("prog_filter_override", (long)progFilterOverride));
		if (Cheats.Get().GetBoardId() > 0)
		{
			gameAttributes.Add(BnetAttribute.CreateAttribute("cheat_board_override", Cheats.Get().GetBoardId()));
		}
		Cheats.Get().ClearBoardId();
		if (ReconnectMgr.Get().GetBypassGameReconnect())
		{
			gameAttributes.Add(BnetAttribute.CreateAttribute("bypass", val: true));
			ReconnectMgr.Get().SetBypassGameReconnect(shouldBypass: false);
		}
		if (seasonId.HasValue)
		{
			gameAttributes.Add(BnetAttribute.CreateAttribute("season_id", seasonId.Value));
		}
		if (snapshot != null)
		{
			gameAttributes.Add(BnetAttribute.CreateAttribute("snapshot", snapshot));
		}
		if (restoredSavedGameState)
		{
			gameAttributes.Add(BnetAttribute.CreateAttribute("load_game", val: true));
		}
		if (lettuceMapNodeId.HasValue)
		{
			gameAttributes.Add(BnetAttribute.CreateAttribute("lettuce_map_node_id", lettuceMapNodeId.Value));
		}
		BattleNet.QueueMatchmaking(matchmakerAttributesFilter, gameAttributes, player);
		m_state.LastFindGameParameters = new FindGameResult();
		m_state.LastFindGameParameters.TimeSpentMilliseconds = (long)TimeUtils.GetElapsedTimeSinceEpoch(null).TotalMilliseconds;
		m_state.LastFindGameParameters.GameSessionInfo = new Blizzard.Telemetry.WTCG.Client.GameSessionInfo();
		m_state.LastFindGameParameters.GameSessionInfo.Version = GetVersion();
		m_state.LastFindGameParameters.GameSessionInfo.ScenarioId = scenarioId;
		m_state.LastFindGameParameters.GameSessionInfo.BrawlLibraryItemId = brawlLibraryItemId;
		if (seasonId.HasValue)
		{
			m_state.LastFindGameParameters.GameSessionInfo.SeasonId = seasonId.Value;
		}
		m_state.LastFindGameParameters.GameSessionInfo.GameType = (Blizzard.Telemetry.WTCG.Client.GameType)gameType;
		m_state.LastFindGameParameters.GameSessionInfo.FormatType = (Blizzard.Telemetry.WTCG.Client.FormatType)formatType;
		m_state.LastFindGameParameters.GameSessionInfo.ClientDeckId = deckId;
		m_state.LastFindGameParameters.GameSessionInfo.ClientHeroCardId = heroCardDbId;
	}

	public void CancelFindGame()
	{
		if (m_state.FindingBnetGameType == BnetGameType.BGT_UNKNOWN)
		{
			return;
		}
		if (!IsLoggedIn())
		{
			m_state.FindingBnetGameType = BnetGameType.BGT_UNKNOWN;
			return;
		}
		BnetGameType gameType = GetFindingBnetGameType();
		if (!IsNoAccountTutorialGame(gameType))
		{
			BattleNet.CancelMatchmaking();
		}
		m_state.FindingBnetGameType = BnetGameType.BGT_UNKNOWN;
	}

	private bool IsNoAccountTutorialGame(BnetGameType gameType)
	{
		if (ShouldBeConnectedToAurora())
		{
			return false;
		}
		if (gameType != BnetGameType.BGT_TUTORIAL)
		{
			return false;
		}
		return true;
	}

	public void EnterFriendlyChallengeGame(PegasusShared.FormatType formatType, BrawlType brawlType, int scenarioId, int seasonId, int brawlLibraryItemId, BnetGameAccountId player2GameAccountId, DeckShareState player1DeckShareState, long player1DeckId, DeckShareState player2DeckShareState, long player2DeckId, long? player1HeroCardDbId, long? player2HeroCardDbId, long? player1RandomHeroCardDbId, long? player2RandomHeroCardDbId, long? player1CardBackId, long? player2CardBackId)
	{
		long bnetGameType = 1L;
		PegasusShared.GameType gameType = PegasusShared.GameType.GT_VS_FRIEND;
		List<Blizzard.GameService.Protocol.V2.Client.Attribute> matchmakerAttributesFilter = BnetAttribute.CreateAttributeCollection(BnetAttribute.CreateAttribute("GameType", bnetGameType), BnetAttribute.CreateAttribute("ScenarioId", scenarioId));
		List<Blizzard.GameService.Protocol.V2.Client.Attribute> gameAttributes = BnetAttribute.CreateAttributeCollection(BnetAttribute.CreateAttribute("type", bnetGameType), BnetAttribute.CreateAttribute("scenario", scenarioId), BnetAttribute.CreateAttribute("format", (long)formatType), BnetAttribute.CreateAttribute("season_id", seasonId), BnetAttribute.CreateAttribute("brawl_library_item_id", brawlLibraryItemId));
		if (Cheats.Get().GetBoardId() > 0)
		{
			gameAttributes.Add(BnetAttribute.CreateAttribute("cheat_board_override", Cheats.Get().GetBoardId()));
		}
		Cheats.Get().ClearBoardId();
		BnetMatchmakingPlayer player1 = new BnetMatchmakingPlayer(BnetGameAccountId.GetGameAccountHandle(BnetPresenceMgr.Get().GetMyGameAccountId()), BnetAttribute.CreateAttribute("type", bnetGameType), BnetAttribute.CreateAttribute("scenario", scenarioId), BnetAttribute.CreateAttribute("deck_share_state", (long)player1DeckShareState), BnetAttribute.CreateAttribute("deck", player1DeckId), BnetAttribute.CreateAttribute("player_type", 1L), BnetAttribute.CreateAttribute("season_id", seasonId));
		if (player1HeroCardDbId.HasValue)
		{
			player1.AddAttributes(BnetAttribute.CreateAttribute("hero_card_id", player1HeroCardDbId.Value));
		}
		else if (player1RandomHeroCardDbId.HasValue)
		{
			player1.AddAttributes(BnetAttribute.CreateAttribute("random_hero_card_id", player1RandomHeroCardDbId.Value));
		}
		if (player1CardBackId.HasValue)
		{
			player1.AddAttributes(BnetAttribute.CreateAttribute("card_back_id", player1CardBackId.Value));
		}
		if (!string.IsNullOrEmpty(Cheats.Get().GetPlayerTags()))
		{
			player1.AddAttributes(BnetAttribute.CreateAttribute("cheat_player_tags", Cheats.Get().GetPlayerTags()));
		}
		Cheats.Get().ClearAllPlayerTags();
		BnetMatchmakingPlayer player2 = new BnetMatchmakingPlayer(BnetGameAccountId.GetGameAccountHandle(player2GameAccountId), BnetAttribute.CreateAttribute("type", bnetGameType), BnetAttribute.CreateAttribute("scenario", scenarioId), BnetAttribute.CreateAttribute("deck_share_state", (long)player2DeckShareState), BnetAttribute.CreateAttribute("deck", player2DeckId), BnetAttribute.CreateAttribute("player_type", 2L), BnetAttribute.CreateAttribute("season_id", seasonId));
		if (player2HeroCardDbId.HasValue)
		{
			player2.AddAttributes(BnetAttribute.CreateAttribute("hero_card_id", player2HeroCardDbId.Value));
		}
		else if (player2RandomHeroCardDbId.HasValue)
		{
			player2.AddAttributes(BnetAttribute.CreateAttribute("random_hero_card_id", player2RandomHeroCardDbId.Value));
		}
		if (player2CardBackId.HasValue)
		{
			player2.AddAttributes(BnetAttribute.CreateAttribute("card_back_id", player2CardBackId.Value));
		}
		BattleNet.QueueMatchmaking(matchmakerAttributesFilter, gameAttributes, player1, player2);
		m_state.LastFindGameParameters = new FindGameResult();
		m_state.LastFindGameParameters.TimeSpentMilliseconds = (long)TimeUtils.GetElapsedTimeSinceEpoch(null).TotalMilliseconds;
		m_state.LastFindGameParameters.GameSessionInfo = new Blizzard.Telemetry.WTCG.Client.GameSessionInfo();
		m_state.LastFindGameParameters.GameSessionInfo.Version = GetVersion();
		m_state.LastFindGameParameters.GameSessionInfo.ScenarioId = scenarioId;
		m_state.LastFindGameParameters.GameSessionInfo.BrawlLibraryItemId = brawlLibraryItemId;
		m_state.LastFindGameParameters.GameSessionInfo.SeasonId = seasonId;
		m_state.LastFindGameParameters.GameSessionInfo.GameType = (Blizzard.Telemetry.WTCG.Client.GameType)gameType;
		m_state.LastFindGameParameters.GameSessionInfo.FormatType = (Blizzard.Telemetry.WTCG.Client.FormatType)formatType;
		m_state.LastFindGameParameters.GameSessionInfo.ClientDeckId = player1DeckId;
		if (player1HeroCardDbId.HasValue)
		{
			m_state.LastFindGameParameters.GameSessionInfo.ClientHeroCardId = player1HeroCardDbId.Value;
		}
	}

	public void EnterBattlegroundsWithFriend(BnetGameAccountId player2BnetGameAccountId, int scenarioId)
	{
		long bnetGameType = 50L;
		m_state.FindingBnetGameType = BnetGameType.BGT_BATTLEGROUNDS;
		List<Blizzard.GameService.Protocol.V2.Client.Attribute> matchmakerAttributesFilter = BnetAttribute.CreateAttributeCollection(BnetAttribute.CreateAttribute("GameType", bnetGameType), BnetAttribute.CreateAttribute("ScenarioId", scenarioId));
		List<Blizzard.GameService.Protocol.V2.Client.Attribute> gameAttributes = BnetAttribute.CreateAttributeCollection(BnetAttribute.CreateAttribute("type", bnetGameType), BnetAttribute.CreateAttribute("scenario", scenarioId), BnetAttribute.CreateAttribute("format", 2L));
		BnetGameAccountId player1BnetGameAccountId = BnetPresenceMgr.Get().GetMyGameAccountId();
		BnetMatchmakingPlayer player1 = new BnetMatchmakingPlayer(BnetGameAccountId.GetGameAccountHandle(player1BnetGameAccountId), BnetAttribute.CreateAttribute("type", bnetGameType), BnetAttribute.CreateAttribute("scenario", scenarioId), BnetAttribute.CreateAttribute("player_type", 1L));
		if (!string.IsNullOrEmpty(Cheats.Get().GetPlayerTags()))
		{
			player1.AddAttributes(BnetAttribute.CreateAttribute("cheat_player_tags", Cheats.Get().GetPlayerTags()));
		}
		BnetPartyId currentPartyId = PartyManager.Get().GetCurrentPartyId();
		if (BattleNet.GetMemberAttribute(currentPartyId, player1BnetGameAccountId, "battlegrounds_strike_id", out int player1BattlegroundsStrikeIdInt))
		{
			player1.AddAttributes(BnetAttribute.CreateAttribute("battlegrounds_strike_id", player1BattlegroundsStrikeIdInt));
		}
		if (BattleNet.GetMemberAttribute(currentPartyId, player1BnetGameAccountId, "battlegrounds_board_skin_id", out int player1BattlegroundsBoardSkinIdInt))
		{
			player1.AddAttributes(BnetAttribute.CreateAttribute("battlegrounds_board_skin_id", player1BattlegroundsBoardSkinIdInt));
		}
		Cheats.Get().ClearAllPlayerTags();
		BnetMatchmakingPlayer player2 = new BnetMatchmakingPlayer(BnetGameAccountId.GetGameAccountHandle(player2BnetGameAccountId), BnetAttribute.CreateAttribute("type", bnetGameType), BnetAttribute.CreateAttribute("scenario", scenarioId), BnetAttribute.CreateAttribute("player_type", 2L));
		if (BattleNet.GetMemberAttribute(currentPartyId, player2BnetGameAccountId, "battlegrounds_strike_id", out int player2BattlegroundsStrikeIdInt))
		{
			player2.AddAttributes(BnetAttribute.CreateAttribute("battlegrounds_strike_id", player2BattlegroundsStrikeIdInt));
		}
		if (BattleNet.GetMemberAttribute(currentPartyId, player2BnetGameAccountId, "battlegrounds_board_skin_id", out int player2BattlegroundsBoardSkinIdInt))
		{
			player2.AddAttributes(BnetAttribute.CreateAttribute("battlegrounds_board_skin_id", player2BattlegroundsBoardSkinIdInt));
		}
		BattleNet.QueueMatchmaking(matchmakerAttributesFilter, gameAttributes, player1, player2);
	}

	public void EnterBattlegroundsWithParty(BnetParty.PartyMember[] members, int scenarioId)
	{
		bool isDuos = scenarioId == 5173;
		bool isPrivateParty = false;
		long bnetGameType;
		if (isDuos)
		{
			if (members.Length <= PartyManager.Get().GetBattlegroundsMaxRankedPartySize())
			{
				bnetGameType = 65L;
				m_state.FindingBnetGameType = BnetGameType.BGT_BATTLEGROUNDS_DUO;
			}
			else
			{
				bnetGameType = 67L;
				m_state.FindingBnetGameType = BnetGameType.BGT_BATTLEGROUNDS_DUO_FRIENDLY;
				isPrivateParty = true;
			}
		}
		else if (members.Length <= PartyManager.Get().GetBattlegroundsMaxRankedPartySize())
		{
			bnetGameType = 50L;
			m_state.FindingBnetGameType = BnetGameType.BGT_BATTLEGROUNDS;
		}
		else
		{
			bnetGameType = 51L;
			m_state.FindingBnetGameType = BnetGameType.BGT_BATTLEGROUNDS_FRIENDLY;
			isPrivateParty = true;
		}
		BattleNet.SetPartyAttributes(PartyManager.Get().GetCurrentPartyId(), BnetAttribute.CreateAttribute("finding_bnet_game_type", bnetGameType));
		int partySize = PartyManager.Get().GetCurrentPartySize();
		BnetEntityId partyLeaderId = PartyManager.Get().GetLeader() ?? new BnetEntityId(0uL, 0uL);
		List<Blizzard.GameService.Protocol.V2.Client.Attribute> matchmakerAttributesFilter = BnetAttribute.CreateAttributeCollection(BnetAttribute.CreateAttribute("GameType", bnetGameType), BnetAttribute.CreateAttribute("ScenarioId", scenarioId));
		List<Blizzard.GameService.Protocol.V2.Client.Attribute> gameAttributes = BnetAttribute.CreateAttributeCollection(BnetAttribute.CreateAttribute("type", bnetGameType), BnetAttribute.CreateAttribute("scenario", scenarioId), BnetAttribute.CreateAttribute("format", 2L));
		BnetPartyId currentPartyId = PartyManager.Get().GetCurrentPartyId();
		List<BnetMatchmakingPlayer> players = new List<BnetMatchmakingPlayer>();
		Map<BnetGameAccountId, int> duosTeamIds = PartyManager.Get().GetDuosTeams(isDuos, isPrivateParty, members);
		for (int i = 0; i < members.Length; i++)
		{
			BnetGameAccountId bnetGameAccountId = members[i].GameAccountId;
			BnetMatchmakingPlayer player = new BnetMatchmakingPlayer(BnetGameAccountId.GetGameAccountHandle(bnetGameAccountId), BnetAttribute.CreateAttribute("type", bnetGameType), BnetAttribute.CreateAttribute("scenario", scenarioId), BnetAttribute.CreateAttribute("player_type", 2L), BnetAttribute.CreateAttribute("party_size", partySize), BnetAttribute.CreateAttribute("party_leader_game_account_id_hi", partyLeaderId.High), BnetAttribute.CreateAttribute("party_leader_game_account_id_lo", partyLeaderId.Low));
			if (isDuos)
			{
				player.AddAttributes(BnetAttribute.CreateAttribute("battlegrounds_duos_team", duosTeamIds[bnetGameAccountId]));
			}
			string playerTags;
			if (bnetGameAccountId == BnetPresenceMgr.Get().GetMyGameAccountId() && !string.IsNullOrEmpty(Cheats.Get().GetPlayerTags()))
			{
				player.AddAttributes(BnetAttribute.CreateAttribute("cheat_player_tags", Cheats.Get().GetPlayerTags()));
				Cheats.Get().ClearAllPlayerTags();
			}
			else if (BattleNet.GetMemberAttribute(currentPartyId, bnetGameAccountId, "cheat_player_tags", out playerTags))
			{
				player.AddAttributes(BnetAttribute.CreateAttribute("cheat_player_tags", playerTags));
			}
			if (BattleNet.GetMemberAttribute(currentPartyId, bnetGameAccountId, "battlegrounds_strike_id", out int battlegroundsStrikeIdInt))
			{
				player.AddAttributes(BnetAttribute.CreateAttribute("battlegrounds_strike_id", battlegroundsStrikeIdInt));
			}
			if (BattleNet.GetMemberAttribute(currentPartyId, bnetGameAccountId, "battlegrounds_board_skin_id", out int battlegroundsBoardSkinIdInt))
			{
				player.AddAttributes(BnetAttribute.CreateAttribute("battlegrounds_board_skin_id", battlegroundsBoardSkinIdInt));
			}
			players.Add(player);
		}
		BattleNet.QueueMatchmaking(matchmakerAttributesFilter, gameAttributes, players.ToArray());
	}

	public void EnterMercenariesCoOpWithFriend(BnetGameAccountId player2GameAccountId, int scenarioId, int? mapNodeId)
	{
		long bnetGameType = 60L;
		m_state.FindingBnetGameType = BnetGameType.BGT_MERCENARIES_PVE_COOP;
		List<Blizzard.GameService.Protocol.V2.Client.Attribute> matchmakerAttributesFilter = BnetAttribute.CreateAttributeCollection(BnetAttribute.CreateAttribute("GameType", bnetGameType), BnetAttribute.CreateAttribute("ScenarioId", scenarioId));
		List<Blizzard.GameService.Protocol.V2.Client.Attribute> gameAttributes = BnetAttribute.CreateAttributeCollection(BnetAttribute.CreateAttribute("type", bnetGameType), BnetAttribute.CreateAttribute("scenario", scenarioId), BnetAttribute.CreateAttribute("format", 2L));
		if (mapNodeId.HasValue)
		{
			gameAttributes.Add(BnetAttribute.CreateAttribute("lettuce_map_node_id", mapNodeId.Value));
		}
		BnetEntityId partyLeaderId = PartyManager.Get().GetLeader() ?? new BnetEntityId(0uL, 0uL);
		BnetMatchmakingPlayer player1 = new BnetMatchmakingPlayer(BnetGameAccountId.GetGameAccountHandle(BnetPresenceMgr.Get().GetMyGameAccountId()), BnetAttribute.CreateAttribute("type", bnetGameType), BnetAttribute.CreateAttribute("scenario", scenarioId), BnetAttribute.CreateAttribute("player_type", 1L));
		if (!string.IsNullOrEmpty(Cheats.Get().GetPlayerTags()))
		{
			player1.AddAttributes(BnetAttribute.CreateAttribute("cheat_player_tags", Cheats.Get().GetPlayerTags()));
		}
		player1.AddAttributes(BnetAttribute.CreateAttribute("party_leader_game_account_id_hi", partyLeaderId.High));
		player1.AddAttributes(BnetAttribute.CreateAttribute("party_leader_game_account_id_lo", partyLeaderId.Low));
		Cheats.Get().ClearAllPlayerTags();
		BnetMatchmakingPlayer player2 = new BnetMatchmakingPlayer(BnetGameAccountId.GetGameAccountHandle(player2GameAccountId), BnetAttribute.CreateAttribute("type", bnetGameType), BnetAttribute.CreateAttribute("scenario", scenarioId), BnetAttribute.CreateAttribute("player_type", 2L), BnetAttribute.CreateAttribute("party_leader_game_account_id_hi", partyLeaderId.High), BnetAttribute.CreateAttribute("party_leader_game_account_id_lo", partyLeaderId.Low));
		BattleNet.QueueMatchmaking(matchmakerAttributesFilter, gameAttributes, player1, player2);
	}

	public void EnterMercenariesFriendlyChallenge(int scenarioId, long player1TeamId, bool player1Sharing, long player2TeamId, bool player2Sharing, BnetGameAccountId player2GameAccountId)
	{
		long bnetGameType = 61L;
		PegasusShared.GameType gameType = PegasusShared.GameType.GT_MERCENARIES_FRIENDLY;
		List<Blizzard.GameService.Protocol.V2.Client.Attribute> matchmakerAttributesFilter = BnetAttribute.CreateAttributeCollection();
		matchmakerAttributesFilter.Add(BnetAttribute.CreateAttribute("GameType", bnetGameType));
		matchmakerAttributesFilter.Add(BnetAttribute.CreateAttribute("ScenarioId", scenarioId));
		List<Blizzard.GameService.Protocol.V2.Client.Attribute> gameAttributes = BnetAttribute.CreateAttributeCollection();
		gameAttributes.Add(BnetAttribute.CreateAttribute("type", bnetGameType));
		gameAttributes.Add(BnetAttribute.CreateAttribute("scenario", scenarioId));
		BnetMatchmakingPlayer player1 = new BnetMatchmakingPlayer(BnetGameAccountId.GetGameAccountHandle(BnetPresenceMgr.Get().GetMyGameAccountId()), BnetAttribute.CreateAttribute("type", bnetGameType), BnetAttribute.CreateAttribute("scenario", scenarioId), BnetAttribute.CreateAttribute("deck_share_state", player1Sharing ? 2 : 0), BnetAttribute.CreateAttribute("mercenaries_team", player1TeamId), BnetAttribute.CreateAttribute("player_type", 1L));
		BnetMatchmakingPlayer player2 = new BnetMatchmakingPlayer(BnetGameAccountId.GetGameAccountHandle(player2GameAccountId), BnetAttribute.CreateAttribute("type", bnetGameType), BnetAttribute.CreateAttribute("scenario", scenarioId), BnetAttribute.CreateAttribute("deck_share_state", player2Sharing ? 2 : 0), BnetAttribute.CreateAttribute("mercenaries_team", player2TeamId), BnetAttribute.CreateAttribute("player_type", 2L));
		BattleNet.QueueMatchmaking(matchmakerAttributesFilter, gameAttributes, player1, player2);
		m_state.LastFindGameParameters = new FindGameResult();
		m_state.LastFindGameParameters.TimeSpentMilliseconds = (long)TimeUtils.GetElapsedTimeSinceEpoch(null).TotalMilliseconds;
		m_state.LastFindGameParameters.GameSessionInfo = new Blizzard.Telemetry.WTCG.Client.GameSessionInfo();
		m_state.LastFindGameParameters.GameSessionInfo.Version = GetVersion();
		m_state.LastFindGameParameters.GameSessionInfo.ScenarioId = scenarioId;
		m_state.LastFindGameParameters.GameSessionInfo.GameType = (Blizzard.Telemetry.WTCG.Client.GameType)gameType;
		m_state.LastFindGameParameters.GameSessionInfo.ClientDeckId = player1TeamId;
	}

	public void OnFindGameStateChanged(FindGameState prevState, FindGameState newState, uint errorCode)
	{
		switch (newState)
		{
		case FindGameState.CLIENT_ERROR:
		case FindGameState.BNET_ERROR:
		case FindGameState.SERVER_GAME_CONNECTING:
			SendTelemetry_FindGameResult(errorCode);
			break;
		case FindGameState.CLIENT_CANCELED:
		case FindGameState.BNET_QUEUE_ENTERED:
		case FindGameState.BNET_QUEUE_DELAYED:
		case FindGameState.BNET_QUEUE_UPDATED:
		case FindGameState.BNET_QUEUE_CANCELED:
		case FindGameState.SERVER_GAME_STARTED:
		case FindGameState.SERVER_GAME_CANCELED:
			break;
		}
	}

	private void SendTelemetry_FindGameResult(uint errorCode)
	{
		if (m_state.LastFindGameParameters != null)
		{
			string errorStr;
			if (errorCode >= 1000000)
			{
				ErrorCode errorCode2 = (ErrorCode)errorCode;
				errorStr = errorCode2.ToString();
			}
			else
			{
				BattleNetErrors battleNetErrors = (BattleNetErrors)errorCode;
				errorStr = battleNetErrors.ToString();
			}
			long timeSpentMilliseconds = (long)(TimeUtils.GetElapsedTimeSinceEpoch(null).TotalMilliseconds - (double)m_state.LastFindGameParameters.TimeSpentMilliseconds);
			m_state.LastFindGameParameters.ResultCode = errorCode;
			m_state.LastFindGameParameters.ResultCodeString = errorStr;
			m_state.LastFindGameParameters.TimeSpentMilliseconds = timeSpentMilliseconds;
			TelemetryManager.Client().SendFindGameResult(m_state.LastFindGameParameters);
			m_state.LastFindGameParameters = null;
		}
	}

	public UnavailableReason GetHearthstoneUnavailable(bool gamePacket)
	{
		UnavailableReason reason = new UnavailableReason();
		if (gamePacket)
		{
			Deadend deadend = m_connectApi.GetDeadendGame();
			reason.mainReason = deadend.Reply1;
			reason.subReason = deadend.Reply2;
			reason.extraData = deadend.Reply3;
		}
		else
		{
			DeadendUtil deadend2 = m_connectApi.GetDeadendUtil();
			reason.mainReason = deadend2.Reply1;
			reason.subReason = deadend2.Reply2;
			reason.extraData = deadend2.Reply3;
		}
		return reason;
	}

	private void SendDefaultDisconnectTelemetry()
	{
		GameServerInfo gameServerInfo = GetLastGameServerJoined();
		TelemetryManager.Client().SendDisconnect("GAME", TelemetryUtil.GetReasonFromBnetError(BattleNetErrors.ERROR_OK), null, gameServerInfo?.Address, gameServerInfo?.Port);
	}

	public GameServerInfo GetLastGameServerJoined()
	{
		return m_state.LastGameServerInfo;
	}

	public void ClearLastGameServerJoined()
	{
		m_state.LastGameServerInfo = null;
	}

	private void SendGameServerHandshake(GameServerInfo gameInfo)
	{
		GameNetLogger.Log(Blizzard.T5.Core.LogLevel.Information, "Network.SendGameServerHandshake()");
		NetCache.Get().DispatchClientOptionsToServer();
		if (gameInfo.SpectatorMode)
		{
			BnetGameAccountId gameAccountId = BnetPresenceMgr.Get().GetMyGameAccountId();
			m_connectApi.SendSpectatorGameHandshake(BattleNet.GetVersion(), GetPlatformBuilder(), gameInfo, new BnetId
			{
				Hi = gameAccountId.High,
				Lo = gameAccountId.Low
			});
		}
		else
		{
			m_connectApi.SendGameHandshake(gameInfo, GetPlatformBuilder());
		}
	}

	public GameCancelInfo GetGameCancelInfo()
	{
		GameNetLogger.Log(Blizzard.T5.Core.LogLevel.Debug, "Network.GetGameCancelInfo()");
		GameCanceled packet = m_connectApi.GetGameCancelInfo();
		if (packet == null)
		{
			return null;
		}
		return new GameCancelInfo
		{
			CancelReason = (GameCancelInfo.Reason)packet.Reason_
		};
	}

	public void GetGameState()
	{
		m_connectApi.GetGameState();
	}

	public void ResendOptions()
	{
		m_connectApi.ResendOptions();
	}

	public void UpdateBattlegroundInfo()
	{
		m_connectApi.UpdateBattlegroundInfo();
	}

	public void UpdateBattlegroundHeroArmorTierList()
	{
		m_connectApi.UpdateBattlegroundHeroArmorTierList();
	}

	public void UpdateBattlegroundsPlayerAnomaly()
	{
		m_connectApi.UpdateBattlegroundsPlayerAnomaly();
	}

	public void ToggleAIPlayer()
	{
		m_connectApi.ToggleAIPlayer();
	}

	public void SetBattlegroundHeroBuddyGained(int value, int playerId)
	{
		m_connectApi.SetBattlegroundHeroBuddyGained(value, playerId);
	}

	public void SetBattlegroundHeroBuddyProgress(int progress, int playerId)
	{
		m_connectApi.SetBattlegroundHeroBuddyProgress(progress, playerId);
	}

	public void ReplaceBattlegroundHero(int heroID, int playerId)
	{
		m_connectApi.ReplaceBattlegroundHero(heroID, playerId);
	}

	public void RequestReplaceBattlegroundsMulliganHero(int heroID)
	{
		m_connectApi.RequestReplaceBattlegroundsMulliganHero(heroID);
	}

	public void RequestReplaceAllBattlegroundsMulliganHeroExcept(List<int> heroIDsToKeep)
	{
		m_connectApi.RequestReplaceAllBattlegroundsMulliganHeroExcept(heroIDsToKeep);
	}

	public void RequestGameRoundHistory()
	{
		m_connectApi.RequestGameRoundHistory();
	}

	public void RequestRealtimeBattlefieldRaces()
	{
		m_connectApi.RequestRealtimeBattlefieldRaces();
	}

	public TurnTimerInfo GetTurnTimerInfo()
	{
		PegasusGame.TurnTimer packet = m_connectApi.GetTurnTimerInfo();
		if (packet == null)
		{
			return null;
		}
		return new TurnTimerInfo
		{
			Seconds = packet.Seconds,
			Turn = packet.Turn,
			Show = packet.Show
		};
	}

	public int GetNAckOption()
	{
		return m_connectApi.GetNAckOption()?.Id ?? 0;
	}

	public SpectatorNotify GetSpectatorNotify()
	{
		return m_connectApi.GetSpectatorNotify();
	}

	public AIDebugInformation GetAIDebugInformation()
	{
		return m_connectApi.GetAIDebugInformation();
	}

	public RopeTimerDebugInformation GetRopeTimerDebugInformation()
	{
		return m_connectApi.GetRopeTimerDebugInformation();
	}

	public ScriptDebugInformation GetScriptDebugInformation()
	{
		return m_connectApi.GetScriptDebugInformation();
	}

	public GameRoundHistory GetGameRoundHistory()
	{
		return m_connectApi.GetGameRoundHistory();
	}

	public GameRealTimeBattlefieldRaces GetGameRealTimeBattlefieldRaces()
	{
		return m_connectApi.GetGameRealTimeBattlefieldRaces();
	}

	public PlayerRealTimeBattlefieldRaces GetPlayerRealTimeBattlefieldRaces()
	{
		return m_connectApi.GetPlayerRealTimeBattlefieldRaces();
	}

	public BattlegroundsRatingChange GetBattlegroundsRatingChange()
	{
		return m_connectApi.GetBattlegroundsRatingChange();
	}

	public GameGuardianVars GetGameGuardianVars()
	{
		return m_connectApi.GetGameGuardianVars();
	}

	public UpdateBattlegroundInfo GetBattlegroundInfo()
	{
		return m_connectApi.GetBattlegroundInfo();
	}

	public GetBattlegroundHeroArmorTierList GetBattlegroundHeroArmorTierList()
	{
		return m_connectApi.GetBattlegroundHeroArmorTierList();
	}

	public GetBattlegroundsPlayerAnomaly GetBattlegroundsPlayerAnomaly()
	{
		return m_connectApi.GetBattlegroundsPlayerAnomaly();
	}

	public DebugMessage GetDebugMessage()
	{
		return m_connectApi.GetDebugMessage();
	}

	public ScriptLogMessage GetScriptLogMessage()
	{
		return m_connectApi.GetScriptLogMessage();
	}

	public AchievementProgress GetAchievementInGameProgress()
	{
		return m_connectApi.GetAchievementInGameProgress();
	}

	public void SendClientScriptGameEvent(ClientScriptGameEventType eventType, int data)
	{
		m_connectApi.SendClientScriptGameEvent(eventType, data);
	}

	public EntityChoices GetEntityChoices()
	{
		PegasusGame.EntityChoices packet = m_connectApi.GetEntityChoices();
		if (packet == null)
		{
			return null;
		}
		return new EntityChoices
		{
			ID = packet.Id,
			ChoiceType = (CHOICE_TYPE)packet.ChoiceType,
			CountMax = packet.CountMax,
			CountMin = packet.CountMin,
			Entities = CopyIntList(packet.Entities),
			Source = packet.Source,
			PlayerId = packet.PlayerId,
			HideChosen = packet.HideChosen
		};
	}

	public EntitiesChosen GetEntitiesChosen()
	{
		PegasusGame.EntitiesChosen packet = m_connectApi.GetEntitiesChosen();
		if (packet == null)
		{
			return null;
		}
		return new EntitiesChosen
		{
			ID = packet.ChooseEntities.Id,
			Entities = CopyIntList(packet.ChooseEntities.Entities),
			PlayerId = packet.PlayerId,
			ChoiceType = (CHOICE_TYPE)packet.ChoiceType
		};
	}

	public MulliganChooseOneTentativeSelection GetMulliganChooseOneTentativeSelection()
	{
		PegasusGame.MulliganChooseOneTentativeSelection packet = m_connectApi.GetMulliganChooseOneTentativeSelection();
		if (packet == null)
		{
			return null;
		}
		return new MulliganChooseOneTentativeSelection
		{
			EntityId = (packet.HasEntityID ? packet.EntityID : 0),
			IsConfirmation = (packet.HasIsConfirmation && packet.IsConfirmation),
			IsFromTeammate = packet.IsFromTeammate
		};
	}

	public void SendChoices(int id, List<int> picks)
	{
		m_connectApi.SendChoices(id, picks);
	}

	public void SendMulliganChooseOneTentativeSelect(int entityId, bool isConfirmation)
	{
		m_connectApi.SendMulliganChooseOneTentativeSelect(entityId, isConfirmation);
	}

	public void SendPreRefreshBGHeroes()
	{
		m_connectApi.SendPreRefreshBGHeroes();
	}

	public void SendOption(int id, int index, int target, int sub, int pos)
	{
		m_connectApi.SendOption(id, index, target, sub, pos);
	}

	public Options GetOptions()
	{
		AllOptions packet = m_connectApi.GetAllOptions();
		Options result = new Options
		{
			ID = packet.Id
		};
		for (int i = 0; i < packet.Options.Count; i++)
		{
			PegasusGame.Option option = packet.Options[i];
			Options.Option nextOption = new Options.Option();
			nextOption.Type = (Options.Option.OptionType)option.Type_;
			if (option.HasMainOption)
			{
				nextOption.Main.ID = option.MainOption.Id;
				nextOption.Main.PlayErrorInfo.PlayError = (PlayErrors.ErrorType)option.MainOption.PlayError;
				nextOption.Main.PlayErrorInfo.PlayErrorParam = (option.MainOption.HasPlayErrorParam ? new int?(option.MainOption.PlayErrorParam) : ((int?)null));
				nextOption.Main.Targets = CopyTargetOptionList(option.MainOption.Targets);
			}
			for (int j = 0; j < option.SubOptions.Count; j++)
			{
				SubOption subOption = option.SubOptions[j];
				Options.Option.SubOption nextSubOption = new Options.Option.SubOption();
				nextSubOption.ID = subOption.Id;
				nextSubOption.PlayErrorInfo.PlayError = (PlayErrors.ErrorType)subOption.PlayError;
				nextSubOption.PlayErrorInfo.PlayErrorParam = (subOption.HasPlayErrorParam ? new int?(subOption.PlayErrorParam) : ((int?)null));
				nextSubOption.Targets = CopyTargetOptionList(subOption.Targets);
				nextOption.Subs.Add(nextSubOption);
			}
			result.List.Add(nextOption);
		}
		return result;
	}

	private List<Options.Option.TargetOption> CopyTargetOptionList(IList<TargetOption> originalList)
	{
		List<Options.Option.TargetOption> copyList = new List<Options.Option.TargetOption>();
		for (int i = 0; i < originalList.Count; i++)
		{
			TargetOption targetOption = originalList[i];
			Options.Option.TargetOption copyTargetOption = new Options.Option.TargetOption();
			copyTargetOption.CopyFrom(targetOption);
			copyList.Add(copyTargetOption);
		}
		return copyList;
	}

	private List<int> CopyIntList(IList<int> intList)
	{
		int[] copyBuffer = new int[intList.Count];
		intList.CopyTo(copyBuffer, 0);
		return new List<int>(copyBuffer);
	}

	public void SendUserUI(int overCard, int heldCard, int arrowOrigin, int x, int y)
	{
		if (NetCache.Get().GetNetObject<NetCache.NetCacheFeatures>().Games.ShowUserUI != 0)
		{
			m_connectApi.SendUserUi(overCard, heldCard, arrowOrigin, x, y);
		}
	}

	public void SendUserUI(int overCard, int heldCard, int arrowOrigin, int x, int y, bool useTeammatesGamestate)
	{
		if (NetCache.Get().GetNetObject<NetCache.NetCacheFeatures>().Games.ShowUserUI != 0)
		{
			m_connectApi.SendUserUi(overCard, heldCard, arrowOrigin, x, y, useTeammatesGamestate);
		}
	}

	public void SendEmote(EmoteType emote)
	{
		m_connectApi.SendEmote((int)emote);
	}

	public void SendBattlegroundsEmote(EmoteType emote, int battlegroundsEmoteId)
	{
		m_connectApi.SendBattlegroundsEmote((int)emote, battlegroundsEmoteId);
	}

	public void SendSelection(int selectedEntityId)
	{
		m_connectApi.SendSelection(selectedEntityId);
	}

	public void SendRemoveSpectators(bool regenerateSpectatorPassword, params BnetGameAccountId[] bnetGameAccountIds)
	{
		List<BnetId> bnetIds = new List<BnetId>();
		for (int i = 0; i < bnetGameAccountIds.Length; i++)
		{
			bnetIds.Add(new BnetId
			{
				Hi = bnetGameAccountIds[i].High,
				Lo = bnetGameAccountIds[i].Low
			});
		}
		m_connectApi.SendRemoveSpectators(regenerateSpectatorPassword, bnetIds);
	}

	public UserUI GetUserUI()
	{
		PegasusGame.UserUI packet = m_connectApi.GetUserUi();
		if (packet == null)
		{
			return null;
		}
		UserUI result = new UserUI();
		if (packet.HasPlayerId)
		{
			result.playerId = packet.PlayerId;
		}
		if (packet.HasFromTeammate)
		{
			result.fromTeammate = packet.FromTeammate;
		}
		if (packet.HasMouseInfo)
		{
			MouseInfo mouseInfo = packet.MouseInfo;
			result.mouseInfo = new UserUI.MouseInfo();
			result.mouseInfo.ArrowOriginID = mouseInfo.ArrowOrigin;
			result.mouseInfo.HeldCardID = mouseInfo.HeldCard;
			result.mouseInfo.OverCardID = mouseInfo.OverCard;
			result.mouseInfo.X = mouseInfo.X;
			result.mouseInfo.Y = mouseInfo.Y;
			if (mouseInfo.HasUseTeammatesGamestate)
			{
				result.mouseInfo.UseTeammatesGamestate = mouseInfo.UseTeammatesGamestate;
			}
		}
		else if (packet.HasEmote)
		{
			result.emoteInfo = new UserUI.EmoteInfo();
			result.emoteInfo.Emote = packet.Emote;
			if (packet.HasBattlegroundsEmoteId)
			{
				result.emoteInfo.BattlegroundsEmoteId = packet.BattlegroundsEmoteId;
			}
		}
		else if (packet.HasSelectedEntityId)
		{
			result.selectionInfo = new UserUI.SelectionInfo();
			result.selectionInfo.SelectedEntityID = packet.SelectedEntityId;
		}
		return result;
	}

	public GameSetup GetGameSetupInfo()
	{
		PegasusGame.GameSetup packet = m_connectApi.GetGameSetup();
		if (packet == null)
		{
			return null;
		}
		GameSetup result = new GameSetup();
		result.Board = packet.Board;
		result.BoardLayout = packet.BoardLayout;
		result.BaconFavoriteBoardSkin = packet.BaconFavoriteBoardSkin;
		result.MaxSecretZoneSizePerPlayer = packet.MaxSecretZoneSizePerPlayer;
		result.MaxSecretsPerPlayer = packet.MaxSecretsPerPlayer;
		result.MaxQuestsPerPlayer = packet.MaxQuestsPerPlayer;
		result.MaxFriendlyMinionsPerPlayer = packet.MaxFriendlyMinionsPerPlayer;
		if (packet.HasBaconTeammateFavoriteBoardSkin)
		{
			result.BaconTeammateFavoriteBoardSkin = packet.BaconTeammateFavoriteBoardSkin;
		}
		if (packet.HasKeepAliveFrequencySeconds)
		{
			m_gameServerKeepAliveFrequencySeconds = packet.KeepAliveFrequencySeconds;
		}
		else
		{
			m_gameServerKeepAliveFrequencySeconds = 0u;
		}
		if (packet.HasKeepAliveRetry)
		{
			m_gameServerKeepAliveRetry = packet.KeepAliveRetry;
		}
		else
		{
			m_gameServerKeepAliveRetry = 1u;
		}
		if (packet.HasKeepAliveWaitForInternetSeconds)
		{
			m_gameServerKeepAliveWaitForInternetSeconds = packet.KeepAliveWaitForInternetSeconds;
		}
		else
		{
			m_gameServerKeepAliveWaitForInternetSeconds = 20u;
		}
		if (packet.HasDisconnectWhenStuckSeconds)
		{
			result.DisconnectWhenStuckSeconds = packet.DisconnectWhenStuckSeconds;
		}
		return result;
	}

	public List<PowerHistory> GetPowerHistory()
	{
		PegasusGame.PowerHistory packet = m_connectApi.GetPowerHistory();
		if (packet == null)
		{
			return null;
		}
		List<PowerHistory> result = new List<PowerHistory>();
		for (int i = 0; i < packet.List.Count; i++)
		{
			PowerHistoryData history = packet.List[i];
			PowerHistory entry = null;
			if (history.HasFullEntity)
			{
				entry = GetFullEntity(history.FullEntity);
			}
			else if (history.HasShowEntity)
			{
				entry = GetShowEntity(history.ShowEntity);
			}
			else if (history.HasHideEntity)
			{
				entry = GetHideEntity(history.HideEntity);
			}
			else if (history.HasChangeEntity)
			{
				entry = GetChangeEntity(history.ChangeEntity);
			}
			else if (history.HasTagChange)
			{
				entry = GetTagChange(history.TagChange);
			}
			else if (history.HasPowerStart)
			{
				entry = GetBlockStart(history.PowerStart);
			}
			else if (history.HasPowerEnd)
			{
				entry = GetBlockEnd(history.PowerEnd);
			}
			else if (history.HasCreateGame)
			{
				entry = GetCreateGame(history.CreateGame);
			}
			else if (history.HasResetGame)
			{
				entry = GetResetGame(history.ResetGame);
			}
			else if (history.HasMetaData)
			{
				entry = GetMetaData(history.MetaData);
			}
			else if (history.HasSubSpellStart)
			{
				entry = GetSubSpellStart(history.SubSpellStart);
			}
			else if (history.HasSubSpellEnd)
			{
				entry = GetSubSpellEnd(history.SubSpellEnd);
			}
			else if (history.HasVoSpell)
			{
				entry = GetVoSpell(history.VoSpell);
			}
			else if (history.HasVoBanter)
			{
				entry = GetVoBanter(history.VoBanter);
			}
			else if (history.HasCachedTagForDormantChange)
			{
				entry = GetCachedTagForDormantChange(history.CachedTagForDormantChange);
			}
			else if (history.HasShuffleDeck)
			{
				entry = GetShuffleDeck(history.ShuffleDeck);
			}
			else if (history.HasTagListChange)
			{
				entry = GetTagListChange(history.TagListChange);
			}
			else
			{
				Debug.LogError("Network.GetPowerHistory() - received invalid PowerHistoryData packet");
			}
			if (entry != null)
			{
				result.Add(entry);
			}
		}
		return result;
	}

	private static HistFullEntity GetFullEntity(PowerHistoryEntity entity)
	{
		return new HistFullEntity
		{
			Entity = Entity.CreateFromProto(entity)
		};
	}

	private static HistShowEntity GetShowEntity(PowerHistoryEntity entity)
	{
		return new HistShowEntity
		{
			Entity = Entity.CreateFromProto(entity)
		};
	}

	private static HistHideEntity GetHideEntity(PowerHistoryHide hide)
	{
		return new HistHideEntity
		{
			Entity = hide.Entity,
			Zone = hide.Zone
		};
	}

	private static HistChangeEntity GetChangeEntity(PowerHistoryEntity entity)
	{
		return new HistChangeEntity
		{
			Entity = Entity.CreateFromProto(entity)
		};
	}

	private static HistTagChange GetTagChange(PowerHistoryTagChange tagChange)
	{
		return new HistTagChange
		{
			Entity = tagChange.Entity,
			Tag = tagChange.Tag,
			Value = tagChange.Value,
			ChangeDef = tagChange.ChangeDef
		};
	}

	private static HistBlockStart GetBlockStart(PowerHistoryStart start)
	{
		return new HistBlockStart(start.Type)
		{
			Entities = new List<int> { start.Source },
			Target = start.Target,
			SubOption = start.SubOption,
			EffectCardId = new List<string> { start.EffectCardId },
			IsEffectCardIdClientCached = new List<bool> { false },
			EffectIndex = start.EffectIndex,
			TriggerKeyword = start.TriggerKeyword,
			ShowInHistory = start.ShowInHistory,
			IsDeferrable = start.IsDeferrable,
			IsBatchable = start.IsBatchable,
			IsDeferBlocker = start.IsDeferBlocker,
			ForceShowBigCard = start.ForceShowBigCard
		};
	}

	private static HistBlockEnd GetBlockEnd(PowerHistoryEnd end)
	{
		return new HistBlockEnd();
	}

	private static HistCreateGame GetCreateGame(PowerHistoryCreateGame createGame)
	{
		return HistCreateGame.CreateFromProto(createGame);
	}

	private static HistResetGame GetResetGame(PowerHistoryResetGame resetGame)
	{
		return HistResetGame.CreateFromProto(resetGame);
	}

	private static HistMetaData GetMetaData(PowerHistoryMetaData metaData)
	{
		HistMetaData result = new HistMetaData();
		result.MetaType = (metaData.HasType ? metaData.Type : HistoryMeta.Type.TARGET);
		result.Data = (metaData.HasData ? metaData.Data : 0);
		for (int i = 0; i < metaData.Info.Count; i++)
		{
			int info = metaData.Info[i];
			result.Info.Add(info);
		}
		for (int j = 0; j < metaData.AdditionalData.Count; j++)
		{
			int additionalData = metaData.AdditionalData[j];
			result.AdditionalData.Add(additionalData);
		}
		return result;
	}

	private static HistSubSpellStart GetSubSpellStart(PowerHistorySubSpellStart subSpellStart)
	{
		return HistSubSpellStart.CreateFromProto(subSpellStart);
	}

	private static HistSubSpellEnd GetSubSpellEnd(PowerHistorySubSpellEnd subSpellEnd)
	{
		return new HistSubSpellEnd();
	}

	private static HistVoSpell GetVoSpell(PowerHistoryVoTask voSubspellTask)
	{
		return HistVoSpell.CreateFromProto(voSubspellTask);
	}

	private static HistVoBanter GetVoBanter(PowerHistoryVoBanter voBanterTask)
	{
		return HistVoBanter.CreateFromProto(voBanterTask);
	}

	private static HistCachedTagForDormantChange GetCachedTagForDormantChange(PowerHistoryCachedTagForDormantChange tagChange)
	{
		return HistCachedTagForDormantChange.CreateFromProto(tagChange);
	}

	private static HistShuffleDeck GetShuffleDeck(PowerHistoryShuffleDeck shuffleDeck)
	{
		return HistShuffleDeck.CreateFromProto(shuffleDeck);
	}

	private static HistTagListChange GetTagListChange(PowerHistoryTagListChange tagListChange)
	{
		return HistTagListChange.CreateFromProto(tagListChange);
	}

	public ServerResult GetServerResult()
	{
		return m_connectApi.GetServerResult();
	}

	public void SendPingTeammateEntity(int entityID, int pingType, bool teammateOwned)
	{
		m_connectApi.SendPingTeammateEntity(entityID, pingType, teammateOwned);
	}

	public void SendNotifyTeammateChooseOne(List<int> subCardIDs)
	{
		m_connectApi.SendNotifyTeammateChooseOne(subCardIDs);
	}

	public void RequestTeammatesGamestate()
	{
		m_connectApi.SendRequestTeammatesGamestate();
	}

	private bool IsInGame()
	{
		return GameState.Get() != null;
	}

	public void RequestSteamUserInfo()
	{
		m_connectApi.RequestSteamUserInfo();
	}

	public SteamPurchaseResponse GetSteamPurchaseResponse()
	{
		return m_connectApi.GetSteamPurcahseResponse();
	}

	public ExternalPurchaseUpdate GetExternalPurchaseUpdate()
	{
		return m_connectApi.GetExternalPurchaseUpdate();
	}

	public GetSteamUserInfoResponse GetGetSteamUserInfoResponse()
	{
		return m_connectApi.GetGetSteamUserInfoResponse();
	}
}
