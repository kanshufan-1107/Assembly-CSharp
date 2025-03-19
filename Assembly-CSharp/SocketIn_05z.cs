using System;
using UnityEngine;

[ExecuteInEditMode]
public class SocketIn_05z : MonoBehaviour
{
	[Header("Control Points")]
	public Vector3 StartPosition;

	public Vector3 EndPosition;

	public float Radius1;

	public float Radius2;

	public bool InvertDirection;

	public AnimationCurve Elevation;

	public AnimationCurve Pitch;

	public AnimationCurve Roll;

	[Header("Timeline Hooks")]
	public float Time;

	public float ElevationScale = 1f;

	private void UpdateAnimation()
	{
		float time = Mathf.Clamp01(Time);
		float radius = Mathf.LerpUnclamped(Radius1, Radius2, time);
		Vector3 a = StartPosition + Vector3.right * Radius1;
		Vector3 origin2 = EndPosition + Vector3.right * Radius2;
		Vector3 vector = Vector3.LerpUnclamped(a, origin2, time);
		float angle = (float)Math.PI * 2f * time * (InvertDirection ? (-1f) : 1f);
		Vector3 position = vector + new Vector3(0f - Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * radius;
		if (Elevation != null)
		{
			position.y = Mathf.LerpUnclamped(EndPosition.y, StartPosition.y, Elevation.Evaluate(time));
		}
		position.y *= ElevationScale;
		float roll = 0f;
		if (Roll != null)
		{
			roll = Roll.Evaluate(time);
		}
		float pitch = 0f;
		if (Pitch != null)
		{
			pitch = Pitch.Evaluate(time);
		}
		base.transform.localPosition = position;
		base.transform.localRotation = Quaternion.Euler(new Vector3(pitch, 360f * time * (InvertDirection ? (-1f) : 1f), roll));
	}

	private void LateUpdate()
	{
		UpdateAnimation();
	}
}
