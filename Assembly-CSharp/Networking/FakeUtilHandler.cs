using System;
using PegasusUtil;

namespace Networking;

internal static class FakeUtilHandler
{
	internal static bool FakeUtilOutbound(int type, IProtoBuf body)
	{
		bool success = true;
		switch (type)
		{
		case 201:
		case 240:
			FakeProcessPacket(type, body);
			break;
		case 327:
			((GenericRequestList)body).Requests.ForEach(delegate(GenericRequest request)
			{
				success = FakeProcessPacket(request.RequestId, body, request.RequestSubId) && success;
			});
			break;
		default:
			success = false;
			break;
		case 239:
		case 276:
		case 284:
		case 305:
			break;
		}
		return success;
	}

	private static bool FakeProcessPacket(int type, IProtoBuf body, int subId = 0)
	{
		bool success = true;
		switch (type)
		{
		case 201:
			if (subId == 0)
			{
				subId = (int)((body is GetAccountInfo accountInfo) ? accountInfo.Request_ : ((GetAccountInfo.Request)0));
			}
			success = FakeUtilOutboundGetAccountInfo((GetAccountInfo.Request)subId);
			break;
		case 340:
			Network.Get().FakeHandleType(ClientStaticAssetsResponse.PacketID.ID);
			break;
		default:
			Log.Net.PrintWarning("FakeUtilOutbound: unable to simulate response for requestId={0} subId={1}", type, subId);
			break;
		}
		return success;
	}

	private static bool FakeUtilOutboundGetAccountInfo(GetAccountInfo.Request request)
	{
		Enum responsePacketId = null;
		switch (request)
		{
		case GetAccountInfo.Request.DECK_LIST:
			responsePacketId = DeckList.PacketID.ID;
			break;
		case GetAccountInfo.Request.MEDAL_INFO:
			responsePacketId = MedalInfo.PacketID.ID;
			break;
		case GetAccountInfo.Request.CARD_BACKS:
			responsePacketId = CardBacks.PacketID.ID;
			break;
		case GetAccountInfo.Request.PLAYER_RECORD:
			responsePacketId = PlayerRecords.PacketID.ID;
			break;
		case GetAccountInfo.Request.DECK_LIMIT:
			responsePacketId = ProfileDeckLimit.PacketID.ID;
			break;
		case GetAccountInfo.Request.CAMPAIGN_INFO:
			responsePacketId = ProfileProgress.PacketID.ID;
			break;
		case GetAccountInfo.Request.CARD_VALUES:
			responsePacketId = CardValues.PacketID.ID;
			break;
		case GetAccountInfo.Request.FEATURES:
			responsePacketId = GuardianVars.PacketID.ID;
			break;
		case GetAccountInfo.Request.REWARD_PROGRESS:
			responsePacketId = RewardProgress.PacketID.ID;
			break;
		case GetAccountInfo.Request.HERO_XP:
			responsePacketId = HeroXP.PacketID.ID;
			break;
		case GetAccountInfo.Request.TAVERN_BRAWL_INFO:
			responsePacketId = TavernBrawlInfo.PacketID.ID;
			break;
		case GetAccountInfo.Request.TAVERN_BRAWL_RECORD:
			responsePacketId = TavernBrawlPlayerRecordResponse.PacketID.ID;
			break;
		case GetAccountInfo.Request.FAVORITE_HEROES:
			responsePacketId = FavoriteHeroesResponse.PacketID.ID;
			break;
		case GetAccountInfo.Request.ACCOUNT_LICENSES:
			responsePacketId = AccountLicensesInfoResponse.PacketID.ID;
			break;
		case GetAccountInfo.Request.COINS:
			responsePacketId = CosmeticCoins.PacketID.ID;
			break;
		case GetAccountInfo.Request.BATTLEGROUNDS_SKINS:
			responsePacketId = BattlegroundsHeroSkinsResponse.PacketID.ID;
			break;
		case GetAccountInfo.Request.BATTLEGROUNDS_GUIDE_SKINS:
			responsePacketId = BattlegroundsGuideSkinsResponse.PacketID.ID;
			break;
		case GetAccountInfo.Request.BATTLEGROUNDS_BOARD_SKINS:
			responsePacketId = BattlegroundsBoardSkinsResponse.PacketID.ID;
			break;
		case GetAccountInfo.Request.BATTLEGROUNDS_FINISHERS:
			responsePacketId = BattlegroundsFinishersResponse.PacketID.ID;
			break;
		case GetAccountInfo.Request.BATTLEGROUNDS_EMOTES:
			responsePacketId = BattlegroundsEmotesResponse.PacketID.ID;
			break;
		}
		if (responsePacketId != null)
		{
			return Network.Get().FakeHandleType(responsePacketId);
		}
		return false;
	}
}
