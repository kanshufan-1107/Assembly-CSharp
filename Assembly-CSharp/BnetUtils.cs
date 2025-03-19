using Blizzard.GameService.SDK.Client.Integration;
using PegasusShared;
using UnityEngine;

public static class BnetUtils
{
	public static BnetPlayer GetPlayer(BnetAccountId id)
	{
		if (id == null)
		{
			return null;
		}
		BnetPlayer player = BnetNearbyPlayerMgr.Get().FindNearbyStranger(id);
		if (player == null)
		{
			player = BnetPresenceMgr.Get().GetPlayer(id);
		}
		return player;
	}

	public static BnetPlayer GetPlayer(BnetGameAccountId id)
	{
		if (id == null)
		{
			return null;
		}
		BnetPlayer player = BnetNearbyPlayerMgr.Get().FindNearbyStranger(id);
		if (player == null)
		{
			player = BnetPresenceMgr.Get().GetPlayer(id);
		}
		return player;
	}

	public static string GetPlayerBestName(BnetGameAccountId id)
	{
		string name = GetPlayer(id)?.GetBestName();
		if (string.IsNullOrEmpty(name))
		{
			name = GameStrings.Get("GLOBAL_PLAYER_PLAYER");
		}
		return name;
	}

	public static bool HasPlayerBestNamePresence(BnetGameAccountId id)
	{
		return !string.IsNullOrEmpty(GetPlayer(id)?.GetBestName());
	}

	public static string GetInviterBestName(PartyInvite invite)
	{
		if (invite != null && !string.IsNullOrEmpty(invite.InviterName))
		{
			return invite.InviterName;
		}
		string name = ((invite == null) ? null : GetPlayer(invite.InviterId))?.GetBestName();
		if (string.IsNullOrEmpty(name))
		{
			name = GameStrings.Get("GLOBAL_PLAYER_PLAYER");
		}
		return name;
	}

	public static bool CanReceiveWhisperFrom(BnetAccountId id)
	{
		if (BnetPresenceMgr.Get().GetMyPlayer().IsBusy())
		{
			return false;
		}
		if (BnetFriendMgr.Get().IsFriend(id))
		{
			return true;
		}
		return false;
	}

	public static BnetPartyId CreatePartyId(BnetId protoEntityId)
	{
		return new BnetPartyId(protoEntityId.Hi, protoEntityId.Lo);
	}

	public static BnetId CreatePegasusBnetId(BnetPartyId partyId)
	{
		BnetId bnetId = new BnetId();
		BnetEntityId bnetPartyEntityId = partyId.ToBnetEntityId();
		bnetId.Hi = bnetPartyEntityId.High;
		bnetId.Lo = bnetPartyEntityId.Low;
		return bnetId;
	}

	public static BnetId CreatePegasusBnetId(BnetEntityId src)
	{
		return new BnetId
		{
			Hi = src.High,
			Lo = src.Low
		};
	}

	public static string GetNameForProgramId(BnetProgramId programId)
	{
		string nameTag = BnetProgramId.GetNameTag(programId);
		if (nameTag != null)
		{
			return GameStrings.Get(nameTag);
		}
		return null;
	}

	public static ulong? TryGetGameAccountId()
	{
		if (!BattleNet.IsInitialized())
		{
			return null;
		}
		return BattleNet.GetMyGameAccountId().Low;
	}

	public static ulong? TryGetBnetAccountId()
	{
		if (!BattleNet.IsInitialized())
		{
			return null;
		}
		return BattleNet.GetMyAccoundId().Low;
	}

	public static BnetRegion? TryGetBnetRegion()
	{
		if (!BattleNet.IsInitialized())
		{
			return null;
		}
		return BattleNet.GetAccountRegion();
	}

	public static BnetRegion? TryGetGameRegion()
	{
		if (!BattleNet.IsInitialized())
		{
			return null;
		}
		return BattleNet.GetCurrentRegion();
	}

	public static bool IsPlayerPartOfSamplingPercentage(float samplingPercentage)
	{
		float? gameAccountId = TryGetGameAccountId();
		if (gameAccountId.HasValue)
		{
			if (gameAccountId.Value % 100f / 100f >= samplingPercentage)
			{
				return false;
			}
			return true;
		}
		Debug.LogError("Could Not Retrieve Game Account Id");
		return false;
	}
}
