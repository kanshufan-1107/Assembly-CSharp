using System;
using UnityEngine.Playables;

namespace Hearthstone.Timeline;

[Serializable]
public abstract class FireAndForgetBehaviour : TimelineEffectBehaviour<FireAndForgetHelper>
{
	protected override object[] GetHelperInitializationData(PlayableInfo _)
	{
		OnFireAndForgetInitialize();
		return new object[0];
	}

	protected override void InitializeFrame(Playable playable, FrameData info, object playerData)
	{
	}

	protected override void OnEnter(Playable playable, FrameData info)
	{
		OnFireAndForgetEnter(playable, info);
	}

	protected override void OnExit(Playable playable, FrameData info)
	{
		OnFireAndForgetExit(playable, info);
	}

	protected override void UpdateFrame(Playable playable, FrameData info, object playerData, float normalizedTime)
	{
		base.Helper.UpdateEffect((float)playable.GetTime(), !playable.GetGraph().IsPlaying());
	}

	protected abstract void OnFireAndForgetInitialize();

	protected abstract void OnFireAndForgetEnter(Playable playable, FrameData info);

	protected abstract void OnFireAndForgetExit(Playable playable, FrameData info);
}
