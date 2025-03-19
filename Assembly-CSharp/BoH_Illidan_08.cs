using System.Collections;
using System.Collections.Generic;
using Blizzard.T5.Core;
using UnityEngine;

public class BoH_Illidan_08 : BoH_Illidan_Dungeon
{
	private static Map<GameEntityOption, bool> s_booleanOptions = InitBooleanOptions();

	private static readonly AssetReference VO_Story_Hero_Akama_Male_Draenei_Story_Illidan_Mission2EmoteResponse_01 = new AssetReference("VO_Story_Hero_Akama_Male_Draenei_Story_Illidan_Mission2EmoteResponse_01.prefab:7bc4523472ca1e64e8464d0845aed713");

	private static readonly AssetReference VO_Story_Hero_Akama_Male_Draenei_Story_Illidan_Mission2HeroPower_01 = new AssetReference("VO_Story_Hero_Akama_Male_Draenei_Story_Illidan_Mission2HeroPower_01.prefab:7de16afc4ec3c5347bb5ee1d6db03351");

	private static readonly AssetReference VO_Story_Hero_Akama_Male_Draenei_Story_Illidan_Mission2HeroPower_02 = new AssetReference("VO_Story_Hero_Akama_Male_Draenei_Story_Illidan_Mission2HeroPower_02.prefab:254ae7d35f552bf4ab0b90f92adf1a68");

	private static readonly AssetReference VO_Story_Hero_Akama_Male_Draenei_Story_Illidan_Mission2HeroPower_03 = new AssetReference("VO_Story_Hero_Akama_Male_Draenei_Story_Illidan_Mission2HeroPower_03.prefab:b94f4320e6244cf4cbd0d774e474cf9c");

	private static readonly AssetReference VO_Story_Hero_Akama_Male_Draenei_Story_Illidan_Mission2Idle_01 = new AssetReference("VO_Story_Hero_Akama_Male_Draenei_Story_Illidan_Mission2Idle_01.prefab:095bbdea30a1de744830dff25bf531af");

	private static readonly AssetReference VO_Story_Hero_Akama_Male_Draenei_Story_Illidan_Mission2Idle_02 = new AssetReference("VO_Story_Hero_Akama_Male_Draenei_Story_Illidan_Mission2Idle_02.prefab:36367bfbbacf1d647b45b27f94ec2ba5");

	private static readonly AssetReference VO_Story_Hero_Akama_Male_Draenei_Story_Illidan_Mission2Idle_03 = new AssetReference("VO_Story_Hero_Akama_Male_Draenei_Story_Illidan_Mission2Idle_03.prefab:9b2c8dc6713493c4aa7838c2d871d9c6");

	private static readonly AssetReference VO_Story_Hero_Akama_Male_Draenei_Story_Illidan_Mission2Loss_01 = new AssetReference("VO_Story_Hero_Akama_Male_Draenei_Story_Illidan_Mission2Loss_01.prefab:b0b2c58f9e925e64581813043db7fe17");

	private static readonly AssetReference VO_Story_Hero_Akama_Male_Draenei_Story_Illidan_Mission8ExchangeB_01 = new AssetReference("VO_Story_Hero_Akama_Male_Draenei_Story_Illidan_Mission8ExchangeB_01.prefab:701d92a50ed40f349b5c94670ec82e11");

	private static readonly AssetReference VO_Story_Hero_Akama_Male_Draenei_Story_Illidan_Mission8ExchangeC_01 = new AssetReference("VO_Story_Hero_Akama_Male_Draenei_Story_Illidan_Mission8ExchangeC_01.prefab:7071a70408134a340a828c5a32c49f3e");

	private static readonly AssetReference VO_Story_Hero_Akama_Male_Draenei_Story_Illidan_Mission8Intro_03 = new AssetReference("VO_Story_Hero_Akama_Male_Draenei_Story_Illidan_Mission8Intro_03.prefab:f977b9b6dc4e3c44c9fce9b222028948");

	private static readonly AssetReference VO_Story_Hero_Illidan_Male_NightElf_Story_Illidan_Mission8ExchangeA_01 = new AssetReference("VO_Story_Hero_Illidan_Male_NightElf_Story_Illidan_Mission8ExchangeA_01.prefab:22819671f42d56347aa95cf5591a02f8");

	private static readonly AssetReference VO_Story_Hero_Illidan_Male_NightElf_Story_Illidan_Mission8ExchangeB_02 = new AssetReference("VO_Story_Hero_Illidan_Male_NightElf_Story_Illidan_Mission8ExchangeB_02.prefab:3886a4c59b73303499ff064003743682");

	private static readonly AssetReference VO_Story_Hero_Illidan_Male_NightElf_Story_Illidan_Mission8ExchangeC_02 = new AssetReference("VO_Story_Hero_Illidan_Male_NightElf_Story_Illidan_Mission8ExchangeC_02.prefab:faab5041dd393a847bea197fbced18a5");

	private static readonly AssetReference VO_Story_Hero_Illidan_Male_NightElf_Story_Illidan_Mission8ExchangeD_01 = new AssetReference("VO_Story_Hero_Illidan_Male_NightElf_Story_Illidan_Mission8ExchangeD_01.prefab:e49c717c1ee701a48811cad5ebfb6f87");

	private static readonly AssetReference VO_Story_Hero_Illidan_Male_NightElf_Story_Illidan_Mission8ExchangeE_01 = new AssetReference("VO_Story_Hero_Illidan_Male_NightElf_Story_Illidan_Mission8ExchangeE_01.prefab:66dc8eb9fa9770143b05b59432d25322");

	private static readonly AssetReference VO_Story_Hero_Illidan_Male_NightElf_Story_Illidan_Mission8Intro_01 = new AssetReference("VO_Story_Hero_Illidan_Male_NightElf_Story_Illidan_Mission8Intro_01.prefab:2c4da10556607624da803737b3ee5a0d");

	private static readonly AssetReference VO_Story_Hero_Illidan_Male_NightElf_Story_Illidan_Mission8Intro_02 = new AssetReference("VO_Story_Hero_Illidan_Male_NightElf_Story_Illidan_Mission8Intro_02.prefab:d4108e33c7ca3fd408733db1752b4d25");

	private static readonly AssetReference VO_Story_Hero_Illidan_Male_NightElf_Story_Illidan_Mission8Victory_02 = new AssetReference("VO_Story_Hero_Illidan_Male_NightElf_Story_Illidan_Mission8Victory_02.prefab:dc3c553298d37524182008c7a841ef70");

	private static readonly AssetReference VO_Story_Hero_Maiev_Female_NightElf_Story_Illidan_Mission8ExchangeD_02 = new AssetReference("VO_Story_Hero_Maiev_Female_NightElf_Story_Illidan_Mission8ExchangeD_02.prefab:bac291a078cbbb54887d60c199ec6fac");

	private static readonly AssetReference VO_Story_Hero_Maiev_Female_NightElf_Story_Illidan_Mission8ExchangeE_02 = new AssetReference("VO_Story_Hero_Maiev_Female_NightElf_Story_Illidan_Mission8ExchangeE_02.prefab:39797607791886b448a6c45d38956aa6");

	private static readonly AssetReference VO_Story_Hero_Maiev_Female_NightElf_Story_Illidan_Mission8Victory_01 = new AssetReference("VO_Story_Hero_Maiev_Female_NightElf_Story_Illidan_Mission8Victory_01.prefab:f635909e7d038eb46bf2582c137a81a0");

	private static readonly AssetReference VO_Story_Hero_Maiev_Female_NightElf_Story_Illidan_Mission2EmoteResponse_01 = new AssetReference("VO_Story_Hero_Maiev_Female_NightElf_Story_Illidan_Mission2EmoteResponse_01.prefab:f55ff625d75d6474499b73ddf67a17a7");

	private static readonly AssetReference VO_Story_Hero_Maiev_Female_NightElf_Story_Illidan_Mission2HeroPower_01 = new AssetReference("VO_Story_Hero_Maiev_Female_NightElf_Story_Illidan_Mission2HeroPower_01.prefab:9943dbdf4281b90469b0dc2eacc95f1f");

	private static readonly AssetReference VO_Story_Hero_Maiev_Female_NightElf_Story_Illidan_Mission2HeroPower_02 = new AssetReference("VO_Story_Hero_Maiev_Female_NightElf_Story_Illidan_Mission2HeroPower_02.prefab:a3c32cde871e0794b914783bfdf949d1");

	private static readonly AssetReference VO_Story_Hero_Maiev_Female_NightElf_Story_Illidan_Mission2HeroPower_03 = new AssetReference("VO_Story_Hero_Maiev_Female_NightElf_Story_Illidan_Mission2HeroPower_03.prefab:1e3f4c5c55471524287cca2d9d05bbcf");

	private static readonly AssetReference VO_Story_Hero_Maiev_Female_NightElf_Story_Illidan_Mission2Idle_01 = new AssetReference("VO_Story_Hero_Maiev_Female_NightElf_Story_Illidan_Mission2Idle_01.prefab:cfedaa229fc159d468c3a56f0ce5debf");

	private static readonly AssetReference VO_Story_Hero_Maiev_Female_NightElf_Story_Illidan_Mission2Idle_02 = new AssetReference("VO_Story_Hero_Maiev_Female_NightElf_Story_Illidan_Mission2Idle_02.prefab:871a815e4653ac343a002b05f03b7910");

	private static readonly AssetReference VO_Story_Hero_Maiev_Female_NightElf_Story_Illidan_Mission2Idle_03 = new AssetReference("VO_Story_Hero_Maiev_Female_NightElf_Story_Illidan_Mission2Idle_03.prefab:80d3934865ffd254fab55fe96767c6ee");

	private static readonly AssetReference VO_Story_Hero_Maiev_Female_NightElf_Story_Illidan_Mission2Loss_01 = new AssetReference("VO_Story_Hero_Maiev_Female_NightElf_Story_Illidan_Mission2Loss_01.prefab:73ed18410a7c51d4cbe6ce84e2dfc51c");

	public static readonly AssetReference MaievBrassRing = new AssetReference("Maiev_BrassRing_Quote.prefab:32a15dc6f5ca637499225d598df88188");

	private List<string> m_InGame_BossUsesHeroPowerLines = new List<string> { VO_Story_Hero_Akama_Male_Draenei_Story_Illidan_Mission2HeroPower_01, VO_Story_Hero_Akama_Male_Draenei_Story_Illidan_Mission2HeroPower_02, VO_Story_Hero_Akama_Male_Draenei_Story_Illidan_Mission2HeroPower_03 };

	private new List<string> m_BossIdleLines = new List<string> { VO_Story_Hero_Akama_Male_Draenei_Story_Illidan_Mission2Idle_01, VO_Story_Hero_Akama_Male_Draenei_Story_Illidan_Mission2Idle_02, VO_Story_Hero_Akama_Male_Draenei_Story_Illidan_Mission2Idle_03 };

	private new List<string> m_BossIdleLinesCopy = new List<string> { VO_Story_Hero_Akama_Male_Draenei_Story_Illidan_Mission2Idle_01, VO_Story_Hero_Akama_Male_Draenei_Story_Illidan_Mission2Idle_02, VO_Story_Hero_Akama_Male_Draenei_Story_Illidan_Mission2Idle_03 };

	private List<string> m_BossMaievIdleLines = new List<string> { VO_Story_Hero_Maiev_Female_NightElf_Story_Illidan_Mission2Idle_01, VO_Story_Hero_Maiev_Female_NightElf_Story_Illidan_Mission2Idle_02, VO_Story_Hero_Maiev_Female_NightElf_Story_Illidan_Mission2Idle_03 };

	private List<string> m_BossMaievIdleLinesCopy = new List<string> { VO_Story_Hero_Maiev_Female_NightElf_Story_Illidan_Mission2Idle_01, VO_Story_Hero_Maiev_Female_NightElf_Story_Illidan_Mission2Idle_02, VO_Story_Hero_Maiev_Female_NightElf_Story_Illidan_Mission2Idle_03 };

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

	public BoH_Illidan_08()
	{
		m_gameOptions.AddBooleanOptions(s_booleanOptions);
	}

	public override void PreloadAssets()
	{
		base.PreloadAssets();
		List<string> m_SoundFiles = new List<string>
		{
			VO_Story_Hero_Akama_Male_Draenei_Story_Illidan_Mission2EmoteResponse_01, VO_Story_Hero_Akama_Male_Draenei_Story_Illidan_Mission2HeroPower_01, VO_Story_Hero_Akama_Male_Draenei_Story_Illidan_Mission2HeroPower_02, VO_Story_Hero_Akama_Male_Draenei_Story_Illidan_Mission2HeroPower_03, VO_Story_Hero_Akama_Male_Draenei_Story_Illidan_Mission2Idle_01, VO_Story_Hero_Akama_Male_Draenei_Story_Illidan_Mission2Idle_02, VO_Story_Hero_Akama_Male_Draenei_Story_Illidan_Mission2Idle_03, VO_Story_Hero_Akama_Male_Draenei_Story_Illidan_Mission2Loss_01, VO_Story_Hero_Akama_Male_Draenei_Story_Illidan_Mission8ExchangeB_01, VO_Story_Hero_Akama_Male_Draenei_Story_Illidan_Mission8ExchangeC_01,
			VO_Story_Hero_Akama_Male_Draenei_Story_Illidan_Mission8Intro_03, VO_Story_Hero_Illidan_Male_NightElf_Story_Illidan_Mission8ExchangeA_01, VO_Story_Hero_Illidan_Male_NightElf_Story_Illidan_Mission8ExchangeB_02, VO_Story_Hero_Illidan_Male_NightElf_Story_Illidan_Mission8ExchangeC_02, VO_Story_Hero_Illidan_Male_NightElf_Story_Illidan_Mission8ExchangeD_01, VO_Story_Hero_Illidan_Male_NightElf_Story_Illidan_Mission8ExchangeE_01, VO_Story_Hero_Illidan_Male_NightElf_Story_Illidan_Mission8Intro_01, VO_Story_Hero_Illidan_Male_NightElf_Story_Illidan_Mission8Intro_02, VO_Story_Hero_Illidan_Male_NightElf_Story_Illidan_Mission8Victory_02, VO_Story_Hero_Maiev_Female_NightElf_Story_Illidan_Mission8ExchangeD_02,
			VO_Story_Hero_Maiev_Female_NightElf_Story_Illidan_Mission8ExchangeE_02, VO_Story_Hero_Maiev_Female_NightElf_Story_Illidan_Mission8Victory_01, VO_Story_Hero_Maiev_Female_NightElf_Story_Illidan_Mission2EmoteResponse_01, VO_Story_Hero_Maiev_Female_NightElf_Story_Illidan_Mission2HeroPower_01, VO_Story_Hero_Maiev_Female_NightElf_Story_Illidan_Mission2HeroPower_02, VO_Story_Hero_Maiev_Female_NightElf_Story_Illidan_Mission2HeroPower_03, VO_Story_Hero_Maiev_Female_NightElf_Story_Illidan_Mission2Idle_01, VO_Story_Hero_Maiev_Female_NightElf_Story_Illidan_Mission2Idle_02, VO_Story_Hero_Maiev_Female_NightElf_Story_Illidan_Mission2Idle_03, VO_Story_Hero_Maiev_Female_NightElf_Story_Illidan_Mission2Loss_01
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
		m_OverrideMusicTrack = MusicPlaylistType.InGame_BT_FinalBoss;
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
		Gameplay.Get().GetNameBannerForSide(Player.Side.OPPOSING).SetName("");
		Gameplay.Get().GetNameBannerForSide(Player.Side.OPPOSING).UpdateHeroNameBanner();
		switch (missionEvent)
		{
		case 515:
			if (GameState.Get().GetOpposingSidePlayer().GetHero()
				.GetCardId() == "Story_10_Akama_008hb")
			{
				yield return MissionPlayVO(enemyActor, VO_Story_Hero_Akama_Male_Draenei_Story_Illidan_Mission2EmoteResponse_01);
			}
			else
			{
				yield return MissionPlayVO(enemyActor, VO_Story_Hero_Maiev_Female_NightElf_Story_Illidan_Mission2EmoteResponse_01);
			}
			break;
		case 514:
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Illidan_Male_NightElf_Story_Illidan_Mission8Intro_01);
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Illidan_Male_NightElf_Story_Illidan_Mission8Intro_02);
			break;
		case 104:
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Akama_Male_Draenei_Story_Illidan_Mission2Loss_01);
			GameState.Get().SetBusy(busy: false);
			break;
		case 507:
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Maiev_Female_NightElf_Story_Illidan_Mission2Loss_01);
			GameState.Get().SetBusy(busy: false);
			break;
		case 504:
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Maiev_Female_NightElf_Story_Illidan_Mission8Victory_01);
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Illidan_Male_NightElf_Story_Illidan_Mission8Victory_02);
			GameState.Get().SetBusy(busy: false);
			break;
		case 101:
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Illidan_Male_NightElf_Story_Illidan_Mission8ExchangeD_01);
			yield return MissionPlayVO(MaievBrassRing, VO_Story_Hero_Maiev_Female_NightElf_Story_Illidan_Mission8ExchangeD_02);
			GameState.Get().SetBusy(busy: false);
			break;
		case 102:
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Illidan_Male_NightElf_Story_Illidan_Mission8ExchangeE_01);
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Maiev_Female_NightElf_Story_Illidan_Mission8ExchangeE_02);
			break;
		case 103:
			yield return MissionPlayVOOnce(enemyActor, m_InGame_BossUsesHeroPowerLines);
			break;
		case 105:
			InitVisuals();
			GameState.Get().GetOpposingSidePlayer().UpdateDisplayInfo();
			Gameplay.Get().GetNameBannerForSide(Player.Side.OPPOSING).UpdateHeroNameBanner();
			break;
		default:
			yield return base.HandleMissionEventWithTiming(missionEvent);
			break;
		}
	}

	public override IEnumerator OnPlayThinkEmoteWithTiming()
	{
		if (m_enemySpeaking)
		{
			yield break;
		}
		Player currentPlayer = GameState.Get().GetCurrentPlayer();
		if (!currentPlayer.IsFriendlySide() || currentPlayer.GetHeroCard().HasActiveEmoteSound())
		{
			yield break;
		}
		string opposingHeroCard = GameState.Get().GetOpposingSidePlayer().GetHero()
			.GetCardId();
		float thinkEmoteBossIdleChancePercentage = GetThinkEmoteBossIdleChancePercentage();
		float randomThink = Random.Range(0f, 1f);
		if (thinkEmoteBossIdleChancePercentage > randomThink || (!m_Mission_FriendlyPlayIdleLines && m_Mission_EnemyPlayIdleLines))
		{
			Actor enemyActor = GameState.Get().GetOpposingSidePlayer().GetHeroCard()
				.GetActor();
			if (opposingHeroCard == "Story_10_Akama_008hb")
			{
				string voLine = PopRandomLine(m_BossIdleLinesCopy);
				if (m_BossIdleLinesCopy.Count == 0)
				{
					m_BossIdleLinesCopy = new List<string>(m_BossIdleLines);
				}
				yield return MissionPlayVO(enemyActor, voLine);
			}
			else if (opposingHeroCard == "Story_10_Maiev_003hb")
			{
				string voLine2 = PopRandomLine(m_BossMaievIdleLinesCopy);
				if (m_BossMaievIdleLinesCopy.Count == 0)
				{
					m_BossMaievIdleLinesCopy = new List<string>(m_BossMaievIdleLines);
				}
				yield return MissionPlayVO(enemyActor, voLine2);
			}
		}
		else if (m_Mission_FriendlyPlayIdleLines)
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
				.PlayEmote(thinkEmote)
				.GetActiveAudioSource();
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
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Akama_Male_Draenei_Story_Illidan_Mission8Intro_03);
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Illidan_Male_NightElf_Story_Illidan_Mission8ExchangeA_01);
			break;
		case 5:
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Akama_Male_Draenei_Story_Illidan_Mission8ExchangeB_01);
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Illidan_Male_NightElf_Story_Illidan_Mission8ExchangeB_02);
			break;
		case 7:
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Akama_Male_Draenei_Story_Illidan_Mission8ExchangeC_01);
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Illidan_Male_NightElf_Story_Illidan_Mission8ExchangeC_02);
			break;
		}
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
		component.FsmVariables.GetFsmBool("RunningMan").Value = false;
		component.FsmVariables.GetFsmBool("MineCart").Value = false;
		component.FsmVariables.GetFsmBool("Airship").Value = false;
		component.FsmVariables.GetFsmBool("Destroyer").Value = true;
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
		string counterName = GameStrings.FormatPlurals("BOH_ILLIDAN_08", pluralNumbers);
		m_turnCounter.ChangeDialogText(counterName, cost.ToString(), "", "");
	}
}
