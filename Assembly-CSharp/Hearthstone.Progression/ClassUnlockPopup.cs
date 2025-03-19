using System.Linq;
using Hearthstone.DataModels;
using Hearthstone.UI;
using UnityEngine;

namespace Hearthstone.Progression;

public class ClassUnlockPopup : MonoBehaviour
{
	private const string OPEN = "OPEN";

	[SerializeField]
	private Widget m_deckPouchWidget;

	[SerializeField]
	private Widget m_heroPreviewPortraitWidget;

	private WidgetTemplate m_widget;

	private TAG_CLASS m_tagClass;

	private ClassPreviewDataModel m_classPreviewDataModel;

	private ProfileClassIconDataModel m_profileClassIconDataModel;

	private bool m_ownsDataModel;

	public static ClassPreviewDataModel BuildClassPreviewDataModel(TAG_CLASS tagClass, bool showClassWins, string heroCardIdOverride = null)
	{
		ClassDbfRecord classRecord = GameDbf.Class.GetRecord((int)tagClass);
		if (classRecord == null)
		{
			Log.All.PrintError("[ClassUnlockPopup] Unable to find class record!");
			return null;
		}
		if (classRecord.DefaultHeroCardRecord == null)
		{
			Log.All.PrintError("[ClassUnlockPopup] No default Hero Card!");
			return null;
		}
		string heroCardId = (string.IsNullOrEmpty(heroCardIdOverride) ? GameUtils.TranslateDbIdToCardId(classRecord.DefaultHeroCardRecord.ID) : heroCardIdOverride);
		string heroPowerCardId = GameUtils.GetHeroPowerCardIdFromHero(heroCardId);
		HeroDataModel heroDM = new HeroDataModel();
		heroDM.Name = classRecord.DefaultHeroCardRecord.Name.GetString();
		heroDM.HeroCard = new CardDataModel
		{
			CardId = heroCardId
		};
		heroDM.HeroPowerCard = new CardDataModel
		{
			CardId = heroPowerCardId
		};
		return new ClassPreviewDataModel
		{
			TagClass = tagClass,
			Name = GameStrings.GetClassName(tagClass),
			Description = classRecord.PreviewDesc.GetString(),
			Strengths = classRecord.StrengthsDesc.GetString(),
			Weaknesses = classRecord.WeaknessesDesc.GetString(),
			Hero = heroDM,
			ShowClassWins = showClassWins
		};
	}

	private void Awake()
	{
		m_widget = GetComponent<WidgetTemplate>();
		if (m_widget == null)
		{
			Log.All.PrintError("[ClassPreviewPopup] Reference to widget not found!");
			return;
		}
		m_widget.RegisterEventListener(EventHandler);
		m_deckPouchWidget.RegisterReadyListener(OnPouchReady);
	}

	private void OnDisable()
	{
		Reset();
	}

	private void Reset()
	{
		if (m_heroPreviewPortraitWidget != null)
		{
			m_heroPreviewPortraitWidget.UnbindDataModel(111);
		}
		if (m_classPreviewDataModel != null && m_ownsDataModel)
		{
			m_widget.UnbindDataModel(m_classPreviewDataModel.DataModelId);
		}
		m_ownsDataModel = false;
		m_classPreviewDataModel = null;
		m_tagClass = TAG_CLASS.INVALID;
	}

	private void EventHandler(string eventName)
	{
		if (!(eventName == "OPEN"))
		{
			return;
		}
		ClassPreviewDataModel classPreviewDM = m_widget.GetDataModel<ClassPreviewDataModel>();
		if (classPreviewDM != null)
		{
			m_classPreviewDataModel = classPreviewDM;
			m_ownsDataModel = false;
			m_tagClass = classPreviewDM.TagClass;
			ProfileClassIconDataModel classIconDataModel = ProfilePageClassProgress.BuildClassIconDataModel(m_tagClass);
			if (classIconDataModel != null)
			{
				m_widget.BindDataModel(classIconDataModel);
				m_profileClassIconDataModel = classIconDataModel;
			}
		}
		else
		{
			InitializeClassDataFromReward();
		}
		if (m_heroPreviewPortraitWidget != null)
		{
			m_heroPreviewPortraitWidget.BindDataModel(m_classPreviewDataModel.Hero);
		}
		UpdatePouch();
	}

	private void OnPouchReady(object unused)
	{
		UpdatePouch();
	}

	private void UpdatePouch()
	{
		if (m_tagClass == TAG_CLASS.INVALID)
		{
			m_tagClass = (TAG_CLASS)GetHeroClassIdFromReward();
		}
		DeckTemplateDbfRecord deckTemplateRecord = GameDbf.DeckTemplate.GetRecord((DeckTemplateDbfRecord record) => record.IsStarterDeck && record.ClassId == (int)m_tagClass && EventTimingManager.Get().IsEventActive(record.Event));
		if (deckTemplateRecord != null)
		{
			DeckPouchDataModel deckDataModel = ShopDeckPouchDisplay.CreateDeckPouchDataModelFromDeckTemplate(deckTemplateRecord, null);
			if (deckDataModel != null)
			{
				m_deckPouchWidget.BindDataModel(deckDataModel);
			}
		}
	}

	private void InitializeClassDataFromReward()
	{
		ClassPreviewDataModel classPreviewDM = BuildClassPreviewDataModel((TAG_CLASS)GetHeroClassIdFromReward(), GameUtils.HasCompletedApprentice());
		m_ownsDataModel = true;
		if (classPreviewDM == null)
		{
			return;
		}
		if (m_widget == null)
		{
			m_widget = GetComponent<WidgetTemplate>();
			if (m_widget == null)
			{
				Log.All.PrintError("[ClassPreviewPopup] Reference to widget not found!");
				return;
			}
		}
		m_classPreviewDataModel = classPreviewDM;
		m_widget.BindDataModel(m_classPreviewDataModel);
	}

	private int GetHeroClassIdFromReward()
	{
		if (!m_widget.GetDataModel(34, out var dm) || !(dm is RewardListDataModel rewardListDM))
		{
			Log.All.PrintError("[ClassPreviewPopup] Hero unlock reward missing!");
			return -1;
		}
		RewardItemDataModel heroItemDM = rewardListDM.Items.FirstOrDefault((RewardItemDataModel item) => item.ItemType == RewardItemType.HERO_CLASS);
		if (heroItemDM == null)
		{
			Log.All.PrintError("[ClassPreviewPopup] Hero unlock reward missing!");
			return -1;
		}
		return heroItemDM.HeroClassId;
	}
}
