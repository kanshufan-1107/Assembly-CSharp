using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VarianWrynn : SuperSpell
{
	public string m_perMinionSound;

	public Spell m_varianSpellPrefab;

	public Spell m_deckSpellPrefab;

	public float m_spellLeadTime = 1f;

	protected override void OnAction(SpellStateType prevStateType)
	{
		m_effectsPendingFinish++;
		base.OnAction(prevStateType);
		StartCoroutine(DoVariansCoolThing());
	}

	private IEnumerator DoVariansCoolThing()
	{
		Card sourceCard = m_taskList.GetSourceEntity().GetCard();
		List<Spell> fxObjects = new List<Spell>();
		if (m_varianSpellPrefab != null && m_taskList.IsOrigin())
		{
			Spell spell = SpellManager.Get().GetSpell(m_varianSpellPrefab);
			fxObjects.Add(spell);
			spell.SetSource(sourceCard.gameObject);
			spell.Activate();
		}
		List<PowerTask> tasks = m_taskList.GetTaskList();
		bool foundTarget = false;
		bool lastWasMinion = false;
		int i = 0;
		while (i < tasks.Count)
		{
			Network.PowerHistory power = tasks[i].GetPower();
			if (power.Type == Network.PowerType.SHOW_ENTITY)
			{
				Network.HistShowEntity showEntity = (Network.HistShowEntity)power;
				if (!foundTarget)
				{
					Card targetCard = GameState.Get().GetEntity(showEntity.Entity.ID).GetCard();
					foundTarget = true;
					if (m_deckSpellPrefab != null && m_taskList.IsOrigin())
					{
						Spell spell2 = SpellManager.Get().GetSpell(m_deckSpellPrefab);
						fxObjects.Add(spell2);
						spell2.SetSource(targetCard.gameObject);
						spell2.Activate();
						while (!spell2.IsFinished())
						{
							yield return null;
						}
					}
				}
				bool complete = false;
				PowerTaskList.CompleteCallback completeCallback = delegate
				{
					complete = true;
				};
				m_taskList.DoTasks(0, i, completeCallback);
				if (lastWasMinion)
				{
					yield return new WaitForSeconds(m_spellLeadTime);
				}
				lastWasMinion = IsMinion(showEntity);
				while (!complete)
				{
					yield return null;
				}
			}
			int num = i + 1;
			i = num;
		}
		foreach (Spell fxObj in fxObjects)
		{
			SpellManager.Get().ReleaseSpell(fxObj);
		}
		m_effectsPendingFinish--;
		FinishIfPossible();
	}

	private bool IsMinion(Network.HistShowEntity showEntity)
	{
		for (int j = 0; j < showEntity.Entity.Tags.Count; j++)
		{
			Network.Entity.Tag tag = showEntity.Entity.Tags[j];
			if (tag.Name == 202)
			{
				return tag.Value == 4;
			}
		}
		return false;
	}
}
