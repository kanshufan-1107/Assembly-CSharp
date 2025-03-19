using System.Collections;
using System.Collections.Generic;
using Blizzard.T5.AssetManager;
using UnityEngine;

public class Tutorial_Fight_001 : Tutorial_Dungeon
{
	private static readonly AssetReference VO_TUTR_Jaina_JainaProudmoore_Human_InGame_Introduction_01 = new AssetReference("VO_TUTR_Jaina_JainaProudmoore_Human_InGame_Introduction_01.prefab:665e8d1428156394698a35cb218e90fe");

	private static readonly AssetReference VO_TUTR_Jaina_JainaProudmoore_Human_InGame_Introduction_02 = new AssetReference("VO_TUTR_Jaina_JainaProudmoore_Human_InGame_Introduction_02.prefab:104a9c1c909e425478231ebd642a7bb2");

	private static readonly AssetReference VO_TUTR_Jaina_JainaProudmoore_Human_InGame_Introduction_03 = new AssetReference("VO_TUTR_Jaina_JainaProudmoore_Human_InGame_Introduction_03.prefab:f3946e51444983f4082dcb813f5d4cbe");

	private static readonly AssetReference VO_TUTR_Jaina_JainaProudmoore_Human_InGame_OnDrawAOESpell_01 = new AssetReference("VO_TUTR_Jaina_JainaProudmoore_Human_InGame_OnDrawAOESpell_01.prefab:b3ddc8ab3e27b264995dd329a9ccee67");

	private static readonly AssetReference VO_TUTR_Jaina_JainaProudmoore_Human_InGame_OnPlayedHuffer_01 = new AssetReference("VO_TUTR_Jaina_JainaProudmoore_Human_InGame_OnPlayedHuffer_01.prefab:40a0c156a09fa59469d6b39cee667554");

	private static readonly AssetReference VO_TUTR_Jaina_JainaProudmoore_Human_InGame_OnPlayedKingKrush_01 = new AssetReference("VO_TUTR_Jaina_JainaProudmoore_Human_InGame_OnPlayedKingKrush_01.prefab:e488aed324a95ab4a9c21be334ee280d");

	private static readonly AssetReference VO_TUTR_Jaina_JainaProudmoore_Human_InGame_OnPlayToMySide_01 = new AssetReference("VO_TUTR_Jaina_JainaProudmoore_Human_InGame_OnPlayToMySide_01.prefab:8140a0cd4722c364fa98a1bcfae703e7");

	private static readonly AssetReference VO_TUTR_Jaina_JainaProudmoore_Human_InGame_Turn_01 = new AssetReference("VO_TUTR_Jaina_JainaProudmoore_Human_InGame_Turn_01.prefab:101fa17c458ec5f4b8f9bd8f5b02c531");

	private static readonly AssetReference VO_TUTR_Jaina_JainaProudmoore_Human_InGame_Turn_02 = new AssetReference("VO_TUTR_Jaina_JainaProudmoore_Human_InGame_Turn_02.prefab:dabe31dc3702ca6448f85d6bb87aa385");

	private static readonly AssetReference VO_TUTR_Jaina_JainaProudmoore_Human_InGame_Turn_03 = new AssetReference("VO_TUTR_Jaina_JainaProudmoore_Human_InGame_Turn_03.prefab:4df02cba07b10d140be9eba133deff75");

	private static readonly AssetReference VO_TUTR_Jaina_JainaProudmoore_Human_InGame_Turn_04 = new AssetReference("VO_TUTR_Jaina_JainaProudmoore_Human_InGame_Turn_04.prefab:19b68a41451750a44903d41affe74236");

	private static readonly AssetReference VO_TUTR_Jaina_JainaProudmoore_Human_InGame_VictoryPostExplosion_01 = new AssetReference("VO_TUTR_Jaina_JainaProudmoore_Human_InGame_VictoryPostExplosion_01.prefab:899dc6f97c5017449a4474c0aabcb3a1");

	private static readonly AssetReference VO_TUTR_Rexxar_Rexxar_Orc_InGame_Introduction_01 = new AssetReference("VO_TUTR_Rexxar_Rexxar_Orc_InGame_Introduction_01.prefab:62ee0bacc176c304bad8addbed99aaa9");

	private static readonly AssetReference VO_TUTR_Rexxar_Rexxar_Orc_InGame_Introduction_02 = new AssetReference("VO_TUTR_Rexxar_Rexxar_Orc_InGame_Introduction_02.prefab:945a9b75933c65f4fa9857cd7d96def9");

	private static readonly AssetReference VO_TUTR_Rexxar_Rexxar_Orc_InGame_Introduction_03 = new AssetReference("VO_TUTR_Rexxar_Rexxar_Orc_InGame_Introduction_03.prefab:2a7fa46fe10b5214f817bcae2b40fbc3");

	private static readonly AssetReference VO_TUTR_Rexxar_Rexxar_Orc_InGame_Introduction_04 = new AssetReference("VO_TUTR_Rexxar_Rexxar_Orc_InGame_Introduction_04.prefab:ef1e9248ba6ccef4e94f3df9b520ed30");

	private static readonly AssetReference VO_TUTR_Rexxar_Rexxar_Orc_InGame_Introduction_05 = new AssetReference("VO_TUTR_Rexxar_Rexxar_Orc_InGame_Introduction_05.prefab:5671fd0213b86b74d9415c4ef21df10d");

	private static readonly AssetReference VO_TUTR_Rexxar_Rexxar_Orc_InGame_OnPlayCharge_01 = new AssetReference("VO_TUTR_Rexxar_Rexxar_Orc_InGame_OnPlayCharge_01.prefab:c74e3f6519be8b94bb46147ec2af055c");

	private static readonly AssetReference VO_TUTR_Rexxar_Rexxar_Orc_InGame_OnPlayCompanion_01 = new AssetReference("VO_TUTR_Rexxar_Rexxar_Orc_InGame_OnPlayCompanion_01.prefab:0cab41afb63babf43b9563002126dbe5");

	private static readonly AssetReference VO_TUTR_Rexxar_Rexxar_Orc_InGame_OnPlayedArcaneShot_01 = new AssetReference("VO_TUTR_Rexxar_Rexxar_Orc_InGame_OnPlayedArcaneShot_01.prefab:70ddaa1c7a6b07f49b76259aa8e90bd1");

	private static readonly AssetReference VO_TUTR_Rexxar_Rexxar_Orc_InGame_OnPlayedStonetuskBoar_01 = new AssetReference("VO_TUTR_Rexxar_Rexxar_Orc_InGame_OnPlayedStonetuskBoar_01.prefab:7b6e49280eb42544ebaffe3aef7728e0");

	private static readonly AssetReference VO_TUTR_Rexxar_Rexxar_Orc_InGame_OnPlayRush_01 = new AssetReference("VO_TUTR_Rexxar_Rexxar_Orc_InGame_OnPlayRush_01.prefab:c60605f884ad6154eb72caadd5b38445");

	private static readonly AssetReference VO_TUTR_Rexxar_Rexxar_Orc_InGame_OnPlayUnleash_01 = new AssetReference("VO_TUTR_Rexxar_Rexxar_Orc_InGame_OnPlayUnleash_01.prefab:02461d642d7bd0642bbca788757d9092");

	private static readonly AssetReference VO_TUTR_Rexxar_Rexxar_Orc_InGame_OnTarget_01 = new AssetReference("VO_TUTR_Rexxar_Rexxar_Orc_InGame_OnTarget_01.prefab:2fa8bcd3ebef561499c721ee821e0a80");

	private static readonly AssetReference VO_TUTR_Jaina_JainaProudmoore_Human_ERROR_FULL_MINIONS_01 = new AssetReference("VO_TUTR_Jaina_JainaProudmoore_Human_ERROR_FULL_MINIONS_01.prefab:05e1d6990382ddf4fa28624dd55cb97f");

	private static readonly AssetReference VO_TUTR_Jaina_JainaProudmoore_Human_ERROR_GENERIC_01 = new AssetReference("VO_TUTR_Jaina_JainaProudmoore_Human_ERROR_GENERIC_01.prefab:61cd598c8d23c3246bcdc743cc8a69a3");

	private static readonly AssetReference VO_TUTR_Jaina_JainaProudmoore_Human_ERROR_HEROPOWER_01 = new AssetReference("VO_TUTR_Jaina_JainaProudmoore_Human_ERROR_HEROPOWER_01.prefab:a6acab4eb09cb0147a6412b615d45d60");

	private static readonly AssetReference VO_TUTR_Jaina_JainaProudmoore_Human_ERROR_NEEDMANA_01 = new AssetReference("VO_TUTR_Jaina_JainaProudmoore_Human_ERROR_NEEDMANA_01.prefab:f74023104fb28a441be47696ed84b145");

	private static readonly AssetReference VO_TUTR_Jaina_JainaProudmoore_Human_ERROR_NOMANA_01 = new AssetReference("VO_TUTR_Jaina_JainaProudmoore_Human_ERROR_NOMANA_01.prefab:b1158e99fa43a624493dcbe3251793e4");

	private static readonly AssetReference VO_TUTR_Jaina_JainaProudmoore_Human_ERROR_PLAY_01 = new AssetReference("VO_TUTR_Jaina_JainaProudmoore_Human_ERROR_PLAY_01.prefab:ae75786adf3d139499b238f45b0044ef");

	private static readonly AssetReference VO_TUTR_Jaina_JainaProudmoore_Human_ERROR_SUMMON_SICKNESS_01 = new AssetReference("VO_TUTR_Jaina_JainaProudmoore_Human_ERROR_SUMMON_SICKNESS_01.prefab:f1f89512f6736a74e91fed9b6d23a5b0");

	private static readonly AssetReference VO_TUTR_Jaina_JainaProudmoore_Human_ERROR_TARGET_01 = new AssetReference("VO_TUTR_Jaina_JainaProudmoore_Human_ERROR_TARGET_01.prefab:47ca686b234c2c2459610ac0243239db");

	private static readonly AssetReference VO_TUTR_Jaina_JainaProudmoore_Human_ERROR_TAUNT_01 = new AssetReference("VO_TUTR_Jaina_JainaProudmoore_Human_ERROR_TAUNT_01.prefab:693299a6c090cc0438cd5eee7acdbc64");

	private static readonly AssetReference VO_TUTR_Jaina_JainaProudmoore_Human_HELPER_EMPTYBOARD_01 = new AssetReference("VO_TUTR_Jaina_JainaProudmoore_Human_HELPER_EMPTYBOARD_01.prefab:bd8a20533f26bfa4286909ff4f55d531");

	private static readonly AssetReference VO_TUTR_Jaina_JainaProudmoore_Human_HELPER_EMPTYBOARD2_01 = new AssetReference("VO_TUTR_Jaina_JainaProudmoore_Human_HELPER_EMPTYBOARD2_01.prefab:30d1acd0b758e5044b3afadb597d7dc0");

	private static readonly AssetReference VO_TUTR_Jaina_JainaProudmoore_Human_HELPER_ENEMY_FULLBOARD_01 = new AssetReference("VO_TUTR_Jaina_JainaProudmoore_Human_HELPER_ENEMY_FULLBOARD_01.prefab:04c4b69147ae6fb478b0a1ffe016989c");

	private static readonly AssetReference VO_TUTR_Jaina_JainaProudmoore_Human_HELPER_ENEMY_SPECIALMINION_01 = new AssetReference("VO_TUTR_Jaina_JainaProudmoore_Human_HELPER_ENEMY_SPECIALMINION_01.prefab:80a73d2ae948e21408f11fceb629e6ca");

	private static readonly AssetReference VO_TUTR_Jaina_JainaProudmoore_Human_HELPER_HEROFOCUS_GARROSH_01 = new AssetReference("VO_TUTR_Jaina_JainaProudmoore_Human_HELPER_HEROFOCUS_GARROSH_01.prefab:1dcb1df84926e8d429128d05bd43ac4e");

	private static readonly AssetReference VO_TUTR_Jaina_JainaProudmoore_Human_HELPER_HEROFOCUS_LICHKING_01 = new AssetReference("VO_TUTR_Jaina_JainaProudmoore_Human_HELPER_HEROFOCUS_LICHKING_01.prefab:c30cfe53e1337d64b815356f9b5c19b1");

	private static readonly AssetReference VO_TUTR_Jaina_JainaProudmoore_Human_HELPER_HEROFOCUS_REXXAR_01 = new AssetReference("VO_TUTR_Jaina_JainaProudmoore_Human_HELPER_HEROFOCUS_REXXAR_01.prefab:2a7f8e555ff7ff94bae5be8bc79d8de8");

	private static readonly AssetReference VO_TUTR_Jaina_JainaProudmoore_Human_HELPER_SECOND_MINION_01 = new AssetReference("VO_TUTR_Jaina_JainaProudmoore_Human_HELPER_SECOND_MINION_01.prefab:8f8aaccd4530a5d44978f105dd09e398");

	private static readonly AssetReference VO_TUTR_Jaina_JainaProudmoore_Human_HELPER_SPELL_01 = new AssetReference("VO_TUTR_Jaina_JainaProudmoore_Human_HELPER_SPELL_01.prefab:92e11b261a353274fa189f2074a05539");

	private static readonly AssetReference TutorialConfig = new AssetReference("TutorialConfig.asset:8fd8521aa30221946a920d274f64719f");

	private static readonly AssetReference VO_TUTR_Rexxar_Death_01 = new AssetReference("VO_TUTR_Rexxar_Death_01.prefab:ca09ec464ad0b5b4c934fda9a4db22aa");

	private Notification m_handBounceArrow;

	private Card m_friendlyCardWyrm;

	private Notification m_wyrmHover;

	private Notification m_winHelp;

	private Notification m_historyHelp;

	private TooltipPanel m_attackHelpPanel;

	private TooltipPanel m_healthHelpPanel;

	private GameObject m_attackLabel;

	private GameObject m_healthLabel;

	private bool m_canTargetKingKrush;

	private int m_timerIndex;

	private Tutorial_Config m_tutorialConfig;

	private Entity m_entityinTargetMode;

	private Entity m_entityGrabbed;

	private Entity m_entityMouseOver;

	private Entity m_entityDrawnFromDeck;

	private bool m_createdTutorialNotification;

	private bool m_hasSuspendedTutorialNotification;

	private bool m_hasSuspendedHelpNotification;

	public override void PreloadAssets()
	{
		base.PreloadAssets();
		List<string> m_SoundFiles = new List<string>
		{
			VO_TUTR_Jaina_JainaProudmoore_Human_InGame_Introduction_01, VO_TUTR_Jaina_JainaProudmoore_Human_InGame_Introduction_02, VO_TUTR_Jaina_JainaProudmoore_Human_InGame_Introduction_03, VO_TUTR_Jaina_JainaProudmoore_Human_InGame_OnDrawAOESpell_01, VO_TUTR_Jaina_JainaProudmoore_Human_InGame_OnPlayedHuffer_01, VO_TUTR_Jaina_JainaProudmoore_Human_InGame_OnPlayedKingKrush_01, VO_TUTR_Jaina_JainaProudmoore_Human_InGame_OnPlayToMySide_01, VO_TUTR_Jaina_JainaProudmoore_Human_InGame_Turn_01, VO_TUTR_Jaina_JainaProudmoore_Human_InGame_Turn_02, VO_TUTR_Jaina_JainaProudmoore_Human_InGame_Turn_03,
			VO_TUTR_Jaina_JainaProudmoore_Human_InGame_Turn_04, VO_TUTR_Jaina_JainaProudmoore_Human_InGame_VictoryPostExplosion_01, VO_TUTR_Rexxar_Rexxar_Orc_InGame_Introduction_01, VO_TUTR_Rexxar_Rexxar_Orc_InGame_Introduction_02, VO_TUTR_Rexxar_Rexxar_Orc_InGame_Introduction_03, VO_TUTR_Rexxar_Rexxar_Orc_InGame_Introduction_04, VO_TUTR_Rexxar_Rexxar_Orc_InGame_Introduction_05, VO_TUTR_Rexxar_Rexxar_Orc_InGame_OnPlayCharge_01, VO_TUTR_Rexxar_Rexxar_Orc_InGame_OnPlayCompanion_01, VO_TUTR_Rexxar_Rexxar_Orc_InGame_OnPlayedArcaneShot_01,
			VO_TUTR_Rexxar_Rexxar_Orc_InGame_OnPlayedStonetuskBoar_01, VO_TUTR_Rexxar_Rexxar_Orc_InGame_OnPlayRush_01, VO_TUTR_Rexxar_Rexxar_Orc_InGame_OnPlayUnleash_01, VO_TUTR_Rexxar_Rexxar_Orc_InGame_OnTarget_01, VO_TUTR_Jaina_JainaProudmoore_Human_ERROR_FULL_MINIONS_01, VO_TUTR_Jaina_JainaProudmoore_Human_ERROR_GENERIC_01, VO_TUTR_Jaina_JainaProudmoore_Human_ERROR_HEROPOWER_01, VO_TUTR_Jaina_JainaProudmoore_Human_ERROR_NEEDMANA_01, VO_TUTR_Jaina_JainaProudmoore_Human_ERROR_NOMANA_01, VO_TUTR_Jaina_JainaProudmoore_Human_ERROR_PLAY_01,
			VO_TUTR_Jaina_JainaProudmoore_Human_ERROR_SUMMON_SICKNESS_01, VO_TUTR_Jaina_JainaProudmoore_Human_ERROR_TARGET_01, VO_TUTR_Jaina_JainaProudmoore_Human_ERROR_TAUNT_01, VO_TUTR_Jaina_JainaProudmoore_Human_HELPER_EMPTYBOARD_01, VO_TUTR_Jaina_JainaProudmoore_Human_HELPER_EMPTYBOARD2_01, VO_TUTR_Jaina_JainaProudmoore_Human_HELPER_ENEMY_FULLBOARD_01, VO_TUTR_Jaina_JainaProudmoore_Human_HELPER_ENEMY_SPECIALMINION_01, VO_TUTR_Jaina_JainaProudmoore_Human_HELPER_HEROFOCUS_GARROSH_01, VO_TUTR_Jaina_JainaProudmoore_Human_HELPER_HEROFOCUS_LICHKING_01, VO_TUTR_Jaina_JainaProudmoore_Human_HELPER_HEROFOCUS_REXXAR_01,
			VO_TUTR_Jaina_JainaProudmoore_Human_HELPER_SECOND_MINION_01, VO_TUTR_Jaina_JainaProudmoore_Human_HELPER_SPELL_01, VO_TUTR_Rexxar_Death_01
		};
		SetBossVOLines(m_SoundFiles);
		foreach (string soundFile in m_SoundFiles)
		{
			PreloadSound(soundFile);
		}
		AssetLoader.Get()?.LoadAsset<Tutorial_Config>(TutorialConfig, OnTutorialConfigLoaded, AssetLoadingOptions.None);
		ManaCrystalMgr manaCrystalsDisplay = ManaCrystalMgr.Get();
		if (manaCrystalsDisplay != null)
		{
			manaCrystalsDisplay.DisableManaCountDisplay();
		}
	}

	private void OnTutorialConfigLoaded(AssetReference assetRef, AssetHandle<Tutorial_Config> asset, object assetId)
	{
		if (asset == null)
		{
			Debug.LogError("Tutorial_Fight_001: could not load Tutorial Config file");
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
			endTurn.OnButtonStateChanged += OnEndTurnButtonStateChange;
		}
		if ((bool)UniversalInputManager.UsePhoneUI)
		{
			ZoneHand.MobileHandStateChange += OnMobileHandStateChange;
			Card.HandToPlaySummonOutStart += OnHandToPlaySummonOutStart;
		}
		BnetBar.SkipTutorialSelected += OnSkipTutorialButtonSelected;
		BnetBarMenuButton.SettingsMenuSelectedEvent += OnSettingMenuToggled;
		HistoryManager.HistoryBarHoverEvent += OnHistoryBarHover;
	}

	public override void OnDecommissionGame()
	{
		base.OnDecommissionGame();
		NotificationManager.Get().DestroyAllNotificationsNowWithNoAnim();
		HideTutorialNotification();
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

	private void OnHandToPlaySummonOutStart(Card card)
	{
		if (m_handBounceArrow != null)
		{
			NotificationManager.Get().DestroyNotification(m_handBounceArrow, 0f);
			m_handBounceArrow = null;
		}
	}

	private void OnMobileHandStateChange(bool handState)
	{
		List<Card> cardsInHandFriendly = GameState.Get().GetFriendlySidePlayer().GetHandZone()
			.GetCards();
		if (cardsInHandFriendly == null || cardsInHandFriendly.Count <= 0)
		{
			return;
		}
		foreach (Card item in cardsInHandFriendly)
		{
			Entity handCardEntity = item.GetEntity();
			ChangeAnimationStateForTutorial(handCardEntity, handCardEntity.GetCardId(), GetTag(GAME_TAG.TURN), handCardEntity.GetZone(), SpellStateType.BIRTH);
		}
	}

	public override void NotifyOfCardMousedOver(Entity mousedOverEntity)
	{
		base.NotifyOfCardMousedOver(mousedOverEntity);
		m_entityMouseOver = mousedOverEntity;
		if (mousedOverEntity.GetZone() == TAG_ZONE.HAND)
		{
			ShowAttackAndHealthLabel(mousedOverEntity.GetCard(), TAG_ZONE.HAND);
			ChangeAnimationStateForTutorial(mousedOverEntity, mousedOverEntity.GetCardId(), GetTag(GAME_TAG.TURN), mousedOverEntity.GetZone(), SpellStateType.IDLE);
			ToggleNotificationSuspension(active: true);
		}
	}

	public override void NotifyOfCardMousedOff(Entity mousedOffEntity)
	{
		int turn = GetTag(GAME_TAG.TURN);
		TAG_ZONE zoneTag = mousedOffEntity.GetZone();
		m_entityMouseOver = null;
		if (zoneTag == TAG_ZONE.HAND)
		{
			HideAttackAndHealthLabel();
			ToggleNotificationSuspension(active: false);
			if (mousedOffEntity.GetCardId() == "TUTR_NEW1_012t2" && turn == 1 && !m_createdTutorialNotification)
			{
				ShowPlayMinionTooltip();
			}
			else if (m_entityinTargetMode == null)
			{
				ChangeAnimationStateForTutorial(mousedOffEntity, mousedOffEntity.GetCardId(), turn, zoneTag, SpellStateType.BIRTH);
			}
			else if (m_entityinTargetMode != null && m_entityinTargetMode != mousedOffEntity)
			{
				ChangeAnimationStateForTutorial(mousedOffEntity, mousedOffEntity.GetCardId(), turn, zoneTag, SpellStateType.BIRTH);
			}
		}
	}

	public override void NotifyOfBigCardForCardInPlayShown(Actor mainCardActor, Actor extraCardActor)
	{
		if (mainCardActor != null)
		{
			ShowAttackAndHealthLabel(mainCardActor, TAG_ZONE.PLAY);
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
			int numAttacks = m_entityinTargetMode.GetTag(GAME_TAG.NUM_ATTACKS_THIS_TURN);
			if (m_entityinTargetMode.IsMinion() && numAttacks <= 0)
			{
				ChangeAnimationStateForTutorial(m_entityinTargetMode, m_entityinTargetMode.GetCardId(), turn, m_entityinTargetMode.GetZone(), SpellStateType.BIRTH);
			}
			else
			{
				ChangeAnimationStateForTutorial(m_entityinTargetMode, m_entityinTargetMode.GetCardId(), turn, m_entityinTargetMode.GetZone(), SpellStateType.BIRTH);
			}
		}
	}

	public override void NotifyOfMinionPlayed(Entity minion)
	{
		if ((minion.GetCardId() == "TUTR_DAL_092t2" || minion.GetCardId() == "TUTR_NEW1_012t2") && m_createdTutorialNotification)
		{
			HideTutorialNotification();
		}
		if (minion.GetCardId() == "TUTR_DAL_092t2")
		{
			List<Card> cardsInPlayFriendly = GameState.Get().GetFriendlySidePlayer().GetBattlefieldZone()
				.GetCards();
			if (cardsInPlayFriendly != null)
			{
				foreach (Card item in cardsInPlayFriendly)
				{
					Entity playZoneEntity = item.GetEntity();
					if (playZoneEntity.GetCardId() == "TUTR_NEW1_012t2")
					{
						ChangeAnimationStateForTutorial(playZoneEntity, playZoneEntity.GetCardId(), GetTag(GAME_TAG.TURN), playZoneEntity.GetZone(), SpellStateType.BIRTH);
					}
				}
			}
		}
		HideAttackAndHealthLabel();
	}

	public override void NotifyOfCardGrabbed(Entity entity)
	{
		m_entityGrabbed = entity;
		ChangeAnimationStateForTutorial(entity, entity.GetCardId(), GetTag(GAME_TAG.TURN), entity.GetZone(), SpellStateType.IDLE);
		HideAttackAndHealthLabel();
	}

	public override void NotifyOfCardDropped(Entity entity)
	{
		int turn = GetTag(GAME_TAG.TURN);
		m_entityGrabbed = null;
		ChangeAnimationStateForTutorial(entity, entity.GetCardId(), turn, entity.GetZone(), SpellStateType.BIRTH);
		HideAttackAndHealthLabel();
	}

	public override void NotifyOfMulliganInitialized()
	{
		base.NotifyOfMulliganInitialized();
	}

	public override bool NotifyOfEndTurnButtonPushed()
	{
		Network.Options options = GameState.Get().GetOptionsPacket();
		if (options != null && !options.HasValidOption())
		{
			NotificationManager.Get().DestroyAllNotificationsNowWithNoAnim();
			NotificationManager.Get().DestroyAllArrows();
			HideTutorialNotification();
			return true;
		}
		return false;
	}

	public override bool NotifyOfCardTooltipDisplayShow(Card card)
	{
		if (m_wyrmHover != null)
		{
			NotificationManager.Get().DestroyNotification(m_wyrmHover, 0f);
		}
		ToggleNotificationSuspension(active: true);
		return true;
	}

	public override void NotifyOfCardTooltipDisplayHide(Card card)
	{
		HideAttackAndHealthLabel();
		ToggleNotificationSuspension(active: false);
	}

	public override bool NotifyOfBattlefieldCardClicked(Entity clickedEntity, bool wasInTargetMode)
	{
		if (clickedEntity.GetCardId() == "TUTR_EX1_543" && !m_canTargetKingKrush && wasInTargetMode)
		{
			Actor friendlyActor = GameState.Get().GetFriendlySidePlayer().GetHero()
				.GetCard()
				.GetActor();
			Gameplay.Get().StartCoroutine(PlaySoundAndWait("VO_TUTR_Jaina_JainaProudmoore_Human_HELPER_HEROFOCUS_REXXAR_01.prefab:2a7f8e555ff7ff94bae5be8bc79d8de8", GameStrings.Get("VO_TUTR_Jaina_JainaProudmoore_Human_HELPER_HEROFOCUS_REXXAR_01"), Notification.SpeechBubbleDirection.BottomLeft, friendlyActor));
			m_canTargetKingKrush = true;
			return false;
		}
		return true;
	}

	public override void NotifyOfGameOver(TAG_PLAYSTATE gameResult)
	{
		base.NotifyOfGameOver(gameResult);
		if (m_attackHelpPanel != null)
		{
			Object.Destroy(m_attackHelpPanel.gameObject);
			m_attackHelpPanel = null;
		}
		if (m_healthHelpPanel != null)
		{
			Object.Destroy(m_healthHelpPanel.gameObject);
			m_healthHelpPanel = null;
		}
		NotificationManager.Get().DestroyAllNotificationsNowWithNoAnim();
		HideTutorialNotification();
		EndTurnButton endTurn = EndTurnButton.Get();
		if (endTurn != null)
		{
			endTurn.OnButtonStateChanged -= OnEndTurnButtonStateChange;
		}
		if ((bool)UniversalInputManager.UsePhoneUI)
		{
			ZoneHand.MobileHandStateChange -= OnMobileHandStateChange;
			Card.HandToPlaySummonOutStart -= OnHandToPlaySummonOutStart;
		}
		BnetBar.SkipTutorialSelected -= OnSkipTutorialButtonSelected;
		BnetBarMenuButton.SettingsMenuSelectedEvent -= OnSettingMenuToggled;
		HistoryManager.HistoryBarHoverEvent -= OnHistoryBarHover;
		if (gameResult == TAG_PLAYSTATE.WON)
		{
			GameUtils.SetTutorialProgress(TutorialProgress.REXXAR_COMPLETE, tutorialComplete: false);
		}
	}

	public override void NotifyOfEntityAttacked(Entity attacker, Entity defender)
	{
		if (m_winHelp != null)
		{
			NotificationManager.Get().DestroyNotification(m_winHelp, 0f);
		}
		if (attacker != null)
		{
			ChangeAnimationStateForTutorial(attacker, attacker.GetCardId(), GetTag(GAME_TAG.TURN), attacker.GetZone(), SpellStateType.IDLE);
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
				endTurn.ShowEndTurnBouncingArrowButtonAfterWait(m_tutorialConfig.m_endTurnButtonDelay[m_timerIndex], m_tutorialConfig.m_EndTurnButtonArrowOffset);
				if (m_timerIndex < m_tutorialConfig.m_endTurnButtonDelay.Count - 1)
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

	private void ShowAttackAndHealthLabel(Card card, TAG_ZONE zone)
	{
		ShowAttackAndHealthLabel(card.GetActor(), zone);
	}

	private void ShowAttackAndHealthLabel(Actor actor, TAG_ZONE zone)
	{
		IAssetLoader assetLoader = AssetLoader.Get();
		if (m_attackLabel == null)
		{
			assetLoader.InstantiatePrefab("NumberLabel.prefab:597544d5ed24b994f95fe56e28584992", delegate(AssetReference name, GameObject go, object data)
			{
				if (actor == null)
				{
					Log.Gameplay.PrintError("Invalid Actor Data ");
				}
				else
				{
					m_attackLabel = go;
					PositionAttackLabelPanel(go, actor, zone);
					UberText component = go.GetComponent<UberText>();
					if (component != null)
					{
						component.Text = GameStrings.Get("GLOBAL_ATTACK");
					}
				}
			}, null, AssetLoadingOptions.IgnorePrefabPosition);
		}
		else
		{
			m_attackLabel.SetActive(value: true);
			PositionAttackLabelPanel(m_attackLabel, actor, zone);
		}
		if (m_healthLabel == null)
		{
			assetLoader.InstantiatePrefab("NumberLabel.prefab:597544d5ed24b994f95fe56e28584992", delegate(AssetReference name, GameObject go, object data)
			{
				if (actor == null)
				{
					Log.Gameplay.PrintError("Invalid Actor Data ");
				}
				else
				{
					m_healthLabel = go;
					PositionHealthLabelPanel(go, actor, zone);
					UberText component2 = go.GetComponent<UberText>();
					if (component2 != null)
					{
						component2.Text = GameStrings.Get("GLOBAL_HEALTH");
					}
				}
			}, null, AssetLoadingOptions.IgnorePrefabPosition);
		}
		else
		{
			m_healthLabel.SetActive(value: true);
			PositionHealthLabelPanel(m_healthLabel, actor, zone);
		}
	}

	private void HideAttackAndHealthLabel()
	{
		if (m_attackLabel != null)
		{
			Object.Destroy(m_attackLabel.gameObject);
		}
		if (m_healthLabel != null)
		{
			Object.Destroy(m_healthLabel.gameObject);
		}
	}

	private IEnumerator ShowArrowInSeconds(float seconds)
	{
		yield return new WaitForSeconds(seconds);
		List<Card> handCards = GameState.Get().GetFriendlySidePlayer().GetHandZone()
			.GetCards();
		if (handCards.Count == 0)
		{
			yield break;
		}
		Card cardInHand = handCards[0];
		while (iTween.Count(cardInHand.gameObject) > 0)
		{
			yield return null;
		}
		if (!cardInHand.IsMousedOver() && !(InputManager.Get().GetHeldCard() == cardInHand))
		{
			ZoneHand zoneInstance = cardInHand.GetZone() as ZoneHand;
			if (zoneInstance != null && !zoneInstance.HandEnlarged())
			{
				ShowHandBouncingArrow();
			}
		}
	}

	private void ShowHandBouncingArrow()
	{
		if (!(m_tutorialConfig == null))
		{
			if (m_handBounceArrow != null)
			{
				NotificationManager.Get().DestroyNotification(m_handBounceArrow, 0f);
				m_handBounceArrow = null;
			}
			List<Card> cardsInHand = GameState.Get().GetFriendlySidePlayer().GetHandZone()
				.GetCards();
			if (cardsInHand.Count != 0)
			{
				Card cardInHand = cardsInHand[0];
				Vector3 cardInHandPosition = cardInHand.transform.position;
				Vector3 bounceArrowPos = ((!UniversalInputManager.UsePhoneUI) ? new Vector3(cardInHandPosition.x + 0.1f, cardInHandPosition.y + 1f, cardInHandPosition.z + 1.75f) : (cardInHandPosition + m_tutorialConfig.m_handBouncingArrowOffsetMobile));
				m_handBounceArrow = NotificationManager.Get().CreateBouncingArrow(UserAttentionBlocker.NONE, bounceArrowPos, new Vector3(0f, 0f, 0f));
				m_handBounceArrow.transform.parent = cardInHand.transform;
			}
		}
	}

	private void PositionAttackLabelPanel(GameObject attackLabelPanel, Actor cardActor, TAG_ZONE zone)
	{
		if (attackLabelPanel == null || cardActor == null)
		{
			return;
		}
		GameObject numberToLabel = cardActor.GetAttackTextObject();
		if (numberToLabel == null || m_tutorialConfig == null)
		{
			attackLabelPanel.SetActive(value: false);
			return;
		}
		attackLabelPanel.transform.parent = numberToLabel.transform;
		attackLabelPanel.transform.localEulerAngles = Vector3.zero;
		attackLabelPanel.transform.localPosition = Vector3.zero;
		if (zone == TAG_ZONE.HAND)
		{
			attackLabelPanel.transform.localScale = m_tutorialConfig.m_attackAndHealthLabelScaleHand;
			Vector3 offset = m_tutorialConfig.m_attackLabelPositionOffsetHand;
			if ((bool)UniversalInputManager.UsePhoneUI)
			{
				offset = m_tutorialConfig.m_attackLabelOffsetHandMobile;
				attackLabelPanel.transform.localScale = m_tutorialConfig.m_attackAndHealthLabelScaleHandMobile;
			}
			attackLabelPanel.transform.localPosition = attackLabelPanel.transform.localPosition + offset;
		}
		if (zone == TAG_ZONE.PLAY)
		{
			attackLabelPanel.transform.localScale = m_tutorialConfig.m_attackAndHealthLabelScalePlay;
			Vector3 offset2 = m_tutorialConfig.m_attackLabelPositionOffsetOnPlay;
			if ((bool)UniversalInputManager.UsePhoneUI)
			{
				attackLabelPanel.transform.localEulerAngles = new Vector3(0f, 0f, 1f);
				offset2 = m_tutorialConfig.m_attackLabelPositionOffsetOnPlayMobile;
				attackLabelPanel.transform.localScale = m_tutorialConfig.m_attackAndHealthLabelScalePlayMobile;
			}
			attackLabelPanel.transform.localPosition = attackLabelPanel.transform.localPosition + offset2;
		}
	}

	private void PositionHealthLabelPanel(GameObject healthLabelPanel, Actor cardActor, TAG_ZONE zone)
	{
		if (healthLabelPanel == null && cardActor == null)
		{
			return;
		}
		GameObject numberToLabel = cardActor.GetHealthTextObject();
		if (numberToLabel == null || m_tutorialConfig == null)
		{
			healthLabelPanel.SetActive(value: false);
			return;
		}
		healthLabelPanel.transform.parent = numberToLabel.transform;
		healthLabelPanel.transform.localEulerAngles = Vector3.zero;
		healthLabelPanel.transform.localPosition = Vector3.zero;
		if (zone == TAG_ZONE.HAND)
		{
			healthLabelPanel.transform.localScale = m_tutorialConfig.m_attackAndHealthLabelScaleHand;
			Vector3 offset = m_tutorialConfig.m_healthLabelPositionOffsetHand;
			if ((bool)UniversalInputManager.UsePhoneUI)
			{
				offset = m_tutorialConfig.m_healthLabelPositionOffsetHandMobile;
				healthLabelPanel.transform.localScale = m_tutorialConfig.m_attackAndHealthLabelScaleHandMobile;
			}
			healthLabelPanel.transform.localPosition = healthLabelPanel.transform.localPosition + offset;
		}
		if (zone == TAG_ZONE.PLAY)
		{
			Vector3 offset2 = m_tutorialConfig.m_healthLabelPositionOffsetOnPlay;
			healthLabelPanel.transform.localScale = m_tutorialConfig.m_attackAndHealthLabelScalePlay;
			if ((bool)UniversalInputManager.UsePhoneUI)
			{
				offset2 = m_tutorialConfig.m_healthLabelPositionOffsetOnPlayMobile;
				healthLabelPanel.transform.localScale = m_tutorialConfig.m_attackAndHealthLabelScalePlayMobile;
			}
			healthLabelPanel.transform.localPosition = healthLabelPanel.transform.localPosition + offset2;
		}
	}

	private void ShowPlayMinionTooltip()
	{
		if (NotificationManager.Get() == null)
		{
			Debug.LogError($"Tutorial_001: notificationManager is null");
		}
		else if (m_tutorialConfig != null)
		{
			ShowTutorialNotification(m_tutorialConfig.m_PlayMinionTooltipPosition, m_tutorialConfig.m_PlayMinionTooltipPositionMobile, "TUTR_HELPER_Fight_01_PlayCard");
		}
	}

	private void ChangeAnimationStateForTutorial(Entity entity, string cardId, int turn, TAG_ZONE zone, SpellStateType spellState)
	{
		ZoneHand zoneInstance = entity.GetCard().GetZone() as ZoneHand;
		if (zoneInstance != null && (bool)UniversalInputManager.UsePhoneUI && m_tutorialConfig != null)
		{
			if (m_handBounceArrow != null)
			{
				NotificationManager.Get().DestroyNotification(m_handBounceArrow, 0f);
				m_handBounceArrow = null;
			}
			Gameplay.Get().StopCoroutine(ShowArrowInSeconds(m_tutorialConfig.m_showArrowOnMiniHandDelay));
			if (!zoneInstance.HandEnlarged())
			{
				if (m_entityMouseOver == null && m_entityGrabbed == null)
				{
					Gameplay.Get().StartCoroutine(ShowArrowInSeconds(m_tutorialConfig.m_showArrowOnMiniHandDelay));
				}
				spellState = SpellStateType.IDLE;
				if (m_entityDrawnFromDeck != null)
				{
					m_entityDrawnFromDeck = null;
					return;
				}
			}
		}
		if (cardId == "TUTR_NEW1_012t2" && zone == TAG_ZONE.HAND)
		{
			Spell spell = entity.GetCard().GetActorSpell(SpellType.TUTORIAL_PLAY_MINION);
			if (spell != null)
			{
				spell.ActivateState(spellState);
			}
		}
		if (cardId == "TUTR_DAL_092t2" && zone == TAG_ZONE.HAND && (turn == 7 || turn == 5))
		{
			Spell spell2 = entity.GetCard().GetActorSpell(SpellType.TUTORIAL_PLAY_MINION_NEUTRAL);
			if (spell2 != null)
			{
				spell2.ActivateState(spellState);
			}
		}
		if (cardId == "TUTR_NEW1_012t2" && zone == TAG_ZONE.PLAY && (turn == 3 || turn == 5))
		{
			Spell spell3 = entity.GetCard().GetActorSpell(SpellType.TUTORIAL_TARGET_OPPONENT);
			int numAttacks = entity.GetNumAttacksThisTurn();
			if (IsArcaneServentInPlay())
			{
				switch (entity.GetRealTimeZonePosition())
				{
				case 1:
					spell3.transform.rotation = Quaternion.Euler(m_tutorialConfig.m_attackOpponentInstructionAnimationRot);
					break;
				case 2:
					spell3.transform.rotation = Quaternion.Euler(-1f * m_tutorialConfig.m_attackOpponentInstructionAnimationRot);
					break;
				}
			}
			if (spell3 != null && numAttacks < 1)
			{
				spell3.ActivateState(spellState);
			}
		}
		if (cardId == "TUTR_RLK_843s2" && zone == TAG_ZONE.HAND)
		{
			Spell spell4 = entity.GetCard().GetActorSpell(SpellType.TUTORIAL_TARGET_MINION);
			if (spell4 != null)
			{
				spell4.ActivateState(spellState);
			}
		}
		if (cardId == "TUTR_CS2_029s2" && zone == TAG_ZONE.HAND)
		{
			Spell spell5 = entity.GetCard().GetActorSpell(SpellType.TUTORIAL_TARGET_OPPONENT);
			if (spell5 != null)
			{
				spell5.ActivateState(spellState);
			}
		}
	}

	private bool IsArcaneServentInPlay()
	{
		List<Card> cardsInPlayFriendly = GameState.Get().GetFriendlySidePlayer().GetBattlefieldZone()
			.GetCards();
		if (cardsInPlayFriendly != null)
		{
			foreach (Card item in cardsInPlayFriendly)
			{
				if (item.GetEntity().GetCardId() == "TUTR_DAL_092t2")
				{
					return true;
				}
			}
		}
		return false;
	}

	private void ShowTutorialNotification(Vector3 tooltipPosition, Vector3 tooltipPostionMobile, string gameStringKey, NotificationManager.TutorialPopupType type = NotificationManager.TutorialPopupType.IMPORTANT)
	{
		NotificationManager notificationManager = NotificationManager.Get();
		if (notificationManager == null)
		{
			Debug.LogError(GetType().Name + " could not load Tutorial Config file");
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

	private void HideTutorialNotification(float delay = 0f, float animationDuration = -1f)
	{
		NotificationManager notificationManager = NotificationManager.Get();
		if (notificationManager == null)
		{
			Debug.LogError($"Tutorial_001: notificationManager is null");
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
		if (m_winHelp != null)
		{
			m_winHelp.gameObject.SetActive(value: false);
		}
		if (m_wyrmHover != null)
		{
			m_wyrmHover.gameObject.SetActive(value: false);
		}
		if (m_historyHelp != null)
		{
			m_historyHelp.gameObject.SetActive(value: false);
		}
		m_hasSuspendedHelpNotification = true;
	}

	private void ShowSuspendedHelpNotification()
	{
		if (m_winHelp != null)
		{
			m_winHelp.gameObject.SetActive(value: true);
		}
		if (m_wyrmHover != null)
		{
			m_wyrmHover.gameObject.SetActive(value: true);
		}
		if (m_historyHelp != null)
		{
			m_historyHelp.gameObject.SetActive(value: true);
		}
		m_hasSuspendedHelpNotification = false;
	}

	protected override IEnumerator HandleMissionEventWithTiming(int missionEvent)
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
		switch (missionEvent)
		{
		case 650:
		{
			HideTutorialNotification();
			if (m_tutorialConfig != null)
			{
				ShowTutorialNotification(m_tutorialConfig.m_DamageRexxarWithMinionTooltipOffset, m_tutorialConfig.m_DamageRexxarWithMinionTooltipOffsetMobile, "TUTR_HELPER_Fight_01_AttackHero");
			}
			Card cardInPlayFriendly = GameState.Get().GetFriendlySidePlayer().GetBattlefieldZone()
				.GetFirstCard();
			if (cardInPlayFriendly != null)
			{
				Entity playZoneEntity = cardInPlayFriendly.GetEntity();
				ChangeAnimationStateForTutorial(playZoneEntity, playZoneEntity.GetCardId(), GetTag(GAME_TAG.TURN), playZoneEntity.GetZone(), SpellStateType.BIRTH);
			}
			break;
		}
		case 651:
			HideTutorialNotification();
			break;
		case 652:
			ShowTutorialNotification(m_tutorialConfig.m_MinionTradeTooltipPositionOffset, m_tutorialConfig.m_MinionTradeTooltipPositionOffsetMobile, "TUTR_HELPER_Fight_01_MinionsDestroyed", NotificationManager.TutorialPopupType.GRAPHIC);
			break;
		case 653:
		{
			yield return new WaitForSeconds(0.5f);
			m_friendlyCardWyrm = GameState.Get().GetFriendlySidePlayer().GetBattlefieldZone()
				.GetFirstCard();
			Vector3 wyrmPosition = m_friendlyCardWyrm.transform.position;
			Vector3 helpPosition01 = new Vector3(wyrmPosition.x - 3f, wyrmPosition.y, wyrmPosition.z);
			if (UniversalInputManager.Get().IsTouchMode())
			{
				m_wyrmHover = NotificationManager.Get().CreatePopupText(UserAttentionBlocker.NONE, helpPosition01, TutorialEntity.GetTextScale(), GameStrings.Get("TUTR_HELPER_Fight_01_PlayMinion_Mobile"));
			}
			else
			{
				m_wyrmHover = NotificationManager.Get().CreatePopupText(UserAttentionBlocker.NONE, helpPosition01, TutorialEntity.GetTextScale(), GameStrings.Get("TUTR_HELPER_Fight_01_PlayMinion"));
			}
			m_wyrmHover.ShowPopUpArrow(Notification.PopUpArrowDirection.Right);
			break;
		}
		case 654:
			HideTutorialNotification(0f, 1f);
			break;
		case 655:
		{
			Card card = GameState.Get().GetFriendlySidePlayer().GetHandZone()
				.GetFirstCard();
			if (card != null)
			{
				m_entityDrawnFromDeck = card.GetEntity();
				ChangeAnimationStateForTutorial(m_entityDrawnFromDeck, m_entityDrawnFromDeck.GetCardId(), GetTag(GAME_TAG.TURN), m_entityDrawnFromDeck.GetZone(), SpellStateType.BIRTH);
			}
			break;
		}
		case 750:
			yield return MissionPlaySound(VO_TUTR_Rexxar_Death_01);
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
		m_entityinTargetMode = null;
		if (m_handBounceArrow != null)
		{
			NotificationManager.Get().DestroyNotification(m_handBounceArrow, 0f);
			m_handBounceArrow = null;
		}
		if (m_tutorialConfig == null)
		{
			yield break;
		}
		switch (turn)
		{
		case 1:
		{
			GameState gameState = GameState.Get();
			if (gameState == null)
			{
				Debug.LogError("RLK_Prologue.HandleStartOfTurnWithTiming(): GameState is null");
				break;
			}
			gameState.SetBusy(busy: true);
			yield return new WaitForSeconds(m_tutorialConfig.m_PlayerTurn1Delay);
			ShowPlayMinionTooltip();
			gameState.SetBusy(busy: false);
			break;
		}
		case 3:
			yield return new WaitForSeconds(m_tutorialConfig.m_PlayerTurn2Delay);
			yield return MissionPlayVO(friendlyActor, VO_TUTR_Jaina_JainaProudmoore_Human_HELPER_SPELL_01);
			ShowTutorialNotification(m_tutorialConfig.m_DamageSpellTutorialPosition, m_tutorialConfig.m_DamageSpellTutorialPositionMobile, "TUTR_HELPER_Fight_01_PlaySpell");
			break;
		case 5:
		{
			Vector3 tooltipPostion = m_tutorialConfig.m_WindContionTooltipPositionOffset;
			if ((bool)UniversalInputManager.UsePhoneUI)
			{
				tooltipPostion = m_tutorialConfig.m_WinContionTooltipPositionOffsetMobile;
			}
			m_winHelp = NotificationManager.Get().CreatePopupText(UserAttentionBlocker.NONE, tooltipPostion, TutorialEntity.GetTextScale(), GameStrings.Get("TUTR_HELPER_Fight_01_WinCondition"), convertLegacyPosition: false);
			m_winHelp.ShowPopUpArrow(Notification.PopUpArrowDirection.LeftUp);
			yield return new WaitForSeconds(m_tutorialConfig.m_PlayerTurn3Delay);
			Card cardInhandFriendly2 = GameState.Get().GetFriendlySidePlayer().GetHandZone()
				.GetFirstCard();
			if (cardInhandFriendly2 != null)
			{
				Entity handCardEntity2 = (m_entityDrawnFromDeck = cardInhandFriendly2.GetEntity());
				ChangeAnimationStateForTutorial(handCardEntity2, handCardEntity2.GetCardId(), GetTag(GAME_TAG.TURN), handCardEntity2.GetZone(), SpellStateType.BIRTH);
			}
			break;
		}
		case 7:
		{
			yield return new WaitForSeconds(1f);
			yield return MissionPlayVO(friendlyActor, VO_TUTR_Jaina_JainaProudmoore_Human_InGame_Turn_04);
			yield return new WaitForSeconds(m_tutorialConfig.m_PlayerTurn4Delay);
			Card cardInhandFriendly = GameState.Get().GetFriendlySidePlayer().GetHandZone()
				.GetFirstCard();
			if (cardInhandFriendly != null)
			{
				Entity handCardEntity = (m_entityDrawnFromDeck = cardInhandFriendly.GetEntity());
				ChangeAnimationStateForTutorial(handCardEntity, handCardEntity.GetCardId(), GetTag(GAME_TAG.TURN), handCardEntity.GetZone(), SpellStateType.BIRTH);
			}
			Vector3 helpPosition01 = m_tutorialConfig.m_historyHelpNotificationPosition;
			if ((bool)UniversalInputManager.UsePhoneUI)
			{
				helpPosition01 = m_tutorialConfig.m_historyHelpNotificationPositionMobile;
			}
			m_historyHelp = NotificationManager.Get().CreatePopupText(UserAttentionBlocker.NONE, helpPosition01, TutorialEntity.GetTextScale(), GameStrings.Get("TUTR_HELPER_Fight_01_HistoryTiles"), convertLegacyPosition: false);
			m_historyHelp.ShowPopUpArrow(Notification.PopUpArrowDirection.Left);
			NotificationManager.Get().DestroyNotification(m_historyHelp, 4f);
			break;
		}
		case 9:
			HideTutorialNotification();
			yield return new WaitForSeconds(m_tutorialConfig.m_PlayerTurn5Delay);
			break;
		}
	}
}
