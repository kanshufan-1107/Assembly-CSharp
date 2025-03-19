using System.Collections;
using System.Collections.Generic;
using Blizzard.T5.Core;

public class BoH_Anduin_04 : BoH_Anduin_Dungeon
{
	private static Map<GameEntityOption, bool> s_booleanOptions = InitBooleanOptions();

	private static readonly AssetReference VO_Story_Hero_Anduin_Male_Human_Story_Anduin_Mission4ExchangeA_02 = new AssetReference("VO_Story_Hero_Anduin_Male_Human_Story_Anduin_Mission4ExchangeA_02.prefab:ae5f2899f8b49264f8941693121789d1");

	private static readonly AssetReference VO_Story_Hero_Anduin_Male_Human_Story_Anduin_Mission4ExchangeB_02 = new AssetReference("VO_Story_Hero_Anduin_Male_Human_Story_Anduin_Mission4ExchangeB_02.prefab:57035c975aac3634fbabee05bd2bfad5");

	private static readonly AssetReference VO_Story_Hero_Anduin_Male_Human_Story_Anduin_Mission4ExchangeC_02 = new AssetReference("VO_Story_Hero_Anduin_Male_Human_Story_Anduin_Mission4ExchangeC_02.prefab:f82248615609ea14f838ddd7f7fe4135");

	private static readonly AssetReference VO_Story_Hero_Anduin_Male_Human_Story_Anduin_Mission4Intro_02 = new AssetReference("VO_Story_Hero_Anduin_Male_Human_Story_Anduin_Mission4Intro_02.prefab:0c2129a03c27fe94ca40820ec0f388b0");

	private static readonly AssetReference VO_Story_Hero_Anduin_Male_Human_Story_Anduin_Mission4Victory_02 = new AssetReference("VO_Story_Hero_Anduin_Male_Human_Story_Anduin_Mission4Victory_02.prefab:ad84d064db74aa74386d7242bf7501e0");

	private static readonly AssetReference VO_Story_Hero_Velen_Male_Draenei_Story_Anduin_Mission4EmoteResponse_01 = new AssetReference("VO_Story_Hero_Velen_Male_Draenei_Story_Anduin_Mission4EmoteResponse_01.prefab:d6c339d5b9c66e845b4a8f2eb101a1c9");

	private static readonly AssetReference VO_Story_Hero_Velen_Male_Draenei_Story_Anduin_Mission4ExchangeA_01 = new AssetReference("VO_Story_Hero_Velen_Male_Draenei_Story_Anduin_Mission4ExchangeA_01.prefab:d6920c5f05cf06740892df2d691015f0");

	private static readonly AssetReference VO_Story_Hero_Velen_Male_Draenei_Story_Anduin_Mission4ExchangeB_01 = new AssetReference("VO_Story_Hero_Velen_Male_Draenei_Story_Anduin_Mission4ExchangeB_01.prefab:f9ae865db21f3084ea1eee5183e2dc2f");

	private static readonly AssetReference VO_Story_Hero_Velen_Male_Draenei_Story_Anduin_Mission4ExchangeB_03 = new AssetReference("VO_Story_Hero_Velen_Male_Draenei_Story_Anduin_Mission4ExchangeB_03.prefab:8f2527609dc832149be5b84e4cfae775");

	private static readonly AssetReference VO_Story_Hero_Velen_Male_Draenei_Story_Anduin_Mission4ExchangeC_01 = new AssetReference("VO_Story_Hero_Velen_Male_Draenei_Story_Anduin_Mission4ExchangeC_01.prefab:5f1540d482260404a86189779bec6989");

	private static readonly AssetReference VO_Story_Hero_Velen_Male_Draenei_Story_Anduin_Mission4ExchangeC_03 = new AssetReference("VO_Story_Hero_Velen_Male_Draenei_Story_Anduin_Mission4ExchangeC_03.prefab:10090a19ddb50c54ca7637c9ef440348");

	private static readonly AssetReference VO_Story_Hero_Velen_Male_Draenei_Story_Anduin_Mission4HeroPower_01 = new AssetReference("VO_Story_Hero_Velen_Male_Draenei_Story_Anduin_Mission4HeroPower_01.prefab:e4967dd3bb6552f48a2e33178227e572");

	private static readonly AssetReference VO_Story_Hero_Velen_Male_Draenei_Story_Anduin_Mission4HeroPower_02 = new AssetReference("VO_Story_Hero_Velen_Male_Draenei_Story_Anduin_Mission4HeroPower_02.prefab:2a4ee7df84637f0459370e5298116fdb");

	private static readonly AssetReference VO_Story_Hero_Velen_Male_Draenei_Story_Anduin_Mission4HeroPower_03 = new AssetReference("VO_Story_Hero_Velen_Male_Draenei_Story_Anduin_Mission4HeroPower_03.prefab:1531b88cbf9f6534fa780c210ce6e826");

	private static readonly AssetReference VO_Story_Hero_Velen_Male_Draenei_Story_Anduin_Mission4Idle_01 = new AssetReference("VO_Story_Hero_Velen_Male_Draenei_Story_Anduin_Mission4Idle_01.prefab:48b2044506f6a0a4c94dac5f518aba89");

	private static readonly AssetReference VO_Story_Hero_Velen_Male_Draenei_Story_Anduin_Mission4Idle_02 = new AssetReference("VO_Story_Hero_Velen_Male_Draenei_Story_Anduin_Mission4Idle_02.prefab:fd223d6d9f224ba49938d316971902c5");

	private static readonly AssetReference VO_Story_Hero_Velen_Male_Draenei_Story_Anduin_Mission4Idle_03 = new AssetReference("VO_Story_Hero_Velen_Male_Draenei_Story_Anduin_Mission4Idle_03.prefab:2778000859c493b40b1fa70a2b026e30");

	private static readonly AssetReference VO_Story_Hero_Velen_Male_Draenei_Story_Anduin_Mission4Intro_01 = new AssetReference("VO_Story_Hero_Velen_Male_Draenei_Story_Anduin_Mission4Intro_01.prefab:c123e6db3506ec647859edcbdf018be8");

	private static readonly AssetReference VO_Story_Hero_Velen_Male_Draenei_Story_Anduin_Mission4Loss_01 = new AssetReference("VO_Story_Hero_Velen_Male_Draenei_Story_Anduin_Mission4Loss_01.prefab:1f7b699d54d522c41a9d441330821b56");

	private static readonly AssetReference VO_Story_Hero_Velen_Male_Draenei_Story_Anduin_Mission4Victory_01 = new AssetReference("VO_Story_Hero_Velen_Male_Draenei_Story_Anduin_Mission4Victory_01.prefab:d2b24436384676f499cd5fa5d7a287f6");

	private static readonly AssetReference VO_Story_Hero_Velen_Male_Draenei_Story_Anduin_Mission4Victory_02 = new AssetReference("VO_Story_Hero_Velen_Male_Draenei_Story_Anduin_Mission4Victory_02.prefab:bc33fbd89561db64dac6c2273f1455b5");

	private List<string> m_BossUsesHeroPowerLines = new List<string> { VO_Story_Hero_Velen_Male_Draenei_Story_Anduin_Mission4HeroPower_01, VO_Story_Hero_Velen_Male_Draenei_Story_Anduin_Mission4HeroPower_02, VO_Story_Hero_Velen_Male_Draenei_Story_Anduin_Mission4HeroPower_03 };

	private new List<string> m_BossIdleLines = new List<string> { VO_Story_Hero_Velen_Male_Draenei_Story_Anduin_Mission4Idle_01, VO_Story_Hero_Velen_Male_Draenei_Story_Anduin_Mission4Idle_02, VO_Story_Hero_Velen_Male_Draenei_Story_Anduin_Mission4Idle_03 };

	private HashSet<string> m_playedLines = new HashSet<string>();

	private static Map<GameEntityOption, bool> InitBooleanOptions()
	{
		return new Map<GameEntityOption, bool> { 
		{
			GameEntityOption.DO_OPENING_TAUNTS,
			false
		} };
	}

	public BoH_Anduin_04()
	{
		m_gameOptions.AddBooleanOptions(s_booleanOptions);
	}

	public override void PreloadAssets()
	{
		base.PreloadAssets();
		List<string> soundFiles = new List<string>
		{
			VO_Story_Hero_Anduin_Male_Human_Story_Anduin_Mission4ExchangeA_02, VO_Story_Hero_Anduin_Male_Human_Story_Anduin_Mission4ExchangeB_02, VO_Story_Hero_Anduin_Male_Human_Story_Anduin_Mission4ExchangeC_02, VO_Story_Hero_Anduin_Male_Human_Story_Anduin_Mission4Intro_02, VO_Story_Hero_Anduin_Male_Human_Story_Anduin_Mission4Victory_02, VO_Story_Hero_Velen_Male_Draenei_Story_Anduin_Mission4EmoteResponse_01, VO_Story_Hero_Velen_Male_Draenei_Story_Anduin_Mission4ExchangeA_01, VO_Story_Hero_Velen_Male_Draenei_Story_Anduin_Mission4ExchangeB_01, VO_Story_Hero_Velen_Male_Draenei_Story_Anduin_Mission4ExchangeB_03, VO_Story_Hero_Velen_Male_Draenei_Story_Anduin_Mission4ExchangeC_01,
			VO_Story_Hero_Velen_Male_Draenei_Story_Anduin_Mission4ExchangeC_03, VO_Story_Hero_Velen_Male_Draenei_Story_Anduin_Mission4HeroPower_01, VO_Story_Hero_Velen_Male_Draenei_Story_Anduin_Mission4HeroPower_02, VO_Story_Hero_Velen_Male_Draenei_Story_Anduin_Mission4HeroPower_03, VO_Story_Hero_Velen_Male_Draenei_Story_Anduin_Mission4Idle_01, VO_Story_Hero_Velen_Male_Draenei_Story_Anduin_Mission4Idle_02, VO_Story_Hero_Velen_Male_Draenei_Story_Anduin_Mission4Idle_03, VO_Story_Hero_Velen_Male_Draenei_Story_Anduin_Mission4Intro_01, VO_Story_Hero_Velen_Male_Draenei_Story_Anduin_Mission4Loss_01, VO_Story_Hero_Velen_Male_Draenei_Story_Anduin_Mission4Victory_01,
			VO_Story_Hero_Velen_Male_Draenei_Story_Anduin_Mission4Victory_02
		};
		SetBossVOLines(soundFiles);
		foreach (string soundFile in soundFiles)
		{
			PreloadSound(soundFile);
		}
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
		yield return MissionPlayVO(enemyActor, VO_Story_Hero_Velen_Male_Draenei_Story_Anduin_Mission4Intro_01);
		yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Anduin_Male_Human_Story_Anduin_Mission4Intro_02);
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

	public override void OnCreateGame()
	{
		base.OnCreateGame();
		m_OverrideMulliganMusicTrack = MusicPlaylistType.InGame_BT;
		m_standardEmoteResponseLine = VO_Story_Hero_Velen_Male_Draenei_Story_Anduin_Mission4EmoteResponse_01;
	}

	protected override IEnumerator HandleMissionEventWithTiming(int missionEvent)
	{
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
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Velen_Male_Draenei_Story_Anduin_Mission4Loss_01);
			GameState.Get().SetBusy(busy: false);
			break;
		case 504:
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Velen_Male_Draenei_Story_Anduin_Mission4Victory_01);
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Velen_Male_Draenei_Story_Anduin_Mission4Victory_02);
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Anduin_Male_Human_Story_Anduin_Mission4Victory_02);
			GameState.Get().SetBusy(busy: false);
			break;
		case 515:
			yield return MissionPlayVO(enemyActor, m_standardEmoteResponseLine);
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
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Velen_Male_Draenei_Story_Anduin_Mission4ExchangeA_01);
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Anduin_Male_Human_Story_Anduin_Mission4ExchangeA_02);
			break;
		case 3:
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Velen_Male_Draenei_Story_Anduin_Mission4ExchangeB_01);
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Anduin_Male_Human_Story_Anduin_Mission4ExchangeB_02);
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Velen_Male_Draenei_Story_Anduin_Mission4ExchangeB_03);
			break;
		case 7:
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Velen_Male_Draenei_Story_Anduin_Mission4ExchangeC_01);
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Anduin_Male_Human_Story_Anduin_Mission4ExchangeC_02);
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Velen_Male_Draenei_Story_Anduin_Mission4ExchangeC_03);
			break;
		}
	}
}
