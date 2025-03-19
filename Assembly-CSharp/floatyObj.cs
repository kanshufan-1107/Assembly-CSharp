using Hearthstone.UI.Core;
using UnityEngine;

public class floatyObj : MonoBehaviour
{
	public float frequencyMin = 0.0001f;

	public float frequencyMax = 0.001f;

	public float magnitude = 0.0001f;

	private float m_interval;

	[Overridable]
	public bool Enabled
	{
		get
		{
			return base.enabled;
		}
		set
		{
			base.enabled = value;
		}
	}

	private void Start()
	{
		m_interval = Random.Range(frequencyMin, frequencyMax);
	}

	private void FixedUpdate()
	{
		float floaty = Mathf.Sin(Time.time * m_interval) * magnitude;
		Vector3 floaty3 = new Vector3(floaty, floaty, floaty);
		base.transform.position += floaty3;
		base.transform.eulerAngles += floaty3;
	}
}
