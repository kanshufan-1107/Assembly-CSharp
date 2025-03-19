using UnityEngine;
using UnityEngine.Playables;

namespace Hearthstone.Timeline;

public class ControlRandomizedParticleControlPlayable : PlayableBehaviour
{
	private float m_LastTime = -1f;

	private float m_SystemTime;

	public ParticleSystem particleSystem { get; private set; }

	public static ScriptPlayable<ControlRandomizedParticleControlPlayable> Create(PlayableGraph graph, ParticleSystem component)
	{
		if (component == null)
		{
			return ScriptPlayable<ControlRandomizedParticleControlPlayable>.Null;
		}
		ScriptPlayable<ControlRandomizedParticleControlPlayable> handle = ScriptPlayable<ControlRandomizedParticleControlPlayable>.Create(graph);
		handle.GetBehaviour().Initialize(component);
		return handle;
	}

	public void Initialize(ParticleSystem ps)
	{
		particleSystem = ps;
		m_SystemTime = 0f;
		SetRandomSeed();
	}

	private void SetRandomSeed()
	{
		particleSystem.Stop(withChildren: true, ParticleSystemStopBehavior.StopEmittingAndClear);
	}

	public override void PrepareFrame(Playable playable, FrameData data)
	{
		if (particleSystem == null || !particleSystem.gameObject.activeInHierarchy)
		{
			return;
		}
		float localTime = (float)playable.GetTime();
		if (!Mathf.Approximately(m_LastTime, -1f) && Mathf.Approximately(m_LastTime, localTime))
		{
			return;
		}
		float epsilon = Time.fixedDeltaTime * 0.5f;
		float simTime = localTime;
		float expectedDelta = simTime - m_LastTime;
		float startDelay = particleSystem.main.startDelay.Evaluate(particleSystem.randomSeed);
		float particleSystemDurationLoop0 = particleSystem.main.duration + startDelay;
		float expectedSystemTime = ((simTime > particleSystemDurationLoop0) ? m_SystemTime : (m_SystemTime - startDelay));
		if (simTime < m_LastTime || simTime < epsilon || Mathf.Approximately(m_LastTime, -1f) || expectedDelta > particleSystem.main.duration || !(Mathf.Abs(expectedSystemTime - particleSystem.time) < Time.maximumParticleDeltaTime))
		{
			particleSystem.Simulate(0f, withChildren: true, restart: true);
			particleSystem.Simulate(simTime, withChildren: true, restart: false);
			m_SystemTime = simTime;
		}
		else
		{
			float particleSystemDuration = ((simTime > particleSystemDurationLoop0) ? particleSystem.main.duration : particleSystemDurationLoop0);
			float fracTime = simTime % particleSystemDuration;
			float deltaTime = fracTime - m_SystemTime;
			if (deltaTime < 0f - epsilon)
			{
				deltaTime = fracTime + particleSystemDurationLoop0 - m_SystemTime;
			}
			particleSystem.Simulate(deltaTime, withChildren: true, restart: false);
			m_SystemTime += deltaTime;
		}
		m_LastTime = localTime;
	}

	public override void OnBehaviourPlay(Playable playable, FrameData info)
	{
		m_LastTime = -1f;
	}

	public override void OnBehaviourPause(Playable playable, FrameData info)
	{
		m_LastTime = -1f;
	}
}
