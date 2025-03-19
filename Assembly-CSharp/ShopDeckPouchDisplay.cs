using System.Collections.Generic;
using Blizzard.T5.AssetManager;
using Hearthstone.DataModels;
using Hearthstone.UI;
using UnityEngine;

public class ShopDeckPouchDisplay : MonoBehaviour
{
	public enum DKRuneTypes
	{
		Blood = 1,
		Frost,
		Unholy
	}

	public Widget deckWidget;

	public RewardItemDataModel m_rewardItemDataModel;

	public Material portraitMaterial;

	private Material m_temporaryPortraitMaterial;

	private AssetHandle<Texture> m_portraitTextureHandle;

	private AssetHandle<Material> m_portraitMaterialHandle;

	private int m_currentDisplayCardId;

	public void OnDestroy()
	{
		SafelyDisposeTempPortrait();
		AssetHandle.SafeDispose(ref m_portraitMaterialHandle);
		AssetHandle.SafeDispose(ref m_portraitTextureHandle);
	}

	private void SafelyDisposeTempPortrait()
	{
		if (m_temporaryPortraitMaterial != null)
		{
			Object.Destroy(m_temporaryPortraitMaterial);
			m_temporaryPortraitMaterial = null;
		}
	}

	public void SetRewardItem()
	{
		if (deckWidget == null)
		{
			Log.Store.PrintWarning("[ShopDeckPouchDisplay.SetRewardItem] DeckWidget reference is null!");
		}
		else
		{
			deckWidget.RegisterDoneChangingStatesListener(SetupRewardOnWidgetReady);
		}
	}

	private void SetupRewardOnWidgetReady(object _)
	{
		deckWidget.RemoveDoneChangingStatesListener(SetupRewardOnWidgetReady);
		SetupRewardFromWidget();
	}

	private void SetupRewardFromWidget()
	{
		m_rewardItemDataModel = deckWidget.GetDataModel<RewardItemDataModel>();
		RewardItemDataModel rewardItemDataModel = m_rewardItemDataModel;
		if (rewardItemDataModel != null && rewardItemDataModel.ItemType == RewardItemType.SELLABLE_DECK)
		{
			int rewardId = m_rewardItemDataModel.ItemId;
			SellableDeckDbfRecord rewardRecord = GameDbf.SellableDeck.GetRecord(rewardId);
			if (rewardRecord == null)
			{
				Log.Store.PrintWarning("[ShopDeckPouchDisplay.SetRewardItem] Failed to find sellable deck DB record {0}!", rewardId);
			}
			else if (rewardRecord.DeckTemplateRecord == null || rewardRecord.DeckTemplateRecord.DeckRecord == null)
			{
				Log.Store.PrintWarning("[ShopDeckPouchDisplay.SetRewardItem] The DB record {0} does NOT have a deck template with a valid deck record!", rewardRecord.ID);
			}
			else
			{
				SetDeckPouchData(deckWidget, rewardRecord.DeckTemplateRecord);
			}
		}
	}

	public void SetDeckPouchData(Widget widget, DeckTemplateDbfRecord record)
	{
		if (widget == null)
		{
			Log.Store.PrintWarning("[ShopDeckPouchDisplay.SetDeckPouchData] Deck widget is null!");
			return;
		}
		Material portrait = GetPortraitMaterialFromDeckTemplateRecord(record);
		DeckPouchDataModel deckDataModel = CreateDeckPouchDataModelFromDeckTemplate(record, portrait);
		widget.BindDataModel(deckDataModel);
	}

	public static DeckPouchDataModel CreateDeckPouchDataModelFromDeckTemplate(DeckTemplateDbfRecord record, Material portraitMaterial)
	{
		DeckDbfRecord deckRecord = record?.DeckRecord;
		List<DeckCardDbfRecord> cards = deckRecord?.Cards;
		if (cards == null)
		{
			return new DeckPouchDataModel();
		}
		List<DKRuneTypes> dkRunes = GetDKRunesOrNullIfNone(record);
		int[] rarityCounts = GetRarityCounts(cards);
		DeckPouchDataModel deckPouchDataModel = new DeckPouchDataModel();
		deckPouchDataModel.DeckTemplateId = record.ID;
		deckPouchDataModel.Pouch = new AdventureLoadoutOptionDataModel
		{
			Name = deckRecord.Name,
			DisplayTexture = portraitMaterial,
			DisplayColor = CollectionPageManager.ColorForClass((TAG_CLASS)record.ClassId)
		};
		deckPouchDataModel.Details = new DeckDetailsDataModel
		{
			Product = new ProductDataModel
			{
				DescriptionHeader = GameStrings.Format("GLUE_COLLECTION_NEW_DECK_DETAIL_HEADER", GameStrings.GetClassName((TAG_CLASS)record.ClassId)),
				Description = GameStrings.Format("GLUE_COLLECTION_NEW_DECK_DETAIL_DESC", rarityCounts[5], rarityCounts[4], rarityCounts[3], rarityCounts[1])
			},
			AltDescription = deckRecord.AltDescription
		};
		deckPouchDataModel.Class = (TAG_CLASS)record.ClassId;
		DeckPouchDataModel result = deckPouchDataModel;
		if (dkRunes != null)
		{
			result.Pouch.DKRunes.AddRange(dkRunes);
		}
		return result;
	}

	public static List<DKRuneTypes> GetDKRunesOrNullIfNone(DeckTemplateDbfRecord record)
	{
		if (record.ClassId != 1 || record.DKRunes.Count == 0)
		{
			return null;
		}
		List<DKRuneTypes> result = new List<DKRuneTypes>();
		foreach (DkRuneListDbfRecord rune in record.DKRunes)
		{
			result.Add((DKRuneTypes)rune.Rune);
		}
		return result;
	}

	private Material GetPortraitMaterialFromDeckTemplateRecord(DeckTemplateDbfRecord record)
	{
		if (portraitMaterial != null && record.DisplayCardId != 0)
		{
			using (DefLoader.DisposableCardDef displayCard = DefLoader.Get().GetCardDef(record.DisplayCardId))
			{
				if (m_temporaryPortraitMaterial == null || m_currentDisplayCardId != record.DisplayCardId)
				{
					SafelyDisposeTempPortrait();
					m_temporaryPortraitMaterial = new Material(portraitMaterial);
				}
				AssetHandle.Set(ref m_portraitTextureHandle, displayCard?.CardDef.GetPortraitTextureHandle());
				m_temporaryPortraitMaterial.mainTexture = m_portraitTextureHandle;
				m_currentDisplayCardId = record.DisplayCardId;
			}
			return m_temporaryPortraitMaterial;
		}
		AssetLoader.Get().LoadAsset(ref m_portraitMaterialHandle, record.DisplayTexture);
		return m_portraitMaterialHandle;
	}

	public static int[] GetRarityCounts(List<DeckCardDbfRecord> cards)
	{
		DefLoader defLoader = DefLoader.Get();
		int[] rarityCounts = new int[6];
		cards.ForEach(delegate(DeckCardDbfRecord r)
		{
			rarityCounts[(int)(defLoader.GetEntityDef(r.CardId)?.GetRarity() ?? TAG_RARITY.INVALID)]++;
		});
		return rarityCounts;
	}
}
