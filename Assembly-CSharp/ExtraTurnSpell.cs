using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExtraTurnSpell : Spell
{
	public float m_WaitAnim = 4f;

	public string m_TurnText = "GAMEPLAY_NEXT_TURN";

	public string m_AnimName = "ENDTURN_NEXT_TURN";

	public bool m_DoTimeScale = true;

	protected override void OnAction(SpellStateType prevStateType)
	{
		StartCoroutine(SpellEffect(prevStateType));
		base.OnAction(prevStateType);
	}

	private IEnumerator SpellEffect(SpellStateType prevStateType)
	{
		Entity entity = m_taskList.GetSourceEntity();
		if (entity == null)
		{
			yield break;
		}
		Player controller = entity.GetController();
		if (controller == null || controller.GetSide() != Player.Side.FRIENDLY)
		{
			yield break;
		}
		EndTurnButton endButton = EndTurnButton.Get();
		if (endButton == null)
		{
			yield break;
		}
		endButton.AddInputBlocker();
		yield return new WaitForSeconds(m_WaitAnim);
		Animation anim = endButton.m_EndTurnButtonMesh.gameObject.GetComponent<Animation>();
		float animationDuration = anim.GetClip(m_AnimName).length;
		anim.Play(m_AnimName);
		List<PowerTask> taskList = m_taskList.GetTaskList();
		for (int i = 0; i < taskList.Count; i++)
		{
			if (taskList[i].GetPower() is Network.HistTagChange { Tag: 272 })
			{
				m_taskList.DoTasks(0, i + 1, null);
				break;
			}
		}
		endButton.DisplayExtraTurnState();
		yield return new WaitForSeconds(animationDuration);
		if (endButton.IsInWaitingState())
		{
			animationDuration = anim.GetClip("ENDTURN_WAITING").length;
			anim.Play("ENDTURN_WAITING");
			yield return new WaitForSeconds(animationDuration);
		}
		endButton.RemoveInputBlocker();
		endButton.DisplayExtraTurnState();
	}
}
