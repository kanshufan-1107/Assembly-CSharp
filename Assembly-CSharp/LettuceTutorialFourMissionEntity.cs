using System.Collections;
using System.Collections.Generic;
using Blizzard.T5.Core;
using UnityEngine;

public class LettuceTutorialFourMissionEntity : LettuceMissionEntity
{
	private enum TutorialStep
	{
		Invalid,
		BENCH_TUTORIAL,
		BENCH_TUTORIAL_CLICKED,
		BENCH_DONE_TUTORIAL,
		REPLACEMENT
	}

	private static readonly Map<GameEntityOption, bool> s_booleanOptions = new Map<GameEntityOption, bool> { 
	{
		GameEntityOption.WAIT_FOR_RATING_INFO,
		false
	} };

	private static readonly Map<GameEntityOption, string> s_stringOptions = new Map<GameEntityOption, string>();

	private Notification m_handBounceArrow;

	private bool m_shouldShowHandBounceArrow;

	private TutorialStep m_currentTutorialStep;

	private static readonly AssetReference VO_NEW1_024_Attack_02 = new AssetReference("VO_NEW1_024_Attack_02.prefab:2e3944f60849f71409744641036cd71e");

	private static readonly AssetReference VO_BGS_053_Male_Orc_Play_01 = new AssetReference("VO_BGS_053_Male_Orc_Play_01.prefab:e44994899574498429f95fa821363676");

	private Notification.SpeechBubbleDirection enemyMinionSpeakingDirection = Notification.SpeechBubbleDirection.BottomLeft;

	private Notification endTurnNotifier;

	public override void PreloadAssets()
	{
		base.PreloadAssets();
		foreach (string soundFile in new List<string> { VO_NEW1_024_Attack_02, VO_BGS_053_Male_Orc_Play_01 })
		{
			PreloadSound(soundFile);
		}
	}

	public LettuceTutorialFourMissionEntity()
	{
		m_gameOptions.AddOptions(s_booleanOptions, s_stringOptions);
		m_abilityOrderSpeechBubblesEnabled = true;
		m_enemyAbilityOrderSpeechBubblesEnabled = false;
	}

	protected override void OnLettuceMissionEntityReconnect(int currentTurn)
	{
		if (GetTag<ACTION_STEP_TYPE>(GAME_TAG.ACTION_STEP_TYPE) != ACTION_STEP_TYPE.LETTUCE_MERCENARY_SELECTION)
		{
			return;
		}
		switch (currentTurn)
		{
		case 1:
			if (GameState.Get().GetFriendlySidePlayer().GetBattlefieldZone()
				.GetCards()
				.Count == 0)
			{
				SetTutorialStep(TutorialStep.BENCH_TUTORIAL);
			}
			break;
		case 2:
			if (GameState.Get().GetFriendlySidePlayer().GetHandZone()
				.GetCards()
				.Count > 0)
			{
				SetTutorialStep(TutorialStep.REPLACEMENT);
			}
			break;
		}
	}

	public override void NotifyOfStartOfTurnEventsFinished()
	{
		int tag = GetTag(GAME_TAG.TURN);
		int actionStep = GameState.Get().GetGameEntity().GetTag(GAME_TAG.ACTION_STEP_TYPE);
		if (tag == 1 && actionStep == 1)
		{
			SetTutorialStep(TutorialStep.BENCH_TUTORIAL);
		}
		if (tag == 2 && actionStep == 1)
		{
			SetTutorialStep(TutorialStep.REPLACEMENT);
		}
		if (actionStep == 0)
		{
			ShowAllMercenaryAbilityOrderBubbles();
		}
	}

	private void SetTutorialStep(TutorialStep step)
	{
		m_currentTutorialStep = step;
		GameEntity.Coroutines.StartCoroutine(TransitionTutorialStepCoroutine(step));
	}

	private IEnumerator TransitionTutorialStepCoroutine(TutorialStep step)
	{
		if (!GameMgr.Get().IsSpectator())
		{
			switch (step)
			{
			case TutorialStep.BENCH_TUTORIAL:
				EndTurnButton.Get().AddInputBlocker();
				GameState.Get().GetFriendlySidePlayer().GetHandZone()
					.AddInputBlocker();
				CreateTutorialDialog(LettuceTutorialResources.LettuceTutorialPopupBenchPrefab, "GAMEPLAY_LETTUCE_BENCH_TITLE_TUTORIAL", "GAMEPLAY_LETTUCE_BENCH_BODY_TUTORIAL", "GAMEPLAY_LETTUCE_BENCH_BUTTON_TUTORIAL", UserPressedBenchTutorial, Vector2.zero);
				break;
			case TutorialStep.BENCH_TUTORIAL_CLICKED:
				GameState.Get().GetFriendlySidePlayer().GetHandZone()
					.RemoveInputBlocker();
				GameState.Get().SetBusy(busy: false);
				yield return new WaitForSeconds(5f);
				ShowHandBounceArrow();
				break;
			case TutorialStep.BENCH_DONE_TUTORIAL:
				HideHandBounceArrow();
				break;
			case TutorialStep.REPLACEMENT:
				GameState.Get().SetBusy(busy: true);
				CreateTutorialDialog(LettuceTutorialResources.LettuceTutorialPopupDeathReplacePrefab, "GAMEPLAY_LETTUCE_REPLACEMENT_TITLE_TUTORIAL", "GAMEPLAY_LETTUCE_REPLACEMENT_BODY_TUTORIAL", "GAMEPLAY_LETTUCE_REPLACEMENT_BUTTON_TUTORIAL", UserPressedReplacementTutorial, Vector2.zero);
				break;
			}
		}
	}

	private void UserPressedBenchTutorial(UIEvent e)
	{
		SetTutorialStep(TutorialStep.BENCH_TUTORIAL_CLICKED);
		GameEntity.Coroutines.StartCoroutine(PlayVOOnUserPressedReplacementTutorial());
	}

	protected IEnumerator PlayVOOnUserPressedReplacementTutorial()
	{
		float delayTime = 0.5f;
		string m_SpeakerActor = "LETL_813_H1";
		GameState.Get().SetBusy(busy: true);
		yield return new WaitForSeconds(delayTime);
		GameState.Get().SetBusy(busy: false);
		Actor speakerActor = FindEnemyActorInPlayByDesignCode(m_SpeakerActor);
		yield return PlayLineAlways(speakerActor, VO_NEW1_024_Attack_02, enemyMinionSpeakingDirection);
	}

	protected IEnumerator PlayLineAlways(Actor speaker, string line, Notification.SpeechBubbleDirection direction, float duration = 2.5f)
	{
		yield return GameEntity.Coroutines.StartCoroutine(PlaySoundAndBlockSpeech(line, direction, speaker, duration));
	}

	private void UserPressedReplacementTutorial(UIEvent e)
	{
		GameState.Get().SetBusy(busy: false);
	}

	private void HideNotification(Notification notification, bool hideImmediately = false)
	{
		if (notification != null)
		{
			if (hideImmediately)
			{
				NotificationManager.Get().DestroyNotificationNowWithNoAnim(notification);
			}
			else
			{
				NotificationManager.Get().DestroyNotification(notification, 0f);
			}
		}
	}

	public override void NotifyOfCardMousedOver(Entity mousedOverEntity)
	{
		base.NotifyOfCardMousedOver(mousedOverEntity);
		if (mousedOverEntity.GetZone() == TAG_ZONE.HAND)
		{
			HideNotification(m_handBounceArrow);
		}
	}

	public override void NotifyOfCardMousedOff(Entity mousedOffEntity)
	{
		base.NotifyOfCardMousedOff(mousedOffEntity);
		if (mousedOffEntity.GetZone() == TAG_ZONE.HAND && m_shouldShowHandBounceArrow)
		{
			Gameplay.Get().StartCoroutine(ShowArrowInSeconds(10f));
		}
	}

	public override void NotifyOfCardGrabbed(Entity grabbedEntity)
	{
		if (grabbedEntity.GetZone() == TAG_ZONE.HAND && m_shouldShowHandBounceArrow)
		{
			HideNotification(m_handBounceArrow);
		}
	}

	public override void NotifyOfCardDropped(Entity entity)
	{
		if (GameState.Get().GetGameEntity().GetTag(GAME_TAG.TURN) <= 1)
		{
			if (GameState.Get().GetFriendlySidePlayer().GetBattlefieldZone()
				.GetCardCount() < 2)
			{
				Gameplay.Get().StartCoroutine(ShowArrowInSeconds(10f));
				return;
			}
			HideHandBounceArrow();
			EndTurnButton.Get().RemoveInputBlocker();
			EndTurnButton.Get().Reset();
		}
	}

	private Card GetNominateSuggestionCard()
	{
		List<Card> cardsInHand = GameState.Get().GetFriendlySidePlayer().GetHandZone()
			.GetCards();
		if (cardsInHand.Count == 0)
		{
			return null;
		}
		foreach (TAG_ROLE role in new List<TAG_ROLE>
		{
			TAG_ROLE.TANK,
			TAG_ROLE.FIGHTER,
			TAG_ROLE.CASTER
		})
		{
			Card suggestedCard = GetCardByRole(cardsInHand, role);
			if (suggestedCard != null)
			{
				return suggestedCard;
			}
		}
		return null;
	}

	private Card GetCardByRole(List<Card> cards, TAG_ROLE role)
	{
		foreach (Card card in cards)
		{
			if (card.GetEntity().GetTag<TAG_ROLE>(GAME_TAG.LETTUCE_ROLE) == role)
			{
				return card;
			}
		}
		return null;
	}

	private void ShowHandBounceArrow()
	{
		m_shouldShowHandBounceArrow = true;
		HideNotification(m_handBounceArrow);
		Card suggestedCard = GetNominateSuggestionCard();
		if (!(suggestedCard == null))
		{
			Vector3 cardInHandPosition = suggestedCard.transform.position;
			Vector3 bounceArrowPos = ((!UniversalInputManager.UsePhoneUI) ? new Vector3(cardInHandPosition.x, cardInHandPosition.y, cardInHandPosition.z + 2f) : new Vector3(cardInHandPosition.x - 0.08f, cardInHandPosition.y + 0.2f, cardInHandPosition.z + 1.2f));
			m_handBounceArrow = NotificationManager.Get().CreateBouncingArrow(UserAttentionBlocker.NONE, bounceArrowPos, new Vector3(0f, 0f, 0f));
			m_handBounceArrow.transform.parent = suggestedCard.transform;
		}
	}

	private void HideHandBounceArrow()
	{
		m_shouldShowHandBounceArrow = false;
		HideNotification(m_handBounceArrow);
	}

	private IEnumerator ShowArrowInSeconds(float seconds)
	{
		yield return new WaitForSeconds(seconds);
		List<Card> handCards = GameState.Get().GetFriendlySidePlayer().GetHandZone()
			.GetCards();
		if (handCards.Count != 0)
		{
			Card cardInHand = handCards[0];
			while (iTween.Count(cardInHand.gameObject) > 0)
			{
				yield return null;
			}
			if (!cardInHand.IsMousedOver() && !(InputManager.Get().GetHeldCard() == cardInHand) && m_shouldShowHandBounceArrow)
			{
				ShowHandBounceArrow();
			}
		}
	}

	public override bool NotifyOfEndTurnButtonPushed()
	{
		if (GetTag(GAME_TAG.TURN) >= 1)
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
}
