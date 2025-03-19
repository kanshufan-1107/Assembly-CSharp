using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

namespace Hearthstone.Timeline;

[Serializable]
public class ControlRandomizedBehaviour : TimelineEffectBehaviour<ControlRandomizedHelper>
{
	private class ParticleSystemWithData
	{
		private ParticleSystem m_baseParticleSystem;

		private ParticleSystem[] m_particleSystems = new ParticleSystem[0];

		private bool[] m_useAutoRandomSeeds = new bool[0];

		public bool IsAlive
		{
			get
			{
				if (m_particleSystems == null || m_particleSystems.Length == 0)
				{
					return false;
				}
				ParticleSystem[] particleSystems = m_particleSystems;
				for (int i = 0; i < particleSystems.Length; i++)
				{
					if (!particleSystems[i].IsAlive())
					{
						return false;
					}
				}
				return true;
			}
		}

		public ParticleSystemWithData(ParticleSystem particleSystem)
		{
			if (!(particleSystem == null))
			{
				m_baseParticleSystem = particleSystem;
				m_particleSystems = m_baseParticleSystem.GetComponentsInChildren<ParticleSystem>();
				m_useAutoRandomSeeds = new bool[m_particleSystems.Length];
				for (int i = 0; i < m_particleSystems.Length; i++)
				{
					m_useAutoRandomSeeds[i] = m_particleSystems[i].useAutoRandomSeed;
				}
			}
		}

		public ParticleSystemWithData(ParticleSystemWithData particleSystemWithData)
			: this(particleSystemWithData.m_baseParticleSystem)
		{
		}

		public bool ParticleSystemEquals(ParticleSystemWithData other)
		{
			return m_baseParticleSystem == other.m_baseParticleSystem;
		}

		public void SetRandomSeedToRandomInt()
		{
			if (!IsAlive && m_particleSystems != null && m_particleSystems.Length != 0)
			{
				ParticleSystem[] particleSystems = m_particleSystems;
				for (int i = 0; i < particleSystems.Length; i++)
				{
					particleSystems[i].randomSeed = (uint)UnityEngine.Random.Range(1, 9999);
				}
			}
		}

		public void DisableUseAutoRandomSeed()
		{
			if (m_particleSystems != null && m_particleSystems.Length != 0)
			{
				for (int i = 0; i < m_particleSystems.Length; i++)
				{
					m_particleSystems[i].Stop(withChildren: false, ParticleSystemStopBehavior.StopEmittingAndClear);
					m_particleSystems[i].useAutoRandomSeed = false;
				}
			}
		}

		public void ResetUseAutoRandomSeed()
		{
			if (m_particleSystems == null || m_particleSystems.Length == 0)
			{
				return;
			}
			for (int i = 0; i < m_particleSystems.Length; i++)
			{
				if (!(m_particleSystems[i] == null))
				{
					m_particleSystems[i].Stop(withChildren: false, ParticleSystemStopBehavior.StopEmittingAndClear);
					m_particleSystems[i].useAutoRandomSeed = m_useAutoRandomSeeds[i];
				}
			}
		}
	}

	private readonly List<ParticleSystemWithData> m_particleSystemsBelongingToThisTrack = new List<ParticleSystemWithData>();

	private readonly List<ParticleSystemWithData> m_particleSystemsBeingControlled = new List<ParticleSystemWithData>();

	protected override object[] GetHelperInitializationData(PlayableInfo playableInfo)
	{
		return new object[0];
	}

	protected override void InitializeFrame(Playable playable, FrameData info, object playerData)
	{
	}

	protected override void OnEnter(Playable playable, FrameData info)
	{
	}

	protected override void OnExit(Playable playable, FrameData info)
	{
	}

	protected override void UpdateFrame(Playable playable, FrameData info, object playerData, float normalizedTime)
	{
		base.Helper.UpdateEffect((float)playable.GetTime(), !playable.GetGraph().IsPlaying());
	}

	public override void OnBehaviourPlay(Playable playable, FrameData info)
	{
		base.OnBehaviourPlay(playable, info);
		for (int i = 0; i < m_particleSystemsBelongingToThisTrack.Count; i++)
		{
			ParticleSystemWithData particleSystem = m_particleSystemsBelongingToThisTrack[i];
			if (IndexOfParticleSystem(particleSystem) == -1)
			{
				AddParticleSystemToList(particleSystem);
			}
			SwitchParticleSystemFromDefaultToManualSeed(particleSystem);
			RandomizeParticleSystemSeed(particleSystem);
		}
	}

	public override void OnBehaviourPause(Playable playable, FrameData info)
	{
		base.OnBehaviourPause(playable, info);
		for (int i = 0; i < m_particleSystemsBelongingToThisTrack.Count; i++)
		{
			ParticleSystemWithData particleSystem = m_particleSystemsBelongingToThisTrack[i];
			if (IndexOfParticleSystem(particleSystem) == -1)
			{
				AddParticleSystemToList(particleSystem);
				SwitchParticleSystemFromDefaultToManualSeed(particleSystem);
				RandomizeParticleSystemSeed(particleSystem);
			}
		}
	}

	public override void ProcessFrame(Playable playable, FrameData info, object playerData)
	{
		base.ProcessFrame(playable, info, playerData);
		if (info.effectivePlayState != 0)
		{
			return;
		}
		for (int i = 0; i < m_particleSystemsBelongingToThisTrack.Count; i++)
		{
			ParticleSystemWithData particleSystem = m_particleSystemsBelongingToThisTrack[i];
			if (IndexOfParticleSystem(particleSystem) == -1)
			{
				AddParticleSystemToList(particleSystem);
				SwitchParticleSystemFromDefaultToManualSeed(particleSystem);
				RandomizeParticleSystemSeed(particleSystem);
			}
		}
	}

	public override void OnPlayableDestroy(Playable playable)
	{
		base.OnPlayableDestroy(playable);
		for (int i = 0; i < m_particleSystemsBelongingToThisTrack.Count; i++)
		{
			ParticleSystemWithData particleSystem = m_particleSystemsBelongingToThisTrack[i];
			SwitchParticleSystemFromManualToDefaultSeed(particleSystem);
			RemoveParticleSystemFromList(particleSystem);
		}
	}

	public void SetParticleSystemsBelongingToThisTrack(IList<ParticleSystem> particleSystems)
	{
		m_particleSystemsBelongingToThisTrack.Clear();
		if (particleSystems != null)
		{
			for (int i = 0; i < particleSystems.Count; i++)
			{
				ParticleSystem particleSystem = particleSystems[i];
				m_particleSystemsBelongingToThisTrack.Add(new ParticleSystemWithData(particleSystem));
			}
		}
	}

	private int IndexOfParticleSystem(ParticleSystemWithData ps)
	{
		if (ps == null)
		{
			return -1;
		}
		for (int i = 0; i < m_particleSystemsBeingControlled.Count; i++)
		{
			if (ps.ParticleSystemEquals(m_particleSystemsBeingControlled[i]))
			{
				return i;
			}
		}
		return -1;
	}

	private void AddParticleSystemToList(ParticleSystemWithData psToAdd)
	{
		if (psToAdd != null)
		{
			m_particleSystemsBeingControlled.Add(new ParticleSystemWithData(psToAdd));
		}
	}

	private void RemoveParticleSystemFromList(ParticleSystemWithData psToRemove)
	{
		if (psToRemove != null)
		{
			int i = IndexOfParticleSystem(psToRemove);
			if (i != -1)
			{
				m_particleSystemsBeingControlled.RemoveAt(i);
			}
		}
	}

	private void SwitchParticleSystemFromDefaultToManualSeed(ParticleSystemWithData ps)
	{
		ps?.DisableUseAutoRandomSeed();
	}

	private void SwitchParticleSystemFromManualToDefaultSeed(ParticleSystemWithData ps)
	{
		ps?.ResetUseAutoRandomSeed();
	}

	private void RandomizeParticleSystemSeed(ParticleSystemWithData target)
	{
		target?.SetRandomSeedToRandomInt();
	}
}
