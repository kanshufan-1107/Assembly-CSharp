using UnityEngine;

public class floatyObj2 : MonoBehaviour
{
	public float frequencyMin = 0.0001f;

	public float frequencyMax = 0.001f;

	public float magnitude = 0.0001f;

	public float frequencyMinRot = 0.0001f;

	public float frequencyMaxRot = 0.001f;

	public float magnitudeRot;

	private float m_interval;

	private float m_rotationInterval;

	private void Start()
	{
		m_interval = Random.Range(frequencyMin, frequencyMax);
		m_rotationInterval = Random.Range(frequencyMinRot, frequencyMaxRot);
	}

	private void Update()
	{
		float floaty = Mathf.Sin(Time.time * m_interval) * magnitude;
		base.transform.position += new Vector3(floaty, floaty, floaty);
		float rotFloaty = Mathf.Sin(Time.time * m_rotationInterval) * magnitudeRot;
		base.transform.eulerAngles += new Vector3(rotFloaty, rotFloaty, rotFloaty);
	}
}
