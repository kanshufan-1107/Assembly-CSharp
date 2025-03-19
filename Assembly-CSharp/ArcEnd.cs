using UnityEngine;

public class ArcEnd : MonoBehaviour
{
	private Vector3 s;

	public Light l;

	private void Start()
	{
		s = base.transform.localScale;
	}

	private void FixedUpdate()
	{
		Vector3 relativePos = Camera.main.transform.position - base.transform.position;
		Quaternion rot = Quaternion.LookRotation(Vector3.up, relativePos);
		base.transform.rotation = rot;
		base.transform.Rotate(Vector3.up, Random.value * 360f);
		if (Random.value > 0.8f)
		{
			base.transform.localScale = s * 1.5f;
			if (l != null)
			{
				l.range = 100f;
				l.intensity = 1.5f;
			}
		}
		else
		{
			base.transform.localScale = s;
			if (l != null)
			{
				l.range = 50f;
				l.intensity = 1f;
			}
		}
	}
}
