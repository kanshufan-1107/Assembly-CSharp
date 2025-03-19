using System.Collections;
using System.Collections.Generic;
using Blizzard.T5.Core;
using UnityEngine;

public class BoH_Faelin_15 : BoH_Faelin_Dungeon
{
	private static Map<GameEntityOption, bool> s_booleanOptions = InitBooleanOptions();

	private static readonly AssetReference VO_Story_11_Azshara_016hb_Female_Naga_Story_Faelin_Mission15_Lorebook_1_01 = new AssetReference("VO_Story_11_Azshara_016hb_Female_Naga_Story_Faelin_Mission15_Lorebook_1_01.prefab:e0b70d3c2b027f04c9589ec7618ea206");

	private static readonly AssetReference VO_Story_11_Dathril_012hb_Male_Naga_Story_Faelin_Mission15ExchangeC_01 = new AssetReference("VO_Story_11_Dathril_012hb_Male_Naga_Story_Faelin_Mission15ExchangeC_01.prefab:237f964a056d5a84bb653ece833865be");

	private static readonly AssetReference VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission15ExchangeB_01 = new AssetReference("VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission15ExchangeB_01.prefab:3580aa0e504a84e45b79149f77991896");

	private static readonly AssetReference VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission15ExchangeC_01 = new AssetReference("VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission15ExchangeC_01.prefab:978e6fb58326aaa4597bd2fefff56f9e");

	private static readonly AssetReference VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission15ExchangeD_01 = new AssetReference("VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission15ExchangeD_01.prefab:bf674ebdb6ef0d043867b46350b941ce");

	private static readonly AssetReference VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission15Start_01 = new AssetReference("VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission15Start_01.prefab:506dedceb62b89346a3c5676bdff18c3");

	private static readonly AssetReference VO_Story_11_Finley_009hp_Male_Murloc_Story_Faelin_Mission15ExchangeB_01 = new AssetReference("VO_Story_11_Finley_009hp_Male_Murloc_Story_Faelin_Mission15ExchangeB_01.prefab:1a36729fa78d621469be15083de4ace1");

	private static readonly AssetReference VO_Story_11_Finley_009hp_Male_Murloc_Story_Faelin_Mission15Victory_01 = new AssetReference("VO_Story_11_Finley_009hp_Male_Murloc_Story_Faelin_Mission15Victory_01.prefab:00bcc92778eb43e41a473cc13e99d17d");

	private static readonly AssetReference VO_Story_11_Grace_005hp_Female_Human_Story_Faelin_Mission15ExchangeA_01 = new AssetReference("VO_Story_11_Grace_005hp_Female_Human_Story_Faelin_Mission15ExchangeA_01.prefab:0e09a28370631c447b9e5a61dff0ebad");

	private static readonly AssetReference VO_Story_11_Grace_005hp_Female_Human_Story_Faelin_Mission15ExchangeD_01 = new AssetReference("VO_Story_11_Grace_005hp_Female_Human_Story_Faelin_Mission15ExchangeD_01.prefab:91576cbb8a707ff41aeffd177eba4fd6");

	private static readonly AssetReference VO_Story_11_Grace_005hp_Female_Human_Story_Faelin_Mission15Victory_01 = new AssetReference("VO_Story_11_Grace_005hp_Female_Human_Story_Faelin_Mission15Victory_01.prefab:4bdeb2a20ed078e43bbe424b4fdb26c8");

	private static readonly AssetReference VO_Story_11_Halus_010hp_Male_BloodElf_Story_Faelin_Mission15ExchangeA_01 = new AssetReference("VO_Story_11_Halus_010hp_Male_BloodElf_Story_Faelin_Mission15ExchangeA_01.prefab:a572472a9e7f14c4e85d1c29288cca51");

	private static readonly AssetReference VO_Story_11_Halus_010hp_Male_BloodElf_Story_Faelin_Mission15ExchangeE_01 = new AssetReference("VO_Story_11_Halus_010hp_Male_BloodElf_Story_Faelin_Mission15ExchangeE_01.prefab:049de12d75b60b943a8f03873497c343");

	private static readonly AssetReference VO_Story_11_Halus_010hp_Male_BloodElf_Story_Faelin_Mission15Victory_01 = new AssetReference("VO_Story_11_Halus_010hp_Male_BloodElf_Story_Faelin_Mission15Victory_01.prefab:d500c6b7ac11c1242bb44b9c6273c290");

	private static readonly AssetReference VO_Story_11_Handmaiden_015hb_Female_Naga_Story_Faelin_Mission15_Lorebook_2_01 = new AssetReference("VO_Story_11_Handmaiden_015hb_Female_Naga_Story_Faelin_Mission15_Lorebook_2_01.prefab:ebff5243dae50244995f25ae81dafc3d");

	private static readonly AssetReference VO_Story_11_Handmaiden_015hb_Female_Naga_Story_Faelin_Mission15EmoteResponse_01 = new AssetReference("VO_Story_11_Handmaiden_015hb_Female_Naga_Story_Faelin_Mission15EmoteResponse_01.prefab:90bf5ffe66e47f64ea94cce4848e259b");

	private static readonly AssetReference VO_Story_11_Handmaiden_015hb_Female_Naga_Story_Faelin_Mission15ExchangeA_01 = new AssetReference("VO_Story_11_Handmaiden_015hb_Female_Naga_Story_Faelin_Mission15ExchangeA_01.prefab:ddac542ee18f00348b3bea80608a0d86");

	private static readonly AssetReference VO_Story_11_Handmaiden_015hb_Female_Naga_Story_Faelin_Mission15ExchangeD_01 = new AssetReference("VO_Story_11_Handmaiden_015hb_Female_Naga_Story_Faelin_Mission15ExchangeD_01.prefab:30b26b18299b5ac4caad1cf5d33f01e3");

	private static readonly AssetReference VO_Story_11_Handmaiden_015hb_Female_Naga_Story_Faelin_Mission15ExchangeE_01 = new AssetReference("VO_Story_11_Handmaiden_015hb_Female_Naga_Story_Faelin_Mission15ExchangeE_01.prefab:6037b57a1dd4fff4bb9a79ca4f8ffdc2");

	private static readonly AssetReference VO_Story_11_Handmaiden_015hb_Female_Naga_Story_Faelin_Mission15ExchangeE_02 = new AssetReference("VO_Story_11_Handmaiden_015hb_Female_Naga_Story_Faelin_Mission15ExchangeE_02.prefab:6d2e506f6a855c141b610458f1d84a10");

	private static readonly AssetReference VO_Story_11_Handmaiden_015hb_Female_Naga_Story_Faelin_Mission15HeroPower_01 = new AssetReference("VO_Story_11_Handmaiden_015hb_Female_Naga_Story_Faelin_Mission15HeroPower_01.prefab:54af6678eeaff2b43a506839254bb99d");

	private static readonly AssetReference VO_Story_11_Handmaiden_015hb_Female_Naga_Story_Faelin_Mission15HeroPower_02 = new AssetReference("VO_Story_11_Handmaiden_015hb_Female_Naga_Story_Faelin_Mission15HeroPower_02.prefab:bb36b0aab2852d54bbed0dd660ffcb79");

	private static readonly AssetReference VO_Story_11_Handmaiden_015hb_Female_Naga_Story_Faelin_Mission15HeroPower_03 = new AssetReference("VO_Story_11_Handmaiden_015hb_Female_Naga_Story_Faelin_Mission15HeroPower_03.prefab:5c5a1ae5a9fc7a142b2f2f78949f4ea4");

	private static readonly AssetReference VO_Story_11_Handmaiden_015hb_Female_Naga_Story_Faelin_Mission15Idle_01 = new AssetReference("VO_Story_11_Handmaiden_015hb_Female_Naga_Story_Faelin_Mission15Idle_01.prefab:794bcb89fcbb63249ac744dbd33c959d");

	private static readonly AssetReference VO_Story_11_Handmaiden_015hb_Female_Naga_Story_Faelin_Mission15Idle_02 = new AssetReference("VO_Story_11_Handmaiden_015hb_Female_Naga_Story_Faelin_Mission15Idle_02.prefab:b3c6b7451dbdd2b4bb79eb3dd369e68d");

	private static readonly AssetReference VO_Story_11_Handmaiden_015hb_Female_Naga_Story_Faelin_Mission15Idle_03 = new AssetReference("VO_Story_11_Handmaiden_015hb_Female_Naga_Story_Faelin_Mission15Idle_03.prefab:d57685bf17413224dbc69a92707470ba");

	private static readonly AssetReference VO_Story_11_Handmaiden_015hb_Female_Naga_Story_Faelin_Mission15Loss_01 = new AssetReference("VO_Story_11_Handmaiden_015hb_Female_Naga_Story_Faelin_Mission15Loss_01.prefab:980cdfff7b492934facaa6f34210fd11");

	private static readonly AssetReference VO_Story_11_Handmaiden_015hb_Female_Naga_Story_Faelin_Mission15Start_01 = new AssetReference("VO_Story_11_Handmaiden_015hb_Female_Naga_Story_Faelin_Mission15Start_01.prefab:c80f96137a0edab4b9087a143589d99c");

	private static readonly AssetReference VO_Story_11_Handmaiden_015hb_Female_Naga_Story_Faelin_Mission15Start_02 = new AssetReference("VO_Story_11_Handmaiden_015hb_Female_Naga_Story_Faelin_Mission15Start_02.prefab:56319f8b5fe0a804eb43da3dc1ce93f0");

	private static readonly AssetReference VO_Story_11_Handmaiden_015hb_Female_Naga_Story_Faelin_Mission15Victory_01 = new AssetReference("VO_Story_11_Handmaiden_015hb_Female_Naga_Story_Faelin_Mission15Victory_01.prefab:ac58f9eb8ad28d042a2393ea77473e69");

	private Notification m_popup;

	private Dictionary<int, string[]> m_popUpInfo = new Dictionary<int, string[]>
	{
		{
			228,
			new string[1] { "BOH_FAELIN_MISSION15_LOREBOOK_01" }
		},
		{
			229,
			new string[1] { "BOH_FAELIN_MISSION15_LOREBOOK_02" }
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

	public static readonly AssetReference FaelinBrassRing = new AssetReference("SKN23-002_H_FAELIN_BrassRing_Quote.prefab:9984152d900140547aa7d721f3e49428");

	private new List<string> m_BossIdleLines = new List<string> { VO_Story_11_Handmaiden_015hb_Female_Naga_Story_Faelin_Mission15Idle_01, VO_Story_11_Handmaiden_015hb_Female_Naga_Story_Faelin_Mission15Idle_02, VO_Story_11_Handmaiden_015hb_Female_Naga_Story_Faelin_Mission15Idle_03 };

	private List<string> m_BossUsesHeroPowerLines = new List<string> { VO_Story_11_Handmaiden_015hb_Female_Naga_Story_Faelin_Mission15HeroPower_01, VO_Story_11_Handmaiden_015hb_Female_Naga_Story_Faelin_Mission15HeroPower_02, VO_Story_11_Handmaiden_015hb_Female_Naga_Story_Faelin_Mission15HeroPower_03 };

	private HashSet<string> m_playedLines = new HashSet<string>();

	private static Map<GameEntityOption, bool> InitBooleanOptions()
	{
		return new Map<GameEntityOption, bool> { 
		{
			GameEntityOption.DO_OPENING_TAUNTS,
			false
		} };
	}

	public BoH_Faelin_15()
	{
		m_gameOptions.AddBooleanOptions(s_booleanOptions);
	}

	public override void PreloadAssets()
	{
		base.PreloadAssets();
		List<string> m_SoundFiles = new List<string>
		{
			VO_Story_11_Handmaiden_015hb_Female_Naga_Story_Faelin_Mission15Start_01, VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission15Start_01, VO_Story_11_Handmaiden_015hb_Female_Naga_Story_Faelin_Mission15Start_02, VO_Story_11_Handmaiden_015hb_Female_Naga_Story_Faelin_Mission15ExchangeA_01, VO_Story_11_Grace_005hp_Female_Human_Story_Faelin_Mission15ExchangeA_01, VO_Story_11_Halus_010hp_Male_BloodElf_Story_Faelin_Mission15ExchangeA_01, VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission15ExchangeB_01, VO_Story_11_Finley_009hp_Male_Murloc_Story_Faelin_Mission15ExchangeB_01, VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission15ExchangeC_01, VO_Story_11_Dathril_012hb_Male_Naga_Story_Faelin_Mission15ExchangeC_01,
			VO_Story_11_Handmaiden_015hb_Female_Naga_Story_Faelin_Mission15ExchangeD_01, VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission15ExchangeD_01, VO_Story_11_Grace_005hp_Female_Human_Story_Faelin_Mission15ExchangeD_01, VO_Story_11_Handmaiden_015hb_Female_Naga_Story_Faelin_Mission15ExchangeE_01, VO_Story_11_Handmaiden_015hb_Female_Naga_Story_Faelin_Mission15ExchangeE_02, VO_Story_11_Halus_010hp_Male_BloodElf_Story_Faelin_Mission15ExchangeE_01, VO_Story_11_Handmaiden_015hb_Female_Naga_Story_Faelin_Mission15Victory_01, VO_Story_11_Grace_005hp_Female_Human_Story_Faelin_Mission15Victory_01, VO_Story_11_Finley_009hp_Male_Murloc_Story_Faelin_Mission15Victory_01, VO_Story_11_Halus_010hp_Male_BloodElf_Story_Faelin_Mission15Victory_01,
			VO_Story_11_Handmaiden_015hb_Female_Naga_Story_Faelin_Mission15EmoteResponse_01, VO_Story_11_Handmaiden_015hb_Female_Naga_Story_Faelin_Mission15HeroPower_01, VO_Story_11_Handmaiden_015hb_Female_Naga_Story_Faelin_Mission15HeroPower_02, VO_Story_11_Handmaiden_015hb_Female_Naga_Story_Faelin_Mission15HeroPower_03, VO_Story_11_Handmaiden_015hb_Female_Naga_Story_Faelin_Mission15Loss_01, VO_Story_11_Handmaiden_015hb_Female_Naga_Story_Faelin_Mission15Idle_01, VO_Story_11_Handmaiden_015hb_Female_Naga_Story_Faelin_Mission15Idle_02, VO_Story_11_Handmaiden_015hb_Female_Naga_Story_Faelin_Mission15Idle_03, VO_Story_11_Azshara_016hb_Female_Naga_Story_Faelin_Mission15_Lorebook_1_01, VO_Story_11_Handmaiden_015hb_Female_Naga_Story_Faelin_Mission15_Lorebook_2_01
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
		popUpPos = new Vector3(0f, 0f, -40f);
		switch (missionEvent)
		{
		case 515:
			yield return MissionPlayVO(enemyActor, VO_Story_11_Handmaiden_015hb_Female_Naga_Story_Faelin_Mission15EmoteResponse_01);
			break;
		case 514:
			yield return MissionPlayVO(enemyActor, VO_Story_11_Handmaiden_015hb_Female_Naga_Story_Faelin_Mission15Start_01);
			yield return MissionPlayVO(friendlyActor, VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission15Start_01);
			yield return MissionPlayVO(enemyActor, VO_Story_11_Handmaiden_015hb_Female_Naga_Story_Faelin_Mission15Start_02);
			break;
		case 507:
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVO(enemyActor, VO_Story_11_Handmaiden_015hb_Female_Naga_Story_Faelin_Mission15Loss_01);
			GameState.Get().SetBusy(busy: false);
			break;
		case 504:
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVO(enemyActor, VO_Story_11_Handmaiden_015hb_Female_Naga_Story_Faelin_Mission15Victory_01);
			yield return MissionPlayVO(HalusBrassRing, VO_Story_11_Halus_010hp_Male_BloodElf_Story_Faelin_Mission15Victory_01);
			GameState.Get().SetBusy(busy: false);
			break;
		case 101:
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVO(enemyActor, VO_Story_11_Handmaiden_015hb_Female_Naga_Story_Faelin_Mission15Victory_01);
			yield return MissionPlayVO(GraceBrassRing, VO_Story_11_Grace_005hp_Female_Human_Story_Faelin_Mission15Victory_01);
			GameState.Get().SetBusy(busy: false);
			break;
		case 102:
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVO(enemyActor, VO_Story_11_Handmaiden_015hb_Female_Naga_Story_Faelin_Mission15Victory_01);
			yield return MissionPlayVO(FinleyBrassRing, VO_Story_11_Finley_009hp_Male_Murloc_Story_Faelin_Mission15Victory_01);
			GameState.Get().SetBusy(busy: false);
			break;
		case 103:
			yield return MissionPlayVO(enemyActor, VO_Story_11_Handmaiden_015hb_Female_Naga_Story_Faelin_Mission15ExchangeA_01);
			yield return MissionPlayVO(HalusBrassRing, VO_Story_11_Halus_010hp_Male_BloodElf_Story_Faelin_Mission15ExchangeA_01);
			break;
		case 104:
			yield return MissionPlayVO(enemyActor, VO_Story_11_Handmaiden_015hb_Female_Naga_Story_Faelin_Mission15ExchangeA_01);
			yield return MissionPlayVO(GraceBrassRing, VO_Story_11_Grace_005hp_Female_Human_Story_Faelin_Mission15ExchangeA_01);
			break;
		case 105:
			yield return MissionPlayVO(enemyActor, VO_Story_11_Handmaiden_015hb_Female_Naga_Story_Faelin_Mission15ExchangeA_01);
			break;
		case 106:
			yield return MissionPlayVO(friendlyActor, VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission15ExchangeB_01);
			break;
		case 107:
			yield return MissionPlayVO(friendlyActor, VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission15ExchangeB_01);
			yield return MissionPlayVO(FinleyBrassRing, VO_Story_11_Finley_009hp_Male_Murloc_Story_Faelin_Mission15ExchangeB_01);
			break;
		case 108:
			yield return MissionPlayVO(enemyActor, VO_Story_11_Handmaiden_015hb_Female_Naga_Story_Faelin_Mission15ExchangeD_01);
			yield return MissionPlayVO(friendlyActor, VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission15ExchangeD_01);
			break;
		case 109:
			yield return MissionPlayVO(enemyActor, VO_Story_11_Handmaiden_015hb_Female_Naga_Story_Faelin_Mission15ExchangeD_01);
			yield return MissionPlayVO(friendlyActor, VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission15ExchangeD_01);
			yield return MissionPlayVO(GraceBrassRing, VO_Story_11_Grace_005hp_Female_Human_Story_Faelin_Mission15ExchangeD_01);
			break;
		case 110:
			yield return MissionPlayVO(enemyActor, VO_Story_11_Handmaiden_015hb_Female_Naga_Story_Faelin_Mission15ExchangeE_01);
			yield return MissionPlayVO(enemyActor, VO_Story_11_Handmaiden_015hb_Female_Naga_Story_Faelin_Mission15ExchangeE_02);
			yield return MissionPlayVO(HalusBrassRing, VO_Story_11_Halus_010hp_Male_BloodElf_Story_Faelin_Mission15ExchangeE_01);
			break;
		case 111:
			yield return MissionPlayVO(enemyActor, VO_Story_11_Handmaiden_015hb_Female_Naga_Story_Faelin_Mission15ExchangeE_01);
			yield return MissionPlayVO(enemyActor, VO_Story_11_Handmaiden_015hb_Female_Naga_Story_Faelin_Mission15ExchangeE_02);
			break;
		case 112:
			yield return MissionPlayVO(BookBrassRing, VO_Story_11_Azshara_016hb_Female_Naga_Story_Faelin_Mission15_Lorebook_1_01);
			break;
		case 113:
			yield return MissionPlayVO(BookBrassRing, VO_Story_11_Handmaiden_015hb_Female_Naga_Story_Faelin_Mission15_Lorebook_2_01);
			break;
		case 228:
			if (!(m_popup != null))
			{
				GameState.Get().SetBusy(busy: true);
				m_popup = NotificationManager.Get().CreatePopupText(UserAttentionBlocker.NONE, popUpPos, TutorialEntity.GetTextScale() * popUpScale, GameStrings.Get(m_popUpInfo[missionEvent][0]), convertLegacyPosition: false, NotificationManager.PopupTextType.FANCY);
				yield return GameEntity.Coroutines.StartCoroutine(PlaySoundAndBlockSpeech(VO_Story_11_Azshara_016hb_Female_Naga_Story_Faelin_Mission15_Lorebook_1_01, Notification.SpeechBubbleDirection.None, GetEnemyActorByCardId("Story_11_Mission1_Lorebook1"), 2.5f));
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
				yield return GameEntity.Coroutines.StartCoroutine(PlaySoundAndBlockSpeech(VO_Story_11_Handmaiden_015hb_Female_Naga_Story_Faelin_Mission15_Lorebook_2_01, Notification.SpeechBubbleDirection.None, GetEnemyActorByCardId("Story_11_Mission1_Lorebook1"), 2.5f));
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
			Actor friendlyActor = GameState.Get().GetFriendlySidePlayer().GetHero()
				.GetCard()
				.GetActor();
			if (cardID == "Story_11_Dathril2")
			{
				yield return MissionPlayVO(friendlyActor, VO_Story_11_Faelin_000hp_Male_Nightborne_Story_Faelin_Mission15ExchangeC_01);
				yield return MissionPlayVO(GetEnemyActorByCardId("Story_11_Dathril2"), VO_Story_11_Dathril_012hb_Male_Naga_Story_Faelin_Mission15ExchangeC_01);
			}
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
		GameState.Get().GetFriendlySidePlayer().GetHero()
			.GetCard()
			.GetActor();
	}

	public override bool NotifyOfBattlefieldCardClicked(Entity clickedEntity, bool wasInTargetMode)
	{
		if (!base.NotifyOfBattlefieldCardClicked(clickedEntity, wasInTargetMode))
		{
			return false;
		}
		if (!wasInTargetMode)
		{
			if (clickedEntity.GetCardId() == "Story_11_Mission15_Lorebook1")
			{
				if (m_popup == null)
				{
					Network.Get().SendClientScriptGameEvent(ClientScriptGameEventType.MISSION_EVENT, 228);
					HandleMissionEvent(228);
				}
				return false;
			}
			if (clickedEntity.GetCardId() == "Story_11_Mission15_Lorebook2")
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
