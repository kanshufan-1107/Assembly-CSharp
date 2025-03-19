public struct DesaturateParameters
{
	public static DesaturateParameters Default = new DesaturateParameters(0.5f);

	public static DesaturateParameters None = new DesaturateParameters(0f);

	public const float DefaultAmount = 0.5f;

	public float Amount;

	public DesaturateParameters(float amount = 0.5f)
	{
		Amount = amount;
	}
}
