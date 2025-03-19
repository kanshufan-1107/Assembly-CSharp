using UnityEngine;
using UnityEngine.Playables;

namespace Hearthstone.Timeline;

public abstract class TimelineEffectHelper : MonoBehaviour
{
	private bool m_listeningToPlayableDirector;

	protected float Duration { get; private set; } = 1f;

	protected bool ReceivedOriginalValues { get; private set; }

	protected bool Initialized { get; private set; }

	protected ITimelineEffectBehaviour Behaviour { get; private set; }

	public static T Spawn<T>(GameObject target, float duration, ITimelineEffectBehaviour behaviour, params object[] initializationData) where T : TimelineEffectHelper
	{
		T instance = target.AddComponent<T>();
		instance.Initialized = false;
		instance.SyncInstancesOfThisEffectType<T>(target);
		instance.Duration = duration;
		instance.Behaviour = behaviour;
		if (!instance.m_listeningToPlayableDirector)
		{
			if (instance.Behaviour != null && instance.Behaviour.GetDirector() != null)
			{
				instance.Behaviour.GetDirector().played += instance.OnDirectorPlayed;
				instance.Behaviour.GetDirector().paused += instance.OnDirectorPaused;
				instance.Behaviour.GetDirector().stopped += instance.OnDirectorStopped;
			}
			instance.m_listeningToPlayableDirector = true;
		}
		instance.Initialize(initializationData);
		instance.Initialized = true;
		instance.SyncInstancesOfThisEffectType<T>(target);
		return instance;
	}

	private void SyncInstancesOfThisEffectType<T>(GameObject target) where T : TimelineEffectHelper
	{
		T[] components = target.GetComponents<T>();
		foreach (T other in components)
		{
			if (other != null && other != this)
			{
				if (Initialized && !other.Initialized)
				{
					other.CopyOriginalValuesFrom(this);
					other.ReceivedOriginalValues = true;
				}
				else if (!Initialized && other.Initialized)
				{
					CopyOriginalValuesFrom(other);
					ReceivedOriginalValues = true;
				}
			}
		}
	}

	protected abstract void CopyOriginalValuesFrom<T>(T other) where T : TimelineEffectHelper;

	protected abstract void Initialize(params object[] values);

	private void OnDisable()
	{
		Kill(TimelineEffectKillCause.Disable);
	}

	private void OnDestroy()
	{
		OnDestroyTimelineEffectHelper();
		if (this != null && base.gameObject != null)
		{
			ResetTarget(TimelineEffectResetCause.Destroy);
		}
	}

	protected virtual void OnDestroyTimelineEffectHelper()
	{
	}

	public void Kill(TimelineEffectKillCause cause)
	{
		if (this != null && base.gameObject != null)
		{
			ResetTarget(TimelineEffectResetCause.Kill);
			if (Application.isPlaying)
			{
				Object.Destroy(this);
			}
			else
			{
				Object.DestroyImmediate(this);
			}
		}
		if (m_listeningToPlayableDirector)
		{
			if (Behaviour != null && Behaviour.GetDirector() != null)
			{
				Behaviour.GetDirector().played -= OnDirectorPlayed;
				Behaviour.GetDirector().paused -= OnDirectorPaused;
				Behaviour.GetDirector().stopped -= OnDirectorStopped;
			}
			m_listeningToPlayableDirector = false;
		}
		OnKill(cause);
	}

	protected virtual void OnDirectorPlayed(PlayableDirector obj)
	{
	}

	protected virtual void OnDirectorPaused(PlayableDirector obj)
	{
	}

	protected virtual void OnDirectorStopped(PlayableDirector obj)
	{
	}

	protected abstract void OnKill(TimelineEffectKillCause cause);

	protected abstract void ResetTarget(TimelineEffectResetCause cause);

	protected abstract void UpdateTarget(float normalizedTime);

	public void UpdateEffect(float timeSinceStarted)
	{
		UpdateEffect(timeSinceStarted, isScrubbing: false);
	}

	public void UpdateEffect(float timeSinceStarted, bool isScrubbing)
	{
		float normalizedTime = Mathf.Clamp(timeSinceStarted / Duration, 0f, 1f);
		if (!Mathf.Approximately(normalizedTime, 0f) && !Mathf.Approximately(normalizedTime, 1f))
		{
			UpdateTarget(normalizedTime);
		}
		else
		{
			ResetTarget(TimelineEffectResetCause.Other);
		}
	}
}
