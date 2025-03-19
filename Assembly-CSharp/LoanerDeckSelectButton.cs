using Blizzard.T5.MaterialService.Extensions;
using Hearthstone.DataModels;
using Hearthstone.UI;
using UnityEngine;

public class LoanerDeckSelectButton : MonoBehaviour
{
	[HideInInspector]
	public DeckTemplateDbfRecord DeckTemplateRecord;

	[HideInInspector]
	public LoanerDeckDetailsController DeckDetailsController;

	[HideInInspector]
	public LoanerDecksInfoDataModel DataModel;

	public const string BUTTON_SELECTED = "Selected";

	[SerializeField]
	private GameObject m_portraitObject;

	[SerializeField]
	private int m_portraitMaterialIndex;

	[SerializeField]
	private UberText m_deckName;

	private const string ICON_TEXTURE_OVERRIDE_EVENT = "Default";

	private VisualController m_iconTextureController;

	private DefLoader.DisposableCardDef m_cardDef;

	public void OnDeckChoiceButtonClicked(string eventName)
	{
		if (!(eventName != "Selected") && DeckTemplateRecord != null && !(DeckDetailsController == null))
		{
			if (DataModel != null)
			{
				DataModel.DeckChoiceTemplateId = DeckTemplateRecord.ID;
				DeckDbfRecord deckRecord = GameDbf.Deck.GetRecord(DeckTemplateRecord.DeckId);
				DataModel.DeckChoiceName = deckRecord.Name;
				DataModel.DeckChoiceFlavourText = deckRecord.Description;
				DataModel.DeckChoiceClassName = GameStrings.GetClassName((TAG_CLASS)DeckTemplateRecord.ClassId);
			}
			DeckDetailsController.ShowDeckChoiceDetails(DeckTemplateRecord);
			DeckDetailsController.ShowDeckCardList(DeckTemplateRecord.DeckRecord);
			if (LoanerDeckDisplay.Get() != null)
			{
				LoanerDeckDisplay.Get().SetCurrentlySelectedDeckTemplate(DeckTemplateRecord);
			}
		}
	}

	public void SetDeckSelectButtonIcon(CollectionDeck deck)
	{
		if (deck.HeroCardID != null)
		{
			if (m_deckName != null)
			{
				m_deckName.Text = deck.Name;
			}
			DefLoader.Get().LoadFullDef(deck.GetDisplayHeroCardID(rerollFavoriteHero: false), OnHeroFullDefLoaded);
		}
	}

	public void SetDeckSelectButtonIcon(string heroCardId, string deckName)
	{
		if (!string.IsNullOrEmpty(heroCardId))
		{
			if (m_deckName != null)
			{
				m_deckName.Text = deckName;
			}
			DefLoader.Get().LoadFullDef(heroCardId, OnHeroFullDefLoaded);
		}
	}

	private void OnHeroFullDefLoaded(string cardID, DefLoader.DisposableFullDef def, object userData)
	{
		if (def == null || def.DisposableCardDef == null)
		{
			return;
		}
		m_cardDef?.Dispose();
		m_cardDef = def.DisposableCardDef.Share();
		def.Dispose();
		Material portraitMaterial = m_cardDef.CardDef.GetCustomDeckPortrait();
		if (portraitMaterial == null)
		{
			Log.CollectionDeckBox.PrintError("Custom Deck Portrait Material is null!");
			return;
		}
		if (m_portraitObject == null)
		{
			Log.CollectionDeckBox.PrintError("Custom Deck Portrait GameObject is null!");
			return;
		}
		Renderer portraitObjectRenderer = m_portraitObject.GetComponent<Renderer>();
		if (portraitObjectRenderer == null)
		{
			Log.CollectionDeckBox.PrintError("Custom Deck Portrait GameObject doesnt have a renderer!");
		}
		else
		{
			portraitObjectRenderer.SetSharedMaterial(m_portraitMaterialIndex, portraitMaterial);
		}
	}

	private void OnDestroy()
	{
		m_cardDef?.Dispose();
	}
}
