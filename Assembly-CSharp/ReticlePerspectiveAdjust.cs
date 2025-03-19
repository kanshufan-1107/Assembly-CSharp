using UnityEngine;

public class ReticlePerspectiveAdjust : MonoBehaviour
{
	public float m_HorizontalAdjustment = 20f;

	public float m_VertialAdjustment = 20f;

	private void Update()
	{
		Camera camera = Camera.main;
		if (!(camera == null))
		{
			Vector3 vector = camera.WorldToScreenPoint(base.transform.position);
			float widthDelta = vector.x / (float)camera.pixelWidth - 0.5f;
			float heightDelta = 0f - (vector.y / (float)camera.pixelHeight - 0.5f);
			base.transform.rotation = Quaternion.identity;
			base.transform.Rotate(new Vector3(m_VertialAdjustment * heightDelta, 0f, m_HorizontalAdjustment * widthDelta), Space.World);
		}
	}
}
