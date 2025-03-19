using System.Collections;
using Blizzard.T5.Core;

public class YoggSaronSpell : Spell
{
	public Spell m_MistSpellPrefab;

	private static Map<int, Spell> s_mistSpellInstances = new Map<int, Spell>();

	public override bool CanPurge()
	{
		return s_mistSpellInstances.Count == 0;
	}

	public override bool AddPowerTargets()
	{
		int taskListID = m_taskList.GetOrigin().GetId();
		if (s_mistSpellInstances.ContainsKey(taskListID) && !m_taskList.IsEndOfBlock())
		{
			return false;
		}
		return true;
	}

	protected override void OnAction(SpellStateType prevStateType)
	{
		base.OnAction(prevStateType);
		StartCoroutine(DoEffectsWithTiming());
	}

	private IEnumerator DoEffectsWithTiming()
	{
		int taskListID = m_taskList.GetOrigin().GetId();
		Spell mistSpellInstance;
		if (!s_mistSpellInstances.ContainsKey(taskListID))
		{
			mistSpellInstance = SpellManager.Get().GetSpell(m_MistSpellPrefab);
			s_mistSpellInstances[taskListID] = mistSpellInstance;
			if ((bool)mistSpellInstance)
			{
				mistSpellInstance.ActivateState(SpellStateType.BIRTH);
				while (mistSpellInstance.GetActiveState() != SpellStateType.IDLE)
				{
					yield return null;
				}
			}
		}
		else
		{
			mistSpellInstance = s_mistSpellInstances[taskListID];
		}
		if ((bool)mistSpellInstance && m_taskList.IsEndOfBlock())
		{
			mistSpellInstance.ActivateState(SpellStateType.DEATH);
			while (!mistSpellInstance.IsFinished())
			{
				yield return null;
			}
			OnSpellFinished();
			while (mistSpellInstance.GetActiveState() != 0)
			{
				yield return null;
			}
			s_mistSpellInstances.Remove(taskListID);
			SpellManager.Get().ReleaseSpell(mistSpellInstance);
		}
		if (GetActiveState() != 0)
		{
			OnStateFinished();
		}
	}
}
