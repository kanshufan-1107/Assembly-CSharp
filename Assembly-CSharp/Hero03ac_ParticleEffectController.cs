using System;
using System.Collections.Generic;
using UnityEngine;

public class Hero03ac_ParticleEffectController : StateDrivenAnimation
{
	[Serializable]
	public class ParticleSystemBinding
	{
		public string Channel;

		public ParticleSystem ParticleSystem;

		public float MaxEmissionRate;
	}

	public ParticleSystemBinding[] ParticleSystems;

	protected override void UpdateAnimation(in Dictionary<string, float> channelValues)
	{
		if (ParticleSystems == null)
		{
			return;
		}
		ParticleSystemBinding[] particleSystems = ParticleSystems;
		foreach (ParticleSystemBinding binding in particleSystems)
		{
			if (binding.ParticleSystem != null && channelValues.TryGetValue(binding.Channel, out var weight))
			{
				ParticleSystem.EmissionModule emission = binding.ParticleSystem.emission;
				emission.rateOverTimeMultiplier = binding.MaxEmissionRate * weight;
			}
		}
	}
}
