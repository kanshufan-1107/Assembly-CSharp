using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Blizzard.GameService.SDK.Client.Integration;
using Hearthstone;
using Hearthstone.Util;
using PegasusShared;
using PegasusUtil;
using UnityEngine;

public static class OfflineDataCache
{
	public class OfflineData
	{
		public int UniqueFakeDeckId = -999;

		public List<long> FakeDeckIds;

		public List<DeckInfo> OriginalDeckList;

		public List<DeckInfo> LocalDeckList;

		public List<DeckContents> OriginalDeckContents;

		public List<DeckContents> LocalDeckContents;

		public bool m_hasChangedFavoriteHeroesOffline;

		public List<FavoriteHero> FavoriteHeroes;

		public bool m_hasChangedCardBacksOffline;

		public CardBacks CardBacks;

		public Collection Collection;

		public bool m_hasChangedCoinsOffline;

		public CosmeticCoins CosmeticCoins;
	}

	public static void CacheLocalAndOriginalDeckList(ref OfflineData data, List<DeckInfo> localDecklist, List<DeckInfo> originalDecklist)
	{
		data.LocalDeckList = localDecklist;
		data.OriginalDeckList = originalDecklist;
		Log.Offline.PrintDebug("OfflineDataCache: Caching local deck list. Local Count={0}, Original Count={1}", localDecklist.Count, originalDecklist.Count);
	}

	public static void CacheLocalAndOriginalDeckList(List<DeckInfo> localDecklist, List<DeckInfo> originalDecklist)
	{
		OfflineData offlineData = ReadOfflineDataFromFile();
		offlineData.LocalDeckList = localDecklist;
		offlineData.OriginalDeckList = originalDecklist;
		WriteOfflineDataToFile(offlineData);
		Log.Offline.PrintDebug("OfflineDataCache: Caching local deck list. Local Count={0}, Original Count={1}", localDecklist.Count, originalDecklist.Count);
	}

	public static void CacheLocalAndOriginalDeckContents(ref OfflineData data, DeckContents localDeckContents, DeckContents originalDeckContents)
	{
		SetLocalDeckContentsInOfflineData(ref data, localDeckContents);
		SetOriginalDeckContentsInOfflineData(ref data, originalDeckContents);
	}

	private static void SetOriginalDeckContentsInOfflineData(ref OfflineData data, DeckContents packet)
	{
		if (data.OriginalDeckContents == null)
		{
			data.OriginalDeckContents = new List<DeckContents>();
		}
		data.OriginalDeckContents.RemoveAll((DeckContents c) => c.DeckId == packet.DeckId);
		data.OriginalDeckContents.Add(packet);
		Log.Offline.PrintDebug("OfflineDataCache: Caching original deck contents: id={0}", packet.DeckId);
	}

	private static void SetLocalDeckContentsInOfflineData(ref OfflineData data, DeckContents packet)
	{
		if (data.LocalDeckContents == null)
		{
			data.LocalDeckContents = new List<DeckContents>();
		}
		data.LocalDeckContents.RemoveAll((DeckContents c) => c.DeckId == packet.DeckId);
		data.LocalDeckContents.Add(packet);
		Log.Offline.PrintDebug("OfflineDataCache: Caching local deck contents: id={0}", packet.DeckId);
	}

	public static void CacheFavoriteHeroes(ref OfflineData data, FavoriteHeroesResponse packet)
	{
		data.FavoriteHeroes = new List<FavoriteHero>(packet.FavoriteHeroes);
		Log.Offline.PrintDebug("OfflineDataCache: Caching favorite heroes: {0}", packet.ToHumanReadableString());
	}

	public static void CacheCardBacks(ref OfflineData data, CardBacks packet)
	{
		data.CardBacks = packet;
		Log.Offline.PrintDebug("OfflineDataCache: Caching favorite card backs: {0}", packet.ToHumanReadableString());
	}

	public static void CacheCoins(ref OfflineData data, CosmeticCoins packet)
	{
		data.CosmeticCoins = packet;
		Log.Offline.PrintDebug("OfflineDataCache: Caching favorite coins: {0}", packet.ToHumanReadableString());
	}

	public static List<DeckContents> GetLocalDeckContentsFromCache()
	{
		return ReadOfflineDataFromFile().LocalDeckContents;
	}

	public static List<FavoriteHero> GetFavoriteHeroesFromCache()
	{
		return ReadOfflineDataFromFile().FavoriteHeroes;
	}

	public static CardBacks GetCardBacksFromCache()
	{
		return ReadOfflineDataFromFile().CardBacks;
	}

	public static CosmeticCoins GetCoinsFromCache()
	{
		return ReadOfflineDataFromFile().CosmeticCoins;
	}

	public static DeckInfo GetDeckInfoFromDeckList(long deckId, List<DeckInfo> deckList)
	{
		if (deckList == null)
		{
			return null;
		}
		int count = 0;
		foreach (DeckInfo deck in deckList)
		{
			if (deck.Id == deckId)
			{
				count++;
			}
		}
		if (count > 1)
		{
			Log.Offline.PrintError("GetDeckInfoFromDeckList: Found multiple decks in cache with id: {0}", deckId);
		}
		foreach (DeckInfo deckInfo in deckList)
		{
			if (deckInfo.Id == deckId)
			{
				return deckInfo;
			}
		}
		Log.Offline.PrintWarning("GetDeckInfoFromDeckList: No deck header found with id: {0}", deckId);
		return null;
	}

	public static DeckContents GetDeckContentsFromDeckContentsList(long deckId, List<DeckContents> list)
	{
		if (list == null)
		{
			return null;
		}
		if (list.Count((DeckContents d) => d.DeckId == deckId) > 1)
		{
			Log.Offline.PrintError("GetDeckContentsFromDeckContentsList: Found multiple decks in cache with id: {0}", deckId);
		}
		foreach (DeckContents contents in list)
		{
			if (contents.DeckId == deckId)
			{
				return contents;
			}
		}
		Log.Offline.PrintWarning("GetDeckContentsFromDeckContentsList: No deck contents found with id: {0}", deckId);
		return null;
	}

	public static List<long> GetFakeDeckIds(OfflineData data = null)
	{
		if (data == null)
		{
			data = ReadOfflineDataFromFile();
		}
		List<long> validFakeIds = new List<long>();
		if (data.FakeDeckIds == null)
		{
			return validFakeIds;
		}
		foreach (long fakeId in data.FakeDeckIds)
		{
			if (IsValidFakeId(fakeId) && !validFakeIds.Contains(fakeId))
			{
				validFakeIds.Add(fakeId);
			}
		}
		return validFakeIds;
	}

	public static List<DeckInfo> GetFakeDeckInfos(OfflineData data)
	{
		List<DeckInfo> fakeDecks = new List<DeckInfo>();
		foreach (long fakeDeckId in GetFakeDeckIds(data))
		{
			DeckInfo fakeDeck = null;
			foreach (DeckInfo deckInfo in data.LocalDeckList)
			{
				if (deckInfo.Id == fakeDeckId)
				{
					fakeDeck = deckInfo;
					break;
				}
			}
			if (fakeDeck != null)
			{
				fakeDecks.Add(fakeDeck);
			}
		}
		return fakeDecks;
	}

	public static void ClearFakeDeckIds(ref OfflineData data)
	{
		if (data.FakeDeckIds == null)
		{
			return;
		}
		foreach (long fakeDeckId in data.FakeDeckIds)
		{
			DeleteDeck(fakeDeckId);
		}
		data.FakeDeckIds = new List<long>();
		data.UniqueFakeDeckId--;
		Log.Offline.PrintDebug("OfflineDataCache: Clearing Fake Deck Ids");
	}

	public static bool UpdateDeckWithNewId(long oldId, long newId)
	{
		OfflineData data = ReadOfflineDataFromFile();
		Log.Offline.PrintDebug("OfflineDataCache: Updating deck {0} with new id {1}", oldId, newId);
		DeckInfo deckInfo = GetDeckInfoFromDeckList(oldId, data.LocalDeckList);
		DeckContents deckContents = GetDeckContentsFromDeckContentsList(oldId, data.LocalDeckContents);
		if (deckInfo != null)
		{
			deckInfo.Id = newId;
			if (deckContents != null)
			{
				deckContents.DeckId = newId;
				WriteOfflineDataToFile(data);
				return true;
			}
			Log.Offline.PrintError("UpdateDeckWithNewId: No deck contents found in Offline Data Cache with old id: {0}", oldId);
			return false;
		}
		Log.Offline.PrintError("UpdateDeckWithNewId: No deck info found in Offline Data Cache with old id: {0}", oldId);
		return false;
	}

	public static void RenameDeck(long deckId, string newName)
	{
		OfflineData data = ReadOfflineDataFromFile();
		DeckInfo deckInfo = GetDeckInfoFromDeckList(deckId, data.LocalDeckList);
		if (deckInfo == null)
		{
			Log.Offline.PrintError("Received a rename command for deck id={0}, name={1}, but a deck with that id was not found in the OfflineDataCache.", deckId, newName);
		}
		else
		{
			deckInfo.Name = newName;
			Log.Offline.PrintDebug("OfflineDataCache: Renaming deck {0} to {1}", deckId, newName);
			WriteOfflineDataToFile(data);
		}
	}

	public static void SetFavoriteCardBack(int cardBackId, bool isFavorite = true)
	{
		OfflineData data = ReadOfflineDataFromFile();
		if (isFavorite)
		{
			data.CardBacks.FavoriteCardBacks.Add(cardBackId);
			Log.Offline.PrintDebug("OfflineDataCache: Added favorite card back {0}", cardBackId);
		}
		else
		{
			data.CardBacks.FavoriteCardBacks.Remove(cardBackId);
			Log.Offline.PrintDebug("OfflineDataCache: Removed favorite card back {0}", cardBackId);
		}
		data.m_hasChangedCardBacksOffline = true;
		WriteOfflineDataToFile(data);
	}

	public static void ClearCardBackDirtyFlag(ref OfflineData data)
	{
		data.m_hasChangedCardBacksOffline = false;
		Log.Offline.PrintDebug("OfflineDataCache: Clearing card back flag");
	}

	public static void SetFavoriteCosmeticCoin(ref OfflineData data, int coinId, bool isFavorite)
	{
		if (isFavorite)
		{
			data.CosmeticCoins.FavoriteCoins.Add(coinId);
			Log.Offline.PrintDebug("OfflineDataCache: Added favorite coin {0}", coinId);
		}
		else
		{
			data.CosmeticCoins.FavoriteCoins.Remove(coinId);
			Log.Offline.PrintDebug("OfflineDataCache: Removed favorite coin {0}", coinId);
		}
		data.m_hasChangedCoinsOffline = true;
		WriteOfflineDataToFile(data);
	}

	public static void ClearCoinDirtyFlag(ref OfflineData data)
	{
		data.m_hasChangedCoinsOffline = false;
		Log.Offline.PrintDebug("OfflineDataCache: Clearing coin flag");
	}

	public static void SetFavoriteHero(int heroClass, PegasusShared.CardDef cardDef, bool wasCalledOffline, bool isFavorite)
	{
		OfflineData data = ReadOfflineDataFromFile();
		FavoriteHero existingFavorite = data.FavoriteHeroes.Find((FavoriteHero favorite) => favorite.Hero.Asset == cardDef.Asset);
		if (existingFavorite != null)
		{
			if (isFavorite)
			{
				existingFavorite.Hero = cardDef;
			}
			else
			{
				data.FavoriteHeroes.Remove(existingFavorite);
			}
		}
		else if (isFavorite)
		{
			data.FavoriteHeroes.Add(new FavoriteHero
			{
				ClassId = heroClass,
				Hero = cardDef
			});
		}
		if (wasCalledOffline)
		{
			data.m_hasChangedFavoriteHeroesOffline = true;
		}
		Log.Offline.PrintDebug("OfflineDataCache: Setting favorite hero for class {0} to {1}", heroClass, cardDef.ToHumanReadableString());
		WriteOfflineDataToFile(data);
	}

	public static void ClearFavoriteHeroesDirtyFlag()
	{
		OfflineData offlineData = ReadOfflineDataFromFile();
		offlineData.m_hasChangedFavoriteHeroesOffline = false;
		Log.Offline.PrintDebug("OfflineDataCache: Clearing favorite hero flag");
		WriteOfflineDataToFile(offlineData);
	}

	public static long GetCachedCollectionVersion(OfflineData data = null)
	{
		if (data == null)
		{
			data = ReadOfflineDataFromFile();
		}
		if (data != null && data.Collection != null && data.Collection.HasCollectionVersion)
		{
			return data.Collection.CollectionVersion;
		}
		return 0L;
	}

	public static long GetCachedCollectionVersionLastModified(OfflineData data = null)
	{
		if (data == null)
		{
			data = ReadOfflineDataFromFile();
		}
		if (data != null && data.Collection != null && data.Collection.HasCollectionVersionLastModified)
		{
			return data.Collection.CollectionVersionLastModified;
		}
		return 0L;
	}

	public static void CacheCollection(ref OfflineData data, Collection collection)
	{
		data.Collection = collection;
	}

	public static List<DeckModificationTimes> GetCachedDeckContentsTimes(OfflineData data = null)
	{
		List<DeckModificationTimes> deckTimes = new List<DeckModificationTimes>();
		if (data == null)
		{
			data = ReadOfflineDataFromFile();
		}
		if (data == null || data.LocalDeckContents == null || data.LocalDeckList == null)
		{
			return deckTimes;
		}
		foreach (DeckContents deckContent in data.LocalDeckContents)
		{
			DeckInfo deckInfo = data.LocalDeckList.Find((DeckInfo list) => list.Id == deckContent.DeckId);
			if (deckInfo != null)
			{
				deckTimes.Add(new DeckModificationTimes
				{
					DeckId = deckContent.DeckId,
					LastModified = deckInfo.LastModified
				});
			}
		}
		return deckTimes;
	}

	public static void ApplyDeckSetDataLocally(DeckSetData packet)
	{
		OfflineData data = ReadOfflineDataFromFile();
		ApplyDeckSetDataToDeck(packet, data.LocalDeckList, data.LocalDeckContents);
		WriteOfflineDataToFile(data);
	}

	public static void ApplyDeckSetDataToOriginalDeck(DeckSetData packet)
	{
		OfflineData data = ReadOfflineDataFromFile();
		ApplyDeckSetDataToDeck(packet, data.OriginalDeckList, data.OriginalDeckContents);
		Log.Offline.PrintDebug("OfflineDataCache: Applying deck changes to deck. Changes: {0}", packet.ToHumanReadableString());
		WriteOfflineDataToFile(data);
	}

	public static void ApplyDeckSetDataToDeck(DeckSetData packet, List<DeckInfo> deckList, List<DeckContents> deckContentsList)
	{
		DeckInfo matchingDeckInfo = null;
		foreach (DeckInfo deckInfo in deckList)
		{
			if (deckInfo.Id == packet.Deck)
			{
				matchingDeckInfo = deckInfo;
				break;
			}
		}
		if (matchingDeckInfo == null)
		{
			matchingDeckInfo = new DeckInfo();
			matchingDeckInfo.Id = packet.Deck;
			deckList.Add(matchingDeckInfo);
		}
		DeckContents matchingDeckContents = null;
		foreach (DeckContents deckContents in deckContentsList)
		{
			if (deckContents.DeckId == packet.Deck)
			{
				matchingDeckContents = deckContents;
				break;
			}
		}
		if (matchingDeckContents == null)
		{
			matchingDeckContents = new DeckContents();
			matchingDeckContents.DeckId = packet.Deck;
			deckContentsList.Add(matchingDeckContents);
		}
		if (packet.HasCardBack)
		{
			matchingDeckInfo.HasCardBack = true;
			matchingDeckInfo.CardBack = packet.CardBack;
		}
		else if (packet.HasRemovingCardBack)
		{
			matchingDeckInfo.HasCardBack = false;
		}
		if (packet.HasHero)
		{
			matchingDeckInfo.Hero = packet.Hero;
		}
		if (packet.HasSortOrder)
		{
			matchingDeckInfo.SortOrder = packet.SortOrder;
		}
		if (packet.HasPastedDeckHash)
		{
			matchingDeckInfo.PastedDeckHash = packet.PastedDeckHash;
		}
		if (packet.Cards != null)
		{
			foreach (DeckCardData cardData in packet.Cards)
			{
				bool cardFound = false;
				foreach (DeckCardData matchingCardData in matchingDeckContents.Cards)
				{
					if (matchingCardData.Def.Asset == cardData.Def.Asset && matchingCardData.Def.Premium == cardData.Def.Premium)
					{
						matchingCardData.Qty = cardData.Qty;
						cardFound = true;
						break;
					}
				}
				if (!cardFound)
				{
					matchingDeckContents.Cards.Add(cardData);
				}
			}
		}
		if (packet.SideboardCards != null)
		{
			foreach (SideBoardCardData packetSideboardCardData in packet.SideboardCards)
			{
				bool cardFound2 = false;
				foreach (SideBoardCardData matchingSideboardCardData in matchingDeckContents.SideboardCards)
				{
					if (matchingSideboardCardData.Def.Asset == packetSideboardCardData.Def.Asset && matchingSideboardCardData.Def.Premium == packetSideboardCardData.Def.Premium)
					{
						matchingSideboardCardData.Qty = packetSideboardCardData.Qty;
						cardFound2 = true;
						break;
					}
				}
				if (!cardFound2)
				{
					matchingDeckContents.SideboardCards.Add(packetSideboardCardData);
				}
			}
		}
		if (matchingDeckInfo.Name == null)
		{
			matchingDeckInfo.Name = "Unknown";
		}
		matchingDeckInfo.LastModified = (long)TimeUtils.DateTimeToUnixTimeStamp(DateTime.Now);
	}

	public static bool GenerateDeckSetDataFromDiff(long deckId, DeckInfo patchingDeckInfo, DeckInfo originalDeckInfo, DeckContents patchingDeckContents, DeckContents originalDeckContents, out DeckSetData deckSetData)
	{
		deckSetData = new DeckSetData();
		deckSetData.Deck = deckId;
		bool packetHasChanged = false;
		if (!patchingDeckInfo.HasCardBack && originalDeckInfo.HasCardBack)
		{
			deckSetData.RemovingCardBack = true;
			deckSetData.HasCardBack = false;
			packetHasChanged = true;
		}
		else if (patchingDeckInfo.HasCardBack && (!originalDeckInfo.HasCardBack || patchingDeckInfo.CardBack != originalDeckInfo.CardBack))
		{
			deckSetData.HasCardBack = true;
			deckSetData.CardBack = patchingDeckInfo.CardBack;
			packetHasChanged = true;
		}
		if (patchingDeckInfo.Hero != originalDeckInfo.Hero)
		{
			deckSetData.Hero = patchingDeckInfo.Hero;
			packetHasChanged = true;
		}
		if (patchingDeckInfo.RandomHeroUseFavorite != originalDeckInfo.RandomHeroUseFavorite)
		{
			deckSetData.RandomHeroUseFavorite = patchingDeckInfo.RandomHeroUseFavorite;
			packetHasChanged = true;
		}
		if (!string.Equals(patchingDeckInfo.PastedDeckHash, originalDeckInfo.PastedDeckHash))
		{
			deckSetData.PastedDeckHash = patchingDeckInfo.PastedDeckHash;
			packetHasChanged = true;
		}
		if (patchingDeckInfo.SortOrder != originalDeckInfo.SortOrder)
		{
			deckSetData.SortOrder = patchingDeckInfo.SortOrder;
			packetHasChanged = true;
		}
		deckSetData.Cards = GetDeckContentsDelta(patchingDeckContents, originalDeckContents);
		if (deckSetData.Cards.Any())
		{
			packetHasChanged = true;
		}
		return packetHasChanged;
	}

	public static bool GenerateDeckSetDataFromDiff(long deckId, List<DeckInfo> patchingDeckList, List<DeckInfo> originalDeckList, List<DeckContents> patchingDeckContentsList, List<DeckContents> originalDeckContentsList, out DeckSetData deckSetData)
	{
		DeckInfo localDeckInfo = GetDeckInfoFromDeckList(deckId, patchingDeckList);
		DeckInfo originalDeckInfo = GetDeckInfoFromDeckList(deckId, originalDeckList);
		DeckContents localDeckContents = GetDeckContentsFromDeckContentsList(deckId, patchingDeckContentsList);
		DeckContents originalDeckContents = GetDeckContentsFromDeckContentsList(deckId, originalDeckContentsList);
		if (localDeckInfo == null)
		{
			localDeckInfo = new DeckInfo();
		}
		if (originalDeckInfo == null)
		{
			originalDeckInfo = new DeckInfo();
		}
		if (localDeckContents == null)
		{
			localDeckContents = new DeckContents();
		}
		if (originalDeckContents == null)
		{
			originalDeckContents = new DeckContents();
		}
		return GenerateDeckSetDataFromDiff(deckId, localDeckInfo, originalDeckInfo, localDeckContents, originalDeckContents, out deckSetData);
	}

	public static RenameDeck GenerateRenameDeckFromDiff(long deckId, DeckInfo patchingDeckInfo, DeckInfo originalDeckInfo)
	{
		if (!string.Equals(patchingDeckInfo.Name, originalDeckInfo.Name))
		{
			return new RenameDeck
			{
				Deck = deckId,
				Name = patchingDeckInfo.Name
			};
		}
		return null;
	}

	public static void DeleteDeck(long deckId)
	{
		OfflineData data = ReadOfflineDataFromFile();
		data.LocalDeckList.RemoveAll((DeckInfo d) => d.Id == deckId);
		data.LocalDeckContents.RemoveAll((DeckContents d) => d.DeckId == deckId);
		Log.Offline.PrintDebug("OfflineDataCache: Deleting deck: {0}", deckId);
		WriteOfflineDataToFile(data);
	}

	public static void RemoveAllOldDecksContents(ref OfflineData data)
	{
		DeckContents[] array;
		if (data.LocalDeckContents != null)
		{
			array = data.LocalDeckContents.ToArray();
			foreach (DeckContents deckContents in array)
			{
				if (!data.LocalDeckList.Any((DeckInfo d) => d.Id == deckContents.DeckId))
				{
					data.LocalDeckContents.Remove(deckContents);
				}
			}
		}
		if (data.OriginalDeckContents == null)
		{
			return;
		}
		array = data.OriginalDeckContents.ToArray();
		foreach (DeckContents deckContents2 in array)
		{
			if (!data.OriginalDeckList.Any((DeckInfo d) => d.Id == deckContents2.DeckId))
			{
				data.OriginalDeckContents.Remove(deckContents2);
			}
		}
	}

	public static DeckInfo CreateDeck(DeckType deckType, string name, int heroDbId, FormatType formatType, long sortOrder, DeckSourceType sourceType, string pastedDeckHash = null)
	{
		OfflineData data = ReadOfflineDataFromFile();
		long fakeId = GetAndRecordNextFakeId(data.FakeDeckIds, data);
		DeckInfo deckInfo = new DeckInfo
		{
			Id = fakeId,
			DeckType = deckType,
			Name = name,
			Hero = heroDbId,
			SourceType = sourceType,
			SortOrder = sortOrder,
			PastedDeckHash = pastedDeckHash,
			Validity = (ulong)((formatType == FormatType.FT_STANDARD) ? 128 : 0),
			FormatType = formatType
		};
		data.LocalDeckList.Add(deckInfo);
		DeckContents deckContents = new DeckContents();
		deckContents.DeckId = fakeId;
		data.LocalDeckContents.Add(deckContents);
		Log.Offline.PrintDebug("OfflineDataCache: Creating offline deck: id={0}", fakeId);
		if (!WriteOfflineDataToFile(data))
		{
			return null;
		}
		return deckInfo;
	}

	public static List<SetFavoriteCardBack> GenerateSetFavoriteCardBackFromDiff(OfflineData data, List<int> receivedFavoriteCardBacks)
	{
		List<SetFavoriteCardBack> setFavoriteRequests = new List<SetFavoriteCardBack>();
		if (!data.m_hasChangedCardBacksOffline || data.CardBacks == null)
		{
			return null;
		}
		List<int> addedFavorites = data.CardBacks.FavoriteCardBacks.Except(receivedFavoriteCardBacks).ToList();
		if (addedFavorites.Count > 0)
		{
			for (int i = 0; i < addedFavorites.Count; i++)
			{
				setFavoriteRequests.Add(new SetFavoriteCardBack
				{
					CardBack = addedFavorites[i],
					IsFavorite = true
				});
			}
		}
		List<int> removedFavorites = receivedFavoriteCardBacks.Except(data.CardBacks.FavoriteCardBacks).ToList();
		if (removedFavorites.Count > 0)
		{
			for (int j = 0; j < removedFavorites.Count; j++)
			{
				setFavoriteRequests.Add(new SetFavoriteCardBack
				{
					CardBack = removedFavorites[j],
					IsFavorite = false
				});
			}
		}
		return setFavoriteRequests;
	}

	public static List<SetFavoriteCosmeticCoin> GenerateSetFavoriteCosmeticCoinFromDiff(OfflineData data, List<int> receivedFavoriteCoins)
	{
		List<SetFavoriteCosmeticCoin> packetsToSend = new List<SetFavoriteCosmeticCoin>();
		if (!data.m_hasChangedCoinsOffline)
		{
			return packetsToSend;
		}
		if (data.CosmeticCoins == null)
		{
			return packetsToSend;
		}
		foreach (int localFavoriteId in data.CosmeticCoins.FavoriteCoins)
		{
			if (!receivedFavoriteCoins.Contains(localFavoriteId))
			{
				SetFavoriteCosmeticCoin packet = new SetFavoriteCosmeticCoin
				{
					Coin = localFavoriteId,
					IsFavorite = true
				};
				packetsToSend.Add(packet);
			}
		}
		foreach (int remoteFavoriteId in receivedFavoriteCoins)
		{
			if (!data.CosmeticCoins.FavoriteCoins.Contains(remoteFavoriteId))
			{
				SetFavoriteCosmeticCoin packet2 = new SetFavoriteCosmeticCoin
				{
					Coin = remoteFavoriteId,
					IsFavorite = false
				};
				packetsToSend.Add(packet2);
			}
		}
		return packetsToSend;
	}

	public static List<SetFavoriteHero> GenerateSetFavoriteHeroFromDiff(OfflineData data, NetCache.NetCacheFavoriteHeroes receivedFavoriteHeroes)
	{
		List<SetFavoriteHero> packetsToSend = new List<SetFavoriteHero>();
		if (!data.m_hasChangedFavoriteHeroesOffline)
		{
			return packetsToSend;
		}
		if (data.FavoriteHeroes == null)
		{
			return packetsToSend;
		}
		foreach (FavoriteHero localFavorite in data.FavoriteHeroes)
		{
			if (!receivedFavoriteHeroes.FavoriteHeroes.Any(((TAG_CLASS, NetCache.CardDefinition) hero) => hero.Item2.Name == GameUtils.TranslateDbIdToCardId(localFavorite.Hero.Asset)))
			{
				SetFavoriteHero packet = new SetFavoriteHero
				{
					FavoriteHero = new FavoriteHero
					{
						ClassId = localFavorite.ClassId,
						Hero = localFavorite.Hero
					},
					IsFavorite = true
				};
				packetsToSend.Add(packet);
			}
		}
		foreach (var remoteFavorite in receivedFavoriteHeroes.FavoriteHeroes)
		{
			if (!data.FavoriteHeroes.Any((FavoriteHero favorite) => GameUtils.TranslateDbIdToCardId(favorite.Hero.Asset) == remoteFavorite.Item2.Name))
			{
				SetFavoriteHero packet2 = new SetFavoriteHero
				{
					FavoriteHero = new FavoriteHero
					{
						ClassId = (int)remoteFavorite.Item1,
						Hero = new PegasusShared.CardDef
						{
							Asset = GameUtils.TranslateCardIdToDbId(remoteFavorite.Item2.Name),
							Premium = (int)remoteFavorite.Item2.Premium
						}
					},
					IsFavorite = false
				};
				packetsToSend.Add(packet2);
			}
		}
		return packetsToSend;
	}

	private static string GetCacheFolderPath()
	{
		return string.Format("{0}/{1}", PlatformFilePaths.CachePath, "Offline");
	}

	private static string GetCacheFilePath()
	{
		string folder = GetCacheFolderPath();
		BnetGameAccountId gameAccountId = Network.Get().GetMyGameAccountId() ?? new BnetGameAccountId(0uL, 0uL);
		string user = $"{gameAccountId.High}_{gameAccountId.Low}";
		string region = Network.Get().GetCurrentRegion().ToString();
		string devVersion = "";
		if (HearthstoneApplication.IsInternal())
		{
			devVersion = string.Format("_{0}", "31.4");
			devVersion = devVersion.Replace(".", "_");
		}
		return $"{folder}/offlineData_{user}_{region}{devVersion}.cache";
	}

	private static void CreateCacheFolder()
	{
		string folderPath = GetCacheFolderPath();
		if (Directory.Exists(folderPath))
		{
			return;
		}
		try
		{
			Directory.CreateDirectory(folderPath);
		}
		catch (Exception ex)
		{
			Debug.LogError($"UberText.CreateCacheFolder() - Failed to create {folderPath}. Reason={ex.Message}");
		}
	}

	public static bool WriteOfflineDataToFile(OfflineData data)
	{
		CreateCacheFolder();
		string filePath = GetCacheFilePath();
		try
		{
			using BinaryWriter writer = new BinaryWriter(File.Open(filePath, FileMode.Create, FileAccess.Write));
			writer.Write(1);
			IOfflineDataSerializer serializer = OfflineDataSerializer.GetSerializer(1);
			if (serializer == null)
			{
				Debug.LogErrorFormat("Could not find serializer for writing version {0}. Make sure a new seralizer is added when incrementing versions.", 1);
				return false;
			}
			serializer.Serialize(data, writer);
		}
		catch (IOException ex)
		{
			Log.Offline.PrintError("WriteOfflineDataToFile - Is disk full? - Exception: {0}", ex.InnerException);
			return false;
		}
		catch (UnauthorizedAccessException ex2)
		{
			Log.Offline.PrintError("WriteOfflineDataToFile - Are write permissions correctly applied to the file attempting to be accessed? - Exception: {0}", ex2.InnerException);
			return false;
		}
		catch (Exception ex3)
		{
			Log.Offline.PrintError("WriteOfflineDataToFile - Unexpected exception thrown - Exception: {0}", ex3.InnerException);
			return false;
		}
		return true;
	}

	public static OfflineData ReadOfflineDataFromFile()
	{
		OfflineData data = new OfflineData();
		string cacheFolderPath = GetCacheFolderPath();
		string filePath = GetCacheFilePath();
		if (!Directory.Exists(cacheFolderPath) || !File.Exists(filePath))
		{
			return data;
		}
		bool needsClear = false;
		try
		{
			using BinaryReader reader = new BinaryReader(File.Open(filePath, FileMode.Open));
			int versionNumber = reader.ReadInt32();
			IOfflineDataSerializer serializer = OfflineDataSerializer.GetSerializer(versionNumber);
			if (serializer == null)
			{
				Debug.LogWarningFormat("Could not find serializer for offline data version {0}", versionNumber);
				needsClear = true;
			}
			else
			{
				data = serializer.Deserialize(reader);
			}
		}
		catch (EndOfStreamException)
		{
			Log.Offline.PrintError("ReadOfflineDataFromFile - Not all protos are represented. Is this a new cache file?");
			needsClear = true;
		}
		catch (ProtocolBufferException)
		{
			Log.Offline.PrintError("Error parsing cached protobufs from cache. Recreating cache file.");
			needsClear = true;
		}
		if (needsClear)
		{
			ClearLocalCacheFile();
		}
		return data;
	}

	public static void ClearLocalCacheFile()
	{
		OfflineData data = new OfflineData();
		Log.Offline.PrintDebug("OfflineDataCache: Clearing local cache file");
		WriteOfflineDataToFile(data);
	}

	private static List<DeckCardData> GetDeckContentsDelta(DeckContents deckContentsLocal, DeckContents deckContentsOriginal)
	{
		List<DeckCardData> cardsLocal = deckContentsLocal.Cards;
		List<DeckCardData> cardsOriginal = deckContentsOriginal.Cards;
		List<DeckCardData> deckSetDataDelta = new List<DeckCardData>();
		foreach (PegasusShared.CardDef cardDef in new HashSet<PegasusShared.CardDef>(from c in cardsLocal.Union(cardsOriginal).Except(cardsLocal.Intersect(cardsOriginal)).ToList()
			select c.Def))
		{
			DeckCardData cardInLocal = cardsLocal.FirstOrDefault((DeckCardData c) => c.Def.Asset == cardDef.Asset && c.Def.Premium == cardDef.Premium);
			DeckCardData cardInOriginal = cardsOriginal.FirstOrDefault((DeckCardData c) => c.Def.Asset == cardDef.Asset && c.Def.Premium == cardDef.Premium);
			int qtyInLocal = cardInLocal?.Qty ?? 0;
			int qtyInOriginal = cardInOriginal?.Qty ?? 0;
			if (qtyInLocal != qtyInOriginal)
			{
				DeckCardData cardToAdd = new DeckCardData
				{
					Def = cardDef,
					Qty = qtyInLocal
				};
				deckSetDataDelta.Add(cardToAdd);
			}
		}
		return deckSetDataDelta;
	}

	private static long GetAndRecordNextFakeId(List<long> usedIds, OfflineData data)
	{
		if (usedIds == null)
		{
			usedIds = new List<long>();
		}
		while (usedIds.Contains(data.UniqueFakeDeckId))
		{
			data.UniqueFakeDeckId--;
		}
		usedIds.Add(data.UniqueFakeDeckId);
		return data.UniqueFakeDeckId;
	}

	private static bool IsValidFakeId(long id)
	{
		return id < 0;
	}
}
