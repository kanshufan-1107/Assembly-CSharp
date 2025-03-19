using UnityEngine;

public class AncientCurseSpell : SuperSpell
{
	public void DoHeroDamage()
	{
		PowerTaskList taskList = GameState.Get().GetPowerProcessor().GetCurrentTaskList();
		if (taskList == null)
		{
			Debug.LogWarning("AncientCurseSpell.DoHeroDamage() called when there was no current PowerTaskList!");
		}
		else
		{
			GameUtils.DoDamageTasks(taskList, GetSourceCard(), GetVisualTargetCard());
		}
	}
}
