using UnityEngine;

public class LightPhase : MonoBehaviour
{
	public float duration = 1f;

	public float minPower = 3f;

	public float maxPower = 8f;

	public float speed = 0.01f;

	private float targetIntensity;

	private float lastTargetTimestamp;

	private float timeToWaitForNewTarget = 1f;

	public void Update()
	{
		float currTime = Time.time;
		if (currTime - lastTargetTimestamp > timeToWaitForNewTarget)
		{
			targetIntensity = Random.Range(minPower, maxPower);
			lastTargetTimestamp = currTime;
		}
		Light light = GetComponent<Light>();
		float num = targetIntensity - light.intensity;
		float normalizedThisToTarget = num / Mathf.Abs(num);
		if (light.intensity != targetIntensity)
		{
			light.intensity += normalizedThisToTarget * speed;
		}
	}
}
