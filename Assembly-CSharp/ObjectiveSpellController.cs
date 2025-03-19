internal class ObjectiveSpellController : SpellController
{
	protected override bool AddPowerSourceAndTargets(PowerTaskList taskList)
	{
		if (!HasSourceCard(taskList))
		{
			return false;
		}
		Card sourceCard = taskList.GetSourceEntity().GetCard();
		SetSource(sourceCard);
		return true;
	}

	protected override void OnProcessTaskList()
	{
		if (m_taskList.IsStartOfBlock())
		{
			FireActorSpell();
		}
		base.OnProcessTaskList();
	}

	private bool FireActorSpell()
	{
		Card card = GetSource();
		if (card == null)
		{
			Log.All.PrintWarning("ObjectiveSpellController - source card is null");
			return false;
		}
		Actor actor = card.GetActor();
		if (actor == null)
		{
			Log.All.PrintWarning("ObjectiveSpellController - source card actor is null");
			return false;
		}
		Spell spell = actor.GetComponent<Spell>();
		if (spell == null)
		{
			Log.All.PrintWarning("ObjectiveSpellController - source card [" + card.name + " | " + card.ToString() + "] doesn't have Spell component");
			return false;
		}
		spell.SafeActivateState(SpellStateType.ACTION);
		return true;
	}
}
