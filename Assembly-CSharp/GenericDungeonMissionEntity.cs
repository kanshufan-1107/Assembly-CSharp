using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenericDungeonMissionEntity : MissionEntity
{
	public enum VOSpeaker
	{
		INVALID,
		FRIENDLY_HERO,
		OPPONENT_HERO
	}

	protected class VOPool
	{
		public List<string> m_soundFiles;

		public float m_chanceToPlay = 0.2f;

		public ShouldPlayValue m_shouldPlay = ShouldPlayValue.Once;

		public VOSpeaker m_speaker;

		public string m_quotePrefabPath;

		public GameSaveKeySubkeyId m_oncePerAccountGameSaveSubkey = GameSaveKeySubkeyId.INVALID;

		public long m_timesOncePerAccountVOSeen;

		public VOPool(List<string> soundFiles, float chanceToPlay, ShouldPlayValue shouldPlay, VOSpeaker speaker, string quotePrefabPath = "", GameSaveKeySubkeyId oncePerAccountGameSaveSubkey = GameSaveKeySubkeyId.INVALID)
		{
			m_soundFiles = soundFiles;
			m_chanceToPlay = chanceToPlay;
			m_shouldPlay = shouldPlay;
			m_speaker = speaker;
			m_quotePrefabPath = quotePrefabPath;
			m_oncePerAccountGameSaveSubkey = oncePerAccountGameSaveSubkey;
		}
	}

	protected Dictionary<int, VOPool> m_VOPools = new Dictionary<int, VOPool>();

	private GameSaveKeyId m_gameSaveDataClientKey = GameSaveKeyId.INVALID;

	public virtual AdventureDbId GetAdventureID()
	{
		return AdventureDbId.INVALID;
	}

	public override void PreloadAssets()
	{
		AdventureDataDbfRecord record = GameDbf.AdventureData.GetRecord((AdventureDataDbfRecord r) => r.AdventureId == (int)GetAdventureID());
		if (record != null)
		{
			m_gameSaveDataClientKey = (GameSaveKeyId)record.GameSaveDataClientKey;
		}
		foreach (KeyValuePair<int, VOPool> pool in m_VOPools)
		{
			foreach (string file in pool.Value.m_soundFiles)
			{
				PreloadSound(file);
			}
			if (m_gameSaveDataClientKey != GameSaveKeyId.INVALID && pool.Value.m_oncePerAccountGameSaveSubkey != GameSaveKeySubkeyId.INVALID)
			{
				GameSaveDataManager.Get().GetSubkeyValue(m_gameSaveDataClientKey, pool.Value.m_oncePerAccountGameSaveSubkey, out pool.Value.m_timesOncePerAccountVOSeen);
			}
		}
	}

	protected virtual bool CanPlayVOLines(Entity heroEntity, VOSpeaker speaker)
	{
		return true;
	}

	protected Card ResolveSpeakerCard(VOSpeaker speaker)
	{
		return speaker switch
		{
			VOSpeaker.FRIENDLY_HERO => GameState.Get().GetFriendlySidePlayer()?.GetHeroCard(), 
			VOSpeaker.OPPONENT_HERO => GameState.Get().GetOpposingSidePlayer()?.GetHeroCard(), 
			_ => null, 
		};
	}

	protected override IEnumerator HandleMissionEventWithTiming(int missionEvent)
	{
		if (!m_VOPools.ContainsKey(missionEvent))
		{
			yield break;
		}
		while (m_enemySpeaking)
		{
			yield return null;
		}
		VOPool pool = m_VOPools[missionEvent];
		if (pool == null || (pool.m_oncePerAccountGameSaveSubkey != GameSaveKeySubkeyId.INVALID && pool.m_timesOncePerAccountVOSeen > 0))
		{
			yield break;
		}
		Actor speakerActor = null;
		if (string.IsNullOrEmpty(pool.m_quotePrefabPath))
		{
			Card speakerCard = ResolveSpeakerCard(pool.m_speaker);
			if (speakerCard == null)
			{
				yield break;
			}
			Entity speakerEntity = speakerCard.GetEntity();
			if (speakerEntity == null)
			{
				yield break;
			}
			speakerActor = speakerCard.GetActor();
			if (speakerActor == null || !CanPlayVOLines(speakerEntity, pool.m_speaker))
			{
				yield break;
			}
		}
		List<string> potentialLines = new List<string>(pool.m_soundFiles);
		if (potentialLines == null || potentialLines.Count == 0)
		{
			yield break;
		}
		float chanceToPlay = pool.m_chanceToPlay;
		float chanceRoll = Random.Range(0f, 1f);
		if (chanceToPlay < chanceRoll)
		{
			yield break;
		}
		string randomLine;
		while (true)
		{
			randomLine = potentialLines[Random.Range(0, potentialLines.Count)];
			if (!NotificationManager.Get().HasSoundPlayedThisSession(randomLine))
			{
				break;
			}
			potentialLines.Remove(randomLine);
			if (potentialLines.Count != 0)
			{
				continue;
			}
			if (pool.m_shouldPlay == ShouldPlayValue.Always)
			{
				for (int i = 0; i < pool.m_soundFiles.Count; i++)
				{
					NotificationManager.Get().ForceRemoveSoundFromPlayedList(pool.m_soundFiles[i]);
				}
				randomLine = pool.m_soundFiles[Random.Range(0, pool.m_soundFiles.Count)];
				break;
			}
			yield break;
		}
		if (!string.IsNullOrEmpty(randomLine))
		{
			if (pool.m_oncePerAccountGameSaveSubkey != GameSaveKeySubkeyId.INVALID)
			{
				pool.m_timesOncePerAccountVOSeen++;
				GameSaveDataManager.Get().SaveSubkey(new GameSaveDataManager.SubkeySaveRequest(m_gameSaveDataClientKey, pool.m_oncePerAccountGameSaveSubkey, 1L));
			}
			if (string.IsNullOrEmpty(pool.m_quotePrefabPath))
			{
				yield return PlayCriticalLine(speakerActor, randomLine);
				yield break;
			}
			m_enemySpeaking = true;
			yield return PlayBossLine(pool.m_quotePrefabPath, randomLine);
			m_enemySpeaking = false;
		}
	}

	protected IEnumerator WaitForEntitySoundToFinish(Entity entity)
	{
		List<CardSoundSpell> soundSpells = entity.GetCard().GetPlaySoundSpells(0, loadIfNeeded: false);
		if (soundSpells == null || soundSpells.Count <= 0)
		{
			yield break;
		}
		CardSoundSpell firstSoundSpell = soundSpells[0];
		if (!(firstSoundSpell == null))
		{
			while (firstSoundSpell.GetActiveAudioSource() != null && firstSoundSpell.GetActiveAudioSource().isPlaying)
			{
				yield return null;
			}
		}
	}

	public override string GetNameBannerSubtextOverride(Player.Side playerSide)
	{
		return string.Empty;
	}

	public override bool ShouldShowHeroClassDuringMulligan(Player.Side playerSide)
	{
		return playerSide == Player.Side.FRIENDLY;
	}

	protected static string GetOpposingHeroCardID(List<Network.PowerHistory> powerList, Network.HistCreateGame createGame)
	{
		int opposingHeroEntityID = 0;
		foreach (Network.HistCreateGame.PlayerData netPlayer in createGame.Players)
		{
			if (netPlayer.GameAccountId.IsEmpty())
			{
				opposingHeroEntityID = netPlayer.Player.Tags.Find((Network.Entity.Tag x) => x.Name == 27).Value;
				break;
			}
		}
		for (int i = 0; i < powerList.Count; i++)
		{
			Network.PowerHistory power = powerList[i];
			if (power.Type == Network.PowerType.FULL_ENTITY)
			{
				Network.Entity netEnt = ((Network.HistFullEntity)power).Entity;
				if (netEnt.ID == opposingHeroEntityID)
				{
					return netEnt.CardID;
				}
			}
		}
		return "";
	}

	protected virtual float ChanceToPlayRandomVOLine()
	{
		return 0.5f;
	}

	protected string PopRandomLineWithChance(List<string> lines)
	{
		if (lines.Count == 0 || lines == null)
		{
			return null;
		}
		float num = ChanceToPlayRandomVOLine();
		float chanceRoll = Random.Range(0f, 1f);
		if (num < chanceRoll)
		{
			return null;
		}
		string randomLine = lines[Random.Range(0, lines.Count)];
		lines.Remove(randomLine);
		return randomLine;
	}
}
