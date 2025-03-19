using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BOM_05_Tamsin_Fight_008 : BOM_05_Tamsin_Dungeon
{
	private static readonly AssetReference VO_Story_Hero_Cornelius_Male_Human_BOM_Tamsin_Mission8ExchangeB_01 = new AssetReference("VO_Story_Hero_Cornelius_Male_Human_BOM_Tamsin_Mission8ExchangeB_01.prefab:28ca985006b6e7f48bada18ca15ff9d1");

	private static readonly AssetReference VO_Story_Hero_Cornelius_Male_Human_BOM_Tamsin_Mission8ExchangeC_01 = new AssetReference("VO_Story_Hero_Cornelius_Male_Human_BOM_Tamsin_Mission8ExchangeC_01.prefab:a9948b289e10b874682231a90f799245");

	private static readonly AssetReference VO_Story_Hero_Cornelius_Male_Human_BOM_Tamsin_Mission8ExchangeD_01 = new AssetReference("VO_Story_Hero_Cornelius_Male_Human_BOM_Tamsin_Mission8ExchangeD_01.prefab:f3d9c476f4ad4e240bcb5d059018cfbb");

	private static readonly AssetReference VO_Story_Hero_Cornelius_Male_Human_BOM_Tamsin_Mission8ExchangeF_01 = new AssetReference("VO_Story_Hero_Cornelius_Male_Human_BOM_Tamsin_Mission8ExchangeF_01.prefab:22656daf6cbbfda488924d43b75b1c8a");

	private static readonly AssetReference VO_Story_Hero_Cornelius_Male_Human_BOM_Tamsin_Mission8ExchangeG_01 = new AssetReference("VO_Story_Hero_Cornelius_Male_Human_BOM_Tamsin_Mission8ExchangeG_01.prefab:a654d24d03f58a04d90e780b28052902");

	private static readonly AssetReference VO_Story_Hero_Cornelius_Male_Human_BOM_Tamsin_Mission8ExchangeH_01 = new AssetReference("VO_Story_Hero_Cornelius_Male_Human_BOM_Tamsin_Mission8ExchangeH_01.prefab:2e435f906a10a544fba306fdde355e1b");

	private static readonly AssetReference VO_Story_Hero_Cornelius_Male_Human_BOM_Tamsin_Mission8ExchangeI_01 = new AssetReference("VO_Story_Hero_Cornelius_Male_Human_BOM_Tamsin_Mission8ExchangeI_01.prefab:3701609207e97db4dbefc79f17c24c6a");

	private static readonly AssetReference VO_Story_Hero_Cornelius_Male_Human_BOM_Tamsin_Mission8Intro_01 = new AssetReference("VO_Story_Hero_Cornelius_Male_Human_BOM_Tamsin_Mission8Intro_01.prefab:dd798ef8f2a7eb34cbda0a2a97d675b4");

	private static readonly AssetReference VO_Story_Hero_Tamsin_Female_Undead_BOM_Tamsin_Mission8ExchangeA_01 = new AssetReference("VO_Story_Hero_Tamsin_Female_Undead_BOM_Tamsin_Mission8ExchangeA_01.prefab:bcc894e7f0fca3749a13a9e5d9f3d31c");

	private static readonly AssetReference VO_Story_Hero_Tamsin_Female_Undead_BOM_Tamsin_Mission8ExchangeB_02 = new AssetReference("VO_Story_Hero_Tamsin_Female_Undead_BOM_Tamsin_Mission8ExchangeB_02.prefab:05f4f6627d11f074d8f7436a7e715744");

	private static readonly AssetReference VO_Story_Hero_Tamsin_Female_Undead_BOM_Tamsin_Mission8ExchangeD_02 = new AssetReference("VO_Story_Hero_Tamsin_Female_Undead_BOM_Tamsin_Mission8ExchangeD_02.prefab:09a3e3a84139279479bc34291af6cebb");

	private static readonly AssetReference VO_Story_Hero_Tamsin_Female_Undead_BOM_Tamsin_Mission8ExchangeE_01 = new AssetReference("VO_Story_Hero_Tamsin_Female_Undead_BOM_Tamsin_Mission8ExchangeE_01.prefab:1ba50e7f865e5e444978b88920dcccfb");

	private static readonly AssetReference VO_Story_Hero_Tamsin_Female_Undead_BOM_Tamsin_Mission8ExchangeE_02 = new AssetReference("VO_Story_Hero_Tamsin_Female_Undead_BOM_Tamsin_Mission8ExchangeE_02.prefab:640c0743fb2eb774cbba8e7dfb618014");

	private static readonly AssetReference VO_Story_Hero_Tamsin_Female_Undead_BOM_Tamsin_Mission8ExchangeE_03 = new AssetReference("VO_Story_Hero_Tamsin_Female_Undead_BOM_Tamsin_Mission8ExchangeE_03.prefab:6a3a514472ed4d44487473b37c57a210");

	private static readonly AssetReference VO_Story_Hero_Tamsin_Female_Undead_BOM_Tamsin_Mission8ExchangeE_04 = new AssetReference("VO_Story_Hero_Tamsin_Female_Undead_BOM_Tamsin_Mission8ExchangeE_04.prefab:e9eec3092e8bdef48b7f1e328093bbe4");

	private static readonly AssetReference VO_Story_Hero_Tamsin_Female_Undead_BOM_Tamsin_Mission8ExchangeE_05 = new AssetReference("VO_Story_Hero_Tamsin_Female_Undead_BOM_Tamsin_Mission8ExchangeE_05.prefab:b6538ee7d5ba09b46aabd462b48769b0");

	private static readonly AssetReference VO_Story_Hero_Tamsin_Female_Undead_BOM_Tamsin_Mission8ExchangeE_06 = new AssetReference("VO_Story_Hero_Tamsin_Female_Undead_BOM_Tamsin_Mission8ExchangeE_06.prefab:d56c3af902a2cff4e93fa4f80b0746fb");

	private static readonly AssetReference VO_Story_Hero_Tamsin_Female_Undead_BOM_Tamsin_Mission8ExchangeI_02 = new AssetReference("VO_Story_Hero_Tamsin_Female_Undead_BOM_Tamsin_Mission8ExchangeI_02.prefab:50f9abadd2c46bd4d9097dd7a7f5ab66");

	private static readonly AssetReference VO_Story_Hero_Tamsin_Female_Undead_BOM_Tamsin_Mission8Intro_02 = new AssetReference("VO_Story_Hero_Tamsin_Female_Undead_BOM_Tamsin_Mission8Intro_02.prefab:ba2d3e269daf0cb4180049a8bde0dbbf");

	private static readonly AssetReference VO_Story_Hero_Cornelius_Male_Human_BOM_Cariel_Mission3Death_01 = new AssetReference("VO_Story_Hero_Cornelius_Male_Human_BOM_Cariel_Mission3Death_01.prefab:9373b14fb180a7543893f48678752151");

	private static readonly AssetReference VO_PVPDR_Hero_Tamsin_Female_Forsaken_Death_01 = new AssetReference("VO_PVPDR_Hero_Tamsin_Female_Forsaken_Death_01.prefab:0261caf1dfbfae146bf927a03851b086");

	private Dictionary<int, string[]> m_popUpInfo = new Dictionary<int, string[]> { 
	{
		228,
		new string[1] { "BOM_TAMSIN_08" }
	} };

	private float popUpScale = 1.25f;

	private Vector3 popUpPos;

	private List<string> m_VO_Story_Hero_Tamsin_Female_Undead_BOM_Tamsin_Mission8ExchangeELines = new List<string> { VO_Story_Hero_Tamsin_Female_Undead_BOM_Tamsin_Mission8ExchangeE_01, VO_Story_Hero_Tamsin_Female_Undead_BOM_Tamsin_Mission8ExchangeE_02, VO_Story_Hero_Tamsin_Female_Undead_BOM_Tamsin_Mission8ExchangeE_03, VO_Story_Hero_Tamsin_Female_Undead_BOM_Tamsin_Mission8ExchangeE_04 };

	private HashSet<string> m_playedLines = new HashSet<string>();

	public override void PreloadAssets()
	{
		base.PreloadAssets();
		List<string> m_SoundFiles = new List<string>
		{
			VO_Story_Hero_Cornelius_Male_Human_BOM_Tamsin_Mission8ExchangeB_01, VO_Story_Hero_Cornelius_Male_Human_BOM_Tamsin_Mission8ExchangeC_01, VO_Story_Hero_Cornelius_Male_Human_BOM_Tamsin_Mission8ExchangeD_01, VO_Story_Hero_Cornelius_Male_Human_BOM_Tamsin_Mission8ExchangeF_01, VO_Story_Hero_Cornelius_Male_Human_BOM_Tamsin_Mission8ExchangeG_01, VO_Story_Hero_Cornelius_Male_Human_BOM_Tamsin_Mission8ExchangeH_01, VO_Story_Hero_Cornelius_Male_Human_BOM_Tamsin_Mission8ExchangeI_01, VO_Story_Hero_Cornelius_Male_Human_BOM_Tamsin_Mission8Intro_01, VO_Story_Hero_Tamsin_Female_Undead_BOM_Tamsin_Mission8ExchangeA_01, VO_Story_Hero_Tamsin_Female_Undead_BOM_Tamsin_Mission8ExchangeB_02,
			VO_Story_Hero_Tamsin_Female_Undead_BOM_Tamsin_Mission8ExchangeD_02, VO_Story_Hero_Tamsin_Female_Undead_BOM_Tamsin_Mission8ExchangeE_01, VO_Story_Hero_Tamsin_Female_Undead_BOM_Tamsin_Mission8ExchangeE_02, VO_Story_Hero_Tamsin_Female_Undead_BOM_Tamsin_Mission8ExchangeE_03, VO_Story_Hero_Tamsin_Female_Undead_BOM_Tamsin_Mission8ExchangeE_04, VO_Story_Hero_Tamsin_Female_Undead_BOM_Tamsin_Mission8ExchangeE_05, VO_Story_Hero_Tamsin_Female_Undead_BOM_Tamsin_Mission8ExchangeE_06, VO_Story_Hero_Tamsin_Female_Undead_BOM_Tamsin_Mission8ExchangeI_02, VO_Story_Hero_Tamsin_Female_Undead_BOM_Tamsin_Mission8Intro_02, VO_Story_Hero_Cornelius_Male_Human_BOM_Cariel_Mission3Death_01,
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
		popUpPos = new Vector3(0f, 0f, -40f);
		switch (missionEvent)
		{
		case 514:
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Cornelius_Male_Human_BOM_Tamsin_Mission8Intro_01);
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Tamsin_Female_Undead_BOM_Tamsin_Mission8Intro_02);
			break;
		case 516:
			MissionPause(pause: true);
			yield return MissionPlaySound(enemyActor, VO_Story_Hero_Cornelius_Male_Human_BOM_Cariel_Mission3Death_01);
			MissionPause(pause: false);
			break;
		case 519:
			yield return MissionPlaySound(friendlyActor, VO_PVPDR_Hero_Tamsin_Female_Forsaken_Death_01);
			break;
		case 515:
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Cornelius_Male_Human_BOM_Tamsin_Mission8ExchangeC_01);
			break;
		case 100:
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Cornelius_Male_Human_BOM_Tamsin_Mission8ExchangeF_01);
			break;
		case 101:
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Cornelius_Male_Human_BOM_Tamsin_Mission8ExchangeG_01);
			break;
		case 102:
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Cornelius_Male_Human_BOM_Tamsin_Mission8ExchangeH_01);
			break;
		case 103:
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Cornelius_Male_Human_BOM_Tamsin_Mission8ExchangeI_01);
			break;
		case 104:
			yield return MissionPlayVOOnceInOrder(friendlyActor, m_VO_Story_Hero_Tamsin_Female_Undead_BOM_Tamsin_Mission8ExchangeELines);
			break;
		case 105:
			yield return MissionPlayVOOnce(friendlyActor, VO_Story_Hero_Tamsin_Female_Undead_BOM_Tamsin_Mission8ExchangeE_06);
			break;
		case 106:
			yield return MissionPlayVOOnce(friendlyActor, VO_Story_Hero_Tamsin_Female_Undead_BOM_Tamsin_Mission8ExchangeI_02);
			break;
		case 107:
			yield return MissionPlayVOOnce(friendlyActor, VO_Story_Hero_Tamsin_Female_Undead_BOM_Tamsin_Mission8ExchangeE_05);
			break;
		case 228:
		{
			yield return new WaitForSeconds(2f);
			Notification popup = NotificationManager.Get().CreatePopupText(UserAttentionBlocker.NONE, popUpPos, TutorialEntity.GetTextScale() * popUpScale, GameStrings.Get(m_popUpInfo[missionEvent][0]), convertLegacyPosition: false, NotificationManager.PopupTextType.FANCY);
			NotificationManager.Get().DestroyNotification(popup, 7.5f);
			break;
		}
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
		case 3:
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Tamsin_Female_Undead_BOM_Tamsin_Mission8ExchangeA_01);
			break;
		case 7:
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Cornelius_Male_Human_BOM_Tamsin_Mission8ExchangeB_01);
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Tamsin_Female_Undead_BOM_Tamsin_Mission8ExchangeB_02);
			break;
		case 11:
			yield return MissionPlayVO(enemyActor, VO_Story_Hero_Cornelius_Male_Human_BOM_Tamsin_Mission8ExchangeD_01);
			yield return MissionPlayVO(friendlyActor, VO_Story_Hero_Tamsin_Female_Undead_BOM_Tamsin_Mission8ExchangeD_02);
			break;
		}
	}
}
