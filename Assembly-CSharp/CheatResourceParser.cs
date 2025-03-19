using Blizzard.T5.Core.Utils;

public class CheatResourceParser
{
	public static bool TryParse(string[] args, out CheatResource resource, out string errMsg)
	{
		resource = null;
		errMsg = null;
		if (args.Length == 0)
		{
			errMsg = "Missing valid resource. You must specify one of the following valid resources: cards, gold, dust, tutorial, hero, pack, arenaticket, arena.";
			return false;
		}
		string[] resourceSplit = args[0].Split('=');
		switch (resourceSplit[0])
		{
		case "cards":
			resource = new FullCardCollectionCheatResource();
			return true;
		case "gold":
		{
			int? amount = null;
			if (resourceSplit.Length > 1)
			{
				if (!int.TryParse(resourceSplit[1], out var deltaAmount2))
				{
					errMsg = "Failed to parse gold amount. The amount must be a valid number.";
					return false;
				}
				amount = deltaAmount2;
			}
			resource = new GoldCheatResource
			{
				Amount = amount
			};
			return true;
		}
		case "dust":
		{
			int? dustAmount = null;
			if (resourceSplit.Length > 1)
			{
				if (!int.TryParse(resourceSplit[1], out var deltaAmount))
				{
					errMsg = "Failed to parse dust amount. The amount must be a valid number.";
					return false;
				}
				dustAmount = deltaAmount;
			}
			resource = new DustCheatResource
			{
				Amount = dustAmount
			};
			return true;
		}
		case "tutorial":
		{
			int? progressValue = null;
			if (resourceSplit.Length > 1)
			{
				if (!int.TryParse(resourceSplit[1], out var progress))
				{
					errMsg = "Failed to parse progress value. The amount must be a valid number.";
					return false;
				}
				progressValue = progress;
			}
			resource = new TutorialCheatResource
			{
				Progress = progressValue
			};
			return true;
		}
		case "arenaticket":
		{
			int? ticketCount = null;
			if (resourceSplit.Length > 1)
			{
				if (!int.TryParse(resourceSplit[1], out var count))
				{
					errMsg = "Failed to parse ticket count value. The amount must be a valid number.";
					return false;
				}
				ticketCount = count;
			}
			resource = new ArenaTicketCheatResource
			{
				TicketCount = ticketCount
			};
			return true;
		}
		case "arena":
		{
			int? arenaWins = null;
			int? arenaLosses = null;
			if (args.Length > 1)
			{
				string[] subArgs3 = args.Slice(1);
				MultiAttributeParser attributeParser3 = new MultiAttributeParser();
				if (!attributeParser3.load(subArgs3, out errMsg))
				{
					return false;
				}
				if (!attributeParser3.getIntAttribute("win", out arenaWins, out errMsg))
				{
					return false;
				}
				if (!attributeParser3.getIntAttribute("loss", out arenaLosses, out errMsg))
				{
					return false;
				}
			}
			resource = new ArenaCheatResource
			{
				Win = arenaWins,
				Loss = arenaLosses
			};
			return true;
		}
		case "pack":
		{
			int? packCount = null;
			int? typeID = null;
			if (args.Length > 1)
			{
				string[] subArgs2 = args.Slice(1);
				MultiAttributeParser attributeParser2 = new MultiAttributeParser();
				if (!attributeParser2.load(subArgs2, out errMsg))
				{
					return false;
				}
				if (!attributeParser2.getIntAttribute("count", out packCount, out errMsg))
				{
					return false;
				}
				if (!attributeParser2.getIntAttribute("typeID", out typeID, out errMsg))
				{
					return false;
				}
			}
			resource = new PackCheatResource
			{
				PackCount = packCount,
				TypeID = typeID
			};
			return true;
		}
		case "hero":
		{
			string className = null;
			string gameType = null;
			bool? golden = null;
			int? level = null;
			int? wins = null;
			if (args.Length > 1)
			{
				string[] subArgs = args.Slice(1);
				MultiAttributeParser attributeParser = new MultiAttributeParser();
				if (!attributeParser.load(subArgs, out errMsg))
				{
					return false;
				}
				attributeParser.getStringAttribute("class", out className);
				attributeParser.getStringAttribute("gametype", out gameType);
				if (!attributeParser.getIntAttribute("level", out level, out errMsg))
				{
					return false;
				}
				if (!attributeParser.getIntAttribute("wins", out wins, out errMsg))
				{
					return false;
				}
				if (!attributeParser.getBoolAttribute("golden", out golden, out errMsg))
				{
					return false;
				}
			}
			TAG_PREMIUM premium = ((golden.HasValue && golden.Value) ? TAG_PREMIUM.GOLDEN : TAG_PREMIUM.NORMAL);
			resource = new HeroCheatResource
			{
				ClassName = className,
				Level = level,
				Wins = wins,
				Gametype = gameType,
				Premium = premium
			};
			return true;
		}
		case "adventureownership":
			resource = new AllAdventureOwnershipCheatResource();
			return true;
		default:
			errMsg = "Missing valid resource. You must specify one of the following valid resources: cards, gold, dust, tutorial, hero, pack, arenaticket, arena.";
			return false;
		}
	}
}
