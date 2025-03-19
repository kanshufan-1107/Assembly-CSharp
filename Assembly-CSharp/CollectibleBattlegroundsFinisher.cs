using System;
using System.Collections.Generic;
using System.Text;
using Hearthstone;
using Hearthstone.DataModels;

public class CollectibleBattlegroundsFinisher : ICollectible, IComparable
{
	private SearchableString m_searchableString;

	public BattlegroundsFinisherDbfRecord DbfRecord { get; }

	public BattlegroundsFinisherId FinisherId { get; }

	public int OwnedCount
	{
		get
		{
			if (!CollectionManager.Get().OwnsBattlegroundsFinisher(FinisherId))
			{
				return 0;
			}
			return 1;
		}
	}

	public bool IsNewCollectible => CollectionManager.Get().ShouldShowNewBattlegroundsFinisherGlow(FinisherId);

	public CollectibleBattlegroundsFinisher(BattlegroundsFinisherDbfRecord record)
	{
		if (record == null)
		{
			Error.AddDevFatal("CollectibleBattlegroundsFinisher: DBF record unexpectedly null!");
			return;
		}
		DbfRecord = record;
		FinisherId = BattlegroundsFinisherId.FromTrustedValue(DbfRecord.ID);
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
		BattlegroundsFinisherDbfRecord objRecord = obj as BattlegroundsFinisherDbfRecord;
		if (objRecord == null && obj is CollectibleBattlegroundsFinisher objAsCollectibleFinisher)
		{
			objRecord = objAsCollectibleFinisher.DbfRecord;
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

	public BattlegroundsFinisherDataModel CreateFinisherDataModel()
	{
		BattlegroundsFinisherDataModel finisherDataModel = new BattlegroundsFinisherDataModel();
		if (DbfRecord == null)
		{
			Log.CollectionManager.PrintError("CollectionUtils.CreateFinisherDataModel(): DBF record was null!");
			return finisherDataModel;
		}
		finisherDataModel.FinisherDbiId = DbfRecord.ID;
		finisherDataModel.DisplayName = DbfRecord.CollectionShortName;
		finisherDataModel.DetailsDisplayName = DbfRecord.CollectionName;
		finisherDataModel.Description = DbfRecord.Description;
		finisherDataModel.ShopDetailsTexture = DbfRecord.DetailsTexture;
		finisherDataModel.ShopDetailsMovie = DbfRecord.DetailsMovie;
		finisherDataModel.BodyMaterial = DbfRecord.MiniBodyMaterial;
		finisherDataModel.ArtMaterial = DbfRecord.MiniArtMaterial;
		finisherDataModel.CapsuleType = DbfRecord.CapsuleType;
		finisherDataModel.Rarity = GameStrings.Get(GameStrings.GetRarityTextKey((TAG_RARITY)DbfRecord.Rarity));
		finisherDataModel.IsForCollectionPage = true;
		finisherDataModel.DetailsRenderConfig = DbfRecord.DetailsRenderConfig;
		finisherDataModel.CosmeticsRenderingEnabled = NetCache.Get()?.GetNetObject<NetCache.NetCacheFeatures>()?.Collection?.CosmeticsRenderingEnabled == true;
		if (CollectionManager.Get().IsFullyLoaded())
		{
			finisherDataModel.IsFavorite = CollectionManager.Get().IsFavoriteBattlegroundsFinisher(FinisherId);
			finisherDataModel.IsOwned = CollectionManager.Get().OwnsBattlegroundsFinisher(FinisherId);
			finisherDataModel.IsNew = CollectionManager.Get().ShouldShowNewBattlegroundsFinisherGlow(FinisherId);
		}
		return finisherDataModel;
	}
}
