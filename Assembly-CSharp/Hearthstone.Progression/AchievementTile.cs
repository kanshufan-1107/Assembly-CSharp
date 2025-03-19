using Hearthstone.Core.Streaming;
using Hearthstone.DataModels;
using Hearthstone.Streaming;
using Hearthstone.UI;
using UnityEngine;

namespace Hearthstone.Progression;

public class AchievementTile : MonoBehaviour
{
	private Widget m_widget;

	private void Awake()
	{
		m_widget = GetComponent<WidgetTemplate>();
		m_widget.WillLoadSynchronously = true;
		m_widget.RegisterDoneChangingStatesListener(delegate
		{
			if (ProgressUtils.ShowDebugIds)
			{
				m_widget.TriggerEvent("DEBUG_SHOW_ID");
			}
		}, null, callImmediatelyIfSet: true, doOnce: true);
	}

	public void BindDataModel(AchievementDataModel achievement)
	{
		m_widget.BindDataModel(achievement);
	}

	public static string GetLockedMessage(DownloadTags.Content tag)
	{
		string moduleName = DownloadUtils.GetGameModeName(tag);
		return GameStrings.Format("GLOBAL_PROGRESSION_ACHIEVEMENT_TILE_LOCKED_MESSAGE", moduleName);
	}
}
