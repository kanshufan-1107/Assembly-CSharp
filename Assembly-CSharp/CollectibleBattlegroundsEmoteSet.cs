using System.Collections.Generic;
using System.Linq;

public class CollectibleBattlegroundsEmoteSet : CollectibleFilteredSet<CollectibleBattlegroundsEmote>
{
	private class CollectibleEmoteComparer : IComparer<CollectibleBattlegroundsEmote>
	{
		public int Compare(CollectibleBattlegroundsEmote emote1, CollectibleBattlegroundsEmote emote2)
		{
			bool emote1IsDefault = emote1.EmoteId.IsDefaultEmote();
			bool emote2IsDefault = emote2.EmoteId.IsDefaultEmote();
			if (emote1IsDefault && !emote2IsDefault)
			{
				return -1;
			}
			if (emote2IsDefault && !emote1IsDefault)
			{
				return 1;
			}
			bool emote1IsOwned = emote1.OwnedCount > 0;
			bool emote2IsOwned = emote2.OwnedCount > 0;
			if (emote1IsOwned && !emote2IsOwned)
			{
				return -1;
			}
			if (emote2IsOwned && !emote1IsOwned)
			{
				return 1;
			}
			string emoteName1 = emote1.DbfRecord.CollectionShortName;
			string emoteName2 = emote2.DbfRecord.CollectionShortName;
			if (emoteName1 != emoteName2)
			{
				return emoteName1.CompareTo(emoteName2);
			}
			return emote1.EmoteId.ToValue().CompareTo(emote2.EmoteId.ToValue());
		}
	}

	private static readonly CollectibleEmoteComparer s_EmoteComparer = new CollectibleEmoteComparer();

	public CollectibleBattlegroundsEmoteSet()
		: base((IComparer<CollectibleBattlegroundsEmote>)s_EmoteComparer)
	{
	}

	public int AddItemsFromDbf()
	{
		List<CollectibleBattlegroundsEmote> items = GetItemsFromDbf();
		return AddItems(items);
	}

	private static List<CollectibleBattlegroundsEmote> GetItemsFromDbf()
	{
		return (from record in GameDbf.BattlegroundsEmote.GetRecords()
			select new CollectibleBattlegroundsEmote(record)).ToList();
	}
}
