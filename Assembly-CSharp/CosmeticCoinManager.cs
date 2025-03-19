using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Blizzard.T5.Core;
using Blizzard.T5.Jobs;
using Blizzard.T5.Services;
using Hearthstone;
using Hearthstone.Core;
using Hearthstone.DataModels;
using Hearthstone.Progression;
using Hearthstone.UI;
using PegasusUtil;
using UnityEngine;

public class CosmeticCoinManager : IService
{
	public delegate void FavoriteCoinsChangedCallback(int cardBackId, bool isFavorite);

	private List<CollectibleCard> m_coinCards = new List<CollectibleCard>();

	private Map<int, int> m_cardIdCoinIdMap = new Map<int, int>();

	private string m_searchText;

	private bool m_shouldSort = true;

	private List<CollectibleCard> m_sortedCoinCards;

	private Widget m_coinPreviewWidget;

	public static readonly AssetReference COIN_PREVIEW_PREFAB = new AssetReference("CoinPreview.prefab:4c9e68cbb43064f4287a44286773f026");

	public event FavoriteCoinsChangedCallback OnFavoriteCoinsChanged;

	public Type[] GetDependencies()
	{
		return new Type[2]
		{
			typeof(NetCache),
			typeof(SceneMgr)
		};
	}

	public IEnumerator<IAsyncJobResult> Initialize(ServiceLocator serviceLocator)
	{
		HearthstoneApplication.Get().Resetting += Resetting;
		NetCache netCache = serviceLocator.Get<NetCache>();
		netCache.FavoriteCoinChanged += OnFavoriteCoinChanged;
		netCache.RegisterUpdatedListener(typeof(NetCache.NetCacheCoins), NetCache_OnNetCacheCoinsUpdated);
		serviceLocator.Get<Network>().RegisterNetHandler(CoinUpdate.PacketID.ID, ReceiveCoinUpdateMessage);
		InitCoinData();
		yield break;
	}

	public void Shutdown()
	{
		if (ServiceManager.TryGet<NetCache>(out var netCache))
		{
			netCache.FavoriteCoinChanged -= OnFavoriteCoinChanged;
		}
		HearthstoneApplication hearthstoneApp = HearthstoneApplication.Get();
		if (hearthstoneApp != null)
		{
			hearthstoneApp.Resetting -= Resetting;
		}
	}

	private void Resetting()
	{
		InitCoinData();
	}

	public static CosmeticCoinManager Get()
	{
		return ServiceManager.Get<CosmeticCoinManager>();
	}

	private void InitCoinData()
	{
		Processor.RunCoroutine(InitCoinDataWhenReady());
	}

	private bool ShouldIncludeCoin(CollectibleCard coinCard, bool requireOwned)
	{
		if (requireOwned && !IsOwnedCoinCard(coinCard.CardId))
		{
			return false;
		}
		if (string.IsNullOrEmpty(m_searchText))
		{
			return true;
		}
		if (!m_cardIdCoinIdMap.TryGetValue(coinCard.CardDbId, out var coinId))
		{
			Log.CoinManager.PrintWarning("ShouldIncludeCoin: Coin id for card not found.");
			return false;
		}
		CosmeticCoinDbfRecord coinRecord = GameDbf.CosmeticCoin.GetRecord(coinId);
		if (coinRecord == null)
		{
			Log.CoinManager.PrintWarning("ShouldIncludeCoin: Coin record not found.");
			return false;
		}
		string favoriteString = GameStrings.Get("GLUE_COLLECTION_MANAGER_SEARCH_FAVORITE");
		string missingString = GameStrings.Get("GLUE_COLLECTION_MANAGER_SEARCH_MISSING");
		string extraString = GameStrings.Get("GLUE_COLLECTION_MANAGER_SEARCH_EXTRA");
		string[] specialTerms = new string[3] { favoriteString, missingString, extraString };
		string[] lowercaseSearchTermsArray = m_searchText.ToLower().Split(CollectibleFilteredSet<ICollectible>.SearchTokenDelimiters, StringSplitOptions.RemoveEmptyEntries);
		if (lowercaseSearchTermsArray.Contains(extraString))
		{
			return false;
		}
		if (lowercaseSearchTermsArray.Contains(favoriteString) && !IsFavoriteCoin(coinId))
		{
			return false;
		}
		if (lowercaseSearchTermsArray.Contains(missingString) && IsOwnedCoinCard(coinCard.CardId))
		{
			return false;
		}
		foreach (string term in lowercaseSearchTermsArray)
		{
			if (!specialTerms.Contains(term) && !coinRecord.Name.GetString().ToLower().Contains(term))
			{
				return false;
			}
		}
		return true;
	}

	private IEnumerator InitCoinDataWhenReady()
	{
		DefLoader defLoader = DefLoader.Get();
		while (!defLoader.HasLoadedEntityDefs())
		{
			yield return null;
		}
		NetCache.NetCacheCoins netCacheCoins = GetCoins();
		if (netCacheCoins == null)
		{
			yield break;
		}
		AddNewCoin(1);
		if (netCacheCoins.FavoriteCoins.Count == 0)
		{
			netCacheCoins.FavoriteCoins.Add(1);
		}
		m_coinCards.Clear();
		m_cardIdCoinIdMap.Clear();
		foreach (CosmeticCoinDbfRecord record in GameDbf.CosmeticCoin.GetRecords())
		{
			CardDbfRecord cardRecord = record.CardRecord;
			EntityDef entityDef = defLoader.GetEntityDef(cardRecord.NoteMiniGuid);
			CollectibleCard card = new CollectibleCard(cardRecord, entityDef, TAG_PREMIUM.NORMAL);
			m_coinCards.Add(card);
			if (m_cardIdCoinIdMap.ContainsKey(card.CardDbId))
			{
				Log.CoinManager.PrintError($"Duplicate coin found: Coin:{record.ID} Card:{card.CardDbId} Existing:{m_cardIdCoinIdMap[card.CardDbId]}");
			}
			else
			{
				m_cardIdCoinIdMap.Add(card.CardDbId, record.ID);
			}
		}
		UpdateCoinCards();
	}

	public int GetCoinPageCount(int coinsPerPage)
	{
		return Mathf.CeilToInt((float)GetFilteredCoins().Count / (float)coinsPerPage);
	}

	public List<CollectibleCard> GetPageOfCoinCards(int currentPage, int coinsPerPage)
	{
		int pageCount = GetCoinPageCount(coinsPerPage);
		currentPage = Mathf.Min(currentPage, pageCount);
		return GetFilteredCoins().Skip(coinsPerPage * (currentPage - 1)).Take(coinsPerPage).ToList();
	}

	public List<CollectibleCard> GetFilteredCoins()
	{
		bool requireOwned = CollectionManager.Get().IsInEditMode();
		return (from coinCard in GetOrderedCoinCards()
			where ShouldIncludeCoin(coinCard, requireOwned)
			select coinCard).ToList();
	}

	public List<CollectibleCard> GetOrderedCoinCards()
	{
		CollectibleDisplay cd = CollectionManager.Get()?.GetCollectibleDisplay();
		bool sortListenerExists = cd != null && cd.ViewModeChangedListenerExists(OnSwitchViewMode);
		if (!m_shouldSort && m_sortedCoinCards != null && sortListenerExists)
		{
			return m_sortedCoinCards;
		}
		List<CollectibleCard> coinCards = new List<CollectibleCard>(m_coinCards);
		GetCoinsOwned();
		coinCards.Sort(delegate(CollectibleCard lhs, CollectibleCard rhs)
		{
			if (CardBackManager.Get().MultipleFavoriteCardBacksEnabled())
			{
				bool flag = IsFavoriteCoinCard(lhs.CardId);
				bool flag2 = IsFavoriteCoinCard(rhs.CardId);
				if (flag != flag2)
				{
					if (!flag)
					{
						return 1;
					}
					return -1;
				}
			}
			bool flag3 = IsOwnedCoinCard(lhs.CardId);
			bool flag4 = IsOwnedCoinCard(rhs.CardId);
			if (flag3 != flag4)
			{
				if (!flag3)
				{
					return 1;
				}
				return -1;
			}
			CardSetDbfRecord cardSet = GameDbf.GetIndex().GetCardSet(lhs.Set);
			CardSetDbfRecord cardSet2 = GameDbf.GetIndex().GetCardSet(rhs.Set);
			if (cardSet != null && cardSet2 != null)
			{
				int num = cardSet.ReleaseOrder.CompareTo(cardSet2.ReleaseOrder);
				if (num != 0)
				{
					return cardSet.ReleaseOrder.CompareTo(cardSet2.ReleaseOrder);
				}
				return num;
			}
			m_cardIdCoinIdMap.TryGetValue(lhs.CardDbId, out var value);
			m_cardIdCoinIdMap.TryGetValue(rhs.CardDbId, out var value2);
			return value.CompareTo(value2);
		});
		m_sortedCoinCards = coinCards;
		SetShouldSort(shouldSort: false);
		if (cd != null && !sortListenerExists)
		{
			cd.OnViewModeChanged += OnSwitchViewMode;
		}
		return m_sortedCoinCards;
	}

	private void OnSwitchViewMode(CollectionUtils.ViewMode prevMode, CollectionUtils.ViewMode mode, CollectionUtils.ViewModeData userdata, bool triggerResponse)
	{
		if (mode != CollectionUtils.ViewMode.COINS)
		{
			SetShouldSort(shouldSort: true);
		}
	}

	public void SetShouldSort(bool shouldSort)
	{
		m_shouldSort = shouldSort;
	}

	public bool IsFavoriteCoin(int coinId)
	{
		return GetCoins()?.FavoriteCoins.Contains(coinId) ?? (coinId == 1);
	}

	public bool IsFavoriteCoinCard(string coinCard)
	{
		int coinId = GetCoinIdFromCoinCard(coinCard);
		return IsFavoriteCoin(coinId);
	}

	public int GetTotalFavoriteCoins()
	{
		return GetCoins()?.FavoriteCoins.Count ?? 1;
	}

	public int GetCoinIdFromCoinCard(string coinCard)
	{
		foreach (CollectibleCard card in m_coinCards)
		{
			if (card.CardId == coinCard)
			{
				return m_cardIdCoinIdMap[card.CardDbId];
			}
		}
		return -1;
	}

	public string GetCoinCardFromCoinId(int coinId)
	{
		foreach (CollectibleCard card in m_coinCards)
		{
			if (m_cardIdCoinIdMap[card.CardDbId] == coinId)
			{
				return card.CardId;
			}
		}
		Log.CoinManager.PrintWarning("GetCoinCardFromCoinId(): Coin card could not be found for ID " + coinId);
		return null;
	}

	public void UpdateCoinCards()
	{
		HashSet<int> ownedCoinIds = GetCoinsOwned();
		if (ownedCoinIds == null)
		{
			return;
		}
		foreach (CollectibleCard card in m_coinCards)
		{
			int coinId = m_cardIdCoinIdMap[card.CardDbId];
			card.OwnedCount = (ownedCoinIds.Contains(coinId) ? 1 : 0);
		}
	}

	public void AddNewCoin(int coinId)
	{
		NetCache.NetCacheCoins netCacheCoins = GetCoins();
		if (netCacheCoins == null)
		{
			Log.CoinManager.PrintWarning($"AddNewCoin({coinId}): trying to access NetCacheCoins before it's been loaded");
		}
		else
		{
			netCacheCoins.Coins.Add(coinId);
		}
	}

	public void RequestSetFavoriteCosmeticCoin(int newFavoriteCoinID, bool isFavorite)
	{
		OfflineDataCache.OfflineData data = OfflineDataCache.ReadOfflineDataFromFile();
		Network.Get().SetFavoriteCosmeticCoin(ref data, newFavoriteCoinID, isFavorite);
		OfflineDataCache.WriteOfflineDataToFile(data);
	}

	public void OnFavoriteCoinChanged(int newFavoriteCoinID, bool isFavorite)
	{
		Log.CoinManager.Print(string.Format("CoinManager - Favorite Coin Changed" + $" ID: {newFavoriteCoinID}" + $" Favorite: {isFavorite}"));
		this.OnFavoriteCoinsChanged?.Invoke(newFavoriteCoinID, isFavorite);
	}

	private void NetCache_OnNetCacheCoinsUpdated()
	{
		InitCoinData();
	}

	private void ReceiveCoinUpdateMessage()
	{
		CoinUpdate updateMessage = Network.Get().GetCoinUpdate();
		if (updateMessage == null)
		{
			return;
		}
		NetCache.NetCacheCoins netCacheCoins = NetCache.Get().GetNetObject<NetCache.NetCacheCoins>();
		if (netCacheCoins == null)
		{
			return;
		}
		foreach (int coinId in updateMessage.AddCoinId)
		{
			netCacheCoins.Coins.Add(coinId);
			Log.CoinManager.Print(string.Format($"CoinManager - Coin added. ID: {coinId}"));
		}
		foreach (int coinId2 in updateMessage.RemoveCoinId)
		{
			netCacheCoins.Coins.Remove(coinId2);
			Log.CoinManager.Print(string.Format($"CoinManager - Coin removed. ID: {coinId2}"));
		}
		if (updateMessage.HasFavoriteCoinId)
		{
			netCacheCoins.FavoriteCoins.Add(updateMessage.FavoriteCoinId);
			Log.CoinManager.Print(string.Format("CoinManager - Coin Favorited. " + $"ID: {updateMessage.FavoriteCoinId}"));
		}
		if (updateMessage.HasUnfavoriteCoinId)
		{
			netCacheCoins.FavoriteCoins.Remove(updateMessage.UnfavoriteCoinId);
			Log.CoinManager.Print(string.Format("CoinManager - Coin Unfavorited. " + $"ID: {updateMessage.UnfavoriteCoinId}"));
		}
		UpdateCoinCards();
	}

	private NetCache.NetCacheCoins GetCoinsFromOfflineData()
	{
		CosmeticCoins offlineCoins = OfflineDataCache.GetCoinsFromCache();
		if (offlineCoins == null)
		{
			return null;
		}
		return new NetCache.NetCacheCoins
		{
			Coins = new HashSet<int>(offlineCoins.Coins),
			FavoriteCoins = new HashSet<int>(offlineCoins.FavoriteCoins)
		};
	}

	public NetCache.NetCacheCoins GetCoins()
	{
		NetCache.NetCacheCoins netCacheCoins = NetCache.Get().GetNetObject<NetCache.NetCacheCoins>();
		if (netCacheCoins == null)
		{
			return GetCoinsFromOfflineData();
		}
		return netCacheCoins;
	}

	public HashSet<int> GetCoinsOwned()
	{
		NetCache.NetCacheCoins netCacheCoins = GetCoins();
		if (netCacheCoins == null)
		{
			Log.CoinManager.PrintWarning("GetCoinsOwned: Trying to access NetCacheCoins before it's been loaded");
			return null;
		}
		return netCacheCoins.Coins;
	}

	public int GetTotalCoinsOwned()
	{
		return GetCoinsOwned()?.Count ?? 1;
	}

	public bool IsOwnedCoinCard(string cardId)
	{
		CardDbfRecord cardRecord = GameUtils.GetCardRecord(cardId);
		if (cardRecord == null)
		{
			Log.CoinManager.PrintWarning("IsCoinCardOwned: Card record not found.");
			return false;
		}
		if (!m_cardIdCoinIdMap.TryGetValue(cardRecord.ID, out var coinId))
		{
			Log.CoinManager.PrintWarning("IsCoinCardOwned: Coin id for card not found.");
			return false;
		}
		return GetCoinsOwned()?.Contains(coinId) ?? false;
	}

	public void ShowCoinPreview(string cardId, Transform startTransform)
	{
		if (m_coinPreviewWidget != null)
		{
			return;
		}
		CardDbfRecord cardRecord = GameUtils.GetCardRecord(cardId);
		if (cardRecord == null)
		{
			Log.CoinManager.PrintWarning("ShowCoinPreview: Card record not found.");
			return;
		}
		if (!m_cardIdCoinIdMap.TryGetValue(cardRecord.ID, out var coinId))
		{
			Log.CoinManager.PrintWarning("ShowCoinPreview: Coin id for card not found.");
			return;
		}
		CosmeticCoinDbfRecord coinRecord = GameDbf.CosmeticCoin.GetRecord(coinId);
		if (coinRecord == null)
		{
			Log.CoinManager.PrintWarning("ShowCoinPreview: Coin record not found.");
			return;
		}
		m_coinPreviewWidget = WidgetInstance.Create(COIN_PREVIEW_PREFAB);
		m_coinPreviewWidget.RegisterReadyListener(delegate
		{
			m_coinPreviewWidget.GetComponentInChildren<CoinPreview>().EnterPreviewWhenReady(new CardDataModel
			{
				CardId = cardRecord.NoteMiniGuid,
				Name = coinRecord.Name,
				FlavorText = cardRecord.FlavorText,
				Premium = TAG_PREMIUM.NORMAL,
				ArtistCredit = (string.IsNullOrWhiteSpace(coinRecord.CardRecord.ArtistName) ? string.Empty : GameStrings.Format("GLUE_COLLECTION_ARTIST", coinRecord.CardRecord.ArtistName))
			}, coinId, startTransform);
		});
	}

	public void SetSearchText(string searchText)
	{
		m_searchText = searchText?.ToLower();
	}

	public void FindCoinToUse(long deckId, out int cosmeticCoinToUse, out int? deckCosmeticCoin)
	{
		CollectionDeck deck = CollectionManager.Get()?.GetDeck(deckId);
		deckCosmeticCoin = deck?.CosmeticCoinID;
		if (deck == null)
		{
			bool shouldLimitToFavorites = !GameUtils.IsGSDFlagSet(GameSaveKeyId.COLLECTION_MANAGER, GameSaveKeySubkeyId.COLLECTION_MANAGER_RANDOM_COSMETIC_COIN_USE_ALL_OWNED);
			cosmeticCoinToUse = GetRandomCoinIdOwnedByPlayer(shouldLimitToFavorites);
		}
		else if (!deckCosmeticCoin.HasValue)
		{
			cosmeticCoinToUse = GetRandomCoinIdOwnedByPlayer(deck.RandomCoinUseFavorite);
		}
		else
		{
			cosmeticCoinToUse = deckCosmeticCoin.Value;
		}
	}

	public int GetRandomCoinIdOwnedByPlayer(bool shouldLimitToFavorites)
	{
		NetCache.NetCacheCoins netCacheCoins = GetCoins();
		if (netCacheCoins == null)
		{
			Debug.LogWarning($"CosmeticCoinManager.GetRandomCoinIdOwnedByPlayer({shouldLimitToFavorites}): trying to access NetCacheCoins before it's been loaded");
			return 1;
		}
		HashSet<int> obj = (shouldLimitToFavorites ? netCacheCoins.FavoriteCoins : netCacheCoins.Coins);
		List<int> coinIds = new List<int>();
		foreach (int coinId in obj)
		{
			if (GameDbf.CosmeticCoin.GetRecord(coinId).Enabled)
			{
				coinIds.Add(coinId);
			}
		}
		int randomId = 1;
		if (coinIds.Count > 0)
		{
			int randomIndex = UnityEngine.Random.Range(0, coinIds.Count);
			randomId = coinIds[randomIndex];
		}
		return randomId;
	}
}
