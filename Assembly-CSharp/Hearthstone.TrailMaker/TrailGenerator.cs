using System;
using System.Collections;
using UnityEngine;

namespace Hearthstone.TrailMaker;

[DisallowMultipleComponent]
[AddComponentMenu("")]
public class TrailGenerator : MonoBehaviour
{
	private enum OriginMode
	{
		OriginAndRotation,
		StretchedBetweenTwoPoints,
		StretchedBetweenTwoObjects
	}

	private enum TrailPhase
	{
		Inactive,
		Active,
		Dissipating
	}

	private enum InterpolationMode
	{
		Never,
		OnlyDuringSlowdowns,
		Always
	}

	public delegate void UpdateHandler(MutableArray<TrailPosition> list, bool isEditor);

	public UpdateHandler OnUpdate;

	public bool isOn;

	[SerializeField]
	private OriginMode m_originMode;

	[SerializeField]
	private Vector3 m_origin;

	[SerializeField]
	private Vector3 m_angle;

	[SerializeField]
	[Min(0f)]
	private float m_extent;

	[SerializeField]
	private Transform m_attachObject;

	[SerializeField]
	private Transform m_attachObjectB;

	[SerializeField]
	private Vector3 m_originB;

	[SerializeField]
	[Min(0f)]
	private float m_lifespan;

	[SerializeField]
	[Min(0f)]
	private int m_maxCount;

	[SerializeField]
	private InterpolationMode m_interpolationMode;

	[Min(0.001f)]
	[SerializeField]
	private float m_maxInterval;

	[Min(1f)]
	[SerializeField]
	private float m_interpolationAmountNormal;

	[SerializeField]
	[Min(1f)]
	private float m_interpolationAmountSlowdown;

	[SerializeField]
	private bool m_viewAligned;

	private bool m_startingUp;

	private bool m_wasOn;

	private TrailPhase m_phase;

	private readonly MutableArray<TrailPosition> m_trail = new MutableArray<TrailPosition>();

	private readonly MutableArray<TrailPosition> m_trailCopy = new MutableArray<TrailPosition>();

	private Camera m_camera;

	private Vector3 Origin
	{
		get
		{
			if (m_attachObject == null)
			{
				Debug.LogWarning(GetType().Name + " is missing a target.");
				m_attachObject = base.transform;
			}
			if (m_originMode == OriginMode.StretchedBetweenTwoPoints || (m_originMode == OriginMode.StretchedBetweenTwoObjects && m_attachObjectB == null))
			{
				return m_attachObject.TransformPoint(Vector3.Lerp(m_origin, m_originB, 0.5f));
			}
			if (m_originMode == OriginMode.StretchedBetweenTwoObjects)
			{
				return Vector3.Lerp(m_attachObject.TransformPoint(m_origin), m_attachObjectB.TransformPoint(m_originB), 0.5f);
			}
			return m_attachObject.TransformPoint(m_origin);
		}
	}

	private Vector3 Min
	{
		get
		{
			if (m_attachObject == null)
			{
				Debug.LogWarning(GetType().Name + " is missing a target.");
				m_attachObject = base.transform;
			}
			Vector3 value;
			if (m_originMode == OriginMode.StretchedBetweenTwoPoints || m_originMode == OriginMode.StretchedBetweenTwoObjects)
			{
				value = m_attachObject.TransformPoint(m_origin);
			}
			else
			{
				float s = m_angle.z * ((float)Math.PI / 180f);
				float t = m_angle.y * ((float)Math.PI / 180f);
				Vector3 p = m_extent * new Vector3
				{
					x = Mathf.Cos(s) * Mathf.Sin(t),
					y = Mathf.Sin(s) * Mathf.Sin(t),
					z = Mathf.Cos(t)
				};
				value = m_attachObject.TransformPoint(m_origin - p);
			}
			if (m_viewAligned && Camera != null)
			{
				Vector3 origin = Origin;
				Plane plane = new Plane((Camera.transform.position - origin).normalized, origin);
				Ray ray = new Ray(Camera.transform.position, (value - Camera.transform.position).normalized);
				plane.Raycast(ray, out var enter);
				value = ray.GetPoint(enter);
			}
			return value;
		}
	}

	private Vector3 Max
	{
		get
		{
			if (m_attachObject == null)
			{
				Debug.LogWarning(GetType().Name + " is missing a target.");
				m_attachObject = base.transform;
			}
			Vector3 value;
			if (m_originMode == OriginMode.StretchedBetweenTwoPoints || (m_originMode == OriginMode.StretchedBetweenTwoObjects && m_attachObjectB == null))
			{
				value = m_attachObject.TransformPoint(m_originB);
			}
			else if (m_originMode == OriginMode.StretchedBetweenTwoObjects)
			{
				value = m_attachObjectB.TransformPoint(m_originB);
			}
			else
			{
				float s = m_angle.z * ((float)Math.PI / 180f);
				float t = m_angle.y * ((float)Math.PI / 180f);
				Vector3 p = m_extent * new Vector3
				{
					x = Mathf.Cos(s) * Mathf.Sin(t),
					y = Mathf.Sin(s) * Mathf.Sin(t),
					z = Mathf.Cos(t)
				};
				value = m_attachObject.TransformPoint(m_origin + p);
			}
			if (m_viewAligned && Camera != null)
			{
				Vector3 origin = Origin;
				Plane plane = new Plane((Camera.transform.position - origin).normalized, origin);
				Ray ray = new Ray(Camera.transform.position, (value - Camera.transform.position).normalized);
				plane.Raycast(ray, out var enter);
				value = ray.GetPoint(enter);
			}
			return value;
		}
	}

	public float Lifespan => m_lifespan;

	private Camera Camera
	{
		get
		{
			if (m_camera == null)
			{
				m_camera = Camera.main;
			}
			return m_camera;
		}
	}

	private void Reset()
	{
		m_originMode = OriginMode.OriginAndRotation;
		m_origin = new Vector3(0f, -1f, 0f);
		m_angle = Vector3.zero;
		m_extent = 1f;
		m_originB = m_origin;
		m_attachObject = base.transform;
		m_attachObjectB = null;
		m_lifespan = 1f;
		m_maxCount = 20;
		m_interpolationMode = InterpolationMode.Never;
		m_maxInterval = 0.005f;
		m_interpolationAmountNormal = 1f;
		m_interpolationAmountSlowdown = 5f;
		m_viewAligned = false;
	}

	private void Awake()
	{
		if (m_attachObject == null)
		{
			m_attachObject = base.transform;
		}
	}

	private void Start()
	{
		if (Application.isPlaying)
		{
			ResetTrail();
			StartCoroutine(DelayedStart());
		}
	}

	private void ResetTrail()
	{
		m_startingUp = true;
		m_wasOn = false;
		m_phase = TrailPhase.Inactive;
		m_trail.Clear();
	}

	private IEnumerator DelayedStart()
	{
		yield return new WaitForEndOfFrame();
		m_startingUp = false;
	}

	private void LateUpdate()
	{
		if (Application.isPlaying && !m_startingUp)
		{
			if (isOn != m_wasOn)
			{
				m_wasOn = isOn;
				Activate(isOn);
			}
			UpdateTrail(isEditor: false);
		}
	}

	public void Activate(bool newIsActive)
	{
		if (newIsActive)
		{
			Activate();
		}
		else
		{
			Deactivate();
		}
	}

	public void Activate()
	{
		if (m_phase != TrailPhase.Active)
		{
			m_phase = TrailPhase.Active;
		}
	}

	public void Deactivate()
	{
		if (m_phase == TrailPhase.Active)
		{
			m_phase = TrailPhase.Dissipating;
		}
	}

	private void UpdateTrail(bool isEditor)
	{
		switch (m_phase)
		{
		case TrailPhase.Active:
		{
			m_trail.RemoveOld(m_lifespan);
			ExtendTrail(out var _, out var slowdown2);
			m_trail.UpdateDistances();
			if (isEditor || m_interpolationMode == InterpolationMode.Never)
			{
				m_trailCopy.Clone(m_trail);
			}
			else if (m_interpolationMode == InterpolationMode.OnlyDuringSlowdowns)
			{
				if (slowdown2)
				{
					TrailSplineInterpolator.Interpolate(m_trail, m_trailCopy, m_interpolationAmountSlowdown);
				}
				else
				{
					m_trailCopy.Clone(m_trail);
				}
			}
			else if (slowdown2)
			{
				TrailSplineInterpolator.Interpolate(m_trail, m_trailCopy, m_interpolationAmountSlowdown);
			}
			else
			{
				TrailSplineInterpolator.Interpolate(m_trail, m_trailCopy, m_interpolationAmountNormal);
			}
			m_trailCopy.Dissolve(m_maxCount);
			OnUpdate?.Invoke(m_trailCopy, isEditor);
			break;
		}
		case TrailPhase.Dissipating:
			m_trail.RemoveOld(m_lifespan);
			if (m_trail.Count > 0)
			{
				ExtendTrail(out var extensionCount, out var slowdown);
				MoveTrail(extensionCount);
				m_trail.UpdateDistances();
				switch (m_interpolationMode)
				{
				case InterpolationMode.Never:
					m_trailCopy.Clone(m_trail);
					break;
				case InterpolationMode.OnlyDuringSlowdowns:
					if (slowdown)
					{
						TrailSplineInterpolator.Interpolate(m_trail, m_trailCopy, m_interpolationAmountSlowdown);
					}
					else
					{
						m_trailCopy.Clone(m_trail);
					}
					break;
				default:
					if (slowdown)
					{
						TrailSplineInterpolator.Interpolate(m_trail, m_trailCopy, m_interpolationAmountSlowdown);
					}
					else
					{
						TrailSplineInterpolator.Interpolate(m_trail, m_trailCopy, m_interpolationAmountNormal);
					}
					break;
				}
				m_trailCopy.Dissolve(m_maxCount);
				OnUpdate?.Invoke(m_trailCopy, isEditor);
			}
			else
			{
				OnUpdate?.Invoke(null, isEditor);
				m_phase = TrailPhase.Inactive;
			}
			break;
		}
	}

	private void ExtendTrail(out int extensionCount, out bool slowdown)
	{
		extensionCount = 1;
		slowdown = false;
		if (m_phase == TrailPhase.Active && m_trail.Count > 0)
		{
			TrailPosition prev = m_trail.GetLast();
			float time = TrailUtility.GetTime();
			float interval = time - prev.time;
			if (interval > m_maxInterval)
			{
				slowdown = true;
				int count = Mathf.FloorToInt(interval / m_maxInterval);
				Vector3 origin = Origin;
				Vector3 min = Min;
				Vector3 max = Max;
				for (int i = 1; i < count; i++)
				{
					extensionCount++;
					float t = (float)i / (float)count;
					m_trail.AddToEnd(new TrailPosition
					{
						origin = Vector3.Lerp(prev.origin, origin, t),
						min = Vector3.Lerp(prev.min, min, t),
						max = Vector3.Lerp(prev.max, max, t),
						time = Mathf.Lerp(prev.time, time, t)
					});
				}
			}
		}
		m_trail.AddToEnd(new TrailPosition
		{
			origin = Origin,
			min = Min,
			max = Max,
			time = TrailUtility.GetTime()
		});
	}

	private void MoveTrail(int count = 1)
	{
		while (count > 0 && m_trail.Count > 1)
		{
			for (int i = m_trail.Count - 2; i >= 0; i--)
			{
				TrailPosition trailPosition = m_trail.Get(i + 1);
				trailPosition.time = m_trail.Get(i).time;
				m_trail.Set(i + 1, trailPosition);
			}
			m_trail.RemoveFromStart();
			count--;
		}
	}
}
