using System.Collections;
using System.Collections.Generic;
using Blizzard.T5.Core;
using UnityEngine;

public class BoH_Illidan_07 : BoH_Illidan_Dungeon
{
	private static Map<GameEntityOption, bool> s_booleanOptions = InitBooleanOptions();

	private static readonly AssetReference VO_EX1_614_Male_NightElf_HunterPrince_Start2Response_01 = new AssetReference("VO_EX1_614_Male_NightElf_HunterPrince_Start2Response_01.prefab:ccd6345a277977f47867c65a485a87cf");

	private static readonly AssetReference VO_Story_Hero_Arthas_Male_Human_Story_Illidan_Mission7EmoteResponse_01 = new AssetReference("VO_Story_Hero_Arthas_Male_Human_Story_Illidan_Mission7EmoteResponse_01.prefab:cade225bbda4a5a48a5127551daf6b0a");

	private static readonly AssetReference VO_Story_Hero_Arthas_Male_Human_Story_Illidan_Mission7ExchangeA_01 = new AssetReference("VO_Story_Hero_Arthas_Male_Human_Story_Illidan_Mission7ExchangeA_01.prefab:d413829bb23c380448f5ba1653bc4d44");

	private static readonly AssetReference VO_Story_Hero_Arthas_Male_Human_Story_Illidan_Mission7ExchangeC_01 = new AssetReference("VO_Story_Hero_Arthas_Male_Human_Story_Illidan_Mission7ExchangeC_01.prefab:90bc14b05879f764aa26dbdd038ea7cb");

	private static readonly AssetReference VO_Story_Hero_Arthas_Male_Human_Story_Illidan_Mission7HeroPower_01 = new AssetReference("VO_Story_Hero_Arthas_Male_Human_Story_Illidan_Mission7HeroPower_01.prefab:72358aa41973f9d4db1d32ffc04ae267");

	private static readonly AssetReference VO_Story_Hero_Arthas_Male_Human_Story_Illidan_Mission7HeroPower_02 = new AssetReference("VO_Story_Hero_Arthas_Male_Human_Story_Illidan_Mission7HeroPower_02.prefab:1abd0a82ab2e5264b83532ec144b687a");

	private static readonly AssetReference VO_Story_Hero_Arthas_Male_Human_Story_Illidan_Mission7HeroPower_03 = new AssetReference("VO_Story_Hero_Arthas_Male_Human_Story_Illidan_Mission7HeroPower_03.prefab:49011e222271aa84d8a2789427a55476");

	private static readonly AssetReference VO_Story_Hero_Arthas_Male_Human_Story_Illidan_Mission7Idle_01 = new AssetReference("VO_Story_Hero_Arthas_Male_Human_Story_Illidan_Mission7Idle_01.prefab:6e485fd7ad42cfe40b21128c0e4e9755");

	private static readonly AssetReference VO_Story_Hero_Arthas_Male_Human_Story_Illidan_Mission7Idle_02 = new AssetReference("VO_Story_Hero_Arthas_Male_Human_Story_Illidan_Mission7Idle_02.prefab:bd1d627ad3c471e46b172dab0e5484f3");

	private static readonly AssetReference VO_Story_Hero_Arthas_Male_Human_Story_Illidan_Mission7Idle_03 = new AssetReference("VO_Story_Hero_Arthas_Male_Human_Story_Illidan_Mission7Idle_03.prefab:bd9adebf6f7b8f449bf2b671e3defb9d");

	private static readonly AssetReference VO_Story_Hero_Arthas_Male_Human_Story_Illidan_Mission7Intro_02 = new AssetReference("VO_Story_Hero_Arthas_Male_Human_Story_Illidan_Mission7Intro_02.prefab:be8c9266db92ee544b5f648aa5f89b09");

	private static readonly AssetReference VO_Story_Hero_Arthas_Male_Human_Story_Illidan_Mission7Loss_01 = new AssetReference("VO_Story_Hero_Arthas_Male_Human_Story_Illidan_Mission7Loss_01.prefab:41ce798073bbea64b9a4183ac042e511");

	private static readonly AssetReference VO_Story_Hero_Arthas_Male_Human_Story_Illidan_Mission7Victory_01 = new AssetReference("VO_Story_Hero_Arthas_Male_Human_Story_Illidan_Mission7Victory_01.prefab:ecd57d9ab0608ac48b7a598d38f09b6a");

	private static readonly AssetReference VO_Story_Hero_Arthas_Male_Human_Story_IllidanMission7Obelisk_04 = new AssetReference("VO_Story_Hero_Arthas_Male_Human_Story_IllidanMission7Obelisk_04.prefab:e22623cfcc99dfa4a97d40d024d4a154");

	private static readonly AssetReference VO_Story_Hero_Illidan_Male_NightElf_Story_Illidan_Mission7ExchangeA_02 = new AssetReference("VO_Story_Hero_Illidan_Male_NightElf_Story_Illidan_Mission7ExchangeA_02.prefab:2aa56f90d40c400459f6f2462cc64f4d");

	private static readonly AssetReference VO_Story_Hero_Illidan_Male_NightElf_Story_Illidan_Mission7ExchangeB_02 = new AssetReference("VO_Story_Hero_Illidan_Male_NightElf_Story_Illidan_Mission7ExchangeB_02.prefab:834652c2178cfd14991dc6c13efe66ab");

	private static readonly AssetReference VO_Story_Hero_Illidan_Male_NightElf_Story_Illidan_Mission7Intro_01 = new AssetReference("VO_Story_Hero_Illidan_Male_NightElf_Story_Illidan_Mission7Intro_01.prefab:3b629bd15380b784f9d1ce0d0e8bc51f");

	private static readonly AssetReference VO_Story_Hero_Illidan_Male_NightElf_Story_Illidan_Mission7Victory_04 = new AssetReference("VO_Story_Hero_Illidan_Male_NightElf_Story_Illidan_Mission7Victory_04.prefab:7533c54477aea774aad30af9c2dcdf79");

	private static readonly AssetReference VO_Story_Hero_Illidan_Male_NightElf_Story_IllidanMission7Obelisk_01 = new AssetReference("VO_Story_Hero_Illidan_Male_NightElf_Story_IllidanMission7Obelisk_01.prefab:4f98f3aff84def34b943ff36da0dde4f");

	private static readonly AssetReference VO_Story_Hero_Illidan_Male_NightElf_Story_IllidanMission7Obelisk_02 = new AssetReference("VO_Story_Hero_Illidan_Male_NightElf_Story_IllidanMission7Obelisk_02.prefab:62e999f8771143047bcfc34131273632");

	private static readonly AssetReference VO_Story_Hero_Illidan_Male_NightElf_Story_IllidanMission7Obelisk_03 = new AssetReference("VO_Story_Hero_Illidan_Male_NightElf_Story_IllidanMission7Obelisk_03.prefab:38adb6e726286e340a70362df7058ad1");

	private static readonly AssetReference VO_Story_Minion_Kaelthas_Male_BloodElf_Story_Illidan_Mission7ExchangeB_01 = new AssetReference("VO_Story_Minion_Kaelthas_Male_BloodElf_Story_Illidan_Mission7ExchangeB_01.prefab:d730801b498f48d439c83cefc0f3d796");

	private static readonly AssetReference VO_Story_Minion_Kaelthas_Male_BloodElf_Story_Illidan_Mission7ExchangeC_02 = new AssetReference("VO_Story_Minion_Kaelthas_Male_BloodElf_Story_Illidan_Mission7ExchangeC_02.prefab:e2a040d54830f82439056582427890b1");

	private static readonly AssetReference VO_TB_PrinceHunter_ArthasH_Male_Human_HunterPrince_Start2_01 = new AssetReference("VO_TB_PrinceHunter_ArthasH_Male_Human_HunterPrince_Start2_01.prefab:c6f6c665e36826242abf83366398e5ef");

	private static readonly AssetReference VO_TB_PrinceHunter_ArthasH_Male_Human_HunterPrince_Start3_01 = new AssetReference("VO_TB_PrinceHunter_ArthasH_Male_Human_HunterPrince_Start3_01.prefab:e290f2d7e1bebac428d4a1d388aa5fcf");

	public static readonly AssetReference KaelthasBrassRing = new AssetReference("Kaelthas_BrassRing_Quote.prefab:e2c98e804ab04dd49bfbd665c1647eca");

	private new List<string> m_BossIdleLines = new List<string> { VO_Story_Hero_Arthas_Male_Human_Story_Illidan_Mission7Idle_01, VO_Story_Hero_Arthas_Male_Human_Story_Illidan_Mission7Idle_02, VO_Story_Hero_Arthas_Male_Human_Story_Illidan_Mission7Idle_03 };

	private HashSet<string> m_playedLines = new HashSet<string>();

	private Notification m_turnCounter;

	private static Map<GameEntityOption, bool> InitBooleanOptions()
	{
		return new Map<GameEntityOption, bool> { 
		{
			GameEntityOption.DO_OPENING_TAUNTS,
			false
		} };
	}

	public BoH_Illidan_07()
	{
		m_gameOptions.AddBooleanOptions(s_booleanOptions);
	}

	public override void PreloadAssets()
	{
		base.PreloadAssets();
		List<string> m_SoundFiles = new List<string>
		{
			VO_EX1_614_Male_NightElf_HunterPrince_Start2Response_01, VO_Story_Hero_Arthas_Male_Human_Story_Illidan_Mission7EmoteResponse_01, VO_Story_Hero_Arthas_Male_Human_Story_Illidan_Mission7ExchangeA_01, VO_Story_Hero_Arthas_Male_Human_Story_Illidan_Mission7ExchangeC_01, VO_Story_Hero_Arthas_Male_Human_Story_Illidan_Mission7HeroPower_01, VO_Story_Hero_Arthas_Male_Human_Story_Illidan_Mission7HeroPower_02, VO_Story_Hero_Arthas_Male_Human_Story_Illidan_Mission7HeroPower_03, VO_Story_Hero_Arthas_Male_Human_Story_Illidan_Mission7Idle_01, VO_Story_Hero_Arthas_Male_Human_Story_Illidan_Mission7Idle_02, VO_Story_Hero_Arthas_Male_Human_Story_Illidan_Mission7Idle_03,
			VO_Story_Hero_Arthas_Male_Human_Story_Illidan_Mission7Intro_02, VO_Story_Hero_Arthas_Male_Human_Story_Illidan_Mission7Loss_01, VO_Story_Hero_Arthas_Male_Human_Story_Illidan_Mission7Victory_01, VO_Story_Hero_Arthas_Male_Human_Story_IllidanMission7Obelisk_04, VO_Story_Hero_Illidan_Male_NightElf_Story_Illidan_Mission7ExchangeA_02, VO_Story_Hero_Illidan_Male_NightElf_Story_Illidan_Mission7ExchangeB_02, VO_Story_Hero_Illidan_Male_NightElf_Story_Illidan_Mission7Intro_01, VO_Story_Hero_Illidan_Male_NightElf_Story_Illidan_Mission7Victory_04, VO_Story_Hero_Illidan_Male_NightElf_Story_IllidanMission7Obelisk_01, VO_Story_Hero_Illidan_Male_NightElf_Story_IllidanMission7Obelisk_02,
			VO_Story_Hero_Illidan_Male_NightElf_Story_IllidanMission7Obelisk_03, VO_Story_Minion_Kaelthas_Male_BloodElf_Story_Illidan_Mission7ExchangeB_01, VO_Story_Minion_Kaelthas_Male_BloodElf_Story_Illidan_Mission7ExchangeC_02, VO_TB_PrinceHunter_ArthasH_Male_Human_HunterPrince_Start2_01, VO_TB_PrinceHunter_ArthasH_Male_Human_HunterPrince_Start3_01
		};
		SetBossVOLines(m_SoundFiles);
		foreach (string soundFile in m_SoundFiles)
		{
			PreloadSound(soundFile);
		}
	}

	public override bool ShouldPlayHeroBlowUpSpells(TAG_PLAYSTATE playState)
	{
		return playState != TAG_PLAYSTATE.WON;
	}

	public override List<string> GetBossIdleLines()
	{
		return m_BossIdleLines;
	}

	public override void OnCreateGame()
	{
		base.OnCreateGame();
		m_OverrideMusicTrack = MusicPlaylistType.InGame_ICCLichKing;
	}

	protected override IEnumerator HandleMissionEventWithTiming(int missionEvent)
	{
		while (m_enemySpeaking)
		{
			yield return null;
		}
		Actor enemyActor = GameState.Get().GetOpposingSidePlayer().GetHero()
			.GetCard()
			.GetActor();
		Actor friendlyActor = GameState.Get().GetFriendlySidePlayer().GetHero()
			.GetCard()
			.GetActor();
		switch (missionEvent)
		{
		case 515:
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Arthas_Male_Human_Story_Illidan_Mission7EmoteResponse_01);
			break;
		case 514:
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Illidan_Male_NightElf_Story_Illidan_Mission7Intro_01);
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Arthas_Male_Human_Story_Illidan_Mission7Intro_02);
			break;
		case 507:
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Arthas_Male_Human_Story_Illidan_Mission7Loss_01);
			GameState.Get().SetBusy(busy: false);
			break;
		case 504:
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Arthas_Male_Human_Story_Illidan_Mission7Victory_01);
			yield return MissionPlayVO(friendlyActor, VO_EX1_614_Male_NightElf_HunterPrince_Start2Response_01);
			yield return MissionPlayVO(enemyActor, VO_TB_PrinceHunter_ArthasH_Male_Human_HunterPrince_Start3_01);
			GameState.Get().SetBusy(busy: false);
			break;
		case 101:
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Illidan_Male_NightElf_Story_Illidan_Mission7Victory_04);
			yield return MissionPlayVO(enemyActor, VO_TB_PrinceHunter_ArthasH_Male_Human_HunterPrince_Start2_01);
			GameState.Get().SetBusy(busy: false);
			break;
		case 102:
			yield return MissionPlayVOOnce(friendlyActor, VO_Story_Hero_Illidan_Male_NightElf_Story_IllidanMission7Obelisk_02);
			break;
		case 103:
			yield return MissionPlayVOOnce(friendlyActor, VO_Story_Hero_Illidan_Male_NightElf_Story_IllidanMission7Obelisk_03);
			break;
		case 104:
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVOOnce(enemyActor, VO_Story_Hero_Arthas_Male_Human_Story_IllidanMission7Obelisk_04);
			GameState.Get().SetBusy(busy: false);
			break;
		default:
			yield return base.HandleMissionEventWithTiming(missionEvent);
			break;
		}
	}

	protected override IEnumerator RespondToFriendlyPlayedCardWithTiming(Entity entity)
	{
		yield return base.RespondToFriendlyPlayedCardWithTiming(entity);
		while (m_enemySpeaking)
		{
			yield return null;
		}
		while (entity.GetCardType() == TAG_CARDTYPE.INVALID)
		{
			yield return null;
		}
		if (!m_playedLines.Contains(entity.GetCardId()) || entity.GetCardType() == TAG_CARDTYPE.HERO_POWER)
		{
			yield return WaitForEntitySoundToFinish(entity);
			string cardID = entity.GetCardId();
			m_playedLines.Add(cardID);
			GameState.Get().GetOpposingSidePlayer().GetHero()
				.GetCard()
				.GetActor();
			GameState.Get().GetFriendlySidePlayer().GetHero()
				.GetCard()
				.GetActor();
		}
	}

	protected override IEnumerator RespondToPlayedCardWithTiming(Entity entity)
	{
		while (m_enemySpeaking)
		{
			yield return null;
		}
		while (entity.GetCardType() == TAG_CARDTYPE.INVALID)
		{
			yield return null;
		}
		if (!m_playedLines.Contains(entity.GetCardId()) || entity.GetCardType() == TAG_CARDTYPE.HERO_POWER)
		{
			yield return base.RespondToPlayedCardWithTiming(entity);
			yield return WaitForEntitySoundToFinish(entity);
			string cardID = entity.GetCardId();
			m_playedLines.Add(cardID);
			GameState.Get().GetOpposingSidePlayer().GetHero()
				.GetCard()
				.GetActor();
			GameState.Get().GetFriendlySidePlayer().GetHero()
				.GetCard()
				.GetActor();
		}
	}

	protected override IEnumerator HandleStartOfTurnWithTiming(int turn)
	{
		while (m_enemySpeaking)
		{
			yield return null;
		}
		Actor enemyActor = GameState.Get().GetOpposingSidePlayer().GetHero()
			.GetCard()
			.GetActor();
		Actor friendlyActor = GameState.Get().GetFriendlySidePlayer().GetHero()
			.GetCard()
			.GetActor();
		switch (turn)
		{
		case 1:
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Illidan_Male_NightElf_Story_IllidanMission7Obelisk_01);
			break;
		case 3:
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Arthas_Male_Human_Story_Illidan_Mission7ExchangeA_01);
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Illidan_Male_NightElf_Story_Illidan_Mission7ExchangeA_02);
			break;
		case 7:
			yield return MissionPlayVO(KaelthasBrassRing, VO_Story_Minion_Kaelthas_Male_BloodElf_Story_Illidan_Mission7ExchangeB_01);
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Illidan_Male_NightElf_Story_Illidan_Mission7ExchangeB_02);
			break;
		case 9:
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Arthas_Male_Human_Story_Illidan_Mission7HeroPower_01);
			break;
		case 11:
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Arthas_Male_Human_Story_Illidan_Mission7HeroPower_03);
			break;
		case 13:
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Arthas_Male_Human_Story_Illidan_Mission7ExchangeC_01);
			yield return MissionPlayVO(KaelthasBrassRing, VO_Story_Minion_Kaelthas_Male_BloodElf_Story_Illidan_Mission7ExchangeC_02);
			break;
		case 17:
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Arthas_Male_Human_Story_Illidan_Mission7HeroPower_02);
			break;
		}
	}

	public override void NotifyOfMulliganEnded()
	{
		base.NotifyOfMulliganEnded();
		InitVisuals();
	}

	private void InitVisuals()
	{
		int cost = GetCost();
		InitTurnCounter(cost);
	}

	public override void OnTagChanged(TagDelta change)
	{
		base.OnTagChanged(change);
		if (change.tag == 48 && change.newValue != change.oldValue)
		{
			UpdateVisuals(change.newValue);
		}
	}

	private void InitTurnCounter(int cost)
	{
		GameObject turnCounterGo = AssetLoader.Get().InstantiatePrefab("LOE_Turn_Timer.prefab:b05530aa55868554fb8f0c66632b3c22");
		m_turnCounter = turnCounterGo.GetComponent<Notification>();
		PlayMakerFSM component = m_turnCounter.GetComponent<PlayMakerFSM>();
		component.FsmVariables.GetFsmBool("RunningMan").Value = true;
		component.FsmVariables.GetFsmBool("MineCart").Value = false;
		component.FsmVariables.GetFsmBool("Airship").Value = false;
		component.FsmVariables.GetFsmBool("Destroyer").Value = false;
		component.SendEvent("Birth");
		Actor enemyActor = GameState.Get().GetOpposingSidePlayer().GetHeroCard()
			.GetActor();
		m_turnCounter.transform.parent = enemyActor.gameObject.transform;
		m_turnCounter.transform.localPosition = new Vector3(-1.4f, 0.187f, -0.11f);
		m_turnCounter.transform.localScale = Vector3.one * 0.52f;
		UpdateTurnCounterText(cost);
	}

	private void UpdateVisuals(int cost)
	{
		UpdateTurnCounter(cost);
	}

	private void UpdateTurnCounter(int cost)
	{
		m_turnCounter.GetComponent<PlayMakerFSM>().SendEvent("Action");
		if (cost <= 0)
		{
			Object.Destroy(m_turnCounter.gameObject);
		}
		else
		{
			UpdateTurnCounterText(cost);
		}
	}

	private void UpdateTurnCounterText(int cost)
	{
		GameStrings.PluralNumber[] pluralNumbers = new GameStrings.PluralNumber[1]
		{
			new GameStrings.PluralNumber
			{
				m_index = 0,
				m_number = cost
			}
		};
		string counterName = GameStrings.FormatPlurals("BOH_ILLIDAN_07", pluralNumbers);
		m_turnCounter.ChangeDialogText(counterName, cost.ToString(), "", "");
	}

	public override void StartGameplaySoundtracks()
	{
		MusicManager.Get().StartPlaylist(MusicPlaylistType.InGame_ICCLichKing);
	}
}
