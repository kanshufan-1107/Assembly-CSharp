using System.Collections;
using System.Collections.Generic;
using Blizzard.T5.Core;

public class BoH_Guldan_03 : BoH_Guldan_Dungeon
{
	private static Map<GameEntityOption, bool> s_booleanOptions = InitBooleanOptions();

	private static readonly AssetReference VO_Story_Hero_Guldan_Male_Orc_Story_Guldan_Mission3ExchangeA_02 = new AssetReference("VO_Story_Hero_Guldan_Male_Orc_Story_Guldan_Mission3ExchangeA_02.prefab:638756bbb7f98e84aab6dc3662f3384f");

	private static readonly AssetReference VO_Story_Hero_Guldan_Male_Orc_Story_Guldan_Mission3ExchangeB_02 = new AssetReference("VO_Story_Hero_Guldan_Male_Orc_Story_Guldan_Mission3ExchangeB_02.prefab:1642e871345ffa64a9b67bd6f8b93e61");

	private static readonly AssetReference VO_Story_Hero_Guldan_Male_Orc_Story_Guldan_Mission3ExchangeC_02 = new AssetReference("VO_Story_Hero_Guldan_Male_Orc_Story_Guldan_Mission3ExchangeC_02.prefab:ce07b9d3421817a489dba68d4a661bb0");

	private static readonly AssetReference VO_Story_Hero_Guldan_Male_Orc_Story_Guldan_Mission3ExchangeD_02 = new AssetReference("VO_Story_Hero_Guldan_Male_Orc_Story_Guldan_Mission3ExchangeD_02.prefab:465cd3660458e85469aa68ef256a31c2");

	private static readonly AssetReference VO_Story_Hero_Guldan_Male_Orc_Story_Guldan_Mission3ExchangeD_04 = new AssetReference("VO_Story_Hero_Guldan_Male_Orc_Story_Guldan_Mission3ExchangeD_04.prefab:102028208d752744a96876995f3132e0");

	private static readonly AssetReference VO_Story_Hero_Guldan_Male_Orc_Story_Guldan_Mission3ExchangeE_01 = new AssetReference("VO_Story_Hero_Guldan_Male_Orc_Story_Guldan_Mission3ExchangeE_01.prefab:8c52ce035ceeb044c9a8ca0f3b360a3f");

	private static readonly AssetReference VO_Story_Hero_Guldan_Male_Orc_Story_Guldan_Mission3ExchangeE_03 = new AssetReference("VO_Story_Hero_Guldan_Male_Orc_Story_Guldan_Mission3ExchangeE_03.prefab:06ba5d765c584444081cb43aec314bdd");

	private static readonly AssetReference VO_Story_Hero_Guldan_Male_Orc_Story_Guldan_Mission3Intro_02 = new AssetReference("VO_Story_Hero_Guldan_Male_Orc_Story_Guldan_Mission3Intro_02.prefab:21253e330e7522349a656b6991041585");

	private static readonly AssetReference VO_Story_Hero_Guldan_Male_Orc_Story_Guldan_Mission3ExchangeF_01 = new AssetReference("VO_Story_Hero_Guldan_Male_Orc_Story_Guldan_Mission3ExchangeF_01.prefab:d1df24b1275178d4e96f45dbd2e1c007");

	private static readonly AssetReference VO_Story_Hero_Guldan_Male_Orc_Story_Guldan_Mission3ExchangeF_03 = new AssetReference("VO_Story_Hero_Guldan_Male_Orc_Story_Guldan_Mission3ExchangeF_03.prefab:a94883d053b228e41b1192f8421815d7");

	private static readonly AssetReference VO_Story_Hero_Nerzhul_Male_Orc_Story_Guldan_Mission3EmoteResponse_01 = new AssetReference("VO_Story_Hero_Nerzhul_Male_Orc_Story_Guldan_Mission3EmoteResponse_01.prefab:c9ceb04bace94344e9531dac8f34d90d");

	private static readonly AssetReference VO_Story_Hero_Nerzhul_Male_Orc_Story_Guldan_Mission3ExchangeA_01 = new AssetReference("VO_Story_Hero_Nerzhul_Male_Orc_Story_Guldan_Mission3ExchangeA_01.prefab:4dd1ee5d889603a4d83bb0156d4ab0f0");

	private static readonly AssetReference VO_Story_Hero_Nerzhul_Male_Orc_Story_Guldan_Mission3ExchangeB_01 = new AssetReference("VO_Story_Hero_Nerzhul_Male_Orc_Story_Guldan_Mission3ExchangeB_01.prefab:e550bc97e28c991499f171aa0c768169");

	private static readonly AssetReference VO_Story_Hero_Nerzhul_Male_Orc_Story_Guldan_Mission3ExchangeC_01 = new AssetReference("VO_Story_Hero_Nerzhul_Male_Orc_Story_Guldan_Mission3ExchangeC_01.prefab:6194280a910ca2f4b90e66cd5c8bdec0");

	private static readonly AssetReference VO_Story_Hero_Nerzhul_Male_Orc_Story_Guldan_Mission3ExchangeD_01 = new AssetReference("VO_Story_Hero_Nerzhul_Male_Orc_Story_Guldan_Mission3ExchangeD_01.prefab:206d6c2088ce36949a5e14aeffa4d020");

	private static readonly AssetReference VO_Story_Hero_Nerzhul_Male_Orc_Story_Guldan_Mission3ExchangeD_03 = new AssetReference("VO_Story_Hero_Nerzhul_Male_Orc_Story_Guldan_Mission3ExchangeD_03.prefab:f9a55feac7a584346aef1d4cb33b28e4");

	private static readonly AssetReference VO_Story_Hero_Nerzhul_Male_Orc_Story_Guldan_Mission3ExchangeE_02 = new AssetReference("VO_Story_Hero_Nerzhul_Male_Orc_Story_Guldan_Mission3ExchangeE_02.prefab:223bce9d9e9aa944d968272626d9b3b5");

	private static readonly AssetReference VO_Story_Hero_Nerzhul_Male_Orc_Story_Guldan_Mission3HeroPower_01 = new AssetReference("VO_Story_Hero_Nerzhul_Male_Orc_Story_Guldan_Mission3HeroPower_01.prefab:02be22765aea4a74da6fffaba27917b8");

	private static readonly AssetReference VO_Story_Hero_Nerzhul_Male_Orc_Story_Guldan_Mission3HeroPower_02 = new AssetReference("VO_Story_Hero_Nerzhul_Male_Orc_Story_Guldan_Mission3HeroPower_02.prefab:7336f482a2be22040aea25039614ac89");

	private static readonly AssetReference VO_Story_Hero_Nerzhul_Male_Orc_Story_Guldan_Mission3HeroPower_03 = new AssetReference("VO_Story_Hero_Nerzhul_Male_Orc_Story_Guldan_Mission3HeroPower_03.prefab:e255b148a72d98144a29acd2b1cfd783");

	private static readonly AssetReference VO_Story_Hero_Nerzhul_Male_Orc_Story_Guldan_Mission3Idle_01 = new AssetReference("VO_Story_Hero_Nerzhul_Male_Orc_Story_Guldan_Mission3Idle_01.prefab:7e2d11edc04a5804f8f0f2bbbbc54849");

	private static readonly AssetReference VO_Story_Hero_Nerzhul_Male_Orc_Story_Guldan_Mission3Idle_02 = new AssetReference("VO_Story_Hero_Nerzhul_Male_Orc_Story_Guldan_Mission3Idle_02.prefab:72e499b6689a2bb4db101e05adfdc45f");

	private static readonly AssetReference VO_Story_Hero_Nerzhul_Male_Orc_Story_Guldan_Mission3Idle_03 = new AssetReference("VO_Story_Hero_Nerzhul_Male_Orc_Story_Guldan_Mission3Idle_03.prefab:99f4e13e401b9d84da1aca5c77a62962");

	private static readonly AssetReference VO_Story_Hero_Nerzhul_Male_Orc_Story_Guldan_Mission3Intro_01 = new AssetReference("VO_Story_Hero_Nerzhul_Male_Orc_Story_Guldan_Mission3Intro_01.prefab:3bd488216e4a679468025a14b0d06187");

	private static readonly AssetReference VO_Story_Hero_Nerzhul_Male_Orc_Story_Guldan_Mission3Intro_03 = new AssetReference("VO_Story_Hero_Nerzhul_Male_Orc_Story_Guldan_Mission3Intro_03.prefab:84a25d9fa6a269449b5019b743c6e943");

	private static readonly AssetReference VO_Story_Hero_Nerzhul_Male_Orc_Story_Guldan_Mission3Loss_01 = new AssetReference("VO_Story_Hero_Nerzhul_Male_Orc_Story_Guldan_Mission3Loss_01.prefab:252f64d8f39f2d3408a5b5efba698adb");

	private static readonly AssetReference VO_Story_Hero_Nerzhul_Male_Orc_Story_Guldan_Mission3Victory_02 = new AssetReference("VO_Story_Hero_Nerzhul_Male_Orc_Story_Guldan_Mission3Victory_02.prefab:26bfdbecf7712c84599cb15f9c73d2ce");

	private static readonly AssetReference VO_Story_Hero_Nerzhul_Male_Orc_Story_Guldan_Mission3Victory_04 = new AssetReference("VO_Story_Hero_Nerzhul_Male_Orc_Story_Guldan_Mission3Victory_04.prefab:4212737beec27604bb9361b322b869e4");

	private List<string> m_BossUsesHeroPowerLines = new List<string> { VO_Story_Hero_Nerzhul_Male_Orc_Story_Guldan_Mission3HeroPower_01, VO_Story_Hero_Nerzhul_Male_Orc_Story_Guldan_Mission3HeroPower_02, VO_Story_Hero_Nerzhul_Male_Orc_Story_Guldan_Mission3HeroPower_03 };

	private new List<string> m_BossIdleLines = new List<string> { VO_Story_Hero_Nerzhul_Male_Orc_Story_Guldan_Mission3Idle_01, VO_Story_Hero_Nerzhul_Male_Orc_Story_Guldan_Mission3Idle_02, VO_Story_Hero_Nerzhul_Male_Orc_Story_Guldan_Mission3Idle_03 };

	private List<string> m_EmoteResponseLines = new List<string> { VO_Story_Hero_Nerzhul_Male_Orc_Story_Guldan_Mission3EmoteResponse_01 };

	private HashSet<string> m_playedLines = new HashSet<string>();

	private static Map<GameEntityOption, bool> InitBooleanOptions()
	{
		return new Map<GameEntityOption, bool> { 
		{
			GameEntityOption.DO_OPENING_TAUNTS,
			false
		} };
	}

	public BoH_Guldan_03()
	{
		m_gameOptions.AddBooleanOptions(s_booleanOptions);
	}

	public override void PreloadAssets()
	{
		base.PreloadAssets();
		List<string> m_SoundFiles = new List<string>
		{
			VO_Story_Hero_Guldan_Male_Orc_Story_Guldan_Mission3ExchangeA_02, VO_Story_Hero_Guldan_Male_Orc_Story_Guldan_Mission3ExchangeB_02, VO_Story_Hero_Guldan_Male_Orc_Story_Guldan_Mission3ExchangeC_02, VO_Story_Hero_Guldan_Male_Orc_Story_Guldan_Mission3ExchangeD_02, VO_Story_Hero_Guldan_Male_Orc_Story_Guldan_Mission3ExchangeD_04, VO_Story_Hero_Guldan_Male_Orc_Story_Guldan_Mission3ExchangeE_01, VO_Story_Hero_Guldan_Male_Orc_Story_Guldan_Mission3ExchangeE_03, VO_Story_Hero_Guldan_Male_Orc_Story_Guldan_Mission3Intro_02, VO_Story_Hero_Guldan_Male_Orc_Story_Guldan_Mission3ExchangeF_01, VO_Story_Hero_Guldan_Male_Orc_Story_Guldan_Mission3ExchangeF_03,
			VO_Story_Hero_Nerzhul_Male_Orc_Story_Guldan_Mission3EmoteResponse_01, VO_Story_Hero_Nerzhul_Male_Orc_Story_Guldan_Mission3ExchangeA_01, VO_Story_Hero_Nerzhul_Male_Orc_Story_Guldan_Mission3ExchangeB_01, VO_Story_Hero_Nerzhul_Male_Orc_Story_Guldan_Mission3ExchangeC_01, VO_Story_Hero_Nerzhul_Male_Orc_Story_Guldan_Mission3ExchangeD_01, VO_Story_Hero_Nerzhul_Male_Orc_Story_Guldan_Mission3ExchangeD_03, VO_Story_Hero_Nerzhul_Male_Orc_Story_Guldan_Mission3ExchangeE_02, VO_Story_Hero_Nerzhul_Male_Orc_Story_Guldan_Mission3HeroPower_01, VO_Story_Hero_Nerzhul_Male_Orc_Story_Guldan_Mission3HeroPower_02, VO_Story_Hero_Nerzhul_Male_Orc_Story_Guldan_Mission3HeroPower_03,
			VO_Story_Hero_Nerzhul_Male_Orc_Story_Guldan_Mission3Idle_01, VO_Story_Hero_Nerzhul_Male_Orc_Story_Guldan_Mission3Idle_02, VO_Story_Hero_Nerzhul_Male_Orc_Story_Guldan_Mission3Idle_03, VO_Story_Hero_Nerzhul_Male_Orc_Story_Guldan_Mission3Intro_01, VO_Story_Hero_Nerzhul_Male_Orc_Story_Guldan_Mission3Intro_03, VO_Story_Hero_Nerzhul_Male_Orc_Story_Guldan_Mission3Loss_01, VO_Story_Hero_Nerzhul_Male_Orc_Story_Guldan_Mission3Victory_02, VO_Story_Hero_Nerzhul_Male_Orc_Story_Guldan_Mission3Victory_04
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
		yield return MissionPlayVO(enemyActor, VO_Story_Hero_Nerzhul_Male_Orc_Story_Guldan_Mission3Intro_01);
		yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Guldan_Male_Orc_Story_Guldan_Mission3Intro_02);
		yield return MissionPlayVO(enemyActor, VO_Story_Hero_Nerzhul_Male_Orc_Story_Guldan_Mission3Intro_03);
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
		m_standardEmoteResponseLine = VO_Story_Hero_Nerzhul_Male_Orc_Story_Guldan_Mission3EmoteResponse_01;
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
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Nerzhul_Male_Orc_Story_Guldan_Mission3Loss_01);
			GameState.Get().SetBusy(busy: false);
			break;
		case 504:
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Guldan_Male_Orc_Story_Guldan_Mission3ExchangeF_01);
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Nerzhul_Male_Orc_Story_Guldan_Mission3Victory_02);
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Guldan_Male_Orc_Story_Guldan_Mission3ExchangeF_03);
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Nerzhul_Male_Orc_Story_Guldan_Mission3Victory_04);
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
			Actor enemyActor = GameState.Get().GetOpposingSidePlayer().GetHero()
				.GetCard()
				.GetActor();
			Actor friendlyActor = GameState.Get().GetFriendlySidePlayer().GetHero()
				.GetCard()
				.GetActor();
			if (cardID == "Story_09_TerongorShaman")
			{
				yield return PlayLineOnlyOnce(friendlyActor, VO_Story_Hero_Guldan_Male_Orc_Story_Guldan_Mission3ExchangeE_01);
				yield return PlayLineOnlyOnce(enemyActor, VO_Story_Hero_Nerzhul_Male_Orc_Story_Guldan_Mission3ExchangeE_02);
				yield return PlayLineOnlyOnce(friendlyActor, VO_Story_Hero_Guldan_Male_Orc_Story_Guldan_Mission3ExchangeE_03);
			}
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
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Nerzhul_Male_Orc_Story_Guldan_Mission3ExchangeA_01);
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Guldan_Male_Orc_Story_Guldan_Mission3ExchangeA_02);
			break;
		case 3:
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Nerzhul_Male_Orc_Story_Guldan_Mission3ExchangeB_01);
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Guldan_Male_Orc_Story_Guldan_Mission3ExchangeB_02);
			break;
		case 7:
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Nerzhul_Male_Orc_Story_Guldan_Mission3ExchangeC_01);
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Guldan_Male_Orc_Story_Guldan_Mission3ExchangeC_02);
			break;
		case 11:
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Nerzhul_Male_Orc_Story_Guldan_Mission3ExchangeD_01);
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Guldan_Male_Orc_Story_Guldan_Mission3ExchangeD_02);
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Nerzhul_Male_Orc_Story_Guldan_Mission3ExchangeD_03);
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Guldan_Male_Orc_Story_Guldan_Mission3ExchangeD_04);
			break;
		}
	}
}
