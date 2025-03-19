using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class TentacleAnimation : MonoBehaviour
{
	private const int RANDOM_INIT_COUNT = 5;

	public float m_MaxAngle = 45f;

	public float m_AnimSpeed = 0.5f;

	public float m_Secondary = 10f;

	public float m_Smooth = 3f;

	[Range(0f, 1f)]
	public float m_X_Intensity = 1f;

	[Range(0f, 1f)]
	public float m_Y_Intensity = 1f;

	[Range(0f, 1f)]
	public float m_Z_Intensity = 1f;

	public AnimationCurve m_IntensityCurve;

	public List<Transform> m_Bones;

	public List<Transform> m_ControlBones;

	private float[] m_intensityValues;

	private float[] m_angleX;

	private float[] m_angleY;

	private float[] m_angleZ;

	private float[] m_velocityX;

	private float[] m_velocityY;

	private float[] m_velocityZ;

	private float[] m_lastX;

	private float[] m_lastY;

	private float[] m_lastZ;

	private Quaternion[] m_orgRotation;

	private float m_jointStep;

	private float m_secondaryAnim = 0.05f;

	private float m_smoothing = 0.3f;

	private float m_seedX;

	private float m_seedY;

	private float m_seedZ;

	private float m_timeSeed;

	private int[] m_randomNumbers;

	private int m_randomCount;

	private void Start()
	{
		Init();
	}

	private void Update()
	{
		if (m_Bones == null)
		{
			return;
		}
		CalculateBoneAngles();
		if (m_ControlBones.Count > 0)
		{
			for (int b = 0; b < m_Bones.Count; b++)
			{
				m_Bones[b].localRotation = m_ControlBones[b].localRotation;
				m_Bones[b].localPosition = m_ControlBones[b].localPosition;
				m_Bones[b].localScale = m_ControlBones[b].localScale;
				m_Bones[b].Rotate(m_angleX[b], m_angleY[b], m_angleZ[b]);
			}
		}
		else
		{
			for (int i = 0; i < m_Bones.Count; i++)
			{
				m_Bones[i].rotation = m_orgRotation[i];
				m_Bones[i].Rotate(m_angleX[i], m_angleY[i], m_angleZ[i]);
			}
		}
		m_secondaryAnim = m_Secondary * 0.01f;
		m_smoothing = m_Smooth * 0.1f;
	}

	private void Init()
	{
		if (m_Bones != null)
		{
			if (m_IntensityCurve == null || m_IntensityCurve.length < 1)
			{
				m_IntensityCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
			}
			m_randomNumbers = new int[513];
			for (int i = 0; i < 5; i++)
			{
				m_randomNumbers[i] = Random.Range(0, 255);
			}
			m_randomCount = 4;
			m_secondaryAnim = m_Secondary * 0.01f;
			m_smoothing = 0f;
			m_jointStep = 1f / (float)m_Bones.Count;
			m_timeSeed = Random.Range(1, 100);
			m_seedX = Random.Range(1, 10);
			m_seedY = Random.Range(1, 10);
			m_seedZ = Random.Range(1, 10);
			m_intensityValues = new float[m_Bones.Count];
			m_angleX = new float[m_Bones.Count];
			m_angleY = new float[m_Bones.Count];
			m_angleZ = new float[m_Bones.Count];
			m_velocityX = new float[m_Bones.Count];
			m_velocityY = new float[m_Bones.Count];
			m_velocityZ = new float[m_Bones.Count];
			m_lastX = new float[m_Bones.Count];
			m_lastY = new float[m_Bones.Count];
			m_lastZ = new float[m_Bones.Count];
			InitBones();
		}
	}

	private void InitBones()
	{
		if (m_ControlBones.Count < m_Bones.Count)
		{
			m_orgRotation = new Quaternion[m_Bones.Count];
			for (int b = 0; b < m_Bones.Count; b++)
			{
				m_orgRotation[b] = m_Bones[b].rotation;
			}
		}
		else
		{
			for (int i = 0; i < m_Bones.Count; i++)
			{
				m_Bones[i].rotation = m_ControlBones[i].rotation;
			}
		}
		for (int j = 0; j < m_Bones.Count; j++)
		{
			m_lastX[j] = m_Bones[j].eulerAngles.x;
			m_lastY[j] = m_Bones[j].eulerAngles.y;
			m_lastZ[j] = m_Bones[j].eulerAngles.z;
			m_velocityX[j] = 0f;
			m_velocityY[j] = 0f;
			m_velocityZ[j] = 0f;
			m_intensityValues[j] = m_IntensityCurve.Evaluate((float)j * m_jointStep);
		}
	}

	private void CalculateBoneAngles()
	{
		for (int b = 0; b < m_Bones.Count; b++)
		{
			m_angleX[b] = CalculateAngle(b, m_lastX, m_velocityX, m_seedX) * m_X_Intensity;
			m_angleY[b] = CalculateAngle(b, m_lastY, m_velocityY, m_seedY) * m_Y_Intensity;
			m_angleZ[b] = CalculateAngle(b, m_lastZ, m_velocityZ, m_seedZ) * m_Z_Intensity;
		}
	}

	private float CalculateAngle(int index, float[] last, float[] velocity, float offset)
	{
		float n = Time.timeSinceLevelLoad * m_AnimSpeed + m_timeSeed - (float)index * m_secondaryAnim;
		float angle = Simplex1D(n + offset) * m_intensityValues[index] * m_MaxAngle;
		float v = velocity[index];
		angle = Mathf.SmoothDamp(last[index], angle, ref v, m_smoothing);
		velocity[index] = v;
		last[index] = angle;
		return angle;
	}

	private float Simplex1D(float x)
	{
		int xf = (int)Mathf.Floor(x);
		int xfp1 = xf + 1;
		float xmxf = x - (float)xf;
		float xmxfm1 = xmxf - 1f;
		float num = Mathf.Pow(1f - xmxf * xmxf, 4f) * Interpolate(GetRandomNumber(xf & 0xFF), xmxf);
		float n1 = Mathf.Pow(1f - xmxfm1 * xmxfm1, 4f) * Interpolate(GetRandomNumber(xfp1 & 0xFF), xmxfm1);
		return (num + n1) * 0.395f;
	}

	private float Interpolate(int h, float x)
	{
		h &= 0xF;
		float g = 1f + (float)(h & 7);
		if ((h & 8) != 0)
		{
			return (0f - g) * x;
		}
		return g * x;
	}

	private int GetRandomNumber(int index)
	{
		if (index > m_randomCount)
		{
			for (int i = m_randomCount + 1; i <= index + 1; i++)
			{
				m_randomNumbers[i] = Random.Range(0, 255);
			}
			m_randomCount = index + 1;
		}
		return m_randomNumbers[index];
	}
}
