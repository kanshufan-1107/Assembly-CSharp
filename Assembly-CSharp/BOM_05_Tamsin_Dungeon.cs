using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BOM_05_Tamsin_Dungeon : BOM_05_Tamsin_MissionEntity
{
	public readonly AssetReference Brukan_BrassRing = new AssetReference("Brukan_BrassRing_Quote.prefab:16aa2801dfe06db489bd2259944af32b");

	public readonly AssetReference Tamsin_BrassRing = new AssetReference("Tamsin_BrassRing_Quote.prefab:62964357f9958d64f9346685fc1f87f5");

	public readonly AssetReference Dawngrasp_BrassRing = new AssetReference("Dawngrasp_BrassRing_Quote.prefab:45d9ad7c018bcf7429f8ff3d10e2aaf0");

	public readonly AssetReference Cariel_BrassRing_Quote = new AssetReference("Cariel_BrassRing_Quote.prefab:0a68b69767569144c8001265992df14f");

	public readonly AssetReference Xyrella2_BrassRing_Quote = new AssetReference("Xyrella2_BrassRing_Quote.prefab:d239b219d1d4962448ce25db0c6d4d28");

	public readonly AssetReference Hamuul_20_4_BrassRing_Quote = new AssetReference("Hamuul_20_4_BrassRing_Quote.prefab:54c037c90dc48994b8db6374e72f32ab");

	public readonly AssetReference Naralex_BrassRing = new AssetReference("Tavish_BrassRing_Quote.prefab:ad6adae48f4bfba4da53b7138111c1e3");

	private static readonly AssetReference VO_Story_Hero_Xyrella_Female_Draenei_Story_Xyrella_HPHeal_01 = new AssetReference("VO_Story_Hero_Xyrella_Female_Draenei_Story_Xyrella_HPHeal_01.prefab:53f58e50aac9ccc41a764aff34c50340");

	private static readonly AssetReference VO_Story_Hero_Xyrella_Female_Draenei_Story_Xyrella_HPHeal_02 = new AssetReference("VO_Story_Hero_Xyrella_Female_Draenei_Story_Xyrella_HPHeal_02.prefab:e5614706798e2cf43ac1fca0e2581af8");

	private static readonly AssetReference VO_Story_Hero_Xyrella_Female_Draenei_Story_Xyrella_HPHeal_03 = new AssetReference("VO_Story_Hero_Xyrella_Female_Draenei_Story_Xyrella_HPHeal_03.prefab:e1577bcbb62807e45aa6c808714db2e7");

	private static readonly AssetReference VO_Story_Hero_Xyrella_Female_Draenei_Story_Xyrella_HPHeal_04 = new AssetReference("VO_Story_Hero_Xyrella_Female_Draenei_Story_Xyrella_HPHeal_04.prefab:97404acfc3770224fb1d352abadce4fa");

	private static readonly AssetReference VO_Story_Hero_Xyrella_Female_Draenei_Story_Xyrella_HPHeal_05 = new AssetReference("VO_Story_Hero_Xyrella_Female_Draenei_Story_Xyrella_HPHeal_05.prefab:ba5c601009dc72f4da982fa94abd7e7c");

	private static readonly AssetReference VO_Story_Hero_Xyrella_Female_Draenei_Story_Xyrella_HPHeal_06 = new AssetReference("VO_Story_Hero_Xyrella_Female_Draenei_Story_Xyrella_HPHeal_06.prefab:6246f9c7f3340b14f89b396dc3cc05fe");

	private static readonly AssetReference VO_Story_Minion_Scabbs_Male_Gnome_Story_Xyrella_Trigger_01 = new AssetReference("VO_Story_Minion_Scabbs_Male_Gnome_Story_Xyrella_Trigger_01.prefab:ba9ea9cc6632a5e44883797e594f9b66");

	private static readonly AssetReference VO_Story_Minion_Scabbs_Male_Gnome_Story_Xyrella_Trigger_02 = new AssetReference("VO_Story_Minion_Scabbs_Male_Gnome_Story_Xyrella_Trigger_02.prefab:42ba2603fb3d75b499fdcab3041574d8");

	private static readonly AssetReference VO_Story_Minion_Scabbs_Male_Gnome_Story_Xyrella_Trigger_04 = new AssetReference("VO_Story_Minion_Scabbs_Male_Gnome_Story_Xyrella_Trigger_04.prefab:63a368c752c74d343ad61f4ec3b38642");

	private static readonly AssetReference VO_Story_Minion_Tavish_Male_Dwarf_Story_Xyrella_Trigger_02 = new AssetReference("VO_Story_Minion_Tavish_Male_Dwarf_Story_Xyrella_Trigger_02.prefab:60004d0fb7385e547a8224910590ae8e");

	private static readonly AssetReference VO_Story_Minion_Tavish_Male_Dwarf_Story_Xyrella_Trigger_03 = new AssetReference("VO_Story_Minion_Tavish_Male_Dwarf_Story_Xyrella_Trigger_03.prefab:5c82e945fe93bc241bd3e0e8e7a24dea");

	private static readonly AssetReference VO_Story_Minion_Cariel_Female_Human_EndDormant_03 = new AssetReference("VO_Story_Minion_Cariel_Female_Human_EndDormant_03.prefab:fc225626c1d1ae14a8b509bc46ada31c");

	private static readonly AssetReference VO_PVPDR_Hero_Cariel_Female_Human_Greetings_01 = new AssetReference("VO_PVPDR_Hero_Cariel_Female_Human_Greetings_01.prefab:d9f8ecfd0c9012f439f6eba42606e077");

	private static readonly AssetReference VO_PVPDR_Hero_Cariel_Female_Human_Start_01 = new AssetReference("VO_PVPDR_Hero_Cariel_Female_Human_Start_01.prefab:81050842099d21642acc88629c917953");

	private List<string> m_Xyrella_HeroPowerLines = new List<string> { VO_Story_Hero_Xyrella_Female_Draenei_Story_Xyrella_HPHeal_01, VO_Story_Hero_Xyrella_Female_Draenei_Story_Xyrella_HPHeal_02, VO_Story_Hero_Xyrella_Female_Draenei_Story_Xyrella_HPHeal_03, VO_Story_Hero_Xyrella_Female_Draenei_Story_Xyrella_HPHeal_04, VO_Story_Hero_Xyrella_Female_Draenei_Story_Xyrella_HPHeal_05, VO_Story_Hero_Xyrella_Female_Draenei_Story_Xyrella_HPHeal_06 };

	private List<string> m_Scabbs_HeroPowerLines = new List<string> { VO_Story_Minion_Scabbs_Male_Gnome_Story_Xyrella_Trigger_01, VO_Story_Minion_Scabbs_Male_Gnome_Story_Xyrella_Trigger_02, VO_Story_Minion_Scabbs_Male_Gnome_Story_Xyrella_Trigger_04 };

	private List<string> m_Tavish_HeroPowerLines = new List<string> { VO_Story_Minion_Tavish_Male_Dwarf_Story_Xyrella_Trigger_02, VO_Story_Minion_Tavish_Male_Dwarf_Story_Xyrella_Trigger_03 };

	private List<string> m_Cariel_HeroPowerLines = new List<string> { VO_Story_Minion_Cariel_Female_Human_EndDormant_03, VO_PVPDR_Hero_Cariel_Female_Human_Greetings_01, VO_PVPDR_Hero_Cariel_Female_Human_Start_01 };

	public bool CarielIsHeroPower;

	public bool ScabbsIsHeroPower;

	public bool TavishIsHeroPower;

	public bool XyrellaIsHeroPower;

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
		List<string> soundFiles = new List<string>
		{
			VO_Story_Hero_Xyrella_Female_Draenei_Story_Xyrella_HPHeal_01, VO_Story_Hero_Xyrella_Female_Draenei_Story_Xyrella_HPHeal_02, VO_Story_Hero_Xyrella_Female_Draenei_Story_Xyrella_HPHeal_03, VO_Story_Hero_Xyrella_Female_Draenei_Story_Xyrella_HPHeal_04, VO_Story_Hero_Xyrella_Female_Draenei_Story_Xyrella_HPHeal_05, VO_Story_Hero_Xyrella_Female_Draenei_Story_Xyrella_HPHeal_06, VO_Story_Minion_Tavish_Male_Dwarf_Story_Xyrella_Trigger_02, VO_Story_Minion_Tavish_Male_Dwarf_Story_Xyrella_Trigger_03, VO_Story_Minion_Scabbs_Male_Gnome_Story_Xyrella_Trigger_01, VO_Story_Minion_Scabbs_Male_Gnome_Story_Xyrella_Trigger_02,
			VO_Story_Minion_Scabbs_Male_Gnome_Story_Xyrella_Trigger_04, VO_Story_Minion_Cariel_Female_Human_EndDormant_03, VO_PVPDR_Hero_Cariel_Female_Human_Greetings_01, VO_PVPDR_Hero_Cariel_Female_Human_Start_01
		};
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
		Actor friendlyHeroPowerActor = GameState.Get().GetFriendlySidePlayer().GetHeroPower()
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
			foreach (string currentLine in m_BossVOLines)
			{
				SceneDebugger.Get().AddMessage(currentLine);
				yield return MissionPlayVO(enemyActor, currentLine);
			}
			break;
		case 1013:
			GameState.Get().GetGameEntity().SetTag(GAME_TAG.MISSION_EVENT, 0);
			foreach (string currentLine2 in m_PlayerVOLines)
			{
				SceneDebugger.Get().AddMessage(currentLine2);
				yield return MissionPlayVO(enemyActor, currentLine2);
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
		case 58027:
			CarielIsHeroPower = true;
			XyrellaIsHeroPower = false;
			ScabbsIsHeroPower = false;
			TavishIsHeroPower = false;
			break;
		case 58024:
			CarielIsHeroPower = false;
			XyrellaIsHeroPower = true;
			ScabbsIsHeroPower = false;
			TavishIsHeroPower = false;
			break;
		case 58025:
			CarielIsHeroPower = false;
			XyrellaIsHeroPower = false;
			ScabbsIsHeroPower = true;
			TavishIsHeroPower = false;
			break;
		case 58026:
			CarielIsHeroPower = false;
			XyrellaIsHeroPower = false;
			ScabbsIsHeroPower = false;
			TavishIsHeroPower = true;
			break;
		case 508:
			if (CarielIsHeroPower)
			{
				yield return MissionPlaySound(friendlyHeroPowerActor, m_Cariel_HeroPowerLines);
			}
			if (XyrellaIsHeroPower)
			{
				yield return MissionPlaySound(friendlyHeroPowerActor, m_Xyrella_HeroPowerLines);
			}
			if (ScabbsIsHeroPower)
			{
				yield return MissionPlaySound(friendlyHeroPowerActor, m_Scabbs_HeroPowerLines);
			}
			if (TavishIsHeroPower)
			{
				yield return MissionPlaySound(friendlyHeroPowerActor, m_Tavish_HeroPowerLines);
			}
			break;
		default:
			yield return base.HandleMissionEventWithTiming(missionEvent);
			break;
		}
	}
}
