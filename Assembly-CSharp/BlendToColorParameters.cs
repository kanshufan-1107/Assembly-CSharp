using UnityEngine;

public struct BlendToColorParameters
{
	public static BlendToColorParameters Default = default(BlendToColorParameters);

	public static BlendToColorParameters None = new BlendToColorParameters(Color.white, 0f);

	public Color BlendColor;

	public float Amount;

	public BlendToColorParameters(Color blendColor, float amount)
	{
		BlendColor = blendColor;
		Amount = amount;
	}
}
