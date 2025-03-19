using System;
using UnityEngine;

namespace Hearthstone.Timeline;

[Serializable]
public sealed class AnimatedFloat
{
	public enum Type
	{
		Float,
		Curve,
		RandomBetweenTwoFloats,
		RandomBetweenTwoCurves
	}

	public Type type;

	public float value;

	public float valueB;

	public AnimationCurve curve = new AnimationCurve();

	public AnimationCurve curveB = new AnimationCurve();

	public object Get(float normalizedTime, float randomValue)
	{
		return type switch
		{
			Type.Float => value, 
			Type.Curve => curve.Evaluate(normalizedTime), 
			Type.RandomBetweenTwoFloats => Mathf.Lerp(value, valueB, randomValue), 
			_ => Mathf.Lerp(curve.Evaluate(normalizedTime), curveB.Evaluate(normalizedTime), randomValue), 
		};
	}
}
