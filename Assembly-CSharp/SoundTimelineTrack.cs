using UnityEngine;
using UnityEngine.Timeline;

[TrackBindingType(typeof(AudioSource))]
[TrackClipType(typeof(SoundTimelineAsset))]
public class SoundTimelineTrack : TrackAsset
{
}
