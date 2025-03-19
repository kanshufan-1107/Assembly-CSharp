public class PoweredUpTargetingTextBuilder : CardTextBuilder
{
	public override string GetTargetingArrowText(Entity entity)
	{
		string builtText = base.GetTargetingArrowText(entity);
		int delimiterIndex = builtText.IndexOf('@');
		if (delimiterIndex >= 0)
		{
			entity.HasCombo();
			builtText = ((!entity.GetRealTimePoweredUp() && (!entity.GetController().IsComboActive() || !entity.HasCombo())) ? builtText.Substring(0, delimiterIndex) : builtText.Substring(delimiterIndex + 1));
		}
		return TextUtils.TransformCardText(entity, builtText);
	}
}
