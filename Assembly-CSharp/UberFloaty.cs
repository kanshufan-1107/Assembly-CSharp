using UnityEngine;

public class UberFloaty : MonoBehaviour
{
	public float speed = 1f;

	public float positionBlend = 1f;

	public float frequencyMin = 1f;

	public float frequencyMax = 3f;

	public bool localSpace = true;

	public Vector3 magnitude = new Vector3(0.001f, 0.001f, 0.001f);

	public float rotationBlend = 1f;

	public float frequencyMinRot = 1f;

	public float frequencyMaxRot = 3f;

	public Vector3 magnitudeRot = new Vector3(0f, 0f, 0f);

	private Vector3 m_interval;

	private Vector3 m_offset;

	private Vector3 m_rotationInterval;

	private Vector3 m_startingPosition;

	private Vector3 m_startingRotation;

	private void Start()
	{
		Init();
	}

	private void OnEnable()
	{
		InitTransforms();
	}

	private void Update()
	{
		float time = Time.time * speed;
		Vector3 floaty = default(Vector3);
		floaty.x = Mathf.Sin(time * m_interval.x + m_offset.x) * magnitude.x * m_interval.x;
		floaty.y = Mathf.Sin(time * m_interval.y + m_offset.y) * magnitude.y * m_interval.y;
		floaty.z = Mathf.Sin(time * m_interval.z + m_offset.z) * magnitude.z * m_interval.z;
		Vector3 newPosition = Vector3.Lerp(m_startingPosition, m_startingPosition + floaty, positionBlend);
		if (localSpace)
		{
			base.transform.localPosition = newPosition;
		}
		else
		{
			base.transform.position = newPosition;
		}
		Vector3 rotFloaty = default(Vector3);
		rotFloaty.x = Mathf.Sin(time * m_rotationInterval.x + m_offset.x) * magnitudeRot.x * m_rotationInterval.x;
		rotFloaty.y = Mathf.Sin(time * m_rotationInterval.y + m_offset.y) * magnitudeRot.y * m_rotationInterval.y;
		rotFloaty.z = Mathf.Sin(time * m_rotationInterval.z + m_offset.z) * magnitudeRot.z * m_rotationInterval.z;
		base.transform.eulerAngles = Vector3.Lerp(m_startingRotation, m_startingRotation + rotFloaty, rotationBlend);
	}

	private void InitTransforms()
	{
		if (localSpace)
		{
			m_startingPosition = base.transform.localPosition;
		}
		else
		{
			m_startingPosition = base.transform.position;
		}
		m_startingRotation = base.transform.eulerAngles;
	}

	private void Init()
	{
		InitTransforms();
		m_interval.x = Random.Range(frequencyMin, frequencyMax);
		m_interval.y = Random.Range(frequencyMin, frequencyMax);
		m_interval.z = Random.Range(frequencyMin, frequencyMax);
		m_offset.x = 0.5f * Random.Range(0f - m_interval.x, m_interval.x);
		m_offset.y = 0.5f * Random.Range(0f - m_interval.y, m_interval.y);
		m_offset.z = 0.5f * Random.Range(0f - m_interval.z, m_interval.z);
		m_rotationInterval.x = Random.Range(frequencyMinRot, frequencyMaxRot);
		m_rotationInterval.y = Random.Range(frequencyMinRot, frequencyMaxRot);
		m_rotationInterval.z = Random.Range(frequencyMinRot, frequencyMaxRot);
	}
}
