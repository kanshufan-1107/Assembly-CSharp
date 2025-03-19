using System;
using UnityEngine;
using UnityEngine.Timeline;

namespace Hearthstone.Timeline;

[TrackClipType(typeof(RandomSeedAsset))]
[Obsolete("Use ControlRandomized instead.", false)]
[TrackBindingType(typeof(ParticleSystem))]
public class RandomSeedTrack : TrackAsset
{
}
