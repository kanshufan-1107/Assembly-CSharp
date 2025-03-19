using System;
using UnityEngine;
using UnityEngine.Timeline;

namespace Hearthstone.Timeline;

[Obsolete("Use CameraShaker2 instead.", false)]
[TrackBindingType(typeof(Camera))]
[TrackClipType(typeof(CameraShakerAsset))]
public class CameraShakerTrack : TrackAsset
{
}
