public class ZombeastCardTextBuilder : ModularEntityCardTextBuilder
{
	protected override string BuildFormattedText(Entity entity)
	{
		GetPowersText(entity, out var power1, out var power2);
		RemoveConflictingTags(ref power1);
		RemoveConflictingTags(ref power2);
		return string.Format(GetRawCardTextInHandForCardBeingBuilt(entity), power1, power2);
	}

	public override string GetRawCardTextInHandForCardBeingBuilt(Entity ent)
	{
		GetPowersText(ent, out var power1, out var _);
		if (string.IsNullOrEmpty(power1))
		{
			return "{1}";
		}
		string zombeastCardText = CardTextBuilder.GetRawCardTextInHand("ICC_828t");
		if (string.IsNullOrEmpty(zombeastCardText))
		{
			Log.All.PrintError("ZombeastCardTextBuilder.GetRawCardTextInHandForCardBeingBuilt: Could not find card text for ICC_828t.");
			return "{0}\n{1}";
		}
		return zombeastCardText;
	}

	private void RemoveConflictingTags(ref string power)
	{
		int prevLength = power.Length;
		string disableAutoWrapString = new string(new char[3] { '[', 'x', ']' });
		power = power.Replace(disableAutoWrapString, "");
		if (power.Length != prevLength)
		{
			power = power.Replace('\n', ' ');
		}
	}
}
