using System;
using Hearthstone.UI.Core;
using HutongGames.PlayMaker;
using UnityEngine;

public class UnrepeatableFsmTrigger : MonoBehaviour
{
	[SerializeField]
	private PlayMakerFSM m_targetFsm;

	[SerializeField]
	private string m_targetState;

	private string m_activeAction;

	[Overridable]
	public string StartingAction
	{
		set
		{
			m_activeAction = value;
		}
	}

	[Overridable]
	public string TriggerAction
	{
		get
		{
			return m_activeAction;
		}
		set
		{
			if (string.IsNullOrEmpty(value))
			{
				m_activeAction = string.Empty;
			}
			else if (string.IsNullOrEmpty(m_activeAction) || !m_activeAction.Equals(value, StringComparison.OrdinalIgnoreCase))
			{
				m_activeAction = value;
				TryTriggerFsmState();
			}
		}
	}

	private void TryTriggerFsmState()
	{
		if (m_targetFsm == null || string.IsNullOrEmpty(m_targetState))
		{
			Debug.LogError("Failed to Trigger Fsm as " + base.gameObject.name + "\\UnrepeatableFsmTrigger is miss configured!");
		}
		else
		{
			if (!base.gameObject.activeInHierarchy)
			{
				return;
			}
			FsmState[] fsmStates = m_targetFsm.FsmStates;
			for (int i = 0; i < fsmStates.Length; i++)
			{
				if (fsmStates[i].Name == m_targetState)
				{
					m_targetFsm.SetState(m_targetState);
					break;
				}
			}
		}
	}
}
