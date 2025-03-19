using UnityEngine;

public class SinNoiseFunction : ScriptableObject
{
	public Vector4 Frequency;

	public Vector4 RelativeAmplitude;

	public Vector4 OffsetRate;

	public Vector4 GetAmplitude(float maxAmplitude)
	{
		Vector4 amplitude = RelativeAmplitude;
		float trace = Mathf.Abs(RelativeAmplitude.x);
		trace += Mathf.Abs(RelativeAmplitude.y);
		trace += Mathf.Abs(RelativeAmplitude.z);
		trace += Mathf.Abs(RelativeAmplitude.w);
		if (trace > Mathf.Epsilon)
		{
			amplitude /= trace;
		}
		return amplitude * maxAmplitude;
	}
}
