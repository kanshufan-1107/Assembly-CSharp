using System.Collections;
using System.Collections.Generic;
using Blizzard.T5.Core;
using UnityEngine;

public class BoH_Faelin_Story_Prologue2 : BoH_Faelin_Dungeon
{
	private bool m_isIntroducingCharacter;

	private static Map<GameEntityOption, bool> s_booleanOptions = InitBooleanOptions();

	private static readonly AssetReference VO_Female_Gnome_Story_11_Ini_001hp_InGame_Introduction_01_01_b = new AssetReference("VO_Female_Gnome_Story_11_Ini_001hp_InGame_Introduction_01_01_b.prefab:f86ba45a2b6a4be1a6867bff0dfb82ff");

	private static readonly AssetReference VO_Female_Gnome_Story_11_Ini_001hp_InGame_Turn_01_01_e = new AssetReference("VO_Female_Gnome_Story_11_Ini_001hp_InGame_Turn_01_01_e.prefab:b94783821b1f43a8b43b32bc3d5e0ede");

	private static readonly AssetReference VO_Female_Human_Story_11_GracePrologue_InGame_HE_Custom_102_01_a = new AssetReference("VO_Female_Human_Story_11_GracePrologue_InGame_HE_Custom_102_01_a.prefab:f8591e2d26fb4964b2951185d8ea6b4d");

	private static readonly AssetReference VO_Female_Human_Story_11_GracePrologue_InGame_HE_Custom_105_01_b = new AssetReference("VO_Female_Human_Story_11_GracePrologue_InGame_HE_Custom_105_01_b.prefab:7d317fed520345d7b3e4f797a84ece13");

	private static readonly AssetReference VO_Female_Nightborne_Story_11_Caye_012hp_InGame_HE_Custom_01 = new AssetReference("VO_Female_Nightborne_Story_11_Caye_012hp_InGame_HE_Custom_01.prefab:ffc64d1383004aa4a768af3cafa577f2");

	private static readonly AssetReference VO_Female_Nightborne_Story_11_Caye_012hp_InGame_HE_Custom_02 = new AssetReference("VO_Female_Nightborne_Story_11_Caye_012hp_InGame_HE_Custom_02.prefab:4dd1661d0e554118acb401cf783ac34d");

	private static readonly AssetReference VO_Female_Nightborne_Story_11_Caye_012hp_InGame_HE_Custom_101_01_a = new AssetReference("VO_Female_Nightborne_Story_11_Caye_012hp_InGame_HE_Custom_101_01_a.prefab:6d18777bc76345ff8f521ff9cf06661f");

	private static readonly AssetReference VO_Female_Nightborne_Story_11_Caye_012hp_InGame_HE_Custom_102_01_b = new AssetReference("VO_Female_Nightborne_Story_11_Caye_012hp_InGame_HE_Custom_102_01_b.prefab:d0c678a00488472aa9e3e3379d7a921d");

	private static readonly AssetReference VO_Female_Nightborne_Story_11_Caye_012hp_InGame_HE_Custom_103_01_b = new AssetReference("VO_Female_Nightborne_Story_11_Caye_012hp_InGame_HE_Custom_103_01_b.prefab:a5dbac633ab940a1b9d776a01ab45cea");

	private static readonly AssetReference VO_Female_Nightborne_Story_11_Caye_012hp_InGame_HE_Custom_104_01_b = new AssetReference("VO_Female_Nightborne_Story_11_Caye_012hp_InGame_HE_Custom_104_01_b.prefab:5ab140cf6ad8479d8558658e74e7c3de");

	private static readonly AssetReference VO_Female_Nightborne_Story_11_Caye_012hp_InGame_Turn_01_01_d = new AssetReference("VO_Female_Nightborne_Story_11_Caye_012hp_InGame_Turn_01_01_d.prefab:b643c6c6baff4077a0e02208943d35af");

	private static readonly AssetReference VO_Female_Nightborne_Story_11_Caye_012hp_InGame_Turn_01_01_f = new AssetReference("VO_Female_Nightborne_Story_11_Caye_012hp_InGame_Turn_01_01_f.prefab:4cb8a0bf70d94269b182e0d8f2351f71");

	private static readonly AssetReference VO_Female_Nightborne_Story_11_Caye_012hp_InGame_VictoryPreExplosion_01_a = new AssetReference("VO_Female_Nightborne_Story_11_Caye_012hp_InGame_VictoryPreExplosion_01_a.prefab:8a36477fff2d484abec8b6292690baae");

	private static readonly AssetReference VO_Male_BloodElf_Story_11_HalusPrologue_InGame_HE_Custom_104_01_a = new AssetReference("VO_Male_BloodElf_Story_11_HalusPrologue_InGame_HE_Custom_104_01_a.prefab:772233c6aed742cea07cf5db9ebd3d2e");

	private static readonly AssetReference VO_Male_BloodElf_Story_11_HalusPrologue_InGame_HE_Custom_107_01_b = new AssetReference("VO_Male_BloodElf_Story_11_HalusPrologue_InGame_HE_Custom_107_01_b.prefab:5a211f1eacad4ed7bd228ccd3f62583e");

	private static readonly AssetReference VO_Male_Murloc_Story_11_FinleyPrologue_InGame_HE_Custom_103_01_a = new AssetReference("VO_Male_Murloc_Story_11_FinleyPrologue_InGame_HE_Custom_103_01_a.prefab:ad3a87d59498403ba19a59a3db6c53e2");

	private static readonly AssetReference VO_Male_Murloc_Story_11_FinleyPrologue_InGame_HE_Custom_106_01_b = new AssetReference("VO_Male_Murloc_Story_11_FinleyPrologue_InGame_HE_Custom_106_01_b.prefab:27b36695e50647ccb73ce8c0e54ee7ce");

	private static readonly AssetReference VO_Male_Nightborne_Story_11_Faelin_000hp_InGame_HE_Custom_01 = new AssetReference("VO_Male_Nightborne_Story_11_Faelin_000hp_InGame_HE_Custom_01.prefab:b33c705cdba64c0b95076a97e4ca4da4");

	private static readonly AssetReference VO_Male_Nightborne_Story_11_Faelin_000hp_InGame_HE_Custom_02 = new AssetReference("VO_Male_Nightborne_Story_11_Faelin_000hp_InGame_HE_Custom_02.prefab:03556828bf7040d7a9d117a8c3cf3321");

	private static readonly AssetReference VO_Male_Nightborne_Story_11_Faelin_000hp_InGame_HE_Custom_105_01_a = new AssetReference("VO_Male_Nightborne_Story_11_Faelin_000hp_InGame_HE_Custom_105_01_a.prefab:d1b04c8dd22b41f197b2151363770842");

	private static readonly AssetReference VO_Male_Nightborne_Story_11_Faelin_000hp_InGame_HE_Custom_106_01_a = new AssetReference("VO_Male_Nightborne_Story_11_Faelin_000hp_InGame_HE_Custom_106_01_a.prefab:8bc7535889284bda860c41305385e402");

	private static readonly AssetReference VO_Male_Nightborne_Story_11_Faelin_000hp_InGame_HE_Custom_107_01_a = new AssetReference("VO_Male_Nightborne_Story_11_Faelin_000hp_InGame_HE_Custom_107_01_a.prefab:482c756118cc4ec384800a3857593cd5");

	private static readonly AssetReference VO_Male_Nightborne_Story_11_Faelin_000hp_InGame_Introduction_02_a = new AssetReference("VO_Male_Nightborne_Story_11_Faelin_000hp_InGame_Introduction_02_a.prefab:0241d60662f845ad97c9d3deff04f785");

	private static readonly AssetReference VO_Male_Nightborne_Story_11_Faelin_000hp_InGame_Turn_01_01_b = new AssetReference("VO_Male_Nightborne_Story_11_Faelin_000hp_InGame_Turn_01_01_b.prefab:c7bd16c12c5b4cf28fdee8ceca2bae49");

	private static readonly AssetReference VO_Male_Nightborne_Story_11_Faelin_000hp_InGame_Turn_01_01_c = new AssetReference("VO_Male_Nightborne_Story_11_Faelin_000hp_InGame_Turn_01_01_c.prefab:f7de07f1c0a94501aa46414579776955");

	private static readonly AssetReference VO_Male_Nightborne_Story_11_Faelin_000hp_InGame_Turn_01_02_a = new AssetReference("VO_Male_Nightborne_Story_11_Faelin_000hp_InGame_Turn_01_02_a.prefab:135834c210574c92a4f3e0a44cfcec9f");

	private static readonly AssetReference Story_11_Leviathan_001hb_Death_Sound = new AssetReference("Story_11_Leviathan_001hb_Death_Sound.prefab:d5051de62f48f4b49aa855220792f2f4");

	private static readonly AssetReference Story_11_Leviathan_001hb_EmoteResponse_Sound = new AssetReference("Story_11_Leviathan_001hb_EmoteResponse_Sound.prefab:90b2aae30ca06f648a8033e394c1fa22");

	private static readonly AssetReference Story_11_Leviathan_001hb_Loss_Sound = new AssetReference("Story_11_Leviathan_001hb_Loss_Sound.prefab:c8702d2dff96aa1468e091e19a0f99cb");

	private static readonly AssetReference Story_11_Leviathan_001hb_Rev_Sound = new AssetReference("Story_11_Leviathan_001hb_Rev_Sound.prefab:63e2a91d4276f1a429b25b7ddcddd53d");

	private Notification m_popup;

	private Dictionary<int, string[]> m_popUpInfo = new Dictionary<int, string[]>
	{
		{
			228,
			new string[1] { "BOH_FAELIN_PROLOGUE_02" }
		},
		{
			229,
			new string[1] { "BOH_FAELIN_PROLOGUE_03" }
		},
		{
			230,
			new string[1] { "BOH_FAELIN_PROLOGUE_01" }
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

	public BoH_Faelin_Story_Prologue2()
	{
		m_gameOptions.AddBooleanOptions(s_booleanOptions);
	}

	public override void PreloadAssets()
	{
		base.PreloadAssets();
		List<string> m_SoundFiles = new List<string>
		{
			VO_Female_Gnome_Story_11_Ini_001hp_InGame_Introduction_01_01_b, VO_Female_Gnome_Story_11_Ini_001hp_InGame_Turn_01_01_e, VO_Female_Human_Story_11_GracePrologue_InGame_HE_Custom_102_01_a, VO_Female_Human_Story_11_GracePrologue_InGame_HE_Custom_105_01_b, VO_Female_Nightborne_Story_11_Caye_012hp_InGame_HE_Custom_01, VO_Female_Nightborne_Story_11_Caye_012hp_InGame_HE_Custom_02, VO_Female_Nightborne_Story_11_Caye_012hp_InGame_HE_Custom_101_01_a, VO_Female_Nightborne_Story_11_Caye_012hp_InGame_HE_Custom_102_01_b, VO_Female_Nightborne_Story_11_Caye_012hp_InGame_HE_Custom_103_01_b, VO_Female_Nightborne_Story_11_Caye_012hp_InGame_HE_Custom_104_01_b,
			VO_Female_Nightborne_Story_11_Caye_012hp_InGame_Turn_01_01_d, VO_Female_Nightborne_Story_11_Caye_012hp_InGame_Turn_01_01_f, VO_Female_Nightborne_Story_11_Caye_012hp_InGame_VictoryPreExplosion_01_a, VO_Male_BloodElf_Story_11_HalusPrologue_InGame_HE_Custom_104_01_a, VO_Male_BloodElf_Story_11_HalusPrologue_InGame_HE_Custom_107_01_b, VO_Male_Murloc_Story_11_FinleyPrologue_InGame_HE_Custom_103_01_a, VO_Male_Murloc_Story_11_FinleyPrologue_InGame_HE_Custom_106_01_b, VO_Male_Nightborne_Story_11_Faelin_000hp_InGame_HE_Custom_01, VO_Male_Nightborne_Story_11_Faelin_000hp_InGame_HE_Custom_02, VO_Male_Nightborne_Story_11_Faelin_000hp_InGame_HE_Custom_105_01_a,
			VO_Male_Nightborne_Story_11_Faelin_000hp_InGame_HE_Custom_106_01_a, VO_Male_Nightborne_Story_11_Faelin_000hp_InGame_HE_Custom_107_01_a, VO_Male_Nightborne_Story_11_Faelin_000hp_InGame_Introduction_02_a, VO_Male_Nightborne_Story_11_Faelin_000hp_InGame_Turn_01_01_b, VO_Male_Nightborne_Story_11_Faelin_000hp_InGame_Turn_01_01_c, VO_Male_Nightborne_Story_11_Faelin_000hp_InGame_Turn_01_02_a, Story_11_Leviathan_001hb_Death_Sound, Story_11_Leviathan_001hb_EmoteResponse_Sound, Story_11_Leviathan_001hb_Loss_Sound, Story_11_Leviathan_001hb_Rev_Sound
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
		m_OverrideMusicTrack = MusicPlaylistType.InGame_TSC_Leviathan;
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
			yield return MissionPlaySound(enemyActor, Story_11_Leviathan_001hb_EmoteResponse_Sound);
			break;
		case 520:
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlaySound(enemyActor, Story_11_Leviathan_001hb_Loss_Sound);
			GameState.Get().SetBusy(busy: false);
			break;
		case 108:
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVO(friendlyActor, VO_Male_Nightborne_Story_11_Faelin_000hp_InGame_Introduction_02_a);
			yield return MissionPlayVO(IniBrassRing, VO_Female_Gnome_Story_11_Ini_001hp_InGame_Introduction_01_01_b);
			yield return MissionPlayVO(friendlyActor, VO_Male_Nightborne_Story_11_Faelin_000hp_InGame_Turn_01_02_a);
			yield return MissionPlayVO(friendlyActor, VO_Male_Nightborne_Story_11_Faelin_000hp_InGame_Turn_01_01_b);
			yield return MissionPlayVO(friendlyActor, VO_Male_Nightborne_Story_11_Faelin_000hp_InGame_Turn_01_01_c);
			yield return MissionPlayVO(CayeBrassRing, VO_Female_Nightborne_Story_11_Caye_012hp_InGame_Turn_01_01_d);
			yield return MissionPlayVO(IniBrassRing, VO_Female_Gnome_Story_11_Ini_001hp_InGame_Turn_01_01_e);
			yield return MissionPlayVO(CayeBrassRing, VO_Female_Nightborne_Story_11_Caye_012hp_InGame_Turn_01_01_f);
			GameState.Get().SetBusy(busy: false);
			break;
		case 504:
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVO(CayeBrassRing, VO_Female_Nightborne_Story_11_Caye_012hp_InGame_VictoryPreExplosion_01_a);
			GameState.Get().SetBusy(busy: false);
			break;
		case 101:
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVO(CayeBrassRing, VO_Female_Nightborne_Story_11_Caye_012hp_InGame_HE_Custom_101_01_a);
			GameState.Get().SetBusy(busy: false);
			break;
		case 102:
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVO(BookBrassRing, VO_Female_Human_Story_11_GracePrologue_InGame_HE_Custom_102_01_a);
			yield return MissionPlayVO(CayeBrassRing, VO_Female_Nightborne_Story_11_Caye_012hp_InGame_HE_Custom_102_01_b);
			GameState.Get().SetBusy(busy: false);
			break;
		case 103:
			GameState.Get().SetBusy(busy: true);
			m_isIntroducingCharacter = true;
			yield return MissionPlayVO(BookBrassRing, VO_Male_Murloc_Story_11_FinleyPrologue_InGame_HE_Custom_103_01_a);
			yield return MissionPlayVO(CayeBrassRing, VO_Female_Nightborne_Story_11_Caye_012hp_InGame_HE_Custom_103_01_b);
			GameState.Get().SetBusy(busy: false);
			m_isIntroducingCharacter = false;
			break;
		case 104:
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVO(BookBrassRing, VO_Male_BloodElf_Story_11_HalusPrologue_InGame_HE_Custom_104_01_a);
			yield return MissionPlayVO(CayeBrassRing, VO_Female_Nightborne_Story_11_Caye_012hp_InGame_HE_Custom_104_01_b);
			GameState.Get().SetBusy(busy: false);
			break;
		case 105:
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVO(friendlyActor, VO_Male_Nightborne_Story_11_Faelin_000hp_InGame_HE_Custom_105_01_a);
			yield return MissionPlayVO(GraceBrassRing, VO_Female_Human_Story_11_GracePrologue_InGame_HE_Custom_105_01_b);
			GameState.Get().SetBusy(busy: false);
			break;
		case 106:
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVO(friendlyActor, VO_Male_Nightborne_Story_11_Faelin_000hp_InGame_HE_Custom_106_01_a);
			yield return MissionPlayVO(FinleyBrassRing, VO_Male_Murloc_Story_11_FinleyPrologue_InGame_HE_Custom_106_01_b);
			GameState.Get().SetBusy(busy: false);
			break;
		case 107:
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVO(friendlyActor, VO_Male_Nightborne_Story_11_Faelin_000hp_InGame_HE_Custom_107_01_a);
			yield return MissionPlayVO(HalusBrassRing, VO_Male_BloodElf_Story_11_HalusPrologue_InGame_HE_Custom_107_01_b);
			GameState.Get().SetBusy(busy: false);
			break;
		case 228:
			if (!(m_popup != null))
			{
				GameState.Get().SetBusy(busy: true);
				m_isIntroducingCharacter = true;
				m_popup = NotificationManager.Get().CreatePopupText(UserAttentionBlocker.NONE, popUpPos, TutorialEntity.GetTextScale() * popUpScale, GameStrings.Get(m_popUpInfo[missionEvent][0]), convertLegacyPosition: false, NotificationManager.PopupTextType.FANCY);
				yield return GameEntity.Coroutines.StartCoroutine(PlaySoundAndBlockSpeech(VO_Female_Human_Story_11_GracePrologue_InGame_HE_Custom_102_01_a, Notification.SpeechBubbleDirection.None, GetEnemyActorByCardId("Story_11_Mission1_Lorebook1"), 2.5f));
				NotificationManager.Get().DestroyNotification(m_popup, 0f);
				yield return MissionPlayVO(CayeBrassRing, VO_Female_Nightborne_Story_11_Caye_012hp_InGame_HE_Custom_102_01_b);
				m_popup = null;
				GameState.Get().SetBusy(busy: false);
				m_isIntroducingCharacter = false;
			}
			break;
		case 229:
			if (!(m_popup != null))
			{
				GameState.Get().SetBusy(busy: true);
				m_isIntroducingCharacter = true;
				m_popup = NotificationManager.Get().CreatePopupText(UserAttentionBlocker.NONE, popUpPos, TutorialEntity.GetTextScale() * popUpScale, GameStrings.Get(m_popUpInfo[missionEvent][0]), convertLegacyPosition: false, NotificationManager.PopupTextType.FANCY);
				yield return GameEntity.Coroutines.StartCoroutine(PlaySoundAndBlockSpeech(VO_Male_Murloc_Story_11_FinleyPrologue_InGame_HE_Custom_103_01_a, Notification.SpeechBubbleDirection.None, GetEnemyActorByCardId("Story_11_Mission1_Lorebook1"), 2.5f));
				NotificationManager.Get().DestroyNotification(m_popup, 0f);
				yield return MissionPlayVO(CayeBrassRing, VO_Female_Nightborne_Story_11_Caye_012hp_InGame_HE_Custom_103_01_b);
				m_popup = null;
				GameState.Get().SetBusy(busy: false);
				m_isIntroducingCharacter = false;
			}
			break;
		case 230:
			if (!(m_popup != null))
			{
				GameState.Get().SetBusy(busy: true);
				m_isIntroducingCharacter = true;
				m_popup = NotificationManager.Get().CreatePopupText(UserAttentionBlocker.NONE, popUpPos, TutorialEntity.GetTextScale() * popUpScale, GameStrings.Get(m_popUpInfo[missionEvent][0]), convertLegacyPosition: false, NotificationManager.PopupTextType.FANCY);
				yield return GameEntity.Coroutines.StartCoroutine(PlaySoundAndBlockSpeech(VO_Male_BloodElf_Story_11_HalusPrologue_InGame_HE_Custom_104_01_a, Notification.SpeechBubbleDirection.None, GetEnemyActorByCardId("Story_11_Mission1_Lorebook1"), 2.5f));
				NotificationManager.Get().DestroyNotification(m_popup, 0f);
				yield return MissionPlayVO(CayeBrassRing, VO_Female_Nightborne_Story_11_Caye_012hp_InGame_HE_Custom_104_01_b);
				m_popup = null;
				GameState.Get().SetBusy(busy: false);
				m_isIntroducingCharacter = false;
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
		if (turn == 1)
		{
			yield return MissionPlayVO(friendlyActor, VO_Male_Nightborne_Story_11_Faelin_000hp_InGame_Turn_01_02_a);
			yield return MissionPlayVO(friendlyActor, VO_Male_Nightborne_Story_11_Faelin_000hp_InGame_Turn_01_01_b);
			yield return MissionPlayVO(friendlyActor, VO_Male_Nightborne_Story_11_Faelin_000hp_InGame_Turn_01_01_c);
			yield return MissionPlayVO(CayeBrassRing, VO_Female_Nightborne_Story_11_Caye_012hp_InGame_Turn_01_01_d);
			yield return MissionPlayVO(IniBrassRing, VO_Female_Gnome_Story_11_Ini_001hp_InGame_Turn_01_01_e);
			yield return MissionPlayVO(CayeBrassRing, VO_Female_Nightborne_Story_11_Caye_012hp_InGame_Turn_01_01_f);
		}
	}

	public override bool NotifyOfBattlefieldCardClicked(Entity clickedEntity, bool wasInTargetMode)
	{
		if (!base.NotifyOfBattlefieldCardClicked(clickedEntity, wasInTargetMode))
		{
			return false;
		}
		if (!wasInTargetMode && !m_isIntroducingCharacter)
		{
			if (clickedEntity.GetCardId() == "Story_11_Prologue_Lorebook1")
			{
				if (m_popup == null)
				{
					Network.Get().SendClientScriptGameEvent(ClientScriptGameEventType.MISSION_EVENT, 228);
					HandleMissionEvent(228);
				}
				return false;
			}
			if (clickedEntity.GetCardId() == "Story_11_Prologue_Lorebook2")
			{
				if (m_popup == null)
				{
					Network.Get().SendClientScriptGameEvent(ClientScriptGameEventType.MISSION_EVENT, 229);
					HandleMissionEvent(229);
				}
				return false;
			}
			if (clickedEntity.GetCardId() == "Story_11_Prologue_Lorebook3")
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
