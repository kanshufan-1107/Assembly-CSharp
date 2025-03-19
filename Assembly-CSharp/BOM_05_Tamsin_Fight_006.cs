using System.Collections;
using System.Collections.Generic;

public class BOM_05_Tamsin_Fight_006 : BOM_05_Tamsin_Dungeon
{
	private static readonly AssetReference VO_Story_Hero_ShadowPriestXyrella_Female_Draenei_BOM_Tamsin_Mission6Death_01 = new AssetReference("VO_Story_Hero_ShadowPriestXyrella_Female_Draenei_BOM_Tamsin_Mission6Death_01.prefab:29559b879e1f9a94db8421f001177b52");

	private static readonly AssetReference VO_Story_Hero_ShadowPriestXyrella_Female_Draenei_BOM_Tamsin_Mission6EmoteResponse_01 = new AssetReference("VO_Story_Hero_ShadowPriestXyrella_Female_Draenei_BOM_Tamsin_Mission6EmoteResponse_01.prefab:358cbac8500013243ac2b8342a08892d");

	private static readonly AssetReference VO_Story_Hero_ShadowPriestXyrella_Female_Draenei_BOM_Tamsin_Mission6ExchangeE_02 = new AssetReference("VO_Story_Hero_ShadowPriestXyrella_Female_Draenei_BOM_Tamsin_Mission6ExchangeE_02.prefab:81c7b7fde512aaa4da062a3c17aeffc2");

	private static readonly AssetReference VO_Story_Hero_ShadowPriestXyrella_Female_Draenei_BOM_Tamsin_Mission6ExchangeF_01 = new AssetReference("VO_Story_Hero_ShadowPriestXyrella_Female_Draenei_BOM_Tamsin_Mission6ExchangeF_01.prefab:b515d4d54700fc444b2021d6db0f2b79");

	private static readonly AssetReference VO_Story_Hero_ShadowPriestXyrella_Female_Draenei_BOM_Tamsin_Mission6ExchangeG_01 = new AssetReference("VO_Story_Hero_ShadowPriestXyrella_Female_Draenei_BOM_Tamsin_Mission6ExchangeG_01.prefab:b90f8d499969ca049ac7938c63e231fc");

	private static readonly AssetReference VO_Story_Hero_ShadowPriestXyrella_Female_Draenei_BOM_Tamsin_Mission6HeroPower_01 = new AssetReference("VO_Story_Hero_ShadowPriestXyrella_Female_Draenei_BOM_Tamsin_Mission6HeroPower_01.prefab:a16b4e9bf3d45864cbebe1baebc0f026");

	private static readonly AssetReference VO_Story_Hero_ShadowPriestXyrella_Female_Draenei_BOM_Tamsin_Mission6HeroPower_02 = new AssetReference("VO_Story_Hero_ShadowPriestXyrella_Female_Draenei_BOM_Tamsin_Mission6HeroPower_02.prefab:e6166a0c79ed12c4eb16e704979a73f5");

	private static readonly AssetReference VO_Story_Hero_ShadowPriestXyrella_Female_Draenei_BOM_Tamsin_Mission6HeroPower_03 = new AssetReference("VO_Story_Hero_ShadowPriestXyrella_Female_Draenei_BOM_Tamsin_Mission6HeroPower_03.prefab:e9bc70b98cff3ad409db609e6b6c87f8");

	private static readonly AssetReference VO_Story_Hero_ShadowPriestXyrella_Female_Draenei_BOM_Tamsin_Mission6HeroPower_04 = new AssetReference("VO_Story_Hero_ShadowPriestXyrella_Female_Draenei_BOM_Tamsin_Mission6HeroPower_04.prefab:66b219bae3166d749b4ea3fa7cef9963");

	private static readonly AssetReference VO_Story_Hero_ShadowPriestXyrella_Female_Draenei_BOM_Tamsin_Mission6HeroPower_05 = new AssetReference("VO_Story_Hero_ShadowPriestXyrella_Female_Draenei_BOM_Tamsin_Mission6HeroPower_05.prefab:d204a268dbac7384592c956359c0d379");

	private static readonly AssetReference VO_Story_Hero_ShadowPriestXyrella_Female_Draenei_BOM_Tamsin_Mission6Idle_01 = new AssetReference("VO_Story_Hero_ShadowPriestXyrella_Female_Draenei_BOM_Tamsin_Mission6Idle_01.prefab:d72bd63618901dc48a4f7e986565f6c5");

	private static readonly AssetReference VO_Story_Hero_ShadowPriestXyrella_Female_Draenei_BOM_Tamsin_Mission6Idle_02 = new AssetReference("VO_Story_Hero_ShadowPriestXyrella_Female_Draenei_BOM_Tamsin_Mission6Idle_02.prefab:a7110443aed9d1e4787d9a9055d7b2c6");

	private static readonly AssetReference VO_Story_Hero_ShadowPriestXyrella_Female_Draenei_BOM_Tamsin_Mission6Idle_03 = new AssetReference("VO_Story_Hero_ShadowPriestXyrella_Female_Draenei_BOM_Tamsin_Mission6Idle_03.prefab:cda551ad6292afb4e95431176ebb6f7a");

	private static readonly AssetReference VO_Story_Hero_ShadowPriestXyrella_Female_Draenei_BOM_Tamsin_Mission6Loss_01 = new AssetReference("VO_Story_Hero_ShadowPriestXyrella_Female_Draenei_BOM_Tamsin_Mission6Loss_01.prefab:bb4d6050f192a8640b50d0c3647a89de");

	private static readonly AssetReference VO_Story_Hero_Tamsin_Female_Undead_BOM_Tamsin_Mission6ExchangeA_02 = new AssetReference("VO_Story_Hero_Tamsin_Female_Undead_BOM_Tamsin_Mission6ExchangeA_02.prefab:3b647bac7f3c68a4d973e5fae211e363");

	private static readonly AssetReference VO_Story_Hero_Tamsin_Female_Undead_BOM_Tamsin_Mission6ExchangeB_01 = new AssetReference("VO_Story_Hero_Tamsin_Female_Undead_BOM_Tamsin_Mission6ExchangeB_01.prefab:f811a9e4b05f7304aa2a824a8c4cfc5d");

	private static readonly AssetReference VO_Story_Hero_Tamsin_Female_Undead_BOM_Tamsin_Mission6ExchangeC_02 = new AssetReference("VO_Story_Hero_Tamsin_Female_Undead_BOM_Tamsin_Mission6ExchangeC_02.prefab:57ffe8a27dc996d459e5b794f20047e7");

	private static readonly AssetReference VO_Story_Hero_Tamsin_Female_Undead_BOM_Tamsin_Mission6ExchangeE_01 = new AssetReference("VO_Story_Hero_Tamsin_Female_Undead_BOM_Tamsin_Mission6ExchangeE_01.prefab:fd298797d112ace4b8d5c6dd5fd8f04b");

	private static readonly AssetReference VO_Story_Hero_Tamsin_Female_Undead_BOM_Tamsin_Mission6ExchangeF_02 = new AssetReference("VO_Story_Hero_Tamsin_Female_Undead_BOM_Tamsin_Mission6ExchangeF_02.prefab:d5b9df35af9a634418fd782ad3e96df8");

	private static readonly AssetReference VO_Story_Hero_Tamsin_Female_Undead_BOM_Tamsin_Mission6Intro_02 = new AssetReference("VO_Story_Hero_Tamsin_Female_Undead_BOM_Tamsin_Mission6Intro_02.prefab:03aa3d516da008f4f96543473da48952");

	private static readonly AssetReference VO_Story_Hero_Tamsin_Female_Undead_BOM_Tamsin_Mission6Victory_01 = new AssetReference("VO_Story_Hero_Tamsin_Female_Undead_BOM_Tamsin_Mission6Victory_01.prefab:4cc0b2669ba35d249a74a2f76b841936");

	private static readonly AssetReference VO_Story_Hero_Xyrella_Female_Draenei_BOM_Tamsin_Mission6Death_01 = new AssetReference("VO_Story_Hero_Xyrella_Female_Draenei_BOM_Tamsin_Mission6Death_01.prefab:dc458e9c9a6a4f44a9ca36d4ab0f1196");

	private static readonly AssetReference VO_Story_Hero_Xyrella_Female_Draenei_BOM_Tamsin_Mission6EmoteResponse_01 = new AssetReference("VO_Story_Hero_Xyrella_Female_Draenei_BOM_Tamsin_Mission6EmoteResponse_01.prefab:7ab0cbd8f3b18104b90fc02788388d17");

	private static readonly AssetReference VO_Story_Hero_Xyrella_Female_Draenei_BOM_Tamsin_Mission6ExchangeA_01 = new AssetReference("VO_Story_Hero_Xyrella_Female_Draenei_BOM_Tamsin_Mission6ExchangeA_01.prefab:5f7daa9c637c70e41b4ef3a28509d4d1");

	private static readonly AssetReference VO_Story_Hero_Xyrella_Female_Draenei_BOM_Tamsin_Mission6ExchangeB_02 = new AssetReference("VO_Story_Hero_Xyrella_Female_Draenei_BOM_Tamsin_Mission6ExchangeB_02.prefab:fccef57595891c04c91fea4b23403d04");

	private static readonly AssetReference VO_Story_Hero_Xyrella_Female_Draenei_BOM_Tamsin_Mission6ExchangeC_01 = new AssetReference("VO_Story_Hero_Xyrella_Female_Draenei_BOM_Tamsin_Mission6ExchangeC_01.prefab:c3cd2eaf5581efd48adf44e801789917");

	private static readonly AssetReference VO_Story_Hero_Xyrella_Female_Draenei_BOM_Tamsin_Mission6ExchangeD_01 = new AssetReference("VO_Story_Hero_Xyrella_Female_Draenei_BOM_Tamsin_Mission6ExchangeD_01.prefab:73b7f95be2f47ad4abe42629ed532983");

	private static readonly AssetReference VO_Story_Hero_Xyrella_Female_Draenei_BOM_Tamsin_Mission6Idle_01 = new AssetReference("VO_Story_Hero_Xyrella_Female_Draenei_BOM_Tamsin_Mission6Idle_01.prefab:a0f4a87d84b742e47b3fe47507be1bff");

	private static readonly AssetReference VO_Story_Hero_Xyrella_Female_Draenei_BOM_Tamsin_Mission6Intro_01 = new AssetReference("VO_Story_Hero_Xyrella_Female_Draenei_BOM_Tamsin_Mission6Intro_01.prefab:ae317f976e7bf014784a4f78686ba3db");

	private static readonly AssetReference VO_Story_Hero_Xyrella_Female_Draenei_BOM_Tamsin_Mission6Loss_01 = new AssetReference("VO_Story_Hero_Xyrella_Female_Draenei_BOM_Tamsin_Mission6Loss_01.prefab:b03ae48116ca5fb4a9ed46f2f2fa0f90");

	private static readonly AssetReference VO_PVPDR_Hero_Tamsin_Female_Forsaken_Death_01 = new AssetReference("VO_PVPDR_Hero_Tamsin_Female_Forsaken_Death_01.prefab:0261caf1dfbfae146bf927a03851b086");

	private List<string> m_InGame_BossDeathLines = new List<string> { VO_Story_Hero_Xyrella_Female_Draenei_BOM_Tamsin_Mission6Death_01, VO_Story_Hero_ShadowPriestXyrella_Female_Draenei_BOM_Tamsin_Mission6Death_01 };

	private List<string> m_InGame_EmoteResponseLines = new List<string> { VO_Story_Hero_ShadowPriestXyrella_Female_Draenei_BOM_Tamsin_Mission6EmoteResponse_01, VO_Story_Hero_Xyrella_Female_Draenei_BOM_Tamsin_Mission6EmoteResponse_01 };

	private List<string> m_InGame_BossUsesHeroPowerLines = new List<string> { VO_Story_Hero_ShadowPriestXyrella_Female_Draenei_BOM_Tamsin_Mission6HeroPower_01, VO_Story_Hero_ShadowPriestXyrella_Female_Draenei_BOM_Tamsin_Mission6HeroPower_02, VO_Story_Hero_ShadowPriestXyrella_Female_Draenei_BOM_Tamsin_Mission6HeroPower_03, VO_Story_Hero_ShadowPriestXyrella_Female_Draenei_BOM_Tamsin_Mission6HeroPower_04, VO_Story_Hero_ShadowPriestXyrella_Female_Draenei_BOM_Tamsin_Mission6HeroPower_05 };

	private List<string> m_InGame_BossIdleLines = new List<string> { VO_Story_Hero_ShadowPriestXyrella_Female_Draenei_BOM_Tamsin_Mission6Idle_01, VO_Story_Hero_ShadowPriestXyrella_Female_Draenei_BOM_Tamsin_Mission6Idle_02, VO_Story_Hero_ShadowPriestXyrella_Female_Draenei_BOM_Tamsin_Mission6Idle_03 };

	private List<string> m_InGame_LossPostExplosionLines = new List<string> { VO_Story_Hero_ShadowPriestXyrella_Female_Draenei_BOM_Tamsin_Mission6Loss_01, VO_Story_Hero_Xyrella_Female_Draenei_BOM_Tamsin_Mission6Loss_01 };

	private HashSet<string> m_playedLines = new HashSet<string>();

	public override void PreloadAssets()
	{
		base.PreloadAssets();
		List<string> m_SoundFiles = new List<string>
		{
			VO_Story_Hero_ShadowPriestXyrella_Female_Draenei_BOM_Tamsin_Mission6Death_01, VO_Story_Hero_ShadowPriestXyrella_Female_Draenei_BOM_Tamsin_Mission6EmoteResponse_01, VO_Story_Hero_ShadowPriestXyrella_Female_Draenei_BOM_Tamsin_Mission6ExchangeE_02, VO_Story_Hero_ShadowPriestXyrella_Female_Draenei_BOM_Tamsin_Mission6ExchangeF_01, VO_Story_Hero_ShadowPriestXyrella_Female_Draenei_BOM_Tamsin_Mission6ExchangeG_01, VO_Story_Hero_ShadowPriestXyrella_Female_Draenei_BOM_Tamsin_Mission6HeroPower_01, VO_Story_Hero_ShadowPriestXyrella_Female_Draenei_BOM_Tamsin_Mission6HeroPower_02, VO_Story_Hero_ShadowPriestXyrella_Female_Draenei_BOM_Tamsin_Mission6HeroPower_03, VO_Story_Hero_ShadowPriestXyrella_Female_Draenei_BOM_Tamsin_Mission6HeroPower_04, VO_Story_Hero_ShadowPriestXyrella_Female_Draenei_BOM_Tamsin_Mission6HeroPower_05,
			VO_Story_Hero_ShadowPriestXyrella_Female_Draenei_BOM_Tamsin_Mission6Idle_01, VO_Story_Hero_ShadowPriestXyrella_Female_Draenei_BOM_Tamsin_Mission6Idle_02, VO_Story_Hero_ShadowPriestXyrella_Female_Draenei_BOM_Tamsin_Mission6Idle_03, VO_Story_Hero_ShadowPriestXyrella_Female_Draenei_BOM_Tamsin_Mission6Loss_01, VO_Story_Hero_Tamsin_Female_Undead_BOM_Tamsin_Mission6ExchangeA_02, VO_Story_Hero_Tamsin_Female_Undead_BOM_Tamsin_Mission6ExchangeB_01, VO_Story_Hero_Tamsin_Female_Undead_BOM_Tamsin_Mission6ExchangeC_02, VO_Story_Hero_Tamsin_Female_Undead_BOM_Tamsin_Mission6ExchangeE_01, VO_Story_Hero_Tamsin_Female_Undead_BOM_Tamsin_Mission6ExchangeF_02, VO_Story_Hero_Tamsin_Female_Undead_BOM_Tamsin_Mission6Intro_02,
			VO_Story_Hero_Tamsin_Female_Undead_BOM_Tamsin_Mission6Victory_01, VO_Story_Hero_Xyrella_Female_Draenei_BOM_Tamsin_Mission6Death_01, VO_Story_Hero_Xyrella_Female_Draenei_BOM_Tamsin_Mission6EmoteResponse_01, VO_Story_Hero_Xyrella_Female_Draenei_BOM_Tamsin_Mission6ExchangeA_01, VO_Story_Hero_Xyrella_Female_Draenei_BOM_Tamsin_Mission6ExchangeB_02, VO_Story_Hero_Xyrella_Female_Draenei_BOM_Tamsin_Mission6ExchangeC_01, VO_Story_Hero_Xyrella_Female_Draenei_BOM_Tamsin_Mission6ExchangeD_01, VO_Story_Hero_Xyrella_Female_Draenei_BOM_Tamsin_Mission6Idle_01, VO_Story_Hero_Xyrella_Female_Draenei_BOM_Tamsin_Mission6Intro_01, VO_Story_Hero_Xyrella_Female_Draenei_BOM_Tamsin_Mission6Loss_01,
			VO_PVPDR_Hero_Tamsin_Female_Forsaken_Death_01
		};
		SetBossVOLines(m_SoundFiles);
		foreach (string soundFile in m_SoundFiles)
		{
			PreloadSound(soundFile);
		}
	}

	public override void OnCreateGame()
	{
		base.OnCreateGame();
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
		string opposingHeroCard = GameState.Get().GetOpposingSidePlayer().GetHero()
			.GetCardId();
		Gameplay.Get().GetNameBannerForSide(Player.Side.OPPOSING).SetName("");
		Gameplay.Get().GetNameBannerForSide(Player.Side.OPPOSING).UpdateHeroNameBanner();
		switch (missionEvent)
		{
		case 516:
			MissionPause(pause: true);
			yield return MissionPlayVO(enemyActor, m_InGame_BossDeathLines);
			MissionPause(pause: false);
			break;
		case 519:
			yield return MissionPlaySound(friendlyActor, VO_PVPDR_Hero_Tamsin_Female_Forsaken_Death_01);
			break;
		case 517:
			if (opposingHeroCard == "BOM_05_Xyrella_006hb")
			{
				yield return MissionPlayVO(enemyActor, VO_Story_Hero_Xyrella_Female_Draenei_BOM_Tamsin_Mission6Idle_01);
			}
			else
			{
				yield return MissionPlayVO(enemyActor, m_InGame_BossIdleLines);
			}
			break;
		case 510:
			yield return MissionPlayVO(enemyActor, m_InGame_BossUsesHeroPowerLines);
			break;
		case 515:
			if (opposingHeroCard == "BOM_05_Xyrella_006hb")
			{
				yield return MissionPlayVO(enemyActor, VO_Story_Hero_Xyrella_Female_Draenei_BOM_Tamsin_Mission6EmoteResponse_01);
			}
			else
			{
				yield return MissionPlayVO(enemyActor, VO_Story_Hero_ShadowPriestXyrella_Female_Draenei_BOM_Tamsin_Mission6EmoteResponse_01);
			}
			break;
		case 514:
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Xyrella_Female_Draenei_BOM_Tamsin_Mission6Intro_01);
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Tamsin_Female_Undead_BOM_Tamsin_Mission6Intro_02);
			break;
		case 507:
			MissionPause(pause: true);
			if (opposingHeroCard == "BOM_05_Xyrella_006hb")
			{
				yield return MissionPlayVO(enemyActor, VO_Story_Hero_Xyrella_Female_Draenei_BOM_Tamsin_Mission6Loss_01);
			}
			else
			{
				yield return MissionPlayVO(enemyActor, VO_Story_Hero_ShadowPriestXyrella_Female_Draenei_BOM_Tamsin_Mission6Loss_01);
			}
			MissionPause(pause: false);
			break;
		case 504:
			MissionPause(pause: true);
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Tamsin_Female_Undead_BOM_Tamsin_Mission6Victory_01);
			MissionPause(pause: false);
			break;
		case 100:
			MissionPause(pause: true);
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Xyrella_Female_Draenei_BOM_Tamsin_Mission6ExchangeC_01);
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Tamsin_Female_Undead_BOM_Tamsin_Mission6ExchangeC_02);
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Xyrella_Female_Draenei_BOM_Tamsin_Mission6ExchangeD_01);
			MissionPause(pause: false);
			break;
		case 101:
			MissionPause(pause: true);
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Tamsin_Female_Undead_BOM_Tamsin_Mission6ExchangeE_01);
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_ShadowPriestXyrella_Female_Draenei_BOM_Tamsin_Mission6ExchangeE_02);
			MissionPause(pause: false);
			break;
		case 102:
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_ShadowPriestXyrella_Female_Draenei_BOM_Tamsin_Mission6ExchangeF_01);
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Tamsin_Female_Undead_BOM_Tamsin_Mission6ExchangeF_02);
			break;
		case 103:
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_ShadowPriestXyrella_Female_Draenei_BOM_Tamsin_Mission6ExchangeG_01);
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
		Gameplay.Get().GetNameBannerForSide(Player.Side.OPPOSING).SetName("");
		Gameplay.Get().GetNameBannerForSide(Player.Side.OPPOSING).UpdateHeroNameBanner();
		switch (turn)
		{
		case 5:
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Xyrella_Female_Draenei_BOM_Tamsin_Mission6ExchangeA_01);
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Tamsin_Female_Undead_BOM_Tamsin_Mission6ExchangeA_02);
			break;
		case 9:
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Tamsin_Female_Undead_BOM_Tamsin_Mission6ExchangeB_01);
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Xyrella_Female_Draenei_BOM_Tamsin_Mission6ExchangeB_02);
			break;
		}
	}
}
