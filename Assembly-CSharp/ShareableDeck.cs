using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using PegasusShared;
using PegasusUtil;

public class ShareableDeck
{
	public const int VersionNumberZero = 0;

	public const int VersionNumberOne = 1;

	public const int VersionNumberCurrent = 1;

	[CompilerGenerated]
	private int _003CVersionNumber_003Ek__BackingField;

	public const string CommentLinePrefix = "# ";

	public const string DeckNameLinePrefix = "###";

	public const char DeckCodeWithNameSeparator = '\n';

	public static readonly string DeckCodeWithNameFormat = "### {0}\n{1}";

	private int VersionNumber
	{
		[CompilerGenerated]
		set
		{
			_003CVersionNumber_003Ek__BackingField = value;
		}
	}

	public string DeckName { get; set; }

	public int HeroCardDbId { get; set; }

	public DeckContents DeckContents { get; set; }

	public FormatType FormatType { get; set; }

	public bool IsArenaDeck { get; set; }

	protected ShareableDeck()
	{
		VersionNumber = 1;
		DeckName = string.Empty;
		HeroCardDbId = 0;
		DeckContents = new DeckContents();
		FormatType = FormatType.FT_UNKNOWN;
		IsArenaDeck = false;
	}

	public ShareableDeck(string deckName, int heroCardDbId, DeckContents deckContents, FormatType formatType, bool isArenaDeck)
	{
		DeckName = deckName;
		HeroCardDbId = heroCardDbId;
		DeckContents = deckContents;
		FormatType = formatType;
		IsArenaDeck = isArenaDeck;
		VersionNumber = 1;
	}

	public static ShareableDeck DeserializeFromClipboard()
	{
		return Deserialize(ClipboardUtils.PastedStringFromClipboard);
	}

	public static ShareableDeck Deserialize(string pastedString)
	{
		if (string.IsNullOrEmpty(pastedString))
		{
			return null;
		}
		bool deckHasWildCards = false;
		ShareableDeck shareableDeck = new ShareableDeck();
		try
		{
			string deckName;
			string encodedString = ParseDataFromDeckString(pastedString, out deckName);
			if (string.IsNullOrEmpty(encodedString))
			{
				return null;
			}
			shareableDeck.DeckName = deckName;
			using MemoryStream stream = new MemoryStream(Convert.FromBase64String(encodedString));
			if (!IsValidEncodedDeckHeader(stream))
			{
				return null;
			}
			if (!DeserializeFromVersion((int)ProtocolParser.ReadUInt64(stream), shareableDeck, stream, ref deckHasWildCards))
			{
				return null;
			}
		}
		catch (Exception)
		{
			return null;
		}
		if (deckHasWildCards)
		{
			if (shareableDeck.FormatType == FormatType.FT_STANDARD)
			{
				shareableDeck.FormatType = FormatType.FT_WILD;
			}
		}
		else if (!CollectionManager.Get().ShouldAccountSeeStandardWild())
		{
			shareableDeck.FormatType = FormatType.FT_STANDARD;
		}
		return shareableDeck;
	}

	public static ShareableDeck ParseDeckCode(string input, out string deckName)
	{
		deckName = string.Empty;
		if (string.IsNullOrEmpty(input))
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
			if (string.IsNullOrEmpty(deckLine))
			{
				return null;
			}
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

	public static string GenerateDeckCodeMessage(string deckCode, string deckName = null)
	{
		if (string.IsNullOrWhiteSpace(deckName))
		{
			return deckCode;
		}
		return string.Format(DeckCodeWithNameFormat, deckName, deckCode);
	}

	public static TAG_CLASS ExtractClassFromDeck(ShareableDeck deck)
	{
		string heroCardDesignerId = GameUtils.TranslateDbIdToCardId(deck.HeroCardDbId);
		if (string.IsNullOrEmpty(heroCardDesignerId))
		{
			return TAG_CLASS.INVALID;
		}
		return DefLoader.Get().GetEntityDef(heroCardDesignerId).GetClass();
	}

	private static bool DeserializeFromVersion(int versionNumber, ShareableDeck shareableDeck, MemoryStream stream, ref bool deckHasWildCards)
	{
		return versionNumber switch
		{
			0 => DeserializeFromVersion_0(shareableDeck, stream, ref deckHasWildCards), 
			_ => DeserializeFromVersion_1(shareableDeck, stream, ref deckHasWildCards), 
		};
	}

	private static bool DeserializeFromVersion_0(ShareableDeck shareableDeck, MemoryStream stream, ref bool deckHasWildCards)
	{
		ulong countHeroes = ProtocolParser.ReadUInt64(stream);
		for (ulong i = 0uL; i < countHeroes; i++)
		{
			shareableDeck.HeroCardDbId = (int)ProtocolParser.ReadUInt64(stream);
		}
		if (!GameDbf.Card.HasRecord(shareableDeck.HeroCardDbId))
		{
			return false;
		}
		string heroCardDesignerId = GameUtils.TranslateDbIdToCardId(shareableDeck.HeroCardDbId);
		if (!DefLoader.Get().GetEntityDef(heroCardDesignerId).IsHeroSkin())
		{
			return false;
		}
		shareableDeck.FormatType = (FormatType)ProtocolParser.ReadUInt64(stream);
		if (shareableDeck.FormatType != FormatType.FT_WILD && shareableDeck.FormatType != FormatType.FT_STANDARD)
		{
			return false;
		}
		if (!Deserialize_ReadArrayOfCards(1, TAG_PREMIUM.NORMAL, shareableDeck, stream, ref deckHasWildCards))
		{
			return false;
		}
		if (!Deserialize_ReadArrayOfCards(1, TAG_PREMIUM.GOLDEN, shareableDeck, stream, ref deckHasWildCards))
		{
			return false;
		}
		ulong countMultipleCards = ProtocolParser.ReadUInt64(stream);
		for (uint i2 = 0u; i2 < countMultipleCards; i2++)
		{
			int cardDbId = (int)ProtocolParser.ReadUInt64(stream);
			ulong count = ProtocolParser.ReadUInt64(stream);
			if (!GameDbf.Card.HasRecord(cardDbId) || !GameUtils.IsCardCollectible(GameUtils.TranslateDbIdToCardId(cardDbId)))
			{
				return false;
			}
			if (GameUtils.IsWildCard(GameUtils.TranslateDbIdToCardId(cardDbId)))
			{
				deckHasWildCards = true;
			}
			DeckCardData item = new DeckCardData
			{
				Def = new PegasusShared.CardDef
				{
					Premium = 0,
					Asset = cardDbId
				},
				Qty = (int)count
			};
			shareableDeck.DeckContents.Cards.Add(item);
		}
		ulong countMultipleGoldenCards = ProtocolParser.ReadUInt64(stream);
		for (ulong i3 = 0uL; i3 < countMultipleGoldenCards; i3++)
		{
			int cardDbId2 = (int)ProtocolParser.ReadUInt64(stream);
			ulong count2 = ProtocolParser.ReadUInt64(stream);
			if (!GameDbf.Card.HasRecord(cardDbId2) || !GameUtils.IsCardCollectible(GameUtils.TranslateDbIdToCardId(cardDbId2)))
			{
				return false;
			}
			if (GameUtils.IsWildCard(GameUtils.TranslateDbIdToCardId(cardDbId2)))
			{
				deckHasWildCards = true;
			}
			DeckCardData item2 = new DeckCardData
			{
				Def = new PegasusShared.CardDef
				{
					Premium = 1,
					Asset = cardDbId2
				},
				Qty = (int)count2
			};
			shareableDeck.DeckContents.Cards.Add(item2);
		}
		return true;
	}

	private static bool DeserializeFromVersion_1(ShareableDeck shareableDeck, MemoryStream stream, ref bool deckHasWildCards)
	{
		ulong formatTypeLong = ProtocolParser.ReadUInt64(stream);
		if (formatTypeLong == 0)
		{
			return false;
		}
		bool isValidFormatType = false;
		foreach (FormatType value in Enum.GetValues(typeof(FormatType)))
		{
			if ((ulong)value == formatTypeLong)
			{
				isValidFormatType = true;
				break;
			}
		}
		if (!isValidFormatType)
		{
			return false;
		}
		shareableDeck.FormatType = (FormatType)formatTypeLong;
		ulong countHeroes = ProtocolParser.ReadUInt64(stream);
		for (ulong i = 0uL; i < countHeroes; i++)
		{
			shareableDeck.HeroCardDbId = (int)ProtocolParser.ReadUInt64(stream);
		}
		if (!GameDbf.Card.HasRecord(shareableDeck.HeroCardDbId))
		{
			return false;
		}
		string heroCardDesignerId = GameUtils.TranslateDbIdToCardId(shareableDeck.HeroCardDbId);
		EntityDef heroCard = DefLoader.Get().GetEntityDef(heroCardDesignerId);
		if (!heroCard.IsHeroSkin())
		{
			return false;
		}
		if (shareableDeck.FormatType == FormatType.FT_CLASSIC && !GameUtils.CLASSIC_ORDERED_HERO_CLASSES.Contains(heroCard.GetClass()))
		{
			return false;
		}
		if (!Deserialize_ReadArrayOfCards(1, TAG_PREMIUM.NORMAL, shareableDeck, stream, ref deckHasWildCards))
		{
			return false;
		}
		if (!Deserialize_ReadArrayOfCards(2, TAG_PREMIUM.NORMAL, shareableDeck, stream, ref deckHasWildCards))
		{
			return false;
		}
		ulong countMultipleCards = ProtocolParser.ReadUInt64(stream);
		for (uint i2 = 0u; i2 < countMultipleCards; i2++)
		{
			int cardDbId = (int)ProtocolParser.ReadUInt64(stream);
			ulong count = ProtocolParser.ReadUInt64(stream);
			if (!GameDbf.Card.HasRecord(cardDbId))
			{
				return false;
			}
			if (GameUtils.IsWildCard(GameUtils.TranslateDbIdToCardId(cardDbId)))
			{
				deckHasWildCards = true;
			}
			DeckCardData item = shareableDeck.DeckContents.Cards.FirstOrDefault((DeckCardData deckCardData) => deckCardData != null && deckCardData.Def != null && deckCardData.Def.Asset == cardDbId && deckCardData.Def.Premium == 0);
			if (item == null)
			{
				item = new DeckCardData
				{
					Def = new PegasusShared.CardDef
					{
						Premium = 0,
						Asset = cardDbId
					},
					Qty = (int)count
				};
			}
			else
			{
				item.Qty += (int)count;
			}
			shareableDeck.DeckContents.Cards.Add(item);
		}
		if (stream.ReadByte() == 1)
		{
			if (!Deserialize_SideBoardCards(1, TAG_PREMIUM.NORMAL, shareableDeck, stream, ref deckHasWildCards))
			{
				return false;
			}
			if (!Deserialize_SideBoardCards(2, TAG_PREMIUM.NORMAL, shareableDeck, stream, ref deckHasWildCards))
			{
				return false;
			}
			ulong countMultipleSideBoardCards = ProtocolParser.ReadUInt64(stream);
			for (uint i3 = 0u; i3 < countMultipleSideBoardCards; i3++)
			{
				ulong count2 = ProtocolParser.ReadUInt64(stream);
				int cardDbId2 = (int)ProtocolParser.ReadUInt64(stream);
				if (!GameDbf.Card.HasRecord(cardDbId2))
				{
					return false;
				}
				if (GameUtils.IsWildCard(GameUtils.TranslateDbIdToCardId(cardDbId2)))
				{
					deckHasWildCards = true;
				}
				SideBoardCardData item2 = shareableDeck.DeckContents.SideboardCards.FirstOrDefault((SideBoardCardData deckCardData) => deckCardData != null && deckCardData.Def != null && deckCardData.Def.Asset == cardDbId2 && deckCardData.Def.Premium == 0 && deckCardData.LinkedCardDbId != 0);
				if (item2 == null)
				{
					item2 = new SideBoardCardData
					{
						Def = new PegasusShared.CardDef
						{
							Premium = 0,
							Asset = cardDbId2
						},
						Qty = (int)count2
					};
				}
				else
				{
					item2.Qty += (int)count2;
				}
				item2.LinkedCardDbId = (int)ProtocolParser.ReadUInt64(stream);
				shareableDeck.DeckContents.SideboardCards.Add(item2);
			}
		}
		return true;
	}

	private static bool Deserialize_ReadArrayOfCards(int quantityPerCard, TAG_PREMIUM premium, ShareableDeck shareableDeck, MemoryStream stream, ref bool deckHasWildCards)
	{
		ulong countSingleCards = ProtocolParser.ReadUInt64(stream);
		for (ulong i = 0uL; i < countSingleCards; i++)
		{
			int cardDbId = (int)ProtocolParser.ReadUInt64(stream);
			if (!GameDbf.Card.HasRecord(cardDbId))
			{
				return false;
			}
			if (GameUtils.IsWildCard(GameUtils.TranslateDbIdToCardId(cardDbId)))
			{
				deckHasWildCards = true;
			}
			DeckCardData item = shareableDeck.DeckContents.Cards.FirstOrDefault((DeckCardData deckCardData) => deckCardData != null && deckCardData.Def != null && deckCardData.Def.Asset == cardDbId && deckCardData.Def.Premium == 0);
			if (item == null)
			{
				item = new DeckCardData
				{
					Def = new PegasusShared.CardDef
					{
						Premium = (int)premium,
						Asset = cardDbId
					},
					Qty = quantityPerCard
				};
			}
			else
			{
				item.Qty += quantityPerCard;
			}
			shareableDeck.DeckContents.Cards.Add(item);
		}
		return true;
	}

	private static bool Deserialize_SideBoardCards(int quantityPerCard, TAG_PREMIUM premium, ShareableDeck shareableDeck, MemoryStream stream, ref bool deckHasWildCards)
	{
		ulong countCards = ProtocolParser.ReadUInt64(stream);
		for (ulong i = 0uL; i < countCards; i++)
		{
			int cardDbId = (int)ProtocolParser.ReadUInt64(stream);
			if (!GameDbf.Card.HasRecord(cardDbId))
			{
				return false;
			}
			if (GameUtils.IsWildCard(GameUtils.TranslateDbIdToCardId(cardDbId)))
			{
				deckHasWildCards = true;
			}
			SideBoardCardData item = shareableDeck.DeckContents.SideboardCards.FirstOrDefault((SideBoardCardData deckCardData) => deckCardData != null && deckCardData.Def != null && deckCardData.Def.Asset == cardDbId && deckCardData.Def.Premium == 0);
			if (item == null)
			{
				item = new SideBoardCardData
				{
					Def = new PegasusShared.CardDef
					{
						Premium = (int)premium,
						Asset = cardDbId
					},
					Qty = quantityPerCard
				};
			}
			else
			{
				item.Qty += quantityPerCard;
			}
			item.LinkedCardDbId = (int)ProtocolParser.ReadUInt64(stream);
			shareableDeck.DeckContents.SideboardCards.Add(item);
		}
		return true;
	}

	public virtual string Serialize(bool includeComments = true)
	{
		string encodedString = SerializeToVersion(1);
		if (includeComments)
		{
			return GetDeckStringWithComments(encodedString);
		}
		return encodedString;
	}

	private string SerializeToVersion(int versionNumber)
	{
		return versionNumber switch
		{
			0 => SerializeToVersion_0(), 
			_ => SerializeToVersion_1(), 
		};
	}

	private string SerializeToVersion_0()
	{
		if (DeckContents == null)
		{
			return null;
		}
		byte[] blob = null;
		using (MemoryStream stream = new MemoryStream())
		{
			stream.WriteByte(0);
			ProtocolParser.WriteUInt64(stream, 0uL);
			ProtocolParser.WriteUInt64(stream, 1uL);
			ProtocolParser.WriteUInt64(stream, (ulong)HeroCardDbId);
			ProtocolParser.WriteUInt64(stream, Convert.ToUInt32(FormatType));
			int[] singleCardDbIds = (from d in DeckContents.Cards
				where d.Def.Premium == 0 && d.Qty == 1
				select d.Def.Asset).ToArray();
			int[] singleGoldenCardDbIds = (from d in DeckContents.Cards
				where d.Def.Premium == 1 && d.Qty == 1
				select d.Def.Asset).ToArray();
			DeckCardData[] multipleCardDbIds = DeckContents.Cards.Where((DeckCardData d) => d.Def.Premium == 0 && d.Qty != 1).ToArray();
			DeckCardData[] multipleGoldenCardDbIds = DeckContents.Cards.Where((DeckCardData d) => d.Def.Premium == 1 && d.Qty != 1).ToArray();
			Serialize_WriteArrayOfCards(singleCardDbIds, stream);
			Serialize_WriteArrayOfCards(singleGoldenCardDbIds, stream);
			ProtocolParser.WriteUInt64(stream, (ulong)multipleCardDbIds.Length);
			DeckCardData[] array = multipleCardDbIds;
			foreach (DeckCardData item in array)
			{
				ProtocolParser.WriteUInt64(stream, (ulong)item.Def.Asset);
				ProtocolParser.WriteUInt64(stream, (ulong)Math.Max(0, item.Qty));
			}
			ProtocolParser.WriteUInt64(stream, (ulong)multipleGoldenCardDbIds.Count());
			array = multipleGoldenCardDbIds;
			foreach (DeckCardData item2 in array)
			{
				ProtocolParser.WriteUInt64(stream, (ulong)item2.Def.Asset);
				ProtocolParser.WriteUInt64(stream, (ulong)Math.Max(0, item2.Qty));
			}
			blob = stream.ToArray();
		}
		return Convert.ToBase64String(blob);
	}

	private string SerializeToVersion_1()
	{
		if (DeckContents == null)
		{
			return null;
		}
		byte[] blob = null;
		using (MemoryStream stream = new MemoryStream())
		{
			stream.WriteByte(0);
			ProtocolParser.WriteUInt64(stream, 1uL);
			ProtocolParser.WriteUInt64(stream, Convert.ToUInt32(FormatType));
			ProtocolParser.WriteUInt64(stream, 1uL);
			ProtocolParser.WriteUInt64(stream, (ulong)HeroCardDbId);
			int[] singleCardDbIds = (from d in DeckContents.Cards
				where d.Qty == 1
				select d.Def.Asset into d
				orderby d
				select d).ToArray();
			int[] doubleCardDbIds = (from d in DeckContents.Cards
				where d.Qty == 2
				select d.Def.Asset into d
				orderby d
				select d).ToArray();
			DeckCardData[] multipleCardDbIds = (from d in DeckContents.Cards
				where d.Qty > 2
				orderby d.Def.Asset
				select d).ToArray();
			Serialize_WriteArrayOfCards(singleCardDbIds, stream);
			Serialize_WriteArrayOfCards(doubleCardDbIds, stream);
			ProtocolParser.WriteUInt64(stream, (ulong)multipleCardDbIds.Length);
			DeckCardData[] array = multipleCardDbIds;
			foreach (DeckCardData item in array)
			{
				ProtocolParser.WriteUInt64(stream, (ulong)item.Def.Asset);
				ProtocolParser.WriteUInt64(stream, (ulong)Math.Max(0, item.Qty));
			}
			if (DeckContents.SideboardCards != null && DeckContents.SideboardCards.Count > 0)
			{
				ProtocolParser.WriteBool(stream, val: true);
				Serialize_WriteSideBoardCards(stream, DeckContents);
			}
			else
			{
				ProtocolParser.WriteBool(stream, val: false);
			}
			blob = stream.ToArray();
		}
		return Convert.ToBase64String(blob);
	}

	private void Serialize_WriteSideBoardCards(MemoryStream stream, DeckContents deckContents)
	{
		SideBoardCardData[] singleCards = (from d in DeckContents.SideboardCards
			where d.Qty == 1
			orderby d.Def.Asset
			select d).ToArray();
		SideBoardCardData[] doubleCards = (from d in DeckContents.SideboardCards
			where d.Qty == 2
			orderby d.Def.Asset
			select d).ToArray();
		SideBoardCardData[] multipleCards = (from d in DeckContents.SideboardCards
			where d.Qty > 2
			orderby d.Def.Asset
			select d).ToArray();
		Serialize_WriteArrayOfSideBoardCards(singleCards, stream);
		Serialize_WriteArrayOfSideBoardCards(doubleCards, stream);
		ProtocolParser.WriteUInt64(stream, (ulong)multipleCards.Length);
		SideBoardCardData[] array = multipleCards;
		foreach (SideBoardCardData item in array)
		{
			ProtocolParser.WriteUInt64(stream, (ulong)Math.Max(0, item.Qty));
			ProtocolParser.WriteUInt64(stream, (ulong)item.Def.Asset);
			ProtocolParser.WriteUInt64(stream, (ulong)item.LinkedCardDbId);
		}
	}

	public void Serialize_WriteArrayOfCards(int[] cardDbIds, MemoryStream stream)
	{
		ProtocolParser.WriteUInt64(stream, (ulong)cardDbIds.Length);
		foreach (int cardDbId in cardDbIds)
		{
			ProtocolParser.WriteUInt64(stream, (ulong)cardDbId);
		}
	}

	public void Serialize_WriteArrayOfSideBoardCards(SideBoardCardData[] cards, MemoryStream stream)
	{
		ProtocolParser.WriteUInt64(stream, (ulong)cards.Length);
		foreach (SideBoardCardData card in cards)
		{
			ProtocolParser.WriteUInt64(stream, (ulong)card.Def.Asset);
			ProtocolParser.WriteUInt64(stream, (ulong)card.LinkedCardDbId);
		}
	}

	public override bool Equals(object obj)
	{
		ShareableDeck other = (ShareableDeck)obj;
		if (other == null)
		{
			return false;
		}
		if (FormatType != other.FormatType)
		{
			return false;
		}
		if (DeckContents == null && other.DeckContents != null)
		{
			return false;
		}
		if (DeckContents != null && other.DeckContents == null)
		{
			return false;
		}
		if (DeckContents == null && other.DeckContents == null)
		{
			return true;
		}
		Dictionary<int, int> selfDeckCounts = new Dictionary<int, int>();
		Dictionary<int, int> otherDeckCounts = new Dictionary<int, int>();
		for (int i = 0; i < DeckContents.Cards.Count; i++)
		{
			if (selfDeckCounts.ContainsKey(DeckContents.Cards[i].Def.Asset))
			{
				selfDeckCounts[DeckContents.Cards[i].Def.Asset] += DeckContents.Cards[i].Qty;
			}
			else
			{
				selfDeckCounts[DeckContents.Cards[i].Def.Asset] = DeckContents.Cards[i].Qty;
			}
		}
		for (int j = 0; j < other.DeckContents.Cards.Count; j++)
		{
			if (otherDeckCounts.ContainsKey(other.DeckContents.Cards[j].Def.Asset))
			{
				otherDeckCounts[other.DeckContents.Cards[j].Def.Asset] += other.DeckContents.Cards[j].Qty;
			}
			else
			{
				otherDeckCounts[other.DeckContents.Cards[j].Def.Asset] = other.DeckContents.Cards[j].Qty;
			}
		}
		if (selfDeckCounts.Count != otherDeckCounts.Count)
		{
			return false;
		}
		foreach (KeyValuePair<int, int> kv in selfDeckCounts)
		{
			if (!otherDeckCounts.ContainsKey(kv.Key) || selfDeckCounts[kv.Key] != otherDeckCounts[kv.Key])
			{
				return false;
			}
		}
		return true;
	}

	public override int GetHashCode()
	{
		if (DeckContents != null)
		{
			return DeckContents.GetHashCode() ^ HeroCardDbId.GetHashCode();
		}
		return 0;
	}

	private string GetDeckStringWithComments(string encodedDeck)
	{
		StringBuilder desc = new StringBuilder();
		string heroDesignerId = GameUtils.TranslateDbIdToCardId(HeroCardDbId);
		List<TAG_CLASS> classes = new List<TAG_CLASS>();
		DefLoader.Get().GetEntityDef(heroDesignerId).GetClasses(classes);
		string className = GameStrings.GetClassesName(classes);
		string formatType = GameStrings.GetFormatName(FormatType);
		if (!IsArenaDeck)
		{
			desc.AppendFormat("{0} {1}\n", "###", DeckName);
		}
		else
		{
			desc.AppendFormat("{0} {1}\n", "###", GameStrings.Get("GLUE_COLLECTION_DECK_COPY_COMMENT_HEADER_DECK_ARENA"));
			desc.AppendFormat("{0}{1}\n", "# ", GameStrings.Get("GLUE_COLLECTION_DECK_PASTE_COMMENT_ARENA_WARNING"));
		}
		desc.Append("# ").AppendFormat(GameStrings.Get("GLUE_COLLECTION_DECK_COPY_COMMENT_HEADER_CLASS"), className).Append("\n");
		desc.Append("# ").AppendFormat(GameStrings.Get("GLUE_COLLECTION_DECK_COPY_COMMENT_HEADER_FORMAT"), formatType).Append("\n");
		if (FormatType == FormatType.FT_STANDARD)
		{
			string commentBody = SetRotationManager.Get().GetActiveSetRotationYearLocalizedString();
			desc.Append("# ").Append(commentBody).Append("\n");
		}
		desc.Append("#\n");
		Dictionary<int, List<SideBoardCardData>> cardToSideboardMap = new Dictionary<int, List<SideBoardCardData>>();
		if (DeckContents.SideboardCards != null && DeckContents.SideboardCards.Count > 0)
		{
			foreach (SideBoardCardData card in DeckContents.SideboardCards)
			{
				if (!cardToSideboardMap.ContainsKey(card.LinkedCardDbId))
				{
					cardToSideboardMap.Add(card.LinkedCardDbId, new List<SideBoardCardData>());
				}
				cardToSideboardMap[card.LinkedCardDbId].Add(card);
			}
		}
		if (DeckContents != null)
		{
			foreach (DeckCardData deckCardData in DeckContents.Cards)
			{
				EntityDef cardDef = DefLoader.Get().GetEntityDef(deckCardData.Def.Asset);
				desc.AppendFormat("# {0}x ({1}) {2}\n", deckCardData.Qty, cardDef.GetCost(), cardDef.GetName());
				int cardId = GameUtils.TranslateCardIdToDbId(cardDef.GetCardId());
				if (!cardToSideboardMap.ContainsKey(cardId))
				{
					continue;
				}
				foreach (SideBoardCardData sideBoardCard in cardToSideboardMap[cardId])
				{
					EntityDef sideCardDef = DefLoader.Get().GetEntityDef(sideBoardCard.Def.Asset);
					desc.AppendFormat("#   {0}x ({1}) {2}\n", sideBoardCard.Qty, sideCardDef.GetCost(), sideCardDef.GetName());
				}
			}
		}
		desc.Append("# \n");
		desc.Append(encodedDeck + "\n");
		desc.Append("# \n");
		desc.Append("# " + GameStrings.Get("GLUE_COLLECTION_DECK_PASTE_COMMENT_INSTRUCTIONS") + "\n");
		return desc.ToString();
	}

	private static bool IsValidEncodedDeckHeader(Stream stream)
	{
		byte[] firstBytes = new byte[1];
		if (stream.Read(firstBytes, 0, firstBytes.Length) < firstBytes.Length)
		{
			return false;
		}
		int curPos = 0;
		if (firstBytes[curPos++] != 0)
		{
			return false;
		}
		return true;
	}

	protected static string ParseDataFromDeckString(string deckString, out string deckName)
	{
		string[] source = deckString.Split(new string[3]
		{
			Environment.NewLine,
			"\r",
			"\n"
		}, StringSplitOptions.RemoveEmptyEntries);
		string encodedDeckLine = source.FirstOrDefault((string s) => !s.Trim().StartsWith("#"));
		string deckNameCommentLine = source.FirstOrDefault((string s) => s.Trim().StartsWith("###"));
		deckName = string.Empty;
		if (!string.IsNullOrEmpty(deckNameCommentLine))
		{
			deckName = deckNameCommentLine.Replace("###", string.Empty);
			deckName = deckName.Trim();
		}
		int maxNameLength = CollectionDeck.DefaultMaxDeckNameCharacters;
		if (deckName.Length > maxNameLength)
		{
			deckName = deckName.Substring(0, maxNameLength);
		}
		return encodedDeckLine?.Trim();
	}
}
