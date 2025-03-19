using System.Collections.Generic;

public class HemetJungleHunterSpell : Spell
{
	private int m_cardsDestroyed;

	public override bool AddPowerTargets()
	{
		if (!CanAddPowerTargets())
		{
			return false;
		}
		int cardDestroyedCount = 0;
		List<PowerTask> taskList = m_taskList.GetTaskList();
		for (int i = 0; i < taskList.Count; i++)
		{
			if (!(taskList[i].GetPower() is Network.HistShowEntity showEntity))
			{
				continue;
			}
			foreach (Network.Entity.Tag tag in showEntity.Entity.Tags)
			{
				if (tag.Name == 49 && tag.Value == 6)
				{
					cardDestroyedCount++;
					break;
				}
			}
		}
		m_cardsDestroyed = cardDestroyedCount;
		return true;
	}

	protected override void OnAttachPowerTaskList()
	{
		base.OnAttachPowerTaskList();
		PlayMakerFSM fsm = GetComponent<PlayMakerFSM>();
		if (fsm != null)
		{
			fsm.FsmVariables.GetFsmInt("CardsDestroyed").Value = m_cardsDestroyed;
		}
	}
}
