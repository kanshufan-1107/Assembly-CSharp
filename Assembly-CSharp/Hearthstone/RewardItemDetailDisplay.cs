using Hearthstone.DataModels;
using Hearthstone.UI;
using UnityEngine;

namespace Hearthstone;

[RequireComponent(typeof(WidgetTemplate))]
public class RewardItemDetailDisplay : MonoBehaviour
{
	public const string SHOW_REWARD_DETAIL = "SHOW_REWARD_DETAIL";

	public const string SHOW_REWARD_ENLARGED_DETAIL = "SHOW_REWARD_ENLARGED_DETAIL";

	public const string HIDE_REWARD_DETAIL = "HIDE_REWARD_DETAIL";

	public const string BIND_DATA_MODEL = "BIND_DATA_MODEL";

	public Widget m_enlargedRewardDetailWidget;

	public Widget m_rewardItemWidget;

	public Vector3 m_tooltipPositionOffset;

	public float m_tooltipScale;

	private GameObject m_rewardToolTip;

	private Widget m_widget;

	private RewardItemDataModel m_rewardItemDataModel;

	private void Awake()
	{
		m_widget = GetComponent<Widget>();
		m_widget.RegisterEventListener(delegate(string eventName)
		{
			HandleEvent(eventName, m_widget);
		});
	}

	private void Start()
	{
		if (m_rewardItemWidget != null && m_rewardItemDataModel != null)
		{
			m_rewardItemWidget.BindDataModel(m_rewardItemDataModel);
		}
		if (m_enlargedRewardDetailWidget != null)
		{
			m_enlargedRewardDetailWidget.RegisterDoneChangingStatesListener(OnEnlargedDetailDoneChangingStates, null, callImmediatelyIfSet: false);
			m_enlargedRewardDetailWidget.RegisterReadyListener(OnEnlargedDetailDoneChangingStates, null, callImmediatelyIfReady: false);
		}
	}

	private void OnDestroy()
	{
		DestroyRewardTooltip();
	}

	private void OnEnlargedDetailDoneChangingStates(object widget)
	{
		m_widget.TriggerEvent("SHOW_REWARD_ENLARGED_DETAIL");
	}

	private void HandleEvent(string eventName, Widget widget)
	{
		if (eventName.Equals("SHOW_REWARD_DETAIL"))
		{
			CreateRewardDetailObject();
		}
		else if (eventName.Equals("HIDE_REWARD_DETAIL"))
		{
			DestroyRewardTooltip();
		}
		else if (eventName.Equals("BIND_DATA_MODEL"))
		{
			m_rewardItemDataModel = widget.GetDataModel<EventDataModel>()?.Payload as RewardItemDataModel;
			if (m_rewardItemDataModel != null && m_rewardItemWidget != null)
			{
				m_rewardItemWidget.BindDataModel(m_rewardItemDataModel);
			}
		}
	}

	private void CreateRewardDetailObject()
	{
		if (m_rewardItemDataModel == null || m_rewardItemWidget == null)
		{
			return;
		}
		switch (m_rewardItemDataModel.ItemType)
		{
		case RewardItemType.MERCENARY:
		case RewardItemType.MERCENARY_EQUIPMENT:
		case RewardItemType.MERCENARY_EQUIPMENT_ICON:
			UpdateEnlargedRewardObject();
			break;
		case RewardItemType.MERCENARY_RANDOM_MERCENARY:
			if (m_rewardItemDataModel.MercenaryCoin != null)
			{
				string name2 = GameStrings.Get("GLUE_MERCENARY_RANDOM_COIN_TITLE");
				string description2 = GameStrings.Format("GLUE_MERCENARY_RANDOM_COIN_DESCRIPTION", m_rewardItemDataModel.MercenaryCoin.Quantity);
				CreateRewardTooltip(name2, description2);
			}
			else
			{
				_ = m_rewardItemDataModel.RandomMercenary;
			}
			break;
		case RewardItemType.BOOSTER:
		{
			string name3 = GameStrings.Get("GLUE_MERCENARY_PACK_TITLE");
			string description3 = GameStrings.Get("GLUE_MERCENARY_PACK_DESCRIPTION");
			CreateRewardTooltip(name3, description3);
			break;
		}
		case RewardItemType.MERCENARY_COIN:
		{
			string name = null;
			string description = null;
			if (m_rewardItemDataModel.MercenaryCoin != null && m_rewardItemDataModel.MercenaryCoin.MercenaryName != null)
			{
				name = GameStrings.Format("GLUE_MERCENARY_COIN_TITLE", m_rewardItemDataModel.MercenaryCoin.MercenaryName);
				description = GameStrings.Format("GLUE_MERCENARY_COIN_DESCRIPTION", m_rewardItemDataModel.MercenaryCoin.Quantity, m_rewardItemDataModel.MercenaryCoin.MercenaryName);
			}
			else if (m_rewardItemDataModel.Booster != null)
			{
				name = GameStrings.Get("GLUE_MERCENARY_PACK_TITLE");
				description = GameStrings.Get("GLUE_MERCENARY_PACK_DESCRIPTION");
			}
			else
			{
				name = GameStrings.Get("GLUE_MERCENARY_RANDOM_COIN_TITLE");
				description = GameStrings.Format("GLUE_MERCENARY_RANDOM_COIN_DESCRIPTION", m_rewardItemDataModel.MercenaryCoin.Quantity);
			}
			CreateRewardTooltip(name, description);
			break;
		}
		default:
			Debug.LogWarning("The reward item type is not supported to show detail by RewardItemDetailDisplay now.");
			break;
		}
	}

	private void UpdateEnlargedRewardObject()
	{
		if (!(m_enlargedRewardDetailWidget != null))
		{
			return;
		}
		RewardItemDataModel enlargedItemDataModel = m_enlargedRewardDetailWidget.GetDataModel<RewardItemDataModel>();
		if (enlargedItemDataModel != null && ((enlargedItemDataModel.ItemType == RewardItemType.MERCENARY && enlargedItemDataModel.Mercenary == m_rewardItemDataModel.Mercenary) || (enlargedItemDataModel.ItemType == RewardItemType.MERCENARY_EQUIPMENT && enlargedItemDataModel.MercenaryEquip == m_rewardItemDataModel.MercenaryEquip)))
		{
			m_widget.TriggerEvent("SHOW_REWARD_ENLARGED_DETAIL");
			return;
		}
		enlargedItemDataModel = m_rewardItemDataModel.CloneDataModel();
		if (enlargedItemDataModel.ItemType == RewardItemType.MERCENARY_EQUIPMENT_ICON)
		{
			enlargedItemDataModel.ItemType = RewardItemType.MERCENARY_EQUIPMENT;
		}
		m_widget.BindDataModel(enlargedItemDataModel);
	}

	private void CreateRewardTooltip(string titleName, string content)
	{
		DestroyRewardTooltip();
		TooltipPanel tooltipPanel = TooltipPanelManager.Get().CreateKeywordPanel(0);
		m_rewardToolTip = tooltipPanel.gameObject;
		Vector3 tipPosition = base.transform.position + m_tooltipPositionOffset;
		tooltipPanel.Reset();
		tooltipPanel.Initialize(titleName, content);
		tooltipPanel.SetScale(m_tooltipScale);
		tooltipPanel.transform.position = tipPosition;
		RenderUtils.SetAlpha(m_rewardToolTip, 0f);
		iTween.FadeTo(m_rewardToolTip, iTween.Hash("alpha", 1f, "time", 0.1f));
	}

	private void DestroyRewardTooltip()
	{
		if (m_rewardToolTip != null)
		{
			Object.Destroy(m_rewardToolTip);
			m_rewardToolTip = null;
		}
	}
}
