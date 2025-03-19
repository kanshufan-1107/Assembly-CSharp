using Hearthstone.UI;
using PegasusShared;
using UnityEngine;

public class BaconInfoPopup : MonoBehaviour
{
	public AsyncReference m_PlayTutorialButtonReference;

	public bool m_isDuosPopup;

	private void Start()
	{
		m_PlayTutorialButtonReference.RegisterReadyListener<VisualController>(OnPlayTutorialButtonReady);
	}

	public void OnPlayTutorialButtonReady(VisualController buttonVisualController)
	{
		if (buttonVisualController == null)
		{
			Error.AddDevWarning("UI Error!", "PlayTutorialButton could not be found! You will not be able to click 'Play Tutorial'!");
			return;
		}
		UIBButton button = buttonVisualController.gameObject.GetComponent<UIBButton>();
		if (m_isDuosPopup)
		{
			button.AddEventListener(UIEventType.RELEASE, ResetDuosTutorialButtonReleased);
		}
		else
		{
			button.AddEventListener(UIEventType.RELEASE, PlayTutorialButtonRelease);
		}
	}

	public void PlayTutorialButtonRelease(UIEvent e)
	{
		if (!NetCache.Get().GetNetObject<NetCache.NetCacheFeatures>().Games.BattlegroundsTutorial)
		{
			AlertPopup.PopupInfo info = new AlertPopup.PopupInfo
			{
				m_headerText = GameStrings.Get("GLUE_TOOLTIP_BUTTON_BACON_HEADLINE"),
				m_text = GameStrings.Get("GLUE_TOOLTIP_BUTTON_DISABLED_DESC"),
				m_showAlertIcon = true,
				m_responseDisplay = AlertPopup.ResponseDisplay.OK
			};
			DialogManager.Get().ShowPopup(info);
		}
		else if (PartyManager.Get().IsInParty())
		{
			AlertPopup.PopupInfo info2 = new AlertPopup.PopupInfo
			{
				m_headerText = GameStrings.Get("GLUE_TOOLTIP_BUTTON_BACON_HEADLINE"),
				m_text = GameStrings.Get("GLUE_BACON_PARTY_TUTORIAL_DISABLED"),
				m_showAlertIcon = false,
				m_responseDisplay = AlertPopup.ResponseDisplay.OK
			};
			DialogManager.Get().ShowPopup(info2);
		}
		else
		{
			GameMgr.Get().FindGame(GameType.GT_VS_AI, FormatType.FT_WILD, 3539, 0, 0L, null, null, restoreSavedGameState: false, null, null, 0L);
		}
	}

	private void ResetDuosTutorialButtonReleased(UIEvent e)
	{
		AlertPopup.PopupInfo info = new AlertPopup.PopupInfo
		{
			m_headerText = GameStrings.Get("GLUE_BACON_INFO_POPUP_RESET_DUOS_TUTORIAL_CONFIRMATION_HEADER"),
			m_text = GameStrings.Get("GLUE_BACON_INFO_POPUP_RESET_DUOS_TUTORIAL_CONFIRMATION"),
			m_responseDisplay = AlertPopup.ResponseDisplay.OK,
			m_showAlertIcon = false
		};
		DialogManager.Get().ShowPopup(info);
		BaconDisplay.ClearDuosTutorialFlags();
	}
}
