using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tutorial_Dungeon : Tutorial_MissionEntity
{
	public static AssetReference SWDawngraspMinion_BrassRing_Quote = new AssetReference("SWDawngraspMinion_BrassRing_Quote.prefab:dfa0a79775ba5c34ea888cd56c91f517");

	public static AssetReference Brukan_20_4_BrassRing_Quote = new AssetReference("Brukan_20_4_BrassRing_Quote.prefab:8bece690907cc3b4897efce42d839510");

	public static AssetReference Guff_BrassRing_Quote = new AssetReference("GruffTier5_BrassRing_Quote.prefab:199725ce3c5e52043915e19a0a880c71");

	public static AssetReference Rokara_B_BrassRing_Quote = new AssetReference("Rokara_B_BrassRing_Quote.prefab:301c3d7a32636944884d6fa120099950");

	public static AssetReference Cariel_BrassRing_Quote = new AssetReference("Cariel_BrassRing_Quote.prefab:f92b72ab12fd34a4db73d365311ceb20");

	public static AssetReference Kurtrus_Stormwind_BrassRing_Quote = new AssetReference("Kurtrus_Stormwind_BrassRing_Quote.prefab:76cde32559de9c643af479d3f38970a8");

	public static AssetReference Tavish4_BrassRing_Quote = new AssetReference("Tavish4_BrassRing_Quote.prefab:28458b58b7d010d42b0bda2ff89683e9");

	public static AssetReference Cariel5_BrassRing_Quote = new AssetReference("Cariel5_BrassRing_Quote.prefab:7accbc43ce82f9c40adcdfd07b81bde6");

	public static AssetReference Scabbs5_BrassRing_Quote = new AssetReference("Scabbs5_BrassRing_Quote.prefab:7a1b7f2bbe41dd0409b4571f5c37452b");

	public static AssetReference Tamsin5_BrassRing_Quote = new AssetReference("Tamsin5_BrassRing_Quote.prefab:24d7633f9befde240b99a096f129dbd0");

	public static AssetReference Xyrella3_BrassRing_Quote = new AssetReference("Xyrella3_BrassRing_Quote.prefab:5ac0e3f7c01211944b826ee68336ae51");

	public static AssetReference KurtrusTier5_BrassRing_Quote = new AssetReference("KurtrusTier5_BrassRing_Quote.prefab:4fd3809b8824f6e40bc72288fddb573a");

	public static AssetReference Alterac_XyrellaArt_BrassRing_Quote = new AssetReference("Alterac_XyrellaArt_BrassRing_Quote.prefab:ace91814810897646a63ca8c9ada2fdd");

	public static AssetReference Alterac_CarielArt_BrassRing_Quote = new AssetReference("Alterac_CarielArt_BrassRing_Quote.prefab:8ff3e82fb40459349b450ddaa59e3ecd");

	public static AssetReference Alterac_GuffHero_BrassRing_Quote = new AssetReference("Alterac_GuffHero_BrassRing_Quote.prefab:22fb1479b121d3e4488052fb7d9d674c");

	public static AssetReference Alterac_TavishArt_BrassRing_Quote = new AssetReference("Alterac_TavishArt_BrassRing_Quote.prefab:2b66fcb0841100942b78a0a057e8b705");

	public static AssetReference RokaraTier5_BrassRing_Quote = new AssetReference("RokaraTier5_BrassRing_Quote.prefab:e508949e4c942624798c72b30774529e");

	public static AssetReference DawngraspTier5_BrassRing_Quote = new AssetReference("DawngraspTier5_BrassRing_Quote.prefab:f33138a364e51624092b1307fdec7f70");

	public static AssetReference MinerTogwaggle_BrassRing_Quote = new AssetReference("MinerTogwaggle_BrassRing_Quote.prefab:97cedc05a1267d142a72df6dcd8ca8bc");

	public static AssetReference Alterac_ScabbsArt_BrassRing_Quote = new AssetReference("Alterac_ScabbsArt_BrassRing_Quote.prefab:bd62b693bd9bc0a438041e3693051370");

	public static AssetReference Jaina_BrassRing_Quote = new AssetReference("Jaina_BrassRing_Quote.prefab:7d460f59f3082414bb86a76b27703b00");

	public static AssetReference LadyVashj_BrassRing_Quote = new AssetReference("LadyVashj_BrassRing_Quote.prefab:55c759518fb98bf4887f1010e25cd83b");

	public static AssetReference Nerzhul_BrassRing_Quote = new AssetReference("Nerzhul_BrassRing_Quote.prefab:fcaa7ef9b0afd7c4e9de4132c0577e3c");

	private static readonly AssetReference VO_Story_Hero_Cariel_Female_Human_BOM_Scabbs_HeroPower_01 = new AssetReference("VO_Story_Hero_Cariel_Female_Human_BOM_Scabbs_HeroPower_01.prefab:c40bb67529e2f75488a687b29e366dce");

	private static readonly AssetReference VO_Story_Hero_Cariel_Female_Human_BOM_Scabbs_HeroPower_02 = new AssetReference("VO_Story_Hero_Cariel_Female_Human_BOM_Scabbs_HeroPower_02.prefab:1bd2a973a10a50f449fa853434c5c19c");

	private static readonly AssetReference VO_Story_Hero_Cariel_Female_Human_BOM_Scabbs_HeroPower_03 = new AssetReference("VO_Story_Hero_Cariel_Female_Human_BOM_Scabbs_HeroPower_03.prefab:de12dc2c97279184aae3cc11e1756c0c");

	private static readonly AssetReference VO_Story_Hero_Kurtrus_Male_NightElf_BOM_Scabbs_HeroPower_01 = new AssetReference("VO_Story_Hero_Kurtrus_Male_NightElf_BOM_Scabbs_HeroPower_01.prefab:782455e872e6bc147938f97842d9817e");

	private static readonly AssetReference VO_Story_Hero_Kurtrus_Male_NightElf_BOM_Scabbs_HeroPower_02 = new AssetReference("VO_Story_Hero_Kurtrus_Male_NightElf_BOM_Scabbs_HeroPower_02.prefab:6f7943e790fab104d86b4d19c6ce975b");

	private static readonly AssetReference VO_Story_Hero_Kurtrus_Male_NightElf_BOM_Scabbs_HeroPower_03 = new AssetReference("VO_Story_Hero_Kurtrus_Male_NightElf_BOM_Scabbs_HeroPower_03.prefab:ad35989c054538b4da47fd868b74a657");

	private static readonly AssetReference VO_Story_Hero_Tavish_Male_Dwarf_BOM_Scabbs_HeroPower_01 = new AssetReference("VO_Story_Hero_Tavish_Male_Dwarf_BOM_Scabbs_HeroPower_01.prefab:41c1f8fa116752f46af51caeb6caeed9");

	private static readonly AssetReference VO_Story_Hero_Tavish_Male_Dwarf_BOM_Scabbs_HeroPower_02 = new AssetReference("VO_Story_Hero_Tavish_Male_Dwarf_BOM_Scabbs_HeroPower_02.prefab:c5d62462456c86d4991db76dc7a2bde1");

	private static readonly AssetReference VO_Story_Hero_Tavish_Male_Dwarf_BOM_Scabbs_HeroPower_03 = new AssetReference("VO_Story_Hero_Tavish_Male_Dwarf_BOM_Scabbs_HeroPower_03.prefab:2b8ab2e036415d14e934f46f7f1c2d6d");

	private static readonly AssetReference VO_Story_Hero_Xyrella_Female_Draenei_BOM_Scabbs_HeroPower_01 = new AssetReference("VO_Story_Hero_Xyrella_Female_Draenei_BOM_Scabbs_HeroPower_01.prefab:6cc5a32c122f74f47afad43a2625ccab");

	private static readonly AssetReference VO_Story_Hero_Xyrella_Female_Draenei_BOM_Scabbs_HeroPower_02 = new AssetReference("VO_Story_Hero_Xyrella_Female_Draenei_BOM_Scabbs_HeroPower_02.prefab:56dc1b62b35fd6141bdaefa9e544381f");

	private static readonly AssetReference VO_Story_Hero_Xyrella_Female_Draenei_BOM_Scabbs_HeroPower_03 = new AssetReference("VO_Story_Hero_Xyrella_Female_Draenei_BOM_Scabbs_HeroPower_03.prefab:1bf8a68524d3dad41bc7755a7987d208");

	private List<string> m_Cariel_HeroPowerLines = new List<string> { VO_Story_Hero_Cariel_Female_Human_BOM_Scabbs_HeroPower_01, VO_Story_Hero_Cariel_Female_Human_BOM_Scabbs_HeroPower_02, VO_Story_Hero_Cariel_Female_Human_BOM_Scabbs_HeroPower_03 };

	private List<string> m_Kurtrus_HeroPowerLines = new List<string> { VO_Story_Hero_Kurtrus_Male_NightElf_BOM_Scabbs_HeroPower_01, VO_Story_Hero_Kurtrus_Male_NightElf_BOM_Scabbs_HeroPower_02, VO_Story_Hero_Kurtrus_Male_NightElf_BOM_Scabbs_HeroPower_03 };

	private List<string> m_Tavish_HeroPowerLines = new List<string> { VO_Story_Hero_Tavish_Male_Dwarf_BOM_Scabbs_HeroPower_01, VO_Story_Hero_Tavish_Male_Dwarf_BOM_Scabbs_HeroPower_02, VO_Story_Hero_Tavish_Male_Dwarf_BOM_Scabbs_HeroPower_03 };

	private List<string> m_Xyrella_HeroPowerLines = new List<string> { VO_Story_Hero_Xyrella_Female_Draenei_BOM_Scabbs_HeroPower_01, VO_Story_Hero_Xyrella_Female_Draenei_BOM_Scabbs_HeroPower_02, VO_Story_Hero_Xyrella_Female_Draenei_BOM_Scabbs_HeroPower_03 };

	public bool HeroPowerIsCariel;

	public bool HeroPowerIsKurtrus;

	public bool HeroPowerIsTavish;

	public bool HeroPowerIsXyrella;

	public override void OnCreateGame()
	{
		base.OnCreateGame();
		m_introLine = null;
		m_deathLine = null;
		m_standardEmoteResponseLine = null;
		m_BossIdleLines = new List<string>(GetBossIdleLines());
		m_BossIdleLinesCopy = new List<string>(GetBossIdleLines());
		m_OverrideMusicTrack = MusicPlaylistType.Invalid;
		m_OverrideMulliganMusicTrack = MusicPlaylistType.Invalid;
		m_Mission_EnemyHeroShouldExplodeOnDefeat = false;
		m_Mission_FriendlyHeroShouldExplodeOnDefeat = true;
		m_OverrideBossSubtext = null;
		m_OverridePlayerSubtext = null;
		m_SupressEnemyDeathTextBubble = true;
	}

	public override void PreloadAssets()
	{
		base.PreloadAssets();
		List<string> soundFiles = new List<string>();
		SetBossVOLines(soundFiles);
		foreach (string soundFile in soundFiles)
		{
			PreloadSound(soundFile);
		}
	}

	public sealed override AdventureDbId GetAdventureID()
	{
		return AdventureDbId.TUTORIAL;
	}

	protected override IEnumerator RespondToFriendlyPlayedCardWithTiming(Entity entity)
	{
		GameState.Get().GetFriendlySidePlayer().GetHeroCard()
			.GetActor();
		GameState.Get().GetFriendlySidePlayer().GetHero()
			.GetCardId();
		while (entity.GetCardType() == TAG_CARDTYPE.INVALID)
		{
			yield return null;
		}
		yield return base.RespondToFriendlyPlayedCardWithTiming(entity);
		yield return WaitForEntitySoundToFinish(entity);
		entity.GetCardId();
	}

	public override IEnumerator DoActionsBeforeDealingBaseMulliganCards()
	{
		MissionPause(pause: true);
		yield return HandleMissionEventWithTiming(514);
		MissionPause(pause: false);
	}

	protected override IEnumerator HandleMissionEventWithTiming(int missionEvent)
	{
		while (m_enemySpeaking)
		{
			yield return null;
		}
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
		Actor friendlyActor = GameState.Get().GetFriendlySidePlayer().GetHero()
			.GetCard()
			.GetActor();
		Actor enemyActor = GameState.Get().GetOpposingSidePlayer().GetHeroCard()
			.GetActor();
		Actor friendlyHeroPowerActor = GameState.Get().GetFriendlySidePlayer().GetHeroPower()
			.GetCard()
			.GetActor();
		GameState.Get().GetOpposingSidePlayer().GetHero()
			.GetCardId();
		Random.Range(0f, 1f);
		GetTag(GAME_TAG.TURN);
		GameState.Get().GetGameEntity().GetTag(GAME_TAG.EXTRA_TURNS_TAKEN_THIS_GAME);
		Random.Range(0f, 1f);
		switch (missionEvent)
		{
		case 1000:
			GameState.Get().GetGameEntity().SetTag(GAME_TAG.MISSION_EVENT, 0);
			if (m_PlayPlayerVOLineIndex + 1 >= m_PlayerVOLines.Count)
			{
				m_PlayPlayerVOLineIndex = 0;
			}
			else
			{
				m_PlayPlayerVOLineIndex++;
			}
			SceneDebugger.Get().AddMessage(m_PlayerVOLines[m_PlayPlayerVOLineIndex]);
			yield return PlayBossLine(friendlyActor, m_PlayerVOLines[m_PlayPlayerVOLineIndex]);
			break;
		case 1001:
			GameState.Get().GetGameEntity().SetTag(GAME_TAG.MISSION_EVENT, 0);
			SceneDebugger.Get().AddMessage(m_PlayerVOLines[m_PlayPlayerVOLineIndex]);
			yield return PlayBossLine(friendlyActor, m_PlayerVOLines[m_PlayPlayerVOLineIndex]);
			break;
		case 1002:
			GameState.Get().GetGameEntity().SetTag(GAME_TAG.MISSION_EVENT, 0);
			if (m_PlayBossVOLineIndex + 1 >= m_BossVOLines.Count)
			{
				m_PlayBossVOLineIndex = 0;
			}
			else
			{
				m_PlayBossVOLineIndex++;
			}
			SceneDebugger.Get().AddMessage(m_BossVOLines[m_PlayBossVOLineIndex]);
			yield return PlayBossLine(enemyActor, m_BossVOLines[m_PlayBossVOLineIndex]);
			break;
		case 1003:
			GameState.Get().GetGameEntity().SetTag(GAME_TAG.MISSION_EVENT, 0);
			SceneDebugger.Get().AddMessage(m_BossVOLines[m_PlayBossVOLineIndex]);
			yield return PlayBossLine(enemyActor, m_BossVOLines[m_PlayBossVOLineIndex]);
			break;
		case 1011:
			GameState.Get().GetGameEntity().SetTag(GAME_TAG.MISSION_EVENT, 0);
			foreach (string currentLine3 in m_BossVOLines)
			{
				SceneDebugger.Get().AddMessage(currentLine3);
				yield return MissionPlayVO(enemyActor, currentLine3);
			}
			foreach (string currentLine4 in m_PlayerVOLines)
			{
				SceneDebugger.Get().AddMessage(currentLine4);
				yield return MissionPlayVO(enemyActor, currentLine4);
			}
			break;
		case 1012:
			GameState.Get().GetGameEntity().SetTag(GAME_TAG.MISSION_EVENT, 0);
			foreach (string currentLine in m_BossVOLines)
			{
				SceneDebugger.Get().AddMessage(currentLine);
				yield return MissionPlayVO(enemyActor, currentLine);
			}
			break;
		case 1013:
			GameState.Get().GetGameEntity().SetTag(GAME_TAG.MISSION_EVENT, 0);
			foreach (string currentLine2 in m_PlayerVOLines)
			{
				SceneDebugger.Get().AddMessage(currentLine2);
				yield return MissionPlayVO(enemyActor, currentLine2);
			}
			break;
		case 1010:
			if (m_forceAlwaysPlayLine)
			{
				m_forceAlwaysPlayLine = false;
			}
			else
			{
				m_forceAlwaysPlayLine = true;
			}
			break;
		case 58023:
		{
			SceneMgr.Mode nextMode = GameMgr.Get().GetPostGameSceneMode();
			GameMgr.Get().PreparePostGameSceneMode(nextMode);
			SceneMgr.Get().SetNextMode(nextMode);
			break;
		}
		case 600:
			m_Mission_EnemyHeroShouldExplodeOnDefeat = false;
			break;
		case 610:
			m_Mission_EnemyHeroShouldExplodeOnDefeat = true;
			break;
		case 601:
			m_Mission_FriendlyHeroShouldExplodeOnDefeat = false;
			break;
		case 611:
			m_Mission_FriendlyHeroShouldExplodeOnDefeat = true;
			break;
		case 603:
			m_MissionDisableAutomaticVO = false;
			break;
		case 602:
			m_MissionDisableAutomaticVO = true;
			break;
		case 612:
			m_DoEmoteDrivenStart = true;
			break;
		case 58024:
			HeroPowerIsCariel = true;
			HeroPowerIsKurtrus = false;
			HeroPowerIsTavish = false;
			HeroPowerIsXyrella = false;
			break;
		case 58025:
			HeroPowerIsCariel = false;
			HeroPowerIsKurtrus = true;
			HeroPowerIsTavish = false;
			HeroPowerIsXyrella = false;
			break;
		case 58026:
			HeroPowerIsCariel = false;
			HeroPowerIsKurtrus = false;
			HeroPowerIsTavish = true;
			HeroPowerIsXyrella = false;
			break;
		case 58027:
			HeroPowerIsCariel = false;
			HeroPowerIsKurtrus = false;
			HeroPowerIsTavish = false;
			HeroPowerIsXyrella = true;
			break;
		case 508:
			if (HeroPowerIsCariel)
			{
				yield return MissionPlaySound(friendlyHeroPowerActor, m_Cariel_HeroPowerLines);
			}
			if (HeroPowerIsKurtrus)
			{
				yield return MissionPlaySound(friendlyHeroPowerActor, m_Kurtrus_HeroPowerLines);
			}
			if (HeroPowerIsTavish)
			{
				yield return MissionPlaySound(friendlyHeroPowerActor, m_Tavish_HeroPowerLines);
			}
			if (HeroPowerIsXyrella)
			{
				yield return MissionPlaySound(friendlyHeroPowerActor, m_Xyrella_HeroPowerLines);
			}
			break;
		default:
			yield return base.HandleMissionEventWithTiming(missionEvent);
			break;
		}
	}
}
