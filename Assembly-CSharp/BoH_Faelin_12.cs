using System.Collections;
using System.Collections.Generic;
using Blizzard.T5.Core;
using UnityEngine;

public class BoH_Faelin_12 : BoH_Faelin_Dungeon
{
	private static Map<GameEntityOption, bool> s_booleanOptions = InitBooleanOptions();

	private static readonly AssetReference VO_Story_11_Caye_012hp_Female_Nightborne_Story_Faelin_Mission12ExchangeB_01 = new AssetReference("VO_Story_11_Caye_012hp_Female_Nightborne_Story_Faelin_Mission12ExchangeB_01.prefab:7a41b9c0c8c7903409587d8bd3467e27");

	private static readonly AssetReference VO_Story_11_Dathril_012hb_Male_Naga_Story_Faelin_Mission12EmoteResponse_01 = new AssetReference("VO_Story_11_Dathril_012hb_Male_Naga_Story_Faelin_Mission12EmoteResponse_01.prefab:d4d3e2fb539929d47ae4789794319f87");

	private static readonly AssetReference VO_Story_11_Dathril_012hb_Male_Naga_Story_Faelin_Mission12ExchangeA_01 = new AssetReference("VO_Story_11_Dathril_012hb_Male_Naga_Story_Faelin_Mission12ExchangeA_01.prefab:17d4fa902a310ca4f95b4652ea6b4d3b");

	private static readonly AssetReference VO_Story_11_Dathril_012hb_Male_Naga_Story_Faelin_Mission12ExchangeA_02 = new AssetReference("VO_Story_11_Dathril_012hb_Male_Naga_Story_Faelin_Mission12ExchangeA_02.prefab:3d0253677a7736342a85e83ee7de3b7a");

	private static readonly AssetReference VO_Story_11_Dathril_012hb_Male_Naga_Story_Faelin_Mission12ExchangeD_01 = new AssetReference("VO_Story_11_Dathril_012hb_Male_Naga_Story_Faelin_Mission12ExchangeD_01.prefab:17eedc042fcbe774aaa5bf7c00129bf2");

	private static readonly AssetReference VO_Story_11_Dathril_012hb_Male_Naga_Story_Faelin_Mission12ExchangeE_01 = new AssetReference("VO_Story_11_Dathril_012hb_Male_Naga_Story_Faelin_Mission12ExchangeE_01.prefab:7c156e8e52e34aa4fbf59a4fc88d3c92");

	private static readonly AssetReference VO_Story_11_Dathril_012hb_Male_Naga_Story_Faelin_Mission12HeroPower_01 = new AssetReference("VO_Story_11_Dathril_012hb_Male_Naga_Story_Faelin_Mission12HeroPower_01.prefab:ab4a03213ecc1e248af33440418a30c7");

	private static readonly AssetReference VO_Story_11_Dathril_012hb_Male_Naga_Story_Faelin_Mission12HeroPower_02 = new AssetReference("VO_Story_11_Dathril_012hb_Male_Naga_Story_Faelin_Mission12HeroPower_02.prefab:fc869bffd0e35f946941310ee2ceccd8");

	private static readonly AssetReference VO_Story_11_Dathril_012hb_Male_Naga_Story_Faelin_Mission12HeroPower_03 = new AssetReference("VO_Story_11_Dathril_012hb_Male_Naga_Story_Faelin_Mission12HeroPower_03.prefab:5246609415ab6254589f66c985cd8594");

	private static readonly AssetReference VO_Story_11_Dathril_012hb_Male_Naga_Story_Faelin_Mission12Idle_01 = new AssetReference("VO_Story_11_Dathril_012hb_Male_Naga_Story_Faelin_Mission12Idle_01.prefab:5154157529fe7fe4a884461a9bb3a2c9");

	private static readonly AssetReference VO_Story_11_Dathril_012hb_Male_Naga_Story_Faelin_Mission12Idle_02 = new AssetReference("VO_Story_11_Dathril_012hb_Male_Naga_Story_Faelin_Mission12Idle_02.prefab:76fa6abdd3edb5d48a2c8d8432fb7743");

	private static readonly AssetReference VO_Story_11_Dathril_012hb_Male_Naga_Story_Faelin_Mission12Idle_03 = new AssetReference("VO_Story_11_Dathril_012hb_Male_Naga_Story_Faelin_Mission12Idle_03.prefab:41b1e0b50fc045a43bee13ee7582099b");

	private static readonly AssetReference VO_Story_11_Dathril_012hb_Male_Naga_Story_Faelin_Mission12Loss_01 = new AssetReference("VO_Story_11_Dathril_012hb_Male_Naga_Story_Faelin_Mission12Loss_01.prefab:4c6756ff63b400f4ca40eda08bb9d84e");

	private static readonly AssetReference VO_Story_11_Dathril_012hb_Male_Naga_Story_Faelin_Mission12Start_01 = new AssetReference("VO_Story_11_Dathril_012hb_Male_Naga_Story_Faelin_Mission12Start_01.prefab:f9114b5a98b019f478c82aaba2c236c3");

	private static readonly AssetReference VO_Story_11_Dathril_012hb_Male_Naga_Story_Faelin_Mission12Victory_01 = new AssetReference("VO_Story_11_Dathril_012hb_Male_Naga_Story_Faelin_Mission12Victory_01.prefab:cb7385556fcb7a44fb3000f7c57500f8");

	private static readonly AssetReference VO_Story_11_Dathril_012hb_Male_Naga_Story_Faelin_Mission12Victory_02 = new AssetReference("VO_Story_11_Dathril_012hb_Male_Naga_Story_Faelin_Mission12Victory_02.prefab:fabab8a3c9773ef4c962c43e191ca373");

	private static readonly AssetReference VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission12_Lorebook_1_01 = new AssetReference("VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission12_Lorebook_1_01.prefab:b78c6e4bf5f69a548a201f1779bcaf31");

	private static readonly AssetReference VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission12_Lorebook_1_02 = new AssetReference("VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission12_Lorebook_1_02.prefab:b530574ed7a471b4e9aff04052c77d7d");

	private static readonly AssetReference VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission12ExchangeA_01 = new AssetReference("VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission12ExchangeA_01.prefab:c3ee9be9b6ee6f246bdfe27a85fb1f79");

	private static readonly AssetReference VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission12ExchangeB_01 = new AssetReference("VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission12ExchangeB_01.prefab:b7801113e17e27c4498d40635834a779");

	private static readonly AssetReference VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission12ExchangeC_01 = new AssetReference("VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission12ExchangeC_01.prefab:8b2c4629f1dfc8a48b9d34c97a114054");

	private static readonly AssetReference VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission12ExchangeE_01 = new AssetReference("VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission12ExchangeE_01.prefab:be0a6d8e44e264242a8e6ac9ec7c64a2");

	private static readonly AssetReference VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission12Start_01 = new AssetReference("VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission12Start_01.prefab:a7b1c0961486d69468028cd67a9e2151");

	private static readonly AssetReference VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission12Start_02 = new AssetReference("VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission12Start_02.prefab:948e4d9618c5b9c48a7310f9b9512456");

	private static readonly AssetReference VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission12Victory_01 = new AssetReference("VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission12Victory_01.prefab:82ffd500ba186ed4296ba528aeebb215");

	private static readonly AssetReference VO_Story_11_Finley_009hp_Male_Murloc_Story_Faelin_Mission12ExchangeE_01 = new AssetReference("VO_Story_11_Finley_009hp_Male_Murloc_Story_Faelin_Mission12ExchangeE_01.prefab:b1401c33224f8d3499c21be934a7a22a");

	private static readonly AssetReference VO_Story_11_Grace_005hp_Female_Human_Story_Faelin_Mission12ExchangeE_01 = new AssetReference("VO_Story_11_Grace_005hp_Female_Human_Story_Faelin_Mission12ExchangeE_01.prefab:4ed4afe7c7696a74da65b02d9c91407e");

	private static readonly AssetReference VO_Story_11_Halus_010hp_Male_BloodElf_Story_Faelin_Mission11_Lorebook_3_01 = new AssetReference("VO_Story_11_Halus_010hp_Male_BloodElf_Story_Faelin_Mission11_Lorebook_3_01.prefab:adea2aab4ed859f47b426e1e00047ee3");

	private static readonly AssetReference VO_Story_11_Halus_010hp_Male_BloodElf_Story_Faelin_Mission12ExchangeF_01 = new AssetReference("VO_Story_11_Halus_010hp_Male_BloodElf_Story_Faelin_Mission12ExchangeF_01.prefab:a179d6bebeace1e4e9f827e7aa762633");

	private static readonly AssetReference VO_Story_11_Handmaiden_015hb_Female_NightElf_Story_Faelin_Mission12_Lorebook_1_01 = new AssetReference("VO_Story_11_Handmaiden_015hb_Female_NightElf_Story_Faelin_Mission12_Lorebook_1_01.prefab:e29fa5c01cd93ef4c8c3af6a5fe926c5");

	private static readonly AssetReference VO_Story_11_Dathril_011hp_Male_NightElf_Story_Faelin_Mission12_Lorebook_1_01 = new AssetReference("VO_Story_11_Dathril_011hp_Male_NightElf_Story_Faelin_Mission12_Lorebook_1_01.prefab:ff083916651d9fb42b06e439bea8b694");

	private static readonly AssetReference VO_Story_11_Dathril_011hp_Male_NightElf_Story_Faelin_Mission12_Lorebook_1_02 = new AssetReference("VO_Story_11_Dathril_011hp_Male_NightElf_Story_Faelin_Mission12_Lorebook_1_02.prefab:43bdb9aa854f8ef438667351b015c3e4");

	private Notification m_popup;

	private Dictionary<int, string[]> m_popUpInfo = new Dictionary<int, string[]>
	{
		{
			228,
			new string[1] { "BOH_FAELIN_MISSION12_LOREBOOK_01" }
		},
		{
			229,
			new string[1] { "BOH_FAELIN_MISSION12_LOREBOOK_02" }
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

	public static readonly AssetReference FaelinBrassRing = new AssetReference("SKN23-002_H_BOH_Faelin_Sad_BrassRing_Quote.prefab:a2375dc715c6d284a90383ff376e1b03");

	private new List<string> m_BossIdleLines = new List<string> { VO_Story_11_Dathril_012hb_Male_Naga_Story_Faelin_Mission12Idle_01, VO_Story_11_Dathril_012hb_Male_Naga_Story_Faelin_Mission12Idle_02, VO_Story_11_Dathril_012hb_Male_Naga_Story_Faelin_Mission12Idle_03 };

	private List<string> m_BossUsesHeroPowerLines = new List<string> { VO_Story_11_Dathril_012hb_Male_Naga_Story_Faelin_Mission12HeroPower_01, VO_Story_11_Dathril_012hb_Male_Naga_Story_Faelin_Mission12HeroPower_02, VO_Story_11_Dathril_012hb_Male_Naga_Story_Faelin_Mission12HeroPower_03 };

	private HashSet<string> m_playedLines = new HashSet<string>();

	private static Map<GameEntityOption, bool> InitBooleanOptions()
	{
		return new Map<GameEntityOption, bool> { 
		{
			GameEntityOption.DO_OPENING_TAUNTS,
			false
		} };
	}

	public BoH_Faelin_12()
	{
		m_gameOptions.AddBooleanOptions(s_booleanOptions);
	}

	public override void PreloadAssets()
	{
		base.PreloadAssets();
		List<string> m_SoundFiles = new List<string>
		{
			VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission12Start_01, VO_Story_11_Dathril_012hb_Male_Naga_Story_Faelin_Mission12Start_01, VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission12Start_02, VO_Story_11_Dathril_012hb_Male_Naga_Story_Faelin_Mission12ExchangeA_01, VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission12ExchangeA_01, VO_Story_11_Dathril_012hb_Male_Naga_Story_Faelin_Mission12ExchangeA_02, VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission12ExchangeB_01, VO_Story_11_Caye_012hp_Female_Nightborne_Story_Faelin_Mission12ExchangeB_01, VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission12ExchangeC_01, VO_Story_11_Dathril_012hb_Male_Naga_Story_Faelin_Mission12ExchangeE_01,
			VO_Story_11_Dathril_012hb_Male_Naga_Story_Faelin_Mission12ExchangeD_01, VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission12ExchangeE_01, VO_Story_11_Grace_005hp_Female_Human_Story_Faelin_Mission12ExchangeE_01, VO_Story_11_Finley_009hp_Male_Murloc_Story_Faelin_Mission12ExchangeE_01, VO_Story_11_Halus_010hp_Male_BloodElf_Story_Faelin_Mission12ExchangeF_01, VO_Story_11_Dathril_012hb_Male_Naga_Story_Faelin_Mission12Victory_01, VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission12Victory_01, VO_Story_11_Dathril_012hb_Male_Naga_Story_Faelin_Mission12Victory_02, VO_Story_11_Dathril_012hb_Male_Naga_Story_Faelin_Mission12EmoteResponse_01, VO_Story_11_Dathril_012hb_Male_Naga_Story_Faelin_Mission12HeroPower_01,
			VO_Story_11_Dathril_012hb_Male_Naga_Story_Faelin_Mission12HeroPower_02, VO_Story_11_Dathril_012hb_Male_Naga_Story_Faelin_Mission12HeroPower_03, VO_Story_11_Dathril_012hb_Male_Naga_Story_Faelin_Mission12Loss_01, VO_Story_11_Dathril_012hb_Male_Naga_Story_Faelin_Mission12Idle_01, VO_Story_11_Dathril_012hb_Male_Naga_Story_Faelin_Mission12Idle_02, VO_Story_11_Dathril_012hb_Male_Naga_Story_Faelin_Mission12Idle_03, VO_Story_11_Dathril_011hp_Male_NightElf_Story_Faelin_Mission12_Lorebook_1_01, VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission12_Lorebook_1_01, VO_Story_11_Dathril_011hp_Male_NightElf_Story_Faelin_Mission12_Lorebook_1_02, VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission12_Lorebook_1_02,
			VO_Story_11_Handmaiden_015hb_Female_NightElf_Story_Faelin_Mission12_Lorebook_1_01
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

	public override List<string> GetBossHeroPowerRandomLines()
	{
		return m_BossUsesHeroPowerLines;
	}

	public override void OnCreateGame()
	{
		base.OnCreateGame();
		m_OverrideMusicTrack = MusicPlaylistType.InGame_TSC_Nazjatar;
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
		popUpPos = new Vector3(0f, 0f, -40f);
		switch (missionEvent)
		{
		case 515:
			yield return MissionPlayVO(enemyActor, VO_Story_11_Dathril_012hb_Male_Naga_Story_Faelin_Mission12EmoteResponse_01);
			break;
		case 514:
			yield return MissionPlayVO(friendlyActor, VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission12Start_01);
			break;
		case 507:
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVO(enemyActor, VO_Story_11_Dathril_012hb_Male_Naga_Story_Faelin_Mission12Loss_01);
			GameState.Get().SetBusy(busy: false);
			break;
		case 504:
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVO(FaelinBrassRing, VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission12Victory_01);
			yield return MissionPlayVO(enemyActor, VO_Story_11_Dathril_012hb_Male_Naga_Story_Faelin_Mission12Victory_02);
			GameState.Get().SetBusy(busy: false);
			break;
		case 101:
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVO(enemyActor, VO_Story_11_Dathril_012hb_Male_Naga_Story_Faelin_Mission12Start_01);
			yield return MissionPlayVO(friendlyActor, VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission12Start_02);
			GameState.Get().SetBusy(busy: false);
			break;
		case 102:
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVO(enemyActor, VO_Story_11_Dathril_012hb_Male_Naga_Story_Faelin_Mission12ExchangeA_01);
			GameState.Get().SetBusy(busy: false);
			break;
		case 103:
			yield return MissionPlayVO(friendlyActor, VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission12ExchangeA_01);
			yield return MissionPlayVO(enemyActor, VO_Story_11_Dathril_012hb_Male_Naga_Story_Faelin_Mission12ExchangeA_02);
			break;
		case 104:
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVO(friendlyActor, VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission12ExchangeB_01);
			yield return MissionPlayVO(CayeBrassRing, VO_Story_11_Caye_012hp_Female_Nightborne_Story_Faelin_Mission12ExchangeB_01);
			GameState.Get().SetBusy(busy: false);
			break;
		case 105:
			yield return MissionPlayVO(FaelinBrassRing, VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission12ExchangeE_01);
			yield return MissionPlayVO(HalusBrassRing, VO_Story_11_Halus_010hp_Male_BloodElf_Story_Faelin_Mission12ExchangeF_01);
			break;
		case 106:
			yield return MissionPlayVO(FaelinBrassRing, VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission12ExchangeE_01);
			yield return MissionPlayVO(GraceBrassRing, VO_Story_11_Grace_005hp_Female_Human_Story_Faelin_Mission12ExchangeE_01);
			break;
		case 107:
			yield return MissionPlayVO(FaelinBrassRing, VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission12ExchangeE_01);
			yield return MissionPlayVO(FinleyBrassRing, VO_Story_11_Finley_009hp_Male_Murloc_Story_Faelin_Mission12ExchangeE_01);
			break;
		case 110:
			yield return MissionPlayVO(enemyActor, VO_Story_11_Dathril_012hb_Male_Naga_Story_Faelin_Mission12Victory_01);
			break;
		case 228:
			if (!(m_popup != null))
			{
				GameState.Get().SetBusy(busy: true);
				m_popup = NotificationManager.Get().CreatePopupText(UserAttentionBlocker.NONE, popUpPos, TutorialEntity.GetTextScale() * popUpScale, GameStrings.Get(m_popUpInfo[missionEvent][0]), convertLegacyPosition: false, NotificationManager.PopupTextType.FANCY);
				yield return GameEntity.Coroutines.StartCoroutine(PlaySoundAndBlockSpeech(VO_Story_11_Dathril_011hp_Male_NightElf_Story_Faelin_Mission12_Lorebook_1_01, Notification.SpeechBubbleDirection.None, GetEnemyActorByCardId("Story_11_Mission1_Lorebook1"), 2.5f));
				yield return GameEntity.Coroutines.StartCoroutine(PlaySoundAndBlockSpeech(VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission12_Lorebook_1_01, Notification.SpeechBubbleDirection.None, GetEnemyActorByCardId("Story_11_Mission1_Lorebook1"), 2.5f));
				yield return GameEntity.Coroutines.StartCoroutine(PlaySoundAndBlockSpeech(VO_Story_11_Dathril_011hp_Male_NightElf_Story_Faelin_Mission12_Lorebook_1_02, Notification.SpeechBubbleDirection.None, GetEnemyActorByCardId("Story_11_Mission1_Lorebook1"), 2.5f));
				yield return GameEntity.Coroutines.StartCoroutine(PlaySoundAndBlockSpeech(VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission12_Lorebook_1_02, Notification.SpeechBubbleDirection.None, GetEnemyActorByCardId("Story_11_Mission1_Lorebook1"), 2.5f));
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
				yield return GameEntity.Coroutines.StartCoroutine(PlaySoundAndBlockSpeech(VO_Story_11_Handmaiden_015hb_Female_NightElf_Story_Faelin_Mission12_Lorebook_1_01, Notification.SpeechBubbleDirection.None, GetEnemyActorByCardId("Story_11_Mission1_Lorebook1"), 2.5f));
				NotificationManager.Get().DestroyNotification(m_popup, 0f);
				m_popup = null;
				GameState.Get().SetBusy(busy: false);
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
		Actor enemyActor = GameState.Get().GetOpposingSidePlayer().GetHero()
			.GetCard()
			.GetActor();
		GameState.Get().GetFriendlySidePlayer().GetHero()
			.GetCard()
			.GetActor();
		switch (turn)
		{
		case 7:
			yield return MissionPlayVO(FaelinBrassRing, VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission12ExchangeC_01);
			yield return MissionPlayVO(enemyActor, VO_Story_11_Dathril_012hb_Male_Naga_Story_Faelin_Mission12ExchangeE_01);
			break;
		case 13:
			yield return MissionPlayVO(enemyActor, VO_Story_11_Dathril_012hb_Male_Naga_Story_Faelin_Mission12ExchangeD_01);
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
			if (clickedEntity.GetCardId() == "Story_11_Mission12_Lorebook1")
			{
				if (m_popup == null)
				{
					Network.Get().SendClientScriptGameEvent(ClientScriptGameEventType.MISSION_EVENT, 228);
					HandleMissionEvent(228);
				}
				return false;
			}
			if (clickedEntity.GetCardId() == "Story_11_Mission12_Lorebook2")
			{
				if (m_popup == null)
				{
					Network.Get().SendClientScriptGameEvent(ClientScriptGameEventType.MISSION_EVENT, 229);
					HandleMissionEvent(229);
				}
				return false;
			}
		}
		return true;
	}
}
