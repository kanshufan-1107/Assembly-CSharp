using Hearthstone.DataModels;
using Hearthstone.UI;
using UnityEngine;

public class GameMenuSignUpButton : MonoBehaviour
{
	[SerializeField]
	private UIBButton m_infoButton;

	[SerializeField]
	private TooltipZone m_tooltipZone;

	private void Start()
	{
		m_infoButton.AddEventListener(UIEventType.ROLLOVER, OnInfoButtonRollOver);
		m_infoButton.AddEventListener(UIEventType.ROLLOUT, OnInfoButtonRollOut);
	}

	private void OnInfoButtonRollOver(UIEvent e)
	{
		string headline = GameStrings.Get("GLUE_TEMPORARY_ACCOUNT_GAME_MENU_SIGN_UP_TOOLTIP_HEADER");
		string message = GameStrings.Get("GLUE_TEMPORARY_ACCOUNT_GAME_MENU_SIGN_UP_TOOLTIP");
		m_tooltipZone.ShowBoxTooltip(headline, message);
		m_tooltipZone.LayerOverride = GameLayer.HighPriorityUI;
		float scale = 0.5f;
		if (GlobalDataContext.Get().GetDataModel(0, out var datamodel) && ((DeviceDataModel)datamodel).Screen < ScreenCategory.MiniTablet)
		{
			scale = 0.65f;
		}
		m_tooltipZone.Scale = scale;
	}

	private void OnInfoButtonRollOut(UIEvent e)
	{
		m_tooltipZone.HideTooltip();
	}
}
