using System.Collections.Generic;
using PegasusShared;

namespace Hearthstone.DungeonCrawl;

public class DungeonCrawlDataAdventureAdapter : IDungeonCrawlData
{
	public long SelectedHeroCardDbId
	{
		get
		{
			AdventureConfig ac = AdventureConfig.Get();
			if (ac == null)
			{
				return 0L;
			}
			return ac.SelectedHeroCardDbId;
		}
		set
		{
			AdventureConfig ac = AdventureConfig.Get();
			if (!(ac == null))
			{
				ac.SelectedHeroCardDbId = value;
			}
		}
	}

	public long SelectedDeckId
	{
		get
		{
			AdventureConfig ac = AdventureConfig.Get();
			if (ac == null)
			{
				return 0L;
			}
			return ac.SelectedDeckId;
		}
		set
		{
			AdventureConfig ac = AdventureConfig.Get();
			if (!(ac == null))
			{
				ac.SelectedDeckId = value;
			}
		}
	}

	public long SelectedHeroPowerDbId
	{
		get
		{
			AdventureConfig ac = AdventureConfig.Get();
			if (ac == null)
			{
				return 0L;
			}
			return ac.SelectedHeroPowerDbId;
		}
		set
		{
			AdventureConfig ac = AdventureConfig.Get();
			if (!(ac == null))
			{
				ac.SelectedHeroPowerDbId = value;
			}
		}
	}

	public long SelectedLoadoutTreasureDbId
	{
		get
		{
			AdventureConfig ac = AdventureConfig.Get();
			if (ac == null)
			{
				return 0L;
			}
			return ac.SelectedLoadoutTreasureDbId;
		}
		set
		{
			AdventureConfig ac = AdventureConfig.Get();
			if (!(ac == null))
			{
				ac.SelectedLoadoutTreasureDbId = value;
			}
		}
	}

	public bool AnomalyModeActivated
	{
		get
		{
			AdventureConfig ac = AdventureConfig.Get();
			if (ac == null)
			{
				return false;
			}
			return ac.AnomalyModeActivated;
		}
		set
		{
			AdventureConfig ac = AdventureConfig.Get();
			if (!(ac == null))
			{
				ac.AnomalyModeActivated = value;
			}
		}
	}

	public bool IsDevMode
	{
		get
		{
			AdventureScene adventureScene = AdventureScene.Get();
			if (adventureScene == null)
			{
				return false;
			}
			return adventureScene.IsDevMode;
		}
	}

	public DungeonRunVisualStyle VisualStyle => (DungeonRunVisualStyle)GetSelectedAdventure();

	public GameType GameType => GameType.GT_VS_AI;

	public AdventureDbId GetSelectedAdventure()
	{
		AdventureConfig ac = AdventureConfig.Get();
		if (ac == null)
		{
			return AdventureDbId.INVALID;
		}
		return ac.GetSelectedAdventure();
	}

	public AdventureModeDbId GetSelectedMode()
	{
		AdventureConfig ac = AdventureConfig.Get();
		if (ac == null)
		{
			return AdventureModeDbId.INVALID;
		}
		return ac.GetSelectedMode();
	}

	public void SetMission(ScenarioDbId mission, bool showDetails = true)
	{
		AdventureConfig ac = AdventureConfig.Get();
		if (!(ac == null))
		{
			ac.SetMission(mission, showDetails);
		}
	}

	public ScenarioDbId GetMission()
	{
		AdventureConfig ac = AdventureConfig.Get();
		if (ac == null)
		{
			return ScenarioDbId.INVALID;
		}
		return ac.GetMission();
	}

	public bool SelectableHeroPowersExist()
	{
		AdventureConfig ac = AdventureConfig.Get();
		if (ac == null)
		{
			return false;
		}
		return AdventureUtils.SelectableHeroPowersExistForAdventure(ac.GetSelectedAdventure());
	}

	public bool SelectableDecksExist()
	{
		AdventureConfig ac = AdventureConfig.Get();
		if (ac == null)
		{
			return false;
		}
		return AdventureUtils.SelectableDecksExistForAdventure(ac.GetSelectedAdventure());
	}

	public bool SelectableHeroPowersAndDecksExist()
	{
		AdventureConfig ac = AdventureConfig.Get();
		if (ac == null)
		{
			return false;
		}
		return AdventureUtils.SelectableHeroPowersAndDecksExistForAdventure(ac.GetSelectedAdventure());
	}

	public bool SelectableLoadoutTreasuresExist()
	{
		AdventureConfig ac = AdventureConfig.Get();
		if (ac == null)
		{
			return false;
		}
		return AdventureUtils.SelectableLoadoutTreasuresExistForAdventure(ac.GetSelectedAdventure());
	}

	public void SetMissionOverride(ScenarioDbId missionOverride)
	{
		AdventureConfig ac = AdventureConfig.Get();
		if (!(ac == null))
		{
			ac.SetMissionOverride(missionOverride);
		}
	}

	public ScenarioDbId GetMissionOverride()
	{
		AdventureConfig ac = AdventureConfig.Get();
		if (ac == null)
		{
			return ScenarioDbId.INVALID;
		}
		return ac.GetMissionOverride();
	}

	public ScenarioDbId GetMissionToPlay()
	{
		AdventureConfig ac = AdventureConfig.Get();
		if (ac == null)
		{
			return ScenarioDbId.INVALID;
		}
		return ac.GetMissionToPlay();
	}

	public int GetAdventureBossesInRun(WingDbfRecord wingRecord)
	{
		return AdventureConfig.GetAdventureBossesInRun(wingRecord);
	}

	public bool HeroIsSelectedBeforeDungeonCrawlScreenForSelectedAdventure()
	{
		AdventureConfig ac = AdventureConfig.Get();
		if (ac == null)
		{
			return false;
		}
		return ac.IsHeroSelectedBeforeDungeonCrawlScreenForSelectedAdventure();
	}

	public List<GuestHero> GetGuestHeroesForCurrentAdventure()
	{
		AdventureConfig ac = AdventureConfig.Get();
		if (ac == null)
		{
			return new List<GuestHero>();
		}
		return ac.GetGuestHeroesForCurrentAdventure();
	}

	public bool GuestHeroesExistForCurrentAdventure()
	{
		AdventureConfig ac = AdventureConfig.Get();
		if (ac == null)
		{
			return false;
		}
		return ac.GuestHeroesExistForCurrentAdventure();
	}

	public bool DoesSelectedMissionRequireDeck()
	{
		AdventureConfig ac = AdventureConfig.Get();
		if (ac == null)
		{
			return false;
		}
		return ac.DoesSelectedMissionRequireDeck();
	}

	public List<AdventureHeroPowerDbfRecord> GetHeroPowersForClass(TAG_CLASS classId)
	{
		AdventureConfig ac = AdventureConfig.Get();
		if (ac == null)
		{
			return new List<AdventureHeroPowerDbfRecord>();
		}
		return AdventureUtils.GetHeroPowersForAdventureAndClass(ac.GetSelectedAdventure(), classId);
	}

	public List<AdventureHeroPowerDbfRecord> GetHeroPowersForGuestHero(int guestHeroId)
	{
		AdventureConfig ac = AdventureConfig.Get();
		if (ac == null)
		{
			return new List<AdventureHeroPowerDbfRecord>();
		}
		return AdventureUtils.GetHeroPowersForAdventureAndGuestHero(ac.GetSelectedAdventure(), guestHeroId);
	}

	public List<AdventureDeckDbfRecord> GetDecksForClass(TAG_CLASS classId)
	{
		AdventureConfig ac = AdventureConfig.Get();
		if (ac == null)
		{
			return new List<AdventureDeckDbfRecord>();
		}
		return AdventureUtils.GetDecksForAdventureAndClass(ac.GetSelectedAdventure(), classId);
	}

	public List<AdventureLoadoutTreasuresDbfRecord> GetLoadoutTreasuresForClass(TAG_CLASS classId)
	{
		AdventureConfig ac = AdventureConfig.Get();
		if (ac == null)
		{
			return new List<AdventureLoadoutTreasuresDbfRecord>();
		}
		return AdventureUtils.GetLoadoutTreasuresForAdventureAndClass(ac.GetSelectedAdventure(), classId);
	}

	public List<AdventureLoadoutTreasuresDbfRecord> GetLoadoutTreasuresForGuestHero(int guestHeroId)
	{
		AdventureConfig ac = AdventureConfig.Get();
		if (ac == null)
		{
			return new List<AdventureLoadoutTreasuresDbfRecord>();
		}
		return AdventureUtils.GetLoadoutTreasuresForAdventureAndGuestHero(ac.GetSelectedAdventure(), guestHeroId);
	}

	public bool AdventureHeroPowerIsUnlocked(GameSaveKeyId gameSaveServerKey, AdventureHeroPowerDbfRecord heroPowerRecord, out long unlockProgress, out bool hasUnlockCriteria)
	{
		return AdventureUtils.AdventureHeroPowerIsUnlocked(gameSaveServerKey, heroPowerRecord, out unlockProgress, out hasUnlockCriteria);
	}

	public bool AdventureDeckIsUnlocked(GameSaveKeyId gameSaveServerKey, AdventureDeckDbfRecord deckRecord, out long unlockProgress, out bool hasUnlockCriteria)
	{
		return AdventureUtils.AdventureDeckIsUnlocked(gameSaveServerKey, deckRecord, out unlockProgress, out hasUnlockCriteria);
	}

	public bool AdventureTreasureIsUnlocked(GameSaveKeyId gameSaveServerKey, AdventureLoadoutTreasuresDbfRecord treasureLoadoutRecord, out long unlockProgress, out bool hasUnlockCriteria)
	{
		return AdventureUtils.AdventureTreasureIsUnlocked(gameSaveServerKey, treasureLoadoutRecord, out unlockProgress, out hasUnlockCriteria);
	}

	public bool AdventureTreasureIsUpgraded(GameSaveKeyId gameSaveServerKey, AdventureLoadoutTreasuresDbfRecord treasureLoadoutRecord, out long upgradeProgress)
	{
		return AdventureUtils.AdventureTreasureIsUpgraded(gameSaveServerKey, treasureLoadoutRecord, out upgradeProgress);
	}

	public void SetSelectedAdventureMode(AdventureDbId adventureId, AdventureModeDbId modeId)
	{
		AdventureConfig ac = AdventureConfig.Get();
		if (!(ac == null))
		{
			ac.SetSelectedAdventureMode(adventureId, modeId);
		}
	}

	public bool AdventureRewardIsUnlocked(GameSaveKeyId gameSaveServerKey, GameSaveKeySubkeyId unlockGameSaveSubkey, int unlockValue, int unlockAchievement, out long unlockProgress, out bool hasUnlockCriteria)
	{
		return AdventureUtils.AdventureRewardIsUnlocked(gameSaveServerKey, unlockGameSaveSubkey, unlockValue, unlockAchievement, out unlockProgress, out hasUnlockCriteria);
	}

	public AdventureDef GetAdventureDef()
	{
		AdventureScene adventureScene = AdventureScene.Get();
		if (adventureScene == null)
		{
			return null;
		}
		return adventureScene.GetAdventureDef(GetSelectedAdventure());
	}

	public GameSaveKeyId GetGameSaveClientKey()
	{
		AdventureDbId selectedAdventure = GetSelectedAdventure();
		AdventureModeDbId adventureModeDbId = GetSelectedMode();
		return (GameSaveKeyId)(GameUtils.GetAdventureDataRecord((int)selectedAdventure, (int)adventureModeDbId)?.GameSaveDataClientKey ?? (-1));
	}

	public AdventureWingDef GetWingDef(WingDbId wingId)
	{
		AdventureScene adventureScene = AdventureScene.Get();
		if (adventureScene == null)
		{
			return null;
		}
		return adventureScene.GetWingDef(wingId);
	}

	public AdventureDataDbfRecord GetSelectedAdventureDataRecord()
	{
		return AdventureConfig.Get().GetSelectedAdventureDataRecord();
	}

	public bool HasValidLoadoutForSelectedAdventure()
	{
		return AdventureConfig.Get().HasValidLoadoutForSelectedAdventure();
	}
}
