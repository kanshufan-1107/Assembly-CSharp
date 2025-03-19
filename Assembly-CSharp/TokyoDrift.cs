using System.Collections;
using UnityEngine;

public class TokyoDrift : MonoBehaviour
{
	public float m_DriftPositionAmount = 1f;

	public bool m_DriftPositionAxisX = true;

	public bool m_DriftPositionAxisY = true;

	public bool m_DriftPositionAxisZ = true;

	public float m_DriftSpeed = 0.1f;

	private Vector3 m_originalPosition;

	private Vector3 m_newPosition;

	private float m_posSeedX;

	private float m_posSeedY;

	private float m_posSeedZ;

	private float m_posOffsetX;

	private float m_posOffsetY;

	private float m_posOffsetZ;

	private float m_blend;

	private bool m_blendOut;

	private void Start()
	{
		m_originalPosition = base.transform.localPosition;
		m_newPosition = default(Vector3);
		m_posSeedX = Random.Range(1, 10);
		m_posSeedY = Random.Range(1, 10);
		m_posSeedZ = Random.Range(1, 10);
		m_posOffsetX = Random.Range(0.6f, 0.99f);
		m_posOffsetY = Random.Range(0.6f, 0.99f);
		m_posOffsetZ = Random.Range(0.6f, 0.99f);
	}

	private void OnDisable()
	{
		if (m_blend == 0f)
		{
			return;
		}
		if (!base.gameObject.activeInHierarchy)
		{
			m_blend = 0f;
			m_blendOut = false;
			base.transform.localPosition = m_originalPosition;
			return;
		}
		if (m_blend > 1f)
		{
			m_blend = 1f;
		}
		StartCoroutine(BlendOut());
	}

	private void Update()
	{
		if (!m_blendOut)
		{
			float t = Time.timeSinceLevelLoad * m_DriftSpeed;
			float x = m_originalPosition.x;
			float y = m_originalPosition.y;
			float z = m_originalPosition.z;
			if (m_DriftPositionAxisX)
			{
				x = Mathf.Sin(t + m_posSeedX + Mathf.Cos(t * m_posOffsetX)) * m_DriftPositionAmount;
			}
			if (m_DriftPositionAxisY)
			{
				y = Mathf.Sin(t + m_posSeedY + Mathf.Cos(t * m_posOffsetY)) * m_DriftPositionAmount;
			}
			if (m_DriftPositionAxisZ)
			{
				z = Mathf.Sin(t + m_posSeedZ + Mathf.Cos(t * m_posOffsetZ)) * m_DriftPositionAmount;
			}
			m_newPosition.x = m_originalPosition.x + x;
			m_newPosition.y = m_originalPosition.y + y;
			m_newPosition.z = m_originalPosition.z + z;
			if (m_blend < 1f)
			{
				base.transform.localPosition = Vector3.Lerp(m_originalPosition, m_newPosition, m_blend);
				m_blend += Time.deltaTime * m_DriftSpeed;
			}
			else
			{
				base.transform.localPosition = m_newPosition;
			}
		}
	}

	private IEnumerator BlendOut()
	{
		m_blendOut = true;
		m_blend = 0f;
		while (m_blend < 1f)
		{
			base.transform.localPosition = Vector3.Lerp(m_newPosition, m_originalPosition, m_blend);
			m_blend += Time.deltaTime * m_DriftSpeed;
			yield return null;
		}
		m_blend = 0f;
		m_blendOut = false;
	}
}
