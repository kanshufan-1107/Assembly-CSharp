public class NefarianTB : Spell
{
	protected override void OnAction(SpellStateType prevStateType)
	{
		BlockZoneLayout();
		base.OnAction(prevStateType);
	}

	private void BlockZoneLayout()
	{
		Card sourceCard = GetSourceCard();
		if (sourceCard == null)
		{
			return;
		}
		Player player = sourceCard.GetController();
		if (player != null)
		{
			ZonePlay battlefield = player.GetBattlefieldZone();
			if (!(battlefield == null))
			{
				battlefield.AddLayoutBlocker();
			}
		}
	}
}
