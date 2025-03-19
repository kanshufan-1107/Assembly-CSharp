using System.Collections;
using System.Collections.Generic;
using Blizzard.T5.Core;
using UnityEngine;

public class LettuceTutorialBossMissionEntity : LettuceMissionEntity
{
	private enum TutorialStep
	{
		Invalid,
		MOUSEOVER_INSPECT,
		MOUSEOVER_INSPECT_DONE,
		INTRO_TUTORIAL,
		INSPECT_REMINDER_VO
	}

	private static readonly Map<GameEntityOption, bool> s_booleanOptions = new Map<GameEntityOption, bool> { 
	{
		GameEntityOption.WAIT_FOR_RATING_INFO,
		false
	} };

	private static readonly Map<GameEntityOption, string> s_stringOptions = new Map<GameEntityOption, string>();

	private TutorialStep m_currentTutorialStep;

	private Notification m_clickChampionNotification;

	private readonly List<Card> m_clickBlockedCards = new List<Card>();

	private static readonly AssetReference Valeera_BrassRing_Quote = new AssetReference("Valeera_BrassRing_Quote.prefab:170a2a9fe4b70f04aa2a058f3a27ba7b");

	private static readonly AssetReference VO_TUTORIAL_01_HOGGER_02_02 = new AssetReference("VO_TUTORIAL_01_HOGGER_02_02.prefab:7f321b26431a4974a82deefc368adf63");

	private static readonly AssetReference VO_TUTORIAL_01_HOGGER_10_10 = new AssetReference("VO_TUTORIAL_01_HOGGER_10_10.prefab:119535c251852324cb0794b4fd536627");

	public LettuceTutorialBossMissionEntity()
	{
		m_gameOptions.AddOptions(s_booleanOptions, s_stringOptions);
		m_abilityOrderSpeechBubblesEnabled = true;
	}

	public override void PreloadAssets()
	{
		base.PreloadAssets();
		foreach (string soundFile in new List<string> { VO_TUTORIAL_01_HOGGER_02_02, VO_TUTORIAL_01_HOGGER_10_10 })
		{
			PreloadSound(soundFile);
		}
	}

	public override void OnDecommissionGame()
	{
		NotificationManager.Get().DestroyAllNotificationsNowWithNoAnim();
		base.OnDecommissionGame();
	}

	protected override void OnLettuceMissionEntityReconnect(int currentTurn)
	{
		if (currentTurn == 1 && GetTag<ACTION_STEP_TYPE>(GAME_TAG.ACTION_STEP_TYPE) == ACTION_STEP_TYPE.DEFAULT && !IsAnyFriendlyAbilitySelected())
		{
			SetTutorialStep(TutorialStep.MOUSEOVER_INSPECT);
		}
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
		case TutorialStep.MOUSEOVER_INSPECT:
		{
			SetEndTurnEnableAndBlocker(isEnabled: false);
			AddInputBlockerFriendlyAbilityZone();
			AddClickBlockerForFriendlyMinions();
			yield return new WaitForSeconds(0.5f);
			GameState.Get().SetBusy(busy: true);
			GameState.Get().SetBusy(busy: false);
			Card rightEnemyMinionInPlay = GetRightMostMinionInEnemyPlay();
			if (rightEnemyMinionInPlay != null)
			{
				ShowClickChampionTutorial(rightEnemyMinionInPlay, "GAMEPLAY_CHAMPION_MOUSEOVER_TUTORIAL");
			}
			yield return WaitForEnemyMouseOver();
			break;
		}
		case TutorialStep.MOUSEOVER_INSPECT_DONE:
			SetEndTurnEnableAndBlocker(isEnabled: true);
			RemoveInputBlockerFriendlyAbilityZone();
			RemoveClickBlockerForFriendlyMinions();
			ShowAllMercenaryAbilityOrderBubbles();
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
				tutorialPosition = new Vector3(cardPosition.x + 3.2f, cardPosition.y, cardPosition.z + 1f);
				popUpDirection = Notification.PopUpArrowDirection.Left;
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

	private Card GetRightMostMinionInEnemyPlay()
	{
		List<Card> cardsInPlay = GameState.Get().GetOpposingSidePlayer().GetBattlefieldZone()
			.GetCards();
		foreach (Card card in cardsInPlay)
		{
			if (card.GetEntity().GetTag(GAME_TAG.ZONE_POSITION) == cardsInPlay.Count)
			{
				return card;
			}
		}
		return null;
	}

	private IEnumerator WaitForEnemyMouseOver()
	{
		while (!InputManager.Get().GetMousedOverCard() || InputManager.Get().GetMousedOverCard() != GetRightMostMinionInEnemyPlay())
		{
			yield return null;
		}
		DestroyNotification(m_clickChampionNotification);
		yield return new WaitForSeconds(1f);
		while ((bool)InputManager.Get().GetMousedOverCard())
		{
			yield return null;
		}
		SetTutorialStep(TutorialStep.MOUSEOVER_INSPECT_DONE);
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

	public override bool NotifyOfBattlefieldCardClicked(Entity clickedEntity, bool wasInTargetMode)
	{
		Card entityCard = clickedEntity.GetCard();
		if (m_clickBlockedCards.Contains(entityCard) || (clickedEntity.IsControlledByOpposingSidePlayer() && !GameState.Get().IsInTargetMode()))
		{
			return false;
		}
		return true;
	}

	public override void NotifyOfStartOfTurnEventsFinished()
	{
		int tag = GetTag(GAME_TAG.TURN);
		int actionStep = GameState.Get().GetGameEntity().GetTag(GAME_TAG.ACTION_STEP_TYPE);
		if (tag == 1 && actionStep == 0)
		{
			SetTutorialStep(TutorialStep.MOUSEOVER_INSPECT);
		}
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
}
