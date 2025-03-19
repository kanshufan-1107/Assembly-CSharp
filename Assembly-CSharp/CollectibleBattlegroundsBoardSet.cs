using System.Collections.Generic;
using System.Linq;

public class CollectibleBattlegroundsBoardSet : CollectibleFilteredSet<CollectibleBattlegroundsBoard>
{
	private class CollectibleBoardComparer : IComparer<CollectibleBattlegroundsBoard>
	{
		public int Compare(CollectibleBattlegroundsBoard board1, CollectibleBattlegroundsBoard board2)
		{
			bool board1IsDefault = board1.BoardSkinId.IsDefaultBoard();
			bool board2IsDefault = board2.BoardSkinId.IsDefaultBoard();
			if (board1IsDefault && !board2IsDefault)
			{
				return -1;
			}
			if (board2IsDefault && !board1IsDefault)
			{
				return 1;
			}
			bool board1IsOwned = board1.OwnedCount > 0;
			bool board2IsOwned = board2.OwnedCount > 0;
			if (board1IsOwned && !board2IsOwned)
			{
				return -1;
			}
			if (board2IsOwned && !board1IsOwned)
			{
				return 1;
			}
			string boardName1 = board1.DbfRecord.CollectionShortName;
			string boardName2 = board2.DbfRecord.CollectionShortName;
			if (boardName1 != boardName2)
			{
				return boardName1.CompareTo(boardName2);
			}
			return board1.BoardSkinId.ToValue().CompareTo(board2.BoardSkinId.ToValue());
		}
	}

	private static readonly CollectibleBoardComparer s_BoardsComparer = new CollectibleBoardComparer();

	public CollectibleBattlegroundsBoardSet()
		: base((IComparer<CollectibleBattlegroundsBoard>)s_BoardsComparer)
	{
	}

	public int AddItemsFromDbf()
	{
		List<CollectibleBattlegroundsBoard> items = GetItemsFromDbf();
		return AddItems(items);
	}

	private static List<CollectibleBattlegroundsBoard> GetItemsFromDbf()
	{
		return (from record in GameDbf.BattlegroundsBoardSkin.GetRecords()
			select new CollectibleBattlegroundsBoard(record)).ToList();
	}
}
