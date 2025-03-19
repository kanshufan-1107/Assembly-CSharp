using System.Collections;
using System.Collections.Generic;
using Blizzard.T5.AssetManager;
using UnityEngine;

public class Tutorial_Fight_003 : Tutorial_Dungeon
{
	private static readonly AssetReference VO_TUTR_Jaina_JainaProudmoore_Human_InGame_Introduction_06 = new AssetReference("VO_TUTR_Jaina_JainaProudmoore_Human_InGame_Introduction_06.prefab:310b704ca69a28044a511c47ce38ac25");

	private static readonly AssetReference VO_TUTR_Jaina_JainaProudmoore_Human_InGame_Introduction_07 = new AssetReference("VO_TUTR_Jaina_JainaProudmoore_Human_InGame_Introduction_07.prefab:4413bebf466953346af2a98e559c46ca");

	private static readonly AssetReference VO_TUTR_Jaina_JainaProudmoore_Human_InGame_OnEnemyFullBoard_01 = new AssetReference("VO_TUTR_Jaina_JainaProudmoore_Human_InGame_OnEnemyFullBoard_01.prefab:989f22b1fb5ac1143a596275c79e22e2");

	private static readonly AssetReference VO_TUTR_Jaina_JainaProudmoore_Human_InGame_OnEnemyMinionsFrozen_01 = new AssetReference("VO_TUTR_Jaina_JainaProudmoore_Human_InGame_OnEnemyMinionsFrozen_01.prefab:6b11fcf5f31712c448b4b8f7a905e8d8");

	private static readonly AssetReference VO_TUTR_Jaina_JainaProudmoore_Human_InGame_Turn_09 = new AssetReference("VO_TUTR_Jaina_JainaProudmoore_Human_InGame_Turn_09.prefab:ca631833963cdd742a6d47b4126d36da");

	private static readonly AssetReference VO_TUTR_Jaina_JainaProudmoore_Human_InGame_Turn_10 = new AssetReference("VO_TUTR_Jaina_JainaProudmoore_Human_InGame_Turn_10.prefab:82c65ec3c30a36d4284ed4235ec36de3");

	private static readonly AssetReference VO_TUTR_Jaina_JainaProudmoore_Human_InGame_Turn_11 = new AssetReference("VO_TUTR_Jaina_JainaProudmoore_Human_InGame_Turn_11.prefab:2a6e5e4cea5ceed468c3f4abeecaa0dd");

	private static readonly AssetReference VO_TUTR_Jaina_JainaProudmoore_Human_InGame_Turn_12 = new AssetReference("VO_TUTR_Jaina_JainaProudmoore_Human_InGame_Turn_12.prefab:cc159e1b150070947b5bc1edcbd022bc");

	private static readonly AssetReference VO_TUTR_Jaina_JainaProudmoore_Human_InGame_VictoryPostExplosion_03 = new AssetReference("VO_TUTR_Jaina_JainaProudmoore_Human_InGame_VictoryPostExplosion_03.prefab:48e9583178159e84a969ce18f0975cf8");

	private static readonly AssetReference VO_TUTR_Jaina_JainaProudmoore_Human_InGame_VictoryPostExplosion_04 = new AssetReference("VO_TUTR_Jaina_JainaProudmoore_Human_InGame_VictoryPostExplosion_04.prefab:d07a4704cf46e3d44a413df91c714adc");

	private static readonly AssetReference VO_TUTR_Jaina_JainaProudmoore_Human_HELPER_EMPTYBOARD_01 = new AssetReference("VO_TUTR_Jaina_JainaProudmoore_Human_HELPER_EMPTYBOARD_01.prefab:bd8a20533f26bfa4286909ff4f55d531");

	private static readonly AssetReference VO_TUTR_Jaina_JainaProudmoore_Human_HELPER_ENEMY_FULLBOARD_01 = new AssetReference("VO_TUTR_Jaina_JainaProudmoore_Human_HELPER_ENEMY_FULLBOARD_01.prefab:04c4b69147ae6fb478b0a1ffe016989c");

	private static readonly AssetReference VO_TUTR_Jaina_JainaProudmoore_Human_HELPER_HEROFOCUS_LICHKING_01 = new AssetReference("VO_TUTR_Jaina_JainaProudmoore_Human_HELPER_HEROFOCUS_LICHKING_01.prefab:c30cfe53e1337d64b815356f9b5c19b1");

	private static readonly AssetReference VO_TUTR_LichKing_LichKing_Undead_InGame_Introduction_01 = new AssetReference("VO_TUTR_LichKing_LichKing_Undead_InGame_Introduction_01.prefab:af6311fb5434f79439c557acfd1b5b7b");

	private static readonly AssetReference VO_TUTR_LichKing_LichKing_Undead_InGame_Introduction_02 = new AssetReference("VO_TUTR_LichKing_LichKing_Undead_InGame_Introduction_02.prefab:e0e11ea28aa59174aa748ec45bc0ed8c");

	private static readonly AssetReference VO_TUTR_LichKing_LichKing_Undead_InGame_OnFrostmournePlayed_01 = new AssetReference("VO_TUTR_LichKing_LichKing_Undead_InGame_OnFrostmournePlayed_01.prefab:1aa36af45fd85014aba7b32bfbd1c381");

	private static readonly AssetReference VO_TUTR_LichKing_LichKing_Undead_InGame_OnFrostwyrmsFuryPlayed_01 = new AssetReference("VO_TUTR_LichKing_LichKing_Undead_InGame_OnFrostwyrmsFuryPlayed_01.prefab:30bd602506141f04281f7f304dfeac5a");

	private static readonly AssetReference VO_TUTR_LichKing_LichKing_Undead_InGame_Turn_01 = new AssetReference("VO_TUTR_LichKing_LichKing_Undead_InGame_Turn_01.prefab:468b866560330c9498b3575802760b6e");

	private static readonly AssetReference VO_TUTR_LichKing_LichKing_Undead_InGame_Turn_02 = new AssetReference("VO_TUTR_LichKing_LichKing_Undead_InGame_Turn_02.prefab:0b2264804f7109341ab937b616dd4248");

	private static readonly AssetReference VO_TUTR_LichKing_LichKing_Undead_InGame_Turn_03 = new AssetReference("VO_TUTR_LichKing_LichKing_Undead_InGame_Turn_03.prefab:cab342fce55e561488c3fc182efa8ae3");

	private static readonly AssetReference VO_TUTR_LichKing_LichKing_Undead_InGame_Turn_04 = new AssetReference("VO_TUTR_LichKing_LichKing_Undead_InGame_Turn_04.prefab:38bea4aa3060bb540ab041e2ee2212a4");

	private static readonly AssetReference VO_TUTR_LichKing_LichKing_Undead_InGame_Turn_FirstHeroPower_01 = new AssetReference("VO_TUTR_LichKing_LichKing_Undead_InGame_Turn_FirstHeroPower_01.prefab:46b9487410df724418dd50b69351c5b8");

	private static readonly AssetReference VO_TUTR_Jaina_JainaProudmoore_Human_InGame_Turn_06 = new AssetReference("VO_TUTR_Jaina_JainaProudmoore_Human_InGame_Turn_06.prefab:fedeef810fe1e1443bda03b615bdf8f7");

	private static readonly AssetReference VO_TUTR_Jaina_JainaProudmoore_Human_InGame_Turn_03 = new AssetReference("VO_TUTR_Jaina_JainaProudmoore_Human_InGame_Turn_03.prefab:4df02cba07b10d140be9eba133deff75");

	private static readonly AssetReference TutorialConfig = new AssetReference("TutorialConfig.asset:8fd8521aa30221946a920d274f64719f");

	private static readonly AssetReference VO_TUTR_LichKing_Death_01 = new AssetReference("VO_TUTR_LichKing_Death_01.prefab:0ebba7c17f8b53047b0d611369839f3a");

	private float[] m_waitTimesForEndTurnBounceArrow;

	private int m_timerIndex;

	private int m_gameTurn;

	private Tutorial_Config m_tutorialConfig;

	private Notification m_endTurnNotifier;

	private bool m_hasArcaneMissilesBeenPlayed;

	private bool m_hasArcaneExplosionBeenPlayed;

	private bool m_hasFrostNovaBeenPlayed;

	private bool m_hasBattlemageBeenPlayed;

	private bool m_hasFrostboltBeenPlayed;

	public override void PreloadAssets()
	{
		base.PreloadAssets();
		List<string> m_SoundFiles = new List<string>
		{
			VO_TUTR_Jaina_JainaProudmoore_Human_InGame_Introduction_06, VO_TUTR_Jaina_JainaProudmoore_Human_InGame_Introduction_07, VO_TUTR_Jaina_JainaProudmoore_Human_InGame_OnEnemyFullBoard_01, VO_TUTR_Jaina_JainaProudmoore_Human_InGame_OnEnemyMinionsFrozen_01, VO_TUTR_Jaina_JainaProudmoore_Human_InGame_Turn_09, VO_TUTR_Jaina_JainaProudmoore_Human_InGame_Turn_10, VO_TUTR_Jaina_JainaProudmoore_Human_InGame_Turn_11, VO_TUTR_Jaina_JainaProudmoore_Human_InGame_Turn_12, VO_TUTR_Jaina_JainaProudmoore_Human_InGame_VictoryPostExplosion_03, VO_TUTR_Jaina_JainaProudmoore_Human_InGame_VictoryPostExplosion_04,
			VO_TUTR_Jaina_JainaProudmoore_Human_HELPER_EMPTYBOARD_01, VO_TUTR_Jaina_JainaProudmoore_Human_HELPER_ENEMY_FULLBOARD_01, VO_TUTR_Jaina_JainaProudmoore_Human_HELPER_HEROFOCUS_LICHKING_01, VO_TUTR_LichKing_LichKing_Undead_InGame_Introduction_01, VO_TUTR_LichKing_LichKing_Undead_InGame_Introduction_02, VO_TUTR_LichKing_LichKing_Undead_InGame_OnFrostmournePlayed_01, VO_TUTR_LichKing_LichKing_Undead_InGame_OnFrostwyrmsFuryPlayed_01, VO_TUTR_LichKing_LichKing_Undead_InGame_Turn_01, VO_TUTR_LichKing_LichKing_Undead_InGame_Turn_02, VO_TUTR_LichKing_LichKing_Undead_InGame_Turn_03,
			VO_TUTR_LichKing_LichKing_Undead_InGame_Turn_04, VO_TUTR_LichKing_LichKing_Undead_InGame_Turn_FirstHeroPower_01, VO_TUTR_Jaina_JainaProudmoore_Human_InGame_Turn_06, VO_TUTR_Jaina_JainaProudmoore_Human_InGame_Turn_03, VO_TUTR_LichKing_Death_01
		};
		SetBossVOLines(m_SoundFiles);
		foreach (string soundFile in m_SoundFiles)
		{
			PreloadSound(soundFile);
		}
		AssetLoader.Get()?.LoadAsset<Tutorial_Config>(TutorialConfig, OnTutorialConfigLoaded, AssetLoadingOptions.None);
	}

	private void OnTutorialConfigLoaded(AssetReference assetRef, AssetHandle<Tutorial_Config> asset, object assetId)
	{
		if (asset == null)
		{
			Debug.LogError("Tutorial_Fight_003: could not load Tutorial Config file");
		}
		else
		{
			m_tutorialConfig = asset;
		}
	}

	public override void OnCreateGame()
	{
		base.OnCreateGame();
		EndTurnButton endTurn = EndTurnButton.Get();
		if (endTurn != null)
		{
			m_waitTimesForEndTurnBounceArrow = new float[4] { 5f, 8f, 10f, 10f };
			endTurn.OnButtonStateChanged += OnEndTurnButtonStateChange;
		}
		BoardStandardGame board = BoardStandardGame.Get();
		if (board != null)
		{
			CorpseCounter[] corpseCounterObjects = board.GetComponentsInChildren<CorpseCounter>();
			for (int i = 0; i < corpseCounterObjects.Length; i++)
			{
				corpseCounterObjects[i].gameObject.SetActive(value: false);
			}
		}
	}

	public override bool NotifyOfEndTurnButtonPushed()
	{
		m_gameTurn = GameState.Get().GetTurn();
		Network.Options options = GameState.Get().GetOptionsPacket();
		if (options != null && !options.HasValidOption())
		{
			NotificationManager.Get().DestroyAllNotificationsNowWithNoAnim();
			NotificationManager.Get().DestroyAllArrows();
			return true;
		}
		if (m_endTurnNotifier != null)
		{
			NotificationManager.Get().DestroyNotificationNowWithNoAnim(m_endTurnNotifier);
		}
		Vector3 endTurnPos = EndTurnButton.Get().transform.position;
		Vector3 popUpPos = new Vector3(endTurnPos.x - 3f, endTurnPos.y, endTurnPos.z);
		string textID = "TUTORIAL_NO_ENDTURN_ATK";
		if (!GameState.Get().GetFriendlySidePlayer().HasReadyAttackers())
		{
			textID = "TUTORIAL_NO_ENDTURN";
		}
		if (m_gameTurn == 6 && m_hasArcaneMissilesBeenPlayed && !GameState.Get().GetFriendlySidePlayer().HasReadyAttackers())
		{
			textID = "TUTORIAL_NO_ENDTURN_HP";
		}
		if (m_gameTurn == 8 && m_hasArcaneExplosionBeenPlayed && !GameState.Get().GetFriendlySidePlayer().HasReadyAttackers())
		{
			textID = "TUTORIAL_NO_ENDTURN_HP";
		}
		if (m_gameTurn == 12 && m_hasFrostNovaBeenPlayed)
		{
			textID = "TUTORIAL_NO_ENDTURN_HP";
		}
		if (m_gameTurn == 16 && m_hasBattlemageBeenPlayed && m_hasFrostboltBeenPlayed)
		{
			textID = "TUTORIAL_NO_ENDTURN_HP";
		}
		m_endTurnNotifier = NotificationManager.Get().CreatePopupText(UserAttentionBlocker.NONE, popUpPos, TutorialEntity.GetTextScale(), GameStrings.Get(textID));
		NotificationManager.Get().DestroyNotification(m_endTurnNotifier, 2.5f);
		return false;
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
		case 514:
			yield return MissionPlayVO(friendlyActor, VO_TUTR_Jaina_JainaProudmoore_Human_InGame_Introduction_06);
			yield return MissionPlayVO(enemyActor, VO_TUTR_LichKing_LichKing_Undead_InGame_Introduction_01);
			yield return MissionPlayVO(enemyActor, VO_TUTR_LichKing_LichKing_Undead_InGame_Introduction_02);
			yield return MissionPlayVO(friendlyActor, VO_TUTR_Jaina_JainaProudmoore_Human_InGame_Introduction_07);
			break;
		case 650:
		{
			GameState.Get().SetBusy(busy: true);
			yield return new WaitForSeconds(1.5f);
			Vector3 weaponPosition = GameState.Get().GetOpposingSidePlayer().GetHeroCard()
				.transform.position;
			Vector3 popUpPos = new Vector3(weaponPosition.x - 1.55f, weaponPosition.y, weaponPosition.z - 2.721f);
			Notification weaponHelp = NotificationManager.Get().CreatePopupText(UserAttentionBlocker.NONE, popUpPos, TutorialEntity.GetTextScale(), GameStrings.Get("TUTR_HELPER_Fight_01_PlayWeapon"));
			weaponHelp.ShowPopUpArrow(Notification.PopUpArrowDirection.Up);
			NotificationManager.Get().DestroyNotification(weaponHelp, 5f);
			yield return new WaitForSeconds(5f);
			GameState.Get().SetBusy(busy: false);
			break;
		}
		case 651:
			m_hasArcaneMissilesBeenPlayed = true;
			break;
		case 652:
			m_hasArcaneExplosionBeenPlayed = true;
			break;
		case 653:
			m_hasFrostNovaBeenPlayed = true;
			break;
		case 654:
			m_hasBattlemageBeenPlayed = true;
			break;
		case 655:
			m_hasFrostboltBeenPlayed = true;
			break;
		case 750:
			yield return MissionPlaySound(VO_TUTR_LichKing_Death_01);
			break;
		case 751:
		{
			string audioAsset = GetGameOptions().GetStringOption(GameEntityOption.VICTORY_AUDIO_PATH);
			if (!string.IsNullOrEmpty(audioAsset))
			{
				SoundManager.Get().LoadAndPlay(audioAsset);
			}
			break;
		}
		default:
			yield return base.HandleMissionEventWithTiming(missionEvent);
			break;
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
			GameState.Get().SetBusy(busy: true);
			yield return new WaitForSeconds(1.5f);
			yield return MissionPlayVO(enemyActor, VO_TUTR_LichKing_LichKing_Undead_InGame_Turn_FirstHeroPower_01);
			GameState.Get().SetBusy(busy: false);
			break;
		case 4:
			yield return new WaitForSeconds(1f);
			yield return MissionPlayVO(friendlyActor, VO_TUTR_Jaina_JainaProudmoore_Human_HELPER_EMPTYBOARD_01);
			break;
		case 6:
			yield return new WaitForSeconds(1f);
			yield return MissionPlayVO(friendlyActor, VO_TUTR_Jaina_JainaProudmoore_Human_InGame_Turn_09);
			break;
		case 7:
			GameState.Get().SetBusy(busy: true);
			yield return new WaitForSeconds(1f);
			yield return MissionPlayVO(enemyActor, VO_TUTR_LichKing_LichKing_Undead_InGame_Turn_01);
			GameState.Get().SetBusy(busy: false);
			break;
		case 8:
			GameState.Get().SetBusy(busy: true);
			yield return new WaitForSeconds(1f);
			yield return MissionPlayVO(friendlyActor, VO_TUTR_Jaina_JainaProudmoore_Human_HELPER_ENEMY_FULLBOARD_01);
			GameState.Get().SetBusy(busy: false);
			break;
		case 9:
			GameState.Get().SetBusy(busy: true);
			yield return new WaitForSeconds(1f);
			yield return MissionPlayVO(enemyActor, VO_TUTR_LichKing_LichKing_Undead_InGame_OnFrostmournePlayed_01);
			GameState.Get().SetBusy(busy: false);
			break;
		case 10:
			GameState.Get().SetBusy(busy: true);
			yield return new WaitForSeconds(1f);
			yield return MissionPlayVO(friendlyActor, VO_TUTR_Jaina_JainaProudmoore_Human_InGame_Turn_11);
			GameState.Get().SetBusy(busy: false);
			break;
		case 11:
			GameState.Get().SetBusy(busy: true);
			yield return new WaitForSeconds(1f);
			yield return MissionPlayVO(enemyActor, VO_TUTR_LichKing_LichKing_Undead_InGame_Turn_02);
			GameState.Get().SetBusy(busy: false);
			break;
		case 12:
			yield return new WaitForSeconds(1f);
			yield return MissionPlayVO(friendlyActor, VO_TUTR_Jaina_JainaProudmoore_Human_InGame_Turn_06);
			break;
		case 13:
			GameState.Get().SetBusy(busy: true);
			yield return new WaitForSeconds(1f);
			yield return MissionPlayVO(enemyActor, VO_TUTR_LichKing_LichKing_Undead_InGame_Turn_03);
			GameState.Get().SetBusy(busy: false);
			break;
		case 14:
			yield return new WaitForSeconds(1f);
			yield return MissionPlayVO(friendlyActor, VO_TUTR_Jaina_JainaProudmoore_Human_InGame_OnEnemyFullBoard_01);
			break;
		case 15:
			GameState.Get().SetBusy(busy: true);
			yield return new WaitForSeconds(1f);
			yield return MissionPlayVO(enemyActor, VO_TUTR_LichKing_LichKing_Undead_InGame_OnFrostwyrmsFuryPlayed_01);
			GameState.Get().SetBusy(busy: false);
			break;
		case 16:
			GameState.Get().SetBusy(busy: true);
			yield return new WaitForSeconds(1f);
			yield return MissionPlayVO(friendlyActor, VO_TUTR_Jaina_JainaProudmoore_Human_InGame_Turn_12);
			GameState.Get().SetBusy(busy: false);
			break;
		case 17:
			GameState.Get().SetBusy(busy: true);
			yield return new WaitForSeconds(1f);
			yield return MissionPlayVO(enemyActor, VO_TUTR_LichKing_LichKing_Undead_InGame_Turn_04);
			GameState.Get().SetBusy(busy: false);
			break;
		case 18:
			GameState.Get().SetBusy(busy: true);
			yield return new WaitForSeconds(1f);
			yield return MissionPlayVO(friendlyActor, VO_TUTR_Jaina_JainaProudmoore_Human_InGame_Turn_03);
			GameState.Get().SetBusy(busy: false);
			break;
		}
	}

	public override void NotifyOfGameOver(TAG_PLAYSTATE gameResult)
	{
		base.NotifyOfGameOver(gameResult);
		EndTurnButton endTurn = EndTurnButton.Get();
		if (endTurn != null)
		{
			endTurn.OnButtonStateChanged -= OnEndTurnButtonStateChange;
		}
		if (gameResult == TAG_PLAYSTATE.WON)
		{
			GameUtils.SetTutorialProgress(TutorialProgress.LICH_KING_COMPLETE, tutorialComplete: true);
			if (Network.ShouldBeConnectedToAurora() && Network.IsLoggedIn() && !GameMgr.Get().IsSpectator())
			{
				BnetPresenceMgr.Get().SetGameField(15u, 1);
			}
		}
	}

	private void OnEndTurnButtonStateChange(ActorStateType state)
	{
		EndTurnButton endTurn = EndTurnButton.Get();
		switch (state)
		{
		case ActorStateType.ENDTURN_NO_MORE_PLAYS:
			if (m_tutorialConfig != null)
			{
				endTurn.ShowEndTurnBouncingArrowButtonAfterWait(m_waitTimesForEndTurnBounceArrow[m_timerIndex], m_tutorialConfig.m_EndTurnButtonArrowOffset);
				if (m_timerIndex < m_waitTimesForEndTurnBounceArrow.Length - 1)
				{
					m_timerIndex++;
				}
			}
			break;
		case ActorStateType.ENDTURN_NMP_2_WAITING:
			endTurn.HideEndTurnBouncingArrow();
			break;
		}
	}
}
