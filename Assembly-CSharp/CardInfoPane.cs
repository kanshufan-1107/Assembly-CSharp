using Assets;
using PegasusShared;
using UnityEngine;

public class CardInfoPane : MonoBehaviour
{
	public UberText m_artistName;

	public UberText m_rarityLabel;

	public UberText m_flavorText;

	public UberText m_setName;

	public GameObject m_standardTheming;

	public RarityGem m_rarityGem;

	public GameObject m_wildTheming;

	public RarityGem m_wildRarityGem;

	public GameObject m_twistTheming;

	public RarityGem m_classicRarityGem;

	public void UpdateContent()
	{
		if (!CraftingManager.Get().GetShownCardInfo(out var entityDef, out var premium))
		{
			return;
		}
		TAG_RARITY rarity = entityDef.GetRarity();
		TAG_CARD_SET cardSet = entityDef.GetCardSet();
		if (GameUtils.IsZilliaxModule(entityDef))
		{
			rarity = TAG_RARITY.FREE;
		}
		if (GameUtils.IsLegacySet(cardSet))
		{
			cardSet = TAG_CARD_SET.LEGACY;
		}
		if (rarity == TAG_RARITY.FREE)
		{
			m_rarityLabel.Text = "";
		}
		else
		{
			m_rarityLabel.Text = GameStrings.GetRarityText(rarity);
		}
		AssignRarityColors(rarity, cardSet);
		FormatType formatType = CollectionManager.Get().GetThemeShowing();
		m_wildTheming.SetActive(formatType == FormatType.FT_WILD);
		m_standardTheming.SetActive(formatType == FormatType.FT_STANDARD);
		m_twistTheming.SetActive(formatType == FormatType.FT_TWIST);
		switch (formatType)
		{
		case FormatType.FT_STANDARD:
			m_rarityGem.SetRarityGem(rarity, cardSet, premium);
			break;
		case FormatType.FT_WILD:
			m_wildRarityGem.SetRarityGem(rarity, cardSet, premium);
			break;
		case FormatType.FT_CLASSIC:
			m_classicRarityGem.SetRarityGem(rarity, cardSet, premium);
			break;
		case FormatType.FT_TWIST:
			m_classicRarityGem.SetRarityGem(rarity, cardSet, premium);
			break;
		}
		m_setName.Text = GameStrings.GetCardSetName(cardSet);
		m_artistName.Text = GameStrings.Format("GLUE_COLLECTION_ARTIST", entityDef.GetArtistName(premium));
		m_wildTheming.SetActive(formatType == FormatType.FT_WILD);
		string cardFlavorTextEntry = (GameUtils.IsSavedZilliaxVersion(entityDef) ? ((string)GameDbf.GetIndex().GetCardRecord("TOY_330").FlavorText) : entityDef.GetFlavorText());
		string flavorText = "<color=#000000ff>" + cardFlavorTextEntry + "</color>";
		CardValueDbfRecord cardValueDbfRecord;
		NetCache.CardValue cardValue = CraftingManager.GetCardValue(entityDef.GetCardId(), premium, out cardValueDbfRecord);
		if (cardValue != null && cardValue.IsOverrideActive() && CraftingUI.IsCraftingEventForCardActive(entityDef.GetCardId(), premium, out var _))
		{
			if (!string.IsNullOrEmpty(flavorText))
			{
				flavorText += "\n\n";
			}
			flavorText = ((cardValueDbfRecord == null || cardValueDbfRecord.SellState != CardValue.SellState.PERMANENT_OVERRIDE_USE_CUSTOM_VALUE) ? (flavorText + GameStrings.Get("GLUE_COLLECTION_RECENTLY_NERFED")) : (flavorText + GameStrings.Get("GLUE_COLLECTION_PERMANENTLY_OVERRIDDEN_DISENCHANT")));
		}
		m_flavorText.Text = flavorText;
	}

	private void AssignRarityColors(TAG_RARITY rarity, TAG_CARD_SET cardSet)
	{
		switch (rarity)
		{
		default:
			m_rarityLabel.TextColor = Color.white;
			break;
		case TAG_RARITY.RARE:
			m_rarityLabel.TextColor = new Color(0.11f, 0.33f, 0.8f, 1f);
			break;
		case TAG_RARITY.EPIC:
			m_rarityLabel.TextColor = new Color(0.77f, 0.03f, 1f, 1f);
			break;
		case TAG_RARITY.LEGENDARY:
			m_rarityLabel.TextColor = new Color(1f, 0.56f, 0f, 1f);
			break;
		}
	}
}
