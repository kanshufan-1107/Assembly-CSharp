using UnityEngine;

public class LegendarySkinShadowVolume : MonoBehaviour
{
	[Min(0f)]
	public float Radius;

	private void OnDrawGizmos()
	{
		Gizmos.color = Color.yellow;
		Gizmos.DrawWireSphere(base.transform.position, Radius);
	}

	private void OnDrawGizmosSelected()
	{
		Gizmos.color = new Color32(byte.MaxValue, byte.MaxValue, 0, 128);
		Gizmos.DrawSphere(base.transform.position, Radius);
	}
}
