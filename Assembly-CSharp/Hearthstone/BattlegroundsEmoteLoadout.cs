using Hearthstone.DataModels;
using PegasusUtil;
using UnityEngine;

namespace Hearthstone;

public class BattlegroundsEmoteLoadout
{
	public BattlegroundsEmoteId[] Emotes = new BattlegroundsEmoteId[6];

	public BattlegroundsEmoteLoadout()
	{
	}

	public static BattlegroundsEmoteLoadout MakeFromNetwork(PegasusUtil.BattlegroundsEmoteLoadout sourceLoadout)
	{
		BattlegroundsEmoteLoadout returnValue = new BattlegroundsEmoteLoadout();
		BattlegroundsEmoteId?[] untrustedValues = new BattlegroundsEmoteId?[6]
		{
			BattlegroundsEmoteId.FromUntrustedValue(sourceLoadout.EmoteSlot0),
			BattlegroundsEmoteId.FromUntrustedValue(sourceLoadout.EmoteSlot1),
			BattlegroundsEmoteId.FromUntrustedValue(sourceLoadout.EmoteSlot2),
			BattlegroundsEmoteId.FromUntrustedValue(sourceLoadout.EmoteSlot3),
			BattlegroundsEmoteId.FromUntrustedValue(sourceLoadout.EmoteSlot4),
			BattlegroundsEmoteId.FromUntrustedValue(sourceLoadout.EmoteSlot5)
		};
		for (int i = 0; i < 6; i++)
		{
			if (!untrustedValues[i].HasValue)
			{
				Debug.LogError($"BattlegroundsEmoteLoadout.MakeFromNetwork encountered illegal emote ID at loadout slot {i}.");
				return null;
			}
			returnValue.Emotes[i] = untrustedValues[i].Value;
		}
		return returnValue;
	}

	public static BattlegroundsEmoteLoadout MakeFromDatamodel(BattlegroundsEmoteLoadoutDataModel sourceLoadout)
	{
		BattlegroundsEmoteLoadout returnValue = new BattlegroundsEmoteLoadout();
		BattlegroundsEmoteId?[] untrustedValues = new BattlegroundsEmoteId?[6]
		{
			BattlegroundsEmoteId.FromUntrustedValue(sourceLoadout.EmoteList[0].EmoteDbiId),
			BattlegroundsEmoteId.FromUntrustedValue(sourceLoadout.EmoteList[1].EmoteDbiId),
			BattlegroundsEmoteId.FromUntrustedValue(sourceLoadout.EmoteList[2].EmoteDbiId),
			BattlegroundsEmoteId.FromUntrustedValue(sourceLoadout.EmoteList[3].EmoteDbiId),
			BattlegroundsEmoteId.FromUntrustedValue(sourceLoadout.EmoteList[4].EmoteDbiId),
			BattlegroundsEmoteId.FromUntrustedValue(sourceLoadout.EmoteList[5].EmoteDbiId)
		};
		for (int i = 0; i < 6; i++)
		{
			if (!untrustedValues[i].HasValue)
			{
				Debug.LogError($"BattlegroundsEmoteLoadout.MakeFromDatamodel encountered illegal emote ID at loadout slot {i}.");
				return null;
			}
			returnValue.Emotes[i] = untrustedValues[i].Value;
		}
		return returnValue;
	}

	public BattlegroundsEmoteLoadout(BattlegroundsEmoteLoadout original)
	{
		for (int i = 0; i < 6; i++)
		{
			Emotes[i] = original.Emotes[i];
		}
	}

	public PegasusUtil.BattlegroundsEmoteLoadout ToNetwork()
	{
		return new PegasusUtil.BattlegroundsEmoteLoadout
		{
			EmoteSlot0 = Emotes[0].ToValue(),
			EmoteSlot1 = Emotes[1].ToValue(),
			EmoteSlot2 = Emotes[2].ToValue(),
			EmoteSlot3 = Emotes[3].ToValue(),
			EmoteSlot4 = Emotes[4].ToValue(),
			EmoteSlot5 = Emotes[5].ToValue()
		};
	}

	public bool Equals(BattlegroundsEmoteLoadout other)
	{
		return this == other;
	}

	public override bool Equals(object obj)
	{
		if (obj is BattlegroundsEmoteLoadout id)
		{
			return Equals(id);
		}
		return false;
	}

	public override int GetHashCode()
	{
		int hashCode = 0;
		for (int i = 0; i < 6; i++)
		{
			hashCode += Emotes[i].ToValue() << i * 4;
		}
		return hashCode;
	}

	public override string ToString()
	{
		string strVal = Emotes[0].ToString();
		for (int i = 1; i < 6; i++)
		{
			strVal = strVal + "," + Emotes[i];
		}
		return strVal;
	}

	public static bool operator ==(BattlegroundsEmoteLoadout left, BattlegroundsEmoteLoadout right)
	{
		if ((object)left == null && (object)right == null)
		{
			return true;
		}
		if ((object)left == null || (object)right == null)
		{
			return false;
		}
		for (int i = 0; i < 6; i++)
		{
			if (left.Emotes[i] != right.Emotes[i])
			{
				return false;
			}
		}
		return true;
	}

	public static bool operator !=(BattlegroundsEmoteLoadout left, BattlegroundsEmoteLoadout right)
	{
		return !left.Equals(right);
	}
}
