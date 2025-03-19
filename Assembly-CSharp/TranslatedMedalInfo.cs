using Assets;
using Hearthstone.DataModels;
using PegasusShared;
using UnityEngine;

public class TranslatedMedalInfo
{
	public int leagueId;

	public int earnedStars;

	public int starLevel;

	public int bestStarLevel;

	public int winStreak;

	public int legendIndex;

	public int seasonId;

	public int seasonWins;

	public int seasonGames;

	public int bestEverLeagueId;

	public int bestEverStarLevel;

	public int starsPerWin;

	public FormatType format;

	public LeagueDbfRecord LeagueConfig => RankMgr.Get().GetLeagueRecord(leagueId);

	public LeagueRankDbfRecord RankConfig => RankMgr.Get().GetLeagueRankRecord(leagueId, starLevel);

	public TranslatedMedalInfo ShallowCopy()
	{
		return MemberwiseClone() as TranslatedMedalInfo;
	}

	public bool IsLegendRank()
	{
		return RankConfig.ShowIndividualRanking;
	}

	public bool IsNewPlayer()
	{
		return LeagueConfig.LeagueType == League.LeagueType.NEW_PLAYER;
	}

	public FormatType GetFormatType()
	{
		return format;
	}

	public bool CanLoseStars()
	{
		return RankConfig.CanLoseStars;
	}

	public bool CanLoseLevel()
	{
		return RankConfig.CanLoseLevel;
	}

	public string GetRankName()
	{
		if (RankConfig.RankName != null)
		{
			return RankConfig.RankName.GetString();
		}
		return string.Empty;
	}

	public string GetMedalText()
	{
		if (RankConfig.MedalText != null)
		{
			return RankConfig.MedalText.GetString();
		}
		return string.Empty;
	}

	public int GetMaxStarsAtRank()
	{
		return RankConfig.Stars;
	}

	public int GetMaxStarLevel()
	{
		return RankMgr.Get().GetMaxStarLevel(leagueId);
	}

	public bool IsValid()
	{
		return starLevel >= 1;
	}

	public void CreateOrUpdateDataModel(ref RankedPlayDataModel dataModel, RankedMedal.DisplayMode mode, bool isTooltipEnabled = false, bool hasEarnedCardBack = false)
	{
		if (dataModel == null)
		{
			dataModel = CreateDataModel(mode, isTooltipEnabled, hasEarnedCardBack);
		}
		else
		{
			UpdateDataModel(dataModel, mode, isTooltipEnabled, hasEarnedCardBack);
		}
	}

	public RankedPlayDataModel CreateDataModel(RankedMedal.DisplayMode mode, bool isTooltipEnabled = false, bool hasEarnedCardBack = false)
	{
		RankedPlayDataModel dataModel = new RankedPlayDataModel();
		UpdateDataModel(dataModel, mode, isTooltipEnabled, hasEarnedCardBack);
		return dataModel;
	}

	public void UpdateDataModel(RankedPlayDataModel dataModel, RankedMedal.DisplayMode mode, bool isTooltipEnabled, bool hasEarnedCardBack)
	{
		if (dataModel == null)
		{
			Debug.LogError("TranslatedMedalInfo.UpdateDataModel - ranked play data model was null!");
			return;
		}
		dataModel.DisplayMode = mode;
		dataModel.IsTooltipEnabled = isTooltipEnabled;
		dataModel.HasEarnedCardBack = hasEarnedCardBack;
		dataModel.Stars = earnedStars;
		dataModel.MaxStars = RankConfig.Stars;
		dataModel.StarMultiplier = starsPerWin;
		dataModel.StarLevel = starLevel;
		dataModel.MedalText = GetMedalText();
		dataModel.RankName = GetRankName();
		dataModel.IsNewPlayer = IsNewPlayer();
		dataModel.IsLegend = IsLegendRank();
		dataModel.LegendRank = legendIndex;
		dataModel.FormatType = GetFormatType();
		if (mode != RankedMedal.DisplayMode.Chest && !string.IsNullOrEmpty(RankConfig.MedalTexture))
		{
			ObjectCallback onTextureLoaded = delegate(AssetReference assetRef, Object textureObj, object data)
			{
				dataModel.MedalTexture = textureObj as Texture;
			};
			AssetLoader.Get().LoadTexture(RankConfig.MedalTexture, onTextureLoaded);
		}
	}

	public override string ToString()
	{
		return $"[leagueId={leagueId} starLevel={starLevel} earnedStars={earnedStars} starsPerWin={starsPerWin}]";
	}
}
