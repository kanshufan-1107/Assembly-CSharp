using System;
using UnityEngine;

namespace Hearthstone.Timeline;

[Serializable]
public abstract class AnimatedVector
{
	public enum Type
	{
		Vector,
		Curve,
		RandomBetweenTwoVectors,
		RandomBetweenTwoCurves
	}

	public Type type;

	public AnimationCurve[] curve = new AnimationCurve[0];

	public AnimationCurve[] curveB = new AnimationCurve[0];

	public abstract object Get(float normalizedTime, float randomValue);
}
