using UnityEngine;

public class RotateAnim : MonoBehaviour
{
	private Quaternion targetRotation;

	private bool gogogo;

	private float timeValue;

	private float timePassed;

	private float startingAngle;

	private void Update()
	{
		if (gogogo)
		{
			timePassed += Time.deltaTime;
			float t = timePassed;
			float b = startingAngle;
			float num = b - Quaternion.Angle(base.transform.rotation, targetRotation);
			float d = timeValue;
			float output = num * (0f - Mathf.Pow(2f, -10f * t / d) + 1f) + b;
			base.transform.rotation = Quaternion.RotateTowards(base.transform.rotation, targetRotation, output * Time.deltaTime);
			if (Quaternion.Angle(base.transform.rotation, targetRotation) <= Mathf.Epsilon)
			{
				gogogo = false;
				Object.Destroy(this);
			}
		}
	}

	public void SetTargetRotation(Vector3 target, float timeValueInput)
	{
		targetRotation = Quaternion.Euler(target);
		gogogo = true;
		timeValue = timeValueInput;
		startingAngle = Quaternion.Angle(base.transform.rotation, targetRotation);
	}
}
