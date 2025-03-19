using System;
using UnityEngine;
using UnityEngine.Timeline;

namespace Hearthstone.Timeline;

[TrackBindingType(typeof(ParticleSystem))]
[Obsolete("Use ControlRandomized instead.", false)]
[TrackClipType(typeof(RandomSeedAsset))]
public class RandomSeedTrack : TrackAsset
{
}
