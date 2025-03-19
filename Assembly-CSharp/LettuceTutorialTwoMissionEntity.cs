using System.Collections;
using System.Collections.Generic;
using Blizzard.T5.Core;
using UnityEngine;

public class LettuceTutorialTwoMissionEntity : LettuceMissionEntity
{
	private enum TutorialStep
	{
		Invalid,
		SPEED_TUTORIAL
	}

	private static readonly Map<GameEntityOption, bool> s_booleanOptions = new Map<GameEntityOption, bool> { 
	{
		GameEntityOption.WAIT_FOR_RATING_INFO,
		false
	} };

	private static readonly Map<GameEntityOption, string> s_stringOptions = new Map<GameEntityOption, string>();

	private Notification endTurnNotifier;

	private TutorialStep m_currentTutorialStep;

	private static readonly AssetReference VO_DRG_082_Male_Kobold_Attack_02 = new AssetReference("VO_DRG_082_Male_Kobold_Attack_02.prefab:1941508e37e04de4fb8ae327e9e155a5");

	private static readonly AssetReference VO_DAL_416_Female_Kobold_Play_01 = new AssetReference("VO_DAL_416_Female_Kobold_Play_01.prefab:d3b15a1c1362c734da6573caa0976203");

	private Notification.SpeechBubbleDirection enemyMinionSpeakingDirection = Notification.SpeechBubbleDirection.BottomLeft;

	private TooltipPanel speedHelpPanel;

	private PlatformDependentValue<Vector3> m_speedTooltipPositionLeftmostAbility = new PlatformDependentValue<Vector3>(PlatformCategory.Screen)
	{
		PC = new Vector3(0.35f, 0f, -1.8f),
		Phone = new Vector3(1.5f, 0f, -2.2f)
	};

	private PlatformDependentValue<Vector3> m_speedTooltipPositionMiddleAbility = new PlatformDependentValue<Vector3>(PlatformCategory.Screen)
	{
		PC = new Vector3(-1.55f, 0f, -1.8f),
		Phone = new Vector3(-0.35f, 0f, -2.2f)
	};

	private PlatformDependentValue<float> m_gemScale = new PlatformDependentValue<float>(PlatformCategory.Screen)
	{
		PC = 1.75f,
		Phone = 1.5f
	};

	public override void PreloadAssets()
	{
		base.PreloadAssets();
		foreach (string soundFile in new List<string> { VO_DRG_082_Male_Kobold_Attack_02, VO_DAL_416_Female_Kobold_Play_01 })
		{
			PreloadSound(soundFile);
		}
	}

	public LettuceTutorialTwoMissionEntity()
	{
		m_gameOptions.AddOptions(s_booleanOptions, s_stringOptions);
		m_abilityOrderSpeechBubblesEnabled = true;
		m_enemyAbilityOrderSpeechBubblesEnabled = false;
	}

	protected override void OnLettuceMissionEntityReconnect(int currentTurn)
	{
		if (currentTurn == 1 && !IsAnyFriendlyAbilitySelected())
		{
			SetTutorialStep(TutorialStep.SPEED_TUTORIAL);
		}
	}

	private void SetTutorialStep(TutorialStep step)
	{
		m_currentTutorialStep = step;
		GameEntity.Coroutines.StartCoroutine(TransitionTutorialStepCoroutine(step));
	}

	private IEnumerator TransitionTutorialStepCoroutine(TutorialStep step)
	{
		if (!GameMgr.Get().IsSpectator() && step == TutorialStep.SPEED_TUTORIAL)
		{
			GameState.Get().SetBusy(busy: true);
			CreateTutorialDialog(LettuceTutorialResources.LettuceTutorialPopupSpeedPrefab, "GAMEPLAY_LETTUCE_SPEED_TITLE_TUTORIAL", "GAMEPLAY_LETTUCE_SPEED_BODY_TUTORIAL", "GAMEPLAY_LETTUCE_SPEED_BUTTON_TUTORIAL", UserPressedSpeedTutorial, Vector2.zero);
		}
		yield break;
	}

	private void UserPressedSpeedTutorial(UIEvent e)
	{
		GameState.Get().SetBusy(busy: false);
		GameEntity.Coroutines.StartCoroutine(PlayVOOnUserPressedSpeedTutorialSpeed());
	}

	protected IEnumerator PlayVOOnUserPressedSpeedTutorialSpeed()
	{
		float delayTime = 0.5f;
		string m_SpeakerActor = "LETL_810_01";
		GameState.Get().SetBusy(busy: true);
		yield return new WaitForSeconds(delayTime);
		GameState.Get().SetBusy(busy: false);
		Actor speakerActor = FindEnemyActorInPlayByDesignCode(m_SpeakerActor);
		yield return PlayLineAlways(speakerActor, VO_DAL_416_Female_Kobold_Play_01, enemyMinionSpeakingDirection);
	}

	protected IEnumerator PlayLineAlways(Actor speaker, string line, Notification.SpeechBubbleDirection direction, float duration = 2.5f)
	{
		yield return GameEntity.Coroutines.StartCoroutine(PlaySoundAndBlockSpeech(line, direction, speaker, duration));
	}

	public override void NotifyOfStartOfTurnEventsFinished()
	{
		if (GameState.Get().GetGameEntity().GetTag(GAME_TAG.TURN) == 1)
		{
			SetTutorialStep(TutorialStep.SPEED_TUTORIAL);
		}
	}

	public override bool NotifyOfCardTooltipDisplayShow(Card card)
	{
		if (GameState.Get().IsGameOver())
		{
			return false;
		}
		if (card.GetEntity().IsLettuceAbility() && speedHelpPanel == null)
		{
			ShowSpeedTooltip(card);
		}
		return true;
	}

	private void ShowSpeedTooltip(Card card)
	{
		LayerUtils.SetLayer(card.GetActor().GetCostTextObject().gameObject, GameLayer.Tooltip);
		Actor actor = card.GetActor();
		Vector3 abilityPosition = actor.transform.position;
		Vector3 offset = GetSpeedTooltipOffset(card);
		Vector3 tipPosition = new Vector3(abilityPosition.x + offset.x, abilityPosition.y + offset.y, abilityPosition.z + offset.z);
		speedHelpPanel = TooltipPanelManager.Get().CreateKeywordPanel(0);
		speedHelpPanel.Reset();
		speedHelpPanel.Initialize(GameStrings.Get("GAMEPLAY_LETTUCE_SPEED_LABEL_TUTORIAL"), GameStrings.Get("GAMEPLAY_LETTUCE_SPEED_TOOLTIP_TUTORIAL"));
		speedHelpPanel.SetScale(TooltipPanel.GAMEPLAY_SCALE);
		speedHelpPanel.transform.position = tipPosition;
		RenderUtils.SetAlpha(speedHelpPanel.gameObject, 0f);
		iTween.FadeTo(speedHelpPanel.gameObject, iTween.Hash("alpha", 1f, "time", 0.25f));
		GameObject go = actor.GetCostTextObject();
		EnlargeGameObject(go, m_gemScale);
		MercenaryRoleGemObject roleGemObject = actor.gameObject.GetComponentInChildren<MercenaryRoleGemObject>();
		if (roleGemObject != null)
		{
			EnlargeGameObject(roleGemObject.gameObject, m_gemScale);
		}
	}

	private Vector3 GetSpeedTooltipOffset(Card card)
	{
		if (card == null)
		{
			return Vector3.zero;
		}
		return ZoneMgr.Get().GetLettuceZoneController().GetAbilityTray()
			.GetTrayPositionOfAbility(card) switch
		{
			0 => m_speedTooltipPositionLeftmostAbility, 
			1 => m_speedTooltipPositionMiddleAbility, 
			_ => Vector3.zero, 
		};
	}

	public override void NotifyOfCardTooltipDisplayHide(Card card)
	{
		if (!(speedHelpPanel != null))
		{
			return;
		}
		if (card != null)
		{
			Actor actor = card.GetActor();
			GameObject go = actor.GetCostTextObject();
			LayerUtils.SetLayer(go, GameLayer.Default);
			ShrinkGameObject(go);
			MercenaryRoleGemObject roleGemObject = actor.gameObject.GetComponentInChildren<MercenaryRoleGemObject>();
			if (roleGemObject != null)
			{
				ShrinkGameObject(roleGemObject.gameObject);
			}
		}
		Object.Destroy(speedHelpPanel.gameObject);
	}

	public override List<TooltipPanelManager.TooltipPanelData> GetOverwriteKeywordHelpPanelDisplay(Entity ent)
	{
		if (ent == null)
		{
			return null;
		}
		if (ent.IsLettuceAbility())
		{
			return new List<TooltipPanelManager.TooltipPanelData>();
		}
		return base.GetOverwriteKeywordHelpPanelDisplay(ent);
	}

	private void EnlargeGameObject(GameObject gameObject, float scaleFactor)
	{
		iTween.Stop(gameObject);
		iTween.ScaleTo(gameObject, iTween.Hash("scale", new Vector3(scaleFactor, scaleFactor, scaleFactor), "time", 2f, "easetype", iTween.EaseType.easeOutElastic));
	}

	private void ShrinkGameObject(GameObject gameObject)
	{
		iTween.ScaleTo(gameObject, new Vector3(1f, 1f, 1f), 0.5f);
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
