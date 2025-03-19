public class SpawnMinionsToHandFromPlaySpell : SpawnToHandSpell
{
	protected override void OnAction(SpellStateType prevStateType)
	{
		ZonePlay play = GetSourceCard().GetEntity().GetController().GetBattlefieldZone();
		for (int i = 0; i < m_targets.Count; i++)
		{
			for (int j = 0; j < play.GetCardCount(); j++)
			{
				Card playCard = play.GetCardAtIndex(j);
				Card targetCard = m_targets[i].GetComponent<Card>();
				int entityId = playCard.GetEntity().GetEntityId();
				int linkedEntityId = targetCard.GetEntity().GetRealTimeLinkedEntityId();
				if (entityId == linkedEntityId && AddUniqueOriginForTarget(i, playCard))
				{
					break;
				}
			}
		}
		base.OnAction(prevStateType);
	}
}
