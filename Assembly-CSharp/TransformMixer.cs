using UnityEngine;

[ExecuteInEditMode]
public class TransformMixer : MonoBehaviour
{
	[Range(0f, 1f)]
	public float m_transformMix;

	public bool m_lookAt;

	public bool do_Transform;

	public GameObject m_destination;

	public GameObject m_aim;

	private void LateUpdate()
	{
		if (do_Transform)
		{
			MixTransform();
		}
	}

	private void MixTransform()
	{
		if ((bool)m_destination && m_transformMix < 1f)
		{
			m_aim.transform.position = Vector3.Lerp(base.transform.position, m_destination.transform.position, m_transformMix);
			if (m_lookAt)
			{
				m_aim.transform.LookAt(m_destination.transform, Vector3.up);
			}
		}
	}
}
