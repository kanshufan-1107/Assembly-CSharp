using Hearthstone.DataModels;
using Hearthstone.Streaming;
using Hearthstone.UI;
using UnityEngine;

namespace Hearthstone.Progression;

[RequireComponent(typeof(WidgetTemplate))]
public class AchievementSubCategoryButton : MonoBehaviour, IWidgetEventListener
{
	private WidgetTemplate m_widgetTemplate;

	[SerializeField]
	private TooltipZone m_tooltipZone;

	public WidgetTemplate OwningWidget => m_widgetTemplate;

	private void Awake()
	{
		m_widgetTemplate = GetComponent<WidgetTemplate>();
	}

	private void OnLockedMouseOver()
	{
		AchievementSubcategoryDataModel dataModel = m_widgetTemplate.GetDataModel<AchievementSubcategoryDataModel>();
		if (dataModel != null)
		{
			ShowTooltip(dataModel);
		}
	}

	private void ShowTooltip(AchievementSubcategoryDataModel subcategory)
	{
		string moduleName = DownloadUtils.GetGameModeName(AchievementManager.Get().GetContentTagFromAchievementSubcategory(subcategory.ID));
		string headline = GameStrings.Format("GLOBAL_PROGRESSION_ACHIEVEMENT_SUBCATEGORY_LOCKED_TOOLTIP_TITLE", moduleName);
		string bodyText = GameStrings.Format("GLOBAL_PROGRESSION_ACHIEVEMENT_SUBCATEGORY_LOCKED_TOOLTIP", moduleName);
		m_tooltipZone.ShowBoxTooltip(headline, bodyText);
		m_tooltipZone.Scale = m_tooltipZone.Scale;
	}

	private void OnLockedMouseOut()
	{
		m_tooltipZone.HideTooltip();
	}

	public WidgetEventListenerResponse EventReceived(string eventName, TriggerEventParameters eventParams)
	{
		bool consumed = true;
		if (!(eventName == "ON_LOCKED_MOUSE_OVER"))
		{
			if (eventName == "ON_LOCKED_MOUSE_OUT")
			{
				OnLockedMouseOut();
			}
			else
			{
				consumed = false;
			}
		}
		else
		{
			OnLockedMouseOver();
		}
		WidgetEventListenerResponse result = default(WidgetEventListenerResponse);
		result.Consumed = consumed;
		return result;
	}
}
