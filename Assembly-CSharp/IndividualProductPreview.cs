using Hearthstone.DataModels;
using Hearthstone.UI;
using UnityEngine;

public class IndividualProductPreview : MonoBehaviour
{
	[SerializeField]
	private Widget m_widget;

	private ItemPreviewOptionDataModel m_itemPreviewOptionDataModel;

	private const string REWARD_ITEM_CHANGED_CODE_EVENT = "REWARD_ITEM_CHANGED_CODE";

	private const string CODE_SHOW_POPUP = "SHOW_POPUP";

	private const string CODE_HIDE_POPUP = "HIDE_POPUP";

	private void Awake()
	{
		if (m_widget != null)
		{
			m_widget.RegisterEventListener(HandleEvent);
		}
		UpdateItemPreviewModel();
	}

	private void HandleEvent(string e)
	{
		switch (e)
		{
		case "REWARD_ITEM_CHANGED_CODE":
			UpdateItemPreviewModel();
			break;
		case "SHOW_POPUP":
			ShowSignatureTooltip();
			break;
		case "HIDE_POPUP":
			HideSignatureTooltip();
			break;
		}
	}

	private void ShowSignatureTooltip()
	{
		EventDataModel eventDataModel = m_widget.GetDataModel<EventDataModel>();
		if (eventDataModel == null || !(eventDataModel.Payload is RewardItemDataModel rewardItemDataModel) || (rewardItemDataModel.ItemType == RewardItemType.CARD && rewardItemDataModel.Card.Premium == TAG_PREMIUM.SIGNATURE))
		{
			m_widget.RegisterDoneChangingStatesListener(OnReadyToShowSignatureTooltip);
		}
	}

	private void HideSignatureTooltip()
	{
		EventDataModel eventDataModel = m_widget.GetDataModel<EventDataModel>();
		if (eventDataModel == null || !(eventDataModel.Payload is RewardItemDataModel rewardItemDataModel) || (rewardItemDataModel.ItemType == RewardItemType.CARD && rewardItemDataModel.Card.Premium == TAG_PREMIUM.SIGNATURE))
		{
			m_widget.TriggerEvent("CODE_HIDE_SIGNATURE_TOOLTIP", new TriggerEventParameters(ToString(), null, noDownwardPropagation: false, ignorePlaymaker: true));
		}
	}

	private void OnReadyToShowSignatureTooltip(object payload)
	{
		m_widget.TriggerEvent("CODE_SHOW_SIGNATURE_TOOLTIP", new TriggerEventParameters(ToString(), null, noDownwardPropagation: false, ignorePlaymaker: true));
		m_widget.RemoveDoneChangingStatesListener(OnReadyToShowSignatureTooltip);
	}

	private void UpdateItemPreviewModel()
	{
		if (!(m_widget == null))
		{
			RewardItemDataModel rewardItem = null;
			if (m_widget.GetDataModel(17, out var dataModel))
			{
				rewardItem = (RewardItemDataModel)dataModel;
			}
			if (m_itemPreviewOptionDataModel == null)
			{
				m_itemPreviewOptionDataModel = new ItemPreviewOptionDataModel();
				m_widget.BindDataModel(m_itemPreviewOptionDataModel);
			}
			m_itemPreviewOptionDataModel.PreviewItem = rewardItem;
		}
	}
}
