using System;
using System.Collections.Generic;
using System.Text;
using Hearthstone;
using Hearthstone.DataModels;

public class CollectibleBattlegroundsBoard : ICollectible, IComparable
{
	private SearchableString m_searchableString;

	public BattlegroundsBoardSkinDbfRecord DbfRecord { get; }

	public BattlegroundsBoardSkinId BoardSkinId { get; }

	public int OwnedCount
	{
		get
		{
			if (!CollectionManager.Get().OwnsBattlegroundsBoardSkin(BoardSkinId))
			{
				return 0;
			}
			return 1;
		}
	}

	public bool IsNewCollectible => CollectionManager.Get().ShouldShowNewBattlegroundsBoardSkinGlow(BoardSkinId);

	public CollectibleBattlegroundsBoard(BattlegroundsBoardSkinDbfRecord record)
	{
		if (record == null)
		{
			Error.AddDevFatal("CollectibleBattlegroundsBoard: DBF record unexpectedly null!");
			return;
		}
		DbfRecord = record;
		BoardSkinId = BattlegroundsBoardSkinId.FromTrustedValue(DbfRecord.ID);
	}

	public int CompareTo(object obj)
	{
		if (obj == null)
		{
			return -1;
		}
		if (obj.Equals(this))
		{
			return 0;
		}
		BattlegroundsBoardSkinDbfRecord objRecord = obj as BattlegroundsBoardSkinDbfRecord;
		if (objRecord == null && obj is CollectibleBattlegroundsBoard objAsCollectibleBoard)
		{
			objRecord = objAsCollectibleBoard.DbfRecord;
		}
		if (objRecord == null)
		{
			return -1;
		}
		return DbfRecord.CollectionName.GetString().CompareTo(objRecord.CollectionName.GetString());
	}

	public HashSet<string> GetSearchableTokens()
	{
		HashSet<string> m_searchableTokens = new HashSet<string> { DbfRecord.CollectionName, DbfRecord.CollectionShortName };
		CollectibleCardFilter.AddSearchableTokensToSet((TAG_RARITY)DbfRecord.Rarity, GameStrings.HasRarityText, GameStrings.GetRarityText, m_searchableTokens);
		return m_searchableTokens;
	}

	public SearchableString GetSearchableString()
	{
		if (m_searchableString == null)
		{
			string searchString = new StringBuilder().Append(DbfRecord.CollectionName).Append(" ").Append(DbfRecord.CollectionShortName)
				.Append(" ")
				.Append(DbfRecord.Description)
				.ToString();
			m_searchableString = new SearchableString(searchString);
		}
		return m_searchableString;
	}

	public BattlegroundsBoardSkinDataModel CreateBoardDataModel()
	{
		BattlegroundsBoardSkinDataModel boardSkinDataModel = new BattlegroundsBoardSkinDataModel();
		if (DbfRecord == null)
		{
			Log.CollectionManager.PrintError("CollectionUtils.CreateBoardDataModel(): DBF record was null!");
			return boardSkinDataModel;
		}
		boardSkinDataModel.BoardDbiId = DbfRecord.ID;
		boardSkinDataModel.DisplayName = DbfRecord.CollectionShortName;
		boardSkinDataModel.DetailsDisplayName = DbfRecord.CollectionName;
		boardSkinDataModel.Description = DbfRecord.Description;
		boardSkinDataModel.BorderType = DbfRecord.BorderType;
		boardSkinDataModel.ShopDetailsTexture = ((PlatformSettings.Screen == ScreenCategory.Phone) ? DbfRecord.DetailsTexturePhone : DbfRecord.DetailsTexture);
		boardSkinDataModel.ShopDetailsMovie = ((PlatformSettings.Screen == ScreenCategory.Phone) ? DbfRecord.DetailsMoviePhone : DbfRecord.DetailsMovie);
		boardSkinDataModel.Rarity = GameStrings.Get(GameStrings.GetRarityTextKey((TAG_RARITY)DbfRecord.Rarity));
		boardSkinDataModel.IsForCollectionPage = true;
		boardSkinDataModel.DetailsRenderConfig = DbfRecord.DetailsRenderConfig;
		boardSkinDataModel.CosmeticsRenderingEnabled = NetCache.Get()?.GetNetObject<NetCache.NetCacheFeatures>()?.Collection?.CosmeticsRenderingEnabled == true;
		if (CollectionManager.Get().IsFullyLoaded())
		{
			boardSkinDataModel.IsFavorite = CollectionManager.Get().IsFavoriteBattlegroundsBoardSkin(BoardSkinId);
			boardSkinDataModel.IsOwned = CollectionManager.Get().OwnsBattlegroundsBoardSkin(BoardSkinId);
			boardSkinDataModel.IsNew = CollectionManager.Get().ShouldShowNewBattlegroundsBoardSkinGlow(BoardSkinId);
		}
		return boardSkinDataModel;
	}
}
