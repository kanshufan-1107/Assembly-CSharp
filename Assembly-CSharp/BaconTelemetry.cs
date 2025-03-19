using System.Collections.Generic;
using Blizzard.Telemetry.WTCG.Client;
using Hearthstone;

public class BaconTelemetry
{
	public static void SendBattlegroundsCollectionResultLogin()
	{
		SendBattlegroundsCollectionResult(BattlegroundsCollectionResult.TriggerEvent.Login);
	}

	public static void SendBattlegroundsCollectionResultExitCollection()
	{
		SendBattlegroundsCollectionResult(BattlegroundsCollectionResult.TriggerEvent.ExitBattlegroundsCollection);
	}

	private static void SendBattlegroundsCollectionResult(BattlegroundsCollectionResult.TriggerEvent triggerEvent)
	{
		NetCache.NetCacheBattlegroundsHeroSkins netCacheBGHeroSkins = NetCache.Get().GetNetObject<NetCache.NetCacheBattlegroundsHeroSkins>();
		int numberOfOwnedBattlegroundsHeroSkins = 0;
		List<int> allFavoritedBaseHeroCardIds = new List<int>();
		List<int> allFavoriteBattlegroundsHeroSkinIds = new List<int>();
		if (netCacheBGHeroSkins != null)
		{
			numberOfOwnedBattlegroundsHeroSkins = netCacheBGHeroSkins.OwnedBattlegroundsSkins.Count;
			foreach (KeyValuePair<int, HashSet<int>> keyValuePair in netCacheBGHeroSkins.BattlegroundsFavoriteHeroSkins)
			{
				int baseHeroCardId = keyValuePair.Key;
				foreach (int favoriteBattlegroundsHeroSkinId in keyValuePair.Value)
				{
					if (favoriteBattlegroundsHeroSkinId == BaconHeroSkinUtils.SKIN_ID_FOR_FAVORITED_BASE_HERO)
					{
						allFavoritedBaseHeroCardIds.Add(baseHeroCardId);
					}
					else
					{
						allFavoriteBattlegroundsHeroSkinIds.Add(favoriteBattlegroundsHeroSkinId);
					}
				}
			}
		}
		NetCache.NetCacheBattlegroundsFinishers netCacheBGStrikes = NetCache.Get().GetNetObject<NetCache.NetCacheBattlegroundsFinishers>();
		int numberOfOwnedBattlegroundsStrikes = 0;
		List<int> allFavoriteBattlegroundsStrikeIds = new List<int>();
		if (netCacheBGStrikes != null)
		{
			numberOfOwnedBattlegroundsStrikes = netCacheBGStrikes.OwnedBattlegroundsFinishers.Count;
			foreach (BattlegroundsFinisherId favoriteBattlegroundsStrikeId in netCacheBGStrikes.BattlegroundsFavoriteFinishers)
			{
				if (favoriteBattlegroundsStrikeId.IsValid())
				{
					allFavoriteBattlegroundsStrikeIds.Add(favoriteBattlegroundsStrikeId.ToValue());
				}
			}
		}
		NetCache.NetCacheBattlegroundsGuideSkins netCacheBGGuideSkins = NetCache.Get().GetNetObject<NetCache.NetCacheBattlegroundsGuideSkins>();
		int numberOfOwnedBattlegroundsGuideSkins = 0;
		List<int> allFavoriteBattlegroundsGuideSkins = new List<int>();
		if (netCacheBGGuideSkins != null)
		{
			numberOfOwnedBattlegroundsGuideSkins = netCacheBGGuideSkins.OwnedBattlegroundsGuideSkins.Count;
			if (netCacheBGGuideSkins.BattlegroundsFavoriteGuideSkin.HasValue)
			{
				allFavoriteBattlegroundsGuideSkins.Add(netCacheBGGuideSkins.BattlegroundsFavoriteGuideSkin.Value.ToValue());
			}
		}
		NetCache.NetCacheBattlegroundsBoardSkins netCacheBGBoardSkins = NetCache.Get().GetNetObject<NetCache.NetCacheBattlegroundsBoardSkins>();
		int numberOfOwnedBattlegroundsBoardSkins = 0;
		List<int> allFavoriteBattlegroundsBoardSkins = new List<int>();
		if (netCacheBGBoardSkins != null)
		{
			numberOfOwnedBattlegroundsBoardSkins = netCacheBGBoardSkins.OwnedBattlegroundsBoardSkins.Count;
			foreach (BattlegroundsBoardSkinId favoriteBattlegroundsBoardSkinId in netCacheBGBoardSkins.BattlegroundsFavoriteBoardSkins)
			{
				if (favoriteBattlegroundsBoardSkinId.IsValid())
				{
					allFavoriteBattlegroundsStrikeIds.Add(favoriteBattlegroundsBoardSkinId.ToValue());
				}
			}
		}
		NetCache.NetCacheBattlegroundsEmotes netCacheBGEmotes = NetCache.Get().GetNetObject<NetCache.NetCacheBattlegroundsEmotes>();
		int numberOfOwnedBattlegroundsEmotes = 0;
		List<int> allEquippedBattlegroundsEmotes = new List<int>();
		if (netCacheBGEmotes != null)
		{
			numberOfOwnedBattlegroundsEmotes = netCacheBGEmotes.OwnedBattlegroundsEmotes.Count;
			BattlegroundsEmoteId[] emotes = netCacheBGEmotes.CurrentLoadout.Emotes;
			for (int i = 0; i < emotes.Length; i++)
			{
				BattlegroundsEmoteId battlegroundsEmoteId = emotes[i];
				if (battlegroundsEmoteId.IsValid())
				{
					allEquippedBattlegroundsEmotes.Add(battlegroundsEmoteId.ToValue());
				}
			}
		}
		TelemetryManager.Client().SendBattlegroundsCollectionResult(triggerEvent, numberOfOwnedBattlegroundsHeroSkins, allFavoritedBaseHeroCardIds, allFavoriteBattlegroundsHeroSkinIds, numberOfOwnedBattlegroundsStrikes, allFavoriteBattlegroundsStrikeIds, numberOfOwnedBattlegroundsGuideSkins, allFavoriteBattlegroundsGuideSkins, numberOfOwnedBattlegroundsBoardSkins, allFavoriteBattlegroundsBoardSkins, numberOfOwnedBattlegroundsEmotes, allEquippedBattlegroundsEmotes);
	}
}
