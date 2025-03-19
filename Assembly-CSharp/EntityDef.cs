using System.Collections.Generic;
using Assets;
using UnityEngine;

public class EntityDef : EntityBase
{
	private struct CachedEntityName
	{
		public string Name;

		public int OverrideCardNameValue;

		public string CardId;
	}

	private struct CachedEntityDebugName
	{
		public string Name;

		public string CardId;

		public TAG_CARDTYPE CardType;
	}

	private struct CachedEntityCardId
	{
		public int CardDBId;

		public string CardId;
	}

	protected TagMap m_referencedTags = new TagMap();

	protected TagListMap m_referenceTagLists = new TagListMap();

	private CardTextBuilder m_cardTextBuilder;

	private static readonly CardPortraitQuality s_noTextureQuality = new CardPortraitQuality(0, TAG_PREMIUM.NORMAL);

	private CachedEntityName m_cachedEntityName;

	private CachedEntityDebugName m_cachedEntityDebugName;

	private CachedEntityCardId m_cachedLettuceAbilitySummonedMinion;

	public EntityDef()
	{
	}

	public EntityDef(int size)
		: base(size)
	{
	}

	public override string ToString()
	{
		return GetDebugName();
	}

	public EntityDef Clone()
	{
		EntityDef entityDef = new EntityDef();
		entityDef.m_cardId = base.m_cardId;
		entityDef.ReplaceTags(m_tags);
		entityDef.m_referencedTags.Replace(m_referencedTags);
		return entityDef;
	}

	public bool UseTechLevelManaGem()
	{
		if (!IsMinion() && !IsBaconSpell())
		{
			return false;
		}
		return (GameState.Get()?.GetGameEntity())?.HasTag(GAME_TAG.TECH_LEVEL_MANA_GEM) ?? false;
	}

	public override int GetReferencedTag(int tag)
	{
		return m_referencedTags.GetTag(tag);
	}

	public void SetReferencedTag(int tag, int val)
	{
		m_referencedTags.SetTag(tag, val);
	}

	public TAG_ENCHANTMENT_VISUAL GetEnchantmentBirthVisual()
	{
		return GetTag<TAG_ENCHANTMENT_VISUAL>(GAME_TAG.ENCHANTMENT_BIRTH_VISUAL);
	}

	public TAG_ENCHANTMENT_VISUAL GetEnchantmentIdleVisual()
	{
		return GetTag<TAG_ENCHANTMENT_VISUAL>(GAME_TAG.ENCHANTMENT_IDLE_VISUAL);
	}

	public TAG_RARITY GetRarity()
	{
		return (TAG_RARITY)GetTag(GAME_TAG.RARITY);
	}

	public EntityDef GetSummonedMinionEntityDef()
	{
		int minionDbId = GetTag(GAME_TAG.LETTUCE_ABILITY_SUMMONED_MINION);
		if (m_cachedLettuceAbilitySummonedMinion.CardDBId != minionDbId)
		{
			m_cachedLettuceAbilitySummonedMinion.CardDBId = minionDbId;
			m_cachedLettuceAbilitySummonedMinion.CardId = GameUtils.TranslateDbIdToCardId(m_cachedLettuceAbilitySummonedMinion.CardDBId);
		}
		return DefLoader.Get().GetEntityDef(m_cachedLettuceAbilitySummonedMinion.CardId);
	}

	public (bool valid, int attack, int health) GetSummonedMinionStats()
	{
		EntityDef entityDef = GetSummonedMinionEntityDef();
		if (entityDef != null)
		{
			return (valid: true, attack: entityDef.GetATK(), health: entityDef.GetHealth());
		}
		return (valid: false, attack: 0, health: 0);
	}

	public bool HasValidDisplayName()
	{
		CardDbfRecord record = GameDbf.GetIndex().GetCardRecord(base.m_cardId);
		if (record != null && record.Name != null)
		{
			return record.Name.GetString() != null;
		}
		return false;
	}

	public string GetName()
	{
		if (!IsValidEntityName())
		{
			UpdateEntityName();
		}
		return m_cachedEntityName.Name;
	}

	public string GetShortName()
	{
		CardDbfRecord record = GameDbf.GetIndex().GetCardRecord(base.m_cardId);
		if (record != null && record.ShortName != null)
		{
			return record.ShortName.GetString();
		}
		return null;
	}

	public string GetDebugName()
	{
		if (!IsValidEntityDebugName())
		{
			UpdateEntityDebugName();
		}
		return m_cachedEntityDebugName.Name;
	}

	public string GetArtistName(TAG_PREMIUM premium)
	{
		CardDbfRecord record = GameDbf.GetIndex().GetCardRecord(base.m_cardId);
		if (record == null)
		{
			return "ERROR: NO ARTIST NAME";
		}
		if (premium == TAG_PREMIUM.SIGNATURE)
		{
			return record.SignatureArtistName ?? string.Empty;
		}
		return record.ArtistName ?? string.Empty;
	}

	public string GetFlavorText()
	{
		CardDbfRecord record = GameDbf.GetIndex().GetCardRecord(base.m_cardId);
		if (record == null || record.FlavorText == null)
		{
			return string.Empty;
		}
		return record.FlavorText.GetString() ?? string.Empty;
	}

	public string GetHowToEarnText(TAG_PREMIUM premium)
	{
		CardDbfRecord record = GameDbf.GetIndex().GetCardRecord(base.m_cardId);
		if (record == null)
		{
			return string.Empty;
		}
		switch (premium)
		{
		case TAG_PREMIUM.GOLDEN:
			if (record.HowToGetGoldCard != null)
			{
				return record.HowToGetGoldCard.GetString() ?? string.Empty;
			}
			break;
		case TAG_PREMIUM.SIGNATURE:
			if (record.HowToGetSignatureCard != null)
			{
				return record.HowToGetSignatureCard.GetString() ?? string.Empty;
			}
			break;
		case TAG_PREMIUM.DIAMOND:
			if (record.HowToGetDiamondCard != null)
			{
				return record.HowToGetDiamondCard.GetString() ?? string.Empty;
			}
			break;
		default:
			if (record.HowToGetCard != null)
			{
				return record.HowToGetCard.GetString() ?? string.Empty;
			}
			break;
		}
		return string.Empty;
	}

	public string GetCardTextInHand()
	{
		if (GetCardTextBuilder() != null)
		{
			return GetCardTextBuilder().BuildCardTextInHand(this);
		}
		Debug.LogWarning($"EntityDef.GetCardTextInHand: No textbuilder found for {base.m_cardId}, returning default text");
		return CardTextBuilder.GetDefaultCardTextInHand(this);
	}

	public CardTextBuilder GetCardTextBuilder()
	{
		if (m_cardTextBuilder == null)
		{
			if (HasTag(GAME_TAG.OVERRIDECARDTEXTBUILDER))
			{
				m_cardTextBuilder = CardTextBuilderFactory.Create((Assets.Card.CardTextBuilderType)GetTag(GAME_TAG.OVERRIDECARDTEXTBUILDER));
			}
			else
			{
				CardDbfRecord record = GameDbf.GetIndex().GetCardRecord(base.m_cardId);
				if (record != null)
				{
					m_cardTextBuilder = CardTextBuilderFactory.Create(record.CardTextBuilderType);
				}
			}
		}
		return m_cardTextBuilder;
	}

	public void ClearCardTextBuilder()
	{
		m_cardTextBuilder = null;
	}

	public string GetWatermarkTextureOverride()
	{
		return GameDbf.GetIndex().GetCardRecord(base.m_cardId)?.WatermarkTextureOverride;
	}

	public bool LoadTagFromDBF(string designCode, List<CardTagDbfRecord> tags)
	{
		base.m_cardId = designCode;
		return LoadTagFromDBF_SetTags(tags);
	}

	public static Dictionary<string, EntityDef> LoadBatchCardEntityDefs(List<string> cardIds, out List<string> failedCardIds)
	{
		Dictionary<string, EntityDef> entityDefMap = new Dictionary<string, EntityDef>(cardIds.Count + 1);
		failedCardIds = new List<string>();
		foreach (string designCode in cardIds)
		{
			bool loadSuccess = false;
			EntityDef entityDef = null;
			List<CardTagDbfRecord> tags = null;
			if (GameUtils.TryGetCardTagRecords(designCode, out tags))
			{
				entityDef = new EntityDef(tags.Count);
				loadSuccess = entityDef.LoadTagFromDBF(designCode, tags);
			}
			else
			{
				loadSuccess = false;
			}
			if (!loadSuccess)
			{
				failedCardIds.Add(designCode);
			}
			else
			{
				entityDefMap.Add(designCode, entityDef);
			}
		}
		return entityDefMap;
	}

	public bool IsValidEntityName()
	{
		int overrideNameValue = GetTag(GAME_TAG.OVERRIDECARDNAME);
		if (m_cachedEntityName.OverrideCardNameValue == overrideNameValue && m_cachedEntityName.CardId == base.m_cardId)
		{
			return !string.IsNullOrEmpty(m_cachedEntityName.Name);
		}
		return false;
	}

	public bool IsValidEntityDebugName()
	{
		if (m_cachedEntityDebugName.CardId == base.m_cardId && m_cachedEntityDebugName.CardType == GetCardType())
		{
			return !string.IsNullOrEmpty(m_cachedEntityName.Name);
		}
		return false;
	}

	private bool LoadTagFromDBF_SetTags(List<CardTagDbfRecord> tags)
	{
		if (tags == null)
		{
			Debug.LogError($"EntityDef.LoadDataFromCardXml() - No tags found for the card: {base.m_cardId}");
			return false;
		}
		foreach (CardTagDbfRecord record in tags)
		{
			if (record.IsReferenceTag)
			{
				SetReferencedTag(record.TagId, record.TagValue);
				if (record.IsPowerKeywordTag)
				{
					SetTag(record.TagId, record.TagValue);
				}
			}
			else
			{
				SetTag(record.TagId, record.TagValue);
			}
		}
		return true;
	}

	private void UpdateEntityName()
	{
		int overrideNameValue = GetTag(GAME_TAG.OVERRIDECARDNAME);
		m_cachedEntityName.OverrideCardNameValue = overrideNameValue;
		m_cachedEntityName.CardId = base.m_cardId;
		if (overrideNameValue > 0)
		{
			EntityDef entityDef = DefLoader.Get().GetEntityDef(overrideNameValue);
			if (entityDef != null)
			{
				m_cachedEntityName.Name = entityDef.GetName();
				return;
			}
		}
		if (GetCardTextBuilder() != null)
		{
			m_cachedEntityName.Name = GetCardTextBuilder().BuildCardName(this);
		}
		else
		{
			m_cachedEntityName.Name = CardTextBuilder.GetDefaultCardName(this);
		}
	}

	private void UpdateEntityDebugName()
	{
		string name = null;
		CardDbfRecord record = GameDbf.GetIndex().GetCardRecord(base.m_cardId);
		if (record != null && record.Name != null)
		{
			name = record.Name.GetString();
		}
		TAG_CARDTYPE cardType = GetCardType();
		m_cachedEntityDebugName.CardId = base.m_cardId;
		m_cachedEntityDebugName.CardType = cardType;
		if (name != null)
		{
			m_cachedEntityDebugName.Name = $"[name={name} cardId={base.m_cardId} type={cardType}]";
		}
		else if (base.m_cardId != null)
		{
			m_cachedEntityDebugName.Name = $"[cardId={base.m_cardId} type={cardType}]";
		}
		else
		{
			m_cachedEntityDebugName.Name = $"UNKNOWN ENTITY [cardType={cardType}]";
		}
	}
}
