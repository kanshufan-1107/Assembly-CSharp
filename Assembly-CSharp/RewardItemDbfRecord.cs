using System;
using System.Collections.Generic;
using Assets;
using Blizzard.T5.Jobs;
using UnityEngine;

[Serializable]
public class RewardItemDbfRecord : DbfRecord
{
	[SerializeField]
	private int m_rewardListId;

	[SerializeField]
	private RewardItem.RewardType m_rewardType;

	[SerializeField]
	private int m_quantity = 1;

	[SerializeField]
	private int m_cardId;

	[SerializeField]
	private RewardItem.CardPremiumLevel m_cardPremiumLevel;

	[SerializeField]
	private int m_randomCardBoosterCardSetId;

	[SerializeField]
	private int m_boosterId;

	[SerializeField]
	private RewardItem.BoosterSelector m_boosterSelector;

	[SerializeField]
	private int m_cardBackId;

	[SerializeField]
	private int m_customCoinId;

	[SerializeField]
	private int m_subsetId;

	[SerializeField]
	private bool m_isVirtual;

	[SerializeField]
	private int m_battlegroundsHeroSkinId;

	[SerializeField]
	private int m_battlegroundsGuideSkinId;

	[SerializeField]
	private int m_battlegroundsBoardSkinId;

	[SerializeField]
	private int m_battlegroundsFinisherId;

	[SerializeField]
	private int m_battlegroundsEmoteId;

	[SerializeField]
	private RewardItem.BattlegroundsBonusType m_battlegroundsBonusType;

	[SerializeField]
	private int m_mercenaryId;

	[SerializeField]
	private RewardItem.MercenarySelector m_mercenarySelector;

	[SerializeField]
	private int m_mercenaryArtVariationId;

	[SerializeField]
	private RewardItem.MercenariesPremium m_mercenaryArtPremium;

	[SerializeField]
	private int m_mercenaryEquipmentId;

	[SerializeField]
	private int m_mercenaryRarityId;

	[SerializeField]
	private int m_heroClassId;

	[SerializeField]
	private RewardItem.UnlockableGameMode m_gameModeSelector;

	[SerializeField]
	private int m_deckId;

	[SerializeField]
	private DbfLocValue m_standaloneDescription;

	[DbfField("REWARD_LIST_ID")]
	public int RewardListId => m_rewardListId;

	[DbfField("REWARD_TYPE")]
	public RewardItem.RewardType RewardType => m_rewardType;

	[DbfField("QUANTITY")]
	public int Quantity => m_quantity;

	[DbfField("CARD")]
	public int Card => m_cardId;

	public CardDbfRecord CardRecord => GameDbf.Card.GetRecord(m_cardId);

	[DbfField("CARD_PREMIUM_LEVEL")]
	public RewardItem.CardPremiumLevel CardPremiumLevel => m_cardPremiumLevel;

	[DbfField("RANDOM_CARD_BOOSTER_CARD_SET")]
	public int RandomCardBoosterCardSet => m_randomCardBoosterCardSetId;

	[DbfField("BOOSTER")]
	public int Booster => m_boosterId;

	[DbfField("BOOSTER_SELECTOR")]
	public RewardItem.BoosterSelector BoosterSelector => m_boosterSelector;

	[DbfField("CARD_BACK")]
	public int CardBack => m_cardBackId;

	[DbfField("CUSTOM_COIN")]
	public int CustomCoin => m_customCoinId;

	[DbfField("SUBSET_ID")]
	public int SubsetId => m_subsetId;

	[DbfField("BATTLEGROUNDS_HERO_SKIN_ID")]
	public int BattlegroundsHeroSkinId => m_battlegroundsHeroSkinId;

	public BattlegroundsHeroSkinDbfRecord BattlegroundsHeroSkinRecord => GameDbf.BattlegroundsHeroSkin.GetRecord(m_battlegroundsHeroSkinId);

	[DbfField("BATTLEGROUNDS_GUIDE_SKIN_ID")]
	public int BattlegroundsGuideSkinId => m_battlegroundsGuideSkinId;

	public BattlegroundsGuideSkinDbfRecord BattlegroundsGuideSkinRecord => GameDbf.BattlegroundsGuideSkin.GetRecord(m_battlegroundsGuideSkinId);

	[DbfField("BATTLEGROUNDS_BOARD_SKIN_ID")]
	public int BattlegroundsBoardSkinId => m_battlegroundsBoardSkinId;

	[DbfField("BATTLEGROUNDS_FINISHER_ID")]
	public int BattlegroundsFinisherId => m_battlegroundsFinisherId;

	[DbfField("BATTLEGROUNDS_EMOTE_ID")]
	public int BattlegroundsEmoteId => m_battlegroundsEmoteId;

	[DbfField("BATTLEGROUNDS_BONUS_TYPE")]
	public RewardItem.BattlegroundsBonusType BattlegroundsBonusType => m_battlegroundsBonusType;

	[DbfField("MERCENARY")]
	public int Mercenary => m_mercenaryId;

	public LettuceMercenaryDbfRecord MercenaryRecord => GameDbf.LettuceMercenary.GetRecord(m_mercenaryId);

	[DbfField("MERCENARY_SELECTOR")]
	public RewardItem.MercenarySelector MercenarySelector => m_mercenarySelector;

	[DbfField("MERCENARY_ART_VARIATION")]
	public int MercenaryArtVariation => m_mercenaryArtVariationId;

	public MercenaryArtVariationDbfRecord MercenaryArtVariationRecord => GameDbf.MercenaryArtVariation.GetRecord(m_mercenaryArtVariationId);

	[DbfField("MERCENARY_ART_PREMIUM")]
	public RewardItem.MercenariesPremium MercenaryArtPremium => m_mercenaryArtPremium;

	[DbfField("MERCENARY_EQUIPMENT")]
	public int MercenaryEquipment => m_mercenaryEquipmentId;

	[DbfField("MERCENARY_RARITY")]
	public int MercenaryRarity => m_mercenaryRarityId;

	[DbfField("HERO_CLASS_ID")]
	public int HeroClassId => m_heroClassId;

	[DbfField("GAME_MODE_SELECTOR")]
	public RewardItem.UnlockableGameMode GameModeSelector => m_gameModeSelector;

	[DbfField("DECK_ID")]
	public int DeckId => m_deckId;

	public DeckTemplateDbfRecord DeckRecord => GameDbf.DeckTemplate.GetRecord(m_deckId);

	[DbfField("STANDALONE_DESCRIPTION")]
	public DbfLocValue StandaloneDescription => m_standaloneDescription;

	public override object GetVar(string name)
	{
		return name switch
		{
			"ID" => base.ID, 
			"REWARD_LIST_ID" => m_rewardListId, 
			"REWARD_TYPE" => m_rewardType, 
			"QUANTITY" => m_quantity, 
			"CARD" => m_cardId, 
			"CARD_PREMIUM_LEVEL" => m_cardPremiumLevel, 
			"RANDOM_CARD_BOOSTER_CARD_SET" => m_randomCardBoosterCardSetId, 
			"BOOSTER" => m_boosterId, 
			"BOOSTER_SELECTOR" => m_boosterSelector, 
			"CARD_BACK" => m_cardBackId, 
			"CUSTOM_COIN" => m_customCoinId, 
			"SUBSET_ID" => m_subsetId, 
			"IS_VIRTUAL" => m_isVirtual, 
			"BATTLEGROUNDS_HERO_SKIN_ID" => m_battlegroundsHeroSkinId, 
			"BATTLEGROUNDS_GUIDE_SKIN_ID" => m_battlegroundsGuideSkinId, 
			"BATTLEGROUNDS_BOARD_SKIN_ID" => m_battlegroundsBoardSkinId, 
			"BATTLEGROUNDS_FINISHER_ID" => m_battlegroundsFinisherId, 
			"BATTLEGROUNDS_EMOTE_ID" => m_battlegroundsEmoteId, 
			"BATTLEGROUNDS_BONUS_TYPE" => m_battlegroundsBonusType, 
			"MERCENARY" => m_mercenaryId, 
			"MERCENARY_SELECTOR" => m_mercenarySelector, 
			"MERCENARY_ART_VARIATION" => m_mercenaryArtVariationId, 
			"MERCENARY_ART_PREMIUM" => m_mercenaryArtPremium, 
			"MERCENARY_EQUIPMENT" => m_mercenaryEquipmentId, 
			"MERCENARY_RARITY" => m_mercenaryRarityId, 
			"HERO_CLASS_ID" => m_heroClassId, 
			"GAME_MODE_SELECTOR" => m_gameModeSelector, 
			"DECK_ID" => m_deckId, 
			"STANDALONE_DESCRIPTION" => m_standaloneDescription, 
			_ => null, 
		};
	}

	public override void SetVar(string name, object val)
	{
		switch (name)
		{
		case "ID":
			SetID((int)val);
			break;
		case "REWARD_LIST_ID":
			m_rewardListId = (int)val;
			break;
		case "REWARD_TYPE":
			if (val == null)
			{
				m_rewardType = RewardItem.RewardType.NONE;
			}
			else if (val is RewardItem.RewardType || val is int)
			{
				m_rewardType = (RewardItem.RewardType)val;
			}
			else if (val is string)
			{
				m_rewardType = RewardItem.ParseRewardTypeValue((string)val);
			}
			break;
		case "QUANTITY":
			m_quantity = (int)val;
			break;
		case "CARD":
			m_cardId = (int)val;
			break;
		case "CARD_PREMIUM_LEVEL":
			if (val == null)
			{
				m_cardPremiumLevel = RewardItem.CardPremiumLevel.NORMAL;
			}
			else if (val is RewardItem.CardPremiumLevel || val is int)
			{
				m_cardPremiumLevel = (RewardItem.CardPremiumLevel)val;
			}
			else if (val is string)
			{
				m_cardPremiumLevel = RewardItem.ParseCardPremiumLevelValue((string)val);
			}
			break;
		case "RANDOM_CARD_BOOSTER_CARD_SET":
			m_randomCardBoosterCardSetId = (int)val;
			break;
		case "BOOSTER":
			m_boosterId = (int)val;
			break;
		case "BOOSTER_SELECTOR":
			if (val == null)
			{
				m_boosterSelector = RewardItem.BoosterSelector.NONE;
			}
			else if (val is RewardItem.BoosterSelector || val is int)
			{
				m_boosterSelector = (RewardItem.BoosterSelector)val;
			}
			else if (val is string)
			{
				m_boosterSelector = RewardItem.ParseBoosterSelectorValue((string)val);
			}
			break;
		case "CARD_BACK":
			m_cardBackId = (int)val;
			break;
		case "CUSTOM_COIN":
			m_customCoinId = (int)val;
			break;
		case "SUBSET_ID":
			m_subsetId = (int)val;
			break;
		case "IS_VIRTUAL":
			m_isVirtual = (bool)val;
			break;
		case "BATTLEGROUNDS_HERO_SKIN_ID":
			m_battlegroundsHeroSkinId = (int)val;
			break;
		case "BATTLEGROUNDS_GUIDE_SKIN_ID":
			m_battlegroundsGuideSkinId = (int)val;
			break;
		case "BATTLEGROUNDS_BOARD_SKIN_ID":
			m_battlegroundsBoardSkinId = (int)val;
			break;
		case "BATTLEGROUNDS_FINISHER_ID":
			m_battlegroundsFinisherId = (int)val;
			break;
		case "BATTLEGROUNDS_EMOTE_ID":
			m_battlegroundsEmoteId = (int)val;
			break;
		case "BATTLEGROUNDS_BONUS_TYPE":
			if (val == null)
			{
				m_battlegroundsBonusType = RewardItem.BattlegroundsBonusType.DISCOVER_HERO;
			}
			else if (val is RewardItem.BattlegroundsBonusType || val is int)
			{
				m_battlegroundsBonusType = (RewardItem.BattlegroundsBonusType)val;
			}
			else if (val is string)
			{
				m_battlegroundsBonusType = RewardItem.ParseBattlegroundsBonusTypeValue((string)val);
			}
			break;
		case "MERCENARY":
			m_mercenaryId = (int)val;
			break;
		case "MERCENARY_SELECTOR":
			if (val == null)
			{
				m_mercenarySelector = RewardItem.MercenarySelector.SPECIFIC;
			}
			else if (val is RewardItem.MercenarySelector || val is int)
			{
				m_mercenarySelector = (RewardItem.MercenarySelector)val;
			}
			else if (val is string)
			{
				m_mercenarySelector = RewardItem.ParseMercenarySelectorValue((string)val);
			}
			break;
		case "MERCENARY_ART_VARIATION":
			m_mercenaryArtVariationId = (int)val;
			break;
		case "MERCENARY_ART_PREMIUM":
			if (val == null)
			{
				m_mercenaryArtPremium = RewardItem.MercenariesPremium.PREMIUM_NORMAL;
			}
			else if (val is RewardItem.MercenariesPremium || val is int)
			{
				m_mercenaryArtPremium = (RewardItem.MercenariesPremium)val;
			}
			else if (val is string)
			{
				m_mercenaryArtPremium = RewardItem.ParseMercenariesPremiumValue((string)val);
			}
			break;
		case "MERCENARY_EQUIPMENT":
			m_mercenaryEquipmentId = (int)val;
			break;
		case "MERCENARY_RARITY":
			m_mercenaryRarityId = (int)val;
			break;
		case "HERO_CLASS_ID":
			m_heroClassId = (int)val;
			break;
		case "GAME_MODE_SELECTOR":
			if (val == null)
			{
				m_gameModeSelector = RewardItem.UnlockableGameMode.INVALID;
			}
			else if (val is RewardItem.UnlockableGameMode || val is int)
			{
				m_gameModeSelector = (RewardItem.UnlockableGameMode)val;
			}
			else if (val is string)
			{
				m_gameModeSelector = RewardItem.ParseUnlockableGameModeValue((string)val);
			}
			break;
		case "DECK_ID":
			m_deckId = (int)val;
			break;
		case "STANDALONE_DESCRIPTION":
			m_standaloneDescription = (DbfLocValue)val;
			break;
		}
	}

	public override Type GetVarType(string name)
	{
		return name switch
		{
			"ID" => typeof(int), 
			"REWARD_LIST_ID" => typeof(int), 
			"REWARD_TYPE" => typeof(RewardItem.RewardType), 
			"QUANTITY" => typeof(int), 
			"CARD" => typeof(int), 
			"CARD_PREMIUM_LEVEL" => typeof(RewardItem.CardPremiumLevel), 
			"RANDOM_CARD_BOOSTER_CARD_SET" => typeof(int), 
			"BOOSTER" => typeof(int), 
			"BOOSTER_SELECTOR" => typeof(RewardItem.BoosterSelector), 
			"CARD_BACK" => typeof(int), 
			"CUSTOM_COIN" => typeof(int), 
			"SUBSET_ID" => typeof(int), 
			"IS_VIRTUAL" => typeof(bool), 
			"BATTLEGROUNDS_HERO_SKIN_ID" => typeof(int), 
			"BATTLEGROUNDS_GUIDE_SKIN_ID" => typeof(int), 
			"BATTLEGROUNDS_BOARD_SKIN_ID" => typeof(int), 
			"BATTLEGROUNDS_FINISHER_ID" => typeof(int), 
			"BATTLEGROUNDS_EMOTE_ID" => typeof(int), 
			"BATTLEGROUNDS_BONUS_TYPE" => typeof(RewardItem.BattlegroundsBonusType), 
			"MERCENARY" => typeof(int), 
			"MERCENARY_SELECTOR" => typeof(RewardItem.MercenarySelector), 
			"MERCENARY_ART_VARIATION" => typeof(int), 
			"MERCENARY_ART_PREMIUM" => typeof(RewardItem.MercenariesPremium), 
			"MERCENARY_EQUIPMENT" => typeof(int), 
			"MERCENARY_RARITY" => typeof(int), 
			"HERO_CLASS_ID" => typeof(int), 
			"GAME_MODE_SELECTOR" => typeof(RewardItem.UnlockableGameMode), 
			"DECK_ID" => typeof(int), 
			"STANDALONE_DESCRIPTION" => typeof(DbfLocValue), 
			_ => null, 
		};
	}

	public override IEnumerator<IAsyncJobResult> Job_LoadRecordsFromAssetAsync<T>(string resourcePath, Action<List<T>> resultHandler)
	{
		LoadRewardItemDbfRecords loadRecords = new LoadRewardItemDbfRecords(resourcePath);
		yield return loadRecords;
		resultHandler?.Invoke(loadRecords.GetRecords() as List<T>);
	}

	public override bool LoadRecordsFromAsset<T>(string resourcePath, out List<T> records)
	{
		RewardItemDbfAsset dbfAsset = DbfShared.GetAssetBundle().LoadAsset(resourcePath, typeof(RewardItemDbfAsset)) as RewardItemDbfAsset;
		if (dbfAsset == null)
		{
			records = new List<T>();
			Debug.LogError($"RewardItemDbfAsset.LoadRecordsFromAsset() - failed to load records from assetbundle: {resourcePath}");
			return false;
		}
		for (int i = 0; i < dbfAsset.Records.Count; i++)
		{
			dbfAsset.Records[i].StripUnusedLocales();
		}
		records = dbfAsset.Records as List<T>;
		return true;
	}

	public override bool SaveRecordsToAsset<T>(string assetPath, List<T> records)
	{
		return false;
	}

	public override void StripUnusedLocales()
	{
		m_standaloneDescription.StripUnusedLocales();
	}
}
