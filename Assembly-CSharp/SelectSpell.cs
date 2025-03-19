using System.Collections.Generic;
using PegasusGame;

[CustomEditClass]
public class SelectSpell : Spell
{
	public List<SelectSpellTableEntry> m_Table = new List<SelectSpellTableEntry>();

	private Spell m_selectedSpell;

	private int m_selectionIndex = -1;

	private void LoadSelectedSpell(int selection)
	{
		if (m_selectionIndex == selection && (bool)m_selectedSpell)
		{
			return;
		}
		m_selectionIndex = selection;
		if ((bool)m_selectedSpell)
		{
			if (m_selectedSpell.HasUsableState(SpellStateType.CANCEL))
			{
				m_selectedSpell.ActivateState(SpellStateType.CANCEL);
			}
			else
			{
				m_selectedSpell.Deactivate();
			}
			m_selectedSpell = null;
		}
		SelectSpellTableEntry selectedEntry = null;
		foreach (SelectSpellTableEntry entry in m_Table)
		{
			if (entry.m_Selection == selection)
			{
				selectedEntry = entry;
				break;
			}
		}
		if (selectedEntry != null && selectedEntry.m_Spell != null)
		{
			m_selectedSpell = SpellManager.Get().GetSpell(selectedEntry.m_Spell);
			if (m_selectedSpell != null)
			{
				TransformUtil.AttachAndPreserveLocalTransform(m_selectedSpell.transform, base.gameObject.transform);
			}
		}
	}

	public override bool AttachPowerTaskList(PowerTaskList taskList)
	{
		if (!SetSelectedSpell(taskList))
		{
			return false;
		}
		if (!m_selectedSpell.AttachPowerTaskList(taskList))
		{
			return false;
		}
		return base.AttachPowerTaskList(taskList);
	}

	private bool SetSelectedSpell(PowerTaskList taskList)
	{
		foreach (PowerTask task in taskList.GetTaskList())
		{
			Network.PowerHistory power = task.GetPower();
			if (power.Type == Network.PowerType.META_DATA)
			{
				Network.HistMetaData metaData = (Network.HistMetaData)power;
				if (metaData.MetaType == HistoryMeta.Type.EFFECT_SELECTION)
				{
					int selection = metaData.Data;
					LoadSelectedSpell(selection);
					return m_selectedSpell != null;
				}
			}
		}
		return false;
	}

	protected override void OnBirth(SpellStateType prevStateType)
	{
		LoadSelectedSpell(0);
		if ((bool)m_selectedSpell)
		{
			m_selectedSpell.SetSource(GetSource());
			m_selectedSpell.AddStateFinishedCallback(OnSelectedSpellStateFinished);
			m_selectedSpell.ActivateState(SpellStateType.BIRTH);
		}
		base.OnBirth(prevStateType);
	}

	protected override void OnAction(SpellStateType prevStateType)
	{
		m_selectedSpell.SetSource(GetSource());
		m_selectedSpell.AddFinishedCallback(OnSelectedSpellFinished);
		m_selectedSpell.AddStateFinishedCallback(OnSelectedSpellStateFinished);
		m_selectedSpell.ActivateState(SpellStateType.ACTION);
		base.OnAction(prevStateType);
	}

	protected override void OnCancel(SpellStateType prevStateType)
	{
		if (m_selectedSpell != null && m_selectedSpell.GetActiveState() != 0 && m_selectedSpell.GetActiveState() != SpellStateType.CANCEL)
		{
			m_selectedSpell.ActivateState(SpellStateType.CANCEL);
		}
		base.OnCancel(prevStateType);
	}

	private void OnSelectedSpellFinished(Spell spell, object userData)
	{
		OnSpellFinished();
	}

	private void OnSelectedSpellStateFinished(Spell spell, SpellStateType prevStateType, object userData)
	{
		if (spell.GetActiveState() == SpellStateType.NONE && spell == m_selectedSpell)
		{
			m_selectedSpell = null;
			m_selectionIndex = -1;
			Deactivate();
		}
	}
}
