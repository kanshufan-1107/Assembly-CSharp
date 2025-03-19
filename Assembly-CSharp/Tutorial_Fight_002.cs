using System.Collections;
using System.Collections.Generic;
using Blizzard.T5.AssetManager;
using UnityEngine;

public class Tutorial_Fight_002 : Tutorial_Dungeon
{
	private static readonly AssetReference VO_TUTR_Garrosh_GarroshHellscream_Orc_InGame_Introduction_01 = new AssetReference("VO_TUTR_Garrosh_GarroshHellscream_Orc_InGame_Introduction_01.prefab:d8e727096c16c8842aa8d208a771f04d");

	private static readonly AssetReference VO_TUTR_Garrosh_GarroshHellscream_Orc_InGame_Introduction_02 = new AssetReference("VO_TUTR_Garrosh_GarroshHellscream_Orc_InGame_Introduction_02.prefab:a0af0313814ec4041b18c88c4a2463e4");

	private static readonly AssetReference VO_TUTR_Garrosh_GarroshHellscream_Orc_InGame_OnArmorUp_01 = new AssetReference("VO_TUTR_Garrosh_GarroshHellscream_Orc_InGame_OnArmorUp_01.prefab:dd43591a9aeab22488ea7754d74b15b0");

	private static readonly AssetReference VO_TUTR_Garrosh_GarroshHellscream_Orc_InGame_OnFieryWarAxePlayed_01 = new AssetReference("VO_TUTR_Garrosh_GarroshHellscream_Orc_InGame_OnFieryWarAxePlayed_01.prefab:0adb94ad8429e0246845e898a44cd27d");

	private static readonly AssetReference VO_TUTR_Garrosh_GarroshHellscream_Orc_InGame_OnSpellPlayed_01 = new AssetReference("VO_TUTR_Garrosh_GarroshHellscream_Orc_InGame_OnSpellPlayed_01.prefab:d2233cbf60d5542459797429028e855a");

	private static readonly AssetReference VO_TUTR_Garrosh_GarroshHellscream_Orc_InGame_Turn_01 = new AssetReference("VO_TUTR_Garrosh_GarroshHellscream_Orc_InGame_Turn_01.prefab:36f2d21afc795aa4c8f7c0d8abd8f75c");

	private static readonly AssetReference VO_TUTR_Garrosh_GarroshHellscream_Orc_InGame_Turn_02 = new AssetReference("VO_TUTR_Garrosh_GarroshHellscream_Orc_InGame_Turn_02.prefab:6dc93055b87df1248979f9c48786e54d");

	private static readonly AssetReference VO_TUTR_Garrosh_GarroshHellscream_Orc_InGame_Turn_03 = new AssetReference("VO_TUTR_Garrosh_GarroshHellscream_Orc_InGame_Turn_03.prefab:106e25305b5f94640977d10148ed5414");

	private static readonly AssetReference VO_TUTR_Garrosh_GarroshHellscream_Orc_InGame_Turn_04 = new AssetReference("VO_TUTR_Garrosh_GarroshHellscream_Orc_InGame_Turn_04.prefab:0b73633f756928d45814ca69d58af711");

	private static readonly AssetReference VO_TUTR_Garrosh_GarroshHellscream_Orc_InGame_Turn_05 = new AssetReference("VO_TUTR_Garrosh_GarroshHellscream_Orc_InGame_Turn_05.prefab:36b814b3ae199fe4da9cacdd2b44f81c");

	private static readonly AssetReference VO_TUTR_Jaina_JainaProudmoore_Human_InGame_Introduction_04 = new AssetReference("VO_TUTR_Jaina_JainaProudmoore_Human_InGame_Introduction_04.prefab:c3f143c3949dbc041b6c296f4d5e4888");

	private static readonly AssetReference VO_TUTR_Jaina_JainaProudmoore_Human_InGame_Introduction_05 = new AssetReference("VO_TUTR_Jaina_JainaProudmoore_Human_InGame_Introduction_05.prefab:fba2a3a78b109404ba8bc910948bb0af");

	private static readonly AssetReference VO_TUTR_Jaina_JainaProudmoore_Human_InGame_OnDrawFireball_01 = new AssetReference("VO_TUTR_Jaina_JainaProudmoore_Human_InGame_OnDrawFireball_01.prefab:8e33cb700034c6045a55c3fef2cf2d8a");

	private static readonly AssetReference VO_TUTR_Jaina_JainaProudmoore_Human_InGame_OnDrawLegendary_01 = new AssetReference("VO_TUTR_Jaina_JainaProudmoore_Human_InGame_OnDrawLegendary_01.prefab:8571d9aabd32112428c7573f74b73d22");

	private static readonly AssetReference VO_TUTR_Jaina_JainaProudmoore_Human_InGame_OnNoTauntMinions_01 = new AssetReference("VO_TUTR_Jaina_JainaProudmoore_Human_InGame_OnNoTauntMinions_01.prefab:0601e86a992147f479f174dd61a8910c");

	private static readonly AssetReference VO_TUTR_Jaina_JainaProudmoore_Human_InGame_Turn_04 = new AssetReference("VO_TUTR_Jaina_JainaProudmoore_Human_InGame_Turn_04.prefab:19b68a41451750a44903d41affe74236");

	private static readonly AssetReference VO_TUTR_Jaina_JainaProudmoore_Human_InGame_Turn_05 = new AssetReference("VO_TUTR_Jaina_JainaProudmoore_Human_InGame_Turn_05.prefab:7efe5106c254631408ae77be2bca6c77");

	private static readonly AssetReference VO_TUTR_Jaina_JainaProudmoore_Human_InGame_Turn_06 = new AssetReference("VO_TUTR_Jaina_JainaProudmoore_Human_InGame_Turn_06.prefab:fedeef810fe1e1443bda03b615bdf8f7");

	private static readonly AssetReference VO_TUTR_Jaina_JainaProudmoore_Human_InGame_Turn_07 = new AssetReference("VO_TUTR_Jaina_JainaProudmoore_Human_InGame_Turn_07.prefab:933d9d183d4fc9343a7998e0ea25ea9d");

	private static readonly AssetReference VO_TUTR_Jaina_JainaProudmoore_Human_InGame_Turn_08 = new AssetReference("VO_TUTR_Jaina_JainaProudmoore_Human_InGame_Turn_08.prefab:507c8a6624d6e3349a27c4d4d5ab9ec1");

	private static readonly AssetReference VO_TUTR_Jaina_JainaProudmoore_Human_InGame_VictoryPostExplosion_02 = new AssetReference("VO_TUTR_Jaina_JainaProudmoore_Human_InGame_VictoryPostExplosion_02.prefab:84707b41d6c2f00439dbb41b759cadd5");

	private static readonly AssetReference VO_TUTR_Jaina_JainaProudmoore_Human_InGame_OnDrawAOESpell_01 = new AssetReference("VO_TUTR_Jaina_JainaProudmoore_Human_InGame_OnDrawAOESpell_01.prefab:b3ddc8ab3e27b264995dd329a9ccee67");

	private static readonly AssetReference VO_TUTORIAL_02_JAINA_08_22 = new AssetReference("VO_TUTORIAL_02_JAINA_08_22.prefab:52cd86a7a20daeb4b8d1f3fd2647e9ea");

	private static readonly AssetReference VO_TUTR_Jaina_JainaProudmoore_Human_HELPER_ENEMY_SPECIALMINION_01 = new AssetReference("VO_TUTR_Jaina_JainaProudmoore_Human_HELPER_ENEMY_SPECIALMINION_01.prefab:80a73d2ae948e21408f11fceb629e6ca");

	private static readonly AssetReference VO_TUTORIAL_02_JAINA_07_21 = new AssetReference("VO_TUTORIAL_02_JAINA_07_21.prefab:085c10eb9049776418135df8f6b71046");

	private static readonly AssetReference VO_TUTR_Jaina_JainaProudmoore_Human_HELPER_EMPTYBOARD2_01 = new AssetReference("VO_TUTR_Jaina_JainaProudmoore_Human_HELPER_EMPTYBOARD2_01.prefab:30d1acd0b758e5044b3afadb597d7dc0");

	private static readonly AssetReference TutorialConfig = new AssetReference("TutorialConfig.asset:8fd8521aa30221946a920d274f64719f");

	private static readonly AssetReference VO_TUTR_Garrosh_Death_01 = new AssetReference("VO_TUTR_Garrosh_Death_01.prefab:9a69b2aa3f7b1084f93de7a24253532e");

	private float[] m_waitTimesForEndTurnBounceArrow;

	private int m_timerIndex;

	private int m_gameTurn;

	private Notification m_manaNotification;

	private Notification m_manaRefreshNotification;

	private Notification handBounceArrow;

	private HashSet<string> m_playedLines = new HashSet<string>();

	private Tutorial_Config m_tutorialConfig;

	private bool m_hasFrostwolfBeenPlayed;

	private bool m_hasTauntBeenPlayed;

	private bool m_hasManaWyrmBeenPlayed;

	private Notification m_endTurnNotifier;

	private bool m_createdTutorialNotification;

	private bool m_hasSuspendedTutorialNotification;

	private bool m_hasSuspendedHelpNotification;

	private Entity m_entityinTargetMode;

	public override void PreloadAssets()
	{
		base.PreloadAssets();
		List<string> m_SoundFiles = new List<string>
		{
			VO_TUTR_Garrosh_GarroshHellscream_Orc_InGame_Introduction_01, VO_TUTR_Garrosh_GarroshHellscream_Orc_InGame_Introduction_02, VO_TUTR_Garrosh_GarroshHellscream_Orc_InGame_OnArmorUp_01, VO_TUTR_Garrosh_GarroshHellscream_Orc_InGame_OnFieryWarAxePlayed_01, VO_TUTR_Garrosh_GarroshHellscream_Orc_InGame_OnSpellPlayed_01, VO_TUTR_Garrosh_GarroshHellscream_Orc_InGame_Turn_01, VO_TUTR_Garrosh_GarroshHellscream_Orc_InGame_Turn_02, VO_TUTR_Garrosh_GarroshHellscream_Orc_InGame_Turn_03, VO_TUTR_Garrosh_GarroshHellscream_Orc_InGame_Turn_04, VO_TUTR_Garrosh_GarroshHellscream_Orc_InGame_Turn_05,
			VO_TUTR_Jaina_JainaProudmoore_Human_InGame_Introduction_04, VO_TUTR_Jaina_JainaProudmoore_Human_InGame_Introduction_05, VO_TUTR_Jaina_JainaProudmoore_Human_InGame_OnDrawFireball_01, VO_TUTR_Jaina_JainaProudmoore_Human_InGame_OnDrawLegendary_01, VO_TUTR_Jaina_JainaProudmoore_Human_InGame_OnNoTauntMinions_01, VO_TUTR_Jaina_JainaProudmoore_Human_InGame_Turn_04, VO_TUTR_Jaina_JainaProudmoore_Human_InGame_Turn_05, VO_TUTR_Jaina_JainaProudmoore_Human_InGame_Turn_06, VO_TUTR_Jaina_JainaProudmoore_Human_InGame_Turn_07, VO_TUTR_Jaina_JainaProudmoore_Human_InGame_Turn_08,
			VO_TUTR_Jaina_JainaProudmoore_Human_InGame_VictoryPostExplosion_02, VO_TUTR_Jaina_JainaProudmoore_Human_InGame_OnDrawAOESpell_01, VO_TUTORIAL_02_JAINA_08_22, VO_TUTR_Jaina_JainaProudmoore_Human_HELPER_ENEMY_SPECIALMINION_01, VO_TUTORIAL_02_JAINA_07_21, VO_TUTR_Jaina_JainaProudmoore_Human_HELPER_EMPTYBOARD2_01, VO_TUTR_Garrosh_Death_01
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
			Debug.LogError(GetType().Name + " could not load Tutorial Config file");
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
			m_waitTimesForEndTurnBounceArrow = new float[4] { 3f, 5f, 8f, 10f };
			endTurn.OnButtonStateChanged += OnEndTurnButtonStateChange;
		}
		BnetBar.SkipTutorialSelected += OnSkipTutorialButtonSelected;
		BnetBarMenuButton.SettingsMenuSelectedEvent += OnSettingMenuToggled;
		HistoryManager.HistoryBarHoverEvent += OnHistoryBarHover;
	}

	private void OnHistoryBarHover(bool isOverTile)
	{
		ToggleNotificationSuspension(isOverTile);
	}

	private void OnSkipTutorialButtonSelected(bool active)
	{
		ToggleNotificationSuspension(active);
	}

	private void OnSettingMenuToggled(bool enabled)
	{
		ToggleNotificationSuspension(enabled);
	}

	private void ToggleNotificationSuspension(bool active)
	{
		if (active)
		{
			if (!m_hasSuspendedTutorialNotification && m_createdTutorialNotification)
			{
				SuspendTutorialNotification();
			}
			if (!m_hasSuspendedHelpNotification)
			{
				SuspendHelpNotification();
			}
		}
		else
		{
			if (m_hasSuspendedTutorialNotification && m_createdTutorialNotification)
			{
				ShowSuspendedTutorialNotification();
			}
			if (m_hasSuspendedHelpNotification)
			{
				ShowSuspendedHelpNotification();
			}
		}
	}

	public override void NotifyOfCardMousedOver(Entity mousedOverEntity)
	{
		base.NotifyOfCardMousedOver(mousedOverEntity);
		NotificationManager.Get().DestroyNotification(handBounceArrow, 0f);
		handBounceArrow = null;
		ToggleNotificationSuspension(active: true);
		if (ShouldShowArrowOnCardInHand(mousedOverEntity))
		{
			NotificationManager.Get().DestroyAllArrows();
		}
	}

	public override void NotifyOfCardMousedOff(Entity mousedOffEntity)
	{
		if (mousedOffEntity.GetZone() == TAG_ZONE.HAND)
		{
			ToggleNotificationSuspension(active: false);
		}
	}

	public override void NotifyOfCardGrabbed(Entity entity)
	{
		if (GetTag(GAME_TAG.TURN) == 2 || entity.GetCardId() == "TUTR_NEW1_012t2")
		{
			BoardTutorial.Get().EnableHighlight(enable: true);
		}
	}

	public override void NotifyOfCardDropped(Entity entity)
	{
		if (GetTag(GAME_TAG.TURN) == 2 || entity.GetCardId() == "TUTR_NEW1_012t2")
		{
			BoardTutorial.Get().EnableHighlight(enable: false);
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
			GameUtils.SetTutorialProgress(TutorialProgress.GARROSH_COMPLETE, tutorialComplete: false);
		}
		NotificationManager.Get().DestroyAllNotificationsNowWithNoAnim();
		HideTutorialNotification();
		BnetBar.SkipTutorialSelected -= OnSkipTutorialButtonSelected;
		BnetBarMenuButton.SettingsMenuSelectedEvent -= OnSettingMenuToggled;
		HistoryManager.HistoryBarHoverEvent -= OnHistoryBarHover;
	}

	private bool ShouldShowArrowOnCardInHand(Entity entity)
	{
		if (entity.GetZone() != TAG_ZONE.HAND)
		{
			return false;
		}
		switch (GetTag(GAME_TAG.TURN))
		{
		case 2:
			return true;
		case 4:
			if (GameState.Get().GetFriendlySidePlayer().GetBattlefieldZone()
				.GetCards()
				.Count == 0)
			{
				return true;
			}
			break;
		}
		return false;
	}

	public override bool NotifyOfCardTooltipDisplayShow(Card card)
	{
		if (GameState.Get().IsGameOver())
		{
			return false;
		}
		ToggleNotificationSuspension(active: true);
		return true;
	}

	public override bool NotifyOfTooltipDisplay(TooltipZone tooltip)
	{
		ToggleNotificationSuspension(active: true);
		return false;
	}

	public override void NotifyOfTooltipHide(TooltipZone tooltip)
	{
		ToggleNotificationSuspension(active: false);
	}

	public override void NotifyOfCardTooltipDisplayHide(Card card)
	{
		ToggleNotificationSuspension(active: false);
	}

	public override bool NotifyOfEndTurnButtonPushed()
	{
		m_gameTurn = GameState.Get().GetTurn();
		Network.Options options = GameState.Get().GetOptionsPacket();
		if (options != null && !options.HasValidOption())
		{
			NotificationManager.Get().DestroyAllNotificationsNowWithNoAnim();
			NotificationManager.Get().DestroyAllArrows();
			HideTutorialNotification();
			return true;
		}
		if (m_endTurnNotifier != null)
		{
			NotificationManager.Get().DestroyNotificationNowWithNoAnim(m_endTurnNotifier);
		}
		Vector3 endTurnPos = EndTurnButton.Get().transform.position;
		Vector3 popUpPos = new Vector3(endTurnPos.x - 3f, endTurnPos.y, endTurnPos.z);
		if (m_tutorialConfig != null)
		{
			popUpPos = m_tutorialConfig.m_endTurnWarningPositionOffset;
		}
		string textID = "TUTORIAL_NO_ENDTURN_ATK";
		if (!GameState.Get().GetFriendlySidePlayer().HasReadyAttackers())
		{
			textID = "TUTORIAL_NO_ENDTURN";
		}
		if (m_gameTurn == 5 && m_hasManaWyrmBeenPlayed && !GameState.Get().GetFriendlySidePlayer().HasReadyAttackers())
		{
			textID = "TUTORIAL_NO_ENDTURN_HP";
		}
		m_endTurnNotifier = NotificationManager.Get().CreatePopupText(UserAttentionBlocker.NONE, popUpPos, TutorialEntity.GetTextScale(), GameStrings.Get(textID), convertLegacyPosition: false);
		NotificationManager.Get().DestroyNotification(m_endTurnNotifier, 2.5f);
		return false;
	}

	public override void NotifyOfMinionPlayed(Entity minion)
	{
		if (m_manaNotification != null)
		{
			NotificationManager.Get().DestroyNotification(m_manaNotification, 0f);
		}
		if (m_manaRefreshNotification != null)
		{
			NotificationManager.Get().DestroyNotification(m_manaRefreshNotification, 0f);
		}
	}

	private void ShowTutorialNotification(Vector3 tooltipPosition, Vector3 tooltipPostionMobile, string gameStringKey, NotificationManager.TutorialPopupType type = NotificationManager.TutorialPopupType.IMPORTANT)
	{
		NotificationManager notificationManager = NotificationManager.Get();
		if (notificationManager == null)
		{
			Log.Gameplay.PrintError(GetType().Name + ":notificationManager is null");
			return;
		}
		Vector3 tooltipScale = TutorialEntity.GetTextScale();
		Vector3 tooltipScaleMobile = TutorialEntity.GetTextScale();
		if (m_tutorialConfig != null)
		{
			tooltipScale = m_tutorialConfig.m_TutorialNotificationScale;
			tooltipScaleMobile = m_tutorialConfig.m_TutorialNotificationScaleMobile;
		}
		if ((bool)UniversalInputManager.UsePhoneUI)
		{
			m_createdTutorialNotification = notificationManager.ShowTutorialNotification(UserAttentionBlocker.NONE, tooltipPostionMobile, tooltipScaleMobile, GameStrings.Get(gameStringKey), convertLegacyPosition: false, type);
		}
		else
		{
			m_createdTutorialNotification = notificationManager.ShowTutorialNotification(UserAttentionBlocker.NONE, tooltipPosition, tooltipScale, GameStrings.Get(gameStringKey), convertLegacyPosition: false, type);
		}
	}

	private void ShowTutorialNotification(Vector3 tooltipPosition, Vector3 tooltipPostionMobile, string gameStringKey, Notification.PopUpArrowDirection arrowDirection, NotificationManager.TutorialPopupType type = NotificationManager.TutorialPopupType.IMPORTANT)
	{
		NotificationManager notificationManager = NotificationManager.Get();
		if (notificationManager == null)
		{
			Log.Gameplay.PrintError("Tutorial_001: notificationManager is null");
			return;
		}
		Vector3 tooltipScale = TutorialEntity.GetTextScale();
		Vector3 tooltipScaleMobile = TutorialEntity.GetTextScale();
		if (m_tutorialConfig != null)
		{
			tooltipScale = m_tutorialConfig.m_TutorialNotificationScale;
			tooltipScaleMobile = m_tutorialConfig.m_TutorialNotificationScaleMobile;
		}
		if ((bool)UniversalInputManager.UsePhoneUI)
		{
			m_createdTutorialNotification = notificationManager.ShowTutorialNotification(UserAttentionBlocker.NONE, tooltipPostionMobile, tooltipScaleMobile, GameStrings.Get(gameStringKey), convertLegacyPosition: false, type, arrowDirection);
		}
		else
		{
			m_createdTutorialNotification = notificationManager.ShowTutorialNotification(UserAttentionBlocker.NONE, tooltipPosition, tooltipScale, GameStrings.Get(gameStringKey), convertLegacyPosition: false, type, arrowDirection);
		}
	}

	private void HideTutorialNotification(float delay = 0f, float animationDuration = -1f)
	{
		NotificationManager notificationManager = NotificationManager.Get();
		if (notificationManager == null)
		{
			Log.Gameplay.PrintError("Tutorial_002: notificationManager is null");
			return;
		}
		if (delay > 0f)
		{
			notificationManager.HideTutorialNotification(delay, animationDuration);
		}
		else
		{
			notificationManager.HideTutorialNotification(animationDuration);
		}
		m_createdTutorialNotification = false;
	}

	private void SuspendTutorialNotification()
	{
		NotificationManager notificationManager = NotificationManager.Get();
		if (notificationManager != null)
		{
			notificationManager.HideTutorialNotification(0f);
		}
		m_hasSuspendedTutorialNotification = true;
	}

	private void SuspendHelpNotification()
	{
		if (m_manaNotification != null)
		{
			m_manaNotification.gameObject.SetActive(value: false);
		}
		if (m_manaRefreshNotification != null)
		{
			m_manaRefreshNotification.gameObject.SetActive(value: false);
		}
		m_hasSuspendedHelpNotification = true;
	}

	private void ShowSuspendedHelpNotification()
	{
		if (m_manaNotification != null)
		{
			m_manaNotification.gameObject.SetActive(value: true);
		}
		if (m_manaRefreshNotification != null)
		{
			m_manaRefreshNotification.gameObject.SetActive(value: true);
		}
		m_hasSuspendedHelpNotification = false;
	}

	private void ShowSuspendedTutorialNotification()
	{
		NotificationManager notificationManager = NotificationManager.Get();
		if (notificationManager == null)
		{
			Debug.LogError(GetType().Name + " could not load Tutorial Config file");
			return;
		}
		Vector3 notificationScale = TutorialEntity.GetTextScale();
		Vector3 notificationScaleMobile = TutorialEntity.GetTextScale();
		if (m_tutorialConfig != null)
		{
			notificationScale = m_tutorialConfig.m_TutorialNotificationScale;
			notificationScaleMobile = m_tutorialConfig.m_TutorialNotificationScaleMobile;
		}
		if ((bool)UniversalInputManager.UsePhoneUI)
		{
			m_createdTutorialNotification = notificationManager.ShowExistingTutorialNotification(notificationScaleMobile);
		}
		else
		{
			m_createdTutorialNotification = notificationManager.ShowExistingTutorialNotification(notificationScale);
		}
		m_hasSuspendedTutorialNotification = false;
	}

	protected override IEnumerator HandleMissionEventWithTiming(int missionEvent)
	{
		while (m_enemySpeaking)
		{
			yield return null;
		}
		if (m_tutorialConfig == null)
		{
			yield break;
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
			yield return MissionPlayVO(friendlyActor, VO_TUTR_Jaina_JainaProudmoore_Human_InGame_Introduction_04);
			yield return MissionPlayVO(enemyActor, VO_TUTR_Garrosh_GarroshHellscream_Orc_InGame_Introduction_01);
			break;
		case 650:
		{
			GameState.Get().SetBusy(busy: true);
			Vector3 weaponPosition = GameState.Get().GetOpposingSidePlayer().GetHeroCard()
				.transform.position;
			Vector3 popUpPos = new Vector3(weaponPosition.x - 1.55f, weaponPosition.y, weaponPosition.z - 2.721f);
			Notification weaponHelp = NotificationManager.Get().CreatePopupText(UserAttentionBlocker.NONE, popUpPos, TutorialEntity.GetTextScale(), GameStrings.Get("TUTR_HELPER_Fight_01_PlayWeapon"));
			weaponHelp.ShowPopUpArrow(Notification.PopUpArrowDirection.Up);
			NotificationManager.Get().DestroyNotification(weaponHelp, 3.5f);
			yield return new WaitForSeconds(3.5f);
			GameState.Get().SetBusy(busy: false);
			break;
		}
		case 651:
		{
			ShowTutorialNotification(m_tutorialConfig.m_HeroPowerTutorialPositionOffset, m_tutorialConfig.m_HeroPowerTutorialPositionOffsetMobile, "TUTR_HELPER_Fight_01_HeroPower", Notification.PopUpArrowDirection.LeftDown);
			Entity friendlyHeroPower = GameState.Get().GetFriendlySidePlayer().GetHeroPower();
			ChangeAnimationStateForTutorial(friendlyHeroPower, friendlyHeroPower.GetCardId(), GetTag(GAME_TAG.TURN), friendlyHeroPower.GetZone(), SpellStateType.BIRTH);
			break;
		}
		case 654:
			yield return new WaitForSeconds(2.5f);
			yield return MissionPlayVO(friendlyActor, VO_TUTR_Jaina_JainaProudmoore_Human_InGame_OnDrawAOESpell_01);
			yield return new WaitForSeconds(0.5f);
			break;
		case 655:
		{
			HideTutorialNotification();
			Entity friendlyHeroPower2 = GameState.Get().GetFriendlySidePlayer().GetHeroPower();
			ChangeAnimationStateForTutorial(friendlyHeroPower2, friendlyHeroPower2.GetCardId(), GetTag(GAME_TAG.TURN), friendlyHeroPower2.GetZone(), SpellStateType.IDLE);
			break;
		}
		case 658:
			ShowTutorialNotification(m_tutorialConfig.m_SpellIgnoreTauntTutorialPosition, m_tutorialConfig.m_SpellIgnoreTauntTutorialPositionMobile, "TUTR_HELPER_Fight_01_SpellIgnoresTaunt");
			break;
		case 659:
			m_hasManaWyrmBeenPlayed = true;
			break;
		case 750:
			yield return MissionPlaySound(VO_TUTR_Garrosh_Death_01);
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
		if (m_playedLines.Contains(entity.GetCardId()) && entity.GetCardType() != TAG_CARDTYPE.HERO_POWER)
		{
			yield break;
		}
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
		switch (cardID)
		{
		case "TUTR_NEW1_012":
			if (!m_hasManaWyrmBeenPlayed)
			{
				m_hasManaWyrmBeenPlayed = true;
			}
			break;
		case "TUTR_OG_218":
			if (!m_hasFrostwolfBeenPlayed)
			{
				m_hasFrostwolfBeenPlayed = true;
				GameState.Get().SetBusy(busy: true);
				Vector3 friendlyPosition = entity.GetCard().transform.position;
				Vector3 helpPosition01 = new Vector3(friendlyPosition.x - 3f, friendlyPosition.y, friendlyPosition.z);
				Notification tauntHelp = NotificationManager.Get().CreatePopupText(UserAttentionBlocker.NONE, helpPosition01, TutorialEntity.GetTextScale(), GameStrings.Get("TUTR_HELPER_Fight_01_PlayTaunt"));
				tauntHelp.ShowPopUpArrow(Notification.PopUpArrowDirection.Right);
				NotificationManager.Get().DestroyNotification(tauntHelp, 5f);
				GameState.Get().SetBusy(busy: false);
			}
			break;
		case "TUTR_CS2_027":
			if (!m_hasTauntBeenPlayed)
			{
				yield return MissionPlayVO(friendlyActor, VO_TUTR_Jaina_JainaProudmoore_Human_InGame_Turn_08);
				m_hasTauntBeenPlayed = true;
			}
			break;
		}
	}

	protected override IEnumerator HandleStartOfTurnWithTiming(int turn)
	{
		if (handBounceArrow != null)
		{
			NotificationManager.Get().DestroyNotification(handBounceArrow, 0f);
			handBounceArrow = null;
		}
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
		case 1:
		{
			Vector3 manaPosition2 = ManaCrystalMgr.Get().GetManaCrystalSpawnPosition();
			Vector3 manaPopupPosition2;
			Notification.PopUpArrowDirection direction2;
			if ((bool)UniversalInputManager.UsePhoneUI)
			{
				manaPopupPosition2 = new Vector3(manaPosition2.x - 0.7f, manaPosition2.y + 1.14f, manaPosition2.z + 4.33f);
				direction2 = Notification.PopUpArrowDirection.RightDown;
			}
			else
			{
				manaPopupPosition2 = new Vector3(manaPosition2.x - 0.02f, manaPosition2.y + 0.2f, manaPosition2.z + 1.8f);
				direction2 = Notification.PopUpArrowDirection.Down;
			}
			m_manaNotification = NotificationManager.Get().CreatePopupText(UserAttentionBlocker.NONE, manaPopupPosition2, TutorialEntity.GetTextScale(), GameStrings.Get("TUTR_HELPER_Fight_01_Mana"));
			m_manaNotification.ShowPopUpArrow(direction2);
			break;
		}
		case 3:
		{
			Vector3 manaPosition = ManaCrystalMgr.Get().GetManaCrystalSpawnPosition();
			Vector3 manaPopupPosition;
			Notification.PopUpArrowDirection direction;
			if ((bool)UniversalInputManager.UsePhoneUI)
			{
				manaPopupPosition = new Vector3(manaPosition.x - 0.7f, manaPosition.y + 1.14f, manaPosition.z + 4.33f);
				direction = Notification.PopUpArrowDirection.RightDown;
			}
			else
			{
				manaPopupPosition = new Vector3(manaPosition.x - 0.02f, manaPosition.y + 0.2f, manaPosition.z + 1.8f);
				direction = Notification.PopUpArrowDirection.Down;
			}
			m_manaRefreshNotification = NotificationManager.Get().CreatePopupText(UserAttentionBlocker.NONE, manaPopupPosition, TutorialEntity.GetTextScale(), GameStrings.Get("TUTR_HELPER_Fight_01_Mana_02"));
			m_manaRefreshNotification.ShowPopUpArrow(direction);
			break;
		}
		case 6:
			GameState.Get().SetBusy(busy: true);
			yield return new WaitForSeconds(1.5f);
			yield return MissionPlayVO(enemyActor, VO_TUTR_Garrosh_GarroshHellscream_Orc_InGame_Turn_04);
			GameState.Get().SetBusy(busy: false);
			break;
		case 7:
			GameState.Get().SetBusy(busy: true);
			yield return new WaitForSeconds(1.5f);
			yield return MissionPlayVO(friendlyActor, VO_TUTR_Jaina_JainaProudmoore_Human_InGame_OnDrawFireball_01);
			GameState.Get().SetBusy(busy: false);
			break;
		case 8:
			GameState.Get().SetBusy(busy: true);
			yield return new WaitForSeconds(1.5f);
			yield return MissionPlayVO(enemyActor, VO_TUTR_Garrosh_GarroshHellscream_Orc_InGame_Turn_01);
			GameState.Get().SetBusy(busy: false);
			break;
		case 9:
			GameState.Get().SetBusy(busy: true);
			yield return new WaitForSeconds(1.5f);
			yield return MissionPlayVO(friendlyActor, VO_TUTR_Jaina_JainaProudmoore_Human_HELPER_ENEMY_SPECIALMINION_01);
			GameState.Get().SetBusy(busy: false);
			break;
		case 10:
			GameState.Get().SetBusy(busy: true);
			yield return new WaitForSeconds(1.5f);
			yield return MissionPlayVO(enemyActor, VO_TUTR_Garrosh_GarroshHellscream_Orc_InGame_Turn_03);
			GameState.Get().SetBusy(busy: false);
			break;
		case 11:
			GameState.Get().SetBusy(busy: true);
			yield return new WaitForSeconds(1.5f);
			yield return MissionPlayVO(friendlyActor, VO_TUTR_Jaina_JainaProudmoore_Human_HELPER_EMPTYBOARD2_01);
			GameState.Get().SetBusy(busy: false);
			break;
		case 12:
			yield return new WaitForSeconds(1.5f);
			yield return MissionPlayVO(enemyActor, VO_TUTR_Garrosh_GarroshHellscream_Orc_InGame_Turn_05);
			break;
		case 13:
			yield return new WaitForSeconds(1.5f);
			HideTutorialNotification();
			yield return MissionPlayVO(friendlyActor, VO_TUTR_Jaina_JainaProudmoore_Human_InGame_Turn_07);
			break;
		}
	}

	private void OnEndTurnButtonStateChange(ActorStateType state)
	{
		EndTurnButton endTurn = EndTurnButton.Get();
		switch (state)
		{
		case ActorStateType.ENDTURN_NO_MORE_PLAYS:
			endTurn.ShowEndTurnBouncingArrowButtonAfterWait(m_waitTimesForEndTurnBounceArrow[m_timerIndex], m_tutorialConfig.m_EndTurnButtonArrowOffset);
			if (m_timerIndex < m_waitTimesForEndTurnBounceArrow.Length - 1)
			{
				m_timerIndex++;
			}
			break;
		case ActorStateType.ENDTURN_NMP_2_WAITING:
			endTurn.HideEndTurnBouncingArrow();
			break;
		}
	}

	private void ChangeAnimationStateForTutorial(Entity entity, string cardId, int turn, TAG_ZONE zone, SpellStateType spellState)
	{
		if (cardId == "TUTR_HERO_08bp" && zone == TAG_ZONE.PLAY)
		{
			Spell spell = entity.GetCard().GetActorSpell(SpellType.PLACEHOLDER_TUTORIAL_SPELL_3);
			if (spell != null)
			{
				spell.ActivateState(spellState);
			}
		}
	}

	public override void NotifyOfTargetModeStarted(Entity entity)
	{
		m_entityinTargetMode = entity;
		ChangeAnimationStateForTutorial(entity, entity.GetCardId(), GetTag(GAME_TAG.TURN), entity.GetZone(), SpellStateType.IDLE);
	}

	public override void NotifyOfTargetModeCancelled()
	{
		int turn = GetTag(GAME_TAG.TURN);
		if (m_entityinTargetMode != null)
		{
			ChangeAnimationStateForTutorial(m_entityinTargetMode, m_entityinTargetMode.GetCardId(), turn, m_entityinTargetMode.GetZone(), SpellStateType.BIRTH);
		}
	}
}
