using System;
using UnityEngine;
using UnityEngine.Playables;

namespace Hearthstone.Timeline;

[Serializable]
[Obsolete("Use ControlRandomized instead.", false)]
public class RandomSeedBehaviour : PlayableBehaviour
{
	private System.Random m_random = new System.Random();

	private bool m_triggered;

	public override void ProcessFrame(Playable playable, FrameData info, object playerData)
	{
		if (!m_triggered && Application.isPlaying)
		{
			uint one = (uint)m_random.Next(65536);
			uint two = (uint)m_random.Next(65536);
			ParticleSystem obj = playerData as ParticleSystem;
			obj.Clear();
			obj.Stop();
			obj.randomSeed = (one << 16) | two;
			obj.Simulate(0f, withChildren: true, restart: true);
			obj.Play();
			m_triggered = true;
		}
	}
}
