using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Playables;

namespace Hearthstone.Timeline;

[Serializable]
public abstract class TimelineEffectBehaviour<T> : PlayableBehaviour, ITimelineEffectBehaviour where T : TimelineEffectHelper
{
	private bool m_playheadIsOnTrack;

	private PlayableDirector m_playableDirector;

	public PlayableAsset PlayableAsset
	{
		[CompilerGenerated]
		set
		{
			_003CPlayableAsset_003Ek__BackingField = value;
		}
	}

	protected T Helper { get; private set; }

	protected bool SpawnHelper(GameObject target, float duration, PlayableInfo playableInfo)
	{
		try
		{
			m_playableDirector = playableInfo.playable.GetGraph().GetResolver() as PlayableDirector;
		}
		catch
		{
		}
		if (Helper == null)
		{
			object[] initializationData = GetHelperInitializationData(playableInfo);
			if (initializationData != null)
			{
				Helper = TimelineEffectHelper.Spawn<T>(target, duration, this, initializationData);
			}
			return true;
		}
		return false;
	}

	public override void OnPlayableCreate(Playable playable)
	{
		m_playableDirector = playable.GetGraph().GetResolver() as PlayableDirector;
	}

	protected abstract object[] GetHelperInitializationData(PlayableInfo playableInfo);

	public override void OnGraphStop(Playable playable)
	{
		if (Helper != null)
		{
			Helper.Kill(TimelineEffectKillCause.Stop);
		}
	}

	public override void OnBehaviourPause(Playable playable, FrameData info)
	{
		if (Helper != null)
		{
			Helper.Kill(TimelineEffectKillCause.Pause);
		}
		double duration = playable.GetDuration();
		double time = playable.GetTime();
		float delta = info.deltaTime;
		if (info.evaluationType == FrameData.EvaluationType.Playback && time + (double)delta >= duration)
		{
			m_playheadIsOnTrack = false;
			OnExit(playable, info);
		}
	}

	protected abstract void InitializeFrame(Playable playable, FrameData info, object playerData);

	protected abstract void UpdateFrame(Playable playable, FrameData info, object playerData, float normalizedTime);

	public override void ProcessFrame(Playable playable, FrameData info, object playerData)
	{
		InitializeFrame(playable, info, playerData);
		if (Camera.main != null && SpawnHelper(Camera.main.gameObject, (float)playable.GetDuration(), new PlayableInfo
		{
			playable = playable,
			info = info,
			playerData = playerData
		}))
		{
			m_playheadIsOnTrack = true;
			OnEnter(playable, info);
		}
		if (Helper != null)
		{
			float normalizedTime = (float)playable.GetTime() / (float)playable.GetDuration();
			if (normalizedTime < 1f)
			{
				UpdateFrame(playable, info, playerData, normalizedTime);
			}
			else if (Helper != null)
			{
				Helper.Kill(TimelineEffectKillCause.Complete);
			}
		}
	}

	protected abstract void OnEnter(Playable playable, FrameData info);

	protected abstract void OnExit(Playable playable, FrameData info);

	public PlayableDirector GetDirector()
	{
		return m_playableDirector;
	}
}
