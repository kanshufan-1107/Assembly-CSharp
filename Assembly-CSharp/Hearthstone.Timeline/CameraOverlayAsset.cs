using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Hearthstone.Timeline;

[NotKeyable]
public class CameraOverlayAsset : PlayableAsset, ITimelineClipAsset
{
	public CameraOverlayBehaviour template;

	public ClipCaps clipCaps => ClipCaps.None;

	public CameraOverlayBehaviour Behaviour { get; private set; }

	public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
	{
		ScriptPlayable<CameraOverlayBehaviour> playable = ScriptPlayable<CameraOverlayBehaviour>.Create(graph, template);
		Behaviour = playable.GetBehaviour();
		return playable;
	}
}
