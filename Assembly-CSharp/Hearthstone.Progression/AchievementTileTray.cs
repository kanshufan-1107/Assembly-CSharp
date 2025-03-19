using Hearthstone.DataModels;
using Hearthstone.UI;
using UnityEngine;

namespace Hearthstone.Progression;

[RequireComponent(typeof(WidgetTemplate))]
public class AchievementTileTray : MonoBehaviour
{
	public Listable m_subcategoryButtonList;

	private Widget m_widget;

	private bool m_initialized;

	private AchievementSubcategoryListDataModel subcategoryButtonListDataModel = new AchievementSubcategoryListDataModel();

	private void Awake()
	{
		m_widget = GetComponent<WidgetTemplate>();
		m_widget.RegisterEventListener(HandleEvent);
		m_subcategoryButtonList.BindDataModel(subcategoryButtonListDataModel);
		AchievementManager.Get().OnSelectedCategoryChanged += UpdateSubcategoryList;
	}

	private void OnDestroy()
	{
		AchievementManager achievementManager = AchievementManager.Get();
		if (achievementManager != null)
		{
			achievementManager.OnSelectedCategoryChanged -= UpdateSubcategoryList;
		}
	}

	private void UpdateSubcategoryList(AchievementCategoryDataModel category)
	{
		subcategoryButtonListDataModel.Subcategories = category.Subcategories.Subcategories;
	}

	private void HandleEvent(string eventName)
	{
		if (!(eventName == "CODE_SELECT_CATEGORY"))
		{
			if (eventName == "CODE_SELECT_SUBCATEGORY")
			{
				HandleSelectSubcategory();
			}
		}
		else
		{
			HandleSelectCategory();
		}
	}

	private void HandleSelectSubcategory()
	{
		if (!(m_widget.GetDataModel<EventDataModel>()?.Payload is AchievementSubcategoryDataModel subcategory))
		{
			Debug.LogWarning("Unexpected state: no subcategory payload");
		}
		else
		{
			AchievementManager.Get().SelectSubcategory(subcategory);
		}
	}

	private void HandleSelectCategory()
	{
		if (!(m_widget.GetDataModel<EventDataModel>()?.Payload is AchievementCategoryDataModel category))
		{
			Debug.LogWarning("Unexpected state: no bound category");
			return;
		}
		m_widget.BindDataModel(category);
		if (!m_initialized)
		{
			category.SelectedSubcategory = null;
			m_initialized = true;
		}
		AchievementManager.Get().SelectCategory(category);
	}
}
