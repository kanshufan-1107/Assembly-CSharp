using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Assets;
using Blizzard.Commerce;
using Blizzard.T5.Core;
using Blizzard.T5.Core.Utils;
using Blizzard.T5.Jobs;
using Blizzard.T5.MaterialService.Extensions;
using Blizzard.T5.Services;
using Hearthstone;
using Hearthstone.Core;
using Hearthstone.DataModels;
using Hearthstone.Store;
using PegasusUtil;
using UnityEngine;

public class CardBackManager : IService
{
	public class LoadCardBackData
	{
		public delegate void LoadCardBackCallback(LoadCardBackData cardBackData);

		public int m_CardBackIndex;

		public GameObject m_GameObject;

		public CardBack m_CardBack;

		public LoadCardBackCallback m_Callback;

		public string m_Name;

		public string m_Path;

		public CardBackSlot m_Slot;

		public bool m_Unlit;

		public bool m_ShadowActive;

		public object callbackData;
	}

	public class OwnedCardBack
	{
		public int m_cardBackId;

		public string m_name;

		public bool m_owned;

		public bool m_favorited;

		public bool m_canBuy;

		public int m_sortOrder;

		public int m_sortCategory;

		public long m_seasonId = -1L;
	}

	public enum CardBackSlot
	{
		DEFAULT,
		FRIENDLY,
		OPPONENT,
		FAVORITE
	}

	public delegate void UpdateCardbacksCallback();

	public delegate void FavoriteCardBacksChangedCallback(int cardBackId, bool isFavorite);

	private class CardBackSlotData
	{
		public CardBack m_cardBack;

		public string m_cardBackAssetString;

		public bool m_isLoading;
	}

	private class UpdateCardbacksListener : EventListener<UpdateCardbacksCallback>
	{
		public void Fire()
		{
			m_callback();
		}
	}

	private GameObject m_sceneObject;

	private const int CARD_BACK_PRIMARY_MATERIAL_INDEX = 0;

	private const int CARD_BACK_SECONDARY_MATERIAL_INDEX = 1;

	private Map<int, CardBackData> m_cardBackData;

	private Map<string, CardBack> m_LoadedCardBacks;

	private Map<CardBackSlot, CardBackSlotData> m_LoadedCardBacksBySlot;

	private string m_searchText;

	private List<UpdateCardbacksListener> m_updateCardbacksListeners = new List<UpdateCardbacksListener>();

	private readonly object cardbackListenerCollectionLock = new object();

	private bool m_shouldSort = true;

	private List<OwnedCardBack> m_sortedCardBacks;

	private GameObject SceneObject
	{
		get
		{
			if (m_sceneObject == null)
			{
				m_sceneObject = new GameObject("CardBackManagerSceneObject", typeof(HSDontDestroyOnLoad));
			}
			return m_sceneObject;
		}
	}

	public int TheRandomCardBackID { get; private set; }

	public event FavoriteCardBacksChangedCallback OnFavoriteCardBacksChanged;

	public IEnumerator<IAsyncJobResult> Initialize(ServiceLocator serviceLocator)
	{
		HearthstoneApplication.Get().Resetting += Resetting;
		NetCache netCache = serviceLocator.Get<NetCache>();
		netCache.FavoriteCardBackChanged += OnFavoriteCardBackChanged;
		netCache.RegisterUpdatedListener(typeof(NetCache.NetCacheCardBacks), NetCache_OnNetCacheCardBacksUpdated);
		InitCardBackData();
		Options.Get().RegisterChangedListener(Option.CARD_BACK, OnCheatOptionChanged);
		Options.Get().RegisterChangedListener(Option.CARD_BACK2, OnCheatOptionChanged);
		serviceLocator.Get<SceneMgr>().RegisterSceneLoadedEvent(OnSceneLoaded);
		InitCardBackSlots();
		yield break;
	}

	public Type[] GetDependencies()
	{
		return new Type[4]
		{
			typeof(GameDbf),
			typeof(IAssetLoader),
			typeof(NetCache),
			typeof(SceneMgr)
		};
	}

	public void Shutdown()
	{
		if (ServiceManager.TryGet<NetCache>(out var netCache))
		{
			netCache.FavoriteCardBackChanged -= OnFavoriteCardBackChanged;
		}
		HearthstoneApplication hearthstoneApplication = HearthstoneApplication.Get();
		if (hearthstoneApplication != null)
		{
			hearthstoneApplication.Resetting -= Resetting;
		}
	}

	private void Resetting()
	{
		InitCardBackData();
	}

	public static CardBackManager Get()
	{
		return ServiceManager.Get<CardBackManager>();
	}

	public bool RegisterUpdateCardbacksListener(UpdateCardbacksCallback callback)
	{
		UpdateCardbacksListener listener = new UpdateCardbacksListener();
		listener.SetCallback(callback);
		if (m_updateCardbacksListeners.Contains(listener))
		{
			return false;
		}
		lock (cardbackListenerCollectionLock)
		{
			m_updateCardbacksListeners.Add(listener);
		}
		return true;
	}

	public bool UnregisterUpdateCardbacksListener(UpdateCardbacksCallback callback)
	{
		UpdateCardbacksListener listener = new UpdateCardbacksListener();
		listener.SetCallback(callback);
		bool result = false;
		lock (cardbackListenerCollectionLock)
		{
			return m_updateCardbacksListeners.Remove(listener);
		}
	}

	public void SetSearchText(string searchText)
	{
		m_searchText = searchText?.ToLower();
	}

	public CardBack GetFriendlyCardBack()
	{
		return GetCardBackBySlot(CardBackSlot.FRIENDLY);
	}

	public CardBack GetOpponentCardBack()
	{
		return GetCardBackBySlot(CardBackSlot.OPPONENT);
	}

	public CardBack GetCardBackForActor(Actor actor)
	{
		if (IsActorFriendly(actor))
		{
			return GetFriendlyCardBack();
		}
		return GetOpponentCardBack();
	}

	public CardBack GetCardBackBySlot(CardBackSlot slot)
	{
		if (m_LoadedCardBacksBySlot.TryGetValue(slot, out var slotData))
		{
			return slotData.m_cardBack;
		}
		return null;
	}

	public bool IsCardBackLoading(CardBackSlot slot)
	{
		if (m_LoadedCardBacksBySlot.TryGetValue(slot, out var slotData))
		{
			return slotData.m_isLoading;
		}
		return false;
	}

	public void UpdateAllCardBacksInSceneWhenReady()
	{
		Processor.RunCoroutine(UpdateAllCardBacksInSceneWhenReadyImpl());
	}

	public void SetGameCardBackIDs(int friendlyCardBackID, int opponentCardBackID)
	{
		int validFriendlyCardBackID = GetValidCardBackID(friendlyCardBackID);
		LoadCardBackPrefabIntoSlot(m_cardBackData[validFriendlyCardBackID].PrefabName, CardBackSlot.FRIENDLY);
		int validOpponentCardBackID = GetValidCardBackID(opponentCardBackID);
		LoadCardBackPrefabIntoSlot(m_cardBackData[validOpponentCardBackID].PrefabName, CardBackSlot.OPPONENT);
		UpdateAllCardBacksInSceneWhenReady();
	}

	public bool LoadCardBackByIndex(int cardBackIdx, LoadCardBackData.LoadCardBackCallback callback, object callbackData = null)
	{
		string actorName = "Card_Hidden.prefab:1a94649d257bc284ca6e2962f634a8b9";
		return LoadCardBackByIndex(cardBackIdx, callback, unlit: false, actorName, callbackData);
	}

	public bool LoadCardBackByIndex(int cardBackIdx, LoadCardBackData.LoadCardBackCallback callback, string actorName, object callbackData = null)
	{
		return LoadCardBackByIndex(cardBackIdx, callback, unlit: false, actorName, callbackData);
	}

	public bool LoadCardBackByIndex(int cardBackIdx, LoadCardBackData.LoadCardBackCallback callback, bool unlit, string actorName = "Card_Hidden.prefab:1a94649d257bc284ca6e2962f634a8b9", object callbackData = null)
	{
		if (!m_cardBackData.ContainsKey(cardBackIdx))
		{
			Log.CardbackMgr.Print("CardBackManager.LoadCardBackByIndex() - wrong cardBackIdx {0}", cardBackIdx);
			return false;
		}
		LoadCardBackData cardBackCallbackData = new LoadCardBackData();
		cardBackCallbackData.m_CardBackIndex = cardBackIdx;
		cardBackCallbackData.m_Callback = callback;
		cardBackCallbackData.m_Unlit = unlit;
		cardBackCallbackData.m_Name = m_cardBackData[cardBackIdx].Name;
		cardBackCallbackData.callbackData = callbackData;
		if (!AssetLoader.Get().InstantiatePrefab(actorName, OnLoadHiddenActorAttempted, cardBackCallbackData, AssetLoadingOptions.IgnorePrefabPosition))
		{
			OnLoadHiddenActorAttempted(actorName, null, cardBackCallbackData);
		}
		return true;
	}

	public LoadCardBackData LoadCardBackByIndex(int cardBackIdx, bool unlit = false, string actorName = "Card_Hidden.prefab:1a94649d257bc284ca6e2962f634a8b9", bool shadowActive = false)
	{
		if (!m_cardBackData.ContainsKey(cardBackIdx))
		{
			Log.CardbackMgr.Print("CardBackManager.LoadCardBackByIndex() - wrong cardBackIdx {0}", cardBackIdx);
			return null;
		}
		LoadCardBackData callbackData = new LoadCardBackData();
		callbackData.m_CardBackIndex = cardBackIdx;
		callbackData.m_Unlit = unlit;
		callbackData.m_Name = m_cardBackData[cardBackIdx].Name;
		callbackData.m_GameObject = AssetLoader.Get().InstantiatePrefab(actorName, AssetLoadingOptions.IgnorePrefabPosition);
		if (callbackData.m_GameObject == null)
		{
			Log.CardbackMgr.Print("CardBackManager.LoadCardBackByIndex() - failed to load Actor {0}", actorName);
			return null;
		}
		string fileName = m_cardBackData[cardBackIdx].PrefabName;
		GameObject go = AssetLoader.Get().InstantiatePrefab(fileName);
		if (go == null)
		{
			Log.CardbackMgr.Print("CardBackManager.LoadCardBackByIndex() - failed to load CardBack {0}", fileName);
			return null;
		}
		CardBack cardBack = go.GetComponentInChildren<CardBack>();
		if (cardBack == null)
		{
			Debug.LogWarning("CardBackManager.LoadCardBackByIndex() - cardback=null");
			return null;
		}
		callbackData.m_CardBack = cardBack;
		Actor component = callbackData.m_GameObject.GetComponent<Actor>();
		SetCardBack(component.m_cardMesh, callbackData.m_CardBack, callbackData.m_Unlit, shadowActive);
		component.SetCardbackUpdateIgnore(ignoreUpdate: true);
		callbackData.m_CardBack.gameObject.transform.parent = callbackData.m_GameObject.transform;
		return callbackData;
	}

	public static Actor LoadCardBackActorByPrefab(string cardBackPrefab, bool unlit = false, string actorName = "Card_Hidden.prefab:1a94649d257bc284ca6e2962f634a8b9", bool shadowActive = false)
	{
		if (AssetLoader.Get() == null)
		{
			Debug.LogWarning("CardBackManager.LoadCardBackActorByPrefab() - AssetLoader not available");
			return null;
		}
		GameObject go = AssetLoader.Get().InstantiatePrefab(cardBackPrefab);
		if (go == null)
		{
			Log.CardbackMgr.Print("CardBackManager.LoadCardBackActorByPrefab() - failed to load CardBack {0}", cardBackPrefab);
			return null;
		}
		GameObject actorGameObject = AssetLoader.Get().InstantiatePrefab(actorName, AssetLoadingOptions.IgnorePrefabPosition);
		if (actorGameObject == null)
		{
			Log.CardbackMgr.Print("CardBackManager.LoadCardBackActorByPrefab() - failed to load Actor {0}", actorName);
			return null;
		}
		Actor actor = actorGameObject.GetComponent<Actor>();
		CardBack cardBack = go.GetComponentInChildren<CardBack>();
		if (cardBack == null)
		{
			Debug.LogWarning("CardBackManager.LoadCardBackActorByPrefab() - cardback=null");
			return null;
		}
		SetCardBack(actor.m_cardMesh, cardBack, unlit, shadowActive);
		actor.SetCardbackUpdateIgnore(ignoreUpdate: true);
		cardBack.gameObject.transform.parent = actorGameObject.transform;
		return actor;
	}

	public void AddNewCardBack(int cardBackID)
	{
		NetCache.NetCacheCardBacks netCacheCardBacks = GetCardBacks();
		if (netCacheCardBacks == null)
		{
			Debug.LogWarning($"CollectionManager.AddNewCardBack({cardBackID}): trying to access NetCacheCardBacks before it's been loaded");
			return;
		}
		netCacheCardBacks.CardBacks.Add(cardBackID);
		SetCollectionCardBackOwned(cardBackID);
	}

	public void SetCollectionCardBackOwned(int cardBackId)
	{
		if (m_sortedCardBacks != null)
		{
			OwnedCardBack cardBack = m_sortedCardBacks.Find((OwnedCardBack back) => back.m_cardBackId == cardBackId);
			if (cardBack != null)
			{
				cardBack.m_owned = true;
			}
		}
	}

	public void HandleFavoriteToggle(int cardBackId)
	{
		if (MultipleFavoriteCardBacksEnabled())
		{
			RequestSetFavoriteCardBack(cardBackId, !IsCardBackFavorited(cardBackId));
			return;
		}
		foreach (int previousFavoriteId in GetCardBacks().FavoriteCardBacks)
		{
			RequestSetFavoriteCardBack(previousFavoriteId, isFavorite: false);
		}
		RequestSetFavoriteCardBack(cardBackId);
	}

	public void RequestSetFavoriteCardBack(int cardBackID, bool isFavorite = true)
	{
		Network.Get().SetFavoriteCardBack(cardBackID, isFavorite);
	}

	public string GetCardBackName(int cardBackId)
	{
		if (m_cardBackData.TryGetValue(cardBackId, out var cardBackData))
		{
			return cardBackData.Name;
		}
		return null;
	}

	public int GetNumCardBacksOwned()
	{
		NetCache.NetCacheCardBacks netCacheCardBacks = GetCardBacks();
		if (netCacheCardBacks == null)
		{
			Debug.LogWarning("CardBackManager.GetNumCardBacksOwned(): trying to access NetCacheCardBacks before it's been loaded");
			return 0;
		}
		return netCacheCardBacks.CardBacks.Count;
	}

	public HashSet<int> GetCardBacksOwned()
	{
		NetCache.NetCacheCardBacks netCacheCardBacks = GetCardBacks();
		if (netCacheCardBacks == null)
		{
			Debug.LogWarning("CardBackManager.GetCardBacksOwned(): trying to access NetCacheCardBacks before it's been loaded");
			return null;
		}
		return netCacheCardBacks.CardBacks;
	}

	public NetCache.NetCacheCardBacks GetCardBacks()
	{
		NetCache.NetCacheCardBacks netCacheCardBacks = NetCache.Get().GetNetObject<NetCache.NetCacheCardBacks>();
		if (netCacheCardBacks == null)
		{
			return GetCardBacksFromOfflineData();
		}
		return netCacheCardBacks;
	}

	public NetCache.NetCacheCardBacks GetCardBacksFromOfflineData()
	{
		CardBacks offlineCardBacks = OfflineDataCache.GetCardBacksFromCache();
		if (offlineCardBacks == null)
		{
			return null;
		}
		return new NetCache.NetCacheCardBacks
		{
			CardBacks = new HashSet<int>(offlineCardBacks.CardBacks_),
			FavoriteCardBacks = new HashSet<int>(offlineCardBacks.FavoriteCardBacks)
		};
	}

	public HashSet<int> GetCardBackIds(bool requireOwned = false)
	{
		HashSet<int> cardIds = new HashSet<int>();
		GetCardBacksOwned();
		foreach (KeyValuePair<int, CardBackData> kvpair in m_cardBackData)
		{
			if (ShouldIncludeCardBack(kvpair.Value, requireOwned))
			{
				cardIds.Add(kvpair.Key);
			}
		}
		return cardIds;
	}

	public bool IsCardBackOwned(int cardBackID)
	{
		NetCache.NetCacheCardBacks netCacheCardBacks = GetCardBacks();
		if (netCacheCardBacks == null)
		{
			Debug.LogWarning($"CardBackManager.IsCardBackOwned({cardBackID}): trying to access NetCacheCardBacks before it's been loaded");
			return false;
		}
		return netCacheCardBacks.CardBacks.Contains(cardBackID);
	}

	public bool IsCardBackFavorited(int cardBackID)
	{
		NetCache.NetCacheCardBacks netCacheCardBacks = GetCardBacks();
		if (netCacheCardBacks == null)
		{
			Debug.LogWarning($"CardBackManager.IsCardBackFavorited({cardBackID}): trying to access NetCacheCardBacks before it's been loaded");
			return false;
		}
		return netCacheCardBacks.FavoriteCardBacks.Contains(cardBackID);
	}

	public int TotalFavoriteCardBacks()
	{
		NetCache.NetCacheCardBacks netCacheCardBacks = GetCardBacks();
		if (netCacheCardBacks == null)
		{
			Debug.LogWarning($"CardBackManager.TotalFavoriteCardBacks(): trying to access NetCacheCardBacks before it's been loaded");
			return 0;
		}
		return netCacheCardBacks.FavoriteCardBacks.Count;
	}

	public bool CanToggleFavoriteCardBack(int cardBackId)
	{
		bool isOwned = IsCardBackOwned(cardBackId);
		bool isFavorited = IsCardBackFavorited(cardBackId);
		bool playerHasMultipleFavorites = TotalFavoriteCardBacks() > 1;
		if (!MultipleFavoriteCardBacksEnabled())
		{
			if (isOwned)
			{
				return !isFavorited;
			}
			return false;
		}
		if (isOwned)
		{
			return !isFavorited || playerHasMultipleFavorites;
		}
		return false;
	}

	public int GetCollectionManagerCardBackPurchaseProductId(int cardBackId)
	{
		CardBackDbfRecord cardBack = GameDbf.CardBack.GetRecord(cardBackId);
		if (cardBack == null)
		{
			Debug.LogError("CardBackManager:GetCollectionManagerCardBackPurchaseProductId failed to find card back " + cardBackId + " in the CardBack database");
			return 0;
		}
		return cardBack.CollectionManagerPurchaseProductId;
	}

	public bool CanBuyCardBackFromCollectionManager(int cardBackId)
	{
		if (IsCardBackOwned(cardBackId))
		{
			return false;
		}
		if (!IsCardBackPurchasableFromCollectionManager(cardBackId))
		{
			return false;
		}
		if (NetCache.Get().GetGoldBalance() < GetCollectionManagerCardBackGoldCost(cardBackId))
		{
			return false;
		}
		return true;
	}

	public bool IsCardBackPurchasableFromCollectionManager(int cardBackId)
	{
		if (!StoreManager.Get().IsOpen())
		{
			return false;
		}
		if (!StoreManager.Get().IsBuyCardBacksFromCollectionManagerEnabled())
		{
			return false;
		}
		if (GetCollectionManagerCardBackPurchaseProductId(cardBackId) <= 0)
		{
			return false;
		}
		if (GetCollectionManagerCardBackPriceDataModel(cardBackId) == null)
		{
			Debug.LogError("CardBackManager:IsCardBackPurchasableFromCollectionManager failed to get the price data model for Card Back " + cardBackId);
			return false;
		}
		return true;
	}

	public ProductInfo GetCollectionManagerCardBackProductBundle(int cardBackId)
	{
		int pmtProductId = GetCollectionManagerCardBackPurchaseProductId(cardBackId);
		if (!ProductId.IsValid(pmtProductId) || !ServiceManager.TryGet<IProductDataService>(out var dataService))
		{
			return null;
		}
		if (!dataService.TryGetProduct(pmtProductId, out var bundle))
		{
			Debug.LogError("CardBackManager:GetCollectionManagerCardBackProductBundle: Did not find a bundle with pmtProductId " + pmtProductId + " for Card Back " + cardBackId);
			return null;
		}
		if (bundle.Items == null && !bundle.Items.Any((Network.BundleItem x) => x.ItemType == ProductType.PRODUCT_TYPE_CARD_BACK && x.ProductData == cardBackId))
		{
			Debug.LogError("CardBackManager:GetCollectionManagerCardBackProductBundle: Did not find any items with type PRODUCT_TYPE_CARD_BACK for bundle with pmtProductId " + pmtProductId + " for Card Back " + cardBackId);
			return null;
		}
		return bundle;
	}

	public PriceDataModel GetCollectionManagerCardBackPriceDataModel(int cardBackId)
	{
		ProductInfo bundle = GetCollectionManagerCardBackProductBundle(cardBackId);
		if (bundle == null)
		{
			Debug.LogError("CardBackManager:GetCollectionManagerCardBackPriceDataModel failed to get bundle for Card Back " + cardBackId);
			return null;
		}
		if (!bundle.HasCurrency(CurrencyType.GOLD))
		{
			Debug.LogError("CardBackManager:GetCollectionManagerCardBackPriceDataModel bundle for Card Back " + cardBackId + " has no GTAPP gold cost");
			return null;
		}
		return bundle.GetPriceDataModel(CurrencyType.GOLD);
	}

	private long GetCollectionManagerCardBackGoldCost(int cardBackId)
	{
		ProductInfo bundle = GetCollectionManagerCardBackProductBundle(cardBackId);
		if (bundle == null)
		{
			Debug.LogError("CardBackManager:GetCollectionManagerCardBackGoldCost called for a card back with no valid product bundle. Card Back Id = " + cardBackId);
			return 0L;
		}
		if (!bundle.TryGetVCPrice(CurrencyType.GOLD, out var value))
		{
			Debug.LogError("CardBackManager:GetCollectionManagerCardBackGoldCost called for a card back with no gold cost. Card Back Id = " + cardBackId);
			return 0L;
		}
		return value;
	}

	public List<OwnedCardBack> GetPageOfCardBacks(bool requireOwned, int currentPage)
	{
		int cardsPerPage = CollectiblePageDisplay.GetMaxCardsPerPage();
		return GetFilteredCardBacks(requireOwned).Skip(cardsPerPage * (currentPage - 1)).Take(cardsPerPage).ToList();
	}

	public List<OwnedCardBack> GetFilteredCardBacks(bool requireOwned)
	{
		return (from cardBack in GetAllOrderedCardBacks()
			where ShouldIncludeCardBack(cardBack, requireOwned)
			select cardBack).ToList();
	}

	public List<OwnedCardBack> GetAllOrderedCardBacks()
	{
		CollectibleDisplay cd = CollectionManager.Get()?.GetCollectibleDisplay();
		bool sortListenerExists = cd != null && cd.ViewModeChangedListenerExists(OnSwitchViewMode);
		if (!m_shouldSort && m_sortedCardBacks != null && sortListenerExists)
		{
			return m_sortedCardBacks;
		}
		List<OwnedCardBack> cardBacks = new List<OwnedCardBack>();
		foreach (CardBackData cardBack in m_cardBackData.Values)
		{
			if (cardBack.Enabled)
			{
				CardBackDbfRecord cardBackDbf = GameDbf.CardBack.GetRecord(cardBack.ID);
				long seasonId = -1L;
				if (cardBackDbf.Source == Assets.CardBack.Source.SEASON)
				{
					seasonId = cardBackDbf.Data1;
				}
				cardBacks.Add(new OwnedCardBack
				{
					m_cardBackId = cardBack.ID,
					m_name = cardBack.Name,
					m_owned = IsCardBackOwned(cardBack.ID),
					m_favorited = IsCardBackFavorited(cardBack.ID),
					m_canBuy = CanBuyCardBackFromCollectionManager(cardBack.ID),
					m_sortOrder = cardBackDbf.SortOrder,
					m_sortCategory = (int)cardBackDbf.SortCategory,
					m_seasonId = seasonId
				});
			}
		}
		cardBacks.Sort(delegate(OwnedCardBack lhs, OwnedCardBack rhs)
		{
			if (MultipleFavoriteCardBacksEnabled() && lhs.m_favorited != rhs.m_favorited)
			{
				if (!lhs.m_favorited)
				{
					return 1;
				}
				return -1;
			}
			if (lhs.m_owned != rhs.m_owned)
			{
				if (!lhs.m_owned)
				{
					return 1;
				}
				return -1;
			}
			if (lhs.m_canBuy != rhs.m_canBuy)
			{
				if (!lhs.m_canBuy)
				{
					return 1;
				}
				return -1;
			}
			if (lhs.m_sortCategory != rhs.m_sortCategory)
			{
				if (lhs.m_sortCategory >= rhs.m_sortCategory)
				{
					return 1;
				}
				return -1;
			}
			if (lhs.m_sortOrder != rhs.m_sortOrder)
			{
				if (lhs.m_sortOrder >= rhs.m_sortOrder)
				{
					return 1;
				}
				return -1;
			}
			return (lhs.m_seasonId != rhs.m_seasonId) ? ((lhs.m_seasonId <= rhs.m_seasonId) ? 1 : (-1)) : Mathf.Clamp(lhs.m_cardBackId - rhs.m_cardBackId, -1, 1);
		});
		m_sortedCardBacks = cardBacks;
		SetShouldSort(shouldSort: false);
		if (cd != null && !sortListenerExists)
		{
			cd.OnViewModeChanged += OnSwitchViewMode;
		}
		return m_sortedCardBacks;
	}

	private void OnSwitchViewMode(CollectionUtils.ViewMode prevMode, CollectionUtils.ViewMode mode, CollectionUtils.ViewModeData userdata, bool triggerResponse)
	{
		if (mode != CollectionUtils.ViewMode.CARD_BACKS)
		{
			SetShouldSort(shouldSort: true);
		}
	}

	public void SetShouldSort(bool shouldSort)
	{
		m_shouldSort = shouldSort;
	}

	public void SetCardBackTexture(Renderer renderer, int matIdx, CardBackSlot slot)
	{
		if (IsCardBackLoading(slot))
		{
			Processor.RunCoroutine(SetTextureWhenLoaded(renderer, matIdx, slot));
		}
		else
		{
			SetTexture(renderer, matIdx, slot);
		}
	}

	public void SetCardBackMaterial(Renderer renderer, int matIdx, CardBackSlot slot)
	{
		if (IsCardBackLoading(slot))
		{
			Processor.RunCoroutine(SetMaterialWhenLoaded(renderer, matIdx, slot));
		}
		else
		{
			SetMaterial(renderer, matIdx, slot);
		}
	}

	public void UpdateCardBack(Actor actor, CardBack cardBack)
	{
		if (!(actor.gameObject == null) && !(actor.m_cardMesh == null) && !(cardBack == null))
		{
			SetCardBack(actor.m_cardMesh, cardBack);
		}
	}

	public void UpdateCardBackWithInternalCardBack(Actor actor)
	{
		if (!(actor.gameObject == null) && !(actor.m_cardMesh == null))
		{
			CardBack cardBack = actor.gameObject.GetComponentInChildren<CardBack>();
			if (!(cardBack == null))
			{
				SetCardBack(actor.m_cardMesh, cardBack);
			}
		}
	}

	public void UpdateCardBack(GameObject go, CardBackSlot slot)
	{
		if (!(go == null))
		{
			if (IsCardBackLoading(slot))
			{
				Processor.RunCoroutine(SetCardBackWhenLoaded(go, slot));
			}
			else
			{
				SetCardBack(go, slot);
			}
		}
	}

	public void UpdateDeck(GameObject go, CardBackSlot slot)
	{
		if (!(go == null))
		{
			Processor.RunCoroutine(SetDeckCardBackWhenLoaded(go, slot));
		}
	}

	public void UpdateDragEffect(GameObject go, CardBackSlot slot)
	{
		if (!(go == null))
		{
			if (IsCardBackLoading(slot))
			{
				Processor.RunCoroutine(SetDragEffectsWhenLoaded(go, slot));
			}
			else
			{
				SetDragEffects(go, slot);
			}
		}
	}

	public bool IsActorFriendly(Actor actor)
	{
		if (actor == null)
		{
			Log.CardbackMgr.Print("CardBack IsActorFriendly: actor is null!");
			return true;
		}
		Entity entity = actor.GetEntity();
		if (entity != null)
		{
			Player controller = entity.GetController();
			if (controller != null && controller.GetSide() == Player.Side.OPPOSING)
			{
				return false;
			}
		}
		return true;
	}

	public int GetRandomCardBackIdOwnedByPlayer(bool shouldLimitToFavorites = false)
	{
		NetCache.NetCacheCardBacks netCacheCardBacks = GetCardBacks();
		if (netCacheCardBacks == null)
		{
			Debug.LogWarning($"CardBackMaanager.GetRandomCardBackIdOwnedByPlayer({shouldLimitToFavorites}): trying to access NetCacheCardBacks before it's been loaded");
			return 0;
		}
		HashSet<int> obj = (shouldLimitToFavorites ? netCacheCardBacks.FavoriteCardBacks : netCacheCardBacks.CardBacks);
		List<int> cardBackIds = new List<int>();
		foreach (int cardBackId in obj)
		{
			CardBackDbfRecord record = GameDbf.CardBack.GetRecord(cardBackId);
			if (record.Enabled && !record.IsRandomCardBack)
			{
				cardBackIds.Add(cardBackId);
			}
		}
		int randomId = 0;
		if (cardBackIds.Count > 0)
		{
			int randomIndex = UnityEngine.Random.Range(0, cardBackIds.Count);
			randomId = cardBackIds[randomIndex];
		}
		return randomId;
	}

	public void FindCardBackToUse(long deckId, out int cardBackToUse, out int? deckCardBack)
	{
		CollectionDeck deck = CollectionManager.Get()?.GetDeck(deckId);
		deckCardBack = deck?.CardBackID;
		if (deck == null)
		{
			bool shouldLimitToFavorites = !GameUtils.IsGSDFlagSet(GameSaveKeyId.COLLECTION_MANAGER, GameSaveKeySubkeyId.COLLECTION_MANAGER_RANDOM_CARD_BACK_USE_ALL_OWNED);
			cardBackToUse = GetRandomCardBackIdOwnedByPlayer(shouldLimitToFavorites);
		}
		else if (!deckCardBack.HasValue)
		{
			cardBackToUse = GetRandomCardBackIdOwnedByPlayer(shouldLimitToFavorites: true);
		}
		else
		{
			cardBackToUse = deckCardBack.Value;
		}
		CardBackDbfRecord deckCardBackRecord = GameDbf.CardBack.GetRecord(cardBackToUse);
		if (deckCardBackRecord.IsRandomCardBack && deckCardBackRecord.Enabled)
		{
			cardBackToUse = GetRandomCardBackIdOwnedByPlayer();
		}
	}

	public void LoadRandomCardBackIntoFavoriteSlot(bool updateScene)
	{
		if (!ServiceManager.TryGet<GameMgr>(out var gameMgr) || !gameMgr.IsSpectator())
		{
			bool limitToFavorites = !GameUtils.IsGSDFlagSet(GameSaveKeyId.COLLECTION_MANAGER, GameSaveKeySubkeyId.COLLECTION_MANAGER_RANDOM_CARD_BACK_USE_ALL_OWNED);
			int favoriteId = GetRandomCardBackIdOwnedByPlayer(limitToFavorites);
			LoadCardBackIdIntoSlot(favoriteId, CardBackSlot.FAVORITE);
			if (updateScene)
			{
				UpdateAllCardBacksInSceneWhenReady();
			}
		}
	}

	public bool MultipleFavoriteCardBacksEnabled()
	{
		return NetCache.Get().GetNetObject<NetCache.NetCacheFeatures>().Collection.MultipleFavoriteCardBacks;
	}

	private void InitCardBackSlots()
	{
		CardBackData defaultCardBackData = m_cardBackData[0];
		LoadCardBackPrefabIntoSlot(defaultCardBackData.PrefabName, CardBackSlot.DEFAULT);
		if (!Application.isEditor)
		{
			return;
		}
		if (Options.Get().HasOption(Option.CARD_BACK))
		{
			int cardIdx = Options.Get().GetInt(Option.CARD_BACK);
			if (m_cardBackData.ContainsKey(cardIdx))
			{
				LoadCardBackPrefabIntoSlot(m_cardBackData[cardIdx].PrefabName, CardBackSlot.FRIENDLY);
			}
		}
		if (Options.Get().HasOption(Option.CARD_BACK2))
		{
			int cardIdx2 = Options.Get().GetInt(Option.CARD_BACK2);
			if (m_cardBackData.ContainsKey(cardIdx2))
			{
				LoadCardBackPrefabIntoSlot(m_cardBackData[cardIdx2].PrefabName, CardBackSlot.OPPONENT);
			}
		}
	}

	public void InitCardBackData()
	{
		List<CardBackData> cardBacks = new List<CardBackData>();
		foreach (CardBackDbfRecord record in GameDbf.CardBack.GetRecords())
		{
			if (record.IsRandomCardBack)
			{
				TheRandomCardBackID = record.ID;
			}
			else
			{
				cardBacks.Add(new CardBackData(record.ID, record.Source, record.Data1, record.Name, record.Enabled, record.PrefabName));
			}
		}
		m_cardBackData = new Map<int, CardBackData>();
		foreach (CardBackData cardBack in cardBacks)
		{
			m_cardBackData[cardBack.ID] = cardBack;
		}
		m_LoadedCardBacks = new Map<string, CardBack>();
		m_LoadedCardBacksBySlot = new Map<CardBackSlot, CardBackSlotData>();
	}

	private IEnumerator SetTextureWhenLoaded(Renderer renderer, int matIdx, CardBackSlot slot)
	{
		while (IsCardBackLoading(slot))
		{
			yield return null;
		}
		SetTexture(renderer, matIdx, slot);
	}

	private void SetTexture(Renderer renderer, int matIdx, CardBackSlot slot)
	{
		if (renderer == null)
		{
			return;
		}
		int materialsCount = renderer.GetMaterials().Count;
		if (matIdx < 0 || matIdx >= materialsCount)
		{
			Debug.LogWarningFormat("CardBackManager SetTexture(): matIdx {0} is not within the bounds of renderer's materials (count {1})", matIdx, materialsCount);
			return;
		}
		CardBack cardback = GetCardBackBySlot(slot);
		if (!(cardback == null))
		{
			Texture tex = cardback.m_CardBackTexture;
			if (tex == null)
			{
				Debug.LogWarning($"CardBackManager SetTexture(): texture is null!   obj: {renderer.gameObject.name}  slot: {slot}");
			}
			else
			{
				renderer.GetMaterial(matIdx).mainTexture = tex;
			}
		}
	}

	private IEnumerator SetMaterialWhenLoaded(Renderer renderer, int matIdx, CardBackSlot slot)
	{
		while (IsCardBackLoading(slot))
		{
			yield return null;
		}
		SetMaterial(renderer, matIdx, slot);
	}

	private void SetMaterial(Renderer renderer, int matIdx, CardBackSlot slot)
	{
		if (renderer == null)
		{
			return;
		}
		int materialsCount = renderer.GetMaterials().Count;
		if (matIdx < 0 || matIdx >= materialsCount)
		{
			Debug.LogWarningFormat("CardBackManager SetMaterial(): matIdx {0} is not within the bounds of renderer's materials (count {1})", matIdx, materialsCount);
			return;
		}
		CardBack cardback = GetCardBackBySlot(slot);
		if (!(cardback == null))
		{
			Material mat = cardback.m_CardBackMaterial2D;
			if (mat == null)
			{
				SetTexture(renderer, matIdx, slot);
			}
			else
			{
				renderer.SetSharedMaterial(matIdx, mat);
			}
		}
	}

	public Material GetCardBackMaterialFromSlot(CardBackSlot slot)
	{
		CardBack cardback = GetCardBackBySlot(slot);
		if (cardback == null)
		{
			return null;
		}
		return cardback.m_CardBackMaterial;
	}

	private IEnumerator SetCardBackWhenLoaded(GameObject go, CardBackSlot slot)
	{
		while (IsCardBackLoading(slot))
		{
			yield return null;
		}
		SetCardBack(go, slot);
	}

	private void SetCardBack(GameObject go, CardBackSlot slot)
	{
		CardBack cardBack = GetCardBackBySlot(slot);
		if (cardBack == null)
		{
			Debug.LogWarningFormat("CardBackManager SetCardBack(): cardback not loaded for Slot: {0}", slot);
			cardBack = GetCardBackBySlot(CardBackSlot.DEFAULT);
			if (cardBack == null)
			{
				Debug.LogWarning("CardBackManager SetCardBack(): default cardback not loaded");
				return;
			}
		}
		SetCardBack(go, cardBack);
	}

	public static void SetCardBack(GameObject go, CardBack cardBack)
	{
		SetCardBack(go, cardBack, unlit: false, shadowActive: false);
	}

	public static void SetCardBack(GameObject go, CardBack cardBack, bool unlit, bool shadowActive)
	{
		if (cardBack == null)
		{
			Debug.LogWarning("CardBackManager SetCardBack() cardback=null");
			return;
		}
		if (go == null)
		{
			StackTrace strace = new StackTrace();
			Debug.LogWarningFormat("CardBackManager SetCardBack() go=null, cardBack.name={0}, stacktrace=\n{1}", cardBack.name, strace.ToString());
			return;
		}
		Mesh mesh = cardBack.m_CardBackMesh;
		if (mesh != null)
		{
			MeshFilter meshFilter = go.GetComponent<MeshFilter>();
			if (meshFilter != null)
			{
				meshFilter.mesh = mesh;
			}
		}
		else
		{
			Debug.LogWarning("CardBackManager SetCardBack() mesh=null");
		}
		float lightingBlend = 0f;
		if (!unlit && SceneMgr.Get() != null && SceneMgr.Get().GetMode() == SceneMgr.Mode.GAMEPLAY)
		{
			lightingBlend = 1f;
		}
		Material material = cardBack.m_CardBackMaterial;
		Material material2 = cardBack.m_CardBackMaterial1;
		Material[] cardBackMaterials = new Material[(!(material2 != null)) ? 1 : 2];
		cardBackMaterials[0] = material;
		if (material2 != null)
		{
			cardBackMaterials[1] = material2;
		}
		if (cardBackMaterials.Length != 0 && cardBackMaterials[0] != null)
		{
			Renderer renderer = go.GetComponent<Renderer>();
			renderer.SetSharedMaterials(cardBackMaterials);
			List<Material> materials = renderer.GetMaterials();
			float seed = UnityEngine.Random.Range(0f, 1f);
			foreach (Material mat in materials)
			{
				if (!(mat == null))
				{
					if (mat.HasProperty("_Seed") && mat.GetFloat("_Seed") == 0f)
					{
						mat.SetFloat("_Seed", seed);
					}
					if (mat.HasProperty("_AnimTime") && mat.HasProperty("_FrameCount"))
					{
						MaterialPropertyBlock properties = new MaterialPropertyBlock();
						renderer.GetPropertyBlock(properties);
						float animTime = mat.GetFloat("_AnimTime");
						float frameCount = mat.GetFloat("_FrameCount");
						properties.SetFloat("_TimeOffset", Time.timeSinceLevelLoad - frameCount - seed * animTime);
						renderer.SetPropertyBlock(properties);
					}
					if (mat.HasProperty("_LightingBlend"))
					{
						mat.SetFloat("_LightingBlend", lightingBlend);
					}
				}
			}
		}
		else
		{
			Debug.LogWarning("CardBackManager SetCardBack() material=null");
		}
		if (cardBack.cardBackHelper == CardBack.cardBackHelpers.None)
		{
			RemoveCardBackHelper<CardBackHelperBubbleLevel>(go);
		}
		else if (cardBack.cardBackHelper == CardBack.cardBackHelpers.CardBackHelperBubbleLevel)
		{
			AddCardBackHelper<CardBackHelperBubbleLevel>(go);
		}
		Actor actor = GameObjectUtils.FindComponentInThisOrParents<Actor>(go);
		if (actor != null)
		{
			actor.UpdateMissingCardArt();
			actor.EnableCardbackShadow(shadowActive);
			HighlightState highlightState = actor.GetComponentInChildren<HighlightState>();
			if ((bool)highlightState)
			{
				highlightState.m_StaticSilouetteOverride = cardBack.m_CardBackHighlightTexture;
			}
		}
	}

	private bool ShouldIncludeCardBack(CardBackData cardBackData, bool requireOwned)
	{
		if (!cardBackData.Enabled)
		{
			return false;
		}
		return ShouldIncludeCardBack(cardBackData.ID, cardBackData.Name, requireOwned);
	}

	private bool ShouldIncludeCardBack(OwnedCardBack ownedCardBack, bool requireOwned)
	{
		return ShouldIncludeCardBack(ownedCardBack.m_cardBackId, ownedCardBack.m_name, requireOwned);
	}

	private bool ShouldIncludeCardBack(int cardBackId, string cardBackName, bool requireOwned)
	{
		if (requireOwned && !IsCardBackOwned(cardBackId))
		{
			return false;
		}
		if (!string.IsNullOrEmpty(m_searchText))
		{
			string favoriteString = GameStrings.Get("GLUE_COLLECTION_MANAGER_SEARCH_FAVORITE");
			string missingString = GameStrings.Get("GLUE_COLLECTION_MANAGER_SEARCH_MISSING");
			string extraString = GameStrings.Get("GLUE_COLLECTION_MANAGER_SEARCH_EXTRA");
			string[] specialTerms = new string[3] { favoriteString, missingString, extraString };
			string[] lowercaseSearchTermsArray = m_searchText.ToLower().Split(CollectibleFilteredSet<ICollectible>.SearchTokenDelimiters, StringSplitOptions.RemoveEmptyEntries);
			if (lowercaseSearchTermsArray.Contains(extraString))
			{
				return false;
			}
			if (MultipleFavoriteCardBacksEnabled() && lowercaseSearchTermsArray.Contains(favoriteString))
			{
				bool hasNoFavorite = GetCardBacks().FavoriteCardBacks.Count == 0;
				if (!IsCardBackFavorited(cardBackId) && (!hasNoFavorite || cardBackId != 0))
				{
					return false;
				}
			}
			if (lowercaseSearchTermsArray.Contains(missingString) && IsCardBackOwned(cardBackId))
			{
				return false;
			}
			foreach (string term in lowercaseSearchTermsArray)
			{
				if (!specialTerms.Contains(term) && !cardBackName.ToLower().Contains(term))
				{
					return false;
				}
			}
		}
		return true;
	}

	public static T AddCardBackHelper<T>(GameObject go) where T : MonoBehaviour
	{
		RemoveCardBackHelper<T>(go);
		return go.AddComponent<T>();
	}

	public static bool RemoveCardBackHelper<T>(GameObject go) where T : MonoBehaviour
	{
		T[] helperComponents = go.GetComponents<T>();
		if (helperComponents != null)
		{
			T[] array = helperComponents;
			for (int i = 0; i < array.Length; i++)
			{
				UnityEngine.Object.Destroy(array[i]);
			}
			return true;
		}
		return false;
	}

	private IEnumerator SetDragEffectsWhenLoaded(GameObject go, CardBackSlot slot)
	{
		while (IsCardBackLoading(slot))
		{
			yield return null;
		}
		SetDragEffects(go, slot);
	}

	private void SetDragEffects(GameObject go, CardBackSlot slot)
	{
		if (go == null)
		{
			return;
		}
		CardBackDragEffect dragEffect = go.GetComponentInChildren<CardBackDragEffect>();
		if (dragEffect == null)
		{
			return;
		}
		CardBack cardBack = GetCardBackBySlot(slot);
		if (!(cardBack == null))
		{
			if (dragEffect.m_EffectsRoot != null)
			{
				UnityEngine.Object.Destroy(dragEffect.m_EffectsRoot);
			}
			if (!(cardBack.m_DragEffect == null))
			{
				GameObject effectsObject = (dragEffect.m_EffectsRoot = UnityEngine.Object.Instantiate(cardBack.m_DragEffect));
				effectsObject.transform.parent = dragEffect.gameObject.transform;
				effectsObject.transform.localPosition = Vector3.zero;
				effectsObject.transform.localRotation = Quaternion.identity;
				effectsObject.transform.localScale = Vector3.one;
			}
		}
	}

	private IEnumerator SetDeckCardBackWhenLoaded(GameObject cardBackDeckDisplay, CardBackSlot slot)
	{
		while (IsCardBackLoading(slot))
		{
			yield return null;
		}
		SetDeckCardBack(cardBackDeckDisplay, slot);
	}

	private void SetDeckCardBack(GameObject cardBackDeckDisplay, CardBackSlot slot)
	{
		if (cardBackDeckDisplay == null)
		{
			Debug.LogWarning("CardBackManager SetDeckCardBack(): cardBackDeckDisplay GameObject is null! GameObject could have been destroyed while card back was loading.");
			return;
		}
		CardBack cardBack = GetCardBackBySlot(slot);
		if (cardBack == null)
		{
			Debug.LogWarning("CardBackManager SetDeckCardBack(): cardBack is null!");
			return;
		}
		ZoneDeck deck = cardBackDeckDisplay.GetComponentInParent<ZoneDeck>();
		if (deck != null)
		{
			if (cardBack.GetCustomDeckMeshes(out var customDeckMeshes))
			{
				deck.UpdateToCustomDeckMeshes(customDeckMeshes);
			}
			else
			{
				deck.TryRestoreOriginalDeckMeshes();
			}
		}
		Texture tex = cardBack.m_CardBackTexture;
		if (tex == null)
		{
			Debug.LogWarning("CardBackManager SetDeckCardBack(): texture is null!");
			return;
		}
		Renderer[] componentsInChildren = cardBackDeckDisplay.GetComponentsInChildren<Renderer>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].GetMaterial().mainTexture = tex;
		}
	}

	private void OnCheatOptionChanged(Option option, object prevValue, bool existed, object userData)
	{
		Log.CardbackMgr.Print("Cheat Option Change Called");
		int cbIdx = Options.Get().GetInt(option, 0);
		if (m_cardBackData.ContainsKey(cbIdx))
		{
			CardBackSlot slot = CardBackSlot.FRIENDLY;
			if (option == Option.CARD_BACK2)
			{
				slot = CardBackSlot.OPPONENT;
			}
			LoadCardBackPrefabIntoSlot(m_cardBackData[cbIdx].PrefabName, slot);
			UpdateAllCardBacksInSceneWhenReady();
		}
	}

	private void NetCache_OnNetCacheCardBacksUpdated()
	{
		Processor.RunCoroutine(HandleNetCacheCardBacksWhenReady());
	}

	private IEnumerator HandleNetCacheCardBacksWhenReady()
	{
		while (m_cardBackData == null || FixedRewardsMgr.Get() == null || !FixedRewardsMgr.Get().IsStartupFinished())
		{
			yield return null;
		}
		NetCache.NetCacheCardBacks netCacheCardBacks = GetCardBacks();
		AddNewCardBack(0);
		bool netCacheContainsValidCardback = false;
		foreach (int cardBackId in netCacheCardBacks.FavoriteCardBacks)
		{
			if (m_cardBackData.ContainsKey(cardBackId))
			{
				netCacheContainsValidCardback = true;
				break;
			}
		}
		if (!netCacheContainsValidCardback)
		{
			Log.CardbackMgr.Print("No valid favorite card backs found, set to CardBackDbId.CLASSIC");
			netCacheCardBacks.FavoriteCardBacks = new HashSet<int> { 0 };
		}
		LoadRandomCardBackIntoFavoriteSlot(updateScene: false);
	}

	private IEnumerator UpdateAllCardBacksInSceneWhenReadyImpl()
	{
		while (IsCardBackLoading(CardBackSlot.FRIENDLY) || IsCardBackLoading(CardBackSlot.OPPONENT) || IsCardBackLoading(CardBackSlot.FAVORITE))
		{
			yield return null;
		}
		lock (cardbackListenerCollectionLock)
		{
			foreach (UpdateCardbacksListener updateCardbacksListener in m_updateCardbacksListeners)
			{
				updateCardbacksListener.Fire();
			}
		}
	}

	private void LoadCardBackPrefabIntoSlot(AssetReference assetRef, CardBackSlot slot)
	{
		string cardBackAssetString = assetRef.ToString();
		if (!m_LoadedCardBacksBySlot.TryGetValue(slot, out var cardBackSlotData))
		{
			cardBackSlotData = new CardBackSlotData();
			m_LoadedCardBacksBySlot[slot] = cardBackSlotData;
		}
		if (m_LoadedCardBacks.ContainsKey(cardBackAssetString))
		{
			if (!(m_LoadedCardBacks[cardBackAssetString] == null))
			{
				cardBackSlotData.m_isLoading = false;
				cardBackSlotData.m_cardBackAssetString = cardBackAssetString;
				cardBackSlotData.m_cardBack = m_LoadedCardBacks[cardBackAssetString];
				return;
			}
			m_LoadedCardBacks.Remove(cardBackAssetString);
		}
		if (!(cardBackSlotData.m_cardBackAssetString == cardBackAssetString))
		{
			cardBackSlotData.m_isLoading = true;
			cardBackSlotData.m_cardBackAssetString = cardBackAssetString;
			cardBackSlotData.m_cardBack = null;
			LoadCardBackData data = new LoadCardBackData();
			data.m_Slot = slot;
			data.m_Path = cardBackAssetString;
			if (!AssetLoader.Get().InstantiatePrefab(cardBackAssetString, OnLoadCardBackAttempted, data))
			{
				OnLoadCardBackAttempted(cardBackAssetString, null, data);
			}
		}
	}

	private void OnLoadCardBackAttempted(AssetReference assetRef, GameObject go, object callbackData)
	{
		LoadCardBackData data = callbackData as LoadCardBackData;
		if (go == null)
		{
			Debug.LogWarningFormat("CardBackManager OnCardBackLoaded(): Failed to load CardBack: {0} For: {1}", assetRef, data.m_Slot);
			m_LoadedCardBacksBySlot.Remove(data.m_Slot);
			return;
		}
		go.transform.parent = SceneObject.transform;
		go.transform.position = new Vector3(1000f, -1000f, -1000f);
		CardBack cardBack = go.GetComponent<CardBack>();
		if (cardBack == null)
		{
			Debug.LogWarningFormat("CardBackManager OnCardBackLoaded(): Failed to find CardBack component: {0} slot: {1}", data.m_Path, data.m_Slot);
			return;
		}
		if (cardBack.m_CardBackMesh == null)
		{
			Debug.LogWarningFormat("CardBackManager OnCardBackLoaded(): cardBack.m_CardBackMesh in null! - {0}", data.m_Path);
			return;
		}
		if (cardBack.m_CardBackMaterial == null)
		{
			Debug.LogWarningFormat("CardBackManager OnCardBackLoaded(): cardBack.m_CardBackMaterial in null! - {0}", data.m_Path);
			return;
		}
		if (cardBack.m_CardBackTexture == null)
		{
			Debug.LogWarningFormat("CardBackManager OnCardBackLoaded(): cardBack.m_CardBackTexture in null! - {0}", data.m_Path);
			return;
		}
		m_LoadedCardBacks[data.m_Path] = cardBack;
		if (m_LoadedCardBacksBySlot.TryGetValue(data.m_Slot, out var cardBackSlotData))
		{
			cardBackSlotData.m_isLoading = false;
			cardBackSlotData.m_cardBack = cardBack;
		}
	}

	private void OnLoadHiddenActorAttempted(AssetReference assetRef, GameObject go, object callbackData)
	{
		LoadCardBackData obj = (LoadCardBackData)callbackData;
		int cardBackIndex = obj.m_CardBackIndex;
		string fileName = m_cardBackData[cardBackIndex].PrefabName;
		obj.m_GameObject = go;
		AssetLoader.Get().InstantiatePrefab(fileName, OnHiddenActorCardBackLoaded, callbackData);
	}

	private void OnHiddenActorCardBackLoaded(AssetReference assetRef, GameObject go, object callbackData)
	{
		if (go == null)
		{
			Error.AddDevWarning("Error", "CardBackManager OnHiddenActorCardBackLoaded() path={0}, gameobject=null", assetRef);
			return;
		}
		CardBack cardBack = go.GetComponentInChildren<CardBack>();
		if (cardBack == null)
		{
			Debug.LogWarningFormat("CardBackManager OnHiddenActorCardBackLoaded() path={0}, gameobject={1}, cardback=null", assetRef, go.name);
		}
		else
		{
			LoadCardBackData data = (LoadCardBackData)callbackData;
			data.m_CardBack = cardBack;
			Processor.RunCoroutine(HiddenActorCardBackLoadedSetup(data));
		}
	}

	private IEnumerator HiddenActorCardBackLoadedSetup(LoadCardBackData data)
	{
		yield return null;
		yield return null;
		if (data != null && !(data.m_GameObject == null))
		{
			SetCardBack(data.m_GameObject.GetComponent<Actor>().m_cardMesh, data.m_CardBack, data.m_Unlit, data.m_ShadowActive);
			data.m_CardBack.gameObject.transform.parent = data.m_GameObject.transform;
			data.m_Callback(data);
		}
	}

	private int GetValidCardBackID(int cardBackID)
	{
		if (!m_cardBackData.ContainsKey(cardBackID))
		{
			Log.CardbackMgr.Print("Cardback ID {0} not found, defaulting to Classic", cardBackID);
			return 0;
		}
		return cardBackID;
	}

	public void OnFavoriteCardBackChanged(int newFavoriteCardBackID, bool isFavorite)
	{
		LoadRandomCardBackIntoFavoriteSlot(updateScene: false);
		this.OnFavoriteCardBacksChanged?.Invoke(newFavoriteCardBackID, isFavorite);
	}

	private void OnSceneLoaded(SceneMgr.Mode mode, PegasusScene scene, object userData)
	{
		if (GetCardBackBySlot(CardBackSlot.FRIENDLY) == null)
		{
			LoadCardBackIdIntoSlot(0, CardBackSlot.FRIENDLY);
		}
	}

	private void LoadCardBackIdIntoSlot(int cardBackId, CardBackSlot slot)
	{
		int validCardBackId = GetValidCardBackID(cardBackId);
		if (m_cardBackData.TryGetValue(validCardBackId, out var cardBackData))
		{
			LoadCardBackPrefabIntoSlot(cardBackData.PrefabName, slot);
		}
	}
}
