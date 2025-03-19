using System;
using System.Runtime.CompilerServices;
using System.Text;
using PegasusLettuce;

public class ShareableMercenariesTeam : ShareableDeck
{
	[CompilerGenerated]
	private int _003CVersionNumber_003Ek__BackingField;

	private int VersionNumber
	{
		[CompilerGenerated]
		set
		{
			_003CVersionNumber_003Ek__BackingField = value;
		}
	}

	public LettuceTeam Team { get; set; }

	private ShareableMercenariesTeam()
	{
		VersionNumber = 0;
	}

	public ShareableMercenariesTeam(LettuceTeam team)
	{
		Team = team;
		VersionNumber = 0;
	}

	public override string Serialize(bool includeComments = true)
	{
		string result = Convert.ToBase64String(ProtobufUtil.ToByteArray(LettuceTeam.Convert(Team)));
		if (includeComments)
		{
			result = ModifyWithComments(result);
		}
		return result;
	}

	public new static ShareableMercenariesTeam DeserializeFromClipboard()
	{
		return Deserialize(ClipboardUtils.PastedStringFromClipboard);
	}

	public new static ShareableMercenariesTeam Deserialize(string pastedString)
	{
		if (string.IsNullOrEmpty(pastedString))
		{
			return null;
		}
		ShareableMercenariesTeam shareable = new ShareableMercenariesTeam();
		try
		{
			string deckName;
			string encodedString = ShareableDeck.ParseDataFromDeckString(pastedString, out deckName);
			if (string.IsNullOrEmpty(encodedString))
			{
				return null;
			}
			shareable.DeckName = deckName;
			PegasusLettuce.LettuceTeam result = ProtobufUtil.ParseFrom<PegasusLettuce.LettuceTeam>(Convert.FromBase64String(encodedString));
			shareable.Team = LettuceTeam.Convert(result, initializeWithBase: false, checkOwnership: false);
			if (shareable.Team.TeamType == PegasusLettuce.LettuceTeam.Type.TYPE_INVALID)
			{
				return null;
			}
			return shareable;
		}
		catch (Exception)
		{
			return null;
		}
	}

	public new static ShareableMercenariesTeam ParseDeckCode(string input, out string deckName)
	{
		deckName = string.Empty;
		if (input == null)
		{
			return null;
		}
		if (input.Length <= "###".Length)
		{
			return null;
		}
		string deckCode = input;
		if (input.StartsWith("###"))
		{
			string deckLine = input.Remove(0, "###".Length + 1);
			deckLine = deckLine.TrimEnd();
			char[] splitChars = new char[2] { ' ', '\n' };
			int splitIndex = deckLine.LastIndexOfAny(splitChars);
			if (splitIndex < 0)
			{
				return null;
			}
			if (splitIndex > 0)
			{
				deckName = TextUtils.StripHTMLTags(deckLine.Substring(0, splitIndex));
			}
			deckCode = deckLine.Substring(splitIndex, deckLine.Length - splitIndex);
		}
		return Deserialize(deckCode);
	}

	private string ModifyWithComments(string encodedDeck)
	{
		StringBuilder desc = new StringBuilder();
		desc.Append("###").Append(" ").AppendLine(Team.Name);
		desc.AppendLine("# ");
		if (Team != null)
		{
			foreach (LettuceMercenary merc in Team.GetMercs())
			{
				desc.Append("# ").AppendLine(merc.m_mercName ?? "");
				if (!merc.IsEquipmentSlotUnassigned())
				{
					desc.Append("# ").Append("\t - ").AppendLine(merc.GetSlottedEquipment().GetCardName() ?? "");
				}
			}
		}
		desc.AppendLine("# ");
		desc.AppendLine(encodedDeck);
		desc.AppendLine("# ");
		desc.Append("# ").AppendLine(GameStrings.Get("GLUE_COLLECTION_DECK_PASTE_COMMENT_INSTRUCTIONS"));
		return desc.ToString();
	}

	public override bool Equals(object obj)
	{
		LettuceTeam otherTeam = ((ShareableMercenariesTeam)obj).Team;
		if (Team == null && otherTeam != null)
		{
			return false;
		}
		if (Team != null && otherTeam == null)
		{
			return false;
		}
		if (Team == null && otherTeam == null)
		{
			return true;
		}
		if (Team.GetMercCount() != otherTeam.GetMercCount())
		{
			return false;
		}
		foreach (LettuceMercenary merc in Team.GetMercs())
		{
			if (!otherTeam.IsMercInTeam(merc.ID))
			{
				return false;
			}
			if (!otherTeam.TryGetMerc(merc.ID, out var _))
			{
				return false;
			}
			LettuceMercenary.Loadout loadout = Team.GetLoadout(merc);
			LettuceMercenary.Loadout otherLoadout = otherTeam.GetLoadout(merc);
			if (loadout.m_equipmentRecord?.ID != otherLoadout.m_equipmentRecord?.ID)
			{
				return false;
			}
			if (loadout.m_artVariationRecord?.ID != otherLoadout.m_artVariationRecord?.ID)
			{
				return false;
			}
		}
		return true;
	}

	public override int GetHashCode()
	{
		if (Team != null)
		{
			return Team.GetHashCode();
		}
		return 0;
	}
}
