using System.Collections;
using System.Collections.Generic;

public class TB_MagicalGuardians_Fight_001 : TB_MagicalGuardians_Dungeon
{
	private static readonly AssetReference VO_HERO_08ad_Male_BloodElf_Thanks_01 = new AssetReference("VO_HERO_08ad_Male_BloodElf_Thanks_01.prefab:de3bb8be43953e346bb93c5283d1f58c");

	private static readonly AssetReference VO_HERO_08ad_Male_BloodElf_Well_Played_01 = new AssetReference("VO_HERO_08ad_Male_BloodElf_Well_Played_01.prefab:99b037a4883e18047b6ef1f79cda6313");

	private static readonly AssetReference VO_HERO_08ad_Male_BloodElf_Attack_01 = new AssetReference("VO_HERO_08ad_Male_BloodElf_Attack_01.prefab:083aebd386e3d984089bebd33d9867d8");

	private static readonly AssetReference VO_HERO_08ad_Male_BloodElf_Oops_01 = new AssetReference("VO_HERO_08ad_Male_BloodElf_Oops_01.prefab:4c1f2156e1510d74987d4a58f11d1338");

	private static readonly AssetReference VO_HERO_08ad_Male_BloodElf_Picked_01 = new AssetReference("VO_HERO_08ad_Male_BloodElf_Picked_01.prefab:c6a90985d2261cb47aefe6f32d88a48c");

	private static readonly AssetReference VO_HERO_08ad_Male_BloodElf_Greetings_01 = new AssetReference("VO_HERO_08ad_Male_BloodElf_Greetings_01.prefab:ed71a73d4970a7547a3dde597882e751");

	private static readonly AssetReference VO_HERO_08ad_Male_BloodElf_Threaten_01 = new AssetReference("VO_HERO_08ad_Male_BloodElf_Threaten_01.prefab:14750ced6ec62a54c808288bc6eca593");

	private static readonly AssetReference VO_HERO_08ad_Male_BloodElf_Wow_01 = new AssetReference("VO_HERO_08ad_Male_BloodElf_Wow_01.prefab:7654e11573c69b647b83536afcb1a29f");

	private static readonly AssetReference VO_HERO_08ad_Male_BloodElf_Start_01 = new AssetReference("VO_HERO_08ad_Male_BloodElf_Start_01.prefab:099206039720f654a9e23ca46a4369f6");

	private static readonly AssetReference VO_HERO_06z_Female_NightElf_Start_01 = new AssetReference("VO_HERO_06z_Female_NightElf_Start_01.prefab:0c167a0af7ad3fc4bb4bf9aa2f724c18");

	private static readonly AssetReference VO_HERO_07w_Female_Gnome_Start_01 = new AssetReference("VO_HERO_07w_Female_Gnome_Start_01.prefab:9ac4e0f4f95e7624f898b244ec733aed");

	private static readonly AssetReference VO_HERO_09v_Female_NightElf_Start_01 = new AssetReference("VO_HERO_09v_Female_NightElf_Start_01.prefab:0b2956901495f774ea89f4abcc1a5d51");

	private static readonly AssetReference VO_HERO_04z_Female_BloodElf_Start_01 = new AssetReference("VO_HERO_04z_Female_BloodElf_Start_01.prefab:5a0a0392eea99064c8cd874f0c12ce46");

	private HashSet<string> m_playedLines = new HashSet<string>();

	public override void PreloadAssets()
	{
		base.PreloadAssets();
		List<string> m_SoundFiles = new List<string>
		{
			VO_HERO_08ad_Male_BloodElf_Picked_01, VO_HERO_08ad_Male_BloodElf_Attack_01, VO_HERO_08ad_Male_BloodElf_Oops_01, VO_HERO_08ad_Male_BloodElf_Greetings_01, VO_HERO_08ad_Male_BloodElf_Threaten_01, VO_HERO_08ad_Male_BloodElf_Wow_01, VO_HERO_08ad_Male_BloodElf_Well_Played_01, VO_HERO_08ad_Male_BloodElf_Thanks_01, VO_HERO_08ad_Male_BloodElf_Start_01, VO_HERO_06z_Female_NightElf_Start_01,
			VO_HERO_07w_Female_Gnome_Start_01, VO_HERO_09v_Female_NightElf_Start_01, VO_HERO_04z_Female_BloodElf_Start_01
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
		GameState.Get().GetFriendlySidePlayer().GetHero()
			.GetCard()
			.GetActor();
		switch (missionEvent)
		{
		case 500:
			yield return MissionPlayVO(enemyActor, VO_HERO_08ad_Male_BloodElf_Attack_01);
			break;
		case 504:
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVO(enemyActor, VO_HERO_08ad_Male_BloodElf_Threaten_01);
			GameState.Get().SetBusy(busy: false);
			break;
		case 506:
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVO(enemyActor, VO_HERO_08ad_Male_BloodElf_Oops_01);
			GameState.Get().SetBusy(busy: false);
			break;
		case 514:
			NotificationManager.Get().ResetSoundsPlayedThisSession();
			yield return MissionPlayVO("TB_MagicalGuardians_HERO_08ad", VO_HERO_08ad_Male_BloodElf_Start_01);
			yield return MissionPlayVO("HERO_06z", VO_HERO_06z_Female_NightElf_Start_01);
			yield return MissionPlayVO("HERO_07w", VO_HERO_07w_Female_Gnome_Start_01);
			yield return MissionPlayVO("HERO_04z", VO_HERO_04z_Female_BloodElf_Start_01);
			yield return MissionPlayVO("HERO_09v", VO_HERO_09v_Female_NightElf_Start_01);
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
		GameState.Get().GetFriendlySidePlayer().GetHero()
			.GetCard()
			.GetActor();
		if (turn == 1)
		{
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVO(enemyActor, VO_HERO_08ad_Male_BloodElf_Picked_01);
			GameState.Get().SetBusy(busy: false);
		}
	}

	public override IEnumerator PlayCustomEmoteResponse(EmoteType emoteType, CardSoundSpell emoteSpell)
	{
		Actor enemyActor = GameState.Get().GetOpposingSidePlayer().GetHeroCard()
			.GetActor();
		GameState.Get().GetFriendlySidePlayer().GetHero()
			.GetCardId();
		while (m_enemySpeaking)
		{
			yield return null;
		}
		switch (emoteType)
		{
		case EmoteType.THANKS:
			yield return MissionPlayVOOnce(enemyActor, VO_HERO_08ad_Male_BloodElf_Thanks_01);
			break;
		case EmoteType.WOW:
			yield return MissionPlayVOOnce(enemyActor, VO_HERO_08ad_Male_BloodElf_Wow_01);
			break;
		case EmoteType.GREETINGS:
			yield return MissionPlayVOOnce(enemyActor, VO_HERO_08ad_Male_BloodElf_Greetings_01);
			break;
		}
	}
}
