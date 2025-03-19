using System.Collections.Generic;
using Hearthstone.DataModels;
using Hearthstone.UI;
using UnityEngine;

public class ShopClassVariantSelector : MonoBehaviour
{
	public AsyncReference m_chooseDeckReference;

	public AsyncReference[] m_classButtonReferences;

	private DeckChoiceDataModel m_deckChoiceDataModel;

	private DeckChoiceDataModel[] m_buttonDataModels;

	private Widget[] m_classButtonWidgets;

	private Widget m_chooseDeckWidget;

	private ProductPage m_productPage;

	private List<DeckTemplateDbfRecord> m_deckTemplates;

	private TAG_CLASS[] m_classByButtonIndex = new TAG_CLASS[11]
	{
		TAG_CLASS.DRUID,
		TAG_CLASS.HUNTER,
		TAG_CLASS.MAGE,
		TAG_CLASS.PALADIN,
		TAG_CLASS.PRIEST,
		TAG_CLASS.ROGUE,
		TAG_CLASS.SHAMAN,
		TAG_CLASS.WARLOCK,
		TAG_CLASS.WARRIOR,
		TAG_CLASS.DEMONHUNTER,
		TAG_CLASS.DEATHKNIGHT
	};

	protected virtual void Start()
	{
		m_classButtonWidgets = new Widget[m_classButtonReferences.Length];
		m_buttonDataModels = new DeckChoiceDataModel[m_classButtonReferences.Length];
		for (int i = 0; i < m_classButtonReferences.Length; i++)
		{
			int classIndex = i;
			m_classButtonReferences[classIndex].RegisterReadyListener(delegate(Widget w)
			{
				SetupDataModelForButton(w, classIndex);
			});
		}
		m_deckChoiceDataModel = new DeckChoiceDataModel();
		m_chooseDeckReference.RegisterReadyListener(delegate(Widget w)
		{
			m_chooseDeckWidget = w;
			w.BindDataModel(m_deckChoiceDataModel);
		});
	}

	public void SetProductPage(ProductPage productPage)
	{
		m_productPage = productPage;
	}

	public void SetProduct(ProductDataModel product)
	{
		DataModelList<ProductDataModel> productVariants = product.Variants;
		int numVariants = productVariants.Count;
		if (numVariants <= m_classButtonWidgets.Length)
		{
			for (int variantIndex = 0; variantIndex < numVariants; variantIndex++)
			{
				SetupDataModelForButton(variantIndex, productVariants[variantIndex]);
			}
		}
	}

	public void SetSelectedButtonIndex(int index)
	{
		m_deckChoiceDataModel = m_classButtonWidgets[index].GetDataModel<DeckChoiceDataModel>();
		if (m_productPage != null)
		{
			m_productPage.SelectVariantByIndex(index);
		}
	}

	private void SetupDataModelForButton(Widget w, int index)
	{
		string className = m_classByButtonIndex[index].ToString();
		DeckChoiceDataModel dataModel = new DeckChoiceDataModel();
		dataModel.ButtonClass = className;
		m_classButtonWidgets[index] = w;
		m_buttonDataModels[index] = dataModel;
		w.BindDataModel(dataModel);
	}

	private void SetupDataModelForButton(int index, ProductDataModel productVariant)
	{
		DeckTemplateDbfRecord templateRecord = GetDeckTemplateRecordForProduct(productVariant);
		if (templateRecord != null)
		{
			DeckChoiceDataModel dataModel = new DeckChoiceDataModel();
			int deckClassId = (dataModel.ChoiceClassID = templateRecord.ClassId);
			TAG_CLASS classEnum = (TAG_CLASS)deckClassId;
			dataModel.ButtonClass = classEnum.ToString();
			m_buttonDataModels[index] = dataModel;
			m_classButtonWidgets[index].BindDataModel(dataModel);
			m_classButtonWidgets[index].TriggerEvent("Default");
		}
	}

	private DeckTemplateDbfRecord GetDeckTemplateRecordForProduct(ProductDataModel productVariant)
	{
		if (productVariant.Items.Count < 1 || productVariant.Items[0].ItemType != RewardItemType.SELLABLE_DECK)
		{
			Log.Store.PrintWarning("[ShopClassVariantSelector.OnProductSet] Failed to find variant item!");
			return null;
		}
		int rewardId = productVariant.Items[0].ItemId;
		SellableDeckDbfRecord rewardRecord = GameDbf.SellableDeck.GetRecord(rewardId);
		if (rewardRecord == null)
		{
			Log.Store.PrintWarning("[ShopClassVariantSelector.OnProductSet] Failed to find DB record {0}!", rewardId);
			return null;
		}
		if (rewardRecord.DeckTemplateRecord == null || rewardRecord.DeckTemplateRecord.DeckRecord == null)
		{
			Log.Store.PrintWarning("[ShopClassVariantSelector.OnProductSet] The DB record {0} does NOT have a deck template with a valid deck record!", rewardRecord.ID);
			return null;
		}
		return rewardRecord.DeckTemplateRecord;
	}
}
