public struct VignetteParameters
{
	public static VignetteParameters Default = new VignetteParameters(0.4f);

	public static VignetteParameters None = new VignetteParameters(0f);

	public const float DefaultAmount = 0.4f;

	public float Amount;

	public VignetteParameters(float amount = 0.4f)
	{
		Amount = amount;
	}
}
