using System;
using System.Collections.Generic;
using Hearthstone.DataModels;
using Hearthstone.UI;
using UnityEngine;

namespace Hearthstone.Progression;

[RequireComponent(typeof(WidgetTemplate))]
public class ProfilePageClassProgress : MonoBehaviour
{
	[Serializable]
	public struct ClassIconList
	{
		public Widget m_listWidget;

		public int m_maxClassIcons;
	}

	[SerializeField]
	[Header("Each element defines max number of icons per row. Size is number of rows")]
	private List<ClassIconList> m_classIconLists;

	private const string CLASS_ICON_CLICKED = "CLASS_ICON_CLICKED";

	private const string SHOW_CLASS_POPUP = "CODE_SHOW_CLASS_POPUP";

	private Widget m_widget;

	public static ProfileClassIconDataModel BuildClassIconDataModel(TAG_CLASS tagClass)
	{
		ProfileClassIconDataModel classIconDataModel = new ProfileClassIconDataModel();
		classIconDataModel.Name = GameStrings.GetClassName(tagClass);
		classIconDataModel.TagClass = tagClass;
		classIconDataModel.IsUnlocked = GameUtils.HasUnlockedClass(tagClass);
		if (!GameUtils.HERO_SKIN_ACHIEVEMENTS.TryGetValue(tagClass, out var heroSkinAchievements))
		{
			return null;
		}
		AchievementDataModel golden500WinAchievement = AchievementManager.Get().GetAchievementDataModel(heroSkinAchievements.Golden500Win);
		AchievementDataModel honored1KWinAchievement = AchievementManager.Get().GetAchievementDataModel(heroSkinAchievements.Honored1kWin);
		classIconDataModel.IsGolden = AchievementManager.Get().IsAchievementComplete(golden500WinAchievement.ID);
		classIconDataModel.GoldWinsReq = golden500WinAchievement.Quota;
		classIconDataModel.IsPremium = AchievementManager.Get().IsAchievementComplete(honored1KWinAchievement.ID);
		classIconDataModel.PremiumWinsReq = honored1KWinAchievement.Quota;
		NetCache.NetCacheHeroLevels heroLevels = NetCache.Get().GetNetObject<NetCache.NetCacheHeroLevels>();
		if (heroLevels == null)
		{
			return null;
		}
		NetCache.HeroLevel heroLevel = heroLevels.Levels.Find((NetCache.HeroLevel o) => o.Class == tagClass);
		if (classIconDataModel.IsUnlocked)
		{
			classIconDataModel.CurrentLevel = heroLevel.CurrentLevel.Level;
			classIconDataModel.MaxLevel = heroLevel.CurrentLevel.MaxLevel;
			classIconDataModel.CurrentLevelXP = heroLevel.CurrentLevel.XP;
			classIconDataModel.CurrentLevelXPMax = heroLevel.CurrentLevel.MaxXP;
			classIconDataModel.IsMaxLevel = heroLevel.CurrentLevel.IsMaxLevel();
		}
		else
		{
			classIconDataModel.CurrentLevel = 0;
			classIconDataModel.MaxLevel = 0;
			classIconDataModel.CurrentLevelXP = 0L;
			classIconDataModel.CurrentLevelXPMax = 1L;
			classIconDataModel.IsMaxLevel = false;
		}
		classIconDataModel.Wins = (classIconDataModel.IsGolden ? honored1KWinAchievement.Progress : golden500WinAchievement.Progress);
		classIconDataModel.WinsText = GameStrings.Format("GLOBAL_PROGRESSION_PROFILE_ARENA_WINS", classIconDataModel.Wins);
		int totalLevel = GameUtils.GetTotalHeroLevel().GetValueOrDefault();
		string nextRewardTooltipTitle;
		string nextRewardTooltipDesc;
		if (!classIconDataModel.IsUnlocked)
		{
			string heroName = "";
			string heroCardID = CollectionManager.GetVanillaHero(tagClass);
			if (!string.IsNullOrEmpty(heroCardID))
			{
				DefLoader.DisposableFullDef heroCardDef = DefLoader.Get().GetFullDef(heroCardID);
				if (heroCardDef?.CardDef != null)
				{
					heroName = heroCardDef.EntityDef.GetShortName();
				}
			}
			classIconDataModel.TooltipTitle = classIconDataModel.Name;
			if (classIconDataModel.TagClass == TAG_CLASS.DEATHKNIGHT)
			{
				classIconDataModel.TooltipDesc = GameStrings.Format("GLUE_HERO_LOCKED_PROLOGUE_DESC", classIconDataModel.Name);
			}
			else
			{
				classIconDataModel.TooltipDesc = GameStrings.Format("GLUE_HERO_LOCKED_DESC", heroName, classIconDataModel.Name);
			}
		}
		else if (RewardUtils.GetNextHeroLevelRewardText(tagClass, heroLevel.CurrentLevel.Level, totalLevel, out nextRewardTooltipTitle, out nextRewardTooltipDesc))
		{
			classIconDataModel.TooltipTitle = nextRewardTooltipTitle;
			classIconDataModel.TooltipDesc = nextRewardTooltipDesc;
		}
		else if (heroLevel.CurrentLevel.IsMaxLevel())
		{
			classIconDataModel.TooltipTitle = GameStrings.Format("GLOBAL_PROGRESSION_TOOLTIP_TOTAL_CLASS_WINS", classIconDataModel.Name);
			if (classIconDataModel.IsPremium)
			{
				classIconDataModel.TooltipDesc = GameStrings.Format("GLOBAL_PROGRESSION_TOOLTIP_PREMIUM_WINS_DONE_DESC", classIconDataModel.PremiumWinsReq, classIconDataModel.Name, honored1KWinAchievement.Name);
			}
			else if (classIconDataModel.IsGolden)
			{
				classIconDataModel.TooltipDesc = GameStrings.Format("GLOBAL_PROGRESSION_TOOLTIP_PREMIUM_WINS_DESC", classIconDataModel.PremiumWinsReq, classIconDataModel.Name, honored1KWinAchievement.Name);
			}
			else
			{
				classIconDataModel.TooltipDesc = GameStrings.Format("GLOBAL_PROGRESSION_TOOLTIP_GOLDEN_WINS_DESC", classIconDataModel.GoldWinsReq, classIconDataModel.Name);
			}
		}
		else
		{
			classIconDataModel.TooltipTitle = classIconDataModel.Name;
			classIconDataModel.TooltipDesc = GameStrings.Format("GLOBAL_PROGRESSION_TOOLTIP_CLASS_DEFAULT_DESC", classIconDataModel.CurrentLevel);
		}
		return classIconDataModel;
	}

	private void Awake()
	{
		m_widget = GetComponent<WidgetTemplate>();
		m_widget.RegisterEventListener(delegate(string evt)
		{
			if (evt == "CLASS_ICON_CLICKED")
			{
				EventDataModel dataModel = m_widget.GetDataModel<EventDataModel>();
				if (dataModel != null && dataModel.Payload is TAG_CLASS tagClass)
				{
					ClassPreviewDataModel payload = ClassUnlockPopup.BuildClassPreviewDataModel(tagClass, showClassWins: true);
					EventDataModel eventData = new EventDataModel
					{
						SourceName = ToString(),
						Payload = payload
					};
					SendEventUpwardStateAction.SendEventUpward(base.gameObject, "CODE_SHOW_CLASS_POPUP", eventData);
				}
			}
		});
		UpdateClassIcons();
	}

	private void UpdateClassIcons()
	{
		if (NetCache.Get().GetNetObject<NetCache.NetCacheHeroLevels>() == null)
		{
			return;
		}
		GameUtils.GetTotalHeroLevel().GetValueOrDefault();
		int classIndex = 0;
		foreach (ClassIconList classIconList in m_classIconLists)
		{
			ProfileClassIconListDataModel classIconListDataModel = new ProfileClassIconListDataModel();
			int i = classIndex;
			int rowIndex = 0;
			for (; i < CollectionPageManager.CLASS_TAB_ORDER.Length; i++)
			{
				if (rowIndex >= classIconList.m_maxClassIcons)
				{
					break;
				}
				TAG_CLASS tagClass = CollectionPageManager.CLASS_TAB_ORDER[i];
				if (tagClass != TAG_CLASS.NEUTRAL)
				{
					ProfileClassIconDataModel classIconDataModel = BuildClassIconDataModel(tagClass);
					if (classIconDataModel == null)
					{
						break;
					}
					classIconListDataModel.Icons.Add(classIconDataModel);
					classIconList.m_listWidget.BindDataModel(classIconListDataModel);
					classIndex = i + 1;
					rowIndex++;
				}
			}
		}
	}
}
