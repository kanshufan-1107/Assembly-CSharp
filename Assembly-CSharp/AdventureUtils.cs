using System;
using System.Collections.Generic;
using System.Linq;
using Hearthstone.DataModels;
using Hearthstone.DungeonCrawl;
using Hearthstone.Progression;
using Hearthstone.Store;
using PegasusUtil;
using UnityEngine;

public class AdventureUtils
{
	public delegate void FirstChapterFreePopupCompleteCallback();

	public static List<AdventureLoadoutTreasuresDbfRecord> GetLoadoutTreasuresForAdventureAndClass(AdventureDbId adventure, TAG_CLASS classId)
	{
		List<AdventureLoadoutTreasuresDbfRecord> records = GameDbf.AdventureLoadoutTreasures.GetRecords((AdventureLoadoutTreasuresDbfRecord r) => r.AdventureId == (int)adventure && r.ClassId == (int)classId);
		records.Sort((AdventureLoadoutTreasuresDbfRecord a, AdventureLoadoutTreasuresDbfRecord b) => a.SortOrder.CompareTo(b.SortOrder));
		return records;
	}

	public static List<AdventureLoadoutTreasuresDbfRecord> GetLoadoutTreasuresForAdventureAndGuestHero(AdventureDbId adventure, int guestHeroId)
	{
		int guestHeroIdToUse = GetBaseGuestHeroIdForAdventure(adventure, guestHeroId);
		if (guestHeroIdToUse == 0)
		{
			guestHeroIdToUse = guestHeroId;
		}
		List<AdventureLoadoutTreasuresDbfRecord> records = GameDbf.AdventureLoadoutTreasures.GetRecords((AdventureLoadoutTreasuresDbfRecord r) => r.AdventureId == (int)adventure && r.GuestHeroId == guestHeroIdToUse);
		records.Sort((AdventureLoadoutTreasuresDbfRecord a, AdventureLoadoutTreasuresDbfRecord b) => a.SortOrder.CompareTo(b.SortOrder));
		return records;
	}

	public static List<AdventureHeroPowerDbfRecord> GetHeroPowersForAdventureAndClass(AdventureDbId adventure, TAG_CLASS classId)
	{
		List<AdventureHeroPowerDbfRecord> records = GameDbf.AdventureHeroPower.GetRecords((AdventureHeroPowerDbfRecord r) => r.AdventureId == (int)adventure && r.ClassId == (int)classId);
		records.Sort((AdventureHeroPowerDbfRecord a, AdventureHeroPowerDbfRecord b) => a.SortOrder.CompareTo(b.SortOrder));
		return records;
	}

	public static List<AdventureDeckDbfRecord> GetDecksForAdventureAndClass(AdventureDbId adventure, TAG_CLASS classId)
	{
		List<AdventureDeckDbfRecord> records = GameDbf.AdventureDeck.GetRecords((AdventureDeckDbfRecord r) => r.AdventureId == (int)adventure && r.ClassId == (int)classId);
		records.Sort((AdventureDeckDbfRecord a, AdventureDeckDbfRecord b) => a.SortOrder.CompareTo(b.SortOrder));
		return records;
	}

	public static List<AdventureHeroPowerDbfRecord> GetHeroPowersForAdventureAndGuestHero(AdventureDbId adventure, int guestHeroId)
	{
		int guestHeroIdToUse = GetBaseGuestHeroIdForAdventure(adventure, guestHeroId);
		if (guestHeroIdToUse == 0)
		{
			guestHeroIdToUse = guestHeroId;
		}
		List<AdventureHeroPowerDbfRecord> records = GameDbf.AdventureHeroPower.GetRecords((AdventureHeroPowerDbfRecord r) => r.AdventureId == (int)adventure && r.GuestHeroId == guestHeroIdToUse);
		records.Sort((AdventureHeroPowerDbfRecord a, AdventureHeroPowerDbfRecord b) => a.SortOrder.CompareTo(b.SortOrder));
		return records;
	}

	public static int GetBaseGuestHeroIdForAdventure(AdventureDbId adventure, int guestHeroId)
	{
		return GameDbf.AdventureGuestHeroes.GetRecord((AdventureGuestHeroesDbfRecord r) => r.AdventureId == (int)adventure && r.GuestHeroRecord.ID == guestHeroId)?.BaseGuestHeroId ?? 0;
	}

	public static bool AdventureHeroPowerIsUnlocked(GameSaveKeyId gameSaveServerKey, AdventureHeroPowerDbfRecord heroPowerRecord, out long unlockProgress, out bool hasUnlockCriteria)
	{
		return AdventureRewardIsUnlocked(gameSaveServerKey, (GameSaveKeySubkeyId)heroPowerRecord.UnlockGameSaveSubkey, heroPowerRecord.UnlockValue, heroPowerRecord.UnlockAchievement, out unlockProgress, out hasUnlockCriteria);
	}

	public static bool AdventureDeckIsUnlocked(GameSaveKeyId gameSaveServerKey, AdventureDeckDbfRecord deckRecord, out long unlockProgress, out bool hasUnlockCriteria)
	{
		return AdventureRewardIsUnlocked(gameSaveServerKey, (GameSaveKeySubkeyId)deckRecord.UnlockGameSaveSubkey, deckRecord.UnlockValue, 0, out unlockProgress, out hasUnlockCriteria);
	}

	public static bool AdventureTreasureIsUnlocked(GameSaveKeyId gameSaveServerKey, AdventureLoadoutTreasuresDbfRecord treasureLoadoutRecord, out long unlockProgress, out bool hasUnlockCriteria)
	{
		return AdventureRewardIsUnlocked(gameSaveServerKey, (GameSaveKeySubkeyId)treasureLoadoutRecord.UnlockGameSaveSubkey, treasureLoadoutRecord.UnlockValue, treasureLoadoutRecord.UnlockAchievement, out unlockProgress, out hasUnlockCriteria);
	}

	public static bool AdventureTreasureIsUpgraded(GameSaveKeyId gameSaveServerKey, AdventureLoadoutTreasuresDbfRecord treasureLoadoutRecord, out long upgradeProgress)
	{
		bool hasUnlockCriteria;
		return AdventureRewardIsUnlocked(gameSaveServerKey, (GameSaveKeySubkeyId)treasureLoadoutRecord.UpgradeGameSaveSubkey, treasureLoadoutRecord.UpgradeValue, 0, out upgradeProgress, out hasUnlockCriteria);
	}

	public static bool AdventureRewardIsUnlocked(GameSaveKeyId gameSaveServerKey, GameSaveKeySubkeyId unlockGameSaveSubkey, int unlockValue, int unlockAchievement, out long unlockProgress, out bool hasUnlockCriteria)
	{
		unlockProgress = 0L;
		hasUnlockCriteria = true;
		if (unlockAchievement <= 0 && unlockGameSaveSubkey <= (GameSaveKeySubkeyId)0)
		{
			hasUnlockCriteria = false;
			return true;
		}
		bool achievementUnlocked = false;
		int achievementProgress = 0;
		if (unlockAchievement > 0)
		{
			achievementUnlocked = AchievementManager.Get().IsAchievementComplete(unlockAchievement);
			achievementProgress = AchievementManager.Get().GetAchievementDataModel(unlockAchievement).Progress;
		}
		bool subkeyUnlocked = false;
		if (unlockGameSaveSubkey > (GameSaveKeySubkeyId)0)
		{
			GameSaveDataManager.Get().GetSubkeyValue(gameSaveServerKey, unlockGameSaveSubkey, out unlockProgress);
			subkeyUnlocked = unlockProgress >= unlockValue;
		}
		if (unlockGameSaveSubkey > (GameSaveKeySubkeyId)0 && unlockAchievement > 0)
		{
			unlockProgress += achievementProgress;
			return achievementUnlocked && subkeyUnlocked;
		}
		if (unlockAchievement > 0)
		{
			unlockProgress = achievementProgress;
			return achievementUnlocked;
		}
		if (unlockGameSaveSubkey > (GameSaveKeySubkeyId)0)
		{
			return subkeyUnlocked;
		}
		return false;
	}

	public static int GetFinalAdventureWing(int adventureId, bool excludeOwnedWings, bool excludeInactiveWings = false)
	{
		int maxUnlockOrder = -1;
		int maxWingId = 0;
		foreach (WingDbfRecord record in GameDbf.Wing.GetRecords())
		{
			if (record.AdventureId == adventureId && record.UnlockOrder > maxUnlockOrder && (!excludeOwnedWings || !AdventureProgressMgr.Get().OwnsWing(record.ID)) && (!excludeInactiveWings || AdventureProgressMgr.IsWingEventActive(record.ID)))
			{
				maxUnlockOrder = record.UnlockOrder;
				maxWingId = record.ID;
			}
		}
		return maxWingId;
	}

	public static bool IsAnomalyModeAvailable(AdventureDbId adventureDbId, AdventureModeDbId modeDbId, WingDbId wingDbId)
	{
		if (!IsAnomalyModeLocked(adventureDbId, modeDbId))
		{
			return IsAnomalyModeAllowed(wingDbId);
		}
		return false;
	}

	public static bool IsAnomalyModeLocked(AdventureDbId adventureDbId, AdventureModeDbId modeDbId)
	{
		foreach (ScenarioDbfRecord record in GameDbf.Scenario.GetRecords((ScenarioDbfRecord r) => r.AdventureId == (int)adventureDbId && r.ModeId == (int)modeDbId && r.WingId != 0))
		{
			if (!AdventureProgressMgr.Get().OwnsWing(record.WingId))
			{
				return true;
			}
		}
		return false;
	}

	public static bool IsAnomalyModeAllowed(WingDbId wingDbId)
	{
		WingDbfRecord wingRecord = GameDbf.Wing.GetRecord((int)wingDbId);
		if (wingRecord == null || !wingRecord.AllowsAnomaly)
		{
			return false;
		}
		return true;
	}

	public static AdventureDbId GetAdventureIdForWing(WingDbId wingDbId)
	{
		return (AdventureDbId)(GameDbf.Wing.GetRecord((int)wingDbId)?.AdventureId ?? 0);
	}

	public static bool IsProductTypeAnAdventureWing(ProductType type)
	{
		if ((uint)(type - 3) <= 1u || (uint)(type - 7) <= 1u)
		{
			return true;
		}
		return false;
	}

	public static bool DoesBundleIncludeWingForAdventure(ProductInfo bundle, AdventureDbId adventure)
	{
		if (bundle.Items != null)
		{
			return bundle.Items.Any((Network.BundleItem item) => IsProductTypeAnAdventureWing(item.ItemType) && GetAdventureIdForWing((WingDbId)item.ProductData) == adventure);
		}
		return false;
	}

	public static bool DoesBundleIncludeWing(ProductInfo bundle, int wingId)
	{
		if (bundle.Items != null)
		{
			return bundle.Items.Any((Network.BundleItem item) => IsProductTypeAnAdventureWing(item.ItemType) && item.ProductData == wingId);
		}
		return false;
	}

	public static int GetHeroCardDbIdFromClassForDungeonCrawl(IDungeonCrawlData dungeonCrawlData, TAG_CLASS cardClass)
	{
		return GameUtils.TranslateCardIdToDbId(GetHeroCardIdFromClassForDungeonCrawl(dungeonCrawlData, cardClass));
	}

	public static string GetHeroCardIdFromClassForDungeonCrawl(IDungeonCrawlData dungeonCrawlData, TAG_CLASS cardClass)
	{
		List<GuestHero> guestHeroes = dungeonCrawlData?.GetGuestHeroesForCurrentAdventure();
		if (guestHeroes == null || guestHeroes.Count <= 0)
		{
			NetCache.CardDefinition favoriteHeroDef = CollectionManager.Get().GetRandomFavoriteHero(cardClass, null);
			if (favoriteHeroDef == null)
			{
				Log.Adventures.PrintError("GameUtils.GetHeroCardIdFromClassForAdventure - could not get Hero Card Id from {0}", cardClass);
				return null;
			}
			return favoriteHeroDef.Name;
		}
		if (cardClass == TAG_CLASS.INVALID)
		{
			cardClass = TAG_CLASS.NEUTRAL;
		}
		foreach (GuestHero guestHero in guestHeroes)
		{
			if (GameUtils.GetTagClassFromCardDbId(guestHero.cardDbId) == cardClass)
			{
				CardDbfRecord cardRecord = GameDbf.Card.GetRecord(guestHero.cardDbId);
				if (cardRecord != null)
				{
					return cardRecord.NoteMiniGuid;
				}
			}
		}
		return null;
	}

	public static void DisplayFirstChapterFreePopup(ChapterPageData chapterPageData, FirstChapterFreePopupCompleteCallback callback = null)
	{
		AlertPopup.PopupInfo info = new AlertPopup.PopupInfo();
		info.m_headerText = GameStrings.Get("GLUE_ADVENTURE_ADVENTUREBOOK_DAL_FIRST_TIME_FLOW_HEADER");
		info.m_text = GameStrings.Get("GLUE_ADVENTURE_ADVENTUREBOOK_DAL_FIRST_TIME_FLOW");
		info.m_showAlertIcon = false;
		info.m_responseDisplay = AlertPopup.ResponseDisplay.OK;
		info.m_alertTextAlignment = UberText.AlignmentOptions.Center;
		info.m_alertTextAlignmentAnchor = UberText.AnchorOptions.Middle;
		info.m_responseCallback = delegate
		{
			AdventureConfig.Get().MarkHasSeenFirstTimeFlowComplete();
			AdventureDbfRecord record = GameDbf.Adventure.GetRecord((int)AdventureConfig.Get().GetSelectedAdventure());
			if (record != null && record.MapPageHasButtonsToChapters)
			{
				AdventureBookPageManager.NavigateToMapPage();
			}
			else
			{
				AdventureConfig.AckCurrentWingProgress(chapterPageData.WingRecord.ID);
			}
			if (callback != null)
			{
				callback();
			}
		};
		DialogManager.Get().ShowPopup(info);
	}

	public static bool IsEntireAdventureFree(AdventureDbId adventureID)
	{
		return !GameDbf.Wing.HasRecord((WingDbfRecord r) => r.AdventureId == (int)adventureID && (r.PmtProductIdForSingleWingPurchase != 0 || r.PmtProductIdForThisAndRestOfAdventure != 0));
	}

	public static bool DoesAdventureRequireAllHeroesUnlocked(AdventureDbId adventureId)
	{
		return DoesAdventureRequireAllHeroesUnlocked(adventureId, AdventureConfig.GetDefaultModeDbIdForAdventure(adventureId));
	}

	public static bool DoesAdventureRequireAllHeroesUnlocked(AdventureDbId adventureId, AdventureModeDbId modeId)
	{
		if (adventureId == AdventureDbId.INVALID || modeId == AdventureModeDbId.INVALID)
		{
			return true;
		}
		AdventureDataDbfRecord adventureData = AdventureConfig.GetAdventureDataRecord(adventureId, modeId);
		if (adventureData != null)
		{
			return !adventureData.IgnoreHeroUnlockRequirement;
		}
		return true;
	}

	public static bool IsDuelsAdventure(AdventureDbId adventure)
	{
		return GameDbf.Adventure.GetRecord((int)adventure)?.IsDuels ?? false;
	}

	public static List<GuestHero> GetGuestHeroesForAdventure(AdventureDbId currentAdventure)
	{
		List<AdventureGuestHeroesDbfRecord> sortedGuestHeroRecordsForAdventures = GetSortedGuestHeroRecordsForAdventures(currentAdventure);
		List<GuestHero> cardDbIds = new List<GuestHero>();
		foreach (AdventureGuestHeroesDbfRecord record in sortedGuestHeroRecordsForAdventures)
		{
			if (record.GuestHeroId != 0)
			{
				cardDbIds.Add(new GuestHero
				{
					guestHeroId = record.GuestHeroId,
					cardDbId = GameUtils.GetCardIdFromGuestHeroDbId(record.GuestHeroId)
				});
			}
		}
		return cardDbIds;
	}

	public static List<AdventureGuestHeroesDbfRecord> GetSortedGuestHeroRecordsForAdventures(AdventureDbId currentAdventure)
	{
		List<AdventureGuestHeroesDbfRecord> records = GameDbf.AdventureGuestHeroes.GetRecords((AdventureGuestHeroesDbfRecord r) => r.AdventureId == (int)currentAdventure);
		records.Sort((AdventureGuestHeroesDbfRecord a, AdventureGuestHeroesDbfRecord b) => a.SortOrder.CompareTo(b.SortOrder));
		return records;
	}

	public static CardListDataModel GetAvailableGuestHeroesAsCardListSortedByReleaseDate(AdventureDbId adventure)
	{
		CardListDataModel cardList = new CardListDataModel();
		List<AdventureGuestHeroesDbfRecord> records = GameDbf.AdventureGuestHeroes.GetRecords((AdventureGuestHeroesDbfRecord r) => r.AdventureId == (int)adventure && AdventureProgressMgr.IsWingEventActive(r.WingId));
		DateTime defaultDateIfNoRecordFound = DateTime.MinValue;
		records.Sort((AdventureGuestHeroesDbfRecord a, AdventureGuestHeroesDbfRecord b) => DateTime.Compare(ReleaseDateForAdventureGuestHero(b, defaultDateIfNoRecordFound), ReleaseDateForAdventureGuestHero(a, defaultDateIfNoRecordFound)));
		foreach (AdventureGuestHeroesDbfRecord record in records)
		{
			CardDbfRecord cardRec = GameDbf.Card.GetRecord(GameUtils.GetCardIdFromGuestHeroDbId(record.GuestHeroId));
			if (cardRec != null)
			{
				CardDataModel card = new CardDataModel
				{
					CardId = cardRec.NoteMiniGuid,
					Premium = TAG_PREMIUM.NORMAL
				};
				cardList.Cards.Add(card);
			}
		}
		return cardList;
	}

	public static DateTime ReleaseDateForAdventureGuestHero(AdventureGuestHeroesDbfRecord adventureGuestHero, DateTime defaultDate)
	{
		if (adventureGuestHero.WingRecord == null)
		{
			return defaultDate;
		}
		EventTimingType specialEvent = adventureGuestHero.WingRecord.RequiredEvent;
		if (specialEvent == EventTimingType.UNKNOWN)
		{
			return defaultDate;
		}
		DateTime? dateTime = EventTimingManager.Get().GetEventStartTimeUtc(specialEvent);
		if (dateTime.HasValue)
		{
			return dateTime.Value;
		}
		return defaultDate;
	}

	public static bool DoesAdventureShowNewlyUnlockedGuestHeroTreatment(AdventureDbId adventure)
	{
		switch (adventure)
		{
		case AdventureDbId.GIL:
			return false;
		case AdventureDbId.BLACKROCK_CRASH:
		case AdventureDbId.LOE_REVIVAL:
		case AdventureDbId.TB_BUCKET_BRAWL:
		case AdventureDbId.NAXX_CRASH:
		case AdventureDbId.TEMPLE_OUTRUN:
		case AdventureDbId.ROAD_TO_NORTHREND:
			return false;
		default:
			return true;
		}
	}

	public static bool DoesAdventureHaveUnseenGuestHeroes(AdventureDbId adventure, AdventureModeDbId mode)
	{
		List<long> seenUnlockedHeroIds = null;
		AdventureDataDbfRecord adventureData = AdventureConfig.GetAdventureDataRecord(adventure, mode);
		GameSaveDataManager.Get().GetSubkeyValue((GameSaveKeyId)adventureData.GameSaveDataClientKey, GameSaveKeySubkeyId.DUNGEON_CRAWL_HAS_SEEN_UNLOCKED_HEROES, out seenUnlockedHeroIds);
		foreach (AdventureGuestHeroesDbfRecord guestHero in GameDbf.AdventureGuestHeroes.GetRecords((AdventureGuestHeroesDbfRecord r) => r.AdventureId == (int)adventure))
		{
			if (guestHero.GuestHeroId != 0 && (seenUnlockedHeroIds == null || !seenUnlockedHeroIds.Contains(GameUtils.GetCardIdFromGuestHeroDbId(guestHero.GuestHeroId))) && AdventureProgressMgr.IsWingEventActive(guestHero.WingId) && AdventureProgressMgr.Get().OwnsWing(guestHero.WingId))
			{
				return true;
			}
		}
		return false;
	}

	private static AdventureGuestHeroesDbfRecord GetGuestHeroRecordForAdventure(AdventureDbId adventure, int heroCardDbId)
	{
		return GameDbf.AdventureGuestHeroes.GetRecord((AdventureGuestHeroesDbfRecord r) => r.AdventureId == (int)adventure && r.GuestHeroRecord != null && r.GuestHeroRecord.CardId == heroCardDbId);
	}

	public static List<AdventureHeroPowerDbfRecord> GetHeroPowersForDungeonCrawlHero(IDungeonCrawlData dungeonCrawl, int heroCardDbId)
	{
		if (AdventureConfig.Get().GetSelectedAdventureDataRecord().DungeonCrawlSaveHeroUsingHeroDbId)
		{
			int guestHeroId = GetGuestHeroIdFromHeroCardDbId(dungeonCrawl, heroCardDbId);
			return dungeonCrawl.GetHeroPowersForGuestHero(guestHeroId);
		}
		TAG_CLASS heroClass = GetHeroClassFromHeroId(heroCardDbId);
		return dungeonCrawl.GetHeroPowersForClass(heroClass);
	}

	public static List<AdventureLoadoutTreasuresDbfRecord> GetTreasuresForDungeonCrawlHero(IDungeonCrawlData dungeonCrawl, int heroCardDbId)
	{
		if (AdventureConfig.Get().GetSelectedAdventureDataRecord().DungeonCrawlSaveHeroUsingHeroDbId)
		{
			int guestHeroId = GetGuestHeroIdFromHeroCardDbId(dungeonCrawl, heroCardDbId);
			return dungeonCrawl.GetLoadoutTreasuresForGuestHero(guestHeroId);
		}
		TAG_CLASS heroClass = GetHeroClassFromHeroId(heroCardDbId);
		return dungeonCrawl.GetLoadoutTreasuresForClass(heroClass);
	}

	public static int GetGuestHeroIdFromHeroCardDbId(IDungeonCrawlData dungeonCrawl, int heroCardDbId)
	{
		List<GuestHero> guestHeroes = dungeonCrawl.GetGuestHeroesForCurrentAdventure();
		if (guestHeroes.Count == 0)
		{
			Debug.LogError($"No guest heroes were found for adventure: {dungeonCrawl?.GetSelectedAdventureDataRecord()?.AdventureId}");
			return 0;
		}
		foreach (GuestHero guest in guestHeroes)
		{
			if (guest.cardDbId == heroCardDbId)
			{
				return GameDbf.GuestHero.GetRecord((GuestHeroDbfRecord r) => r.ID == guest.guestHeroId).ID;
			}
		}
		return 0;
	}

	public static bool SelectableLoadoutTreasuresExistForAdventure(AdventureDbId adventure)
	{
		return GameDbf.AdventureLoadoutTreasures.HasRecord((AdventureLoadoutTreasuresDbfRecord r) => r.AdventureId == (int)adventure);
	}

	public static bool SelectableHeroPowersExistForAdventure(AdventureDbId adventure)
	{
		return GameDbf.AdventureHeroPower.HasRecord((AdventureHeroPowerDbfRecord r) => r.AdventureId == (int)adventure);
	}

	public static bool SelectableDecksExistForAdventure(AdventureDbId adventure)
	{
		return GameDbf.AdventureDeck.HasRecord((AdventureDeckDbfRecord r) => r.AdventureId == (int)adventure);
	}

	public static bool SelectableHeroPowersAndDecksExistForAdventure(AdventureDbId adventure)
	{
		bool adventureHasSelectableHeroPowers = SelectableHeroPowersExistForAdventure(adventure);
		bool adventureHasSelectableDecks = SelectableDecksExistForAdventure(adventure);
		if (!adventureHasSelectableHeroPowers && !adventureHasSelectableDecks)
		{
			return false;
		}
		if (adventureHasSelectableHeroPowers && adventureHasSelectableDecks)
		{
			return true;
		}
		Debug.LogError($"Adventure {adventure} has ADVENTURE_HERO_POWER or ADVENTURE_DECK entries defined, but not both! This is not currently suported - you must have entries for both tables, so a Hero Power can be selected first, then a Deck.");
		return false;
	}

	public static bool IsMissionValidForAdventureMode(AdventureDbId adventureId, AdventureModeDbId modeId, ScenarioDbId missionId)
	{
		if (adventureId == AdventureDbId.PVPDR && missionId == ScenarioDbId.PVPDR_Tavern)
		{
			return true;
		}
		if (adventureId == AdventureDbId.INVALID || modeId == AdventureModeDbId.INVALID || missionId == ScenarioDbId.INVALID)
		{
			return false;
		}
		if (GameDbf.Scenario.GetRecord((ScenarioDbfRecord r) => r.ID == (int)missionId && r.AdventureId == (int)adventureId && r.ModeId == (int)modeId) == null)
		{
			return false;
		}
		return true;
	}

	private static bool IsHeroValidForAdventure(AdventureDbId adventureId, int heroCardDbId)
	{
		if (heroCardDbId == 0)
		{
			return false;
		}
		foreach (GuestHero item in GetGuestHeroesForAdventure(adventureId))
		{
			if (heroCardDbId == item.cardDbId)
			{
				return true;
			}
		}
		return false;
	}

	private static bool IsHeroClassValidForAdventure(AdventureDbId adventureId, TAG_CLASS heroClass)
	{
		if (heroClass == TAG_CLASS.INVALID)
		{
			return false;
		}
		foreach (GuestHero item in GetGuestHeroesForAdventure(adventureId))
		{
			if (heroClass == GetHeroClassFromHeroId(item.cardDbId))
			{
				return true;
			}
		}
		return false;
	}

	public static bool IsHeroPowerValidForClassAndAdventure(AdventureDbId adventureId, TAG_CLASS heroClass, int heroPowerDbId)
	{
		if (adventureId == AdventureDbId.INVALID || heroPowerDbId <= 0)
		{
			return false;
		}
		return GameDbf.AdventureHeroPower.HasRecord((AdventureHeroPowerDbfRecord r) => r.CardId == heroPowerDbId && r.AdventureId == (int)adventureId && heroClass == (TAG_CLASS)r.ClassId);
	}

	public static bool IsHeroPowerValidForHeroAndAdventure(AdventureDbId adventureId, int heroCardDbId, int heroPowerDbId)
	{
		if (adventureId == AdventureDbId.INVALID || heroPowerDbId <= 0)
		{
			return false;
		}
		AdventureGuestHeroesDbfRecord adventureGuestHeroRecord = GetGuestHeroRecordForAdventure(adventureId, heroCardDbId);
		int guestHeroIdToUse = adventureGuestHeroRecord?.BaseGuestHeroId ?? 0;
		if (guestHeroIdToUse == 0)
		{
			guestHeroIdToUse = adventureGuestHeroRecord?.GuestHeroId ?? 0;
		}
		if (guestHeroIdToUse != 0)
		{
			return GameDbf.AdventureHeroPower.HasRecord((AdventureHeroPowerDbfRecord r) => r.CardId == heroPowerDbId && r.AdventureId == (int)adventureId && r.GuestHeroId == guestHeroIdToUse);
		}
		return false;
	}

	public static bool IsDeckValidForClassAndAdventure(AdventureDbId adventureId, TAG_CLASS heroClass, int deckDbId)
	{
		if (adventureId == AdventureDbId.INVALID || deckDbId <= 0)
		{
			return false;
		}
		return GameDbf.AdventureDeck.HasRecord((AdventureDeckDbfRecord r) => r.DeckId == deckDbId && r.AdventureId == (int)adventureId && heroClass == (TAG_CLASS)r.ClassId);
	}

	public static bool IsLoadoutTreasureValidForClassAndAdventure(AdventureDbId adventureId, TAG_CLASS heroClass, int treasureDbId)
	{
		if (adventureId == AdventureDbId.INVALID || treasureDbId <= 0)
		{
			return false;
		}
		return GameDbf.AdventureLoadoutTreasures.HasRecord((AdventureLoadoutTreasuresDbfRecord r) => (r.CardId == treasureDbId || r.UpgradedCardId == treasureDbId) && r.AdventureId == (int)adventureId && heroClass == (TAG_CLASS)r.ClassId);
	}

	public static bool IsLoadoutTreasureValidForHeroAndAdventure(AdventureDbId adventureId, int heroCardDbId, int treasureDbId)
	{
		if (adventureId == AdventureDbId.INVALID || treasureDbId <= 0)
		{
			return false;
		}
		AdventureGuestHeroesDbfRecord adventureGuestHeroRecord = GetGuestHeroRecordForAdventure(adventureId, heroCardDbId);
		int guestHeroIdToUse = adventureGuestHeroRecord?.BaseGuestHeroId ?? 0;
		if (guestHeroIdToUse == 0)
		{
			guestHeroIdToUse = adventureGuestHeroRecord?.GuestHeroId ?? 0;
		}
		if (guestHeroIdToUse != 0)
		{
			return GameDbf.AdventureLoadoutTreasures.HasRecord((AdventureLoadoutTreasuresDbfRecord r) => (r.CardId == treasureDbId || r.UpgradedCardId == treasureDbId) && r.AdventureId == (int)adventureId && r.GuestHeroId == guestHeroIdToUse);
		}
		return false;
	}

	public static TAG_CLASS GetHeroClassFromHeroId(int heroCardDbId)
	{
		return GameUtils.GetTagClassFromCardDbId(heroCardDbId);
	}

	public static bool IsValidLoadoutForSelectedAdventureAndClass(AdventureDbId adventureDbId, AdventureModeDbId modeDbId, ScenarioDbId scenarioDbId, TAG_CLASS heroClass, int heroPowerDbId, int deckDbId, int treasureDbId)
	{
		if (!IsMissionValidForAdventureMode(adventureDbId, modeDbId, scenarioDbId))
		{
			Debug.LogFormat("AdventureUtils.IsValidLoadoutForSelectedAdventureAndClass - invalid scenario ID: {0} for adventure ID: {1} with mode ID: {2}.", scenarioDbId, adventureDbId, modeDbId);
			return false;
		}
		if (!IsHeroClassValidForAdventure(adventureDbId, heroClass))
		{
			Debug.LogFormat("AdventureUtils.IsValidLoadoutForSelectedAdventureAndClass - invalid hero class {0} for adventure ID: {1}.", heroClass, adventureDbId);
			return false;
		}
		if (SelectableHeroPowersAndDecksExistForAdventure(adventureDbId))
		{
			if (!IsHeroPowerValidForClassAndAdventure(adventureDbId, heroClass, heroPowerDbId))
			{
				Debug.LogFormat("AdventureUtils.IsValidLoadoutForSelectedAdventureAndClass - invalid loadout hero power ID: {0} for adventure ID: {1}", heroPowerDbId, adventureDbId);
				return false;
			}
			if (!IsDeckValidForClassAndAdventure(adventureDbId, heroClass, deckDbId))
			{
				Debug.LogFormat("AdventureUtils.IsValidLoadoutForSelectedAdventureAndClass - invalid loadout deck ID: {0} for adventure ID: {1}.", deckDbId, adventureDbId);
				return false;
			}
		}
		if (SelectableLoadoutTreasuresExistForAdventure(adventureDbId) && !IsLoadoutTreasureValidForClassAndAdventure(adventureDbId, heroClass, treasureDbId))
		{
			Debug.LogFormat("AdventureUtils.IsValidLoadoutForSelectedAdventureAndClass - invalid loadout treasure ID: {0} for adventure ID: {1}.", treasureDbId, adventureDbId);
			return false;
		}
		return true;
	}

	public static bool IsValidLoadoutForSelectedAdventureAndHero(AdventureDbId adventureDbId, AdventureModeDbId modeDbId, ScenarioDbId scenarioDbId, int heroCardDbId, int heroPowerDbId, int treasureDbId)
	{
		if (!IsMissionValidForAdventureMode(adventureDbId, modeDbId, scenarioDbId))
		{
			Debug.LogFormat("AdventureUtils.IsValidLoadoutForSelectedAdventureAndHero - invalid scenario ID: {0} for adventure ID: {1} with mode ID: {2}.", scenarioDbId, adventureDbId, modeDbId);
			return false;
		}
		if (!IsHeroValidForAdventure(adventureDbId, heroCardDbId))
		{
			Debug.LogFormat("AdventureUtils.IsValidLoadoutForSelectedAdventureAndHero - invalid hero {0} for adventure ID: {1}.", heroCardDbId, adventureDbId);
			return false;
		}
		if (SelectableHeroPowersAndDecksExistForAdventure(adventureDbId))
		{
			if (!IsHeroPowerValidForHeroAndAdventure(adventureDbId, heroCardDbId, heroPowerDbId))
			{
				Debug.LogFormat("AdventureUtils.IsValidLoadoutForSelectedAdventureAndHero - invalid loadout hero power ID: {0} for adventure ID: {1}", heroPowerDbId, adventureDbId);
				return false;
			}
			if (SelectableDecksExistForAdventure(adventureDbId))
			{
				Debug.LogError("AdventureUtils.IsValidLoadoutForSelectedAdventureAndHero - Adventure decks referenced by Hero DB ID is not currently supported!");
			}
		}
		if (SelectableLoadoutTreasuresExistForAdventure(adventureDbId) && !IsLoadoutTreasureValidForHeroAndAdventure(adventureDbId, heroCardDbId, treasureDbId))
		{
			Debug.LogFormat("AdventureUtils.IsValidLoadoutForSelectedAdventureAndHero - invalid loadout treasure ID: {0} for adventure ID: {1}.", treasureDbId, adventureDbId);
			return false;
		}
		return true;
	}

	public static bool CanPlayWingOpenQuote(AdventureWingDef wingDef)
	{
		if (wingDef != null && !string.IsNullOrEmpty(wingDef.m_OpenQuotePrefab) && !string.IsNullOrEmpty(wingDef.m_OpenQuoteVOLine))
		{
			if (!wingDef.m_PlayOpenQuoteInHeroic)
			{
				return !GameUtils.IsModeHeroic(AdventureConfig.Get().GetSelectedMode());
			}
			return true;
		}
		return false;
	}

	public static bool CanPlayWingCompleteQuote(AdventureWingDef wingDef)
	{
		if (wingDef != null && !string.IsNullOrEmpty(wingDef.m_CompleteQuotePrefab) && !string.IsNullOrEmpty(wingDef.m_CompleteQuoteVOLine))
		{
			if (!wingDef.m_PlayCompleteQuoteInHeroic)
			{
				return !GameUtils.IsModeHeroic(AdventureConfig.Get().GetSelectedMode());
			}
			return true;
		}
		return false;
	}

	public static void PlayMissionQuote(AdventureBossDef bossDef, Vector3 position)
	{
		if (bossDef == null)
		{
			return;
		}
		string introLine = bossDef.GetIntroLine();
		if (!string.IsNullOrEmpty(introLine))
		{
			AdventureDef advDef = AdventureScene.Get().GetAdventureDef(AdventureConfig.Get().GetSelectedAdventure());
			string quotePrefab = null;
			if (advDef != null)
			{
				quotePrefab = advDef.m_DefaultQuotePrefab;
			}
			if (!string.IsNullOrEmpty(bossDef.m_quotePrefabOverride))
			{
				quotePrefab = bossDef.m_quotePrefabOverride;
			}
			string gameString = new AssetReference(introLine).GetLegacyAssetName();
			if (!string.IsNullOrEmpty(quotePrefab))
			{
				bool allowRepeat = AdventureScene.Get() != null && AdventureScene.Get().IsDevMode;
				NotificationManager.Get().CreateCharacterQuote(quotePrefab, position, GameStrings.Get(gameString), introLine, allowRepeat);
			}
		}
	}
}
