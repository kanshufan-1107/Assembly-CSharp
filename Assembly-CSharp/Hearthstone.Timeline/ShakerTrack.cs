using UnityEngine;
using UnityEngine.Timeline;

namespace Hearthstone.Timeline;

[TrackBindingType(typeof(Transform))]
[TrackClipType(typeof(ShakerAsset))]
public class ShakerTrack : TrackAsset
{
}
