using Hearthstone.DataModels;
using Hearthstone.UI;
using UnityEngine;

public class RewardTrackItemPopup : MonoBehaviour
{
	private const string HIDE_POPUP = "HIDE_POPUP";

	private WidgetTemplate m_widgetTemplate;

	private int m_lastDataVer;

	private void Awake()
	{
		m_widgetTemplate = GetComponent<WidgetTemplate>();
		if (!(m_widgetTemplate != null))
		{
			return;
		}
		m_widgetTemplate.RegisterDoneChangingStatesListener(HandleDoneChangingStates);
		m_widgetTemplate.RegisterEventListener(delegate(string eventName)
		{
			if (eventName == "HIDE_POPUP")
			{
				m_widgetTemplate.TriggerEvent("CODE_HIDE_SIGNATURE_TOOLTIP", new TriggerEventParameters(ToString(), null, noDownwardPropagation: false, ignorePlaymaker: true));
			}
		});
	}

	private void HandleDoneChangingStates(object unused)
	{
		if (m_widgetTemplate.GetDataModel(236, out var dataModel))
		{
			RewardListDataModel rewardListDM = (dataModel as RewardTrackNodeRewardsDataModel).Items;
			if (rewardListDM != null && !rewardListDM.ChooseOne && rewardListDM.Items.Count == 1 && rewardListDM.Items[0].ItemType == RewardItemType.CARD && rewardListDM.Items[0].Card.Premium == TAG_PREMIUM.SIGNATURE)
			{
				m_widgetTemplate.TriggerEvent("CODE_SHOW_SIGNATURE_TOOLTIP", new TriggerEventParameters(ToString(), null, noDownwardPropagation: false, ignorePlaymaker: true));
			}
		}
	}
}
