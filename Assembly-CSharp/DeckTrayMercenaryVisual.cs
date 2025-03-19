using Blizzard.T5.MaterialService.Extensions;
using Blizzard.T5.Services;
using Hearthstone.UI.Core;
using UnityEngine;

public class DeckTrayMercenaryVisual : MonoBehaviour
{
	public enum CardDefMaterialType
	{
		CustomDeck,
		DeckCardBar
	}

	public GameObject m_portraitObject;

	public int m_portraitMaterialIndex;

	public CardDefMaterialType m_materialType;

	public Renderer m_Frame;

	public Material m_NormalFrameMaterial;

	public Material m_GoldenFrameMaterial;

	public Material m_DiamondFrameMaterial;

	private LettuceMercenary.ArtVariation m_artVariation;

	private DefLoader.DisposableCardDef m_disposableDef;

	private string m_cardId;

	private bool m_isCardPremiumTagSet;

	private TAG_PREMIUM m_cardPremiumTag;

	private string m_desiredCardID;

	private bool m_isDesiredCardPremiumTagSet;

	private TAG_PREMIUM m_desiredCardPremiumTag;

	[Overridable]
	public int TeamId
	{
		set
		{
			LettuceMercenary leader = null;
			LettuceMercenary.Loadout loadout = null;
			LettuceTeam team = CollectionManager.Get().GetTeam(value);
			if (team != null)
			{
				leader = team.GetLeader();
				if (leader != null)
				{
					loadout = team.GetLoadout(leader);
				}
			}
			UpdateVisuals(leader, loadout);
		}
	}

	[Overridable]
	public int MercenaryId
	{
		set
		{
			LettuceMercenary merc = CollectionManager.Get().GetMercenary(value);
			UpdateVisuals(merc, merc?.GetCurrentLoadout());
		}
	}

	[Overridable]
	public string CardId
	{
		set
		{
			if (m_isDesiredCardPremiumTagSet)
			{
				UpdateVisuals(value, m_desiredCardPremiumTag);
				m_desiredCardID = null;
				m_isDesiredCardPremiumTagSet = false;
			}
			else
			{
				m_desiredCardID = value;
			}
		}
	}

	[Overridable]
	public TAG_PREMIUM CardPremium
	{
		set
		{
			if (m_desiredCardID != null)
			{
				UpdateVisuals(m_desiredCardID, value);
				m_desiredCardID = null;
				m_isDesiredCardPremiumTagSet = false;
			}
			else
			{
				m_isDesiredCardPremiumTagSet = true;
				m_desiredCardPremiumTag = value;
			}
		}
	}

	private void OnDestroy()
	{
		m_disposableDef?.Dispose();
		m_disposableDef = null;
	}

	private Material GetMaterial(TAG_PREMIUM premium)
	{
		return m_materialType switch
		{
			CardDefMaterialType.CustomDeck => m_disposableDef?.CardDef?.GetCustomDeckPortrait(), 
			CardDefMaterialType.DeckCardBar => m_disposableDef?.CardDef?.GetDeckCardBarPortrait(premium), 
			_ => null, 
		};
	}

	private void UpdateVisuals(string cardId, TAG_PREMIUM premium)
	{
		m_artVariation = null;
		if (!(m_cardId == cardId) || !m_isCardPremiumTagSet || m_cardPremiumTag != premium)
		{
			m_cardId = cardId;
			m_cardPremiumTag = premium;
			m_isCardPremiumTagSet = true;
			m_disposableDef?.Dispose();
			m_disposableDef = DefLoader.Get()?.GetCardDef(cardId, premium);
			if (m_disposableDef?.CardDef == null)
			{
				Log.Lettuce.PrintError("Card Def is null");
			}
			else
			{
				UpdatePortraiteMaterial(m_cardPremiumTag);
			}
		}
	}

	private void UpdateVisuals(LettuceMercenary mercenary, LettuceMercenary.Loadout loadout)
	{
		m_cardId = string.Empty;
		m_isCardPremiumTagSet = false;
		if (mercenary == null)
		{
			m_disposableDef?.Dispose();
			m_disposableDef = null;
			m_artVariation = null;
			return;
		}
		LettuceMercenary.ArtVariation newArtVariation = null;
		newArtVariation = ((loadout != null) ? mercenary.GetOwnedArtVariation(loadout.m_artVariationRecord.ID, loadout.m_artVariationPremium) : mercenary.GetDefaultOrFirstAvailableArtVariation());
		if (m_artVariation != newArtVariation)
		{
			m_disposableDef?.Dispose();
			m_artVariation = newArtVariation;
			m_disposableDef = DefLoader.Get().GetCardDef(m_artVariation.m_record.CardId);
			UpdatePortraiteMaterial(m_artVariation.m_premium);
		}
	}

	private void UpdatePortraiteMaterial(TAG_PREMIUM cardPremiumTag)
	{
		Renderer portraitObjectRenderer = m_portraitObject.GetComponent<Renderer>();
		portraitObjectRenderer.SetSharedMaterial(m_portraitMaterialIndex, GetMaterial(cardPremiumTag));
		if (m_Frame != null)
		{
			switch (cardPremiumTag)
			{
			case TAG_PREMIUM.NORMAL:
				m_Frame.SetMaterial(m_NormalFrameMaterial);
				break;
			case TAG_PREMIUM.GOLDEN:
				m_Frame.SetMaterial(m_GoldenFrameMaterial);
				break;
			case TAG_PREMIUM.DIAMOND:
				m_Frame.SetMaterial(m_DiamondFrameMaterial);
				break;
			}
		}
		if (m_materialType == CardDefMaterialType.DeckCardBar || ServiceManager.Get<IGraphicsManager>().isVeryLowQualityDevice() || cardPremiumTag == TAG_PREMIUM.NORMAL)
		{
			return;
		}
		Material premiumPortraitMaterial = m_disposableDef.CardDef.GetPremiumPortraitMaterial();
		if (premiumPortraitMaterial != null)
		{
			Material currMaterial = portraitObjectRenderer.GetMaterial(m_portraitMaterialIndex);
			Texture shadowTex = null;
			if (currMaterial.HasProperty("_ShadowTex"))
			{
				shadowTex = currMaterial.GetTexture("_ShadowTex");
			}
			portraitObjectRenderer.SetMaterial(m_portraitMaterialIndex, premiumPortraitMaterial);
			Material material = portraitObjectRenderer.GetMaterial(m_portraitMaterialIndex);
			material.SetTexture("_ShadowTex", shadowTex);
			material.mainTextureOffset = currMaterial.mainTextureOffset;
			material.mainTextureScale = currMaterial.mainTextureScale;
		}
		UberShaderAnimation premiumPortraitAnimation = m_disposableDef.CardDef.GetPortraitAnimation(cardPremiumTag);
		if (premiumPortraitAnimation != null)
		{
			UberShaderController uberController = m_portraitObject.GetComponent<UberShaderController>();
			if (uberController == null)
			{
				uberController = m_portraitObject.AddComponent<UberShaderController>();
			}
			uberController.UberShaderAnimation = Object.Instantiate(premiumPortraitAnimation);
			uberController.m_MaterialIndex = m_portraitMaterialIndex;
		}
	}
}
