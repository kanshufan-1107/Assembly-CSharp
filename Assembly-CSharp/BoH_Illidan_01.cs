using System.Collections;
using System.Collections.Generic;
using Blizzard.T5.Core;
using UnityEngine;

public class BoH_Illidan_01 : BoH_Illidan_Dungeon
{
	private static Map<GameEntityOption, bool> s_booleanOptions = InitBooleanOptions();

	private static readonly AssetReference VO_Story_Hero_Arthas_Male_Human_Story_Illidan_Mission1EmoteResponse_01 = new AssetReference("VO_Story_Hero_Arthas_Male_Human_Story_Illidan_Mission1EmoteResponse_01.prefab:f9601d01c5e07994e9eb88e3cb7c550c");

	private static readonly AssetReference VO_Story_Hero_Arthas_Male_Human_Story_Illidan_Mission1ExchangeA_02 = new AssetReference("VO_Story_Hero_Arthas_Male_Human_Story_Illidan_Mission1ExchangeA_02.prefab:b5c467bf1f33fe84a8ed67422ca29bda");

	private static readonly AssetReference VO_Story_Hero_Arthas_Male_Human_Story_Illidan_Mission1ExchangeB_02 = new AssetReference("VO_Story_Hero_Arthas_Male_Human_Story_Illidan_Mission1ExchangeB_02.prefab:dafdf5bca0ad13844a4134df7f504d36");

	private static readonly AssetReference VO_Story_Hero_Arthas_Male_Human_Story_Illidan_Mission1ExchangeC_02 = new AssetReference("VO_Story_Hero_Arthas_Male_Human_Story_Illidan_Mission1ExchangeC_02.prefab:5474e6902fbad214bbe664006b220125");

	private static readonly AssetReference VO_Story_Hero_Arthas_Male_Human_Story_Illidan_Mission1ExchangeD_02 = new AssetReference("VO_Story_Hero_Arthas_Male_Human_Story_Illidan_Mission1ExchangeD_02.prefab:1f56f81cc6762c74b890d174aba83a64");

	private static readonly AssetReference VO_Story_Hero_Arthas_Male_Human_Story_Illidan_Mission1HeroPower_01 = new AssetReference("VO_Story_Hero_Arthas_Male_Human_Story_Illidan_Mission1HeroPower_01.prefab:6d8ad256547dd1a4d8d431f9d5896cdb");

	private static readonly AssetReference VO_Story_Hero_Arthas_Male_Human_Story_Illidan_Mission1HeroPower_02 = new AssetReference("VO_Story_Hero_Arthas_Male_Human_Story_Illidan_Mission1HeroPower_02.prefab:23088aa7014eac647872d19772bc2de0");

	private static readonly AssetReference VO_Story_Hero_Arthas_Male_Human_Story_Illidan_Mission1HeroPower_03 = new AssetReference("VO_Story_Hero_Arthas_Male_Human_Story_Illidan_Mission1HeroPower_03.prefab:30bd3cc23f4a4f749852eefd7c7a6a82");

	private static readonly AssetReference VO_Story_Hero_Arthas_Male_Human_Story_Illidan_Mission1Idle_01 = new AssetReference("VO_Story_Hero_Arthas_Male_Human_Story_Illidan_Mission1Idle_01.prefab:a9af498a83691f540adc5433ac60ca3c");

	private static readonly AssetReference VO_Story_Hero_Arthas_Male_Human_Story_Illidan_Mission1Idle_02 = new AssetReference("VO_Story_Hero_Arthas_Male_Human_Story_Illidan_Mission1Idle_02.prefab:748a11ae9a19b4244bae1c22cf614885");

	private static readonly AssetReference VO_Story_Hero_Arthas_Male_Human_Story_Illidan_Mission1Idle_03 = new AssetReference("VO_Story_Hero_Arthas_Male_Human_Story_Illidan_Mission1Idle_03.prefab:05e82716959a2d74196be8e1a6c2b60c");

	private static readonly AssetReference VO_Story_Hero_Arthas_Male_Human_Story_Illidan_Mission1Intro_02 = new AssetReference("VO_Story_Hero_Arthas_Male_Human_Story_Illidan_Mission1Intro_02.prefab:f280f79900ed8de41aa2c3655004c98b");

	private static readonly AssetReference VO_Story_Hero_Arthas_Male_Human_Story_Illidan_Mission1Loss_01 = new AssetReference("VO_Story_Hero_Arthas_Male_Human_Story_Illidan_Mission1Loss_01.prefab:19bdb2fb2e79e6a40ab8efdbeb07b8bb");

	private static readonly AssetReference VO_Story_Hero_Arthas_Male_Human_Story_Illidan_Mission1Victory_02 = new AssetReference("VO_Story_Hero_Arthas_Male_Human_Story_Illidan_Mission1Victory_02.prefab:6f80d2126308930429e38822f14de477");

	private static readonly AssetReference VO_Story_Hero_Illidan_Male_NightElf_Story_Illidan_Mission1ExchangeA_01 = new AssetReference("VO_Story_Hero_Illidan_Male_NightElf_Story_Illidan_Mission1ExchangeA_01.prefab:56279125c6ff401438e61d49113bdebd");

	private static readonly AssetReference VO_Story_Hero_Illidan_Male_NightElf_Story_Illidan_Mission1ExchangeB_01 = new AssetReference("VO_Story_Hero_Illidan_Male_NightElf_Story_Illidan_Mission1ExchangeB_01.prefab:c269c55479b6b5b40aea46903b2f80ca");

	private static readonly AssetReference VO_Story_Hero_Illidan_Male_NightElf_Story_Illidan_Mission1ExchangeC_01 = new AssetReference("VO_Story_Hero_Illidan_Male_NightElf_Story_Illidan_Mission1ExchangeC_01.prefab:87eaf15fe5e96ae4bad4f366e7c27aa2");

	private static readonly AssetReference VO_Story_Hero_Illidan_Male_NightElf_Story_Illidan_Mission1ExchangeD_01 = new AssetReference("VO_Story_Hero_Illidan_Male_NightElf_Story_Illidan_Mission1ExchangeD_01.prefab:ae9ff5a3b8853bf4a99fbec1afb568d4");

	private static readonly AssetReference VO_Story_Hero_Illidan_Male_NightElf_Story_Illidan_Mission1Intro_01 = new AssetReference("VO_Story_Hero_Illidan_Male_NightElf_Story_Illidan_Mission1Intro_01.prefab:29a3491548830934db423c073e18e962");

	private static readonly AssetReference VO_Story_Hero_Illidan_Male_NightElf_Story_Illidan_Mission1Victory_01 = new AssetReference("VO_Story_Hero_Illidan_Male_NightElf_Story_Illidan_Mission1Victory_01.prefab:18afd49352ebe934f89d29574dae20e7");

	private List<string> m_BossUsesHeroPowerLines = new List<string> { VO_Story_Hero_Arthas_Male_Human_Story_Illidan_Mission1HeroPower_01, VO_Story_Hero_Arthas_Male_Human_Story_Illidan_Mission1HeroPower_02, VO_Story_Hero_Arthas_Male_Human_Story_Illidan_Mission1HeroPower_03 };

	private new List<string> m_BossIdleLines = new List<string> { VO_Story_Hero_Arthas_Male_Human_Story_Illidan_Mission1Idle_01, VO_Story_Hero_Arthas_Male_Human_Story_Illidan_Mission1Idle_02, VO_Story_Hero_Arthas_Male_Human_Story_Illidan_Mission1Idle_03 };

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

	public BoH_Illidan_01()
	{
		m_gameOptions.AddBooleanOptions(s_booleanOptions);
	}

	public override void PreloadAssets()
	{
		base.PreloadAssets();
		List<string> m_SoundFiles = new List<string>
		{
			VO_Story_Hero_Arthas_Male_Human_Story_Illidan_Mission1EmoteResponse_01, VO_Story_Hero_Arthas_Male_Human_Story_Illidan_Mission1ExchangeA_02, VO_Story_Hero_Arthas_Male_Human_Story_Illidan_Mission1ExchangeB_02, VO_Story_Hero_Arthas_Male_Human_Story_Illidan_Mission1ExchangeC_02, VO_Story_Hero_Arthas_Male_Human_Story_Illidan_Mission1ExchangeD_02, VO_Story_Hero_Arthas_Male_Human_Story_Illidan_Mission1HeroPower_01, VO_Story_Hero_Arthas_Male_Human_Story_Illidan_Mission1HeroPower_02, VO_Story_Hero_Arthas_Male_Human_Story_Illidan_Mission1HeroPower_03, VO_Story_Hero_Arthas_Male_Human_Story_Illidan_Mission1Idle_01, VO_Story_Hero_Arthas_Male_Human_Story_Illidan_Mission1Idle_02,
			VO_Story_Hero_Arthas_Male_Human_Story_Illidan_Mission1Idle_03, VO_Story_Hero_Arthas_Male_Human_Story_Illidan_Mission1Intro_02, VO_Story_Hero_Arthas_Male_Human_Story_Illidan_Mission1Loss_01, VO_Story_Hero_Arthas_Male_Human_Story_Illidan_Mission1Victory_02, VO_Story_Hero_Illidan_Male_NightElf_Story_Illidan_Mission1ExchangeA_01, VO_Story_Hero_Illidan_Male_NightElf_Story_Illidan_Mission1ExchangeB_01, VO_Story_Hero_Illidan_Male_NightElf_Story_Illidan_Mission1ExchangeC_01, VO_Story_Hero_Illidan_Male_NightElf_Story_Illidan_Mission1ExchangeD_01, VO_Story_Hero_Illidan_Male_NightElf_Story_Illidan_Mission1Intro_01, VO_Story_Hero_Illidan_Male_NightElf_Story_Illidan_Mission1Victory_01
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

	public override List<string> GetBossHeroPowerRandomLines()
	{
		return m_BossUsesHeroPowerLines;
	}

	public override List<string> GetBossIdleLines()
	{
		return m_BossIdleLines;
	}

	public override void OnCreateGame()
	{
		base.OnCreateGame();
		m_OverrideMusicTrack = MusicPlaylistType.InGame_DRG;
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
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Arthas_Male_Human_Story_Illidan_Mission1EmoteResponse_01);
			break;
		case 514:
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Illidan_Male_NightElf_Story_Illidan_Mission1Intro_01);
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Arthas_Male_Human_Story_Illidan_Mission1Intro_02);
			break;
		case 507:
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Arthas_Male_Human_Story_Illidan_Mission1Loss_01);
			GameState.Get().SetBusy(busy: false);
			break;
		case 504:
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Illidan_Male_NightElf_Story_Illidan_Mission1Victory_01);
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Arthas_Male_Human_Story_Illidan_Mission1Victory_02);
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
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Illidan_Male_NightElf_Story_Illidan_Mission1ExchangeA_01);
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Arthas_Male_Human_Story_Illidan_Mission1ExchangeA_02);
			break;
		case 3:
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Illidan_Male_NightElf_Story_Illidan_Mission1ExchangeB_01);
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Arthas_Male_Human_Story_Illidan_Mission1ExchangeB_02);
			break;
		case 7:
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Illidan_Male_NightElf_Story_Illidan_Mission1ExchangeC_01);
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Arthas_Male_Human_Story_Illidan_Mission1ExchangeC_02);
			break;
		case 11:
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Illidan_Male_NightElf_Story_Illidan_Mission1ExchangeD_01);
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Arthas_Male_Human_Story_Illidan_Mission1ExchangeD_02);
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
		PlayMakerFSM fsm = m_turnCounter.GetComponent<PlayMakerFSM>();
		if (fsm.ActiveStateName.Equals("Idle"))
		{
			fsm.SendEvent("Action");
		}
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
		string counterName = GameStrings.FormatPlurals("BOH_ILLIDAN_01", pluralNumbers);
		m_turnCounter.ChangeDialogText(counterName, cost.ToString(), "", "");
	}

	public override void StartGameplaySoundtracks()
	{
		MusicManager.Get().StartPlaylist(MusicPlaylistType.InGame_ICCLichKing);
	}
}
