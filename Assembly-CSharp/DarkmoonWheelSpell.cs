using PegasusGame;

public class DarkmoonWheelSpell : SuperSpell
{
	private int m_metadataChoice = -1;

	public override bool ShouldReconnectIfStuck()
	{
		return false;
	}

	public override bool AttachPowerTaskList(PowerTaskList taskList)
	{
		bool returnValue = base.AttachPowerTaskList(taskList);
		m_metadataChoice = GetSpinResultMetadata();
		if (m_metadataChoice == -1)
		{
			return false;
		}
		return returnValue;
	}

	private int GetSpinResultMetadata()
	{
		foreach (PowerTask task in m_taskList.GetTaskList())
		{
			Network.PowerHistory power = task.GetPower();
			if (power.Type == Network.PowerType.META_DATA)
			{
				Network.HistMetaData metaData = power as Network.HistMetaData;
				if (metaData.MetaType == HistoryMeta.Type.EFFECT_SELECTION)
				{
					return metaData.Data;
				}
			}
		}
		return -1;
	}

	protected override void DoActionNow()
	{
		m_startSpell.GameObject.GetComponent<PlayMakerFSM>().FsmVariables.GetFsmInt("YoggWheelOutcome").Value = m_metadataChoice;
		base.DoActionNow();
	}
}
