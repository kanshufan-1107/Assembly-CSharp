using System;
using UnityEngine;
using UnityEngine.Playables;

namespace Hearthstone.Timeline;

[Serializable]
public class ShakerBehaviour : TimelineEffectBehaviour<ShakerHelper>
{
	[SerializeField]
	[Tooltip("Max distance the shake can move (scaled by the intensity curve).")]
	private Vector3 m_Amount = new Vector3(0.1f, 0.1f, 0f);

	[SerializeField]
	[Tooltip("Build or falloff over time.")]
	private AnimationCurve m_IntensityCurve = new AnimationCurve(new Keyframe(0f, 1f, 0f, -2f), new Keyframe(1f, 0f, 0f, 0f));

	[Range(0f, 0.25f)]
	[Tooltip("Time between shakes (zero for every frame).")]
	[SerializeField]
	private float m_interval = 0.075f;

	protected override object[] GetHelperInitializationData(PlayableInfo _)
	{
		return new object[3] { m_Amount, m_IntensityCurve, m_interval };
	}

	protected override void InitializeFrame(Playable playable, FrameData info, object playerData)
	{
		if (playerData != null && (base.Helper == null || (playerData as Component).gameObject != base.Helper.gameObject))
		{
			if (base.Helper != null)
			{
				base.Helper.Kill(TimelineEffectKillCause.Other);
			}
			SpawnHelper((playerData as Component).gameObject, (float)playable.GetDuration(), default(PlayableInfo));
		}
	}

	protected override void OnEnter(Playable playable, FrameData info)
	{
	}

	protected override void OnExit(Playable playable, FrameData info)
	{
	}

	protected override void UpdateFrame(Playable playable, FrameData info, object playerData, float normalizedTime)
	{
		base.Helper.UpdateEffect((float)playable.GetTime(), !playable.GetGraph().IsPlaying());
	}
}
