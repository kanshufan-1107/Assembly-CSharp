using System.Collections.Generic;
using PegasusShared;

namespace Hearthstone.DungeonCrawl;

public interface IDungeonCrawlData
{
	long SelectedHeroCardDbId { get; set; }

	long SelectedDeckId { get; set; }

	long SelectedHeroPowerDbId { get; set; }

	long SelectedLoadoutTreasureDbId { get; set; }

	bool AnomalyModeActivated { get; set; }

	bool IsDevMode { get; }

	DungeonRunVisualStyle VisualStyle { get; }

	GameType GameType { get; }

	AdventureDbId GetSelectedAdventure();

	AdventureModeDbId GetSelectedMode();

	void SetMission(ScenarioDbId mission, bool showDetails = true);

	ScenarioDbId GetMission();

	bool SelectableHeroPowersExist();

	bool SelectableDecksExist();

	bool SelectableHeroPowersAndDecksExist();

	bool SelectableLoadoutTreasuresExist();

	void SetMissionOverride(ScenarioDbId missionOverride);

	ScenarioDbId GetMissionOverride();

	ScenarioDbId GetMissionToPlay();

	int GetAdventureBossesInRun(WingDbfRecord wingRecord);

	bool HeroIsSelectedBeforeDungeonCrawlScreenForSelectedAdventure();

	List<GuestHero> GetGuestHeroesForCurrentAdventure();

	bool GuestHeroesExistForCurrentAdventure();

	bool DoesSelectedMissionRequireDeck();

	List<AdventureHeroPowerDbfRecord> GetHeroPowersForClass(TAG_CLASS classId);

	List<AdventureHeroPowerDbfRecord> GetHeroPowersForGuestHero(int guestHeroId);

	List<AdventureDeckDbfRecord> GetDecksForClass(TAG_CLASS classId);

	List<AdventureLoadoutTreasuresDbfRecord> GetLoadoutTreasuresForClass(TAG_CLASS classId);

	List<AdventureLoadoutTreasuresDbfRecord> GetLoadoutTreasuresForGuestHero(int guestHeroId);

	bool AdventureHeroPowerIsUnlocked(GameSaveKeyId gameSaveServerKey, AdventureHeroPowerDbfRecord heroPowerRecord, out long unlockProgress, out bool hasUnlockCriteria);

	bool AdventureDeckIsUnlocked(GameSaveKeyId gameSaveServerKey, AdventureDeckDbfRecord deckRecord, out long unlockProgress, out bool hasUnlockCriteria);

	bool AdventureTreasureIsUnlocked(GameSaveKeyId gameSaveServerKey, AdventureLoadoutTreasuresDbfRecord treasureLoadoutRecord, out long unlockProgress, out bool hasUnlockCriteria);

	bool AdventureTreasureIsUpgraded(GameSaveKeyId gameSaveServerKey, AdventureLoadoutTreasuresDbfRecord treasureLoadoutRecord, out long upgradeProgress);

	void SetSelectedAdventureMode(AdventureDbId adventureId, AdventureModeDbId modeId);

	bool AdventureRewardIsUnlocked(GameSaveKeyId gameSaveServerKey, GameSaveKeySubkeyId unlockGameSaveSubkey, int unlockValue, int unlockAchievement, out long unlockProgress, out bool hasUnlockCriteria);

	AdventureDef GetAdventureDef();

	GameSaveKeyId GetGameSaveClientKey();

	AdventureWingDef GetWingDef(WingDbId wingId);

	AdventureDataDbfRecord GetSelectedAdventureDataRecord();

	bool HasValidLoadoutForSelectedAdventure();
}
