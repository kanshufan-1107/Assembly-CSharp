using System;
using UnityEngine;

[ExecuteInEditMode]
public class SocketIn_05z_ArrowController : MonoBehaviour
{
	private struct Instance
	{
		public SpriteRenderer Renderer;

		public Quaternion Rotation;

		public float DistanceOffset;
	}

	[Header("Control Parameters")]
	public float ElevationAngle = 50f;

	public float Velocity = 10f;

	public Quaternion SpriteRotation;

	[Header("Arrows")]
	public SpriteRenderer[] Arrows;

	[Header("Timeline Properties")]
	public float Time;

	public float AlphaOverride;

	private Instance[] m_instances;

	private const float c_alphaDistance = 0.1f;

	private const float c_minDistance = 0.4f;

	private const float c_angleVariation = 10f;

	private const float c_distanceVariation = 2f;

	private void OnEnable()
	{
		int numArrows = ((Arrows != null) ? Arrows.Length : 0);
		if (numArrows > 0)
		{
			m_instances = new Instance[numArrows];
			float baseAngle = UnityEngine.Random.Range(0f, 360f);
			for (int idx = 0; idx < numArrows; idx++)
			{
				float radialAngle = baseAngle + 360f * (float)idx / (float)numArrows;
				float elevationAngle = ElevationAngle;
				radialAngle += UnityEngine.Random.Range(-10f, 10f);
				elevationAngle += UnityEngine.Random.Range(-10f, 10f);
				Quaternion rotation = SpriteRotation;
				rotation = Quaternion.Euler(0f, 0f, elevationAngle) * rotation;
				rotation = Quaternion.Euler(0f, radialAngle, 0f) * rotation;
				float distanceOffset = UnityEngine.Random.Range(-2f, 0f);
				m_instances[idx] = new Instance
				{
					Renderer = Arrows[idx],
					Rotation = rotation,
					DistanceOffset = distanceOffset
				};
			}
		}
		else
		{
			m_instances = Array.Empty<Instance>();
		}
	}

	private void UpdateAnimation(Instance instance)
	{
		float distance = Time * Velocity + instance.DistanceOffset;
		Quaternion rotation = instance.Rotation;
		Vector3 position = rotation * new Vector3(distance + 0.4f, 0f, 0f);
		if (instance.Renderer != null)
		{
			float alpha = Mathf.Clamp01(distance / 0.1f);
			instance.Renderer.color = new Color(1f, 1f, 1f, Mathf.Min(alpha, AlphaOverride));
		}
		Transform obj = instance.Renderer.transform;
		obj.localPosition = position;
		obj.localRotation = rotation;
	}

	private void LateUpdate()
	{
		if (m_instances != null)
		{
			Instance[] instances = m_instances;
			foreach (Instance instance in instances)
			{
				UpdateAnimation(instance);
			}
		}
	}
}
