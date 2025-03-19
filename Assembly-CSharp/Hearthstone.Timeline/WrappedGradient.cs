using System;
using UnityEngine;

namespace Hearthstone.Timeline;

[Serializable]
[ExecuteAlways]
public class WrappedGradient
{
	[Serializable]
	public struct ColorKey
	{
		public Color color;

		public float time;
	}

	[Serializable]
	public struct AlphaKey
	{
		public float alpha;

		public float time;
	}

	public ColorKey[] colorKeys = new ColorKey[0];

	public AlphaKey[] alphaKeys = new AlphaKey[0];

	public WrappedGradient(Gradient gradient)
	{
		FromGradient(gradient);
	}

	public WrappedGradient()
		: this(new Gradient())
	{
	}

	public Color Evaluate(float time)
	{
		return ToGradient().Evaluate(time);
	}

	public WrappedGradient GetClone()
	{
		WrappedGradient clone = new WrappedGradient();
		clone.colorKeys = new ColorKey[colorKeys.Length];
		for (int i = 0; i < colorKeys.Length; i++)
		{
			clone.colorKeys[i] = colorKeys[i];
		}
		clone.alphaKeys = new AlphaKey[alphaKeys.Length];
		for (int j = 0; j < alphaKeys.Length; j++)
		{
			clone.alphaKeys[j] = alphaKeys[j];
		}
		return clone;
	}

	public static WrappedGradient CreateFromGradient(Gradient gradient)
	{
		WrappedGradient wrappedGradient = new WrappedGradient();
		wrappedGradient.FromGradient(gradient);
		return wrappedGradient;
	}

	public void FromGradient(Gradient gradient)
	{
		colorKeys = new ColorKey[gradient.colorKeys.Length];
		for (int i = 0; i < colorKeys.Length; i++)
		{
			colorKeys[i] = new ColorKey
			{
				color = gradient.colorKeys[i].color,
				time = gradient.colorKeys[i].time
			};
		}
		alphaKeys = new AlphaKey[gradient.alphaKeys.Length];
		for (int j = 0; j < alphaKeys.Length; j++)
		{
			alphaKeys[j] = new AlphaKey
			{
				alpha = gradient.alphaKeys[j].alpha,
				time = gradient.alphaKeys[j].time
			};
		}
	}

	public Gradient ToGradient()
	{
		Gradient gradient = new Gradient();
		GradientColorKey[] gradientColorKeys = new GradientColorKey[colorKeys.Length];
		for (int i = 0; i < gradientColorKeys.Length; i++)
		{
			gradientColorKeys[i] = new GradientColorKey
			{
				color = colorKeys[i].color,
				time = colorKeys[i].time
			};
		}
		GradientAlphaKey[] gradientAlphaKeys = new GradientAlphaKey[alphaKeys.Length];
		for (int j = 0; j < gradientAlphaKeys.Length; j++)
		{
			gradientAlphaKeys[j] = new GradientAlphaKey
			{
				alpha = alphaKeys[j].alpha,
				time = alphaKeys[j].time
			};
		}
		gradient.SetKeys(gradientColorKeys, gradientAlphaKeys);
		return gradient;
	}
}
