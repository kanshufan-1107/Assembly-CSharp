using System.Collections;
using System.Collections.Generic;
using Blizzard.T5.Core;
using UnityEngine;

public class LettuceTutorialOneMissionEntity : LettuceMissionEntity
{
	private enum TutorialStep
	{
		Invalid,
		INTRO_VO,
		CLICK_FIRST_MERCENARY,
		SELECT_FIRST_ABILITY,
		CLICK_SECOND_MERCENARY,
		SELECT_SECOND_ABILITY,
		FIRST_TURN_END_READY,
		FIRST_COMBAT_START,
		EXPLAIN_ATTACK_TYPE
	}

	private static readonly Map<GameEntityOption, bool> s_booleanOptions = new Map<GameEntityOption, bool>
	{
		{
			GameEntityOption.WAIT_FOR_RATING_INFO,
			false
		},
		{
			GameEntityOption.DISABLE_MANUAL_DISMISSAL_OF_MERC_ABILITY_TRAY,
			true
		}
	};

	private static readonly Map<GameEntityOption, string> s_stringOptions = new Map<GameEntityOption, string>();

	private Notification m_clickChampionNotification;

	private Notification m_queueAbilityNotification;

	private Notification endTurnNotifier;

	private TutorialStep m_currentTutorialStep;

	private readonly List<Card> m_clickBlockedCards = new List<Card>();

	private static readonly AssetReference GiantRat_LOOTA_BOSS_18h_EmoteResponse = new AssetReference("GiantRat_LOOTA_BOSS_18h_EmoteResponse.prefab:323ab0c47034e8043b688bb368fa912c");

	private TooltipPanel attackHelpPanel;

	private TooltipPanel healthHelpPanel;

	private PlatformDependentValue<Vector3> m_attackTooltipPosition = new PlatformDependentValue<Vector3>(PlatformCategory.Screen)
	{
		PC = new Vector3(-2.35f, 0f, -0.62f),
		Phone = new Vector3(-3.5f, 0f, -0.62f)
	};

	private PlatformDependentValue<Vector3> m_healthTooltipPosition = new PlatformDependentValue<Vector3>(PlatformCategory.Screen)
	{
		PC = new Vector3(2.25f, 0f, -0.62f),
		Phone = new Vector3(3.25f, 0f, -0.62f)
	};

	private PlatformDependentValue<float> m_gemScale = new PlatformDependentValue<float>(PlatformCategory.Screen)
	{
		PC = 1.75f,
		Phone = 1.2f
	};

	public override void PreloadAssets()
	{
		base.PreloadAssets();
		foreach (string soundFile in new List<string> { GiantRat_LOOTA_BOSS_18h_EmoteResponse })
		{
			PreloadSound(soundFile);
		}
	}

	public LettuceTutorialOneMissionEntity()
	{
		m_gameOptions.AddOptions(s_booleanOptions, s_stringOptions);
		m_abilityOrderSpeechBubblesEnabled = true;
		m_enemyAbilityOrderSpeechBubblesEnabled = false;
	}

	protected override void OnLettuceMissionEntityReconnect(int currentTurn)
	{
		switch (currentTurn)
		{
		case 1:
		{
			Card leftMostMinionInFriendlyPlay = GetLeftMostMinionInFriendlyPlay();
			if ((object)leftMostMinionInFriendlyPlay != null && leftMostMinionInFriendlyPlay.GetEntity()?.GetSelectedLettuceAbilityID() == 0)
			{
				SetTutorialStep(TutorialStep.CLICK_FIRST_MERCENARY);
				break;
			}
			Card rightMostMinionInFriendlyPlay = GetRightMostMinionInFriendlyPlay();
			if ((object)rightMostMinionInFriendlyPlay != null && rightMostMinionInFriendlyPlay.GetEntity()?.GetSelectedLettuceAbilityID() == 0)
			{
				SetTutorialStep(TutorialStep.CLICK_SECOND_MERCENARY);
			}
			else
			{
				SetTutorialStep(TutorialStep.FIRST_TURN_END_READY);
			}
			break;
		}
		case 2:
			if (!IsAnyFriendlyAbilitySelected())
			{
				SetTutorialStep(TutorialStep.EXPLAIN_ATTACK_TYPE);
			}
			break;
		}
	}

	public override void OnDecommissionGame()
	{
		DestroyAllTutorialPopUps();
		base.OnDecommissionGame();
	}

	private void SetTutorialStep(TutorialStep step)
	{
		m_currentTutorialStep = step;
		GameEntity.Coroutines.StartCoroutine(TransitionTutorialStepCoroutine(step));
	}

	private IEnumerator TransitionTutorialStepCoroutine(TutorialStep step)
	{
		if (GameMgr.Get().IsSpectator())
		{
			yield break;
		}
		switch (step)
		{
		case TutorialStep.CLICK_FIRST_MERCENARY:
		{
			SetEndTurnEnableAndBlocker(isEnabled: false);
			AddClickBlockerForFriendlyMinions();
			Card leftMinionInPlay = GetLeftMostMinionInFriendlyPlay();
			if (leftMinionInPlay != null)
			{
				m_clickBlockedCards.Remove(leftMinionInPlay);
				ShowClickChampionTutorial(leftMinionInPlay);
			}
			break;
		}
		case TutorialStep.SELECT_FIRST_ABILITY:
		{
			AddInputBlockerFriendlyAbilityZone();
			yield return WaitForAbilityToLoad();
			AddClickBlockerForFriendlyMinions();
			RemoveInputBlockerFriendlyAbilityZone();
			yield return new WaitForSeconds(0.5f);
			Card abilityCard = GetAbilityButtonBySlot(0);
			if (abilityCard != null)
			{
				ShowQueueAbilityTutorial(abilityCard);
			}
			yield return WaitForMinionAbilitySelection(GetLeftMostMinionInFriendlyPlay(), TutorialStep.CLICK_SECOND_MERCENARY);
			DestroyNotification(m_queueAbilityNotification);
			break;
		}
		case TutorialStep.CLICK_SECOND_MERCENARY:
		{
			Card rightMinionInPlay = GetRightMostMinionInFriendlyPlay();
			if (rightMinionInPlay != null)
			{
				m_clickBlockedCards.Remove(rightMinionInPlay);
				ShowClickChampionTutorial(rightMinionInPlay);
			}
			break;
		}
		case TutorialStep.SELECT_SECOND_ABILITY:
		{
			GameState.Get().SetBusy(busy: true);
			yield return WaitForAbilityToLoad();
			AddClickBlockerForFriendlyMinions();
			GameState.Get().SetBusy(busy: false);
			yield return new WaitForSeconds(0.5f);
			Card abilityCard = GetAbilityButtonBySlot(0);
			if (abilityCard != null)
			{
				ShowQueueAbilityTutorial(abilityCard);
			}
			yield return WaitForMinionAbilitySelection(GetRightMostMinionInFriendlyPlay(), TutorialStep.FIRST_TURN_END_READY);
			DestroyNotification(m_queueAbilityNotification);
			break;
		}
		case TutorialStep.FIRST_TURN_END_READY:
			DestroyAllTutorialPopUps();
			AddClickBlockerForFriendlyMinions();
			AddInputBlockerFriendlyAbilityZone();
			SetEndTurnEnableAndBlocker(isEnabled: true);
			yield return new WaitForSeconds(1f);
			ShowEndTurnBouncingArrow();
			break;
		case TutorialStep.FIRST_COMBAT_START:
			GameState.Get().SetBusy(busy: true);
			RemoveClickBlockerForFriendlyMinions();
			RemoveInputBlockerFriendlyAbilityZone();
			CreateTutorialDialog(LettuceTutorialResources.LettuceTutorialPopupCombatFlowPrefab, "GAMEPLAY_LETTUCE_COMBAT_TITLE_TUTORIAL", "GAMEPLAY_LETTUCE_COMBAT_BODY_TUTORIAL", "GAMEPLAY_LETTUCE_COMBAT_BUTTON_TUTORIAL", UserPressedCombatTutorial, Vector2.zero);
			break;
		case TutorialStep.EXPLAIN_ATTACK_TYPE:
			GameState.Get().SetBusy(busy: true);
			CreateTutorialDialog(LettuceTutorialResources.LettuceTutorialPopupAbilitiesPrefab, "GAMEPLAY_LETTUCE_ATTACK_TYPE_TITLE_TUTORIAL", "GAMEPLAY_LETTUCE_ATTACK_TYPE_BODY_TUTORIAL", "GAMEPLAY_LETTUCE_ATTACK_TYPE_BUTTON_TUTORIAL", UserPressedAttackTutorial, Vector2.zero);
			break;
		}
	}

	private void ShowClickChampionTutorial(Card card, string textID = "GAMEPLAY_CHAMPION_CLICK_TUTORIAL", bool hideImmediately = false)
	{
		if (!(card == null))
		{
			Vector3 cardPosition = card.transform.position;
			Vector3 tutorialPosition;
			Notification.PopUpArrowDirection popUpDirection;
			if ((bool)UniversalInputManager.UsePhoneUI)
			{
				tutorialPosition = new Vector3(cardPosition.x + 0.05f, cardPosition.y, cardPosition.z + 2.9f);
				popUpDirection = Notification.PopUpArrowDirection.Down;
			}
			else
			{
				tutorialPosition = new Vector3(cardPosition.x, cardPosition.y, cardPosition.z + 2.5f);
				popUpDirection = Notification.PopUpArrowDirection.Down;
			}
			m_clickChampionNotification = NotificationManager.Get().CreatePopupText(UserAttentionBlocker.NONE, tutorialPosition, TutorialEntity.GetTextScale(), GameStrings.Get(textID));
			m_clickChampionNotification.ShowPopUpArrow(popUpDirection);
			m_clickChampionNotification.PulseReminderEveryXSeconds(2f);
		}
	}

	private void ShowQueueAbilityTutorial(Card abilityCard = null, string textID = "GAMEPLAY_QUEUE_ABILITY_TUTORIAL", bool hideImmediately = false)
	{
		if (!(abilityCard == null))
		{
			Vector3 abilityPosition = abilityCard.GetActor().transform.position;
			Vector3 popUpPos = ((!UniversalInputManager.UsePhoneUI) ? new Vector3(abilityPosition.x, abilityPosition.y, abilityPosition.z + 2.25f) : new Vector3(abilityPosition.x, abilityPosition.y, abilityPosition.z + 2.5f));
			m_queueAbilityNotification = NotificationManager.Get().CreatePopupText(UserAttentionBlocker.NONE, popUpPos, TutorialEntity.GetTextScale(), GameStrings.Get(textID));
			m_queueAbilityNotification.ShowPopUpArrow(Notification.PopUpArrowDirection.Down);
			m_queueAbilityNotification.PulseReminderEveryXSeconds(2f);
		}
	}

	public override void NotifyOfStartOfTurnEventsFinished()
	{
		switch (GetTag(GAME_TAG.TURN))
		{
		case 1:
			SetTutorialStep(TutorialStep.CLICK_FIRST_MERCENARY);
			break;
		case 2:
			SetTutorialStep(TutorialStep.EXPLAIN_ATTACK_TYPE);
			break;
		}
	}

	private void ShowEndTurnBouncingArrow()
	{
		if (!EndTurnButton.Get().IsInWaitingState())
		{
			Vector3 endTurnPos = EndTurnButton.Get().transform.position;
			Vector3 arrowPos = new Vector3(endTurnPos.x - 2f, endTurnPos.y, endTurnPos.z);
			NotificationManager.Get().CreateBouncingArrow(UserAttentionBlocker.NONE, arrowPos, new Vector3(0f, -90f, 0f));
		}
	}

	public override bool NotifyOfBattlefieldCardClicked(Entity clickedEntity, bool wasInTargetMode)
	{
		Card entityCard = clickedEntity.GetCard();
		if (m_clickBlockedCards.Contains(entityCard) || (clickedEntity.IsControlledByOpposingSidePlayer() && !GameState.Get().IsInTargetMode()))
		{
			return false;
		}
		DestroyNotification(m_clickChampionNotification);
		if (clickedEntity.IsLettuceAbility())
		{
			DestroyNotification(m_queueAbilityNotification);
		}
		switch (m_currentTutorialStep)
		{
		case TutorialStep.CLICK_FIRST_MERCENARY:
			SetTutorialStep(TutorialStep.SELECT_FIRST_ABILITY);
			break;
		case TutorialStep.CLICK_SECOND_MERCENARY:
			SetTutorialStep(TutorialStep.SELECT_SECOND_ABILITY);
			break;
		}
		return true;
	}

	public override bool NotifyOfEndTurnButtonPushed()
	{
		DestroyAllTutorialPopUps();
		int turnNum = GetTag(GAME_TAG.TURN);
		if (turnNum == 1)
		{
			SetTutorialStep(TutorialStep.FIRST_COMBAT_START);
			return true;
		}
		if (turnNum >= 2)
		{
			if (EndTurnButton.Get().HasNoMorePlays())
			{
				return true;
			}
			if (endTurnNotifier != null)
			{
				NotificationManager.Get().DestroyNotificationNowWithNoAnim(endTurnNotifier);
			}
			Vector3 endTurnPos = EndTurnButton.Get().transform.position;
			Vector3 popUpPos = new Vector3(endTurnPos.x - 3f, endTurnPos.y, endTurnPos.z);
			string textID = "GAMEPLAY_LETTUCE_NO_ENDTURN";
			endTurnNotifier = NotificationManager.Get().CreatePopupText(UserAttentionBlocker.NONE, popUpPos, TutorialEntity.GetTextScale(), GameStrings.Get(textID));
			NotificationManager.Get().DestroyNotification(endTurnNotifier, 2.5f);
			return false;
		}
		return false;
	}

	protected override IEnumerator HandleGameOverWithTiming(TAG_PLAYSTATE gameResult)
	{
		yield return base.HandleGameOverWithTiming(gameResult);
		DestroyAllTutorialPopUps();
		yield return null;
	}

	private IEnumerator WaitForMinionAbilitySelection(Card minionCard, TutorialStep nextTutorialStep)
	{
		Entity minionEnt = minionCard.GetEntity();
		int currentSelectedAbilityID = minionEnt.GetSelectedLettuceAbilityID();
		while (minionEnt.GetSelectedLettuceAbilityID() == currentSelectedAbilityID)
		{
			yield return null;
		}
		SetTutorialStep(nextTutorialStep);
	}

	private IEnumerator WaitForAbilityToLoad()
	{
		while (!ZoneMgr.Get().IsMercenariesAbilityTrayVisible())
		{
			yield return null;
		}
	}

	private void AddClickBlockerForFriendlyMinions()
	{
		foreach (Card card in GameState.Get().GetFriendlySidePlayer().GetBattlefieldZone()
			.GetCards())
		{
			if (!m_clickBlockedCards.Contains(card))
			{
				m_clickBlockedCards.Add(card);
			}
		}
	}

	private void RemoveClickBlockerForFriendlyMinions()
	{
		foreach (Card card in GameState.Get().GetFriendlySidePlayer().GetBattlefieldZone()
			.GetCards())
		{
			if (m_clickBlockedCards.Contains(card))
			{
				m_clickBlockedCards.Remove(card);
			}
		}
	}

	private void DestroyAllTutorialPopUps()
	{
		NotificationManager.Get().DestroyAllArrows();
		NotificationManager.Get().DestroyAllPopUps();
		NotificationManager.Get().DestroyAllNotificationsNowWithNoAnim();
	}

	public override void NotifyOfCardMousedOver(Entity mousedOverEntity)
	{
		base.NotifyOfCardMousedOver(mousedOverEntity);
		if (mousedOverEntity.IsLettuceAbility())
		{
			DestroyNotification(m_queueAbilityNotification);
		}
	}

	private void UserPressedCombatTutorial(UIEvent e)
	{
		GameState.Get().SetBusy(busy: false);
	}

	private void UserPressedAttackTutorial(UIEvent e)
	{
		GameState.Get().SetBusy(busy: false);
	}

	private static void SetEndTurnEnableAndBlocker(bool isEnabled)
	{
		if (isEnabled)
		{
			EndTurnButton.Get().RemoveInputBlocker();
			EndTurnButton.Get().SetDisabled(disabled: false);
			EndTurnButton.Get().Reset();
		}
		else
		{
			EndTurnButton.Get().AddInputBlocker();
			EndTurnButton.Get().SetDisabled(disabled: true);
		}
	}

	public override void NotifyOfCardTooltipBigCardActorShow()
	{
		Card mouseOverCard = BigCard.Get()?.GetCard();
		if (!(mouseOverCard == null))
		{
			Actor bigCardActor = null;
			if (mouseOverCard.IsLettuceAbility())
			{
				bigCardActor = BigCard.Get()?.GetBigCardActor();
			}
			else if (mouseOverCard.GetEntity().IsMinion())
			{
				bigCardActor = BigCard.Get()?.GetExtraBigCardActor();
			}
			if (bigCardActor != null)
			{
				bigCardActor.GetCostTextObject().SetActive(value: false);
				bigCardActor.GetCostText().Text = null;
			}
		}
	}

	public override bool NotifyOfCardTooltipDisplayShow(Card card)
	{
		if (GameState.Get().IsGameOver())
		{
			return false;
		}
		if (card.GetEntity().IsMinion())
		{
			if (attackHelpPanel == null)
			{
				ShowAttackTooltip(card);
				Gameplay.Get().StartCoroutine(ShowHealthTooltipAfterWait(card));
			}
			return false;
		}
		return true;
	}

	private void ShowAttackTooltip(Card card)
	{
		LayerUtils.SetLayer(card.GetActor().GetAttackObject().gameObject, GameLayer.Tooltip);
		Vector3 minionPosition = card.transform.position;
		Vector3 offset = m_attackTooltipPosition;
		Vector3 tipPosition = new Vector3(minionPosition.x + offset.x, minionPosition.y + offset.y, minionPosition.z + offset.z);
		if (attackHelpPanel != null)
		{
			Object.Destroy(attackHelpPanel.gameObject);
		}
		attackHelpPanel = TooltipPanelManager.Get().CreateKeywordPanel(0);
		attackHelpPanel.Reset();
		attackHelpPanel.Initialize(GameStrings.Get("GLOBAL_ATTACK"), GameStrings.Get("TUTORIAL01_HELP_12"));
		attackHelpPanel.SetScale(TooltipPanel.GAMEPLAY_SCALE);
		attackHelpPanel.transform.position = tipPosition;
		RenderUtils.SetAlpha(attackHelpPanel.gameObject, 0f);
		iTween.FadeTo(attackHelpPanel.gameObject, iTween.Hash("alpha", 1f, "time", 0.25f));
		card.GetActor().GetAttackObject().Enlarge(m_gemScale);
		card.GetActor().GetComponentInChildren<LettuceMinionInPlayFrame>()?.EnlargeAttackBauble(m_gemScale);
	}

	private IEnumerator ShowHealthTooltipAfterWait(Card card)
	{
		yield return new WaitForSeconds(0.05f);
		if (!(InputManager.Get().GetMousedOverCard() != card))
		{
			ShowHealthTooltip(card);
		}
	}

	private void ShowHealthTooltip(Card card)
	{
		LayerUtils.SetLayer(card.GetActor().GetHealthObject().gameObject, GameLayer.Tooltip);
		Vector3 minionPosition = card.transform.position;
		Vector3 offset = m_healthTooltipPosition;
		Vector3 tipPosition = new Vector3(minionPosition.x + offset.x, minionPosition.y + offset.y, minionPosition.z + offset.z);
		if (healthHelpPanel != null)
		{
			Object.Destroy(healthHelpPanel.gameObject);
		}
		healthHelpPanel = TooltipPanelManager.Get().CreateKeywordPanel(0);
		healthHelpPanel.Reset();
		healthHelpPanel.Initialize(GameStrings.Get("GLOBAL_HEALTH"), GameStrings.Get("TUTORIAL01_HELP_13"));
		healthHelpPanel.SetScale(TooltipPanel.GAMEPLAY_SCALE);
		healthHelpPanel.transform.position = tipPosition;
		RenderUtils.SetAlpha(healthHelpPanel.gameObject, 0f);
		iTween.FadeTo(healthHelpPanel.gameObject, iTween.Hash("alpha", 1f, "time", 0.25f));
		card.GetActor().GetHealthObject().Enlarge(m_gemScale);
		card.GetActor().GetComponentInChildren<LettuceMinionInPlayFrame>()?.EnlargeHealthBauble(m_gemScale);
	}

	public override void NotifyOfCardTooltipDisplayHide(Card card)
	{
		if (card == null)
		{
			return;
		}
		Actor actor = card.GetActor();
		if (actor == null)
		{
			return;
		}
		LettuceMinionInPlayFrame frame = actor.GetComponentInChildren<LettuceMinionInPlayFrame>();
		if (attackHelpPanel != null)
		{
			GemObject gemObject = actor.GetAttackObject();
			if (gemObject != null)
			{
				LayerUtils.SetLayer(gemObject.gameObject, GameLayer.Default);
				gemObject.Shrink();
			}
			if (frame != null)
			{
				frame.ShrinkAttackBauble();
			}
			Object.Destroy(attackHelpPanel.gameObject);
		}
		if (healthHelpPanel != null)
		{
			GemObject gemObject2 = actor.GetHealthObject();
			if (gemObject2 != null)
			{
				LayerUtils.SetLayer(gemObject2.gameObject, GameLayer.Default);
				gemObject2.Shrink();
			}
			if (frame != null)
			{
				frame.ShrinkHealthBauble();
			}
			Object.Destroy(healthHelpPanel.gameObject);
		}
	}
}
