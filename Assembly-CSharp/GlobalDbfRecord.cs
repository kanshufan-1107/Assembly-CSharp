using System;
using System.Collections.Generic;
using Assets;
using Blizzard.T5.Jobs;
using UnityEngine;

[Serializable]
public class GlobalDbfRecord : DbfRecord
{
	[SerializeField]
	private Global.AssetFlags m_assetFlags = Global.AssetFlags.NOT_PACKAGED_IN_CLIENT;

	[SerializeField]
	private Global.Cardalternatecost m_cardalternatecost;

	[SerializeField]
	private Global.Costcolor m_costcolor;

	[SerializeField]
	private Global.PresenceStatus m_presenceStatus;

	[SerializeField]
	private Global.Region m_region;

	[SerializeField]
	private Global.FormatType m_formatType;

	[SerializeField]
	private Global.RewardType m_rewardType;

	[SerializeField]
	private Global.CardPremiumLevel m_cardPremiumLevel;

	[SerializeField]
	private Global.MissionEventType m_missionEventType;

	[SerializeField]
	private Global.BnetGameType m_bnetGameType;

	[SerializeField]
	private Global.SoundCategory m_soundCategory;

	[SerializeField]
	private Global.GameStringCategory m_gameStringCategory;

	[SerializeField]
	private Global.MercenariesPremium m_mercenariesPremium;

	[SerializeField]
	private Global.MercenaryRewardRuleFlag m_mercenaryRewardRuleFlag;

	[SerializeField]
	private Global.MercenaryRewardSourceFlag m_mercenaryRewardSourceFlag;

	[SerializeField]
	private Global.MercenariesBountyDifficulty m_mercenariesBountyDifficulty = Global.MercenariesBountyDifficulty.NORMAL;

	[SerializeField]
	private Global.CardEmoteEvent m_cardEmoteEvent;

	[SerializeField]
	private Global.RewardTrackType m_rewardTrackType;

	[SerializeField]
	private Global.Boardvisualstate m_boardvisualstate;

	[SerializeField]
	private Global.Baconcombatstep m_baconcombatstep;

	[SerializeField]
	private Global.LeagueType m_leagueType = Global.ParseLeagueTypeValue("unknown");

	[SerializeField]
	private Global.LettuceFaction m_lettuceFaction;

	[SerializeField]
	private Global.TestFeature m_testFeature;

	[SerializeField]
	private Global.TestGroup m_testGroup = Global.TestGroup.INVALID;

	[SerializeField]
	private Global.UnlockableGameMode m_unlockableGameMode;

	[DbfField("ASSET_FLAGS")]
	public Global.AssetFlags AssetFlags => m_assetFlags;

	[DbfField("CardAlternateCost")]
	public Global.Cardalternatecost Cardalternatecost => m_cardalternatecost;

	[DbfField("CostColor")]
	public Global.Costcolor Costcolor => m_costcolor;

	[DbfField("PRESENCE_STATUS")]
	public Global.PresenceStatus PresenceStatus => m_presenceStatus;

	[DbfField("REGION")]
	public Global.Region Region => m_region;

	[DbfField("FORMAT_TYPE")]
	public Global.FormatType FormatType => m_formatType;

	[DbfField("REWARD_TYPE")]
	public Global.RewardType RewardType => m_rewardType;

	[DbfField("CARD_PREMIUM_LEVEL")]
	public Global.CardPremiumLevel CardPremiumLevel => m_cardPremiumLevel;

	[DbfField("MISSION_EVENT_TYPE")]
	public Global.MissionEventType MissionEventType => m_missionEventType;

	[DbfField("BNET_GAME_TYPE")]
	public Global.BnetGameType BnetGameType => m_bnetGameType;

	[DbfField("SOUND_CATEGORY")]
	public Global.SoundCategory SoundCategory => m_soundCategory;

	[DbfField("GAME_STRING_CATEGORY")]
	public Global.GameStringCategory GameStringCategory => m_gameStringCategory;

	[DbfField("MERCENARIES_PREMIUM")]
	public Global.MercenariesPremium MercenariesPremium => m_mercenariesPremium;

	[DbfField("Mercenary_Reward_Rule_Flag")]
	public Global.MercenaryRewardRuleFlag MercenaryRewardRuleFlag => m_mercenaryRewardRuleFlag;

	[DbfField("Mercenary_Reward_Source_Flag")]
	public Global.MercenaryRewardSourceFlag MercenaryRewardSourceFlag => m_mercenaryRewardSourceFlag;

	[DbfField("Mercenaries_Bounty_Difficulty")]
	public Global.MercenariesBountyDifficulty MercenariesBountyDifficulty => m_mercenariesBountyDifficulty;

	[DbfField("Card_Emote_Event")]
	public Global.CardEmoteEvent CardEmoteEvent => m_cardEmoteEvent;

	[DbfField("REWARD_TRACK_TYPE")]
	public Global.RewardTrackType RewardTrackType => m_rewardTrackType;

	[DbfField("BoardVisualState")]
	public Global.Boardvisualstate Boardvisualstate => m_boardvisualstate;

	[DbfField("BaconCombatStep")]
	public Global.Baconcombatstep Baconcombatstep => m_baconcombatstep;

	[DbfField("LEAGUE_TYPE")]
	public Global.LeagueType LeagueType => m_leagueType;

	[DbfField("Lettuce_Faction")]
	public Global.LettuceFaction LettuceFaction => m_lettuceFaction;

	[DbfField("TEST_FEATURE")]
	public Global.TestFeature TestFeature => m_testFeature;

	[DbfField("TEST_GROUP")]
	public Global.TestGroup TestGroup => m_testGroup;

	[DbfField("UNLOCKABLE_GAME_MODE")]
	public Global.UnlockableGameMode UnlockableGameMode => m_unlockableGameMode;

	public void SetAssetFlags(Global.AssetFlags v)
	{
		m_assetFlags = v;
	}

	public void SetCardalternatecost(Global.Cardalternatecost v)
	{
		m_cardalternatecost = v;
	}

	public void SetCostcolor(Global.Costcolor v)
	{
		m_costcolor = v;
	}

	public void SetPresenceStatus(Global.PresenceStatus v)
	{
		m_presenceStatus = v;
	}

	public void SetRegion(Global.Region v)
	{
		m_region = v;
	}

	public void SetFormatType(Global.FormatType v)
	{
		m_formatType = v;
	}

	public void SetRewardType(Global.RewardType v)
	{
		m_rewardType = v;
	}

	public void SetCardPremiumLevel(Global.CardPremiumLevel v)
	{
		m_cardPremiumLevel = v;
	}

	public void SetMissionEventType(Global.MissionEventType v)
	{
		m_missionEventType = v;
	}

	public void SetBnetGameType(Global.BnetGameType v)
	{
		m_bnetGameType = v;
	}

	public void SetSoundCategory(Global.SoundCategory v)
	{
		m_soundCategory = v;
	}

	public void SetGameStringCategory(Global.GameStringCategory v)
	{
		m_gameStringCategory = v;
	}

	public void SetMercenariesPremium(Global.MercenariesPremium v)
	{
		m_mercenariesPremium = v;
	}

	public void SetMercenaryRewardRuleFlag(Global.MercenaryRewardRuleFlag v)
	{
		m_mercenaryRewardRuleFlag = v;
	}

	public void SetMercenaryRewardSourceFlag(Global.MercenaryRewardSourceFlag v)
	{
		m_mercenaryRewardSourceFlag = v;
	}

	public void SetMercenariesBountyDifficulty(Global.MercenariesBountyDifficulty v)
	{
		m_mercenariesBountyDifficulty = v;
	}

	public void SetCardEmoteEvent(Global.CardEmoteEvent v)
	{
		m_cardEmoteEvent = v;
	}

	public void SetRewardTrackType(Global.RewardTrackType v)
	{
		m_rewardTrackType = v;
	}

	public void SetBoardvisualstate(Global.Boardvisualstate v)
	{
		m_boardvisualstate = v;
	}

	public void SetBaconcombatstep(Global.Baconcombatstep v)
	{
		m_baconcombatstep = v;
	}

	public void SetLeagueType(Global.LeagueType v)
	{
		m_leagueType = v;
	}

	public void SetLettuceFaction(Global.LettuceFaction v)
	{
		m_lettuceFaction = v;
	}

	public void SetTestFeature(Global.TestFeature v)
	{
		m_testFeature = v;
	}

	public void SetTestGroup(Global.TestGroup v)
	{
		m_testGroup = v;
	}

	public void SetUnlockableGameMode(Global.UnlockableGameMode v)
	{
		m_unlockableGameMode = v;
	}

	public override object GetVar(string name)
	{
		return name switch
		{
			"ASSET_FLAGS" => m_assetFlags, 
			"CardAlternateCost" => m_cardalternatecost, 
			"CostColor" => m_costcolor, 
			"PRESENCE_STATUS" => m_presenceStatus, 
			"REGION" => m_region, 
			"FORMAT_TYPE" => m_formatType, 
			"REWARD_TYPE" => m_rewardType, 
			"CARD_PREMIUM_LEVEL" => m_cardPremiumLevel, 
			"MISSION_EVENT_TYPE" => m_missionEventType, 
			"BNET_GAME_TYPE" => m_bnetGameType, 
			"SOUND_CATEGORY" => m_soundCategory, 
			"GAME_STRING_CATEGORY" => m_gameStringCategory, 
			"MERCENARIES_PREMIUM" => m_mercenariesPremium, 
			"Mercenary_Reward_Rule_Flag" => m_mercenaryRewardRuleFlag, 
			"Mercenary_Reward_Source_Flag" => m_mercenaryRewardSourceFlag, 
			"Mercenaries_Bounty_Difficulty" => m_mercenariesBountyDifficulty, 
			"Card_Emote_Event" => m_cardEmoteEvent, 
			"REWARD_TRACK_TYPE" => m_rewardTrackType, 
			"BoardVisualState" => m_boardvisualstate, 
			"BaconCombatStep" => m_baconcombatstep, 
			"LEAGUE_TYPE" => m_leagueType, 
			"Lettuce_Faction" => m_lettuceFaction, 
			"TEST_FEATURE" => m_testFeature, 
			"TEST_GROUP" => m_testGroup, 
			"UNLOCKABLE_GAME_MODE" => m_unlockableGameMode, 
			_ => null, 
		};
	}

	public override void SetVar(string name, object val)
	{
		switch (name)
		{
		case "ASSET_FLAGS":
			if (val == null)
			{
				m_assetFlags = Global.AssetFlags.NONE;
			}
			else if (val is Global.AssetFlags || val is int)
			{
				m_assetFlags = (Global.AssetFlags)val;
			}
			else if (val is string)
			{
				m_assetFlags = Global.ParseAssetFlagsValue((string)val);
			}
			break;
		case "CardAlternateCost":
			if (val == null)
			{
				m_cardalternatecost = Global.Cardalternatecost.MANA;
			}
			else if (val is Global.Cardalternatecost || val is int)
			{
				m_cardalternatecost = (Global.Cardalternatecost)val;
			}
			else if (val is string)
			{
				m_cardalternatecost = Global.ParseCardalternatecostValue((string)val);
			}
			break;
		case "CostColor":
			if (val == null)
			{
				m_costcolor = Global.Costcolor.DEFAULT;
			}
			else if (val is Global.Costcolor || val is int)
			{
				m_costcolor = (Global.Costcolor)val;
			}
			else if (val is string)
			{
				m_costcolor = Global.ParseCostcolorValue((string)val);
			}
			break;
		case "PRESENCE_STATUS":
			if (val == null)
			{
				m_presenceStatus = Global.PresenceStatus.LOGIN;
			}
			else if (val is Global.PresenceStatus || val is int)
			{
				m_presenceStatus = (Global.PresenceStatus)val;
			}
			else if (val is string)
			{
				m_presenceStatus = Global.ParsePresenceStatusValue((string)val);
			}
			break;
		case "REGION":
			if (val == null)
			{
				m_region = Global.Region.REGION_UNKNOWN;
			}
			else if (val is Global.Region || val is int)
			{
				m_region = (Global.Region)val;
			}
			else if (val is string)
			{
				m_region = Global.ParseRegionValue((string)val);
			}
			break;
		case "FORMAT_TYPE":
			if (val == null)
			{
				m_formatType = Global.FormatType.FT_UNKNOWN;
			}
			else if (val is Global.FormatType || val is int)
			{
				m_formatType = (Global.FormatType)val;
			}
			else if (val is string)
			{
				m_formatType = Global.ParseFormatTypeValue((string)val);
			}
			break;
		case "REWARD_TYPE":
			if (val == null)
			{
				m_rewardType = Global.RewardType.NONE;
			}
			else if (val is Global.RewardType || val is int)
			{
				m_rewardType = (Global.RewardType)val;
			}
			else if (val is string)
			{
				m_rewardType = Global.ParseRewardTypeValue((string)val);
			}
			break;
		case "CARD_PREMIUM_LEVEL":
			if (val == null)
			{
				m_cardPremiumLevel = Global.CardPremiumLevel.NORMAL;
			}
			else if (val is Global.CardPremiumLevel || val is int)
			{
				m_cardPremiumLevel = (Global.CardPremiumLevel)val;
			}
			else if (val is string)
			{
				m_cardPremiumLevel = Global.ParseCardPremiumLevelValue((string)val);
			}
			break;
		case "MISSION_EVENT_TYPE":
			if (val == null)
			{
				m_missionEventType = Global.MissionEventType.INVALID;
			}
			else if (val is Global.MissionEventType || val is int)
			{
				m_missionEventType = (Global.MissionEventType)val;
			}
			else if (val is string)
			{
				m_missionEventType = Global.ParseMissionEventTypeValue((string)val);
			}
			break;
		case "BNET_GAME_TYPE":
			if (val == null)
			{
				m_bnetGameType = Global.BnetGameType.BGT_UNKNOWN;
			}
			else if (val is Global.BnetGameType || val is int)
			{
				m_bnetGameType = (Global.BnetGameType)val;
			}
			else if (val is string)
			{
				m_bnetGameType = Global.ParseBnetGameTypeValue((string)val);
			}
			break;
		case "SOUND_CATEGORY":
			if (val == null)
			{
				m_soundCategory = Global.SoundCategory.NONE;
			}
			else if (val is Global.SoundCategory || val is int)
			{
				m_soundCategory = (Global.SoundCategory)val;
			}
			else if (val is string)
			{
				m_soundCategory = Global.ParseSoundCategoryValue((string)val);
			}
			break;
		case "GAME_STRING_CATEGORY":
			if (val == null)
			{
				m_gameStringCategory = Global.GameStringCategory.INVALID;
			}
			else if (val is Global.GameStringCategory || val is int)
			{
				m_gameStringCategory = (Global.GameStringCategory)val;
			}
			else if (val is string)
			{
				m_gameStringCategory = Global.ParseGameStringCategoryValue((string)val);
			}
			break;
		case "MERCENARIES_PREMIUM":
			if (val == null)
			{
				m_mercenariesPremium = Global.MercenariesPremium.PREMIUM_NORMAL;
			}
			else if (val is Global.MercenariesPremium || val is int)
			{
				m_mercenariesPremium = (Global.MercenariesPremium)val;
			}
			else if (val is string)
			{
				m_mercenariesPremium = Global.ParseMercenariesPremiumValue((string)val);
			}
			break;
		case "Mercenary_Reward_Rule_Flag":
			if (val == null)
			{
				m_mercenaryRewardRuleFlag = Global.MercenaryRewardRuleFlag.NONE;
			}
			else if (val is Global.MercenaryRewardRuleFlag || val is int)
			{
				m_mercenaryRewardRuleFlag = (Global.MercenaryRewardRuleFlag)val;
			}
			else if (val is string)
			{
				m_mercenaryRewardRuleFlag = Global.ParseMercenaryRewardRuleFlagValue((string)val);
			}
			break;
		case "Mercenary_Reward_Source_Flag":
			if (val == null)
			{
				m_mercenaryRewardSourceFlag = Global.MercenaryRewardSourceFlag.NONE;
			}
			else if (val is Global.MercenaryRewardSourceFlag || val is int)
			{
				m_mercenaryRewardSourceFlag = (Global.MercenaryRewardSourceFlag)val;
			}
			else if (val is string)
			{
				m_mercenaryRewardSourceFlag = Global.ParseMercenaryRewardSourceFlagValue((string)val);
			}
			break;
		case "Mercenaries_Bounty_Difficulty":
			if (val == null)
			{
				m_mercenariesBountyDifficulty = Global.MercenariesBountyDifficulty.NONE;
			}
			else if (val is Global.MercenariesBountyDifficulty || val is int)
			{
				m_mercenariesBountyDifficulty = (Global.MercenariesBountyDifficulty)val;
			}
			else if (val is string)
			{
				m_mercenariesBountyDifficulty = Global.ParseMercenariesBountyDifficultyValue((string)val);
			}
			break;
		case "Card_Emote_Event":
			if (val == null)
			{
				m_cardEmoteEvent = Global.CardEmoteEvent.INVALID;
			}
			else if (val is Global.CardEmoteEvent || val is int)
			{
				m_cardEmoteEvent = (Global.CardEmoteEvent)val;
			}
			else if (val is string)
			{
				m_cardEmoteEvent = Global.ParseCardEmoteEventValue((string)val);
			}
			break;
		case "REWARD_TRACK_TYPE":
			if (val == null)
			{
				m_rewardTrackType = Global.RewardTrackType.NONE;
			}
			else if (val is Global.RewardTrackType || val is int)
			{
				m_rewardTrackType = (Global.RewardTrackType)val;
			}
			else if (val is string)
			{
				m_rewardTrackType = Global.ParseRewardTrackTypeValue((string)val);
			}
			break;
		case "BoardVisualState":
			if (val == null)
			{
				m_boardvisualstate = Global.Boardvisualstate.NONE;
			}
			else if (val is Global.Boardvisualstate || val is int)
			{
				m_boardvisualstate = (Global.Boardvisualstate)val;
			}
			else if (val is string)
			{
				m_boardvisualstate = Global.ParseBoardvisualstateValue((string)val);
			}
			break;
		case "BaconCombatStep":
			if (val == null)
			{
				m_baconcombatstep = Global.Baconcombatstep.INVALID;
			}
			else if (val is Global.Baconcombatstep || val is int)
			{
				m_baconcombatstep = (Global.Baconcombatstep)val;
			}
			else if (val is string)
			{
				m_baconcombatstep = Global.ParseBaconcombatstepValue((string)val);
			}
			break;
		case "LEAGUE_TYPE":
			if (val == null)
			{
				m_leagueType = Global.LeagueType.UNKNOWN;
			}
			else if (val is Global.LeagueType || val is int)
			{
				m_leagueType = (Global.LeagueType)val;
			}
			else if (val is string)
			{
				m_leagueType = Global.ParseLeagueTypeValue((string)val);
			}
			break;
		case "Lettuce_Faction":
			if (val == null)
			{
				m_lettuceFaction = Global.LettuceFaction.NONE;
			}
			else if (val is Global.LettuceFaction || val is int)
			{
				m_lettuceFaction = (Global.LettuceFaction)val;
			}
			else if (val is string)
			{
				m_lettuceFaction = Global.ParseLettuceFactionValue((string)val);
			}
			break;
		case "TEST_FEATURE":
			if (val == null)
			{
				m_testFeature = Global.TestFeature.INVALID;
			}
			else if (val is Global.TestFeature || val is int)
			{
				m_testFeature = (Global.TestFeature)val;
			}
			else if (val is string)
			{
				m_testFeature = Global.ParseTestFeatureValue((string)val);
			}
			break;
		case "TEST_GROUP":
			if (val == null)
			{
				m_testGroup = Global.TestGroup.CONTROL_GROUP;
			}
			else if (val is Global.TestGroup || val is int)
			{
				m_testGroup = (Global.TestGroup)val;
			}
			else if (val is string)
			{
				m_testGroup = Global.ParseTestGroupValue((string)val);
			}
			break;
		case "UNLOCKABLE_GAME_MODE":
			if (val == null)
			{
				m_unlockableGameMode = Global.UnlockableGameMode.INVALID;
			}
			else if (val is Global.UnlockableGameMode || val is int)
			{
				m_unlockableGameMode = (Global.UnlockableGameMode)val;
			}
			else if (val is string)
			{
				m_unlockableGameMode = Global.ParseUnlockableGameModeValue((string)val);
			}
			break;
		}
	}

	public override Type GetVarType(string name)
	{
		return name switch
		{
			"ASSET_FLAGS" => typeof(Global.AssetFlags), 
			"CardAlternateCost" => typeof(Global.Cardalternatecost), 
			"CostColor" => typeof(Global.Costcolor), 
			"PRESENCE_STATUS" => typeof(Global.PresenceStatus), 
			"REGION" => typeof(Global.Region), 
			"FORMAT_TYPE" => typeof(Global.FormatType), 
			"REWARD_TYPE" => typeof(Global.RewardType), 
			"CARD_PREMIUM_LEVEL" => typeof(Global.CardPremiumLevel), 
			"MISSION_EVENT_TYPE" => typeof(Global.MissionEventType), 
			"BNET_GAME_TYPE" => typeof(Global.BnetGameType), 
			"SOUND_CATEGORY" => typeof(Global.SoundCategory), 
			"GAME_STRING_CATEGORY" => typeof(Global.GameStringCategory), 
			"MERCENARIES_PREMIUM" => typeof(Global.MercenariesPremium), 
			"Mercenary_Reward_Rule_Flag" => typeof(Global.MercenaryRewardRuleFlag), 
			"Mercenary_Reward_Source_Flag" => typeof(Global.MercenaryRewardSourceFlag), 
			"Mercenaries_Bounty_Difficulty" => typeof(Global.MercenariesBountyDifficulty), 
			"Card_Emote_Event" => typeof(Global.CardEmoteEvent), 
			"REWARD_TRACK_TYPE" => typeof(Global.RewardTrackType), 
			"BoardVisualState" => typeof(Global.Boardvisualstate), 
			"BaconCombatStep" => typeof(Global.Baconcombatstep), 
			"LEAGUE_TYPE" => typeof(Global.LeagueType), 
			"Lettuce_Faction" => typeof(Global.LettuceFaction), 
			"TEST_FEATURE" => typeof(Global.TestFeature), 
			"TEST_GROUP" => typeof(Global.TestGroup), 
			"UNLOCKABLE_GAME_MODE" => typeof(Global.UnlockableGameMode), 
			_ => null, 
		};
	}

	public override IEnumerator<IAsyncJobResult> Job_LoadRecordsFromAssetAsync<T>(string resourcePath, Action<List<T>> resultHandler)
	{
		LoadGlobalDbfRecords loadRecords = new LoadGlobalDbfRecords(resourcePath);
		yield return loadRecords;
		resultHandler?.Invoke(loadRecords.GetRecords() as List<T>);
	}

	public override bool LoadRecordsFromAsset<T>(string resourcePath, out List<T> records)
	{
		GlobalDbfAsset dbfAsset = DbfShared.GetAssetBundle().LoadAsset(resourcePath, typeof(GlobalDbfAsset)) as GlobalDbfAsset;
		if (dbfAsset == null)
		{
			records = new List<T>();
			Debug.LogError($"GlobalDbfAsset.LoadRecordsFromAsset() - failed to load records from assetbundle: {resourcePath}");
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
	}
}
