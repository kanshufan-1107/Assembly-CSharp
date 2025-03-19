public class EchoOfMedivh : SpawnToHandSpell
{
	protected override void OnAction(SpellStateType prevStateType)
	{
		Player controller = GetSourceCard().GetEntity().GetController();
		ZonePlay play = controller.GetBattlefieldZone();
		if (controller.IsRevealed())
		{
			for (int i = 0; i < m_targets.Count; i++)
			{
				string targetCardId = GetCardIdForTarget(i);
				for (int j = 0; j < play.GetCardCount(); j++)
				{
					Card playCard = play.GetCardAtIndex(j);
					if (playCard.GetPredictedZonePosition() == 0)
					{
						string currCardId = playCard.GetEntity().GetCardId();
						if (!(targetCardId != currCardId) && AddUniqueOriginForTarget(i, playCard))
						{
							break;
						}
					}
				}
			}
		}
		else
		{
			int playZoneIndex = 0;
			for (int k = 0; k < m_targets.Count; k++)
			{
				Card playCard2 = play.GetCardAtIndex(playZoneIndex);
				AddOriginForTarget(k, playCard2);
				playZoneIndex++;
			}
		}
		base.OnAction(prevStateType);
	}
}
