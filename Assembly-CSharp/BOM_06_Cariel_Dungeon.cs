using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BOM_06_Cariel_Dungeon : BOM_06_Cariel_MissionEntity
{
	public readonly AssetReference Cariel_BrassRing_Quote = new AssetReference("Cariel_BrassRing_Quote.prefab:0a68b69767569144c8001265992df14f");

	public readonly AssetReference Cornelius_BrassRing_Quote = new AssetReference("Cornelius_BrassRing_Quote.prefab:c9573291191b0484e88657d665d844a8");

	public readonly AssetReference YoungCornelius_BrassRing_Quote = new AssetReference("YoungCornelius_BrassRing_Quote.prefab:99b9f6f88001de544a711cd375d85ea7");

	public readonly AssetReference Tamsin_BrassRing = new AssetReference("Tamsin4_BrassRing_Quote.prefab:8cb215bc36bd2854fa000e5f6453a338");

	public readonly AssetReference Tavish4_BrassRing_Quote = new AssetReference("Tavish4_BrassRing_Quote.prefab:28458b58b7d010d42b0bda2ff89683e9");

	public readonly AssetReference Kurtrus_Stormwind_BrassRing_Quote = new AssetReference("Kurtrus_Stormwind_BrassRing_Quote.prefab:76cde32559de9c643af479d3f38970a8");

	public override void OnCreateGame()
	{
		base.OnCreateGame();
		m_introLine = null;
		m_deathLine = null;
		m_standardEmoteResponseLine = null;
		m_BossIdleLines = new List<string>(GetBossIdleLines());
		m_BossIdleLinesCopy = new List<string>(GetBossIdleLines());
		m_OverrideMusicTrack = MusicPlaylistType.Invalid;
		m_OverrideMulliganMusicTrack = MusicPlaylistType.Invalid;
		m_Mission_EnemyHeroShouldExplodeOnDefeat = true;
		m_Mission_FriendlyHeroShouldExplodeOnDefeat = true;
		m_OverrideBossSubtext = null;
		m_OverridePlayerSubtext = null;
		m_SupressEnemyDeathTextBubble = false;
	}

	public override void PreloadAssets()
	{
		base.PreloadAssets();
		List<string> soundFiles = new List<string>();
		SetBossVOLines(soundFiles);
		foreach (string soundFile in soundFiles)
		{
			PreloadSound(soundFile);
		}
	}

	public sealed override AdventureDbId GetAdventureID()
	{
		return AdventureDbId.BOM;
	}

	protected override IEnumerator RespondToFriendlyPlayedCardWithTiming(Entity entity)
	{
		GameState.Get().GetFriendlySidePlayer().GetHeroCard()
			.GetActor();
		GameState.Get().GetFriendlySidePlayer().GetHero()
			.GetCardId();
		while (entity.GetCardType() == TAG_CARDTYPE.INVALID)
		{
			yield return null;
		}
		yield return base.RespondToFriendlyPlayedCardWithTiming(entity);
		yield return WaitForEntitySoundToFinish(entity);
		entity.GetCardId();
	}

	public override IEnumerator DoActionsBeforeDealingBaseMulliganCards()
	{
		MissionPause(pause: true);
		yield return HandleMissionEventWithTiming(514);
		MissionPause(pause: false);
	}

	protected override IEnumerator HandleMissionEventWithTiming(int missionEvent)
	{
		while (m_enemySpeaking)
		{
			yield return null;
		}
		if (missionEvent == 911)
		{
			GameState.Get().SetBusy(busy: true);
			while (m_enemySpeaking)
			{
				yield return null;
			}
			GameState.Get().SetBusy(busy: false);
			yield break;
		}
		Actor friendlyActor = GameState.Get().GetFriendlySidePlayer().GetHero()
			.GetCard()
			.GetActor();
		Actor enemyActor = GameState.Get().GetOpposingSidePlayer().GetHeroCard()
			.GetActor();
		GameState.Get().GetFriendlySidePlayer().GetHeroPower()
			.GetCard()
			.GetActor();
		GameState.Get().GetOpposingSidePlayer().GetHero()
			.GetCardId();
		Random.Range(0f, 1f);
		GetTag(GAME_TAG.TURN);
		GameState.Get().GetGameEntity().GetTag(GAME_TAG.EXTRA_TURNS_TAKEN_THIS_GAME);
		Random.Range(0f, 1f);
		switch (missionEvent)
		{
		case 1000:
			GameState.Get().GetGameEntity().SetTag(GAME_TAG.MISSION_EVENT, 0);
			if (m_PlayPlayerVOLineIndex + 1 >= m_PlayerVOLines.Count)
			{
				m_PlayPlayerVOLineIndex = 0;
			}
			else
			{
				m_PlayPlayerVOLineIndex++;
			}
			SceneDebugger.Get().AddMessage(m_PlayerVOLines[m_PlayPlayerVOLineIndex]);
			yield return PlayBossLine(friendlyActor, m_PlayerVOLines[m_PlayPlayerVOLineIndex]);
			break;
		case 1001:
			GameState.Get().GetGameEntity().SetTag(GAME_TAG.MISSION_EVENT, 0);
			SceneDebugger.Get().AddMessage(m_PlayerVOLines[m_PlayPlayerVOLineIndex]);
			yield return PlayBossLine(friendlyActor, m_PlayerVOLines[m_PlayPlayerVOLineIndex]);
			break;
		case 1002:
			GameState.Get().GetGameEntity().SetTag(GAME_TAG.MISSION_EVENT, 0);
			if (m_PlayBossVOLineIndex + 1 >= m_BossVOLines.Count)
			{
				m_PlayBossVOLineIndex = 0;
			}
			else
			{
				m_PlayBossVOLineIndex++;
			}
			SceneDebugger.Get().AddMessage(m_BossVOLines[m_PlayBossVOLineIndex]);
			yield return PlayBossLine(enemyActor, m_BossVOLines[m_PlayBossVOLineIndex]);
			break;
		case 1003:
			GameState.Get().GetGameEntity().SetTag(GAME_TAG.MISSION_EVENT, 0);
			SceneDebugger.Get().AddMessage(m_BossVOLines[m_PlayBossVOLineIndex]);
			yield return PlayBossLine(enemyActor, m_BossVOLines[m_PlayBossVOLineIndex]);
			break;
		case 1011:
			GameState.Get().GetGameEntity().SetTag(GAME_TAG.MISSION_EVENT, 0);
			foreach (string currentLine3 in m_BossVOLines)
			{
				SceneDebugger.Get().AddMessage(currentLine3);
				yield return MissionPlayVO(enemyActor, currentLine3);
			}
			foreach (string currentLine4 in m_PlayerVOLines)
			{
				SceneDebugger.Get().AddMessage(currentLine4);
				yield return MissionPlayVO(enemyActor, currentLine4);
			}
			break;
		case 1012:
			GameState.Get().GetGameEntity().SetTag(GAME_TAG.MISSION_EVENT, 0);
			foreach (string currentLine2 in m_BossVOLines)
			{
				SceneDebugger.Get().AddMessage(currentLine2);
				yield return MissionPlayVO(enemyActor, currentLine2);
			}
			break;
		case 1013:
			GameState.Get().GetGameEntity().SetTag(GAME_TAG.MISSION_EVENT, 0);
			foreach (string currentLine in m_PlayerVOLines)
			{
				SceneDebugger.Get().AddMessage(currentLine);
				yield return MissionPlayVO(enemyActor, currentLine);
			}
			break;
		case 1010:
			if (m_forceAlwaysPlayLine)
			{
				m_forceAlwaysPlayLine = false;
			}
			else
			{
				m_forceAlwaysPlayLine = true;
			}
			break;
		case 58023:
		{
			SceneMgr.Mode nextMode = GameMgr.Get().GetPostGameSceneMode();
			GameMgr.Get().PreparePostGameSceneMode(nextMode);
			SceneMgr.Get().SetNextMode(nextMode);
			break;
		}
		case 600:
			m_Mission_EnemyHeroShouldExplodeOnDefeat = false;
			break;
		case 610:
			m_Mission_EnemyHeroShouldExplodeOnDefeat = true;
			break;
		case 601:
			m_Mission_FriendlyHeroShouldExplodeOnDefeat = false;
			break;
		case 611:
			m_Mission_FriendlyHeroShouldExplodeOnDefeat = true;
			break;
		case 603:
			m_MissionDisableAutomaticVO = false;
			break;
		case 602:
			m_MissionDisableAutomaticVO = true;
			break;
		case 612:
			m_DoEmoteDrivenStart = true;
			break;
		case 516:
			if (m_SupressEnemyDeathTextBubble)
			{
				yield return MissionPlaySound(enemyActor, m_deathLine);
			}
			else
			{
				yield return MissionPlayVO(enemyActor, m_deathLine);
			}
			break;
		default:
			yield return base.HandleMissionEventWithTiming(missionEvent);
			break;
		}
	}
}
