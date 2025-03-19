using UnityEngine;
using UnityEngine.Timeline;

namespace Hearthstone.Timeline;

[TrackBindingType(typeof(Camera))]
[TrackClipType(typeof(CameraShaker2Asset))]
public class CameraShaker2Track : TrackAsset
{
}
