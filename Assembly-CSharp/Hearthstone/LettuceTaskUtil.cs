using System;

namespace Hearthstone;

public class LettuceTaskUtil
{
	public static void ClaimTask(int taskId)
	{
		Network.Get().ClaimMercenaryTask(taskId);
	}

	public static void PromptDismissTask(int taskId, Action onCancel)
	{
		AlertPopup.PopupInfo info = new AlertPopup.PopupInfo
		{
			m_headerText = GameStrings.Get("GLUE_LETTUCE_VILLAGE_DISMISS_TASK_WARNING_HEADER"),
			m_text = GameStrings.Get("GLUE_LETTUCE_VILLAGE_DISMISS_TASK_WARNING_DESCRIPTION"),
			m_showAlertIcon = false,
			m_responseDisplay = AlertPopup.ResponseDisplay.CONFIRM_CANCEL,
			m_responseCallback = delegate(AlertPopup.Response response, object userData)
			{
				switch (response)
				{
				case AlertPopup.Response.CONFIRM:
					DismissTask(taskId);
					break;
				case AlertPopup.Response.CANCEL:
					onCancel();
					break;
				}
			}
		};
		DialogManager.Get().ShowPopup(info);
	}

	public static void DismissTask(int taskId)
	{
		VisitorTaskDbfRecord taskRecord = LettuceVillageDataUtil.GetTaskRecordByID(taskId);
		if (taskRecord == null)
		{
			Log.Lettuce.PrintError($"Error in LettuceVillageTaskBoard.DismissTask: no task record for task id {taskId}");
		}
		else
		{
			Network.Get().DismissMercenaryTask(taskRecord.MercenaryVisitorId);
		}
	}
}
