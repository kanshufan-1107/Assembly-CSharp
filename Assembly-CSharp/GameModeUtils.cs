using Assets;
using PegasusShared;

public static class GameModeUtils
{
	private const int NUM_CHOICES_IN_ARENA = 3;

	public static bool CanAccessGameModes()
	{
		if (!HasUnlockedMode(Global.UnlockableGameMode.ARENA) && !HasUnlockedMode(Global.UnlockableGameMode.SOLO_ADVENTURES) && !HasUnlockedMode(Global.UnlockableGameMode.DUELS))
		{
			return HasUnlockedMode(Global.UnlockableGameMode.MERCENARIES);
		}
		return true;
	}

	public static bool HasSeenMercenariesButtonActivation()
	{
		GameSaveDataManager.Get().GetSubkeyValue(GameSaveKeyId.FTUE, GameSaveKeySubkeyId.FTUE_HAS_SEEN_MERCENARIES_BUTTON_ACTIVATION, out long hasSeenMercenariesButtonActivation);
		return hasSeenMercenariesButtonActivation > 0;
	}

	public static bool HasPlayedAPracticeMatch()
	{
		int totalAiMatches = 0;
		NetCache.NetCachePlayerRecords cachedPlayerRecords = NetCache.Get()?.GetNetObject<NetCache.NetCachePlayerRecords>();
		if (cachedPlayerRecords?.Records == null)
		{
			return false;
		}
		foreach (NetCache.PlayerRecord record in cachedPlayerRecords.Records)
		{
			if (record.Data == 0 && record.RecordType == GameType.GT_VS_AI)
			{
				totalAiMatches += record.Losses;
				totalAiMatches += record.Wins;
			}
		}
		if (GameUtils.IsBattleGroundsTutorialComplete())
		{
			totalAiMatches--;
		}
		return totalAiMatches > 0;
	}

	public static int GetTotalDefaultHeroesUnlocked()
	{
		int heroesUnlocked = 0;
		for (int i = 0; i < GameUtils.DEFAULT_HERO_CLASSES.Length; i++)
		{
			NetCache.HeroLevel heroLevel = GameUtils.GetHeroLevel(GameUtils.DEFAULT_HERO_CLASSES[i]);
			if (heroLevel != null && heroLevel.CurrentLevel.Level > 0)
			{
				heroesUnlocked++;
			}
		}
		return heroesUnlocked;
	}

	public static bool HasUnlockedAllDefaultHeroes()
	{
		return GetTotalDefaultHeroesUnlocked() == GameUtils.DEFAULT_HERO_CLASSES.Length;
	}

	public static GameSaveKeySubkeyId GetModeUnlockPlayerFlagSubkeyForMode(Global.UnlockableGameMode gameMode)
	{
		return gameMode switch
		{
			Global.UnlockableGameMode.TAVERN_BRAWL => GameSaveKeySubkeyId.PLAYER_FLAGS_HAS_UNLOCKED_TAVERN_BRAWL, 
			Global.UnlockableGameMode.ARENA => GameSaveKeySubkeyId.PLAYER_FLAGS_HAS_UNLOCKED_ARENA, 
			Global.UnlockableGameMode.BATTLEGROUNDS => GameSaveKeySubkeyId.PLAYER_FLAGS_HAS_UNLOCKED_BATTLEGROUNDS, 
			Global.UnlockableGameMode.SOLO_ADVENTURES => GameSaveKeySubkeyId.PLAYER_FLAGS_HAS_UNLOCKED_SOLO_ADVENTURES, 
			Global.UnlockableGameMode.DUELS => GameSaveKeySubkeyId.PLAYER_FLAGS_HAS_UNLOCKED_DUELS, 
			Global.UnlockableGameMode.MERCENARIES => GameSaveKeySubkeyId.PLAYER_FLAGS_HAS_UNLOCKED_MERCENARIES, 
			_ => GameSaveKeySubkeyId.INVALID, 
		};
	}

	public static bool HasUnlockedMode(Global.UnlockableGameMode gameMode)
	{
		GameSaveDataManager gsdManager = GameSaveDataManager.Get();
		if (!gsdManager.IsDataReady(GameSaveKeyId.PLAYER_FLAGS))
		{
			return false;
		}
		if (gameMode == Global.UnlockableGameMode.ARENA && GetTotalDefaultHeroesUnlocked() < 3)
		{
			return false;
		}
		GameSaveKeySubkeyId modeFlag = GetModeUnlockPlayerFlagSubkeyForMode(gameMode);
		gsdManager.GetSubkeyValue(GameSaveKeyId.PLAYER_FLAGS, modeFlag, out long hasUnlockedModeFlag);
		return hasUnlockedModeFlag == 1;
	}
}
