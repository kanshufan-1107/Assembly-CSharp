using Assets;
using Hearthstone.DataModels;
using Hearthstone.UI;
using UnityEngine;

[RequireComponent(typeof(WidgetTemplate))]
public class ChooseOneRewardChoiceConfirmation : MonoBehaviour
{
	public const string CLAIM_CLICKED = "CODE_CLAIM_CLICKED";

	private WidgetTemplate m_widget;

	private void Awake()
	{
		m_widget = GetComponent<WidgetTemplate>();
		m_widget.RegisterEventListener(delegate(string eventName)
		{
			if (eventName == "CODE_CLAIM_CLICKED")
			{
				ClaimClicked();
			}
		});
	}

	public void ClaimClicked()
	{
		if (!(m_widget.GetDataModel<EventDataModel>()?.Payload is RewardItemDataModel rewardItemDataModel))
		{
			Debug.LogError("ChooseOneRewardChoiceConfirmation: failed to get reward item data model from event payload!");
			return;
		}
		m_widget.TriggerEvent("HIDE_POPUP_FOR_CONFIRM");
		RewardItemDbfRecord record = GameDbf.RewardItem.GetRecord(rewardItemDataModel.AssetId);
		string rewardText = "";
		if (record.RewardType == RewardItem.RewardType.HERO_SKIN)
		{
			string className = GameStrings.GetClassName(GameUtils.GetTagClassFromCardDbId(record.Card));
			rewardText = GameStrings.Format("GLUE_PROGRESSION_REWARD_TRACK_POPUP_SKIN_CHOICE_CONFIRMATION_TEXT", className, record.CardRecord.Name.GetString());
		}
		else if (record.RewardType == RewardItem.RewardType.DECK)
		{
			string className2 = GameStrings.GetClassName((TAG_CLASS)record.DeckRecord.ClassId);
			rewardText = GameStrings.Format("GLUE_PROGRESSION_REWARD_TRACK_POPUP_DECK_CHOICE_CONFIRMATION_TEXT", className2, record.DeckRecord.DeckRecord.Name.GetString());
		}
		AlertPopup.PopupInfo info = new AlertPopup.PopupInfo
		{
			m_headerText = GameStrings.Get("GLUE_PROGRESSION_REWARD_TRACK_POPUP_SKIN_CHOICE_CONFIRMATION_HEADER"),
			m_text = rewardText,
			m_showAlertIcon = false,
			m_alertTextAlignment = UberText.AlignmentOptions.Center,
			m_responseDisplay = AlertPopup.ResponseDisplay.CONFIRM_CANCEL,
			m_responseCallback = delegate(AlertPopup.Response response, object userData)
			{
				if (response == AlertPopup.Response.CONFIRM)
				{
					m_widget.TriggerEvent("CLAIM_CHOOSE_ONE_REWARD");
				}
				else
				{
					m_widget.TriggerEvent("SHOW_POPUP_AFTER_CONFIRM");
				}
			}
		};
		DialogManager.Get().ShowPopup(info);
	}
}
