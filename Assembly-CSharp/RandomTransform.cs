using UnityEngine;

public class RandomTransform : MonoBehaviour
{
	public bool m_applyOnStart;

	public Vector3 positionMin;

	public Vector3 positionMax;

	public Vector3 rotationMin;

	public Vector3 rotationMax;

	public Vector3 scaleMin = Vector3.one;

	public Vector3 scaleMax = Vector3.one;

	public void Start()
	{
		if (m_applyOnStart)
		{
			Apply();
		}
	}

	public void Apply()
	{
		Vector3 randomPosition = new Vector3(Random.Range(positionMin.x, positionMax.x), Random.Range(positionMin.y, positionMax.y), Random.Range(positionMin.z, positionMax.z));
		Vector3 position = base.transform.localPosition + randomPosition;
		base.transform.localPosition = position;
		Vector3 randomRotation = new Vector3(Random.Range(rotationMin.x, rotationMax.x), Random.Range(rotationMin.y, rotationMax.y), Random.Range(rotationMin.z, rotationMax.z));
		Vector3 rotation = base.transform.localEulerAngles + randomRotation;
		base.transform.localEulerAngles = rotation;
		Vector3 randomScale = new Vector3(Random.Range(scaleMin.x, scaleMax.x), Random.Range(scaleMin.y, scaleMax.y), Random.Range(scaleMin.z, scaleMax.z));
		Vector3 scale = randomScale;
		randomScale.Scale(base.transform.localScale);
		base.transform.localScale = scale;
	}
}
