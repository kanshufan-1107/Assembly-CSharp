using System;
using System.Collections;
using System.Collections.Generic;
using Blizzard.T5.Game.Cutscene;
using UnityEngine;

public class CutsceneTimeline : MonoBehaviour
{
	[Header("Interactable Elements")]
	[SerializeField]
	private CutsceneCaptionDriver m_captionDriver;

	[SerializeField]
	private CutsceneAttackSpellController m_attackSpellController;

	[Min(0f)]
	[SerializeField]
	[Tooltip("Time (seconds) that the active action can run for before timing out. (Note: Action can override this value)")]
	[Header("Timeline Config")]
	private float m_defaultActionTimeoutSeconds = 13f;

	[Min(0f)]
	[Tooltip("Delay (seconds) between transitioned to next action and that action executing - e.g. hero power not firing straight away. (Note: Action can override this value)")]
	[SerializeField]
	private float m_defaultActionStartDelaySeconds = 1f;

	[Tooltip("Delay (seconds) between action execution finishing and start of transition to next action. (Note: Action can override this value)")]
	[Min(0f)]
	[SerializeField]
	private float m_defaultActionFinishDelaySeconds = 2f;

	private readonly List<TimelineAction> m_timelineActions = new List<TimelineAction>();

	private Coroutine m_timelineCoroutine;

	private Coroutine m_actionTimeoutCoroutine;

	public bool IsRunning => m_timelineCoroutine != null;

	private void OnDestroy()
	{
		CleanUp();
	}

	public void CreateTimeline(List<CutsceneSceneDef.CutsceneActionRequest> sceneActionsRequests, in CutsceneLoadedActors loadedActors)
	{
		CleanUp();
		if (sceneActionsRequests == null || sceneActionsRequests.Count == 0)
		{
			return;
		}
		foreach (CutsceneSceneDef.CutsceneActionRequest request in sceneActionsRequests)
		{
			if (request == null || request.ActionType == CutsceneSceneDef.ActionType.NONE)
			{
				continue;
			}
			TimelineAction action;
			switch (request.ActionType)
			{
			case CutsceneSceneDef.ActionType.ATTACK:
				action = new AttackTimelineAction(request.SourceCard.TargetSide == Player.Side.FRIENDLY, m_attackSpellController, GetLoadedActorDataFromRequest(request.SourceCard, in loadedActors), GetLoadedActorDataFromRequest(request.TargetCard, in loadedActors));
				break;
			case CutsceneSceneDef.ActionType.HERO_POWER:
				action = new SuperSpellTimelineAction(GetLoadedActorDataFromRequest(request.SourceCard, in loadedActors), GetLoadedActorDataFromRequest(request.TargetCard, in loadedActors));
				break;
			case CutsceneSceneDef.ActionType.SUMMON:
				if (request.SourceCard == null)
				{
					continue;
				}
				if (request.SourceCard.TargetCardType == CutsceneSceneDef.CardType.HERO)
				{
					action = new SocketInTimelineAction(request.SourceCard.TargetSide, GetLoadedActorDataFromRequest(request.SourceCard, in loadedActors), request, GetHideableActors(request, in loadedActors));
					break;
				}
				if (request.SourceCard.TargetCardType == CutsceneSceneDef.CardType.ALTERNATE_FORM)
				{
					action = new HeroSwapTimelineAction(request.SourceCard.TargetSide, loadedActors.FriendlyHeroActor, GetLoadedActorDataFromRequest(request.SourceCard, in loadedActors), request, GetHideableActors(request, in loadedActors));
					break;
				}
				if (request.SourceCard.TargetCardType != CutsceneSceneDef.CardType.MINION)
				{
					continue;
				}
				action = new SummonMinionTimelineAction(GetLoadedActorDataFromRequest(request.SourceCard, in loadedActors, pickRandomFromCollections: false));
				break;
			case CutsceneSceneDef.ActionType.SPELL:
			{
				Actor source = GetLoadedActorDataFromRequest(request.SourceCard, in loadedActors);
				Actor target = GetLoadedActorDataFromRequest(request.TargetCard, in loadedActors);
				action = ((!request.HasCustomSpell()) ? new SpellTimelineAction(request.SpellType, source, target) : ((!(request.ActionCustomSpell is SuperSpell superSpell)) ? new SpellTimelineAction(request.ActionCustomSpell as Spell, source, target) : new SuperSpellTimelineAction(superSpell, source, target)));
				break;
			}
			case CutsceneSceneDef.ActionType.EMOTE:
				action = new EmoteTimelineAction(request.EmoteType, loadedActors.FriendlyHeroActor);
				break;
			case CutsceneSceneDef.ActionType.RESET:
				action = new ResetTimelineAction(GetHideableActors(request, in loadedActors), loadedActors.FriendlyHeroActor, loadedActors.FriendlyAlternateFormHeroActor);
				break;
			default:
				Log.CosmeticPreview.PrintWarning(string.Format("{0} failed to create {1} due to unhandled request type: {2}", "CutsceneTimeline", "SpellTimelineAction", request.ActionType));
				continue;
			}
			SetActionDisplayPropertiesIfPresent(request, action);
			m_timelineActions.Add(action);
		}
	}

	public void PlayTimeline()
	{
		if (m_timelineCoroutine == null)
		{
			m_timelineCoroutine = StartCoroutine(TimelineCoroutine());
		}
	}

	public void StopTimeline(bool isFullStop = false)
	{
		if (m_timelineCoroutine != null)
		{
			StopCoroutine(m_timelineCoroutine);
			m_timelineCoroutine = null;
		}
		foreach (TimelineAction timelineAction in m_timelineActions)
		{
			timelineAction?.Stop();
		}
		m_captionDriver.ClearCaptionText(isFullStop);
	}

	public void Dispose()
	{
		CleanUp();
	}

	private IEnumerator TimelineCoroutine()
	{
		if (m_timelineActions.Count == 0)
		{
			StopTimeline();
			yield break;
		}
		foreach (TimelineAction timelineAction in m_timelineActions)
		{
			timelineAction.Init();
			yield return null;
		}
		while (m_timelineCoroutine != null)
		{
			foreach (TimelineAction action in m_timelineActions)
			{
				if (action.IsReady)
				{
					yield return new WaitForSeconds(Math.Max(action.StartDelayOverrideSeconds ?? m_defaultActionStartDelaySeconds, 0f));
					if (m_actionTimeoutCoroutine != null)
					{
						StopCoroutine(m_actionTimeoutCoroutine);
					}
					m_actionTimeoutCoroutine = StartCoroutine(TimelineActionTimeoutTimer(action));
					m_captionDriver.SetCaptionText(action.CaptionLocalizedString);
					yield return action.Play();
					if (m_actionTimeoutCoroutine != null)
					{
						StopCoroutine(m_actionTimeoutCoroutine);
						m_actionTimeoutCoroutine = null;
					}
					if (action.ResetAfterPlay)
					{
						action.Reset();
					}
					yield return new WaitForSeconds(Math.Max(action.EndDelayOverrideSeconds ?? m_defaultActionFinishDelaySeconds, 0f));
					action.Stop();
				}
			}
		}
	}

	private IEnumerator TimelineActionTimeoutTimer(TimelineAction action)
	{
		if (action != null)
		{
			float waitTimeSec = action.TimeoutOverrideSeconds ?? m_defaultActionTimeoutSeconds;
			if (!(waitTimeSec <= 0f))
			{
				yield return new WaitForSeconds(waitTimeSec);
				action?.Stop();
			}
		}
	}

	private void CleanUp()
	{
		StopTimeline(isFullStop: true);
		foreach (TimelineAction timelineAction in m_timelineActions)
		{
			timelineAction.Dispose();
		}
		m_timelineActions.Clear();
	}

	private static void SetActionDisplayPropertiesIfPresent(CutsceneSceneDef.CutsceneActionRequest request, TimelineAction action)
	{
		if (request != null && action != null)
		{
			action.CaptionLocalizedString = request.CaptionLocalizedString;
			if (request.HasStartDelayOverride && request.StartDelayOverrideSeconds >= 0f)
			{
				action.StartDelayOverrideSeconds = request.StartDelayOverrideSeconds;
			}
			if (request.HasEndDelayOverride && request.EndDelayOverrideSeconds >= 0f)
			{
				action.EndDelayOverrideSeconds = request.EndDelayOverrideSeconds;
			}
			if (request.HasTimeOutOverride && request.TimeoutOverrideSeconds >= 0f)
			{
				action.TimeoutOverrideSeconds = request.TimeoutOverrideSeconds;
			}
			action.ResetAfterPlay = request.ResetAfterPlay;
		}
	}

	private static Actor GetLoadedActorDataFromRequest(CutsceneSceneDef.CutsceneActionTarget requestTarget, in CutsceneLoadedActors actors, bool pickRandomFromCollections = true)
	{
		if (requestTarget == null || requestTarget.TargetSide == Player.Side.NEUTRAL)
		{
			return null;
		}
		switch (requestTarget.TargetCardType)
		{
		case CutsceneSceneDef.CardType.HERO:
			if (requestTarget.TargetSide != Player.Side.FRIENDLY)
			{
				return actors.OpponentHeroActor;
			}
			return actors.FriendlyHeroActor;
		case CutsceneSceneDef.CardType.ALTERNATE_FORM:
			return actors.FriendlyAlternateFormHeroActor;
		case CutsceneSceneDef.CardType.MINION:
			if (requestTarget.TargetSide == Player.Side.FRIENDLY)
			{
				List<Actor> friendlyMinions = actors.FriendlyMinions;
				if (friendlyMinions != null && friendlyMinions.Count > 0)
				{
					int minionCount = actors.FriendlyMinions.Count;
					int index = ((pickRandomFromCollections && minionCount > 1) ? UnityEngine.Random.Range(0, minionCount) : (minionCount - 1));
					return actors.FriendlyMinions[index];
				}
			}
			if (requestTarget.TargetSide == Player.Side.OPPOSING)
			{
				List<Actor> opponentMinions = actors.OpponentMinions;
				if (opponentMinions != null && opponentMinions.Count > 0)
				{
					int minionCount2 = actors.OpponentMinions.Count;
					int index2 = ((pickRandomFromCollections && minionCount2 > 1) ? UnityEngine.Random.Range(0, minionCount2) : (minionCount2 - 1));
					return actors.OpponentMinions[index2];
				}
			}
			break;
		case CutsceneSceneDef.CardType.HERO_POWER:
			if (requestTarget.TargetSide != Player.Side.FRIENDLY)
			{
				return actors.OpponentHeroPowerActor;
			}
			return actors.FriendlyHeroPowerActor;
		default:
			Log.CosmeticPreview.PrintWarning($"Failed to get actor for action request due to unhandled type: {requestTarget.TargetCardType}");
			break;
		}
		return null;
	}

	private static List<Actor> GetHideableActors(CutsceneSceneDef.CutsceneActionRequest request, in CutsceneLoadedActors loadedActors)
	{
		List<Actor> hideableActors = new List<Actor>();
		Player.Side side = request.SourceCard.TargetSide;
		CutsceneSceneDef.ActionType actionType = request.ActionType;
		_ = request.SourceCard.TargetCardType;
		if ((actionType == CutsceneSceneDef.ActionType.RESET || side == Player.Side.FRIENDLY) && loadedActors.FriendlyMinions != null)
		{
			hideableActors.AddRange(loadedActors.FriendlyMinions);
		}
		if ((actionType == CutsceneSceneDef.ActionType.RESET || side == Player.Side.OPPOSING) && loadedActors.OpponentMinions != null)
		{
			hideableActors.AddRange(loadedActors.OpponentMinions);
		}
		return hideableActors;
	}
}
