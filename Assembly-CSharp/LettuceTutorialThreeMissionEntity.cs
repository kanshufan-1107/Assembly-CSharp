using System.Collections;
using System.Collections.Generic;
using Blizzard.T5.Core;
using UnityEngine;

public class LettuceTutorialThreeMissionEntity : LettuceMissionEntity
{
	private enum TutorialStep
	{
		Invalid,
		WEAKNESS_TUTORIAL,
		WEAKNESS_REMINDER_TUTORIAL
	}

	private static readonly Map<GameEntityOption, bool> s_booleanOptions = new Map<GameEntityOption, bool> { 
	{
		GameEntityOption.WAIT_FOR_RATING_INFO,
		false
	} };

	private static readonly Map<GameEntityOption, string> s_stringOptions = new Map<GameEntityOption, string>();

	protected List<GameObject> m_weaknessLabels = new List<GameObject>();

	private TutorialStep m_currentTutorialStep;

	private static readonly AssetReference VO_MurkEye_Male_Murloc_Bark_02 = new AssetReference("VO_MurkEye_Male_Murloc_Bark_02.prefab:1dab01cf2e464c13bca196c5933dce05");

	private Notification.SpeechBubbleDirection enemyMinionSpeakingDirection = Notification.SpeechBubbleDirection.BottomLeft;

	private Notification endTurnNotifier;

	public override void PreloadAssets()
	{
		base.PreloadAssets();
		foreach (string soundFile in new List<string> { VO_MurkEye_Male_Murloc_Bark_02 })
		{
			PreloadSound(soundFile);
		}
	}

	public LettuceTutorialThreeMissionEntity()
	{
		m_gameOptions.AddOptions(s_booleanOptions, s_stringOptions);
		m_abilityOrderSpeechBubblesEnabled = true;
		m_enemyAbilityOrderSpeechBubblesEnabled = false;
	}

	protected override void OnLettuceMissionEntityReconnect(int currentTurn)
	{
		if (currentTurn == 1 && !IsAnyFriendlyAbilitySelected())
		{
			SetTutorialStep(TutorialStep.WEAKNESS_TUTORIAL);
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
		if (!GameMgr.Get().IsSpectator())
		{
			switch (step)
			{
			case TutorialStep.WEAKNESS_TUTORIAL:
				CreateTutorialDialog(LettuceTutorialResources.LettuceTutorialPopupBonusDamagePrefab, "GAMEPLAY_LETTUCE_WEAKNESS_TITLE_TUTORIAL", "GAMEPLAY_LETTUCE_WEAKNESS_BODY_TUTORIAL", "GAMEPLAY_LETTUCE_WEAKNESS_BUTTON_TUTORIAL", UserPressedWeaknessTutorial, Vector2.zero);
				break;
			}
		}
		yield break;
	}

	protected void UserPressedWeaknessTutorial(UIEvent e)
	{
		SetTutorialStep(TutorialStep.WEAKNESS_REMINDER_TUTORIAL);
		GameEntity.Coroutines.StartCoroutine(PlayVOOnUserPressedWeaknessTutorial());
	}

	protected IEnumerator PlayVOOnUserPressedWeaknessTutorial()
	{
		float delayTime = 0.5f;
		string m_SpeakerActor = "LETL_026H_01";
		GameState.Get().SetBusy(busy: true);
		yield return new WaitForSeconds(delayTime);
		GameState.Get().SetBusy(busy: false);
		Actor speakerActor = FindEnemyActorInPlayByDesignCode(m_SpeakerActor);
		yield return PlayLineAlways(speakerActor, VO_MurkEye_Male_Murloc_Bark_02, enemyMinionSpeakingDirection);
	}

	protected IEnumerator PlayLineAlways(Actor speaker, string line, Notification.SpeechBubbleDirection direction, float duration = 2.5f)
	{
		yield return GameEntity.Coroutines.StartCoroutine(PlaySoundAndBlockSpeech(line, direction, speaker, duration));
	}

	public override void NotifyOfStartOfTurnEventsFinished()
	{
		if (GetTag(GAME_TAG.TURN) == 1)
		{
			SetTutorialStep(TutorialStep.WEAKNESS_TUTORIAL);
		}
	}

	private void WeaknessLabelLoadedCallback(AssetReference assetRef, GameObject go, object callbackData)
	{
		GameObject weaknessLabel = ((Card)callbackData).GetActor().GetHealthTextObject();
		if (weaknessLabel == null)
		{
			Object.Destroy(go);
			return;
		}
		Vector3 weaknessLabelPositionOffset;
		Vector3 weaknessLabelScale;
		if ((bool)UniversalInputManager.UsePhoneUI)
		{
			weaknessLabelPositionOffset = new Vector3(-0.35f, -0.5f, 0.4f);
			weaknessLabelScale = new Vector3(1.5f, 1.5f, 1.5f);
		}
		else
		{
			weaknessLabelPositionOffset = new Vector3(-0.5f, -0.65f, 0.4f);
			weaknessLabelScale = new Vector3(2.25f, 2.25f, 2.25f);
		}
		m_weaknessLabels.Add(go);
		go.transform.parent = weaknessLabel.transform;
		go.transform.localScale = Vector3.zero;
		go.transform.localPosition = weaknessLabelPositionOffset;
		go.transform.localEulerAngles = new Vector3(0f, 0f, 0f);
		iTween.ScaleTo(go, iTween.Hash("scale", weaknessLabelScale, "time", 2f, "easetype", iTween.EaseType.easeOutElastic));
		go.GetComponent<UberText>().Text = GameStrings.Get("GAMEPLAY_LETTUCE_WEAKNESS_LABEL");
	}

	public override bool NotifyOfEndTurnButtonPushed()
	{
		DestroyAllTutorialPopUps();
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

	protected void DestroyAllTutorialPopUps()
	{
		NotificationManager.Get().DestroyAllArrows();
		NotificationManager.Get().DestroyAllPopUps();
		NotificationManager.Get().DestroyAllNotificationsNowWithNoAnim();
		foreach (GameObject weaknessLabel in m_weaknessLabels)
		{
			Object.Destroy(weaknessLabel);
		}
		m_weaknessLabels.Clear();
	}

	public override void OnAbilityTrayShown(Entity entity)
	{
		foreach (GameObject weaknessLabel in m_weaknessLabels)
		{
			Object.Destroy(weaknessLabel);
		}
		m_weaknessLabels.Clear();
		if (!entity.IsMinion() || !entity.IsControlledByFriendlySidePlayer())
		{
			return;
		}
		foreach (Card card in ZoneMgr.Get().FindZoneOfType<ZonePlay>(Player.Side.OPPOSING).GetCards())
		{
			if (entity.IsMyLettuceRoleStrongAgainst(card.GetEntity()))
			{
				AssetLoader.Get().InstantiatePrefab("NumberLabel.prefab:597544d5ed24b994f95fe56e28584992", WeaknessLabelLoadedCallback, card, AssetLoadingOptions.IgnorePrefabPosition);
			}
		}
	}

	public override void OnAbilityTrayDismissed()
	{
		DestroyAllTutorialPopUps();
	}
}
