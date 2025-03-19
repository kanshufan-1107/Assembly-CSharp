using System.Collections;
using Hearthstone.DungeonCrawl;
using UnityEngine;

public abstract class ULDA_MissionEntity : GenericDungeonMissionEntity
{
	public sealed override AdventureDbId GetAdventureID()
	{
		return AdventureDbId.ULDUM;
	}

	protected sealed override bool CanPlayVOLines(Entity speakerEntity, VOSpeaker speaker)
	{
		if (speaker == VOSpeaker.FRIENDLY_HERO)
		{
			return speakerEntity.GetCardId().Contains("ULDA_");
		}
		return base.CanPlayVOLines(speakerEntity, speaker);
	}

	protected override IEnumerator HandleGameOverWithTiming(TAG_PLAYSTATE gameResult)
	{
		if (gameResult == TAG_PLAYSTATE.LOST)
		{
			yield return new WaitForSeconds(5f);
		}
	}

	public override bool ShouldShowHeroClassDuringMulligan(Player.Side playerSide)
	{
		return false;
	}

	public override void OnPlayThinkEmote()
	{
		if (m_enemySpeaking)
		{
			return;
		}
		Player currentPlayer = GameState.Get().GetCurrentPlayer();
		if (currentPlayer.IsFriendlySide() && !currentPlayer.GetHeroCard().HasActiveEmoteSound())
		{
			EmoteType thinkEmote = EmoteType.THINK1;
			switch (Random.Range(1, 4))
			{
			case 1:
				thinkEmote = EmoteType.THINK1;
				break;
			case 2:
				thinkEmote = EmoteType.THINK2;
				break;
			case 3:
				thinkEmote = EmoteType.THINK3;
				break;
			}
			GameState.Get().GetCurrentPlayer().GetHeroCard()
				.PlayEmote(thinkEmote);
		}
	}

	public override void StartMulliganSoundtracks(bool soft)
	{
		if (!soft)
		{
			MusicManager.Get().StartPlaylist(MusicPlaylistType.InGame_ULDMulligan);
		}
	}

	public int GetDefeatedBossCountForFinalBoss()
	{
		int missionId = GameMgr.Get().GetMissionId();
		if (missionId == 3432 || missionId == 3437)
		{
			return 0;
		}
		return 7;
	}

	public override void StartGameplaySoundtracks()
	{
		if (GameUtils.GetDefeatedBossCount() == GetDefeatedBossCountForFinalBoss())
		{
			MusicManager.Get().StartPlaylist(MusicPlaylistType.InGame_ULDFinalBoss);
		}
		else
		{
			base.StartGameplaySoundtracks();
		}
	}

	public static bool GetIsFirstBoss()
	{
		int @int = Options.Get().GetInt(Option.SELECTED_ADVENTURE);
		AdventureModeDbId selectedMode = (AdventureModeDbId)Options.Get().GetInt(Option.SELECTED_ADVENTURE_MODE);
		AdventureDataDbfRecord dataRecord = GetAdventureDataRecord(@int, (int)selectedMode);
		if (dataRecord == null)
		{
			return true;
		}
		if (!DungeonCrawlUtil.IsDungeonRunActive((GameSaveKeyId)dataRecord.GameSaveDataServerKey))
		{
			return true;
		}
		return false;
	}

	public static AdventureDataDbfRecord GetAdventureDataRecord(int adventureId, int modeId)
	{
		foreach (AdventureDataDbfRecord rec in GameDbf.AdventureData.GetRecords())
		{
			if (rec.AdventureId == adventureId && rec.ModeId == modeId)
			{
				return rec;
			}
		}
		return null;
	}
}
