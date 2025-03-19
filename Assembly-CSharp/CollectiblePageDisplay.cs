using System.Collections.Generic;
using Blizzard.T5.MaterialService.Extensions;
using PegasusShared;
using UnityEngine;

public abstract class CollectiblePageDisplay : BookPageDisplay
{
	public GameObject m_cardStartPositionEightCards;

	public UberText m_pageCountText;

	public UberText m_pageNameText;

	public GameObject m_pageFlavorHeader;

	public GameObject m_basePage;

	public Material m_headerMaterial;

	public Material m_pageMaterial;

	public Color m_textColor;

	protected List<CollectionCardVisual> m_collectionCardVisuals = new List<CollectionCardVisual>();

	protected Material m_basePageMaterial;

	public override bool IsLoaded()
	{
		return true;
	}

	public static int GetMaxCardsPerPage()
	{
		CollectionUtils.CollectionPageLayoutSettings.Variables settings = CollectionManager.Get().GetCollectibleDisplay().GetCurrentPageLayoutSettings();
		return settings.m_ColumnCount * settings.m_RowCount;
	}

	public static int GetMaxCardsPerPage(CollectionUtils.ViewMode viewMode)
	{
		if (CollectionManager.Get() == null || CollectionManager.Get().GetCollectibleDisplay() == null)
		{
			Log.CollectionManager.Print("CollectiblePageDisplay.GetMaxCardsPerPage - Null checks failed! mode={0}", viewMode);
			return 0;
		}
		CollectionUtils.CollectionPageLayoutSettings.Variables settings = CollectionManager.Get().GetCollectibleDisplay().GetPageLayoutSettings(viewMode);
		return settings.m_ColumnCount * settings.m_RowCount;
	}

	public CollectionCardVisual GetCardVisual(string cardID, TAG_PREMIUM premium)
	{
		foreach (CollectionCardVisual cardVisual in m_collectionCardVisuals)
		{
			if (cardVisual.IsShown() && cardVisual.GetVisualType() == CollectionUtils.ViewMode.CARDS)
			{
				Actor actor = cardVisual.GetActor();
				if (actor.GetEntityDef().GetCardId().Equals(cardID) && actor.GetPremium() == premium)
				{
					return cardVisual;
				}
			}
		}
		return null;
	}

	public override void Show()
	{
		base.Show();
		MassDisenchant massDisenchant = MassDisenchant.Get();
		if (massDisenchant != null && massDisenchant.IsShown())
		{
			return;
		}
		for (int i = 0; i < m_collectionCardVisuals.Count; i++)
		{
			CollectionCardVisual collectionCardVisual = GetCollectionCardVisual(i);
			if (collectionCardVisual.GetActor() != null)
			{
				collectionCardVisual.Show();
			}
		}
	}

	public override void Hide()
	{
		base.Hide();
		MarkAllShownCardsSeen();
		for (int i = 0; i < m_collectionCardVisuals.Count; i++)
		{
			GetCollectionCardVisual(i).Hide();
		}
	}

	public void UpdateAllSpecialCaseTransforms()
	{
		for (int i = 0; i < m_collectionCardVisuals.Count; i++)
		{
			GetCollectionCardVisual(i).UpdateSpecialCaseTransform();
		}
	}

	public virtual void MarkAllShownCardsSeen()
	{
		for (int i = 0; i < m_collectionCardVisuals.Count; i++)
		{
			CollectionCardVisual collectionCardVisual = GetCollectionCardVisual(i);
			if (collectionCardVisual.IsShown())
			{
				collectionCardVisual.MarkAsSeen();
			}
		}
	}

	public virtual void UpdateCollectionItems(List<CollectionCardActors> actorList, List<ICollectible> nonActorCollectibles, CollectionUtils.ViewMode mode)
	{
		Log.CollectionManager.Print("mode={0}", mode);
		int i;
		for (i = 0; i < actorList.Count && i < GetMaxCardsPerPage(); i++)
		{
			CollectionCardVisual collectionCardVisual = GetCollectionCardVisual(i);
			collectionCardVisual.SetActors(actorList[i], mode);
			collectionCardVisual.Show();
			if (mode == CollectionUtils.ViewMode.HERO_SKINS)
			{
				collectionCardVisual.SetHeroSkinBoxCollider();
			}
			else
			{
				collectionCardVisual.SetDefaultBoxCollider();
			}
		}
		for (int j = i; j < m_collectionCardVisuals.Count; j++)
		{
			CollectionCardVisual collectionCardVisual2 = GetCollectionCardVisual(j);
			collectionCardVisual2.Hide();
			collectionCardVisual2.SetActors(null);
		}
		UpdateCurrentPageCardLocks();
	}

	public void UpdateBasePage()
	{
		if (m_basePageMaterial != null && m_basePage != null)
		{
			m_basePage.GetComponent<MeshRenderer>().SetMaterial(m_basePageMaterial);
		}
	}

	public abstract void ShowNoMatchesFound(bool show, CollectionManager.FindCardsResult findResults = null, bool showHints = true);

	public List<CollectionCardVisual> ApplyRuneCardGhostEffectsForCurrentPage(RunePattern deckRunePattern)
	{
		List<CollectionCardVisual> affectedCards = new List<CollectionCardVisual>();
		foreach (CollectionCardVisual collectionCardVisual in m_collectionCardVisuals)
		{
			if (!collectionCardVisual.IsShown())
			{
				continue;
			}
			Actor actor = collectionCardVisual.GetActor();
			if (!(actor == null))
			{
				EntityDef entityDef = actor.GetEntityDef();
				if (entityDef != null && !deckRunePattern.CanAddRunes(entityDef.GetRuneCost(), DeckRule_DeathKnightRuneLimit.MaxRuneSlots))
				{
					actor.GhostCardEffect(GhostCard.Type.NOT_VALID, actor.GetPremium());
					affectedCards.Add(collectionCardVisual);
				}
			}
		}
		return affectedCards;
	}

	public List<CollectionCardVisual> ApplyTouristCardGhostEffectsForCurrentPage(CollectionDeck collectionDeck)
	{
		List<CollectionCardVisual> affectedCards = new List<CollectionCardVisual>();
		if (collectionDeck.GetRuleset(null).HasMaxTouristsRule(out var maxTourists) && collectionDeck.GetCardCountHasTag(GAME_TAG.TOURIST) >= maxTourists)
		{
			foreach (CollectionCardVisual collectionCardVisual in m_collectionCardVisuals)
			{
				if (!collectionCardVisual.IsShown())
				{
					continue;
				}
				Actor actor = collectionCardVisual.GetActor();
				if (!(actor == null))
				{
					EntityDef entityDef = actor.GetEntityDef();
					if (entityDef != null && entityDef.HasTag(GAME_TAG.TOURIST) && collectionDeck.GetCardIdCount(entityDef.GetCardId()) <= 0)
					{
						actor.GhostCardEffect(GhostCard.Type.NOT_VALID, actor.GetPremium());
						affectedCards.Add(collectionCardVisual);
					}
				}
			}
		}
		return affectedCards;
	}

	public virtual void UpdateCurrentPageCardLocks(bool playSound = false)
	{
		CollectionDeckTray deckTray = CollectionDeckTray.Get();
		if (!(deckTray != null) || deckTray.GetCurrentContentType() == DeckTray.DeckContentTypes.Cards)
		{
			return;
		}
		foreach (CollectionCardVisual collectionCardVisual in m_collectionCardVisuals)
		{
			if (!collectionCardVisual.IsShown())
			{
				continue;
			}
			if (collectionCardVisual.GetVisualType() == CollectionUtils.ViewMode.CARDS)
			{
				Actor actor = collectionCardVisual.GetActor();
				string cardID = actor.GetEntityDef().GetCardId();
				TAG_PREMIUM premium = actor.GetPremium();
				CollectibleCard card = CollectionManager.Get().GetCard(cardID, premium);
				if (!GameUtils.IsCardGameplayEventActive(cardID) && card.OwnedCount > 0)
				{
					collectionCardVisual.ShowLock(CollectionCardVisual.LockType.NOT_PLAYABLE, GameStrings.Get("GLUE_COLLECTION_LOCK_CARD_NOT_PLAYABLE"), playSound);
					continue;
				}
			}
			collectionCardVisual.ShowLock(CollectionCardVisual.LockType.NONE);
		}
	}

	public abstract void SetPageType(FormatType formatType);

	public void SetPageCountText(string text)
	{
		if (m_pageCountText != null)
		{
			m_pageCountText.Text = text;
		}
	}

	public void ActivatePageCountText(bool active)
	{
		if (m_pageCountText != null)
		{
			m_pageCountText.gameObject.SetActive(active);
		}
	}

	protected CollectionCardVisual GetCollectionCardVisual(int index)
	{
		CollectionUtils.CollectionPageLayoutSettings.Variables settings = CollectionManager.Get().GetCollectibleDisplay().GetCurrentPageLayoutSettings();
		float columnSpacing = settings.m_ColumnSpacing;
		int colCount = settings.m_ColumnCount;
		float cardAreaWidth = columnSpacing * (float)(colCount - 1);
		float cardScale = settings.m_Scale;
		float rowSpacing = settings.m_RowSpacing;
		Vector3 positionLocalToPage = m_cardStartPositionEightCards.transform.localPosition + settings.m_Offset;
		int rowCount = index / colCount;
		positionLocalToPage.x += (float)(index % colCount) * columnSpacing - cardAreaWidth * 0.5f;
		positionLocalToPage.z -= rowSpacing * (float)rowCount;
		CollectionCardVisual cardVisual;
		if (index == m_collectionCardVisuals.Count)
		{
			cardVisual = (CollectionCardVisual)GameUtils.Instantiate(CollectionManager.Get().GetCollectibleDisplay().GetCardVisualPrefab(), base.gameObject);
			m_collectionCardVisuals.Insert(index, cardVisual);
		}
		else
		{
			cardVisual = m_collectionCardVisuals[index];
		}
		cardVisual.SetCMRow(rowCount);
		cardVisual.transform.localScale = new Vector3(cardScale, cardScale, cardScale);
		cardVisual.transform.position = base.transform.TransformPoint(positionLocalToPage);
		return cardVisual;
	}

	protected void SetPageNameText(string className)
	{
		if (m_pageNameText != null)
		{
			m_pageNameText.Text = className;
		}
	}

	public static void SetPageFlavorTextures(GameObject header, UnityEngine.Vector2 offset)
	{
		if (!(header == null))
		{
			header.GetComponent<Renderer>().GetMaterial().SetTextureOffset("_MainTex", offset);
			if (header != null)
			{
				header.SetActive(value: true);
			}
		}
	}
}
