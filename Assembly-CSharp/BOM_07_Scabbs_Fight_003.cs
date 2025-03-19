using System.Collections;
using System.Collections.Generic;

public class BOM_07_Scabbs_Fight_003 : BOM_07_Scabbs_Dungeon
{
	private static readonly AssetReference VO_Story_Hero_Dawngrasp_X_BloodElf_BOM_Scabbs_Mission3ExchangeH_01 = new AssetReference("VO_Story_Hero_Dawngrasp_X_BloodElf_BOM_Scabbs_Mission3ExchangeH_01.prefab:9385a1b9f49a431ea017caf8fe47fbd7");

	private static readonly AssetReference VO_Story_Hero_Dawngrasp_X_BloodElf_BOM_Scabbs_Mission3ExchangeI_02 = new AssetReference("VO_Story_Hero_Dawngrasp_X_BloodElf_BOM_Scabbs_Mission3ExchangeI_02.prefab:b65cb7b00ff94a44b0248a1e2f6a2774");

	private static readonly AssetReference VO_Story_Hero_Dawngrasp_X_BloodElf_BOM_Scabbs_Mission3ExchangeJ_02 = new AssetReference("VO_Story_Hero_Dawngrasp_X_BloodElf_BOM_Scabbs_Mission3ExchangeJ_02.prefab:57fc95595648454d8f98e7663cb5633c");

	private static readonly AssetReference VO_Story_Hero_Dawngrasp_X_BloodElf_BOM_Scabbs_Mission3ExchangeJ_03 = new AssetReference("VO_Story_Hero_Dawngrasp_X_BloodElf_BOM_Scabbs_Mission3ExchangeJ_03.prefab:8daa09d5a6654d779a350928dcc33b42");

	private static readonly AssetReference VO_Story_Hero_Dawngrasp_X_BloodElf_BOM_Scabbs_Mission3Victory_02 = new AssetReference("VO_Story_Hero_Dawngrasp_X_BloodElf_BOM_Scabbs_Mission3Victory_02.prefab:22b9a94e9611465192ae9dfc9f942b9c");

	private static readonly AssetReference VO_Story_Hero_Hogger_Male_Gnoll_BOM_Scabbs_Mission3Death_01 = new AssetReference("VO_Story_Hero_Hogger_Male_Gnoll_BOM_Scabbs_Mission3Death_01.prefab:87d03a5747b742c5ba554e59337b8684");

	private static readonly AssetReference VO_Story_Hero_Hogger_Male_Gnoll_BOM_Scabbs_Mission3EmoteResponse_01 = new AssetReference("VO_Story_Hero_Hogger_Male_Gnoll_BOM_Scabbs_Mission3EmoteResponse_01.prefab:3b79ed1ac7ec4ac7b17dcb50669794ac");

	private static readonly AssetReference VO_Story_Hero_Hogger_Male_Gnoll_BOM_Scabbs_Mission3ExchangeA_01 = new AssetReference("VO_Story_Hero_Hogger_Male_Gnoll_BOM_Scabbs_Mission3ExchangeA_01.prefab:75a32fdad9974b5d8b70ea88d0a87625");

	private static readonly AssetReference VO_Story_Hero_Hogger_Male_Gnoll_BOM_Scabbs_Mission3ExchangeB_02 = new AssetReference("VO_Story_Hero_Hogger_Male_Gnoll_BOM_Scabbs_Mission3ExchangeB_02.prefab:0420ade739564762bc41b0d587e51254");

	private static readonly AssetReference VO_Story_Hero_Hogger_Male_Gnoll_BOM_Scabbs_Mission3ExchangeC_01 = new AssetReference("VO_Story_Hero_Hogger_Male_Gnoll_BOM_Scabbs_Mission3ExchangeC_01.prefab:8cabe0aa9be04ed59953be0cc0126a05");

	private static readonly AssetReference VO_Story_Hero_Hogger_Male_Gnoll_BOM_Scabbs_Mission3ExchangeD_01 = new AssetReference("VO_Story_Hero_Hogger_Male_Gnoll_BOM_Scabbs_Mission3ExchangeD_01.prefab:35509d4e7c234eeab7225194c4f1ed20");

	private static readonly AssetReference VO_Story_Hero_Hogger_Male_Gnoll_BOM_Scabbs_Mission3ExchangeE_01 = new AssetReference("VO_Story_Hero_Hogger_Male_Gnoll_BOM_Scabbs_Mission3ExchangeE_01.prefab:51ad6abc395d4ee5a45fbc37b99087da");

	private static readonly AssetReference VO_Story_Hero_Hogger_Male_Gnoll_BOM_Scabbs_Mission3ExchangeE_02 = new AssetReference("VO_Story_Hero_Hogger_Male_Gnoll_BOM_Scabbs_Mission3ExchangeE_02.prefab:b97272b15aa44e0594c7bd26d833d418");

	private static readonly AssetReference VO_Story_Hero_Hogger_Male_Gnoll_BOM_Scabbs_Mission3ExchangeE_03 = new AssetReference("VO_Story_Hero_Hogger_Male_Gnoll_BOM_Scabbs_Mission3ExchangeE_03.prefab:0b14749fc7944418ab808eec81610e10");

	private static readonly AssetReference VO_Story_Hero_Hogger_Male_Gnoll_BOM_Scabbs_Mission3ExchangeF_01 = new AssetReference("VO_Story_Hero_Hogger_Male_Gnoll_BOM_Scabbs_Mission3ExchangeF_01.prefab:6219043d1e0340399e560ffb4be612c2");

	private static readonly AssetReference VO_Story_Hero_Hogger_Male_Gnoll_BOM_Scabbs_Mission3ExchangeF_02 = new AssetReference("VO_Story_Hero_Hogger_Male_Gnoll_BOM_Scabbs_Mission3ExchangeF_02.prefab:68a9d17ba3614688b9ff52e85862770c");

	private static readonly AssetReference VO_Story_Hero_Hogger_Male_Gnoll_BOM_Scabbs_Mission3HeroPower_01 = new AssetReference("VO_Story_Hero_Hogger_Male_Gnoll_BOM_Scabbs_Mission3HeroPower_01.prefab:05cda4035fbe4c6c9c836c8ba5bf251b");

	private static readonly AssetReference VO_Story_Hero_Hogger_Male_Gnoll_BOM_Scabbs_Mission3HeroPower_02 = new AssetReference("VO_Story_Hero_Hogger_Male_Gnoll_BOM_Scabbs_Mission3HeroPower_02.prefab:bbc5febb84a64b9694fb3cf1bd9a1c67");

	private static readonly AssetReference VO_Story_Hero_Hogger_Male_Gnoll_BOM_Scabbs_Mission3HeroPower_03 = new AssetReference("VO_Story_Hero_Hogger_Male_Gnoll_BOM_Scabbs_Mission3HeroPower_03.prefab:68884cf72d2540c6ae49abd3db1f6ab9");

	private static readonly AssetReference VO_Story_Hero_Hogger_Male_Gnoll_BOM_Scabbs_Mission3Idle_01 = new AssetReference("VO_Story_Hero_Hogger_Male_Gnoll_BOM_Scabbs_Mission3Idle_01.prefab:9c2be9293118482799edca13bf4e2d4b");

	private static readonly AssetReference VO_Story_Hero_Hogger_Male_Gnoll_BOM_Scabbs_Mission3Idle_02 = new AssetReference("VO_Story_Hero_Hogger_Male_Gnoll_BOM_Scabbs_Mission3Idle_02.prefab:dfde66dc2b89452cb00f4b1d6381f4c6");

	private static readonly AssetReference VO_Story_Hero_Hogger_Male_Gnoll_BOM_Scabbs_Mission3Idle_03 = new AssetReference("VO_Story_Hero_Hogger_Male_Gnoll_BOM_Scabbs_Mission3Idle_03.prefab:55efbff05d28437e8a42b224988159fa");

	private static readonly AssetReference VO_Story_Hero_Hogger_Male_Gnoll_BOM_Scabbs_Mission3Intro_02 = new AssetReference("VO_Story_Hero_Hogger_Male_Gnoll_BOM_Scabbs_Mission3Intro_02.prefab:c4b09b391ecf4e3e9e4cf989d551b35e");

	private static readonly AssetReference VO_Story_Hero_Hogger_Male_Gnoll_BOM_Scabbs_Mission3Loss_01 = new AssetReference("VO_Story_Hero_Hogger_Male_Gnoll_BOM_Scabbs_Mission3Loss_01.prefab:5b9bce70d4124dbab7bbf73298492c7c");

	private static readonly AssetReference VO_Story_Hero_Scabbs_Male_Gnome_BOM_Scabbs_Mission3ExchangeA_02 = new AssetReference("VO_Story_Hero_Scabbs_Male_Gnome_BOM_Scabbs_Mission3ExchangeA_02.prefab:5821dc4a55104fdfb80dff23126396a9");

	private static readonly AssetReference VO_Story_Hero_Scabbs_Male_Gnome_BOM_Scabbs_Mission3ExchangeB_01 = new AssetReference("VO_Story_Hero_Scabbs_Male_Gnome_BOM_Scabbs_Mission3ExchangeB_01.prefab:495268f2eb0e4635a30fdc9225d5b8eb");

	private static readonly AssetReference VO_Story_Hero_Scabbs_Male_Gnome_BOM_Scabbs_Mission3ExchangeG_01 = new AssetReference("VO_Story_Hero_Scabbs_Male_Gnome_BOM_Scabbs_Mission3ExchangeG_01.prefab:eb50aa35c82c4e1ea177cf71898837c5");

	private static readonly AssetReference VO_Story_Hero_Scabbs_Male_Gnome_BOM_Scabbs_Mission3ExchangeI_01 = new AssetReference("VO_Story_Hero_Scabbs_Male_Gnome_BOM_Scabbs_Mission3ExchangeI_01.prefab:04ba99c356b848c0aaa885ebbbe66dcb");

	private static readonly AssetReference VO_Story_Hero_Scabbs_Male_Gnome_BOM_Scabbs_Mission3ExchangeJ_01 = new AssetReference("VO_Story_Hero_Scabbs_Male_Gnome_BOM_Scabbs_Mission3ExchangeJ_01.prefab:6c1d9b30ba9a4126bff18728892d6bb8");

	private static readonly AssetReference VO_Story_Hero_Scabbs_Male_Gnome_BOM_Scabbs_Mission3Intro_01 = new AssetReference("VO_Story_Hero_Scabbs_Male_Gnome_BOM_Scabbs_Mission3Intro_01.prefab:ad8373244908429888f97459ddee07d5");

	private static readonly AssetReference VO_Story_Hero_Scabbs_Male_Gnome_BOM_Scabbs_Mission3Victory_01 = new AssetReference("VO_Story_Hero_Scabbs_Male_Gnome_BOM_Scabbs_Mission3Victory_01.prefab:1efb7c5bfdc7451dba637836f85cfe81");

	private static readonly AssetReference VO_Story_Hero_Scabbs_Male_Gnome_BOM_Scabbs_Mission3Victory_03 = new AssetReference("VO_Story_Hero_Scabbs_Male_Gnome_BOM_Scabbs_Mission3Victory_03.prefab:87e96f26174843e2b5a3b17f287096f0");

	private static readonly AssetReference VO_Story_Minion_Dawngrasp_X_BloodElf_BOM_Scabbs_Mission3Attack_01 = new AssetReference("VO_Story_Minion_Dawngrasp_X_BloodElf_BOM_Scabbs_Mission3Attack_01.prefab:e82b88eafd3b47babd5e624806b51c54");

	private static readonly AssetReference VO_Story_Minion_Dawngrasp_X_BloodElf_BOM_Scabbs_Mission3Attack_02 = new AssetReference("VO_Story_Minion_Dawngrasp_X_BloodElf_BOM_Scabbs_Mission3Attack_02.prefab:32953a366ab749a3b3afc9c3bf1e8d86");

	private static readonly AssetReference VO_Story_Minion_Dawngrasp_X_BloodElf_BOM_Scabbs_Mission3Death_01 = new AssetReference("VO_Story_Minion_Dawngrasp_X_BloodElf_BOM_Scabbs_Mission3Death_01.prefab:060239a1362f496fbdeaa5e7d0c76c9a");

	private static readonly AssetReference VO_Story_Minion_Dawngrasp_X_BloodElf_BOM_Scabbs_Mission3Play_01 = new AssetReference("VO_Story_Minion_Dawngrasp_X_BloodElf_BOM_Scabbs_Mission3Play_01.prefab:81051a2eca1c40fc9f5965a502e1978b");

	private static readonly AssetReference VO_Story_Minion_Dawngrasp_X_BloodElf_BOM_Scabbs_Mission3Trigger_01 = new AssetReference("VO_Story_Minion_Dawngrasp_X_BloodElf_BOM_Scabbs_Mission3Trigger_01.prefab:af5b18aef5094fe4abf7c470656c4fb3");

	private static readonly AssetReference VO_Story_Minion_Dawngrasp_X_BloodElf_BOM_Scabbs_Mission3Trigger_02 = new AssetReference("VO_Story_Minion_Dawngrasp_X_BloodElf_BOM_Scabbs_Mission3Trigger_02.prefab:0ac5cf5cd3bb48679fb3d7b1c058f96a");

	private static readonly AssetReference VO_Story_Minion_Dawngrasp_X_BloodElf_BOM_Scabbs_Mission3Trigger_03 = new AssetReference("VO_Story_Minion_Dawngrasp_X_BloodElf_BOM_Scabbs_Mission3Trigger_03.prefab:ae02e95082cb4717934f509ee4c02487");

	private static readonly AssetReference VO_Story_Minion_Dawngrasp_X_BloodElf_BOM_Scabbs_Mission3Trigger_04 = new AssetReference("VO_Story_Minion_Dawngrasp_X_BloodElf_BOM_Scabbs_Mission3Trigger_04.prefab:9cdf926e55b74426b2f0a2ef99e69a44");

	private static readonly AssetReference VO_Story_Minion_Dawngrasp_X_BloodElf_BOM_Scabbs_Mission3Trigger_05 = new AssetReference("VO_Story_Minion_Dawngrasp_X_BloodElf_BOM_Scabbs_Mission3Trigger_05.prefab:bfc732f6d2d34c6483273b649c25fb54");

	private static readonly AssetReference VO_PVPDR_Hero_Scabbs_Male_Gnome_Death_01 = new AssetReference("VO_PVPDR_Hero_Scabbs_Male_Gnome_Death_01.prefab:85e10a0b219b1974aaa959bd523c554b");

	private List<string> m_VO_Story_Hero_Hogger_Male_Gnoll_BOM_Scabbs_Mission3ExchangeDLines = new List<string> { VO_Story_Hero_Hogger_Male_Gnoll_BOM_Scabbs_Mission3ExchangeD_01, VO_Story_Minion_Dawngrasp_X_BloodElf_BOM_Scabbs_Mission3Attack_01, VO_Story_Minion_Dawngrasp_X_BloodElf_BOM_Scabbs_Mission3Attack_02 };

	private List<string> m_HoggerPlaysGnoll = new List<string> { VO_Story_Hero_Hogger_Male_Gnoll_BOM_Scabbs_Mission3ExchangeE_01, VO_Story_Hero_Hogger_Male_Gnoll_BOM_Scabbs_Mission3ExchangeE_02, VO_Story_Hero_Hogger_Male_Gnoll_BOM_Scabbs_Mission3ExchangeE_03 };

	private List<string> m_HoggerPlaysSpell = new List<string> { VO_Story_Hero_Hogger_Male_Gnoll_BOM_Scabbs_Mission3ExchangeF_01, VO_Story_Hero_Hogger_Male_Gnoll_BOM_Scabbs_Mission3ExchangeF_02 };

	private List<string> m_DawngraspTriggerEndOfTurn = new List<string> { VO_Story_Minion_Dawngrasp_X_BloodElf_BOM_Scabbs_Mission3Trigger_01, VO_Story_Minion_Dawngrasp_X_BloodElf_BOM_Scabbs_Mission3Trigger_02, VO_Story_Minion_Dawngrasp_X_BloodElf_BOM_Scabbs_Mission3Trigger_03, VO_Story_Minion_Dawngrasp_X_BloodElf_BOM_Scabbs_Mission3Trigger_04, VO_Story_Minion_Dawngrasp_X_BloodElf_BOM_Scabbs_Mission3Trigger_05 };

	private List<string> m_InGame_BossUsesHeroPowerLines = new List<string> { VO_Story_Hero_Hogger_Male_Gnoll_BOM_Scabbs_Mission3HeroPower_01, VO_Story_Hero_Hogger_Male_Gnoll_BOM_Scabbs_Mission3HeroPower_02, VO_Story_Hero_Hogger_Male_Gnoll_BOM_Scabbs_Mission3HeroPower_03 };

	private List<string> m_InGame_BossIdleLines = new List<string> { VO_Story_Hero_Hogger_Male_Gnoll_BOM_Scabbs_Mission3Idle_01, VO_Story_Hero_Hogger_Male_Gnoll_BOM_Scabbs_Mission3Idle_02, VO_Story_Hero_Hogger_Male_Gnoll_BOM_Scabbs_Mission3Idle_03 };

	private HashSet<string> m_playedLines = new HashSet<string>();

	public override void PreloadAssets()
	{
		base.PreloadAssets();
		List<string> m_SoundFiles = new List<string>
		{
			VO_Story_Hero_Dawngrasp_X_BloodElf_BOM_Scabbs_Mission3ExchangeH_01, VO_Story_Hero_Dawngrasp_X_BloodElf_BOM_Scabbs_Mission3ExchangeI_02, VO_Story_Hero_Dawngrasp_X_BloodElf_BOM_Scabbs_Mission3ExchangeJ_02, VO_Story_Hero_Dawngrasp_X_BloodElf_BOM_Scabbs_Mission3ExchangeJ_03, VO_Story_Hero_Dawngrasp_X_BloodElf_BOM_Scabbs_Mission3Victory_02, VO_Story_Hero_Hogger_Male_Gnoll_BOM_Scabbs_Mission3Death_01, VO_Story_Hero_Hogger_Male_Gnoll_BOM_Scabbs_Mission3EmoteResponse_01, VO_Story_Hero_Hogger_Male_Gnoll_BOM_Scabbs_Mission3ExchangeA_01, VO_Story_Hero_Hogger_Male_Gnoll_BOM_Scabbs_Mission3ExchangeB_02, VO_Story_Hero_Hogger_Male_Gnoll_BOM_Scabbs_Mission3ExchangeC_01,
			VO_Story_Hero_Hogger_Male_Gnoll_BOM_Scabbs_Mission3ExchangeD_01, VO_Story_Hero_Hogger_Male_Gnoll_BOM_Scabbs_Mission3ExchangeE_01, VO_Story_Hero_Hogger_Male_Gnoll_BOM_Scabbs_Mission3ExchangeE_02, VO_Story_Hero_Hogger_Male_Gnoll_BOM_Scabbs_Mission3ExchangeE_03, VO_Story_Hero_Hogger_Male_Gnoll_BOM_Scabbs_Mission3ExchangeF_01, VO_Story_Hero_Hogger_Male_Gnoll_BOM_Scabbs_Mission3ExchangeF_02, VO_Story_Hero_Hogger_Male_Gnoll_BOM_Scabbs_Mission3HeroPower_01, VO_Story_Hero_Hogger_Male_Gnoll_BOM_Scabbs_Mission3HeroPower_02, VO_Story_Hero_Hogger_Male_Gnoll_BOM_Scabbs_Mission3HeroPower_03, VO_Story_Hero_Hogger_Male_Gnoll_BOM_Scabbs_Mission3Idle_01,
			VO_Story_Hero_Hogger_Male_Gnoll_BOM_Scabbs_Mission3Idle_02, VO_Story_Hero_Hogger_Male_Gnoll_BOM_Scabbs_Mission3Idle_03, VO_Story_Hero_Hogger_Male_Gnoll_BOM_Scabbs_Mission3Intro_02, VO_Story_Hero_Hogger_Male_Gnoll_BOM_Scabbs_Mission3Loss_01, VO_Story_Hero_Scabbs_Male_Gnome_BOM_Scabbs_Mission3ExchangeA_02, VO_Story_Hero_Scabbs_Male_Gnome_BOM_Scabbs_Mission3ExchangeB_01, VO_Story_Hero_Scabbs_Male_Gnome_BOM_Scabbs_Mission3ExchangeG_01, VO_Story_Hero_Scabbs_Male_Gnome_BOM_Scabbs_Mission3ExchangeI_01, VO_Story_Hero_Scabbs_Male_Gnome_BOM_Scabbs_Mission3ExchangeJ_01, VO_Story_Hero_Scabbs_Male_Gnome_BOM_Scabbs_Mission3Intro_01,
			VO_Story_Hero_Scabbs_Male_Gnome_BOM_Scabbs_Mission3Victory_01, VO_Story_Hero_Scabbs_Male_Gnome_BOM_Scabbs_Mission3Victory_03, VO_Story_Minion_Dawngrasp_X_BloodElf_BOM_Scabbs_Mission3Attack_01, VO_Story_Minion_Dawngrasp_X_BloodElf_BOM_Scabbs_Mission3Attack_02, VO_Story_Minion_Dawngrasp_X_BloodElf_BOM_Scabbs_Mission3Death_01, VO_Story_Minion_Dawngrasp_X_BloodElf_BOM_Scabbs_Mission3Play_01, VO_Story_Minion_Dawngrasp_X_BloodElf_BOM_Scabbs_Mission3Trigger_01, VO_Story_Minion_Dawngrasp_X_BloodElf_BOM_Scabbs_Mission3Trigger_02, VO_Story_Minion_Dawngrasp_X_BloodElf_BOM_Scabbs_Mission3Trigger_03, VO_Story_Minion_Dawngrasp_X_BloodElf_BOM_Scabbs_Mission3Trigger_04,
			VO_Story_Minion_Dawngrasp_X_BloodElf_BOM_Scabbs_Mission3Trigger_05, VO_PVPDR_Hero_Scabbs_Male_Gnome_Death_01
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
		switch (missionEvent)
		{
		case 500:
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Hogger_Male_Gnoll_BOM_Scabbs_Mission3ExchangeC_01);
			break;
		case 516:
			yield return MissionPlaySound(enemyActor, VO_Story_Hero_Hogger_Male_Gnoll_BOM_Scabbs_Mission3Death_01);
			break;
		case 517:
			yield return MissionPlayVO(enemyActor, m_InGame_BossIdleLines);
			break;
		case 510:
			yield return MissionPlayVO(enemyActor, m_InGame_BossUsesHeroPowerLines);
			break;
		case 515:
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Hogger_Male_Gnoll_BOM_Scabbs_Mission3EmoteResponse_01);
			break;
		case 514:
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Scabbs_Male_Gnome_BOM_Scabbs_Mission3Intro_01);
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Hogger_Male_Gnoll_BOM_Scabbs_Mission3Intro_02);
			break;
		case 507:
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Hogger_Male_Gnoll_BOM_Scabbs_Mission3Loss_01);
			GameState.Get().SetBusy(busy: false);
			break;
		case 900:
			yield return MissionPlayVO("BOM_07_Scabbs_Dawngrasp_003t", VO_Story_Minion_Dawngrasp_X_BloodElf_BOM_Scabbs_Mission3Death_01);
			break;
		case 901:
			yield return MissionPlayVO("BOM_07_Scabbs_Dawngrasp_003t", VO_Story_Minion_Dawngrasp_X_BloodElf_BOM_Scabbs_Mission3Play_01);
			break;
		case 505:
			GameState.Get().SetBusy(busy: true);
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Scabbs_Male_Gnome_BOM_Scabbs_Mission3Victory_01);
			yield return MissionPlayVO("BOM_07_Scabbs_Dawngrasp_003t", VO_Story_Hero_Dawngrasp_X_BloodElf_BOM_Scabbs_Mission3Victory_02);
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Scabbs_Male_Gnome_BOM_Scabbs_Mission3Victory_03);
			GameState.Get().SetBusy(busy: false);
			break;
		case 100:
			yield return MissionPlayVO("BOM_07_Scabbs_Dawngrasp_003t", m_DawngraspTriggerEndOfTurn);
			break;
		case 519:
			yield return MissionPlaySound(enemyActor, VO_PVPDR_Hero_Scabbs_Male_Gnome_Death_01);
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
			GameState.Get().GetFriendlySidePlayer().GetHero()
				.GetCard()
				.GetActor();
			switch (cardID)
			{
			case "TUa_003":
			case "SW_062":
				yield return MissionPlayVO(enemyActor, m_HoggerPlaysGnoll);
				break;
			case "TUa_004":
				yield return MissionPlayVO(enemyActor, m_HoggerPlaysSpell);
				break;
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
		case 3:
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Hogger_Male_Gnoll_BOM_Scabbs_Mission3ExchangeA_01);
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Scabbs_Male_Gnome_BOM_Scabbs_Mission3ExchangeA_02);
			break;
		case 5:
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Scabbs_Male_Gnome_BOM_Scabbs_Mission3ExchangeB_01);
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Hogger_Male_Gnoll_BOM_Scabbs_Mission3ExchangeB_02);
			break;
		case 11:
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Scabbs_Male_Gnome_BOM_Scabbs_Mission3ExchangeI_01);
			yield return MissionPlayVO("BOM_07_Scabbs_Dawngrasp_003t", VO_Story_Hero_Dawngrasp_X_BloodElf_BOM_Scabbs_Mission3ExchangeI_02);
			break;
		case 13:
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Scabbs_Male_Gnome_BOM_Scabbs_Mission3ExchangeJ_01);
			yield return MissionPlayVO("BOM_07_Scabbs_Dawngrasp_003t", VO_Story_Hero_Dawngrasp_X_BloodElf_BOM_Scabbs_Mission3ExchangeJ_02);
			yield return MissionPlayVO("BOM_07_Scabbs_Dawngrasp_003t", VO_Story_Hero_Dawngrasp_X_BloodElf_BOM_Scabbs_Mission3ExchangeJ_03);
			break;
		}
	}
}
