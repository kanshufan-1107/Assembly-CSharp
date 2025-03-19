using System;
using UnityEngine;
using UnityEngine.Timeline;

namespace Hearthstone.Timeline;

[Obsolete("Use ControlRandomized instead.", false)]
[TrackBindingType(typeof(ParticleSystem))]
[TrackClipType(typeof(RandomSeedAsset))]
public class RandomSeedTrack : TrackAsset
{
}
