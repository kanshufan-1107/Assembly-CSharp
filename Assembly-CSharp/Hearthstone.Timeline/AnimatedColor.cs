using System;
using UnityEngine;

namespace Hearthstone.Timeline;

[Serializable]
public sealed class AnimatedColor
{
	public enum Type
	{
		Color,
		Gradient,
		RandomBetweenTwoColors,
		RandomBetweenTwoGradients
	}

	public Type type;

	public Color value;

	public Color valueB;

	public WrappedGradient gradient = new WrappedGradient();

	public WrappedGradient gradientB = new WrappedGradient();

	public object Get(float normalizedTime, float randomValue)
	{
		return type switch
		{
			Type.Color => value, 
			Type.Gradient => gradient.Evaluate(normalizedTime), 
			Type.RandomBetweenTwoColors => Color.Lerp(value, valueB, randomValue), 
			_ => Color.Lerp(gradient.Evaluate(normalizedTime), gradientB.Evaluate(normalizedTime), randomValue), 
		};
	}
}
