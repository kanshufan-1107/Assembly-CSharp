using System;
using UnityEngine;
using UnityEngine.Playables;

namespace Hearthstone.Timeline;

[Serializable]
public class CameraShaker2Behaviour : ShakerBehaviour
{
	protected override void InitializeFrame(Playable playable, FrameData info, object playerData)
	{
		if (base.Helper != null)
		{
			base.Helper.Kill(TimelineEffectKillCause.Other);
		}
		if (!Application.isPlaying && playerData != null && (base.Helper == null || (playerData as Component).gameObject != base.Helper.gameObject))
		{
			SpawnHelper((playerData as Component).gameObject, (float)playable.GetDuration(), default(PlayableInfo));
		}
		else if (Camera.main != null)
		{
			SpawnHelper(Camera.main.gameObject, (float)playable.GetDuration(), default(PlayableInfo));
		}
	}
}
