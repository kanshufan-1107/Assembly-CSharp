public struct BlurParameters
{
	public static BlurParameters Default = new BlurParameters(1f, 0.5f, 1f);

	public static BlurParameters None = new BlurParameters(0f, 1f, 0f);

	public const float DefaultAmount = 1f;

	public const float DefaultBrightness = 0.5f;

	public const float DefaultBlend = 1f;

	public float Amount;

	public float Brightness;

	public float Blend;

	public BlurParameters(float amount = 1f, float brightness = 0.5f, float blend = 1f)
	{
		Amount = amount;
		Brightness = brightness;
		Blend = blend;
	}
}
