using System.Collections.Generic;
using UnityEngine;

namespace Hearthstone.DungeonCrawl;

public class DungeonCrawlUtil
{
	public delegate void DungeonRunLoadCallback(GameObject go);

	public delegate void DungeonRunDataLoadedCallback(bool succes);

	private static readonly PlatformDependentValue<string> DUNGEON_RUN_PREFAB_ASSET = new PlatformDependentValue<string>(PlatformCategory.Screen)
	{
		PC = "AdventureDungeonCrawl.prefab:13231000c27ce7d4ebf2bff57e48b8c2",
		Phone = "AdventureDungeonCrawl_phone.prefab:f28e6d94ab29c6145a390a6a2346195a"
	};

	public static DungeonCrawlServices CreateAdventureDungeonCrawlServices(AssetLoadingHelper assetLoadingHelper)
	{
		return new DungeonCrawlServices
		{
			DungeonCrawlData = new DungeonCrawlDataAdventureAdapter(),
			SubsceneController = new DungeonCrawlSubscenControllerAdapter(),
			AssetLoadingHelper = assetLoadingHelper
		};
	}

	public static DungeonCrawlServices CreateTavernBrawlDungeonCrawlServices(AdventureDbId adventureId, AdventureModeDbId modeId, AssetLoadingHelper assetLoadingHelper)
	{
		ScenarioDbId missionId = GetMissionForAdventure(adventureId, modeId);
		WingDbfRecord wingRecord = GameUtils.GetWingRecordFromMissionId((int)missionId);
		DungeonCrawlDataTavernBrawl.DataSet dataSet = default(DungeonCrawlDataTavernBrawl.DataSet);
		dataSet.m_selectedAdventure = adventureId;
		dataSet.m_selectedMode = modeId;
		dataSet.m_mission = missionId;
		dataSet.m_selectableHeroPowersExist = AdventureUtils.SelectableHeroPowersExistForAdventure(adventureId);
		dataSet.m_selectableDecksExist = AdventureUtils.SelectableDecksExistForAdventure(adventureId);
		dataSet.m_selectableHeroPowersAndDecksExist = AdventureUtils.SelectableHeroPowersAndDecksExistForAdventure(adventureId);
		dataSet.m_selectableLoadoutTreasuresExist = AdventureUtils.SelectableLoadoutTreasuresExistForAdventure(adventureId);
		dataSet.m_bossesInRun = AdventureConfig.GetAdventureBossesInRun(wingRecord);
		dataSet.m_heroIsSelectedBeforeDungeonCrawlScreenForSelectedAdventure = false;
		dataSet.m_guestHeroes = GetGuestHeroesForTavernBrawl((int)missionId);
		dataSet.m_requiresDeck = AdventureConfig.DoesMissionRequireDeck(missionId);
		dataSet.m_gameSaveClientKey = GetClientSaveKey(adventureId, modeId);
		DungeonCrawlDataTavernBrawl.DataSet dataSet2 = dataSet;
		return new DungeonCrawlServices
		{
			DungeonCrawlData = new DungeonCrawlDataTavernBrawl(dataSet2),
			SubsceneController = new DungoneCrawlTavernBrawlSubsceneController(),
			AssetLoadingHelper = assetLoadingHelper
		};
	}

	public static void LoadDungeonRunPrefab(DungeonRunLoadCallback callback)
	{
		AssetReference prefab = new AssetReference(DUNGEON_RUN_PREFAB_ASSET);
		AssetLoader.Get().InstantiatePrefab(prefab, delegate(AssetReference assetRef, GameObject go, object data)
		{
			if (callback != null)
			{
				callback(go);
			}
		}, null, AssetLoadingOptions.IgnorePrefabPosition);
	}

	public static bool IsDungeonRunDataReady(AdventureDbId adventureId, AdventureModeDbId adventureModeId)
	{
		AdventureDataDbfRecord dataRecord = GameUtils.GetAdventureDataRecord((int)adventureId, (int)adventureModeId);
		if (dataRecord == null)
		{
			return false;
		}
		GameSaveKeyId gameSaveDataServerKey = (GameSaveKeyId)dataRecord.GameSaveDataServerKey;
		GameSaveKeyId gameSaveDataClientKey = (GameSaveKeyId)dataRecord.GameSaveDataClientKey;
		if (GameSaveDataManager.Get().IsDataReady(gameSaveDataServerKey))
		{
			return GameSaveDataManager.Get().IsDataReady(gameSaveDataClientKey);
		}
		return false;
	}

	public static void ClearDungeonRunServerData(AdventureDbId adventureId, AdventureModeDbId adventureModeId)
	{
		AdventureDataDbfRecord dataRecord = GameUtils.GetAdventureDataRecord((int)adventureId, (int)adventureModeId);
		if (dataRecord != null)
		{
			GameSaveKeyId gameSaveDataServerKey = (GameSaveKeyId)dataRecord.GameSaveDataServerKey;
			GameSaveDataManager.Get().ClearLocalData(gameSaveDataServerKey);
		}
	}

	public static void LoadDungeonRunData(AdventureDbId adventureId, AdventureModeDbId adventureModeId, DungeonRunDataLoadedCallback callback)
	{
		AdventureDataDbfRecord dataRecord = GameUtils.GetAdventureDataRecord((int)adventureId, (int)adventureModeId);
		if (dataRecord == null)
		{
			if (callback != null)
			{
				callback(succes: false);
			}
			return;
		}
		GameSaveKeyId gameSaveDataServerKey = (GameSaveKeyId)dataRecord.GameSaveDataServerKey;
		GameSaveKeyId gameSaveDataClientKey = (GameSaveKeyId)dataRecord.GameSaveDataClientKey;
		bool clientDataRetrieved = false;
		bool serverDataRetrieved = false;
		bool resultSuccesfull = true;
		GameSaveDataManager.Get().Request(gameSaveDataServerKey, delegate(bool success)
		{
			serverDataRetrieved = true;
			resultSuccesfull &= success;
			if (serverDataRetrieved && clientDataRetrieved)
			{
				callback(resultSuccesfull);
			}
		});
		GameSaveDataManager.Get().Request(gameSaveDataClientKey, delegate(bool success)
		{
			clientDataRetrieved = true;
			resultSuccesfull &= success;
			if (serverDataRetrieved && clientDataRetrieved)
			{
				callback(resultSuccesfull);
			}
		});
	}

	public static bool IsDungeonRunInProgress(AdventureDbId adventureId, AdventureModeDbId adventureModeId)
	{
		AdventureDataDbfRecord dataRecord = GameUtils.GetAdventureDataRecord((int)adventureId, (int)adventureModeId);
		if (dataRecord == null)
		{
			return false;
		}
		GameSaveKeyId gameSaveDataServerKey = (GameSaveKeyId)dataRecord.GameSaveDataServerKey;
		if (!GameSaveDataManager.Get().ValidateIfKeyCanBeAccessed(gameSaveDataServerKey, dataRecord.Name))
		{
			return false;
		}
		GameSaveDataManager.Get().GetSubkeyValue(gameSaveDataServerKey, GameSaveKeySubkeyId.DUNGEON_CRAWL_IS_RUN_RETIRED, out long isRunRetired);
		if (isRunRetired > 0)
		{
			return false;
		}
		GameSaveDataManager.Get().GetSubkeyValue(gameSaveDataServerKey, GameSaveKeySubkeyId.DUNGEON_CRAWL_IS_RUN_ACTIVE, out long isRunActive);
		if (isRunActive > 0)
		{
			return true;
		}
		long hasSeenLatestDungeonRunComplete = 0L;
		long bossWhoDefeatedMeId = 0L;
		List<long> defeatedBossIds = null;
		GameSaveDataManager.Get().GetSubkeyValue(gameSaveDataServerKey, GameSaveKeySubkeyId.DUNGEON_CRAWL_BOSS_LOST_TO, out bossWhoDefeatedMeId);
		GameSaveDataManager.Get().GetSubkeyValue(gameSaveDataServerKey, GameSaveKeySubkeyId.DUNGEON_CRAWL_BOSSES_DEFEATED, out defeatedBossIds);
		GameSaveDataManager.Get().GetSubkeyValue(gameSaveDataServerKey, GameSaveKeySubkeyId.DUNGEON_CRAWL_HAS_SEEN_LATEST_DUNGEON_RUN_COMPLETE, out hasSeenLatestDungeonRunComplete);
		bool num = bossWhoDefeatedMeId > 0;
		bool hasDefeatedBosses = defeatedBossIds != null && defeatedBossIds.Count > 0;
		bool hasFoughtBosses = num || hasDefeatedBosses;
		return hasSeenLatestDungeonRunComplete == 0 && hasFoughtBosses;
	}

	private static ScenarioDbId GetMissionForAdventure(AdventureDbId adv, AdventureModeDbId mode)
	{
		List<ScenarioDbfRecord> scenarioRecords = GameDbf.Scenario.GetRecords((ScenarioDbfRecord r) => r.AdventureId == (int)adv && r.ModeId == (int)mode && r.WingId != 0);
		if (scenarioRecords == null || scenarioRecords.Count < 1)
		{
			return ScenarioDbId.INVALID;
		}
		if (scenarioRecords.Count > 1)
		{
			Log.Adventures.PrintWarning("Unexpected number of scenarios found for adventure {0} mode {1}. There should be only one but found {2}", adv, mode, scenarioRecords.Count);
		}
		scenarioRecords.Sort((ScenarioDbfRecord a, ScenarioDbfRecord b) => a.SortOrder.CompareTo(b.SortOrder));
		return (ScenarioDbId)scenarioRecords[0].ID;
	}

	private static GameSaveKeyId GetClientSaveKey(AdventureDbId adv, AdventureModeDbId mode)
	{
		return (GameSaveKeyId)(GameUtils.GetAdventureDataRecord((int)adv, (int)mode)?.GameSaveDataClientKey ?? (-1));
	}

	public static List<GuestHero> GetGuestHeroesForTavernBrawl(int scenarioId)
	{
		List<ScenarioGuestHeroesDbfRecord> records = GameDbf.ScenarioGuestHeroes.GetRecords((ScenarioGuestHeroesDbfRecord r) => r.ScenarioId == scenarioId);
		records.Sort((ScenarioGuestHeroesDbfRecord a, ScenarioGuestHeroesDbfRecord b) => a.SortOrder.CompareTo(b.SortOrder));
		List<GuestHero> cardDbIds = new List<GuestHero>();
		foreach (ScenarioGuestHeroesDbfRecord record in records)
		{
			cardDbIds.Add(new GuestHero
			{
				guestHeroId = record.GuestHeroId,
				cardDbId = GameUtils.GetCardIdFromGuestHeroDbId(record.GuestHeroId)
			});
		}
		return cardDbIds;
	}

	public static bool MigrateDungeonCrawlSubkeys(GameSaveKeyId clientKey, GameSaveKeyId serverKey)
	{
		if (!GameSaveDataManager.Get().ValidateIfKeyCanBeAccessed(clientKey, "MigrateDungeonCrawlSubkeys") || !GameSaveDataManager.Get().ValidateIfKeyCanBeAccessed(serverKey, "MigrateDungeonCrawlSubkeys"))
		{
			Log.Adventures.Print("MigrateDungeonCrawlSubkeys called but one or both keys cannot be migrated. Client key: {0}  Server key: {1}", clientKey, serverKey);
			return false;
		}
		bool migrationPerformed = false;
		if (GameSaveDataManager.Get().MigrateSubkeyIntValue(clientKey, serverKey, GameSaveKeySubkeyId.DUNGEON_CRAWL_HAS_SEEN_LATEST_DUNGEON_RUN_COMPLETE, 0L))
		{
			Log.Adventures.Print("Migrated DUNGEON_CRAWL_HAS_SEEN_LATEST_DUNGEON_RUN_COMPLETE subkey from client to server key!");
			migrationPerformed = true;
		}
		if (GameSaveDataManager.Get().MigrateSubkeyIntValue(clientKey, serverKey, GameSaveKeySubkeyId.DUNGEON_CRAWL_PLAYER_SELECTED_HERO_CLASS, 0L))
		{
			Log.Adventures.Print("Migrated DUNGEON_CRAWL_PLAYER_SELECTED_HERO_CLASS subkey from client to server key!");
			migrationPerformed = true;
		}
		return migrationPerformed;
	}

	public static bool IsDungeonRunActive(GameSaveKeyId serverKey)
	{
		GameSaveDataManager.Get().GetSubkeyValue(serverKey, GameSaveKeySubkeyId.DUNGEON_CRAWL_IS_RUN_ACTIVE, out long isRunActive);
		GameSaveDataManager.Get().GetSubkeyValue(serverKey, GameSaveKeySubkeyId.DUNGEON_CRAWL_IS_RUN_RETIRED, out long isRunRetired);
		if (isRunActive > 0)
		{
			return isRunRetired <= 0;
		}
		return false;
	}

	public static bool IsPVPDRFriendlyEncounter(int missionId)
	{
		if (missionId == 3745)
		{
			return true;
		}
		return false;
	}

	public static List<DbfRecord> CheckForNewlyUnlockedGSDRewardsOfSpecificType(IEnumerable<DbfRecord> rewardDbfRecords, IDungeonCrawlData dungeonCrawlData, GameSaveKeyId gameSaveServerKey, GameSaveKeyId gameSaveClientKey, GameSaveKeySubkeyId awardedRewardsSubkey, GameSaveKeySubkeyId newlyUnlockedRewardsSubkey, List<GameSaveDataManager.SubkeySaveRequest> subkeyUpdates, bool checkForUpgrades = false)
	{
		List<DbfRecord> justUnlockedRewards = new List<DbfRecord>();
		List<long> justUnlockedRewardIds = new List<long>();
		GameSaveDataManager.Get().GetSubkeyValue(gameSaveClientKey, awardedRewardsSubkey, out List<long> awardedRewards);
		bool hasUnlockCriteria = default(bool);
		foreach (DbfRecord rewardRecord in rewardDbfRecords)
		{
			int unlockAchievement = 0;
			long rewardId;
			GameSaveKeySubkeyId unlockSubkey;
			int unlockValue;
			if (rewardRecord is AdventureHeroPowerDbfRecord)
			{
				AdventureHeroPowerDbfRecord obj = (AdventureHeroPowerDbfRecord)rewardRecord;
				rewardId = obj.CardId;
				unlockSubkey = (GameSaveKeySubkeyId)obj.UnlockGameSaveSubkey;
				unlockValue = obj.UnlockValue;
				unlockAchievement = obj.UnlockAchievement;
			}
			else if (rewardRecord is AdventureDeckDbfRecord)
			{
				AdventureDeckDbfRecord obj2 = (AdventureDeckDbfRecord)rewardRecord;
				rewardId = obj2.DeckId;
				unlockSubkey = (GameSaveKeySubkeyId)obj2.UnlockGameSaveSubkey;
				unlockValue = obj2.UnlockValue;
			}
			else
			{
				if (!(rewardRecord is AdventureLoadoutTreasuresDbfRecord))
				{
					Log.Adventures.PrintWarning("Unsupported DbfRecord type in CheckForNewlyUnlockedRewardsOfSpecificType()!");
					return justUnlockedRewards;
				}
				AdventureLoadoutTreasuresDbfRecord loadoutTreasureRecord = (AdventureLoadoutTreasuresDbfRecord)rewardRecord;
				if (checkForUpgrades)
				{
					rewardId = loadoutTreasureRecord.UpgradedCardId;
					unlockSubkey = (GameSaveKeySubkeyId)loadoutTreasureRecord.UpgradeGameSaveSubkey;
					unlockValue = loadoutTreasureRecord.UpgradeValue;
				}
				else
				{
					rewardId = loadoutTreasureRecord.CardId;
					unlockSubkey = (GameSaveKeySubkeyId)loadoutTreasureRecord.UnlockGameSaveSubkey;
					unlockValue = loadoutTreasureRecord.UnlockValue;
				}
				unlockAchievement = loadoutTreasureRecord.UnlockAchievement;
			}
			if ((awardedRewards == null || !awardedRewards.Contains(rewardId)) && dungeonCrawlData.AdventureRewardIsUnlocked(gameSaveServerKey, unlockSubkey, unlockValue, unlockAchievement, out var _, out hasUnlockCriteria) && hasUnlockCriteria)
			{
				justUnlockedRewards.Add(rewardRecord);
				justUnlockedRewardIds.Add(rewardId);
			}
		}
		if (justUnlockedRewards.Count > 0)
		{
			switch (awardedRewardsSubkey)
			{
			case GameSaveKeySubkeyId.DUNGEON_CRAWL_AWARDED_HERO_POWERS:
				Log.Adventures.Print("Just Unlocked Hero Powers: {0}", justUnlockedRewards);
				break;
			case GameSaveKeySubkeyId.DUNGEON_CRAWL_AWARDED_DECKS:
				Log.Adventures.Print("Just Unlocked Decks: {0}", justUnlockedRewards);
				break;
			case GameSaveKeySubkeyId.DUNGEON_CRAWL_AWARDED_LOADOUT_TREASURES:
				Log.Adventures.Print("Just Unlocked or Upgraded Loadout Treasures: {0}", justUnlockedRewards);
				break;
			}
			if (awardedRewards == null)
			{
				awardedRewards = new List<long>();
			}
			awardedRewards.AddRange(justUnlockedRewardIds);
			AddSubkeyValuesToGameSaveDataUpdates(awardedRewards, ref subkeyUpdates, gameSaveClientKey, awardedRewardsSubkey);
			GameSaveDataManager.Get().GetSubkeyValue(gameSaveClientKey, newlyUnlockedRewardsSubkey, out List<long> newlyUnlockedRewards);
			if (newlyUnlockedRewards == null)
			{
				newlyUnlockedRewards = new List<long>();
			}
			foreach (long justUnlockedRewardId in justUnlockedRewardIds)
			{
				if (!newlyUnlockedRewards.Contains(justUnlockedRewardId))
				{
					newlyUnlockedRewards.Add(justUnlockedRewardId);
				}
			}
			AddSubkeyValuesToGameSaveDataUpdates(newlyUnlockedRewards, ref subkeyUpdates, gameSaveClientKey, newlyUnlockedRewardsSubkey);
		}
		return justUnlockedRewards;
	}

	private static void AddSubkeyValuesToGameSaveDataUpdates(List<long> valuesToAdd, ref List<GameSaveDataManager.SubkeySaveRequest> subkeyUpdates, GameSaveKeyId gameSaveKey, GameSaveKeySubkeyId gameSaveSubkey)
	{
		if (valuesToAdd == null || valuesToAdd.Count <= 0 || subkeyUpdates == null)
		{
			Debug.LogWarning("Invalid parameters passed to AddSubkeyValuesToGameSaveDataUpdates()!");
			return;
		}
		List<long> values = new List<long>(valuesToAdd);
		GameSaveDataManager.SubkeySaveRequest existingRequest = subkeyUpdates.Find((GameSaveDataManager.SubkeySaveRequest r) => r.Key == gameSaveKey && r.Subkey == gameSaveSubkey);
		if (existingRequest != null)
		{
			long[] long_Values = existingRequest.Long_Values;
			foreach (long existingValue in long_Values)
			{
				if (!values.Contains(existingValue))
				{
					values.Add(existingValue);
				}
			}
			subkeyUpdates.Remove(existingRequest);
		}
		subkeyUpdates.Add(new GameSaveDataManager.SubkeySaveRequest(gameSaveKey, gameSaveSubkey, values.ToArray()));
	}
}
