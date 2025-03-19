using System.Collections;
using System.Collections.Generic;
using Blizzard.T5.Core;

public class BoH_Guldan_05 : BoH_Guldan_Dungeon
{
	private static Map<GameEntityOption, bool> s_booleanOptions = InitBooleanOptions();

	private static readonly AssetReference VO_Story_Hero_Guldan_Male_Orc_Story_Guldan_Mission5ExchangeA_02 = new AssetReference("VO_Story_Hero_Guldan_Male_Orc_Story_Guldan_Mission5ExchangeA_02.prefab:dd2e00d0521b280468331c77d2981425");

	private static readonly AssetReference VO_Story_Hero_Guldan_Male_Orc_Story_Guldan_Mission5ExchangeB_02 = new AssetReference("VO_Story_Hero_Guldan_Male_Orc_Story_Guldan_Mission5ExchangeB_02.prefab:7d9185b945e46d841827a4769eb0864d");

	private static readonly AssetReference VO_Story_Hero_Guldan_Male_Orc_Story_Guldan_Mission5ExchangeC_02 = new AssetReference("VO_Story_Hero_Guldan_Male_Orc_Story_Guldan_Mission5ExchangeC_02.prefab:84abf07fcbbd7d64d8ed34ec1a48ff64");

	private static readonly AssetReference VO_Story_Hero_Guldan_Male_Orc_Story_Guldan_Mission5ExchangeD_02 = new AssetReference("VO_Story_Hero_Guldan_Male_Orc_Story_Guldan_Mission5ExchangeD_02.prefab:94810835864559240bc1cc590df57fe0");

	private static readonly AssetReference VO_Story_Hero_Guldan_Male_Orc_Story_Guldan_Mission5Intro_02 = new AssetReference("VO_Story_Hero_Guldan_Male_Orc_Story_Guldan_Mission5Intro_02.prefab:2c9b7b92df7b0894b9e2637a7d6e0b16");

	private static readonly AssetReference VO_Story_Hero_Guldan_Male_Orc_Story_Guldan_Mission5Victory_02 = new AssetReference("VO_Story_Hero_Guldan_Male_Orc_Story_Guldan_Mission5Victory_02.prefab:d0ebd4a38a10b5b4ab18a9c298476e56");

	private static readonly AssetReference VO_Story_Hero_Medivh_Male_Human_Story_Guldan_Mission5EmoteResponse_01 = new AssetReference("VO_Story_Hero_Medivh_Male_Human_Story_Guldan_Mission5EmoteResponse_01.prefab:1c8c46966b42fad45a606eae41e9f80c");

	private static readonly AssetReference VO_Story_Hero_Medivh_Male_Human_Story_Guldan_Mission5ExchangeA_01 = new AssetReference("VO_Story_Hero_Medivh_Male_Human_Story_Guldan_Mission5ExchangeA_01.prefab:e752331cc91fc654bab5439dc5844e1b");

	private static readonly AssetReference VO_Story_Hero_Medivh_Male_Human_Story_Guldan_Mission5ExchangeA_03 = new AssetReference("VO_Story_Hero_Medivh_Male_Human_Story_Guldan_Mission5ExchangeA_03.prefab:5cc229216b433de4b943e3b217d063a8");

	private static readonly AssetReference VO_Story_Hero_Medivh_Male_Human_Story_Guldan_Mission5ExchangeB_01 = new AssetReference("VO_Story_Hero_Medivh_Male_Human_Story_Guldan_Mission5ExchangeB_01.prefab:3513be1e095067645960fd2308b9a3c6");

	private static readonly AssetReference VO_Story_Hero_Medivh_Male_Human_Story_Guldan_Mission5ExchangeC_01 = new AssetReference("VO_Story_Hero_Medivh_Male_Human_Story_Guldan_Mission5ExchangeC_01.prefab:3f0adb0ce9653c14fa2ba216c02f8452");

	private static readonly AssetReference VO_Story_Hero_Medivh_Male_Human_Story_Guldan_Mission5ExchangeD_01 = new AssetReference("VO_Story_Hero_Medivh_Male_Human_Story_Guldan_Mission5ExchangeD_01.prefab:25b6dd2926f017c4591511a24b6b4125");

	private static readonly AssetReference VO_Story_Hero_Medivh_Male_Human_Story_Guldan_Mission5HeroPower_01 = new AssetReference("VO_Story_Hero_Medivh_Male_Human_Story_Guldan_Mission5HeroPower_01.prefab:14deae51b27ec684393dec271b2ba7e9");

	private static readonly AssetReference VO_Story_Hero_Medivh_Male_Human_Story_Guldan_Mission5HeroPower_02 = new AssetReference("VO_Story_Hero_Medivh_Male_Human_Story_Guldan_Mission5HeroPower_02.prefab:da12e237ae31abe41b22d7cf6cbe093d");

	private static readonly AssetReference VO_Story_Hero_Medivh_Male_Human_Story_Guldan_Mission5HeroPower_03 = new AssetReference("VO_Story_Hero_Medivh_Male_Human_Story_Guldan_Mission5HeroPower_03.prefab:1027f5d65c4bb6a458d57319251affd7");

	private static readonly AssetReference VO_Story_Hero_Medivh_Male_Human_Story_Guldan_Mission5Idle_01 = new AssetReference("VO_Story_Hero_Medivh_Male_Human_Story_Guldan_Mission5Idle_01.prefab:00a4edce57051204caede1c13363135d");

	private static readonly AssetReference VO_Story_Hero_Medivh_Male_Human_Story_Guldan_Mission5Idle_02 = new AssetReference("VO_Story_Hero_Medivh_Male_Human_Story_Guldan_Mission5Idle_02.prefab:66ee126138752be43a1ea56e296ebbe0");

	private static readonly AssetReference VO_Story_Hero_Medivh_Male_Human_Story_Guldan_Mission5Idle_03 = new AssetReference("VO_Story_Hero_Medivh_Male_Human_Story_Guldan_Mission5Idle_03.prefab:b6e85386950dcc6419d5782cbaf58d85");

	private static readonly AssetReference VO_Story_Hero_Medivh_Male_Human_Story_Guldan_Mission5Intro_01 = new AssetReference("VO_Story_Hero_Medivh_Male_Human_Story_Guldan_Mission5Intro_01.prefab:81cf08a836b7ada4ba138339f81f9f85");

	private static readonly AssetReference VO_Story_Hero_Medivh_Male_Human_Story_Guldan_Mission5Loss_01 = new AssetReference("VO_Story_Hero_Medivh_Male_Human_Story_Guldan_Mission5Loss_01.prefab:30fa8b9688aee69488c35bbef4dd27e4");

	private static readonly AssetReference VO_Story_Hero_Medivh_Male_Human_Story_Guldan_Mission5Victory_01 = new AssetReference("VO_Story_Hero_Medivh_Male_Human_Story_Guldan_Mission5Victory_01.prefab:5db5ff7b08042cb479aad2c07b189038");

	private static readonly AssetReference VO_Story_Hero_Medivh_Male_Human_Story_Guldan_Mission5Victory_03 = new AssetReference("VO_Story_Hero_Medivh_Male_Human_Story_Guldan_Mission5Victory_03.prefab:95e61a8f9fd405744ae05e89a23ae24a");

	private List<string> m_BossUsesHeroPowerLines = new List<string> { VO_Story_Hero_Medivh_Male_Human_Story_Guldan_Mission5HeroPower_01, VO_Story_Hero_Medivh_Male_Human_Story_Guldan_Mission5HeroPower_02, VO_Story_Hero_Medivh_Male_Human_Story_Guldan_Mission5HeroPower_03 };

	private new List<string> m_BossIdleLines = new List<string> { VO_Story_Hero_Medivh_Male_Human_Story_Guldan_Mission5Idle_01, VO_Story_Hero_Medivh_Male_Human_Story_Guldan_Mission5Idle_02, VO_Story_Hero_Medivh_Male_Human_Story_Guldan_Mission5Idle_03 };

	private List<string> m_EmoteResponseLines = new List<string> { VO_Story_Hero_Medivh_Male_Human_Story_Guldan_Mission5EmoteResponse_01 };

	private HashSet<string> m_playedLines = new HashSet<string>();

	private static Map<GameEntityOption, bool> InitBooleanOptions()
	{
		return new Map<GameEntityOption, bool> { 
		{
			GameEntityOption.DO_OPENING_TAUNTS,
			false
		} };
	}

	public BoH_Guldan_05()
	{
		m_gameOptions.AddBooleanOptions(s_booleanOptions);
	}

	public override void PreloadAssets()
	{
		base.PreloadAssets();
		List<string> m_SoundFiles = new List<string>
		{
			VO_Story_Hero_Guldan_Male_Orc_Story_Guldan_Mission5ExchangeA_02, VO_Story_Hero_Guldan_Male_Orc_Story_Guldan_Mission5ExchangeB_02, VO_Story_Hero_Guldan_Male_Orc_Story_Guldan_Mission5ExchangeC_02, VO_Story_Hero_Guldan_Male_Orc_Story_Guldan_Mission5ExchangeD_02, VO_Story_Hero_Guldan_Male_Orc_Story_Guldan_Mission5Intro_02, VO_Story_Hero_Guldan_Male_Orc_Story_Guldan_Mission5Victory_02, VO_Story_Hero_Medivh_Male_Human_Story_Guldan_Mission5EmoteResponse_01, VO_Story_Hero_Medivh_Male_Human_Story_Guldan_Mission5ExchangeA_01, VO_Story_Hero_Medivh_Male_Human_Story_Guldan_Mission5ExchangeA_03, VO_Story_Hero_Medivh_Male_Human_Story_Guldan_Mission5ExchangeB_01,
			VO_Story_Hero_Medivh_Male_Human_Story_Guldan_Mission5ExchangeC_01, VO_Story_Hero_Medivh_Male_Human_Story_Guldan_Mission5ExchangeD_01, VO_Story_Hero_Medivh_Male_Human_Story_Guldan_Mission5HeroPower_01, VO_Story_Hero_Medivh_Male_Human_Story_Guldan_Mission5HeroPower_02, VO_Story_Hero_Medivh_Male_Human_Story_Guldan_Mission5HeroPower_03, VO_Story_Hero_Medivh_Male_Human_Story_Guldan_Mission5Idle_01, VO_Story_Hero_Medivh_Male_Human_Story_Guldan_Mission5Idle_02, VO_Story_Hero_Medivh_Male_Human_Story_Guldan_Mission5Idle_03, VO_Story_Hero_Medivh_Male_Human_Story_Guldan_Mission5Intro_01, VO_Story_Hero_Medivh_Male_Human_Story_Guldan_Mission5Loss_01,
			VO_Story_Hero_Medivh_Male_Human_Story_Guldan_Mission5Victory_01, VO_Story_Hero_Medivh_Male_Human_Story_Guldan_Mission5Victory_03
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
		yield return MissionPlayVO(enemyActor, VO_Story_Hero_Medivh_Male_Human_Story_Guldan_Mission5Intro_01);
		yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Guldan_Male_Orc_Story_Guldan_Mission5Intro_02);
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
		m_OverrideMusicTrack = MusicPlaylistType.InGame_SCH_FinalLevels;
		base.OnCreateGame();
		m_standardEmoteResponseLine = VO_Story_Hero_Medivh_Male_Human_Story_Guldan_Mission5EmoteResponse_01;
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
		case 507:
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Medivh_Male_Human_Story_Guldan_Mission5Loss_01);
			GameState.Get().SetBusy(busy: false);
			break;
		case 504:
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Medivh_Male_Human_Story_Guldan_Mission5Victory_01);
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Guldan_Male_Orc_Story_Guldan_Mission5Victory_02);
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Medivh_Male_Human_Story_Guldan_Mission5Victory_03);
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
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Medivh_Male_Human_Story_Guldan_Mission5ExchangeA_01);
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Guldan_Male_Orc_Story_Guldan_Mission5ExchangeA_02);
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Medivh_Male_Human_Story_Guldan_Mission5ExchangeA_03);
			break;
		case 3:
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Medivh_Male_Human_Story_Guldan_Mission5ExchangeB_01);
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Guldan_Male_Orc_Story_Guldan_Mission5ExchangeB_02);
			break;
		case 5:
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Medivh_Male_Human_Story_Guldan_Mission5ExchangeC_01);
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Guldan_Male_Orc_Story_Guldan_Mission5ExchangeC_02);
			break;
		case 11:
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Medivh_Male_Human_Story_Guldan_Mission5ExchangeD_01);
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Guldan_Male_Orc_Story_Guldan_Mission5ExchangeD_02);
			break;
		}
	}
}
