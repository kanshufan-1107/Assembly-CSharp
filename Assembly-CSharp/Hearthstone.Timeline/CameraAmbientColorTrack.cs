using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Hearthstone.Timeline;

[TrackClipType(typeof(CameraAmbientColorAsset))]
[Obsolete("Use CameraOverlay instead.", false)]
public class CameraAmbientColorTrack : TrackAsset
{
	public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
	{
		return ScriptPlayable<CameraAmbientColorBehaviour>.Create(graph, inputCount);
	}
}
