using System;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class UberShuriken : MonoBehaviour
{
	private const int VORTEX_NOISE_INVERVAL = 3;

	private const int FOLLOW_CURVE_INVERVAL = 3;

	private const int CURL_NOISE_INVERVAL = 3;

	public bool m_IncludeChildren;

	public UberCurve m_UberCurve;

	public bool m_FollowCurveDirection;

	public bool m_FollowCurvePosition;

	public float m_FollowCurvePositionAttraction = 0.5f;

	public float m_FollowCurvePositionIntensity = 1.7f;

	public AnimationCurve m_FollowCurvePositionOverLifetime = new AnimationCurve(new Keyframe(0f, 1f), new Keyframe(1f, 1f));

	public bool m_CurlNoise;

	public float m_CurlNoisePower = 1f;

	public AnimationCurve m_CurlNoiseOverLifetime = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 1f));

	public float m_CurlNoiseScale = 1f;

	public Vector3 m_CurlNoiseAnimation = Vector3.zero;

	public float m_CurlNoiseGizmoSize = 1f;

	public bool m_Twinkle;

	public float m_TwinkleRate = 1f;

	[Range(-1f, 1f)]
	public float m_TwinkleBias;

	public AnimationCurve m_TwinkleOverLifetime = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 1f));

	private List<ParticleSystem> m_particleSystems = new List<ParticleSystem>();

	private ParticleSystem.Particle[] m_particles;

	private float m_time;

	private int m_followCurveIntervalIndex = 1;

	private int m_curlNoiseIntervalIndex = 2;

	private void Awake()
	{
		if (m_UberCurve == null)
		{
			m_UberCurve = GetComponent<UberCurve>();
		}
		UpdateParticleSystemList();
	}

	private void Update()
	{
		m_time = Time.time;
		UpdateParticles();
	}

	private void OnDrawGizmosSelected()
	{
		Gizmos.matrix = base.transform.localToWorldMatrix;
		Gizmos.color = Color.blue;
		if (m_CurlNoise && m_CurlNoiseGizmoSize > 0f)
		{
			int numberOfCircles = 10;
			float circleRadius = Mathf.Max(Mathf.Abs(m_CurlNoiseScale * 0.25f), 1f) * m_CurlNoiseGizmoSize;
			float colIncrement = 1f / ((float)numberOfCircles * 1.2f);
			float colIntensity = 1f;
			float radius = 0f;
			float previousRadius = 0f;
			for (int circle = 0; circle < numberOfCircles; circle++)
			{
				Gizmos.color = new Color(0f, 0f, 1f - colIntensity, 1f);
				colIntensity -= colIncrement;
				radius = (float)circle * 0.75f;
				Vector4[] points = GizmoCirclePoints(20 * Mathf.Max(Mathf.FloorToInt(Mathf.Abs(m_CurlNoiseScale)), 10), radius * circleRadius);
				Vector4 previousPoint = points[points.Length - 1];
				for (int p = 0; p < points.Length; p++)
				{
					Gizmos.color = new Color(points[p].w * 0.5f, points[p].w, 1f, 1f);
					Gizmos.DrawLine(previousPoint, points[p]);
					previousPoint = points[p];
				}
				Vector4[] radLines = GizmoCircleLines(10, previousRadius * circleRadius, radius * circleRadius);
				for (int l = 0; l < radLines.Length; l += 2)
				{
					Gizmos.color = new Color(radLines[l].w * 0.5f, radLines[l].w, 1f, 1f);
					Gizmos.DrawLine(radLines[l], radLines[l + 1]);
				}
				previousRadius = radius;
			}
		}
		Gizmos.matrix = Matrix4x4.identity;
	}

	private Vector4[] GizmoCirclePoints(int numOfPoints, float radius)
	{
		Vector4[] points = new Vector4[numOfPoints];
		float radian = 0f;
		float radianInc = (float)Math.PI * 2f / (float)numOfPoints;
		for (int i = 0; i < numOfPoints; i++)
		{
			radian += radianInc;
			points[i] = GizmoCurlNoisePoint(new Vector3(Mathf.Cos(radian) * radius, Mathf.Sin(radian) * radius, 0f));
		}
		return points;
	}

	private Vector4[] GizmoCircleLines(int numOfPoints, float previousRadius, float radius)
	{
		int count = numOfPoints * 2;
		Vector4[] points = new Vector4[count];
		float radian = 0f;
		float radianInc = 6.283f / (float)numOfPoints;
		for (int i = 0; i < count; i += 2)
		{
			radian += radianInc;
			points[i] = GizmoCurlNoisePoint(new Vector3(Mathf.Cos(radian) * previousRadius, Mathf.Sin(radian) * previousRadius, 0f));
			points[i + 1] = GizmoCurlNoisePoint(new Vector3(Mathf.Cos(radian) * radius, Mathf.Sin(radian) * radius, 0f));
		}
		return points;
	}

	private Vector4 GizmoCurlNoisePoint(Vector3 point)
	{
		float t = m_time;
		float tx = m_CurlNoiseAnimation.x * t;
		float ty = m_CurlNoiseAnimation.y * t;
		float tz = m_CurlNoiseAnimation.z * t;
		Vector3 pPos = point * m_CurlNoiseScale * 0.1f;
		float x = UberMath.SimplexNoise(5f + pPos.x + tx, pPos.y + ty, pPos.z + tz) * m_CurlNoisePower;
		float y = UberMath.SimplexNoise(6f + pPos.y + tx, pPos.z + ty, pPos.x + tz) * m_CurlNoisePower;
		float z = UberMath.SimplexNoise(7f + pPos.z + tx, pPos.x + ty, pPos.y + tz) * m_CurlNoisePower;
		Vector3 curlPos = new Vector3(point.x + x, point.y + y, point.z + z);
		float intensity = 1f;
		intensity = Mathf.Max(x, Mathf.Max(y, z));
		return new Vector4(curlPos.x, curlPos.y, curlPos.z, intensity);
	}

	private void UpdateParticles()
	{
		m_followCurveIntervalIndex = ((m_followCurveIntervalIndex + 1 > 3) ? (m_followCurveIntervalIndex = 0) : (m_followCurveIntervalIndex + 1));
		m_curlNoiseIntervalIndex = ((m_curlNoiseIntervalIndex + 1 <= 3) ? (m_curlNoiseIntervalIndex + 1) : 0);
		foreach (ParticleSystem particleSystem in m_particleSystems)
		{
			if (!(particleSystem == null))
			{
				int particleCount = particleSystem.particleCount;
				if (particleCount == 0)
				{
					break;
				}
				if (m_particles == null || particleCount > m_particles.Length)
				{
					ResizeParticlesBuffer(particleCount);
				}
				particleSystem.GetParticles(m_particles);
				if (m_FollowCurveDirection || m_FollowCurvePosition)
				{
					FollowCurveOverLife(particleSystem, m_particles, particleCount);
				}
				if (m_CurlNoise)
				{
					ParticleCurlNoise(particleSystem, m_particles, particleCount);
				}
				if (m_Twinkle)
				{
					ParticleTwinkle(particleSystem, m_particles, particleCount);
				}
				particleSystem.SetParticles(m_particles, particleCount);
			}
		}
	}

	private void UpdateParticleSystemList()
	{
		m_particleSystems.Clear();
		if (m_IncludeChildren)
		{
			ParticleSystem[] particleSystems = GetComponentsInChildren<ParticleSystem>();
			if (GetComponent<ParticleSystem>() == null || particleSystems.Length == 0)
			{
				Debug.LogError("Failed to find a ParticleSystem");
			}
			ParticleSystem[] array = particleSystems;
			foreach (ParticleSystem ps in array)
			{
				m_particleSystems.Add(ps);
			}
		}
		else
		{
			ParticleSystem particleSystem = GetComponent<ParticleSystem>();
			if (particleSystem == null)
			{
				Debug.LogError("Failed to find a ParticleSystem");
			}
			m_particleSystems.Add(particleSystem);
		}
	}

	private void ResizeParticlesBuffer(int newCount)
	{
		m_particles = new ParticleSystem.Particle[newCount];
	}

	private void FollowCurveOverLife(ParticleSystem particleSystem, ParticleSystem.Particle[] particles, int particleCount)
	{
		if (m_UberCurve == null)
		{
			CreateCurve();
		}
		for (int idx = m_followCurveIntervalIndex; idx < particleCount; idx += 3)
		{
			float pLife = 1f - particles[idx].remainingLifetime / particles[idx].startLifetime;
			if (m_FollowCurvePosition)
			{
				Vector3 cPos = Vector3.zero;
				cPos = ((particleSystem.main.simulationSpace != ParticleSystemSimulationSpace.World) ? m_UberCurve.CatmullRomEvaluateLocalPosition(pLife) : m_UberCurve.CatmullRomEvaluateWorldPosition(pLife));
				Vector3 cDir = cPos - particles[idx].position;
				cDir = Vector3.Lerp(particles[idx].velocity, cDir, m_FollowCurvePositionAttraction);
				particles[idx].velocity = cDir * m_FollowCurvePositionIntensity;
			}
			if (m_FollowCurveDirection)
			{
				Vector3 cDir2 = m_UberCurve.CatmullRomEvaluateDirection(pLife).normalized * particles[idx].velocity.magnitude;
				particles[idx].velocity = cDir2;
			}
		}
	}

	private void CreateCurve()
	{
		if (!(m_UberCurve != null))
		{
			m_UberCurve = GetComponent<UberCurve>();
			if (!(m_UberCurve != null))
			{
				m_UberCurve = base.gameObject.AddComponent<UberCurve>();
			}
		}
	}

	private void ParticleCurlNoise(ParticleSystem particleSystem, ParticleSystem.Particle[] particles, int particleCount)
	{
		float t = m_time;
		float tx = m_CurlNoiseAnimation.x * t;
		float ty = m_CurlNoiseAnimation.y * t;
		float tz = m_CurlNoiseAnimation.z * t;
		for (int idx = m_curlNoiseIntervalIndex; idx < particleCount; idx += 3)
		{
			float pLife = 1f - particles[idx].remainingLifetime / particles[idx].startLifetime;
			float pPower = m_CurlNoiseOverLifetime.Evaluate(pLife) * m_CurlNoisePower;
			Vector3 pDir = particles[idx].velocity;
			Vector3 pPos = particles[idx].position * m_CurlNoiseScale * 0.1f;
			pDir.x += UberMath.SimplexNoise(5f + pPos.x + tx, pPos.y + ty, pPos.z + tz) * pPower;
			pDir.y += UberMath.SimplexNoise(6f + pPos.y + tx, pPos.z + ty, pPos.x + tz) * pPower;
			pDir.z += UberMath.SimplexNoise(7f + pPos.z + tx, pPos.x + ty, pPos.y + tz) * pPower;
			pDir = pDir.normalized * particles[idx].velocity.magnitude;
			particles[idx].velocity = pDir;
		}
	}

	private void ParticleTwinkle(ParticleSystem particleSystem, ParticleSystem.Particle[] particles, int particleCount)
	{
		for (int idx = 0; idx < particleCount; idx++)
		{
			float pLife = particles[idx].remainingLifetime / particles[idx].startLifetime;
			Vector3 pPos = particles[idx].position;
			Color col = particles[idx].startColor;
			col.a = Mathf.Clamp01(UberMath.SimplexNoise((pPos.x + pPos.y + pPos.z - pLife - (float)idx * 3.33f) * m_TwinkleRate, 0.5f) + m_TwinkleBias + pLife * m_TwinkleOverLifetime.Evaluate(pLife));
			particles[idx].startColor = col;
		}
	}
}
