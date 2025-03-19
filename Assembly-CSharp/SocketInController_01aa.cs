using System;
using UnityEngine;

[ExecuteAlways]
public class SocketInController_01aa : MonoBehaviour
{
	[Header("Component 1")]
	public GameObject LeftSprite;

	public GameObject RightSprite;

	public float Pivot;

	public float Angle;

	[Header("Component 2")]
	public GameObject[] OrbitalSprites;

	public float OrbitRadius;

	public float OrbitObjectScale;

	public float OrbitAngleOffset;

	private void Update()
	{
		float angleRadians = (float)Math.PI / 180f * Angle;
		if (LeftSprite != null)
		{
			LeftSprite.transform.localPosition = new Vector3(Mathf.Sin(angleRadians), Mathf.Cos(angleRadians) - 1f, 0f) * Pivot;
			LeftSprite.transform.localRotation = Quaternion.Euler(0f, 0f, 0f - Angle);
		}
		if (RightSprite != null)
		{
			RightSprite.transform.localPosition = new Vector3(Mathf.Sin(0f - angleRadians), Mathf.Cos(0f - angleRadians) - 1f, 0f) * Pivot;
			RightSprite.transform.localRotation = Quaternion.Euler(0f, 0f, Angle);
		}
		int numObjectsInOrbit = ((OrbitalSprites != null) ? OrbitalSprites.Length : 0);
		for (int i = 0; i < numObjectsInOrbit; i++)
		{
			Vector3 scale = Vector3.one * OrbitObjectScale;
			GameObject orbitObject = OrbitalSprites[i];
			if ((bool)orbitObject)
			{
				float angle = (float)Math.PI / 180f * (OrbitAngleOffset + (float)i * 360f / (float)numObjectsInOrbit);
				orbitObject.transform.localPosition = new Vector3(Mathf.Sin(angle), Mathf.Cos(angle), 0f) * OrbitRadius;
				orbitObject.transform.localScale = scale;
			}
		}
	}
}
