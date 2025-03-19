using System;
using UnityEngine;
using UnityEngine.Timeline;

namespace Hearthstone.Timeline;

[TrackBindingType(typeof(Camera))]
[Obsolete("Use CameraShaker2 instead.", false)]
[TrackClipType(typeof(CameraShakerAsset))]
public class CameraShakerTrack : TrackAsset
{
}
