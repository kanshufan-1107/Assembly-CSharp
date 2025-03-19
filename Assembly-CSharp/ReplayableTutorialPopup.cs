using Hearthstone.UI;
using PegasusShared;
using UnityEngine;

public class ReplayableTutorialPopup : MonoBehaviour
{
	private Widget m_widget;

	private const string CODE_REPLAY_TUTORIAL = "PLAY_TUTORIAL_CODE";

	private const string MYSTERY_SCENARIO = "SCENARIO_5393";

	private void Awake()
	{
		m_widget = GetComponent<WidgetTemplate>();
		m_widget.RegisterEventListener(HandleEvent);
	}

	private void HandleEvent(string eventName)
	{
		if (!(eventName == "PLAY_TUTORIAL_CODE"))
		{
			if (eventName == "SCENARIO_5393")
			{
				GameMgr.Get().FindGame(GameType.GT_VS_AI, FormatType.FT_WILD, 5393, 0, 0L, null, null, restoreSavedGameState: false, null, null, 0L);
				m_widget.Hide();
			}
		}
		else
		{
			GameUtils.ReplayTraditionalTutorial();
			m_widget.Hide();
		}
	}
}
