using System.Collections;
using System.Collections.Generic;
using Blizzard.T5.Core;
using UnityEngine;

public class BoH_Faelin_07 : BoH_Faelin_Dungeon
{
	private static Map<GameEntityOption, bool> s_booleanOptions = InitBooleanOptions();

	private static readonly AssetReference VO_Story_11_Caye_012hp_Female_Nightborne_Story_Faelin_Mission7ExchangeA_01 = new AssetReference("VO_Story_11_Caye_012hp_Female_Nightborne_Story_Faelin_Mission7ExchangeA_01.prefab:2b1d339a24a412d4d80019804548ba01");

	private static readonly AssetReference VO_Story_11_Caye_012hp_Female_Nightborne_Story_Faelin_PreMission7_01 = new AssetReference("VO_Story_11_Caye_012hp_Female_Nightborne_Story_Faelin_PreMission7_01.prefab:e8e46674b1267334ea1303a67c8a8393");

	private static readonly AssetReference VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission7ExchangeA_01 = new AssetReference("VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission7ExchangeA_01.prefab:cff48ecc66c2f5f41894269e7feb67fc");

	private static readonly AssetReference VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission7ExchangeC_01 = new AssetReference("VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission7ExchangeC_01.prefab:6d6302e749fdbf54ba8d27d7b67d0d86");

	private static readonly AssetReference VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission7ExchangeD_01 = new AssetReference("VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission7ExchangeD_01.prefab:e64bde16125bbe247903ec2e1bfdaa65");

	private static readonly AssetReference VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission7Start_01 = new AssetReference("VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission7Start_01.prefab:e199c542ea58df14daa2d0d3c69c0f64");

	private static readonly AssetReference VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission7Victory_01 = new AssetReference("VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission7Victory_01.prefab:8cc2e3e638281d9408b194cdc7b67b58");

	private static readonly AssetReference VO_Story_11_Finley_009hp_Male_Murloc_Story_Faelin_Mission7_Lorebook_2_01 = new AssetReference("VO_Story_11_Finley_009hp_Male_Murloc_Story_Faelin_Mission7_Lorebook_2_01.prefab:0965cddcb6f643b438094992203c839c");

	private static readonly AssetReference VO_Story_11_Finley_009hp_Male_Murloc_Story_Faelin_Mission7ExchangeE_01 = new AssetReference("VO_Story_11_Finley_009hp_Male_Murloc_Story_Faelin_Mission7ExchangeE_01.prefab:4f6851bfc59d1d14180e29581dac9aee");

	private static readonly AssetReference VO_Story_11_Grace_005hp_Female_Human_Story_Faelin_Mission7ExchangeC_01 = new AssetReference("VO_Story_11_Grace_005hp_Female_Human_Story_Faelin_Mission7ExchangeC_01.prefab:910393b17d41310458d533997a71b213");

	private static readonly AssetReference VO_Story_11_Halus_010hp_Male_BloodElf_Story_Faelin_Mission7ExchangeC_01 = new AssetReference("VO_Story_11_Halus_010hp_Male_BloodElf_Story_Faelin_Mission7ExchangeC_01.prefab:3c7dfe39f673e5f4780c9c2bb17e01d1");

	private static readonly AssetReference VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission7_Lorebook_1_01 = new AssetReference("VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission7_Lorebook_1_01.prefab:6ca4d6a56110f3f4aa1571eea286f310");

	private static readonly AssetReference VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission7ExchangeB_01 = new AssetReference("VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission7ExchangeB_01.prefab:d863dedb213a21c469a6b9d31560676d");

	private static readonly AssetReference VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission7ExchangeD_01 = new AssetReference("VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission7ExchangeD_01.prefab:b7988fc92bd21ee42a4bbc80c7eac16e");

	private static readonly AssetReference VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission7Start_01 = new AssetReference("VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission7Start_01.prefab:5d392ad1b6a162b489f57c9a54725c0e");

	private static readonly AssetReference VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission7Victory_01 = new AssetReference("VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission7Victory_01.prefab:49159bf68f1ec3040b630190fd3343d1");

	private static readonly AssetReference VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission7Victory_02 = new AssetReference("VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission7Victory_02.prefab:ec7b6329029a44648bad1d7b4331665f");

	private static readonly AssetReference VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission7Victory_03 = new AssetReference("VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission7Victory_03.prefab:aa2dba9d6ef01df419fed0886a790ef2");

	private static readonly AssetReference VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_PreMission7_01 = new AssetReference("VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_PreMission7_01.prefab:4d1544424ecfeb24c9416159f85ca5d5");

	private static readonly AssetReference VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_PreMission7_02 = new AssetReference("VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_PreMission7_02.prefab:d723083fe0a21d04caa0eaa71ca16eef");

	private static readonly AssetReference VO_Story_11_Mission7_Lorebook2_Female_NightElf_Story_Faelin_Mission7_Lorebook_2_01 = new AssetReference("VO_Story_11_Mission7_Lorebook2_Female_NightElf_Story_Faelin_Mission7_Lorebook_2_01.prefab:850e04953fef8514eaad11fb918dcd1c");

	private static readonly AssetReference VO_Story_11_Ozumat_007hb_Male_Beast_Story_Faelin_Mission7Death_01 = new AssetReference("VO_Story_11_Ozumat_007hb_Male_Beast_Story_Faelin_Mission7Death_01.prefab:c119f4c9f25172442b600d3b82caefd9");

	private static readonly AssetReference VO_Story_11_Ozumat_007hb_Male_Beast_Story_Faelin_Mission7EmoteResponse_01 = new AssetReference("VO_Story_11_Ozumat_007hb_Male_Beast_Story_Faelin_Mission7EmoteResponse_01.prefab:0b177dd6eeb7b3442b586d9a26d50cd1");

	private static readonly AssetReference VO_Story_11_Ozumat_007hb_Male_Beast_Story_Faelin_Mission7Loss_01 = new AssetReference("VO_Story_11_Ozumat_007hb_Male_Beast_Story_Faelin_Mission7Loss_01.prefab:56cabed3ed74b78429cd7a4960adeb63");

	private Notification m_popup;

	private Dictionary<int, string[]> m_popUpInfo = new Dictionary<int, string[]>
	{
		{
			228,
			new string[1] { "BOH_FAELIN_MISSION7_LOREBOOK_01" }
		},
		{
			229,
			new string[1] { "BOH_FAELIN_MISSION7_LOREBOOK_02" }
		},
		{
			230,
			new string[1] { "BOH_FAELIN_MISSION7_LOREBOOK_03" }
		}
	};

	private float popUpScale = 1.65f;

	private Vector3 popUpPos;

	public static readonly AssetReference IniBrassRing = new AssetReference("HS25-189_H_Ini_BrassRing_Quote.prefab:9f20435bdae99d24bae357f002abcf27");

	public static readonly AssetReference CayeBrassRing = new AssetReference("LC23-001_H_Caye_BrassRing_Quote.prefab:2364e8ce64f9c404fb569a86b17f3d01");

	public static readonly AssetReference BookBrassRing = new AssetReference("Wisdomball_Pop-up_BrassRing_Quote.prefab:896ee20514caff74db639aa7055838f6");

	public static readonly AssetReference FinleyBrassRing = new AssetReference("HS25-190_M_Finley_BrassRing_Quote.prefab:7123c129afe769e4aa9fb262a8f7c919");

	public static readonly AssetReference HalusBrassRing = new AssetReference("LC23-003_H_Halus_BrassRing_Quote.prefab:36e0aabb3ed4ce84599d9dd4e9ebdcf5");

	public static readonly AssetReference GraceBrassRing = new AssetReference("LC23-002_H_Grace_BrassRing_Quote.prefab:1f849ba2e303ff3449d34b96f960c96a");

	private HashSet<string> m_playedLines = new HashSet<string>();

	private static Map<GameEntityOption, bool> InitBooleanOptions()
	{
		return new Map<GameEntityOption, bool> { 
		{
			GameEntityOption.DO_OPENING_TAUNTS,
			false
		} };
	}

	public BoH_Faelin_07()
	{
		m_gameOptions.AddBooleanOptions(s_booleanOptions);
	}

	public override void PreloadAssets()
	{
		base.PreloadAssets();
		List<string> m_SoundFiles = new List<string>
		{
			VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission7Start_01, VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission7Start_01, VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission7ExchangeA_01, VO_Story_11_Caye_012hp_Female_Nightborne_Story_Faelin_Mission7ExchangeA_01, VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission7ExchangeB_01, VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission7ExchangeC_01, VO_Story_11_Grace_005hp_Female_Human_Story_Faelin_Mission7ExchangeC_01, VO_Story_11_Finley_009hp_Male_Murloc_Story_Faelin_Mission7ExchangeE_01, VO_Story_11_Halus_010hp_Male_BloodElf_Story_Faelin_Mission7ExchangeC_01, VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission7ExchangeD_01,
			VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission7ExchangeD_01, VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission7Victory_01, VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission7Victory_02, VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission7Victory_01, VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission7Victory_03, VO_Story_11_Ozumat_007hb_Male_Beast_Story_Faelin_Mission7Death_01, VO_Story_11_Ozumat_007hb_Male_Beast_Story_Faelin_Mission7EmoteResponse_01, VO_Story_11_Ozumat_007hb_Male_Beast_Story_Faelin_Mission7Loss_01, VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission7_Lorebook_1_01, VO_Story_11_Mission7_Lorebook2_Female_NightElf_Story_Faelin_Mission7_Lorebook_2_01,
			VO_Story_11_Finley_009hp_Male_Murloc_Story_Faelin_Mission7_Lorebook_2_01
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

	public override void OnCreateGame()
	{
		base.OnCreateGame();
		m_Mission_EnemyPlayIdleLines = false;
		m_OverrideMusicTrack = MusicPlaylistType.InGame_TSC_Boss;
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
		popUpPos = new Vector3(0f, 0f, -40f);
		switch (missionEvent)
		{
		case 515:
			yield return MissionPlayVO(enemyActor, VO_Story_11_Ozumat_007hb_Male_Beast_Story_Faelin_Mission7EmoteResponse_01);
			break;
		case 514:
			yield return MissionPlayVO(friendlyActor, VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission7Start_01);
			yield return MissionPlayVO(IniBrassRing, VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission7Start_01);
			yield return MissionPlayVO(enemyActor, VO_Story_11_Ozumat_007hb_Male_Beast_Story_Faelin_Mission7Loss_01);
			break;
		case 507:
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVO(enemyActor, VO_Story_11_Ozumat_007hb_Male_Beast_Story_Faelin_Mission7Loss_01);
			GameState.Get().SetBusy(busy: false);
			break;
		case 504:
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVO(IniBrassRing, VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission7Victory_01);
			yield return MissionPlayVO(IniBrassRing, VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission7Victory_02);
			GameState.Get().SetBusy(busy: false);
			break;
		case 101:
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVO(friendlyActor, VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission7Victory_01);
			yield return MissionPlayVO(IniBrassRing, VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission7Victory_03);
			GameState.Get().SetBusy(busy: false);
			break;
		case 102:
			yield return MissionPlayVO(friendlyActor, VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission7ExchangeC_01);
			yield return MissionPlayVO(HalusBrassRing, VO_Story_11_Halus_010hp_Male_BloodElf_Story_Faelin_Mission7ExchangeC_01);
			break;
		case 103:
			yield return MissionPlayVO(friendlyActor, VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission7ExchangeC_01);
			yield return MissionPlayVO(GraceBrassRing, VO_Story_11_Grace_005hp_Female_Human_Story_Faelin_Mission7ExchangeC_01);
			break;
		case 104:
			yield return MissionPlayVO(friendlyActor, VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission7ExchangeC_01);
			yield return MissionPlayVO(FinleyBrassRing, VO_Story_11_Finley_009hp_Male_Murloc_Story_Faelin_Mission7ExchangeE_01);
			break;
		case 228:
			if (!(m_popup != null))
			{
				GameState.Get().SetBusy(busy: true);
				m_popup = NotificationManager.Get().CreatePopupText(UserAttentionBlocker.NONE, popUpPos, TutorialEntity.GetTextScale() * popUpScale, GameStrings.Get(m_popUpInfo[missionEvent][0]), convertLegacyPosition: false, NotificationManager.PopupTextType.FANCY);
				yield return GameEntity.Coroutines.StartCoroutine(PlaySoundAndBlockSpeech(VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission7_Lorebook_1_01, Notification.SpeechBubbleDirection.None, GetEnemyActorByCardId("Story_11_Mission1_Lorebook1"), 2.5f));
				NotificationManager.Get().DestroyNotification(m_popup, 0f);
				m_popup = null;
				GameState.Get().SetBusy(busy: false);
			}
			break;
		case 229:
			if (!(m_popup != null))
			{
				GameState.Get().SetBusy(busy: true);
				m_popup = NotificationManager.Get().CreatePopupText(UserAttentionBlocker.NONE, popUpPos, TutorialEntity.GetTextScale() * popUpScale, GameStrings.Get(m_popUpInfo[missionEvent][0]), convertLegacyPosition: false, NotificationManager.PopupTextType.FANCY);
				yield return GameEntity.Coroutines.StartCoroutine(PlaySoundAndBlockSpeech(VO_Story_11_Mission7_Lorebook2_Female_NightElf_Story_Faelin_Mission7_Lorebook_2_01, Notification.SpeechBubbleDirection.None, GetEnemyActorByCardId("Story_11_Mission1_Lorebook1"), 2.5f));
				NotificationManager.Get().DestroyNotification(m_popup, 0f);
				m_popup = null;
				GameState.Get().SetBusy(busy: false);
			}
			break;
		case 230:
			if (!(m_popup != null))
			{
				GameState.Get().SetBusy(busy: true);
				m_popup = NotificationManager.Get().CreatePopupText(UserAttentionBlocker.NONE, popUpPos, TutorialEntity.GetTextScale() * popUpScale, GameStrings.Get(m_popUpInfo[missionEvent][0]), convertLegacyPosition: false, NotificationManager.PopupTextType.FANCY);
				yield return GameEntity.Coroutines.StartCoroutine(PlaySoundAndBlockSpeech(VO_Story_11_Mission7_Lorebook2_Female_NightElf_Story_Faelin_Mission7_Lorebook_2_01, Notification.SpeechBubbleDirection.None, GetEnemyActorByCardId("Story_11_Mission1_Lorebook1"), 2.5f));
				NotificationManager.Get().DestroyNotification(m_popup, 0f);
				m_popup = null;
				GameState.Get().SetBusy(busy: false);
				yield return MissionPlayVO(FinleyBrassRing, VO_Story_11_Finley_009hp_Male_Murloc_Story_Faelin_Mission7_Lorebook_2_01);
			}
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
		GameState.Get().GetOpposingSidePlayer().GetHero()
			.GetCard()
			.GetActor();
		Actor friendlyActor = GameState.Get().GetFriendlySidePlayer().GetHero()
			.GetCard()
			.GetActor();
		switch (turn)
		{
		case 1:
			yield return MissionPlayVO(friendlyActor, VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission7ExchangeA_01);
			yield return MissionPlayVO(CayeBrassRing, VO_Story_11_Caye_012hp_Female_Nightborne_Story_Faelin_Mission7ExchangeA_01);
			break;
		case 9:
			yield return MissionPlayVO(IniBrassRing, VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission7ExchangeB_01);
			break;
		case 11:
			yield return MissionPlayVO(IniBrassRing, VO_Story_11_Ini_004hp_Female_MechaGnome_Story_Faelin_Mission7ExchangeD_01);
			yield return MissionPlayVO(friendlyActor, VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission7ExchangeD_01);
			break;
		}
	}

	public override bool NotifyOfBattlefieldCardClicked(Entity clickedEntity, bool wasInTargetMode)
	{
		if (!base.NotifyOfBattlefieldCardClicked(clickedEntity, wasInTargetMode))
		{
			return false;
		}
		if (!wasInTargetMode)
		{
			if (clickedEntity.GetCardId() == "Story_11_Mission7_Lorebook1")
			{
				if (m_popup == null)
				{
					Network.Get().SendClientScriptGameEvent(ClientScriptGameEventType.MISSION_EVENT, 228);
					HandleMissionEvent(228);
				}
				return false;
			}
			if (clickedEntity.GetCardId() == "Story_11_Mission7_Lorebook2")
			{
				if (m_popup == null)
				{
					Network.Get().SendClientScriptGameEvent(ClientScriptGameEventType.MISSION_EVENT, 229);
					HandleMissionEvent(229);
				}
				return false;
			}
			if (clickedEntity.GetCardId() == "Story_11_Mission7_Lorebook3")
			{
				if (m_popup == null)
				{
					Network.Get().SendClientScriptGameEvent(ClientScriptGameEventType.MISSION_EVENT, 230);
					HandleMissionEvent(230);
				}
				return false;
			}
		}
		return true;
	}
}
