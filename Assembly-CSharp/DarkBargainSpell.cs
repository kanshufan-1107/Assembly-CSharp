using System.Collections.Generic;

public class DarkBargainSpell : OverrideCustomDeathSpell
{
	public override bool AddPowerTargets()
	{
		List<PowerTask> tasks = m_taskList.GetTaskList();
		AddMultiplePowerTargets_FromMetaData(tasks);
		return true;
	}
}
