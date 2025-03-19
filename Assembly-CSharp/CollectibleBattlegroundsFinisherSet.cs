using System.Collections.Generic;
using System.Linq;

public class CollectibleBattlegroundsFinisherSet : CollectibleFilteredSet<CollectibleBattlegroundsFinisher>
{
	private class CollectibleFinisherComparer : IComparer<CollectibleBattlegroundsFinisher>
	{
		public int Compare(CollectibleBattlegroundsFinisher finisher1, CollectibleBattlegroundsFinisher finisher2)
		{
			bool finisher1IsDefault = finisher1.FinisherId.IsDefaultFinisher();
			bool finisher2IsDefault = finisher2.FinisherId.IsDefaultFinisher();
			if (finisher1IsDefault && !finisher2IsDefault)
			{
				return -1;
			}
			if (finisher2IsDefault && !finisher1IsDefault)
			{
				return 1;
			}
			bool finisher1IsOwned = finisher1.OwnedCount > 0;
			bool finisher2IsOwned = finisher2.OwnedCount > 0;
			if (finisher1IsOwned && !finisher2IsOwned)
			{
				return -1;
			}
			if (finisher2IsOwned && !finisher1IsOwned)
			{
				return 1;
			}
			string finisherName1 = finisher1.DbfRecord.CollectionShortName;
			string finisherName2 = finisher2.DbfRecord.CollectionShortName;
			if (finisherName1 != finisherName2)
			{
				return finisherName1.CompareTo(finisherName2);
			}
			return finisher1.FinisherId.ToValue().CompareTo(finisher2.FinisherId.ToValue());
		}
	}

	private static readonly CollectibleFinisherComparer s_FinisherComparer = new CollectibleFinisherComparer();

	public CollectibleBattlegroundsFinisherSet()
		: base((IComparer<CollectibleBattlegroundsFinisher>)s_FinisherComparer)
	{
	}

	public int AddItemsFromDbf()
	{
		List<CollectibleBattlegroundsFinisher> items = GetItemsFromDbf();
		return AddItems(items);
	}

	private static List<CollectibleBattlegroundsFinisher> GetItemsFromDbf()
	{
		return (from record in GameDbf.BattlegroundsFinisher.GetRecords()
			where record.Enabled
			select new CollectibleBattlegroundsFinisher(record)).ToList();
	}
}
