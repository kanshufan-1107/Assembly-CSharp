using System.Collections;
using System.Collections.Generic;
using Blizzard.T5.Core;
using UnityEngine;

public class BoH_Guldan_01 : BoH_Guldan_Dungeon
{
	private static Map<GameEntityOption, bool> s_booleanOptions = InitBooleanOptions();

	private static readonly AssetReference VO_Story_Hero_ForgottenWarrior_Male_Orc_Story_Guldan_Mission1EmoteResponse_01 = new AssetReference("VO_Story_Hero_ForgottenWarrior_Male_Orc_Story_Guldan_Mission1EmoteResponse_01.prefab:3d6d08a08f854484cab7fdf670fe96f5");

	private static readonly AssetReference VO_Story_Hero_ForgottenWarrior_Male_Orc_Story_Guldan_Mission1ExchangeA_01 = new AssetReference("VO_Story_Hero_ForgottenWarrior_Male_Orc_Story_Guldan_Mission1ExchangeA_01.prefab:ee2caef077e4569478160403f96d94a0");

	private static readonly AssetReference VO_Story_Hero_ForgottenWarrior_Male_Orc_Story_Guldan_Mission1ExchangeC_01 = new AssetReference("VO_Story_Hero_ForgottenWarrior_Male_Orc_Story_Guldan_Mission1ExchangeC_01.prefab:b71b0496ffc4b9a4bb3e2814123441db");

	private static readonly AssetReference VO_Story_Hero_ForgottenWarrior_Male_Orc_Story_Guldan_Mission1HeroPower_01 = new AssetReference("VO_Story_Hero_ForgottenWarrior_Male_Orc_Story_Guldan_Mission1HeroPower_01.prefab:38c36a27982006d468451a3ed35333c5");

	private static readonly AssetReference VO_Story_Hero_ForgottenWarrior_Male_Orc_Story_Guldan_Mission1HeroPower_02 = new AssetReference("VO_Story_Hero_ForgottenWarrior_Male_Orc_Story_Guldan_Mission1HeroPower_02.prefab:ad838686f01e4664f958b1fbd3c83b2c");

	private static readonly AssetReference VO_Story_Hero_ForgottenWarrior_Male_Orc_Story_Guldan_Mission1HeroPower_03 = new AssetReference("VO_Story_Hero_ForgottenWarrior_Male_Orc_Story_Guldan_Mission1HeroPower_03.prefab:2e9d9a0f13aee794aa1960e1e7a89109");

	private static readonly AssetReference VO_Story_Hero_ForgottenWarrior_Male_Orc_Story_Guldan_Mission1Idle_01 = new AssetReference("VO_Story_Hero_ForgottenWarrior_Male_Orc_Story_Guldan_Mission1Idle_01.prefab:c064fa16da5a1504cb5ea9b4a33c63e3");

	private static readonly AssetReference VO_Story_Hero_ForgottenWarrior_Male_Orc_Story_Guldan_Mission1Idle_02 = new AssetReference("VO_Story_Hero_ForgottenWarrior_Male_Orc_Story_Guldan_Mission1Idle_02.prefab:e7164f22ee5194e4fa79435e1ebc18fc");

	private static readonly AssetReference VO_Story_Hero_ForgottenWarrior_Male_Orc_Story_Guldan_Mission1Idle_03 = new AssetReference("VO_Story_Hero_ForgottenWarrior_Male_Orc_Story_Guldan_Mission1Idle_03.prefab:04fd7dab2532eb14ea73e86415d8f63f");

	private static readonly AssetReference VO_Story_Hero_ForgottenWarrior_Male_Orc_Story_Guldan_Mission1Intro_01 = new AssetReference("VO_Story_Hero_ForgottenWarrior_Male_Orc_Story_Guldan_Mission1Intro_01.prefab:d1a77a6aa34251b4983d679839fa3682");

	private static readonly AssetReference VO_Story_Hero_ForgottenWarrior_Male_Orc_Story_Guldan_Mission1Loss_01 = new AssetReference("VO_Story_Hero_ForgottenWarrior_Male_Orc_Story_Guldan_Mission1Loss_01.prefab:75ac3be967664014fb53214d4ea2af24");

	private static readonly AssetReference VO_Story_Hero_ForgottenWarrior_Male_Orc_Story_Guldan_Mission1Victory_03 = new AssetReference("VO_Story_Hero_ForgottenWarrior_Male_Orc_Story_Guldan_Mission1Victory_03.prefab:ae727ee151014da4493afc2ac1b210d2");

	private static readonly AssetReference VO_Story_Hero_Guldan_Male_Orc_Story_Guldan_Mission1ExchangeB_01 = new AssetReference("VO_Story_Hero_Guldan_Male_Orc_Story_Guldan_Mission1ExchangeB_01.prefab:f4cb514566de85d47a612467cb678d86");

	private static readonly AssetReference VO_Story_Hero_Guldan_Male_Orc_Story_Guldan_Mission1ExchangeC_02 = new AssetReference("VO_Story_Hero_Guldan_Male_Orc_Story_Guldan_Mission1ExchangeC_02.prefab:326ebb565be06084db9ad503d145b3c0");

	private static readonly AssetReference VO_Story_Hero_Guldan_Male_Orc_Story_Guldan_Mission1ExchangeD_01 = new AssetReference("VO_Story_Hero_Guldan_Male_Orc_Story_Guldan_Mission1ExchangeD_01.prefab:62799953c1f2ce14f8093252008a4151");

	private static readonly AssetReference VO_Story_Hero_Guldan_Male_Orc_Story_Guldan_Mission1Intro_02 = new AssetReference("VO_Story_Hero_Guldan_Male_Orc_Story_Guldan_Mission1Intro_02.prefab:0e2d5c928cf563f48b89922b4e582e67");

	private static readonly AssetReference VO_Story_Minion_ForgottenShaman_Male_Orc_Story_Guldan_Mission1ExchangeE_01 = new AssetReference("VO_Story_Minion_ForgottenShaman_Male_Orc_Story_Guldan_Mission1ExchangeE_01.prefab:f1b8d4d6b669c494eb21f6745c3500ae");

	private static readonly AssetReference VO_Story_Minion_ForgottenShaman_Male_Orc_Story_Guldan_Mission1Victory_01 = new AssetReference("VO_Story_Minion_ForgottenShaman_Male_Orc_Story_Guldan_Mission1Victory_01.prefab:e211569e6f4bc8e4fa7c1dad0f50e214");

	private static readonly AssetReference VO_Story_Minion_ForgottenShaman_Male_Orc_Story_Guldan_Mission1Victory_02 = new AssetReference("VO_Story_Minion_ForgottenShaman_Male_Orc_Story_Guldan_Mission1Victory_02.prefab:94782207d8b00e84fac55c37b2920e80");

	private List<string> m_BossUsesHeroPowerLines = new List<string> { VO_Story_Hero_ForgottenWarrior_Male_Orc_Story_Guldan_Mission1HeroPower_01, VO_Story_Hero_ForgottenWarrior_Male_Orc_Story_Guldan_Mission1HeroPower_02, VO_Story_Hero_ForgottenWarrior_Male_Orc_Story_Guldan_Mission1HeroPower_03 };

	private new List<string> m_BossIdleLines = new List<string> { VO_Story_Hero_ForgottenWarrior_Male_Orc_Story_Guldan_Mission1Idle_01, VO_Story_Hero_ForgottenWarrior_Male_Orc_Story_Guldan_Mission1Idle_02, VO_Story_Hero_ForgottenWarrior_Male_Orc_Story_Guldan_Mission1Idle_03 };

	private List<string> m_EmoteResponseLines = new List<string> { VO_Story_Hero_ForgottenWarrior_Male_Orc_Story_Guldan_Mission1EmoteResponse_01 };

	private HashSet<string> m_playedLines = new HashSet<string>();

	private Notification m_turnCounter;

	private MineCartRushArt m_mineCartArt;

	private static Map<GameEntityOption, bool> InitBooleanOptions()
	{
		return new Map<GameEntityOption, bool> { 
		{
			GameEntityOption.DO_OPENING_TAUNTS,
			false
		} };
	}

	public BoH_Guldan_01()
	{
		m_gameOptions.AddBooleanOptions(s_booleanOptions);
	}

	public override void PreloadAssets()
	{
		base.PreloadAssets();
		List<string> m_SoundFiles = new List<string>
		{
			VO_Story_Hero_ForgottenWarrior_Male_Orc_Story_Guldan_Mission1EmoteResponse_01, VO_Story_Hero_ForgottenWarrior_Male_Orc_Story_Guldan_Mission1ExchangeA_01, VO_Story_Hero_ForgottenWarrior_Male_Orc_Story_Guldan_Mission1ExchangeC_01, VO_Story_Hero_ForgottenWarrior_Male_Orc_Story_Guldan_Mission1HeroPower_01, VO_Story_Hero_ForgottenWarrior_Male_Orc_Story_Guldan_Mission1HeroPower_02, VO_Story_Hero_ForgottenWarrior_Male_Orc_Story_Guldan_Mission1HeroPower_03, VO_Story_Hero_ForgottenWarrior_Male_Orc_Story_Guldan_Mission1Idle_01, VO_Story_Hero_ForgottenWarrior_Male_Orc_Story_Guldan_Mission1Idle_02, VO_Story_Hero_ForgottenWarrior_Male_Orc_Story_Guldan_Mission1Idle_03, VO_Story_Hero_ForgottenWarrior_Male_Orc_Story_Guldan_Mission1Intro_01,
			VO_Story_Hero_ForgottenWarrior_Male_Orc_Story_Guldan_Mission1Loss_01, VO_Story_Hero_ForgottenWarrior_Male_Orc_Story_Guldan_Mission1Victory_03, VO_Story_Hero_Guldan_Male_Orc_Story_Guldan_Mission1ExchangeB_01, VO_Story_Hero_Guldan_Male_Orc_Story_Guldan_Mission1ExchangeC_02, VO_Story_Hero_Guldan_Male_Orc_Story_Guldan_Mission1ExchangeD_01, VO_Story_Hero_Guldan_Male_Orc_Story_Guldan_Mission1Intro_02, VO_Story_Minion_ForgottenShaman_Male_Orc_Story_Guldan_Mission1ExchangeE_01, VO_Story_Minion_ForgottenShaman_Male_Orc_Story_Guldan_Mission1Victory_01, VO_Story_Minion_ForgottenShaman_Male_Orc_Story_Guldan_Mission1Victory_02
		};
		SetBossVOLines(m_SoundFiles);
		foreach (string soundFile in m_SoundFiles)
		{
			PreloadSound(soundFile);
		}
	}

	protected override bool GetShouldSuppressDeathTextBubble()
	{
		return true;
	}

	public override bool ShouldPlayHeroBlowUpSpells(TAG_PLAYSTATE playState)
	{
		return playState != TAG_PLAYSTATE.WON;
	}

	public override IEnumerator DoActionsBeforeDealingBaseMulliganCards()
	{
		Actor enemyActor = GameState.Get().GetOpposingSidePlayer().GetHero()
			.GetCard()
			.GetActor();
		Actor friendlyActor = GameState.Get().GetFriendlySidePlayer().GetHero()
			.GetCard()
			.GetActor();
		GameState.Get().SetBusy(busy: true);
		yield return MissionPlayVO(enemyActor, VO_Story_Hero_ForgottenWarrior_Male_Orc_Story_Guldan_Mission1Intro_01);
		yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Guldan_Male_Orc_Story_Guldan_Mission1Intro_02);
		GameState.Get().SetBusy(busy: false);
	}

	public override List<string> GetBossIdleLines()
	{
		return m_BossIdleLines;
	}

	public override List<string> GetBossHeroPowerRandomLines()
	{
		return m_BossUsesHeroPowerLines;
	}

	protected override void PlayEmoteResponse(EmoteType emoteType, CardSoundSpell emoteSpell)
	{
		Actor enemyActor = GameState.Get().GetOpposingSidePlayer().GetHeroCard()
			.GetActor();
		GameState.Get().GetFriendlySidePlayer().GetHeroCard()
			.GetActor();
		GameState.Get().GetFriendlySidePlayer().GetHero()
			.GetCardId();
		if (MissionEntity.STANDARD_EMOTE_RESPONSE_TRIGGERS.Contains(emoteType))
		{
			Gameplay.Get().StartCoroutine(PlaySoundAndBlockSpeech(m_standardEmoteResponseLine, Notification.SpeechBubbleDirection.TopRight, enemyActor));
		}
	}

	public override void OnCreateGame()
	{
		m_OverrideMusicTrack = MusicPlaylistType.InGame_BT;
		base.OnCreateGame();
		m_standardEmoteResponseLine = VO_Story_Hero_ForgottenWarrior_Male_Orc_Story_Guldan_Mission1EmoteResponse_01;
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
		GameState.Get().GetFriendlySidePlayer().GetHero()
			.GetCard()
			.GetActor();
		switch (missionEvent)
		{
		case 507:
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_ForgottenWarrior_Male_Orc_Story_Guldan_Mission1Loss_01);
			GameState.Get().SetBusy(busy: false);
			break;
		case 504:
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVO("Story_09_ForgottenShaman", VO_Story_Minion_ForgottenShaman_Male_Orc_Story_Guldan_Mission1Victory_01);
			yield return MissionPlayVO("Story_09_ForgottenShaman", VO_Story_Minion_ForgottenShaman_Male_Orc_Story_Guldan_Mission1Victory_02);
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_ForgottenWarrior_Male_Orc_Story_Guldan_Mission1Victory_03);
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
			if (cardID == "Story_09_ForgottenShaman")
			{
				yield return PlayLineOnlyOnce(GetFriendlyActorByCardId("Story_09_ForgottenShaman"), VO_Story_Minion_ForgottenShaman_Male_Orc_Story_Guldan_Mission1ExchangeE_01);
			}
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
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_ForgottenWarrior_Male_Orc_Story_Guldan_Mission1ExchangeA_01);
			break;
		case 3:
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Guldan_Male_Orc_Story_Guldan_Mission1ExchangeB_01);
			break;
		case 5:
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_ForgottenWarrior_Male_Orc_Story_Guldan_Mission1ExchangeC_01);
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Guldan_Male_Orc_Story_Guldan_Mission1ExchangeC_02);
			break;
		case 7:
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Guldan_Male_Orc_Story_Guldan_Mission1ExchangeD_01);
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

	private void UpdateMineCartArt()
	{
		Actor enemyActor = GameState.Get().GetOpposingSidePlayer().GetHeroCard()
			.GetActor();
		m_mineCartArt.DoPortraitSwap(enemyActor);
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
		string counterName = GameStrings.FormatPlurals("BOH_GULDAN_01", pluralNumbers);
		m_turnCounter.ChangeDialogText(counterName, cost.ToString(), "", "");
	}
}
