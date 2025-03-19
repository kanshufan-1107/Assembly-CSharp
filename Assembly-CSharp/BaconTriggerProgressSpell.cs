using System.Collections;
using HutongGames.PlayMaker;
using UnityEngine;

public class BaconTriggerProgressSpell : Spell
{
	public UberText m_ProgressText;

	public bool m_StayUp;

	private bool m_textInitialized;

	protected void SetupProgressText(bool fromBirth = false)
	{
		StopCoroutine("WaitThenResetText");
		Card source = GetSourceCard();
		if (source == null)
		{
			return;
		}
		int current = 0;
		bool foundValue = false;
		int total = (source.GetEntity()?.GetEntityDef()?.GetTag(GAME_TAG.TAG_SCRIPT_DATA_NUM_1)).GetValueOrDefault();
		if (m_taskList != null)
		{
			foundValue = m_taskList.GetTagUpdatedValue(source.GetEntity(), GAME_TAG.TAG_SCRIPT_DATA_NUM_1, ref current);
			if (foundValue && current == total)
			{
				current = 0;
			}
		}
		int progress = total - current;
		if ((fromBirth || (!foundValue && m_StayUp)) && source.GetEntity() != null)
		{
			int sdn1 = source.GetEntity().GetTag(GAME_TAG.TAG_SCRIPT_DATA_NUM_1);
			progress = total - sdn1;
			if (sdn1 == 0)
			{
				progress = 0;
			}
		}
		m_ProgressText.Text = GameStrings.Format("GAMEPLAY_PROGRESS_X_OF_Y", progress, total);
		FsmBool fsmVar = m_fsm.FsmVariables.GetFsmBool("TriggerRequested");
		if (fsmVar != null)
		{
			fsmVar.Value = foundValue;
		}
		fsmVar = m_fsm.FsmVariables.GetFsmBool("IsComplete");
		if (fsmVar != null)
		{
			fsmVar.Value = progress == total;
			if (progress == total && m_StayUp)
			{
				StartCoroutine("WaitThenResetText");
			}
		}
		m_textInitialized = true;
	}

	private IEnumerator WaitThenResetText()
	{
		yield return new WaitForSeconds(2f);
		Card source = GetSourceCard();
		if (!(source == null))
		{
			int total = (source.GetEntity()?.GetEntityDef()?.GetTag(GAME_TAG.TAG_SCRIPT_DATA_NUM_1)).GetValueOrDefault();
			m_ProgressText.Text = GameStrings.Format("GAMEPLAY_PROGRESS_X_OF_Y", 0, total);
		}
	}

	protected override void OnBirth(SpellStateType prevStateType)
	{
		if (m_StayUp && !m_textInitialized)
		{
			SetupProgressText(fromBirth: true);
		}
		base.OnBirth(prevStateType);
	}

	protected override void OnAction(SpellStateType prevStateType)
	{
		SetupProgressText();
		base.OnAction(prevStateType);
	}
}
