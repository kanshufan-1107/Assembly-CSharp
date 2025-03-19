using System;
using System.Collections.Generic;
using System.Text;
using Hearthstone;
using Hearthstone.DataModels;

public class CollectibleBattlegroundsEmote : ICollectible, IComparable
{
	private SearchableString m_searchableString;

	public BattlegroundsEmoteDbfRecord DbfRecord { get; }

	public BattlegroundsEmoteId EmoteId { get; }

	public int OwnedCount
	{
		get
		{
			if (!CollectionManager.Get().OwnsBattlegroundsEmote(EmoteId))
			{
				return 0;
			}
			return 1;
		}
	}

	public bool IsNewCollectible => CollectionManager.Get().ShouldShowNewBattlegroundsEmoteGlow(EmoteId);

	public CollectibleBattlegroundsEmote(BattlegroundsEmoteDbfRecord record)
	{
		if (record == null)
		{
			Error.AddDevFatal("CollectibleBattlegroundsEmote: DBF record unexpectedly null!");
			return;
		}
		DbfRecord = record;
		EmoteId = BattlegroundsEmoteId.FromTrustedValue(DbfRecord.ID);
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
		BattlegroundsEmoteDbfRecord objRecord = obj as BattlegroundsEmoteDbfRecord;
		if (objRecord == null && obj is CollectibleBattlegroundsEmote objAsCollectibleEmote)
		{
			objRecord = objAsCollectibleEmote.DbfRecord;
		}
		if (objRecord == null)
		{
			return -1;
		}
		return DbfRecord.CollectionShortName.GetString().CompareTo(objRecord.CollectionShortName.GetString());
	}

	public HashSet<string> GetSearchableTokens()
	{
		HashSet<string> m_searchableTokens = new HashSet<string> { DbfRecord.CollectionShortName };
		CollectibleCardFilter.AddSearchableTokensToSet((TAG_RARITY)DbfRecord.Rarity, GameStrings.HasRarityText, GameStrings.GetRarityText, m_searchableTokens);
		return m_searchableTokens;
	}

	public SearchableString GetSearchableString()
	{
		if (m_searchableString == null)
		{
			string searchString = new StringBuilder().Append(DbfRecord.CollectionShortName).Append(" ").Append(DbfRecord.Description)
				.ToString();
			m_searchableString = new SearchableString(searchString);
		}
		return m_searchableString;
	}

	public BattlegroundsEmoteDataModel CreateEmoteDataModel()
	{
		BattlegroundsEmoteDataModel emoteDataModel = new BattlegroundsEmoteDataModel();
		if (DbfRecord == null)
		{
			Log.CollectionManager.PrintError("CollectionUtils.CreateEmoteDataModel(): DBF record was null!");
			return emoteDataModel;
		}
		emoteDataModel.EmoteDbiId = DbfRecord.ID;
		emoteDataModel.DisplayName = DbfRecord.CollectionShortName;
		emoteDataModel.Description = DbfRecord.Description;
		emoteDataModel.Animation = DbfRecord.AnimationPath;
		emoteDataModel.IsAnimating = DbfRecord.IsAnimating;
		emoteDataModel.BorderType = DbfRecord.BorderType;
		emoteDataModel.XOffset = (float)DbfRecord.XOffset;
		emoteDataModel.ZOffset = (float)DbfRecord.ZOffset;
		emoteDataModel.Rarity = GameStrings.Get(GameStrings.GetRarityTextKey((TAG_RARITY)DbfRecord.Rarity));
		if (CollectionManager.Get().IsFullyLoaded())
		{
			emoteDataModel.IsOwned = CollectionManager.Get().OwnsBattlegroundsEmote(EmoteId);
			emoteDataModel.IsNew = CollectionManager.Get().ShouldShowNewBattlegroundsEmoteGlow(EmoteId);
			emoteDataModel.IsEquipped = CollectionManager.Get().IsEquippedBattlegroundsEmote(EmoteId);
		}
		return emoteDataModel;
	}
}
