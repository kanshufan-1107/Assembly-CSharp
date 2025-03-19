using System;
using System.Collections.Generic;
using System.Linq;
using Assets;
using Blizzard.GameService.SDK.Client.Integration;
using Blizzard.T5.Services;
using Hearthstone;
using Hearthstone.DataModels;
using Hearthstone.DungeonCrawl;
using Hearthstone.Login;
using PegasusGame;
using PegasusLettuce;
using PegasusShared;
using UnityEngine;
using UnityEngine.Events;

public class GameUtils
{
	[Serializable]
	public class StringEvent : UnityEvent<string>
	{
	}

	public delegate void EmoteSoundLoaded(CardSoundSpell emoteObj);

	public delegate void LoadActorCallback(Actor actor);

	public class HeroSkinAchievements
	{
		public int Golden500Win { get; set; }

		public int Honored1kWin { get; set; }
	}

	private class LoadActorCallbackInfo
	{
		public DefLoader.DisposableFullDef fullDef;

		public TAG_PREMIUM premium;
	}

	public static StringEvent OnAnimationExitEvent = new StringEvent();

	private const int TAG_BANNED_IN_TWIST = 3108;

	private const int TAG_BANNED_IN_WILD = 2105;

	public static readonly TAG_CLASS[] ORDERED_HERO_CLASSES = new TAG_CLASS[11]
	{
		TAG_CLASS.DEATHKNIGHT,
		TAG_CLASS.DEMONHUNTER,
		TAG_CLASS.DRUID,
		TAG_CLASS.HUNTER,
		TAG_CLASS.MAGE,
		TAG_CLASS.PALADIN,
		TAG_CLASS.PRIEST,
		TAG_CLASS.ROGUE,
		TAG_CLASS.SHAMAN,
		TAG_CLASS.WARLOCK,
		TAG_CLASS.WARRIOR
	};

	public static readonly TAG_CLASS[] DEFAULT_HERO_CLASSES = new TAG_CLASS[11]
	{
		TAG_CLASS.DEATHKNIGHT,
		TAG_CLASS.DEMONHUNTER,
		TAG_CLASS.DRUID,
		TAG_CLASS.HUNTER,
		TAG_CLASS.MAGE,
		TAG_CLASS.PALADIN,
		TAG_CLASS.PRIEST,
		TAG_CLASS.ROGUE,
		TAG_CLASS.SHAMAN,
		TAG_CLASS.WARLOCK,
		TAG_CLASS.WARRIOR
	};

	public static readonly TAG_CLASS[] CLASSIC_ORDERED_HERO_CLASSES = new TAG_CLASS[9]
	{
		TAG_CLASS.DRUID,
		TAG_CLASS.HUNTER,
		TAG_CLASS.MAGE,
		TAG_CLASS.PALADIN,
		TAG_CLASS.PRIEST,
		TAG_CLASS.ROGUE,
		TAG_CLASS.SHAMAN,
		TAG_CLASS.WARLOCK,
		TAG_CLASS.WARRIOR
	};

	public static readonly Dictionary<TAG_CLASS, HeroSkinAchievements> HERO_SKIN_ACHIEVEMENTS = new Dictionary<TAG_CLASS, HeroSkinAchievements>
	{
		{
			TAG_CLASS.MAGE,
			new HeroSkinAchievements
			{
				Golden500Win = 179,
				Honored1kWin = 180
			}
		},
		{
			TAG_CLASS.PRIEST,
			new HeroSkinAchievements
			{
				Golden500Win = 196,
				Honored1kWin = 197
			}
		},
		{
			TAG_CLASS.WARLOCK,
			new HeroSkinAchievements
			{
				Golden500Win = 213,
				Honored1kWin = 214
			}
		},
		{
			TAG_CLASS.ROGUE,
			new HeroSkinAchievements
			{
				Golden500Win = 230,
				Honored1kWin = 231
			}
		},
		{
			TAG_CLASS.DRUID,
			new HeroSkinAchievements
			{
				Golden500Win = 247,
				Honored1kWin = 248
			}
		},
		{
			TAG_CLASS.DEMONHUNTER,
			new HeroSkinAchievements
			{
				Golden500Win = 264,
				Honored1kWin = 265
			}
		},
		{
			TAG_CLASS.DEATHKNIGHT,
			new HeroSkinAchievements
			{
				Golden500Win = 5520,
				Honored1kWin = 5521
			}
		},
		{
			TAG_CLASS.SHAMAN,
			new HeroSkinAchievements
			{
				Golden500Win = 281,
				Honored1kWin = 282
			}
		},
		{
			TAG_CLASS.HUNTER,
			new HeroSkinAchievements
			{
				Golden500Win = 298,
				Honored1kWin = 299
			}
		},
		{
			TAG_CLASS.PALADIN,
			new HeroSkinAchievements
			{
				Golden500Win = 315,
				Honored1kWin = 316
			}
		},
		{
			TAG_CLASS.WARRIOR,
			new HeroSkinAchievements
			{
				Golden500Win = 332,
				Honored1kWin = 333
			}
		}
	};

	private static ReactiveNetCacheObject<NetCache.NetCacheProfileProgress> s_profileProgress = ReactiveNetCacheObject<NetCache.NetCacheProfileProgress>.CreateInstance();

	public static string STARSHIP_LAUNCH_CARD_ID = "GDB_905";

	private static Comparison<BoosterDbfRecord> SortBoostersDescending = (BoosterDbfRecord a, BoosterDbfRecord b) => b.LatestExpansionOrder.CompareTo(a.LatestExpansionOrder);

	private const int RANKED_SEASON_ID_START = 6;

	private const int RANKED_SEASON_MONTH_START = 4;

	private const int RANKED_SEASON_YEAR_START = 2014;

	private const int MERCENARIES_SEASON_ID_START = 1;

	private const int MERCENARIES_SEASON_MONTH_START = 11;

	private const int MERCENARIES_SEASON_YEAR_START = 2021;

	public static string TranslateDbIdToCardId(int dbId, bool showWarning = false)
	{
		CardDbfRecord rec = GameDbf.Card.GetRecord(dbId);
		if (rec == null)
		{
			if (showWarning)
			{
				Log.All.PrintError("GameUtils.TranslateDbIdToCardId() - Failed to find card with database id {0} in the Card DBF.", dbId);
			}
			return null;
		}
		string cardId = rec.NoteMiniGuid;
		if (cardId == null)
		{
			if (showWarning)
			{
				Log.All.PrintError("GameUtils.TranslateDbIdToCardId() - Card with database id {0} has no NOTE_MINI_GUID field in the Card DBF.", dbId);
			}
			return null;
		}
		return cardId;
	}

	public static int TranslateCardIdToDbId(string cardId, bool showWarning = false)
	{
		CardDbfRecord rec = GetCardRecord(cardId);
		if (rec == null)
		{
			if (showWarning)
			{
				Log.All.PrintError("GameUtils.TranslateCardIdToDbId() - There is no card with NOTE_MINI_GUID {0} in the Card DBF.", cardId);
			}
			return 0;
		}
		return rec.ID;
	}

	public static bool IsCardCollectible(string cardId)
	{
		return GetCardTagValue(cardId, GAME_TAG.COLLECTIBLE) == 1;
	}

	public static bool IsCardInBattlegroundsPool(string cardId)
	{
		if (GetCardTagValue(cardId, GAME_TAG.IS_BACON_POOL_MINION) != 1)
		{
			return GetCardTagValue(cardId, GAME_TAG.BACON_HERO_CAN_BE_DRAFTED) == 1;
		}
		return true;
	}

	public static bool IsAdventureRotated(AdventureDbId adventureID)
	{
		return IsAdventureRotated(adventureID, DateTime.UtcNow);
	}

	public static bool IsAdventureRotated(AdventureDbId adventureID, DateTime utcTimestamp)
	{
		AdventureDbfRecord adventureRecord = GameDbf.Adventure.GetRecord((int)adventureID);
		if (adventureRecord == null)
		{
			return false;
		}
		return !EventTimingManager.Get().IsEventActive(adventureRecord.StandardEvent, utcTimestamp);
	}

	public static bool IsBoosterRotated(BoosterDbId boosterID, DateTime utcTimestamp)
	{
		BoosterDbfRecord boosterRecord = GameDbf.Booster.GetRecord((int)boosterID);
		if (boosterRecord == null)
		{
			return false;
		}
		return !EventTimingManager.Get().IsEventActive(boosterRecord.StandardEvent, utcTimestamp);
	}

	public static bool IsBoosterCatchupPack(BoosterDbId boosterID)
	{
		return GameDbf.Booster.GetRecord((int)boosterID)?.IsCatchupPack ?? false;
	}

	public static FormatType GetCardSetFormat(TAG_CARD_SET cardSet)
	{
		if (cardSet == TAG_CARD_SET.VANILLA)
		{
			return FormatType.FT_CLASSIC;
		}
		if (IsSetRotated(cardSet))
		{
			return FormatType.FT_WILD;
		}
		return FormatType.FT_STANDARD;
	}

	public static TAG_CARD_SET[] GetCardSetsInFormat(FormatType formatType)
	{
		TAG_CARD_SET[] cardSets = null;
		switch (formatType)
		{
		case FormatType.FT_CLASSIC:
			cardSets = GetClassicSets();
			break;
		case FormatType.FT_STANDARD:
			cardSets = GetStandardSets();
			break;
		case FormatType.FT_WILD:
			cardSets = GetAllWildPlayableSets();
			break;
		case FormatType.FT_TWIST:
			cardSets = GetTwistSets();
			break;
		}
		return cardSets;
	}

	public static bool IsCardSetValidForFormat(FormatType formatType, TAG_CARD_SET cardSet)
	{
		switch (formatType)
		{
		case FormatType.FT_CLASSIC:
			return IsClassicCardSet(cardSet);
		case FormatType.FT_WILD:
			if (!IsWildCardSet(cardSet))
			{
				return IsStandardCardSet(cardSet);
			}
			return true;
		case FormatType.FT_STANDARD:
			return IsStandardCardSet(cardSet);
		default:
			return false;
		}
	}

	public static bool IsCardValidForFormat(FormatType formatType, int cardDbId)
	{
		EntityDef def = DefLoader.Get().GetEntityDef(cardDbId);
		return IsCardValidForFormat(formatType, def);
	}

	public static bool IsCardValidForFormat(FormatType formatType, string cardId)
	{
		EntityDef def = DefLoader.Get().GetEntityDef(cardId);
		return IsCardValidForFormat(formatType, def);
	}

	public static bool IsCardValidForFormat(FormatType formatType, EntityDef def)
	{
		if (def != null)
		{
			return IsCardSetValidForFormat(formatType, def.GetCardSet());
		}
		return false;
	}

	public static bool IsWildCardSet(TAG_CARD_SET cardSet)
	{
		return GetCardSetFormat(cardSet) == FormatType.FT_WILD;
	}

	public static bool IsWildCard(int cardDbId)
	{
		return IsWildCard(DefLoader.Get().GetEntityDef(cardDbId));
	}

	public static bool IsWildCard(string cardId)
	{
		return IsWildCard(DefLoader.Get().GetEntityDef(cardId));
	}

	public static bool IsWildCard(EntityDef def)
	{
		if (def != null)
		{
			return IsWildCardSet(def.GetCardSet());
		}
		return false;
	}

	public static bool IsTwistCard(int cardDbId)
	{
		return IsTwistCard(DefLoader.Get().GetEntityDef(cardDbId));
	}

	public static bool IsTwistCard(string cardId)
	{
		return IsTwistCard(DefLoader.Get().GetEntityDef(cardId));
	}

	public static bool IsTwistCard(EntityDef def)
	{
		TAG_CARD_SET[] twistSets = GetTwistSets();
		if (def != null)
		{
			for (int i = 0; i < twistSets.Length; i++)
			{
				if (def.GetCardSet() == twistSets[i])
				{
					return true;
				}
			}
		}
		return false;
	}

	public static bool IsClassicCardSet(TAG_CARD_SET cardSet)
	{
		return GetCardSetFormat(cardSet) == FormatType.FT_CLASSIC;
	}

	public static bool IsClassicCard(int cardDbId)
	{
		return IsClassicCard(DefLoader.Get().GetEntityDef(cardDbId));
	}

	public static bool IsClassicCard(string cardId)
	{
		return IsClassicCard(DefLoader.Get().GetEntityDef(cardId));
	}

	public static bool IsClassicCard(EntityDef def)
	{
		if (def != null)
		{
			return IsClassicCardSet(def.GetCardSet());
		}
		return false;
	}

	public static bool IsCoreCard(string cardId)
	{
		return IsCoreCard(DefLoader.Get().GetEntityDef(cardId));
	}

	public static bool IsCoreCard(EntityDef def)
	{
		return def?.IsCoreCard() ?? false;
	}

	public static bool IsStandardCardSet(TAG_CARD_SET cardSet)
	{
		return GetCardSetFormat(cardSet) == FormatType.FT_STANDARD;
	}

	public static bool IsStandardCard(int cardDbId)
	{
		return IsStandardCard(DefLoader.Get().GetEntityDef(cardDbId));
	}

	public static bool IsStandardCard(string cardId)
	{
		return IsStandardCard(DefLoader.Get().GetEntityDef(cardId));
	}

	public static bool IsStandardCard(EntityDef def)
	{
		if (def != null)
		{
			return IsStandardCardSet(def.GetCardSet());
		}
		return false;
	}

	public static string GetCardSetFormatAsString(TAG_CARD_SET cardSet)
	{
		return GetCardSetFormat(cardSet).ToString().Replace("FT_", "");
	}

	public static bool IsSetRotated(TAG_CARD_SET set)
	{
		return IsSetRotated(set, DateTime.UtcNow);
	}

	public static bool IsSetRotated(TAG_CARD_SET set, DateTime utcTimestamp)
	{
		CardSetDbfRecord setRecord = GameDbf.GetIndex().GetCardSet(set);
		if (setRecord == null)
		{
			return false;
		}
		EventTimingManager eventTimingManager = EventTimingManager.Get();
		if (!eventTimingManager.IsEventActive(setRecord.StandardEvent, utcTimestamp))
		{
			return eventTimingManager.HasEventStarted(setRecord.StandardEvent);
		}
		return false;
	}

	public static bool IsCardRotated(int cardDbId)
	{
		return IsCardRotated(DefLoader.Get().GetEntityDef(cardDbId));
	}

	public static bool IsCardRotated(string cardId)
	{
		return IsCardRotated(DefLoader.Get().GetEntityDef(cardId));
	}

	public static bool IsCardRotated(EntityDef def)
	{
		return IsCardRotated(def, DateTime.UtcNow);
	}

	public static bool IsCardRotated(EntityDef def, DateTime utcTimestamp)
	{
		return IsSetRotated(def.GetCardSet(), utcTimestamp);
	}

	public static bool IsLegacyCard(string cardId)
	{
		return IsLegacyCard(DefLoader.Get().GetEntityDef(cardId));
	}

	public static bool IsLegacyCard(EntityDef def)
	{
		if (def != null)
		{
			return IsLegacySet(def.GetCardSet());
		}
		return false;
	}

	public static bool IsLegacySet(TAG_CARD_SET set)
	{
		return IsLegacySet(set, DateTime.UtcNow);
	}

	public static bool IsLegacySet(TAG_CARD_SET set, DateTime utcTimestamp)
	{
		CardSetDbfRecord setRecord = GameDbf.GetIndex().GetCardSet(set);
		if (setRecord == null)
		{
			return false;
		}
		return EventTimingManager.Get().IsEventActive(setRecord.LegacyCardSetEvent, utcTimestamp);
	}

	public static bool IsBannedByConstructedDenylist(CollectionDeck deck, string designerCardId)
	{
		if (!deck.IsConstructedDeck)
		{
			return false;
		}
		NetCache.NetCacheFeatures netObject = NetCache.Get().GetNetObject<NetCache.NetCacheFeatures>();
		int databaseID = TranslateCardIdToDbId(designerCardId);
		return netObject.ConstructedCardDenylist.Contains(databaseID);
	}

	public static bool IsBannedBySideBoardDenylist(CollectionDeck deck, string designerCardId)
	{
		NetCache.NetCacheFeatures netObject = NetCache.Get().GetNetObject<NetCache.NetCacheFeatures>();
		int databaseID = TranslateCardIdToDbId(designerCardId);
		return netObject.SideboardCardDenylist.Contains(databaseID);
	}

	public static bool IsBannedByTwistDenylist(CollectionDeck deck, string designerCardId)
	{
		if (!deck.IsTwistDeck)
		{
			return false;
		}
		NetCache.NetCacheFeatures netObject = NetCache.Get().GetNetObject<NetCache.NetCacheFeatures>();
		int databaseID = TranslateCardIdToDbId(designerCardId);
		return netObject.TwistCardDenylist.Contains(databaseID);
	}

	public static bool IsBannedByTwistDenylist(DeckTemplateDbfRecord deckTemplate)
	{
		return NetCache.Get().GetNetObject<NetCache.NetCacheFeatures>().TwistDeckTemplateDenylist.Contains(deckTemplate.ID);
	}

	public static bool IsBannedByDuelsDenylist(CollectionDeck deck, string designerCardId)
	{
		if (!deck.IsDuelsDeck)
		{
			return false;
		}
		NetCache.NetCacheFeatures netObject = NetCache.Get().GetNetObject<NetCache.NetCacheFeatures>();
		int databaseID = TranslateCardIdToDbId(designerCardId);
		return netObject.DuelsCardDenylist.Contains(databaseID);
	}

	public static bool IsBannedByStandardDenylist(CollectionDeck deck, string designerCardId)
	{
		if (!deck.IsStandardDeck)
		{
			return false;
		}
		NetCache.NetCacheFeatures netObject = NetCache.Get().GetNetObject<NetCache.NetCacheFeatures>();
		int databaseID = TranslateCardIdToDbId(designerCardId);
		return netObject.StandardCardDenylist.Contains(databaseID);
	}

	public static bool IsBannedByWildDenylist(CollectionDeck deck, string designerCardId)
	{
		if (!deck.IsWildDeck)
		{
			return false;
		}
		NetCache.NetCacheFeatures netObject = NetCache.Get().GetNetObject<NetCache.NetCacheFeatures>();
		int databaseID = TranslateCardIdToDbId(designerCardId);
		return netObject.WildCardDenylist.Contains(databaseID);
	}

	public static bool IsBannedByTavernBrawlDenylist(CollectionDeck deck, string designerCardId)
	{
		if (!deck.IsBrawlDeck)
		{
			return false;
		}
		NetCache.NetCacheFeatures netObject = NetCache.Get().GetNetObject<NetCache.NetCacheFeatures>();
		int databaseID = TranslateCardIdToDbId(designerCardId);
		return netObject.TavernBrawlCardDenylist.Contains(databaseID);
	}

	public static bool IsBanned(CollectionDeck deck, EntityDef def)
	{
		string cardId = def.GetCardId();
		if (!RankMgr.Get().IsCardBannedInCurrentLeague(def) && !IsBannedByDuelsDenylist(deck, cardId) && !IsBannedByConstructedDenylist(deck, cardId) && !IsBannedByTwistDenylist(deck, cardId) && !IsBannedByStandardDenylist(deck, cardId) && !IsBannedByWildDenylist(deck, cardId))
		{
			return IsBannedByTavernBrawlDenylist(deck, cardId);
		}
		return true;
	}

	public static bool IsOnVFXDenylist(string spellPath)
	{
		NetCache.NetCacheFeatures guardianVars = NetCache.Get().GetNetObject<NetCache.NetCacheFeatures>();
		if (guardianVars == null)
		{
			return false;
		}
		foreach (string item in guardianVars.VFXDenylist)
		{
			string[] vfxNameAndGuid = item.Split(':');
			string vfxGuid = "";
			if (vfxNameAndGuid.Length == 2)
			{
				vfxGuid = vfxNameAndGuid[1];
			}
			else
			{
				if (vfxNameAndGuid.Length != 1)
				{
					continue;
				}
				vfxGuid = vfxNameAndGuid[0];
			}
			string[] spellNameAndGuid = spellPath.Split(':');
			string spellGuid = "";
			if (spellNameAndGuid.Length == 2)
			{
				spellGuid = spellNameAndGuid[1];
			}
			else
			{
				if (spellNameAndGuid.Length != 1)
				{
					continue;
				}
				spellGuid = spellNameAndGuid[0];
			}
			if (vfxGuid == spellGuid)
			{
				return true;
			}
		}
		return false;
	}

	public static bool IsCardGameplayEventActive(EntityDef def)
	{
		return IsCardGameplayEventActive(def.GetCardId());
	}

	public static bool IsCardGameplayEventActive(string cardId)
	{
		CardDbfRecord cardRecord = GetCardRecord(cardId);
		if (cardRecord == null)
		{
			Debug.LogWarning($"GameUtils.IsCardGameplayEventActive could not find DBF record for card {cardId}");
			return false;
		}
		EventTimingType gameplayEvent = cardRecord.GameplayEvent;
		if (gameplayEvent == EventTimingType.UNKNOWN)
		{
			CardSetDbfRecord cardSetRecord = GetCardSetRecord(cardId);
			if (cardSetRecord != null)
			{
				gameplayEvent = cardSetRecord.ContentLaunchEvent;
			}
		}
		return EventTimingManager.Get().IsEventActive(gameplayEvent);
	}

	public static bool IsCardGameplayEventEverActive(EntityDef def)
	{
		return IsCardGameplayEventEverActive(def.GetCardId());
	}

	public static bool IsCardGameplayEventEverActive(string cardId)
	{
		CardDbfRecord cardRecord = GetCardRecord(cardId);
		if (cardRecord == null)
		{
			Debug.LogWarning($"GameUtils.IsCardGameplayEventActive could not find DBF record for card {cardId}");
			return false;
		}
		EventTimingType gameplayEvent = cardRecord.GameplayEvent;
		if (gameplayEvent == EventTimingType.UNKNOWN)
		{
			CardSetDbfRecord cardSetRecord = GetCardSetRecord(cardId);
			if (cardSetRecord != null)
			{
				gameplayEvent = cardSetRecord.ContentLaunchEvent;
			}
		}
		return gameplayEvent != EventTimingType.SPECIAL_EVENT_NEVER;
	}

	public static bool IsCardBannedInTwist(EntityDef def)
	{
		if (def == null)
		{
			Debug.LogError("Invalid entity used for checking Banned Tag");
			return false;
		}
		return def.GetTag(3108) > 0;
	}

	public static bool IsCardBannedInWild(EntityDef def)
	{
		if (def == null)
		{
			Debug.LogError("Invalid entity used for checking Banned Tag");
			return false;
		}
		return def.GetTag(2105) > 0;
	}

	public static bool IsCardSetFilterEventActive(string cardId)
	{
		CardSetDbfRecord cardSetRecord = GetCardSetRecord(cardId);
		if (cardSetRecord == null)
		{
			return false;
		}
		return EventTimingManager.Get().IsEventActive(cardSetRecord.SetFilterEvent);
	}

	public static bool IsCardCraftableWhenWild(string cardId)
	{
		EntityDef cardDef = DefLoader.Get().GetEntityDef(cardId);
		if (cardDef == null)
		{
			return false;
		}
		return GameDbf.GetIndex().GetCardSet(cardDef.GetCardSet())?.CraftableWhenWild ?? false;
	}

	public static bool DeckIncludesRotatedCards(int deckId)
	{
		DeckDbfRecord deckRec = GameDbf.Deck.GetRecord(deckId);
		if (deckRec == null)
		{
			Log.Decks.PrintWarning("DeckRuleset.IsDeckWild(): {0} is invalid deck id", deckId);
			return false;
		}
		foreach (DeckCardDbfRecord card in deckRec.Cards)
		{
			if (IsCardRotated(card.CardId))
			{
				return true;
			}
		}
		return false;
	}

	public static bool DeckIncludesTwistCards(int deckId)
	{
		DeckDbfRecord deckRec = GameDbf.Deck.GetRecord(deckId);
		if (deckRec == null)
		{
			Log.Decks.PrintWarning("DeckRuleset.IsDeckTwist(): {0} is invalid deck id", deckId);
			return false;
		}
		foreach (DeckCardDbfRecord card in deckRec.Cards)
		{
			if (!IsTwistCard(card.CardId))
			{
				return false;
			}
		}
		return true;
	}

	public static TAG_CARD_SET[] GetStandardSets()
	{
		List<TAG_CARD_SET> result = new List<TAG_CARD_SET>();
		foreach (TAG_CARD_SET set in CollectionManager.Get().GetDisplayableCardSets())
		{
			if (GetCardSetFormat(set) == FormatType.FT_STANDARD)
			{
				result.Add(set);
			}
		}
		return result.ToArray();
	}

	public static TAG_CARD_SET[] GetWildSets()
	{
		List<TAG_CARD_SET> result = new List<TAG_CARD_SET>();
		foreach (TAG_CARD_SET set in CollectionManager.Get().GetDisplayableCardSets())
		{
			if (GetCardSetFormat(set) == FormatType.FT_WILD)
			{
				result.Add(set);
			}
		}
		return result.ToArray();
	}

	public static TAG_CARD_SET[] GetAllWildPlayableSets()
	{
		List<TAG_CARD_SET> list = new List<TAG_CARD_SET>();
		list.AddRange(GetStandardSets());
		list.AddRange(GetWildSets());
		return list.ToArray();
	}

	public static TAG_CARD_SET[] GetLegacySets()
	{
		List<TAG_CARD_SET> result = new List<TAG_CARD_SET>();
		foreach (TAG_CARD_SET set in CollectionManager.Get().GetDisplayableCardSets())
		{
			if (IsLegacySet(set))
			{
				result.Add(set);
			}
		}
		return result.ToArray();
	}

	public static TAG_CARD_SET[] GetClassicSets()
	{
		return (from cardSet in CollectionManager.Get().GetDisplayableCardSets()
			where IsClassicCardSet(cardSet)
			select cardSet).ToArray();
	}

	public static TAG_CARD_SET[] GetTwistSets()
	{
		return GetSetsForDeckRuleset(DeckRuleset.GetRuleset(FormatType.FT_TWIST));
	}

	public static TAG_CARD_SET[] GetSetsForDeckRuleset(DeckRuleset ruleset)
	{
		List<TAG_CARD_SET> result = new List<TAG_CARD_SET>();
		if (ruleset != null)
		{
			List<TAG_CARD_SET> displayableSets = CollectionManager.Get().GetDisplayableCardSets();
			foreach (TAG_CARD_SET set in ruleset.GetAllowedCardSets())
			{
				if (displayableSets.Contains(set))
				{
					result.Add(set);
				}
			}
		}
		return result.ToArray();
	}

	public static List<TAG_CARD_SET> GetTwistSetsWithFilter(List<HiddenCardSetsDbfRecord> hiddenTwistSetsRecord)
	{
		TAG_CARD_SET[] twistSets = GetTwistSets();
		HashSet<TAG_CARD_SET> hiddenSets = new HashSet<TAG_CARD_SET>();
		List<TAG_CARD_SET> filteredSets = new List<TAG_CARD_SET>();
		if (hiddenTwistSetsRecord != null)
		{
			for (int j = 0; j < hiddenTwistSetsRecord.Count; j++)
			{
				hiddenSets.Add((TAG_CARD_SET)hiddenTwistSetsRecord[j].CardSetId);
			}
		}
		for (int i = 0; i < twistSets.Length; i++)
		{
			if (!hiddenSets.Contains(twistSets[i]))
			{
				filteredSets.Add(twistSets[i]);
			}
		}
		return filteredSets;
	}

	public static void FillTwistDataModelWithValidSets(TwistSeasonInfoDataModel dataModel, List<TAG_CARD_SET> validSets)
	{
		if (dataModel == null || validSets == null)
		{
			return;
		}
		int i = 0;
		while (i < validSets.Count)
		{
			ValidSetDataModel validSet = new ValidSetDataModel();
			validSet.SetNameLeft = GameStrings.GetCardSetName(validSets[i]);
			validSet.SetIconNameLeft = validSets[i].ToString();
			i++;
			if (i < validSets.Count)
			{
				validSet.SetNameRight = GameStrings.GetCardSetName(validSets[i]);
				validSet.SetIconNameRight = validSets[i].ToString();
				i++;
			}
			dataModel.TwistValidSets.Add(validSet);
		}
	}

	public static void FillTwistDataModelWithHeroicDecks(TwistSeasonInfoDataModel dataModel)
	{
		if (dataModel == null)
		{
			return;
		}
		List<CollectionDeck> heroicDecks = RankMgr.Get()?.GetCurrentTwistSeason()?.GetDecks();
		if (heroicDecks == null || heroicDecks.Count == 0)
		{
			return;
		}
		int i = 0;
		while (i < heroicDecks.Count)
		{
			TwistHeroicDeckRowDataModel row = new TwistHeroicDeckRowDataModel();
			TwistHeroicDeckDataModel heroicDeck = new TwistHeroicDeckDataModel();
			FillTwistHeroicDeckModelWithDeckChoices(heroicDeck, heroicDecks[i]);
			row.DeckLeft = heroicDeck;
			i++;
			if (i < heroicDecks.Count)
			{
				heroicDeck = new TwistHeroicDeckDataModel();
				FillTwistHeroicDeckModelWithDeckChoices(heroicDeck, heroicDecks[i]);
				row.DeckRight = heroicDeck;
				i++;
			}
			dataModel.TwistHeroicDeckRow.Add(row);
		}
		dataModel.DoesCurrentSeasonUseHeroicDecks = dataModel.TwistHeroicDeckRow.Count > 0;
	}

	public static void FillTwistHeroicDeckModelWithDeckChoices(TwistHeroicDeckDataModel heroicDeckDataModel, CollectionDeck deck)
	{
		if (heroicDeckDataModel != null && deck != null)
		{
			int heroCardId = TranslateCardIdToDbId(deck.HeroCardID);
			EntityDef heroEntityDef = DefLoader.Get().GetEntityDef(heroCardId);
			heroicDeckDataModel.Name = deck.Name;
			heroicDeckDataModel.HeroCard = new CardDataModel
			{
				CardId = deck.HeroCardID,
				Premium = TAG_PREMIUM.NORMAL,
				Class = heroEntityDef.GetClass()
			};
			heroicDeckDataModel.PassiveCard = new CardDataModel
			{
				CardId = GetHeroPassiveCardIdFromHero(deck.HeroCardID),
				Premium = heroicDeckDataModel.HeroCard.Premium
			};
			heroicDeckDataModel.IsDeckLocked = deck.DeckTemplate_HasUnownedRequirements(out var requiredCardId);
			int requiredCardDbId = TranslateCardIdToDbId(requiredCardId);
			EntityDef requiredEntityDef = DefLoader.Get().GetEntityDef(requiredCardDbId);
			string missingCardName = "Unknown";
			if (requiredEntityDef != null)
			{
				missingCardName = requiredEntityDef.GetName();
			}
			heroicDeckDataModel.RequiredCard = new CardDataModel
			{
				CardId = requiredCardId,
				Premium = TAG_PREMIUM.NORMAL
			};
			heroicDeckDataModel.RequiredDescription = GameStrings.Format("GLUE_DISABLED_DECK_MISSING_REQUIRED_CARD_DESC", missingCardName);
			heroicDeckDataModel.CardCount = deck.GetTotalCardCount();
		}
	}

	public static TAG_CLASS GetTagClassFromCardId(string cardId)
	{
		return DefLoader.Get().GetEntityDef(cardId)?.GetClass() ?? TAG_CLASS.INVALID;
	}

	public static TAG_CLASS GetTagClassFromCardDbId(int cardDbId)
	{
		return (TAG_CLASS)GameDbf.GetIndex().GetCardTagValue(cardDbId, GAME_TAG.CLASS);
	}

	public static int CountAllCollectibleCards()
	{
		return GameDbf.GetIndex().GetCollectibleCardCount();
	}

	public static List<string> GetAllCardIds()
	{
		return GameDbf.GetIndex().GetAllCardIds();
	}

	public static List<string> GetAllCollectibleCardIds()
	{
		return GameDbf.GetIndex().GetCollectibleCardIds();
	}

	public static List<int> GetAllCollectibleCardDbIds()
	{
		return GameDbf.GetIndex().GetCollectibleCardDbIds();
	}

	public static List<string> GetNonHeroSkinCollectibleCardIds()
	{
		List<string> ids = new List<string>();
		foreach (string cardId in GetAllCollectibleCardIds())
		{
			EntityDef entityDef = DefLoader.Get().GetEntityDef(cardId);
			if (entityDef != null && !entityDef.IsHeroSkin())
			{
				ids.Add(cardId);
			}
		}
		return ids;
	}

	public static List<string> GetNonHeroSkinAllCardIds()
	{
		List<string> ids = new List<string>();
		foreach (string cardId in GetAllCardIds())
		{
			EntityDef entityDef = DefLoader.Get().GetEntityDef(cardId);
			if (entityDef != null && !entityDef.IsHeroSkin() && entityDef.GetCardType() != TAG_CARDTYPE.ENCHANTMENT)
			{
				ids.Add(cardId);
			}
		}
		return ids;
	}

	public static CardDbfRecord GetCardRecord(string cardId)
	{
		if (cardId == null)
		{
			return null;
		}
		return GameDbf.GetIndex().GetCardRecord(cardId);
	}

	public static CardSetDbfRecord GetCardSetRecord(string cardId)
	{
		return GetCardSetRecord(GetCardSetFromCardID(cardId));
	}

	public static CardSetDbfRecord GetCardSetRecord(TAG_CARD_SET cardSetId)
	{
		if (cardSetId == TAG_CARD_SET.INVALID)
		{
			return null;
		}
		return GameDbf.GetIndex().GetCardSet(cardSetId);
	}

	public static int GetCardTagValue(string cardId, GAME_TAG tagId)
	{
		int cardDbId = TranslateCardIdToDbId(cardId);
		return GameDbf.GetIndex().GetCardTagValue(cardDbId, tagId);
	}

	public static bool GetCardHasTag(string cardId, GAME_TAG tagId)
	{
		int cardDbId = TranslateCardIdToDbId(cardId);
		return GameDbf.GetIndex().GetCardHasTag(cardDbId, tagId);
	}

	public static int GetCardTagValue(int cardDbId, GAME_TAG tagId)
	{
		return GameDbf.GetIndex().GetCardTagValue(cardDbId, tagId);
	}

	public static List<CounterpartCardsDbfRecord> GetCounterpartCards(int cardDbId)
	{
		return GetCounterpartCards(TranslateDbIdToCardId(cardDbId));
	}

	public static List<CounterpartCardsDbfRecord> GetCounterpartCards(string cardId)
	{
		CardDbfRecord cardRecord = GetCardRecord(cardId);
		if (cardRecord == null)
		{
			Debug.LogWarning($"GameUtils.GetCounterpartCards could not find DBF record for card {cardId}");
			return null;
		}
		return cardRecord.CounterpartCards;
	}

	public static List<string> GetCounterpartCardIds(string cardId)
	{
		List<CounterpartCardsDbfRecord> counterpartCards = GetCounterpartCards(cardId);
		if (counterpartCards == null)
		{
			return null;
		}
		List<string> result = new List<string>();
		foreach (CounterpartCardsDbfRecord counterpartCard in counterpartCards)
		{
			result.Add(counterpartCard.DeckEquivalentCardRecord.NoteMiniGuid);
		}
		return result;
	}

	public static string GetLegacyCounterpartCardId(string cardId, string fallback = null)
	{
		if (fallback == null)
		{
			fallback = cardId;
		}
		List<CounterpartCardsDbfRecord> counterpartCards = GetCounterpartCards(cardId);
		if (counterpartCards == null)
		{
			return fallback;
		}
		foreach (CounterpartCardsDbfRecord item in counterpartCards)
		{
			string counterpartCardId = item.DeckEquivalentCardRecord.NoteMiniGuid;
			if (IsLegacyCard(counterpartCardId))
			{
				return counterpartCardId;
			}
		}
		return fallback;
	}

	public static bool TryGetCardTagRecords(string cardId, out List<CardTagDbfRecord> tagDbfRecords)
	{
		int cardDbId = TranslateCardIdToDbId(cardId);
		return GameDbf.GetIndex().TryGetCardTagRecords(cardDbId, out tagDbfRecords);
	}

	public static string GetHeroPowerCardIdFromHero(string heroCardId)
	{
		int heroPowerId = GetCardTagValue(heroCardId, GAME_TAG.HERO_POWER);
		if (heroPowerId == 0)
		{
			return string.Empty;
		}
		return TranslateDbIdToCardId(heroPowerId);
	}

	public static string GetHeroPowerCardIdFromHero(int heroDbId)
	{
		if (GameDbf.Card.GetRecord(heroDbId) == null)
		{
			Debug.LogError($"GameUtils.GetHeroPowerCardIdFromHero() - failed to find record for heroDbId {heroDbId}");
			return string.Empty;
		}
		return TranslateDbIdToCardId(GetCardTagValue(heroDbId, GAME_TAG.HERO_POWER));
	}

	public static string GetHeroPassiveCardIdFromHero(string heroCardId)
	{
		int heroPassiveId = GetCardTagValue(heroCardId, GAME_TAG.HERO_PASSIVE_ID);
		if (heroPassiveId == 0)
		{
			return string.Empty;
		}
		return TranslateDbIdToCardId(heroPassiveId);
	}

	public static string GetCardIdFromHeroDbId(int heroDbId)
	{
		CardHeroDbfRecord heroRecord = GameDbf.CardHero.GetRecord(heroDbId);
		if (heroRecord == null)
		{
			Debug.LogError($"GameUtils.GetCardIdFromHeroDbId() - failed to find record for heroDbId {heroDbId}");
			return string.Empty;
		}
		return TranslateDbIdToCardId(heroRecord.CardId);
	}

	public static TAG_CARD_SET GetCardSetFromCardID(string cardID)
	{
		EntityDef entityDef = DefLoader.Get().GetEntityDef(cardID);
		if (entityDef == null)
		{
			Debug.LogError($"Null EntityDef in GetCardSetFromCardID() for {cardID}");
			return TAG_CARD_SET.INVALID;
		}
		return entityDef.GetCardSet();
	}

	public static int GetCardIdFromGuestHeroDbId(int guestHeroDbId)
	{
		GuestHeroDbfRecord guestHeroRecord = GameDbf.GuestHero.GetRecord(guestHeroDbId);
		if (guestHeroRecord == null)
		{
			Debug.LogError($"GameUtils.GetCardIdFromGuestHeroDbId() - failed to find record for guestHeroDbId {guestHeroDbId}");
			return 0;
		}
		return guestHeroRecord.CardId;
	}

	public static int GetFavoriteHeroCardDBIdFromClass(TAG_CLASS classTag)
	{
		string classCardId = CollectionManager.Get().GetRandomFavoriteHero(classTag, null)?.Name;
		if (string.IsNullOrEmpty(classCardId))
		{
			classCardId = CollectionManager.GetVanillaHero(classTag);
		}
		return TranslateCardIdToDbId(classCardId);
	}

	public static CardHeroDbfRecord GetCardHeroRecordForCardId(int cardId)
	{
		return GameDbf.GetIndex().GetCardHeroByCardId(cardId);
	}

	public static CardHero.HeroType? GetHeroType(CardRewardData cardRewardData)
	{
		return GetHeroType(cardRewardData.CardID);
	}

	public static CardHero.HeroType? GetHeroType(string cardId)
	{
		return GetHeroType(TranslateCardIdToDbId(cardId));
	}

	public static CardHero.HeroType? GetHeroType(int cardDbId)
	{
		return GetHeroType(GameDbf.Card.GetRecord(cardDbId));
	}

	public static CardHero.HeroType? GetHeroType(CardDbfRecord cardRecord)
	{
		return GetHeroType((cardRecord != null) ? GetCardHeroRecordForCardId(cardRecord.ID) : null);
	}

	public static CardHero.HeroType? GetHeroType(CardHeroDbfRecord heroRecord)
	{
		return heroRecord?.HeroType;
	}

	public static bool IsVanillaHero(string cardId)
	{
		return IsVanillaHero(TranslateCardIdToDbId(cardId));
	}

	public static bool IsVanillaHero(int cardDbId)
	{
		return IsVanillaHero(GameDbf.Card.GetRecord(cardDbId));
	}

	public static bool IsVanillaHero(CardDbfRecord cardRecord)
	{
		return IsVanillaHero((cardRecord != null) ? GetCardHeroRecordForCardId(cardRecord.ID) : null);
	}

	public static bool IsVanillaHero(CardHeroDbfRecord heroRecord)
	{
		return IsVanillaHero(GetHeroType(heroRecord));
	}

	public static bool IsVanillaHero(CardHero.HeroType? heroType)
	{
		return heroType == CardHero.HeroType.VANILLA;
	}

	public static bool IsBattlegroundsHero(string cardId)
	{
		return IsBattlegroundsHero(TranslateCardIdToDbId(cardId));
	}

	public static bool IsBattlegroundsHero(int cardDbId)
	{
		return IsBattlegroundsHero(GameDbf.Card.GetRecord(cardDbId));
	}

	public static bool IsBattlegroundsHero(CardDbfRecord cardRecord)
	{
		return IsBattlegroundsHero(GetCardHeroRecordForCardId(cardRecord.ID));
	}

	public static bool IsBattlegroundsHero(CardHeroDbfRecord heroRecord)
	{
		return IsBattlegroundsHero(GetHeroType(heroRecord));
	}

	public static bool IsBattlegroundsHero(CardHero.HeroType? heroType)
	{
		return heroType == CardHero.HeroType.BATTLEGROUNDS_HERO;
	}

	public static bool IsBattlegroundsGuide(string cardId)
	{
		return IsBattlegroundsGuide(TranslateCardIdToDbId(cardId));
	}

	public static bool IsBattlegroundsGuide(int cardDbId)
	{
		return IsBattlegroundsGuide(GameDbf.Card.GetRecord(cardDbId));
	}

	public static bool IsBattlegroundsGuide(CardDbfRecord cardRecord)
	{
		return IsBattlegroundsGuide((cardRecord != null) ? GetCardHeroRecordForCardId(cardRecord.ID) : null);
	}

	public static bool IsBattlegroundsGuide(CardHeroDbfRecord heroRecord)
	{
		return IsBattlegroundsGuide(GetHeroType(heroRecord));
	}

	public static bool IsBattlegroundsGuide(CardHero.HeroType? heroType)
	{
		return heroType == CardHero.HeroType.BATTLEGROUNDS_GUIDE;
	}

	public static string GetGalakrondCardIdByClass(TAG_CLASS classTag)
	{
		string cardId = "";
		switch (classTag)
		{
		case TAG_CLASS.PRIEST:
			cardId = "DRG_660";
			break;
		case TAG_CLASS.ROGUE:
			cardId = "DRG_610";
			break;
		case TAG_CLASS.SHAMAN:
			cardId = "DRG_620";
			break;
		case TAG_CLASS.WARLOCK:
			cardId = "DRG_600";
			break;
		case TAG_CLASS.WARRIOR:
			cardId = "DRG_650";
			break;
		}
		return cardId;
	}

	public static NetCache.HeroLevel GetHeroLevel(TAG_CLASS heroClass)
	{
		return NetCache.Get().GetNetObject<NetCache.NetCacheHeroLevels>()?.Levels.Find((NetCache.HeroLevel obj) => obj.Class == heroClass);
	}

	public static int? GetTotalHeroLevel()
	{
		int? totalHeroLevel = null;
		NetCache.NetCacheHeroLevels heroLevels = NetCache.Get().GetNetObject<NetCache.NetCacheHeroLevels>();
		if (heroLevels != null)
		{
			totalHeroLevel = 0;
			foreach (NetCache.HeroLevel level in heroLevels.Levels)
			{
				totalHeroLevel += level.CurrentLevel.Level;
			}
		}
		else
		{
			Debug.LogError("GameUtils.GetTotalHeroLevel() - NetCache.NetCacheHeroLevels is null");
		}
		return totalHeroLevel;
	}

	public static bool HasUnlockedClass(TAG_CLASS heroClass)
	{
		return GetHeroLevel(heroClass) != null;
	}

	public static bool HasUnlockedClasses(List<TAG_CLASS> heroClasses)
	{
		foreach (TAG_CLASS heroClass in heroClasses)
		{
			if (!HasUnlockedClass(heroClass))
			{
				return false;
			}
		}
		return true;
	}

	public static bool HasEarnedAllCardsForClass(TAG_CLASS heroClass)
	{
		NetCache.HeroLevel classHeroLevel = GetHeroLevel(heroClass);
		if (classHeroLevel == null)
		{
			return false;
		}
		int nextHeroLevelWithReward;
		return FixedRewardsMgr.Get().GetNextHeroLevelReward(heroClass, classHeroLevel.CurrentLevel.Level, out nextHeroLevelWithReward) == null;
	}

	public static bool HasEarnedAllVanillaClassCards()
	{
		TAG_CLASS[] dEFAULT_HERO_CLASSES = DEFAULT_HERO_CLASSES;
		for (int i = 0; i < dEFAULT_HERO_CLASSES.Length; i++)
		{
			if (!HasEarnedAllCardsForClass(dEFAULT_HERO_CLASSES[i]))
			{
				return false;
			}
		}
		return true;
	}

	public static int CardPremiumSortComparisonAsc(TAG_PREMIUM premium1, TAG_PREMIUM premium2)
	{
		return premium1 - premium2;
	}

	public static int CardPremiumSortComparisonDesc(TAG_PREMIUM premium1, TAG_PREMIUM premium2)
	{
		return premium2 - premium1;
	}

	public static bool CanConcedeCurrentMission()
	{
		if (GameState.Get() == null)
		{
			return false;
		}
		if (GameMgr.Get().IsTraditionalTutorial())
		{
			if (NetCache.Get().GetNetObject<NetCache.NetCacheFeatures>().SkippableTutorialEnabled)
			{
				if (!(GameState.Get().GetGameEntity() is TutorialEntity tutorialEntity))
				{
					return false;
				}
				return tutorialEntity.IsCustomIntroFinished();
			}
			return false;
		}
		if (GameMgr.Get().IsSpectator())
		{
			return false;
		}
		if (GameMgr.Get().IsLettuceTutorial())
		{
			return false;
		}
		return true;
	}

	public static bool CanRestartCurrentMission(bool checkTutorial = true)
	{
		if (GameState.Get() == null)
		{
			return false;
		}
		if (GameState.Get().GetBooleanGameOption(GameEntityOption.DISABLE_RESTART_BUTTON))
		{
			return false;
		}
		if (checkTutorial && GameMgr.Get().IsTraditionalTutorial())
		{
			return false;
		}
		if (GameMgr.Get().IsSpectator())
		{
			return false;
		}
		if (!GameMgr.Get().IsAI())
		{
			return false;
		}
		if (!GameMgr.Get().HasLastPlayedDeckId())
		{
			return false;
		}
		if (!BattleNet.IsConnected())
		{
			return false;
		}
		if (DemoMgr.Get().IsDemo() && !DemoMgr.Get().CanRestartMissions())
		{
			return false;
		}
		if (GameMgr.Get().IsDungeonCrawlMission())
		{
			return false;
		}
		return true;
	}

	public static bool IsWaitingForOpponentReconnect()
	{
		if (GameState.Get() == null)
		{
			return false;
		}
		return GameState.Get().GetGameEntity().HasTag(GAME_TAG.WAIT_FOR_PLAYER_RECONNECT_PERIOD);
	}

	public static Card GetJoustWinner(Network.HistMetaData metaData)
	{
		if (metaData == null)
		{
			return null;
		}
		if (metaData.MetaType != HistoryMeta.Type.JOUST)
		{
			return null;
		}
		return GameState.Get().GetEntity(metaData.Data)?.GetCard();
	}

	public static bool GetJoustAllowTies(Network.HistMetaData metaData)
	{
		if (metaData == null)
		{
			return false;
		}
		if (metaData.MetaType != HistoryMeta.Type.JOUST)
		{
			return false;
		}
		if (metaData.AdditionalData.Count >= 2)
		{
			return metaData.AdditionalData[1] != 0;
		}
		return false;
	}

	public static bool IsHistoryDeathTagChange(Network.HistTagChange tagChange)
	{
		Entity entity = GameState.Get().GetEntity(tagChange.Entity);
		if (entity == null)
		{
			return false;
		}
		if (entity.IsEnchantment())
		{
			return false;
		}
		if (entity.GetCardType() == TAG_CARDTYPE.INVALID)
		{
			return false;
		}
		if (tagChange.Tag == 360 && tagChange.Value == 1)
		{
			return true;
		}
		if (entity.IsMinion() && tagChange.Tag == 49 && tagChange.Value == 4 && entity.GetZone() == TAG_ZONE.PLAY)
		{
			return true;
		}
		return false;
	}

	public static bool IsHistoryDiscardTagChange(Network.HistTagChange tagChange)
	{
		if (tagChange.Tag != 49)
		{
			return false;
		}
		if (GameState.Get().GetEntity(tagChange.Entity).GetZone() != TAG_ZONE.HAND)
		{
			return false;
		}
		if (tagChange.Value != 4)
		{
			return false;
		}
		return true;
	}

	public static bool IsHistoryMovedToSetAsideTagChange(Network.HistTagChange tagChange)
	{
		if (tagChange.Tag != 49)
		{
			return false;
		}
		if (tagChange.Value != 6)
		{
			return false;
		}
		return true;
	}

	public static bool IsEntityDeathTagChange(Network.HistTagChange tagChange)
	{
		if (tagChange.Tag != 49)
		{
			return false;
		}
		if (tagChange.Value != 4)
		{
			return false;
		}
		if (GameState.Get().GetEntity(tagChange.Entity) == null)
		{
			return false;
		}
		return true;
	}

	public static bool IsCharacterDeathTagChange(Network.HistTagChange tagChange)
	{
		if (tagChange.Tag != 49)
		{
			return false;
		}
		if (tagChange.Value != 4)
		{
			return false;
		}
		Entity entity = GameState.Get().GetEntity(tagChange.Entity);
		if (entity == null)
		{
			return false;
		}
		if (!entity.IsCharacter())
		{
			return false;
		}
		return true;
	}

	public static bool IsPreGameOverPlayState(TAG_PLAYSTATE playState)
	{
		if ((uint)(playState - 2) <= 1u || (uint)(playState - 7) <= 1u)
		{
			return true;
		}
		return false;
	}

	public static bool IsGameOverTag(int entityId, int tag, int val)
	{
		return IsGameOverTag(GameState.Get().GetEntity(entityId) as Player, tag, val);
	}

	public static bool IsGameOverTag(Player player, int tag, int val)
	{
		if (player == null)
		{
			return false;
		}
		if (tag != 17)
		{
			return false;
		}
		if (!player.IsFriendlySide() || !player.IsTeamLeader())
		{
			return false;
		}
		if ((uint)(val - 4) <= 2u)
		{
			return true;
		}
		return false;
	}

	public static bool IsFriendlyConcede(Network.HistTagChange tagChange)
	{
		if (tagChange.Tag != 17)
		{
			return false;
		}
		if (!(GameState.Get().GetEntity(tagChange.Entity) is Player player))
		{
			return false;
		}
		if (!player.IsFriendlySide())
		{
			return false;
		}
		return tagChange.Value == 8;
	}

	public static bool IsBeginPhase(TAG_STEP step)
	{
		if ((uint)step <= 4u)
		{
			return true;
		}
		return false;
	}

	public static bool IsPastBeginPhase(TAG_STEP step)
	{
		return !IsBeginPhase(step);
	}

	public static bool IsMainPhase(TAG_STEP step)
	{
		if ((uint)(step - 5) <= 8u || (uint)(step - 16) <= 4u)
		{
			return true;
		}
		return false;
	}

	public static List<Entity> GetEntitiesKilledBySourceAmongstTargets(int damageSourceID, List<Entity> targetEntities)
	{
		List<Entity> clonedTargetEntities = new List<Entity>();
		foreach (Entity target in targetEntities)
		{
			if (target != null)
			{
				clonedTargetEntities.Add(target.CloneForZoneMgr());
			}
		}
		List<Entity> killedEntities = new List<Entity>();
		PowerProcessor powerProcessor = GameState.Get().GetPowerProcessor();
		List<PowerTaskList> powerQueue = new List<PowerTaskList>();
		if (powerProcessor.GetCurrentTaskList() != null)
		{
			powerQueue.Add(powerProcessor.GetCurrentTaskList());
		}
		powerQueue.AddRange(powerProcessor.GetPowerQueue().GetList());
		for (int listIndex = 0; listIndex < powerQueue.Count; listIndex++)
		{
			List<PowerTask> taskList = powerQueue[listIndex].GetTaskList();
			for (int taskIndex = 0; taskIndex < taskList.Count; taskIndex++)
			{
				PowerTask task = taskList[taskIndex];
				Network.HistTagChange tagChange = task.GetPower() as Network.HistTagChange;
				if (tagChange == null)
				{
					continue;
				}
				if (tagChange.Tag == 18)
				{
					clonedTargetEntities.Find((Entity targetEntity) => targetEntity.GetEntityId() == tagChange.Entity)?.SetTag(18, tagChange.Value);
				}
				else if (tagChange.Tag == 49 && tagChange.Value == 4)
				{
					Entity modifiedEntity = clonedTargetEntities.Find((Entity targetEntity) => targetEntity.GetEntityId() == tagChange.Entity);
					if (modifiedEntity != null && modifiedEntity.GetTag(GAME_TAG.LAST_AFFECTED_BY) == damageSourceID)
					{
						killedEntities.Add(modifiedEntity);
					}
				}
			}
		}
		return killedEntities;
	}

	public static void ApplyPower(Entity entity, Network.PowerHistory power)
	{
		switch (power.Type)
		{
		case Network.PowerType.SHOW_ENTITY:
			ApplyShowEntity(entity, (Network.HistShowEntity)power);
			break;
		case Network.PowerType.HIDE_ENTITY:
			ApplyHideEntity(entity, (Network.HistHideEntity)power);
			break;
		case Network.PowerType.TAG_CHANGE:
			ApplyTagChange(entity, (Network.HistTagChange)power);
			break;
		}
	}

	public static void ApplyShowEntity(Entity entity, Network.HistShowEntity showEntity)
	{
		foreach (Network.Entity.Tag tag in showEntity.Entity.Tags)
		{
			entity.SetTag(tag.Name, tag.Value);
		}
	}

	public static void ApplyHideEntity(Entity entity, Network.HistHideEntity hideEntity)
	{
		entity.SetTag(GAME_TAG.ZONE, hideEntity.Zone);
	}

	public static void ApplyTagChange(Entity entity, Network.HistTagChange tagChange)
	{
		entity.SetTag(tagChange.Tag, tagChange.Value);
	}

	public static TAG_ZONE GetFinalZoneForEntity(Entity entity)
	{
		PowerProcessor powerProcessor = GameState.Get().GetPowerProcessor();
		List<PowerTaskList> powerQueue = new List<PowerTaskList>();
		if (powerProcessor.GetCurrentTaskList() != null)
		{
			powerQueue.Add(powerProcessor.GetCurrentTaskList());
		}
		powerQueue.AddRange(powerProcessor.GetPowerQueue().GetList());
		for (int listIndex = powerQueue.Count - 1; listIndex >= 0; listIndex--)
		{
			List<PowerTask> taskList = powerQueue[listIndex].GetTaskList();
			for (int taskIndex = taskList.Count - 1; taskIndex >= 0; taskIndex--)
			{
				if (taskList[taskIndex].GetPower() is Network.HistTagChange tagChange && tagChange.Entity == entity.GetEntityId() && (tagChange.Tag == 49 || tagChange.Tag == 1702))
				{
					return (TAG_ZONE)tagChange.Value;
				}
			}
		}
		TAG_ZONE fakeZone = entity.GetTag<TAG_ZONE>(GAME_TAG.FAKE_ZONE);
		if (fakeZone != 0)
		{
			return fakeZone;
		}
		return entity.GetZone();
	}

	public static bool IsEntityHiddenAfterCurrentTasklist(Entity entity)
	{
		if (!entity.IsHidden())
		{
			return false;
		}
		PowerProcessor powerProcessor = GameState.Get().GetPowerProcessor();
		if (powerProcessor.GetCurrentTaskList() != null)
		{
			foreach (PowerTask task in powerProcessor.GetCurrentTaskList().GetTaskList())
			{
				if (task.GetPower() is Network.HistShowEntity showEnt && showEnt.Entity.ID == entity.GetEntityId() && !string.IsNullOrEmpty(showEnt.Entity.CardID))
				{
					return false;
				}
			}
		}
		return true;
	}

	public static bool IsGalakrond(string cardId)
	{
		switch (cardId)
		{
		case "DRG_600":
		case "DRG_600t2":
		case "DRG_600t3":
		case "DRG_650":
		case "DRG_650t2":
		case "DRG_650t3":
		case "DRG_620":
		case "DRG_620t2":
		case "DRG_620t3":
		case "DRG_660":
		case "DRG_660t2":
		case "DRG_660t3":
		case "DRG_610":
		case "DRG_610t2":
		case "DRG_610t3":
			return true;
		default:
			return false;
		}
	}

	public static bool IsGalakrondInPlay(Player player)
	{
		if (player == null)
		{
			return false;
		}
		Entity hero = player.GetHero();
		if (hero == null)
		{
			return false;
		}
		return IsGalakrond(hero.GetCardId());
	}

	public static void DoDamageTasks(PowerTaskList powerTaskList, Card sourceCard, Card targetCard)
	{
		List<PowerTask> taskList = powerTaskList.GetTaskList();
		if (taskList == null || taskList.Count == 0)
		{
			return;
		}
		int sourceEntityId = sourceCard.GetEntity().GetEntityId();
		int targetEntityId = targetCard.GetEntity().GetEntityId();
		foreach (PowerTask task in taskList)
		{
			Network.PowerHistory power = task.GetPower();
			if (power.Type == Network.PowerType.META_DATA)
			{
				Network.HistMetaData metaData = (Network.HistMetaData)power;
				if (metaData.MetaType != HistoryMeta.Type.DAMAGE && metaData.MetaType != HistoryMeta.Type.HEALING)
				{
					continue;
				}
				foreach (int entityId in metaData.Info)
				{
					if (entityId == sourceEntityId || entityId == targetEntityId)
					{
						task.DoTask();
					}
				}
			}
			else
			{
				if (power.Type != Network.PowerType.TAG_CHANGE)
				{
					continue;
				}
				Network.HistTagChange tagChange = (Network.HistTagChange)power;
				if (tagChange.Entity == sourceEntityId || tagChange.Entity == targetEntityId)
				{
					GAME_TAG tag = (GAME_TAG)tagChange.Tag;
					if (tag == GAME_TAG.DAMAGE || tag == GAME_TAG.EXHAUSTED)
					{
						task.DoTask();
					}
				}
			}
		}
	}

	public static AdventureDbfRecord GetAdventureRecordFromMissionId(int missionId)
	{
		ScenarioDbfRecord missionRec = GameDbf.Scenario.GetRecord(missionId);
		if (missionRec == null)
		{
			return null;
		}
		int adventureId = missionRec.AdventureId;
		return GameDbf.Adventure.GetRecord(adventureId);
	}

	public static WingDbfRecord GetWingRecordFromMissionId(int missionId)
	{
		WingDbId wingId = GetWingIdFromMissionId((ScenarioDbId)missionId);
		if (wingId == WingDbId.INVALID)
		{
			return null;
		}
		return GameDbf.Wing.GetRecord((int)wingId);
	}

	public static WingDbId GetWingIdFromMissionId(ScenarioDbId missionId)
	{
		return (WingDbId)(GameDbf.Scenario.GetRecord((int)missionId)?.WingId ?? 0);
	}

	public static AdventureDataDbfRecord GetAdventureDataRecord(int adventureId, int modeId)
	{
		foreach (AdventureDataDbfRecord rec in GameDbf.AdventureData.GetRecords())
		{
			if (rec.AdventureId == adventureId && rec.ModeId == modeId)
			{
				return rec;
			}
		}
		return null;
	}

	public static List<ScenarioDbfRecord> GetClassChallengeRecords(int adventureId, int wingId)
	{
		List<ScenarioDbfRecord> records = new List<ScenarioDbfRecord>();
		foreach (ScenarioDbfRecord rec in GameDbf.Scenario.GetRecords())
		{
			if (rec.ModeId == 4 && rec.AdventureId == adventureId && rec.WingId == wingId)
			{
				records.Add(rec);
			}
		}
		return records;
	}

	public static TAG_CLASS GetClassChallengeHeroClass(ScenarioDbfRecord rec)
	{
		if (rec.ModeId != 4)
		{
			return TAG_CLASS.INVALID;
		}
		int cardId = rec.Player1HeroCardId;
		return DefLoader.Get().GetEntityDef(cardId)?.GetClass() ?? TAG_CLASS.INVALID;
	}

	public static List<TAG_CLASS> GetClassChallengeHeroClasses(int adventureId, int wingId)
	{
		List<ScenarioDbfRecord> classChallengeRecords = GetClassChallengeRecords(adventureId, wingId);
		List<TAG_CLASS> classes = new List<TAG_CLASS>();
		foreach (ScenarioDbfRecord rec in classChallengeRecords)
		{
			classes.Add(GetClassChallengeHeroClass(rec));
		}
		return classes;
	}

	public static bool IsAIMission(int missionId)
	{
		ScenarioDbfRecord rec = GameDbf.Scenario.GetRecord(missionId);
		if (rec == null)
		{
			return false;
		}
		if (rec.Players == 1)
		{
			return true;
		}
		return false;
	}

	public static bool IsCoopMission(int missionId)
	{
		return GameDbf.Scenario.GetRecord(missionId)?.IsCoop ?? false;
	}

	public static bool IsMercenariesMission(int missionid)
	{
		if (missionid != 3778 && missionid != 3900 && missionid != 3901 && missionid != 4067 && missionid != 3779 && missionid != 3744 && missionid != 3792 && missionid != 3790 && missionid != 3899)
		{
			return missionid == 3862;
		}
		return true;
	}

	public static string GetMissionHeroCardId(int missionId)
	{
		ScenarioDbfRecord rec = GameDbf.Scenario.GetRecord(missionId);
		if (rec == null)
		{
			return null;
		}
		int heroDbId = rec.ClientPlayer2HeroCardId;
		if (heroDbId == 0)
		{
			heroDbId = rec.Player2HeroCardId;
		}
		return TranslateDbIdToCardId(heroDbId);
	}

	public static string GetMissionHeroName(int missionId)
	{
		string cardId = GetMissionHeroCardId(missionId);
		if (cardId == null)
		{
			return null;
		}
		EntityDef entityDef = DefLoader.Get().GetEntityDef(cardId);
		if (entityDef == null)
		{
			Debug.LogError($"GameUtils.GetMissionHeroName() - hero {cardId} for mission {missionId} has no EntityDef");
			return null;
		}
		return entityDef.GetName();
	}

	public static string GetMissionHeroPowerCardId(int missionId)
	{
		ScenarioDbfRecord missionRec = GameDbf.Scenario.GetRecord(missionId);
		if (missionRec == null)
		{
			return null;
		}
		int heroPowerId = missionRec.ClientPlayer2HeroPowerCardId;
		if (heroPowerId != 0)
		{
			return TranslateDbIdToCardId(heroPowerId);
		}
		int heroId = missionRec.ClientPlayer2HeroCardId;
		if (heroId == 0)
		{
			heroId = missionRec.Player2HeroCardId;
		}
		return GetHeroPowerCardIdFromHero(heroId);
	}

	public static bool IsMissionForAdventure(int missionId, int adventureId)
	{
		ScenarioDbfRecord rec = GameDbf.Scenario.GetRecord(missionId);
		if (rec == null)
		{
			return false;
		}
		return adventureId == rec.AdventureId;
	}

	public static bool IsTutorialMission(int missionId)
	{
		return IsMissionForAdventure(missionId, 1);
	}

	public static bool IsPracticeMission(int missionId)
	{
		return IsMissionForAdventure(missionId, 2);
	}

	public static bool IsDungeonCrawlMission(int missionId)
	{
		ScenarioDbfRecord rec = GameDbf.Scenario.GetRecord(missionId);
		if (rec == null)
		{
			return false;
		}
		return DoesAdventureModeUseDungeonCrawlFormat((AdventureModeDbId)rec.ModeId);
	}

	public static bool DoesAdventureModeUseDungeonCrawlFormat(AdventureModeDbId modeId)
	{
		if (modeId != AdventureModeDbId.DUNGEON_CRAWL)
		{
			return modeId == AdventureModeDbId.DUNGEON_CRAWL_HEROIC;
		}
		return true;
	}

	public static bool IsBoosterLatestActiveExpansion(int boosterId)
	{
		return boosterId == (int)GetLatestRewardableBooster();
	}

	public static BoosterDbId GetLatestRewardableBooster()
	{
		return GetRewardableBoosterOffsetFromLatest(0);
	}

	public static BoosterDbId GetRewardableBoosterOffsetFromLatest(int offset)
	{
		List<BoosterDbfRecord> activeBoosters = GetRewardableBoosters();
		if (activeBoosters.Count <= 0)
		{
			Debug.LogError("No active Booster sets found");
			return BoosterDbId.INVALID;
		}
		offset = Mathf.Clamp(offset, 0, activeBoosters.Count - 1);
		return (BoosterDbId)activeBoosters[offset].ID;
	}

	public static BoosterDbId GetLatestCatchupPack()
	{
		BoosterDbfRecord returnRecord = null;
		DateTime? returnRecordStart = null;
		foreach (BoosterDbfRecord record in GameDbf.Booster.GetRecords())
		{
			if (!record.IsCatchupPack)
			{
				continue;
			}
			DateTime? showToUser = EventTimingManager.Get().GetEventStartTimeUtc(record.ShownToClientEvent);
			if (!showToUser.HasValue || showToUser > DateTime.UtcNow)
			{
				DateTime? start = EventTimingManager.Get().GetEventStartTimeUtc(record.OpenPackEvent);
				if (returnRecord == null || (start.HasValue && EventTimingManager.Get().IsEventActive(record.OpenPackEvent) && (!returnRecordStart.HasValue || start.Value > returnRecordStart.Value)))
				{
					returnRecord = record;
					returnRecordStart = start;
				}
			}
		}
		return (BoosterDbId)returnRecord.ID;
	}

	public static BoosterDbId GetRewardableBoosterFromSelector(RewardItem.BoosterSelector selector)
	{
		switch (selector)
		{
		case RewardItem.BoosterSelector.LATEST:
			return GetRewardableBoosterOffsetFromLatest(0);
		case RewardItem.BoosterSelector.LATEST_OFFSET_BY_1:
			return GetRewardableBoosterOffsetFromLatest(1);
		case RewardItem.BoosterSelector.LATEST_OFFSET_BY_2:
			return GetRewardableBoosterOffsetFromLatest(2);
		case RewardItem.BoosterSelector.LATEST_OFFSET_BY_3:
			return GetRewardableBoosterOffsetFromLatest(3);
		case RewardItem.BoosterSelector.LATEST_CATCHUP_PACK:
			return GetLatestCatchupPack();
		default:
			Debug.LogError($"Unknown BoosterSelector {selector}");
			return BoosterDbId.INVALID;
		}
	}

	public static AdventureDbId GetLatestActiveAdventure()
	{
		AdventureDbId latestAdventureId = AdventureDbId.INVALID;
		foreach (AdventureDbfRecord record in GameDbf.Adventure.GetRecords())
		{
			AdventureDbId currentId = (AdventureDbId)record.ID;
			if (!AdventureConfig.IsAdventureComingSoon(currentId) && AdventureConfig.IsAdventureEventActive(currentId) && currentId > latestAdventureId)
			{
				latestAdventureId = currentId;
			}
		}
		return latestAdventureId;
	}

	public static bool IsExpansionMission(int missionId)
	{
		ScenarioDbfRecord rec = GameDbf.Scenario.GetRecord(missionId);
		if (rec == null)
		{
			return false;
		}
		int adventureId = rec.AdventureId;
		if (adventureId == 0)
		{
			return false;
		}
		return IsExpansionAdventure((AdventureDbId)adventureId);
	}

	public static bool IsExpansionAdventure(AdventureDbId adventureId)
	{
		switch (adventureId)
		{
		case AdventureDbId.INVALID:
		case AdventureDbId.TUTORIAL:
		case AdventureDbId.PRACTICE:
		case AdventureDbId.TAVERN_BRAWL:
		case AdventureDbId.RETURNING_PLAYER:
		case AdventureDbId.MERCENARY_PVE:
		case AdventureDbId.BOTS_ON_LADDER:
			return false;
		default:
			return true;
		}
	}

	public static string GetAdventureProductStringKey(int wingID)
	{
		AdventureDbId adventureId = GetAdventureIdByWingId(wingID);
		if (adventureId != 0)
		{
			return GameDbf.Adventure.GetRecord((int)adventureId).ProductStringKey;
		}
		return string.Empty;
	}

	public static AdventureDbId GetAdventureId(int missionId)
	{
		return (AdventureDbId)(GameDbf.Scenario.GetRecord(missionId)?.AdventureId ?? 0);
	}

	public static AdventureDbId GetAdventureIdByWingId(int wingID)
	{
		WingDbfRecord wingRec = GameDbf.Wing.GetRecord(wingID);
		if (wingRec == null)
		{
			return AdventureDbId.INVALID;
		}
		AdventureDbId adventureId = (AdventureDbId)wingRec.AdventureId;
		if (!IsExpansionAdventure(adventureId))
		{
			return AdventureDbId.INVALID;
		}
		return adventureId;
	}

	public static AdventureModeDbId GetAdventureModeId(int missionId)
	{
		return (AdventureModeDbId)(GameDbf.Scenario.GetRecord(missionId)?.ModeId ?? 0);
	}

	public static bool IsHeroicAdventureMission(int missionId)
	{
		return IsModeHeroic(GetAdventureModeId(missionId));
	}

	public static bool IsModeHeroic(AdventureModeDbId mode)
	{
		if (mode != AdventureModeDbId.LINEAR_HEROIC)
		{
			return mode == AdventureModeDbId.DUNGEON_CRAWL_HEROIC;
		}
		return true;
	}

	public static AdventureModeDbId GetNormalModeFromHeroicMode(AdventureModeDbId mode)
	{
		return mode switch
		{
			AdventureModeDbId.DUNGEON_CRAWL_HEROIC => AdventureModeDbId.DUNGEON_CRAWL, 
			AdventureModeDbId.LINEAR_HEROIC => AdventureModeDbId.LINEAR, 
			_ => mode, 
		};
	}

	public static bool IsClassChallengeMission(int missionId)
	{
		return GetAdventureModeId(missionId) == AdventureModeDbId.CLASS_CHALLENGE;
	}

	public static int GetSortedWingUnlockIndex(WingDbfRecord wingRecord)
	{
		List<WingDbfRecord> wingRecords = GameDbf.Wing.GetRecords((WingDbfRecord r) => r.AdventureId == wingRecord.AdventureId);
		bool wingsHaveSameUnlockOrder = false;
		wingRecords.Sort(delegate(WingDbfRecord l, WingDbfRecord r)
		{
			int num = l.UnlockOrder - r.UnlockOrder;
			if (num == 0 && l.ID != r.ID)
			{
				wingsHaveSameUnlockOrder = true;
			}
			return num;
		});
		if (wingsHaveSameUnlockOrder)
		{
			return 0;
		}
		return wingRecords.FindIndex((WingDbfRecord r) => r.ID == wingRecord.ID);
	}

	public static int GetNumWingsInAdventure(AdventureDbId adventureId)
	{
		return GameDbf.Wing.GetRecords((WingDbfRecord r) => r.AdventureId == (int)adventureId).Count;
	}

	public static void ReplayTraditionalTutorial()
	{
		if (s_profileProgress.Value != null)
		{
			SetTutorialProgress(TutorialProgress.NOTHING_COMPLETE, tutorialComplete: false);
			if (!TutorialProgressScreen.HasEverOpenedRewardChest())
			{
				TutorialProgressScreen.SetHasEverOpenedRewardChest();
			}
			if (Network.ShouldBeConnectedToAurora() && Network.IsLoggedIn())
			{
				BnetPresenceMgr.Get().SetGameField(15u, 0);
			}
			GameMgr.Get().FindGame(GameType.GT_TUTORIAL, FormatType.FT_WILD, GetNextTutorial(), 0, 0L, null, null, restoreSavedGameState: false, null, null, 0L);
		}
	}

	public static void CompleteTraditionalTutorial()
	{
		NetCache.NetCacheProfileProgress profileProgress = s_profileProgress.Value;
		if (profileProgress != null && !IsTraditionalTutorialComplete(profileProgress))
		{
			if (GameState.Get().GetGameEntity() is TutorialEntity tutorialEntity)
			{
				tutorialEntity.ClearPreTutorialNotification();
			}
			SetTutorialProgress(TutorialProgress.LICH_KING_COMPLETE, tutorialComplete: true);
			if (Network.ShouldBeConnectedToAurora() && Network.IsLoggedIn())
			{
				BnetPresenceMgr.Get().SetGameField(15u, 1);
			}
			NotificationManager.Get().DestroyAllPopUps();
			BnetBar bnetBar = BnetBar.Get();
			if (bnetBar != null)
			{
				bnetBar.HideSkipTutorialButton();
			}
			if (!DialogManager.Get().ShowInitialDownloadPopupDuringDownload())
			{
				SceneMgr.Get().SetNextMode(SceneMgr.Mode.HUB);
			}
		}
	}

	public static void SetTutorialProgress(TutorialProgress progress, bool tutorialComplete)
	{
		if (!GameMgr.Get().IsSpectator())
		{
			AdTrackingManager.Get().TrackTutorialProgress(progress);
			NetCache.NetCacheProfileProgress info = NetCache.Get().GetNetObject<NetCache.NetCacheProfileProgress>();
			if (info != null)
			{
				info.CampaignProgress = progress;
				info.TutorialComplete = tutorialComplete;
			}
			NetCache.Get().NetCacheChanged<NetCache.NetCacheProfileProgress>();
		}
	}

	public static bool IsTraditionalTutorialComplete()
	{
		NetCache.NetCacheProfileProgress profileProgress = s_profileProgress.Value;
		if (profileProgress != null)
		{
			return IsTraditionalTutorialComplete(profileProgress);
		}
		return false;
	}

	public static bool HasEverCompletedTraditionalTutorial()
	{
		if (!GameSaveDataManager.Get().GetSubkeyValue(GameSaveKeyId.PLAYER_FLAGS, GameSaveKeySubkeyId.PLAYER_FLAGS_HAS_EVER_COMPLETED_TRADITIONAL_TUTORIAL, out long hasEverCompletedTraditionalTutorial))
		{
			return false;
		}
		return hasEverCompletedTraditionalTutorial != 0;
	}

	public static bool AreAllTutorialsComplete(NetCache.NetCacheProfileProgress profileProgress)
	{
		if (!IsTraditionalTutorialComplete(profileProgress))
		{
			return false;
		}
		if (!IsBattleGroundsTutorialComplete())
		{
			return false;
		}
		return IsMercenariesVillageTutorialComplete();
	}

	public static bool IsTraditionalTutorialComplete(NetCache.NetCacheProfileProgress profileProgress)
	{
		if (DemoMgr.Get().GetMode() == DemoMode.BLIZZ_MUSEUM)
		{
			return false;
		}
		if (!profileProgress.TutorialComplete)
		{
			return profileProgress.CampaignProgress >= TutorialProgress.LICH_KING_COMPLETE;
		}
		return true;
	}

	public static bool CanCheckTutorialCompletion()
	{
		if (!GameSaveDataManager.Get().IsDataReady(GameSaveKeyId.BACON))
		{
			return false;
		}
		if (!GameSaveDataManager.Get().IsDataReady(GameSaveKeyId.MERCENARIES))
		{
			return false;
		}
		if (s_profileProgress.Value == null)
		{
			return false;
		}
		return true;
	}

	public static bool IsAnyTutorialComplete()
	{
		if (IsBattleGroundsTutorialComplete())
		{
			return true;
		}
		if (IsMercenariesVillageTutorialComplete())
		{
			return true;
		}
		NetCache.NetCacheProfileProgress profileProgress = s_profileProgress.Value;
		if (profileProgress == null)
		{
			return false;
		}
		return IsTraditionalTutorialComplete(profileProgress);
	}

	public static bool IsBattleGroundsTutorialComplete()
	{
		NetCache.NetCacheFeatures features = NetCache.Get().GetNetObject<NetCache.NetCacheFeatures>();
		bool tutorialDisabled = false;
		if (features != null)
		{
			tutorialDisabled = !features.Games.BattlegroundsTutorial;
		}
		if (tutorialDisabled)
		{
			return false;
		}
		if (GameSaveDataManager.Get() == null || !GameSaveDataManager.Get().IsDataReady(GameSaveKeyId.BACON))
		{
			return false;
		}
		long isTutorialComplete = 0L;
		GameSaveDataManager.Get().GetSubkeyValue(GameSaveKeyId.BACON, GameSaveKeySubkeyId.BACON_HAS_SEEN_TUTORIAL, out isTutorialComplete);
		return isTutorialComplete > 0;
	}

	public static void MarkBattlegGroundsTutorialComplete()
	{
		if (GameSaveDataManager.Get() != null && GameSaveDataManager.Get().IsDataReady(GameSaveKeyId.BACON))
		{
			SetGSDFlag(GameSaveKeyId.BACON, GameSaveKeySubkeyId.BACON_HAS_SEEN_TUTORIAL, enableFlag: true);
		}
	}

	public static bool IsMercenariesPrologueBountyComplete(NetCache.NetCacheMercenariesPlayerInfo playerInfo)
	{
		if (playerInfo == null)
		{
			Debug.LogError("Player Info was null when check prologue bounty completion.  This should be checked before entering this function or undesirable results may occur");
			return false;
		}
		List<LettuceBountyDbfRecord> records = GameDbf.LettuceBounty.GetRecords((LettuceBountyDbfRecord r) => r.BountySetRecord != null && r.BountySetRecord.IsTutorial && r.Enabled).ToList();
		if (records.Count <= 0)
		{
			return false;
		}
		return MercenariesDataUtil.IsBountyComplete(records[0].ID, playerInfo);
	}

	public static bool IsMercenariesVillageTutorialComplete()
	{
		return LettuceTutorialUtils.IsSpecificEventComplete(LettuceTutorialVo.LettuceTutorialEvent.VILLAGE_TUTORIAL_END);
	}

	public static bool HasCompletedApprentice()
	{
		GameSaveDataManager gsdMgr = GameSaveDataManager.Get();
		if (!gsdMgr.IsDataReady(GameSaveKeyId.PLAYER_FLAGS))
		{
			return true;
		}
		if (gsdMgr.GetSubkeyValue(GameSaveKeyId.PLAYER_FLAGS, GameSaveKeySubkeyId.PLAYER_FLAGS_HAS_COMPLETED_APPRENTICE, out long hasCompletedApprenticeFlag))
		{
			return hasCompletedApprenticeFlag > 0;
		}
		return false;
	}

	public static bool CanSkipApprentice()
	{
		if (!HasCompletedApprentice())
		{
			return IsTraditionalTutorialComplete();
		}
		return false;
	}

	public static bool ShouldSkipRailroading()
	{
		GameSaveDataManager gsdManager = GameSaveDataManager.Get();
		if (!gsdManager.IsDataReady(GameSaveKeyId.FTUE))
		{
			return true;
		}
		gsdManager.GetSubkeyValue(GameSaveKeyId.FTUE, GameSaveKeySubkeyId.SHOULD_SKIP_RAILROADING, out long shouldSkipRailroadingFlag);
		return shouldSkipRailroadingFlag == 1;
	}

	public static bool AreAllTutorialsComplete()
	{
		NetCache.NetCacheProfileProgress profileProgress = s_profileProgress.Value;
		if (profileProgress != null)
		{
			return AreAllTutorialsComplete(profileProgress);
		}
		return false;
	}

	public static bool TutorialPreviewVideosEnabled()
	{
		NetCache.NetCacheFeatures features = NetCache.Get().GetNetObject<NetCache.NetCacheFeatures>();
		if (features == null)
		{
			Log.All.Print(" Could not get NetCacheFeatures Object");
			return true;
		}
		return features.TutorialPreviewVideosEnabled;
	}

	public static float TutorialPreviewVideosTimeout()
	{
		NetCache.NetCacheFeatures features = NetCache.Get().GetNetObject<NetCache.NetCacheFeatures>();
		if (features == null)
		{
			Log.All.Print(" Could not get NetCacheFeatures Object");
			return NetCache.NetCacheFeatures.Defaults.TutorialPreviewVideosTimeout;
		}
		return features.TutorialPreviewVideosTimeout;
	}

	public static int GetNextTutorial(NetCache.NetCacheProfileProgress progress)
	{
		if (progress.TutorialComplete)
		{
			return 0;
		}
		if (progress.CampaignProgress == TutorialProgress.NOTHING_COMPLETE)
		{
			return 5287;
		}
		if (progress.CampaignProgress == TutorialProgress.REXXAR_COMPLETE)
		{
			return 5289;
		}
		if (progress.CampaignProgress == TutorialProgress.GARROSH_COMPLETE)
		{
			return 5290;
		}
		return 0;
	}

	public static int GetNextTutorial()
	{
		NetCache.NetCacheProfileProgress profileProgress = s_profileProgress.Value;
		if (profileProgress == null)
		{
			profileProgress = new NetCache.NetCacheProfileProgress
			{
				CampaignProgress = Options.Get().GetEnum<TutorialProgress>(Option.LOCAL_TUTORIAL_PROGRESS),
				TutorialComplete = false
			};
		}
		return GetNextTutorial(profileProgress);
	}

	public static string GetTutorialCardRewardDetails(int missionId)
	{
		switch ((ScenarioDbId)missionId)
		{
		case ScenarioDbId.TUTORIAL_REXXAR:
			return GameStrings.Get("GLOBAL_REWARD_CARD_DETAILS_TUTORIAL01");
		case ScenarioDbId.TUTORIAL_GARROSH:
			return GameStrings.Get("GLOBAL_REWARD_CARD_DETAILS_TUTORIAL02");
		case ScenarioDbId.TUTORIAL_LICH_KING:
			return GameStrings.Get("GLOBAL_REWARD_CARD_DETAILS_TUTORIAL03");
		default:
			Debug.LogWarning($"GameUtils.GetTutorialCardRewardDetails(): no card reward details for mission {missionId}");
			return "";
		}
	}

	public static string GetCurrentTutorialCardRewardDetails()
	{
		return GetTutorialCardRewardDetails(GameMgr.Get().GetMissionId());
	}

	public static int MissionSortComparison(ScenarioDbfRecord rec1, ScenarioDbfRecord rec2)
	{
		return rec1.SortOrder - rec2.SortOrder;
	}

	public static List<ScenarioGuestHeroesDbfRecord> GetScenarioGuestHeroes(int scenarioId)
	{
		return GameDbf.ScenarioGuestHeroes.GetRecords((ScenarioGuestHeroesDbfRecord r) => r.ScenarioId == scenarioId);
	}

	public static int GetDefeatedBossCount()
	{
		int @int = Options.Get().GetInt(Option.SELECTED_ADVENTURE);
		AdventureModeDbId selectedMode = (AdventureModeDbId)Options.Get().GetInt(Option.SELECTED_ADVENTURE_MODE);
		AdventureDataDbfRecord dataRecord = GetAdventureDataRecord(@int, (int)selectedMode);
		if (dataRecord == null)
		{
			return 0;
		}
		GameSaveKeyId gameSaveDataServerKey = (GameSaveKeyId)dataRecord.GameSaveDataServerKey;
		if (!DungeonCrawlUtil.IsDungeonRunActive(gameSaveDataServerKey))
		{
			return 0;
		}
		List<long> defeatedBossIds = null;
		GameSaveDataManager.Get().GetSubkeyValue(gameSaveDataServerKey, GameSaveKeySubkeyId.DUNGEON_CRAWL_BOSSES_DEFEATED, out defeatedBossIds);
		return defeatedBossIds?.Count ?? 0;
	}

	public static List<FixedRewardActionDbfRecord> GetFixedActionRecords(FixedRewardAction.Type actionType)
	{
		return GameDbf.GetIndex().GetFixedActionRecordsForType(actionType);
	}

	public static FixedRewardDbfRecord GetFixedRewardForCard(string cardID, TAG_PREMIUM premium)
	{
		int assetID = TranslateCardIdToDbId(cardID);
		return GameDbf.GetIndex().GetFixedRewardRecordsForCardId(assetID, (int)premium);
	}

	public static List<FixedRewardMapDbfRecord> GetFixedRewardMapRecordsForAction(int actionID)
	{
		return GameDbf.GetIndex().GetFixedRewardMapRecordsForAction(actionID);
	}

	public static int GetFixedRewardCounterpartCardID(int cardID)
	{
		foreach (FixedRewardActionDbfRecord actionRecord in GetFixedActionRecords(FixedRewardAction.Type.OWNS_COUNTERPART_CARD))
		{
			if (!EventTimingManager.Get().IsEventActive(actionRecord.ActiveEvent))
			{
				continue;
			}
			foreach (FixedRewardMapDbfRecord mapRecord in GetFixedRewardMapRecordsForAction(actionRecord.ID))
			{
				FixedRewardDbfRecord rewardRecord = GameDbf.FixedReward.GetRecord(mapRecord.RewardId);
				List<CounterpartCardsDbfRecord> counterpartCards = GetCounterpartCards(rewardRecord.CardId);
				if (counterpartCards == null)
				{
					continue;
				}
				foreach (CounterpartCardsDbfRecord item in counterpartCards)
				{
					if (item.DeckEquivalentCardId == cardID)
					{
						return rewardRecord.CardId;
					}
				}
			}
		}
		return 0;
	}

	public static string GetCounterpartCardIDForFormat(EntityDef cardDef, FormatType formatType)
	{
		List<CounterpartCardsDbfRecord> counterpartCards = GetCounterpartCards(cardDef.GetCardId());
		if (counterpartCards == null)
		{
			return null;
		}
		TAG_CARD_SET[] cardSets = GetCardSetsInFormat(formatType);
		foreach (CounterpartCardsDbfRecord item in counterpartCards)
		{
			string counterpartCardId = item.DeckEquivalentCardRecord.NoteMiniGuid;
			TAG_CARD_SET counterpartCardSet = GetCardSetFromCardID(counterpartCardId);
			TAG_CARD_SET[] array = cardSets;
			foreach (TAG_CARD_SET cardSet in array)
			{
				if (counterpartCardSet == cardSet)
				{
					return counterpartCardId;
				}
			}
		}
		return null;
	}

	public static List<CardSetTimingDbfRecord> GetCardSetTimingsForCard(int cardId)
	{
		return GameDbf.GetIndex().GetCardSetTimingByCardId(cardId);
	}

	public static List<RewardTrackLevelDbfRecord> GetRewardTrackLevelsForRewardTrack(int rewardTrackId)
	{
		return GameDbf.GetIndex().GetRewardTrackLevelsByRewardTrackId(rewardTrackId);
	}

	public static List<RewardItemDbfRecord> GetRewardItemsForRewardList(int rewardListId)
	{
		return GameDbf.GetIndex().GetRewardItemsByRewardTrackId(rewardListId);
	}

	public static bool IsMatchmadeGameType(GameType gameType, int? missionId = null)
	{
		switch (gameType)
		{
		case GameType.GT_PVPDR_PAID:
		case GameType.GT_PVPDR:
			if (missionId.HasValue && DungeonCrawlUtil.IsPVPDRFriendlyEncounter(missionId.Value))
			{
				return false;
			}
			return true;
		case GameType.GT_ARENA:
		case GameType.GT_RANKED:
		case GameType.GT_CASUAL:
		case GameType.GT_BATTLEGROUNDS:
		case GameType.GT_MERCENARIES_PVP:
		case GameType.GT_BATTLEGROUNDS_DUO:
		case GameType.GT_BATTLEGROUNDS_DUO_VS_AI:
		case GameType.GT_BATTLEGROUNDS_DUO_1_PLAYER_VS_AI:
			return true;
		case GameType.GT_VS_AI:
		case GameType.GT_VS_FRIEND:
		case GameType.GT_TUTORIAL:
		case GameType.GT_BATTLEGROUNDS_FRIENDLY:
		case GameType.GT_MERCENARIES_PVE:
		case GameType.GT_MERCENARIES_PVE_COOP:
		case GameType.GT_BATTLEGROUNDS_DUO_FRIENDLY:
			return false;
		default:
			if (IsTavernBrawlGameType(gameType))
			{
				int id = 0;
				if (missionId.HasValue)
				{
					id = missionId.Value;
				}
				else
				{
					TavernBrawlMission mission = TavernBrawlManager.Get().CurrentMission();
					if (mission == null)
					{
						return true;
					}
					id = mission.missionId;
				}
				if (IsAIMission(id))
				{
					return false;
				}
				return true;
			}
			return false;
		}
	}

	public static bool IsBattlegroundsGameType(GameType gametype)
	{
		if (gametype != GameType.GT_BATTLEGROUNDS && gametype != GameType.GT_BATTLEGROUNDS_FRIENDLY && gametype != GameType.GT_BATTLEGROUNDS_PLAYER_VS_AI && gametype != GameType.GT_BATTLEGROUNDS_AI_VS_AI && gametype != GameType.GT_BATTLEGROUNDS_DUO && gametype != GameType.GT_BATTLEGROUNDS_DUO_VS_AI && gametype != GameType.GT_BATTLEGROUNDS_DUO_FRIENDLY)
		{
			return gametype == GameType.GT_BATTLEGROUNDS_DUO_1_PLAYER_VS_AI;
		}
		return true;
	}

	public static bool IsTavernBrawlGameType(GameType gameType)
	{
		if ((uint)(gameType - 16) <= 2u)
		{
			return true;
		}
		return false;
	}

	public static bool IsPvpDrGameType(GameType gameType)
	{
		if ((uint)(gameType - 28) <= 1u)
		{
			return true;
		}
		return false;
	}

	public static bool IsMercenariesGameType(GameType gameType)
	{
		if ((uint)(gameType - 30) <= 4u)
		{
			return true;
		}
		return false;
	}

	public static bool ShouldShowArenaModeIcon()
	{
		return GameMgr.Get().GetGameType() == GameType.GT_ARENA;
	}

	public static bool ShouldShowCasualModeIcon()
	{
		return GameMgr.Get().GetGameType() == GameType.GT_CASUAL;
	}

	public static bool ShouldShowFriendlyChallengeIcon()
	{
		if (GameMgr.Get().GetGameType() == GameType.GT_VS_FRIEND)
		{
			if (FriendChallengeMgr.Get().IsChallengeTavernBrawl())
			{
				return false;
			}
			return true;
		}
		return false;
	}

	public static bool ShouldShowTavernBrawlModeIcon()
	{
		GameType gameType = GameMgr.Get().GetGameType();
		if (gameType == GameType.GT_VS_FRIEND && FriendChallengeMgr.Get().IsChallengeTavernBrawl())
		{
			return true;
		}
		if (IsTavernBrawlGameType(gameType))
		{
			return true;
		}
		return false;
	}

	public static bool ShouldShowAdventureModeIcon()
	{
		int missionId = GameMgr.Get().GetMissionId();
		GameType gameType = GameMgr.Get().GetGameType();
		AdventureDbId adventureId = GetAdventureId(missionId);
		if (IsExpansionMission(missionId) && adventureId != AdventureDbId.TAVERN_BRAWL && !AdventureUtils.IsDuelsAdventure(adventureId) && !IsTavernBrawlGameType(gameType))
		{
			return !IsMercenariesGameType(gameType);
		}
		return false;
	}

	public static bool ShouldShowPvpDrModeIcon()
	{
		return AdventureUtils.IsDuelsAdventure(GetAdventureId(GameMgr.Get().GetMissionId()));
	}

	public static bool IsGameTypeRanked()
	{
		return IsGameTypeRanked(GameMgr.Get().GetGameType());
	}

	public static bool IsGameTypeRanked(GameType gameType)
	{
		if (DemoMgr.Get().IsExpoDemo())
		{
			return false;
		}
		return gameType == GameType.GT_RANKED;
	}

	public static void RequestPlayerPresence(BnetGameAccountId gameAccountId)
	{
		List<PresenceFieldKey> list = new List<PresenceFieldKey>();
		PresenceFieldKey battleTag = default(PresenceFieldKey);
		battleTag.programId = BnetProgramId.BNET.GetValue();
		battleTag.groupId = 2u;
		battleTag.fieldId = 7u;
		battleTag.uniqueId = 0uL;
		list.Add(battleTag);
		battleTag.programId = BnetProgramId.BNET.GetValue();
		battleTag.groupId = 2u;
		battleTag.fieldId = 3u;
		battleTag.uniqueId = 0uL;
		list.Add(battleTag);
		battleTag.programId = BnetProgramId.BNET.GetValue();
		battleTag.groupId = 2u;
		battleTag.fieldId = 5u;
		battleTag.uniqueId = 0uL;
		list.Add(battleTag);
		if (IsGameTypeRanked())
		{
			PresenceFieldKey rank = default(PresenceFieldKey);
			rank.programId = BnetProgramId.HEARTHSTONE.GetValue();
			rank.groupId = 2u;
			rank.fieldId = 18u;
			rank.uniqueId = 0uL;
			list.Add(rank);
		}
		PresenceFieldKey[] fieldList = list.ToArray();
		BattleNet.RequestPresenceFields(isGameAccountEntityId: true, gameAccountId, fieldList);
	}

	public static bool IsAIPlayer(BnetGameAccountId gameAccountId)
	{
		if (gameAccountId == null)
		{
			return false;
		}
		return !gameAccountId.IsValid();
	}

	public static bool IsHumanPlayer(BnetGameAccountId gameAccountId)
	{
		if (gameAccountId == null)
		{
			return false;
		}
		return gameAccountId.IsValid();
	}

	public static bool IsBnetPlayer(BnetGameAccountId gameAccountId)
	{
		if (!IsHumanPlayer(gameAccountId))
		{
			return false;
		}
		return Network.ShouldBeConnectedToAurora();
	}

	public static bool IsGuestPlayer(BnetGameAccountId gameAccountId)
	{
		if (!IsHumanPlayer(gameAccountId))
		{
			return false;
		}
		return !Network.ShouldBeConnectedToAurora();
	}

	public static bool IsAnyTransitionActive()
	{
		SceneMgr sceneMgr = SceneMgr.Get();
		if (sceneMgr != null)
		{
			if (sceneMgr.IsTransitionNowOrPending())
			{
				return true;
			}
			PegasusScene pegasusScene = sceneMgr.GetScene();
			if (pegasusScene != null && pegasusScene.IsTransitioning())
			{
				return true;
			}
		}
		Box box = Box.Get();
		if (box != null && box.IsTransitioningToSceneMode())
		{
			return true;
		}
		LoadingScreen loadingScreen = LoadingScreen.Get();
		if (loadingScreen != null && loadingScreen.IsTransitioning())
		{
			return true;
		}
		return false;
	}

	public static void ExitConfirmation(AlertPopup.ResponseCallback responseCallback)
	{
		AlertPopup.PopupInfo info = new AlertPopup.PopupInfo
		{
			m_headerText = GameStrings.Format("GLOBAL_EXIT_CONFIRM_TITLE"),
			m_text = GameStrings.Format("GLOBAL_EXIT_CONFIRM_MESSAGE"),
			m_alertTextAlignment = UberText.AlignmentOptions.Center,
			m_showAlertIcon = false,
			m_responseDisplay = AlertPopup.ResponseDisplay.CONFIRM_CANCEL,
			m_responseCallback = responseCallback
		};
		DialogManager.Get().ShowPopup(info);
	}

	public static void LogoutConfirmation()
	{
		AlertPopup.PopupInfo info = new AlertPopup.PopupInfo
		{
			m_headerText = GameStrings.Get(Network.ShouldBeConnectedToAurora() ? "GLOBAL_SWITCH_ACCOUNT" : "GLOBAL_LOGIN_CONFIRM_TITLE"),
			m_text = GameStrings.Get(Network.ShouldBeConnectedToAurora() ? "GLOBAL_LOGOUT_CONFIRM_MESSAGE" : "GLOBAL_LOGIN_CONFIRM_MESSAGE"),
			m_alertTextAlignment = UberText.AlignmentOptions.Center,
			m_showAlertIcon = false,
			m_responseDisplay = AlertPopup.ResponseDisplay.CONFIRM_CANCEL,
			m_responseCallback = OnLogoutConfirmationResponse
		};
		DialogManager.Get().ShowPopup(info);
	}

	private static void OnLogoutConfirmationResponse(AlertPopup.Response response, object userData)
	{
		if (response != AlertPopup.Response.CANCEL)
		{
			Logout();
		}
	}

	public static void Logout()
	{
		GameMgr.Get().SetPendingAutoConcede(pendingAutoConcede: true);
		if (Network.ShouldBeConnectedToAurora())
		{
			ServiceManager.Get<ILoginService>()?.ClearAuthentication();
		}
		if (HearthstoneApplication.IsCNMobileBinary)
		{
			Options.Get().SetBool(Option.HAS_ACCEPTED_PRIVACY_POLICY_AND_EULA, val: false);
		}
		HearthstoneApplication.Get().ResetAndForceLogin();
	}

	public static PackOpeningRarity GetPackOpeningRarity(TAG_RARITY tag)
	{
		return tag switch
		{
			TAG_RARITY.COMMON => PackOpeningRarity.COMMON, 
			TAG_RARITY.FREE => PackOpeningRarity.COMMON, 
			TAG_RARITY.RARE => PackOpeningRarity.RARE, 
			TAG_RARITY.EPIC => PackOpeningRarity.EPIC, 
			TAG_RARITY.LEGENDARY => PackOpeningRarity.LEGENDARY, 
			_ => PackOpeningRarity.NONE, 
		};
	}

	public static List<BoosterDbfRecord> GetPackRecordsWithStorePrefab()
	{
		return GameDbf.Booster.GetRecords((BoosterDbfRecord r) => !string.IsNullOrEmpty(r.StorePrefab));
	}

	public static List<AdventureDbfRecord> GetSortedAdventureRecordsWithStorePrefab()
	{
		List<AdventureDbfRecord> records = GameDbf.Adventure.GetRecords((AdventureDbfRecord r) => !string.IsNullOrEmpty(r.StorePrefab));
		records.Sort((AdventureDbfRecord l, AdventureDbfRecord r) => r.SortOrder - l.SortOrder);
		return records;
	}

	public static List<AdventureDbfRecord> GetAdventureRecordsWithDefPrefab()
	{
		return GameDbf.Adventure.GetRecords((AdventureDbfRecord r) => !string.IsNullOrEmpty(r.AdventureDefPrefab));
	}

	public static List<AdventureDataDbfRecord> GetAdventureDataRecordsWithSubDefPrefab()
	{
		return GameDbf.AdventureData.GetRecords((AdventureDataDbfRecord r) => !string.IsNullOrEmpty(r.AdventureSubDefPrefab));
	}

	public static int PackSortingPredicate(BoosterDbfRecord left, BoosterDbfRecord right)
	{
		if (right.ListDisplayOrderCategory != left.ListDisplayOrderCategory)
		{
			return Mathf.Clamp(right.ListDisplayOrderCategory - left.ListDisplayOrderCategory, -1, 1);
		}
		if (right.ListDisplayOrder != left.ListDisplayOrder)
		{
			return Mathf.Clamp(right.ListDisplayOrder - left.ListDisplayOrder, -1, 1);
		}
		return Mathf.Clamp(right.ID - left.ID, -1, 1);
	}

	public static List<int> GetSortedPackIds(bool ascending = true)
	{
		List<BoosterDbfRecord> boosters = GameDbf.Booster.GetRecords();
		if (ascending)
		{
			boosters.Sort((BoosterDbfRecord l, BoosterDbfRecord r) => PackSortingPredicate(r, l));
		}
		else
		{
			boosters.Sort((BoosterDbfRecord l, BoosterDbfRecord r) => PackSortingPredicate(l, r));
		}
		return boosters.Select((BoosterDbfRecord b) => b.ID).ToList();
	}

	public static bool IsFakePackOpeningEnabled()
	{
		if (!HearthstoneApplication.IsInternal())
		{
			return false;
		}
		return Options.Get().GetBool(Option.FAKE_PACK_OPENING);
	}

	public static int GetFakePackCount()
	{
		if (!HearthstoneApplication.IsInternal())
		{
			return 0;
		}
		return Options.Get().GetInt(Option.FAKE_PACK_COUNT);
	}

	public static bool IsFirstPurchaseBundleBooster(StorePackId storePackId)
	{
		if (storePackId.Type == StorePackType.BOOSTER)
		{
			return 181 == storePackId.Id;
		}
		return false;
	}

	public static bool IsMammothBundleBooster(StorePackId storePackId)
	{
		if (storePackId.Type == StorePackType.BOOSTER)
		{
			return 41 == storePackId.Id;
		}
		return false;
	}

	public static bool IsHiddenLicenseBundleBooster(StorePackId storePackId)
	{
		if (storePackId.Type == StorePackType.BOOSTER)
		{
			BoosterDbId id = (BoosterDbId)storePackId.Id;
			if (id == BoosterDbId.MAMMOTH_BUNDLE || id == BoosterDbId.FIRST_PURCHASE)
			{
				return true;
			}
			return false;
		}
		return false;
	}

	public static int GetProductDataFromStorePackId(StorePackId storePackId, int selectedIndex = 0)
	{
		if (storePackId.Type == StorePackType.BOOSTER)
		{
			if (storePackId.Id == 181)
			{
				return 40;
			}
			if (storePackId.Id == 41)
			{
				return 27;
			}
			return storePackId.Id;
		}
		return 0;
	}

	public static int GetProductDataCountFromStorePackId(StorePackId storePackId)
	{
		if (storePackId.Type == StorePackType.BOOSTER)
		{
			return 1;
		}
		return 0;
	}

	public static List<BoosterDbfRecord> GetRewardableBoosters()
	{
		List<BoosterDbfRecord> returnRecords = new List<BoosterDbfRecord>();
		DateTime now = DateTime.UtcNow;
		foreach (BoosterDbfRecord record in GameDbf.Booster.GetRecords())
		{
			if (!IsBoosterRotated((BoosterDbId)record.ID, now) && EventTimingManager.Get().IsEventActive(record.RewardableEvent, now))
			{
				returnRecords.Add(record);
			}
		}
		returnRecords.Sort(SortBoostersDescending);
		return returnRecords;
	}

	public static int GetBoardIdFromAssetName(string name)
	{
		foreach (BoardDbfRecord rec in GameDbf.Board.GetRecords())
		{
			string currPath = rec.Prefab;
			if (!(name != currPath))
			{
				return rec.ID;
			}
		}
		return 0;
	}

	public static UnityEngine.Object Instantiate(GameObject original, GameObject parent, bool withRotation = false)
	{
		if (original == null)
		{
			return null;
		}
		GameObject gameObject = UnityEngine.Object.Instantiate(original);
		SetParent(gameObject, parent, withRotation);
		return gameObject;
	}

	public static UnityEngine.Object Instantiate(Component original, GameObject parent, bool withRotation = false)
	{
		if (original == null)
		{
			return null;
		}
		Component component = UnityEngine.Object.Instantiate(original);
		SetParent(component, parent, withRotation);
		return component;
	}

	public static UnityEngine.Object Instantiate(UnityEngine.Object original)
	{
		if (original == null)
		{
			return null;
		}
		return UnityEngine.Object.Instantiate(original);
	}

	public static UnityEngine.Object InstantiateGameObject(string path, GameObject parent = null, bool withRotation = false)
	{
		if (path == null)
		{
			return null;
		}
		GameObject newobj = AssetLoader.Get().InstantiatePrefab(path);
		if (parent != null)
		{
			SetParent(newobj, parent, withRotation);
		}
		return newobj;
	}

	public static void SetParent(Component child, Component parent, bool withRotation = false)
	{
		SetParent(child.transform, parent.transform, withRotation);
	}

	public static void SetParent(GameObject child, Component parent, bool withRotation = false)
	{
		SetParent(child.transform, parent.transform, withRotation);
	}

	public static void SetParent(Component child, GameObject parent, bool withRotation = false)
	{
		SetParent(child.transform, parent.transform, withRotation);
	}

	public static void SetParent(GameObject child, GameObject parent, bool withRotation = false)
	{
		SetParent(child.transform, parent.transform, withRotation);
	}

	private static void SetParent(Transform child, Transform parent, bool withRotation)
	{
		Vector3 scale = child.localScale;
		Quaternion rotation = child.localRotation;
		child.parent = parent;
		child.localPosition = Vector3.zero;
		child.localScale = scale;
		if (withRotation)
		{
			child.localRotation = rotation;
		}
	}

	public static void ResetTransform(GameObject obj)
	{
		obj.transform.localPosition = Vector3.zero;
		obj.transform.localScale = Vector3.one;
		obj.transform.localRotation = Quaternion.identity;
	}

	public static void ResetTransform(Component comp)
	{
		ResetTransform(comp.gameObject);
	}

	public static T LoadGameObjectWithComponent<T>(string assetPath) where T : Component
	{
		GameObject gameObj = AssetLoader.Get().InstantiatePrefab(assetPath);
		if (gameObj == null)
		{
			return null;
		}
		T comp = gameObj.GetComponent<T>();
		if (comp == null)
		{
			Debug.LogError($"{assetPath} object does not contain {typeof(T)} component.");
			UnityEngine.Object.Destroy(gameObj);
			return null;
		}
		return comp;
	}

	public static T FindChildByName<T>(Transform transform, string name) where T : Component
	{
		foreach (Transform child in transform)
		{
			if (child.name == name)
			{
				return child.GetComponent<T>();
			}
			T nextChild = FindChildByName<T>(child, name);
			if (nextChild != null)
			{
				return nextChild;
			}
		}
		return null;
	}

	public static void PlayCardEffectDefSounds(CardEffectDef cardEffectDef)
	{
		if (cardEffectDef == null)
		{
			return;
		}
		foreach (string soundAssetPath in cardEffectDef.m_SoundSpellPaths)
		{
			AssetLoader.Get().InstantiatePrefab(soundAssetPath, delegate(AssetReference name, GameObject go, object data)
			{
				if (go == null)
				{
					Debug.LogError($"Unable to load spell object: {name}");
				}
				else
				{
					GameObject destroyObj = go;
					CardSoundSpell component = go.GetComponent<CardSoundSpell>();
					if (component == null)
					{
						Debug.LogError($"Card sound spell component not found: {name}");
						UnityEngine.Object.Destroy(destroyObj);
					}
					else
					{
						component.AddStateFinishedCallback(delegate(Spell spell, SpellStateType prevStateType, object userData)
						{
							if (spell.GetActiveState() == SpellStateType.NONE)
							{
								UnityEngine.Object.Destroy(destroyObj);
							}
						});
						component.ForceDefaultAudioSource();
						component.Activate();
					}
				}
			});
		}
	}

	public static bool LoadCardDefEmoteSound(List<EmoteEntryDef> emoteDefs, EmoteType type, EmoteSoundLoaded callback)
	{
		if (callback == null)
		{
			Debug.LogError("No callback provided for LoadEmote!");
			return false;
		}
		if (emoteDefs == null)
		{
			return false;
		}
		EmoteEntryDef emoteDef = emoteDefs.Find((EmoteEntryDef e) => e.m_emoteType == type);
		if (emoteDef == null)
		{
			return false;
		}
		if (!AssetLoader.Get().InstantiatePrefab(emoteDef.m_emoteSoundSpellPath, delegate(AssetReference assetRef, GameObject go, object callbackData)
		{
			callback(go.GetComponent<CardSoundSpell>());
		}))
		{
			callback(null);
		}
		return true;
	}

	public static string GetCardIdFromMercenaryId(int mercenaryId)
	{
		MercenaryArtVariationDbfRecord record = LettuceMercenary.GetDefaultArtVariationRecord(mercenaryId);
		if (record == null)
		{
			Debug.LogErrorFormat("GetCardIdFromMercenaryId() - No record found for merc: {0}", mercenaryId);
			return null;
		}
		return TranslateDbIdToCardId(record.CardId);
	}

	public static int GetMercenaryIdFromCardId(int cardId)
	{
		foreach (LettuceMercenaryDbfRecord mercenaryRecord in GameDbf.LettuceMercenary.GetRecords())
		{
			GameDbf.GetIndex().GetMercenaryArtVariationsByMercenaryID(mercenaryRecord.ID);
			foreach (MercenaryArtVariationDbfRecord mercenaryArtVariation in mercenaryRecord.MercenaryArtVariations)
			{
				if (mercenaryArtVariation.CardId == cardId)
				{
					return mercenaryRecord.ID;
				}
			}
		}
		return 0;
	}

	public static int GetMaxMercenaryLevel()
	{
		return GameDbf.GetIndex().GetMercenaryMaxLevel();
	}

	public static int GetMercenaryLevelFromExperience(int experience)
	{
		int maxAllowedLevel = GetMaxMercenaryLevel();
		List<LettuceMercenaryLevelDbfRecord> levelDbfRecords = GameDbf.LettuceMercenaryLevel.GetRecords();
		for (int level = 1; level <= maxAllowedLevel; level++)
		{
			LettuceMercenaryLevelDbfRecord levelRecord = null;
			int i = 0;
			for (int iMax = levelDbfRecords.Count; i < iMax; i++)
			{
				if (levelDbfRecords[i].Level == level)
				{
					levelRecord = levelDbfRecords[i];
					break;
				}
			}
			if (levelRecord == null)
			{
				Log.Lettuce.PrintError("GetMercenaryLevelFromExperience - Missing mercenary level data!");
				break;
			}
			if (experience < levelRecord.TotalXpRequired)
			{
				return level - 1;
			}
		}
		return maxAllowedLevel;
	}

	public static float GetExperiencePercentageFromExperienceValue(int experience)
	{
		int currentLevel = GetMercenaryLevelFromExperience(experience);
		if (currentLevel < GetMaxMercenaryLevel())
		{
			LettuceMercenaryLevelDbfRecord record = GameDbf.LettuceMercenaryLevel.GetRecord((LettuceMercenaryLevelDbfRecord r) => r.Level == currentLevel);
			return Mathf.InverseLerp(b: GameDbf.LettuceMercenaryLevel.GetRecord((LettuceMercenaryLevelDbfRecord r) => r.Level == currentLevel + 1).TotalXpRequired, a: record.TotalXpRequired, value: experience);
		}
		return 1f;
	}

	public static bool IsActorInMiniHand(Actor actor)
	{
		Entity entity = actor.GetEntity();
		if (entity == null)
		{
			return false;
		}
		ZoneHand zoneInstance = entity.GetCard().GetZone() as ZoneHand;
		if (zoneInstance != null && !zoneInstance.HandEnlarged())
		{
			return true;
		}
		return false;
	}

	public static float GetExperiencePercentageDelta(int startingExperience, int experienceDelta)
	{
		int maxLevel = GetMaxMercenaryLevel();
		if (experienceDelta == 0)
		{
			return 0f;
		}
		int finalExperience = startingExperience + experienceDelta;
		int startingLevel = maxLevel;
		int currentLevel = maxLevel;
		int nextLevel = maxLevel;
		int level;
		for (level = 1; level <= maxLevel; level++)
		{
			LettuceMercenaryLevelDbfRecord levelRecord = GameDbf.LettuceMercenaryLevel.GetRecord((LettuceMercenaryLevelDbfRecord r) => r.Level == level);
			if (levelRecord == null)
			{
				Log.Lettuce.PrintError("GetMercenaryLevelFromExperience - Missing mercenary level data!");
				break;
			}
			if (startingExperience < levelRecord.TotalXpRequired)
			{
				if (level <= startingLevel)
				{
					startingLevel = level - 1;
				}
				if (finalExperience <= levelRecord.TotalXpRequired)
				{
					currentLevel = level - 1;
					nextLevel = level;
					break;
				}
			}
		}
		LettuceMercenaryLevelDbfRecord startingLevelRecord = GameDbf.LettuceMercenaryLevel.GetRecord((LettuceMercenaryLevelDbfRecord r) => r.Level == startingLevel);
		LettuceMercenaryLevelDbfRecord nextLevelRecord = GameDbf.LettuceMercenaryLevel.GetRecord((LettuceMercenaryLevelDbfRecord r) => r.Level == nextLevel);
		if (startingLevel == currentLevel)
		{
			float startingPercentage = Mathf.InverseLerp(startingLevelRecord.TotalXpRequired, nextLevelRecord.TotalXpRequired, startingExperience);
			return Mathf.InverseLerp(startingLevelRecord.TotalXpRequired, nextLevelRecord.TotalXpRequired, finalExperience) - startingPercentage;
		}
		LettuceMercenaryLevelDbfRecord startingNextLevelRecord = GameDbf.LettuceMercenaryLevel.GetRecord((LettuceMercenaryLevelDbfRecord r) => r.Level == startingLevel + 1);
		LettuceMercenaryLevelDbfRecord record = GameDbf.LettuceMercenaryLevel.GetRecord((LettuceMercenaryLevelDbfRecord r) => r.Level == currentLevel);
		int numberOfExtraLevelUps = currentLevel - startingLevel - 1;
		float startingPercentageDelta = 1f - Mathf.InverseLerp(startingLevelRecord.TotalXpRequired, startingNextLevelRecord.TotalXpRequired, startingExperience);
		float finalPercentageDelta = Mathf.InverseLerp(record.TotalXpRequired, nextLevelRecord.TotalXpRequired, finalExperience);
		return (float)numberOfExtraLevelUps + startingPercentageDelta + finalPercentageDelta;
	}

	public static LettuceMercenaryLevelStatsDbfRecord GetMercenaryStatsByLevel(int mercenaryId, int level, out bool isMaxLevel)
	{
		int maxLevel = GetMaxMercenaryLevel();
		int clampedLevel = Mathf.Clamp(level, 1, maxLevel);
		isMaxLevel = clampedLevel == maxLevel;
		LettuceMercenaryLevelDbfRecord levelRecord = null;
		List<LettuceMercenaryLevelDbfRecord> levelDbfRecords = GameDbf.LettuceMercenaryLevel.GetRecords();
		int i = 0;
		for (int iMax = levelDbfRecords.Count; i < iMax; i++)
		{
			LettuceMercenaryLevelDbfRecord record = levelDbfRecords[i];
			if (record.Level == clampedLevel)
			{
				levelRecord = record;
				break;
			}
		}
		if (levelRecord == null)
		{
			Log.Lettuce.PrintError("GetMercenaryStatsByLevel() - Unable to get level dbf record for level {0}", level);
			return null;
		}
		LettuceMercenaryLevelStatsDbfRecord levelStatsRecord = null;
		List<LettuceMercenaryLevelStatsDbfRecord> levelStatsDbfRecords = GameDbf.LettuceMercenaryLevelStats.GetRecords();
		int j = 0;
		for (int iMax2 = levelStatsDbfRecords.Count; j < iMax2; j++)
		{
			LettuceMercenaryLevelStatsDbfRecord record2 = levelStatsDbfRecords[j];
			if (record2.LettuceMercenaryId == mercenaryId && record2.LettuceMercenaryLevelId == levelRecord.ID)
			{
				levelStatsRecord = record2;
				break;
			}
		}
		if (levelStatsRecord == null)
		{
			Log.Lettuce.PrintError("GetMercenaryStatsByLevel() - Unable to get level stats dbf record for level {0}", clampedLevel);
		}
		return levelStatsRecord;
	}

	public static bool IsFinalBossNodeType(int nodeTypeId)
	{
		LettuceMapNodeTypeDbfRecord record = GameDbf.LettuceMapNodeType.GetRecord(nodeTypeId);
		if (record == null)
		{
			return false;
		}
		return record.BossType == LettuceMapNodeType.LettuceMapBossType.FINAL_BOSS;
	}

	public static TAG_ROLE GetMercenaryTagRoleFromProtoRole(Mercenary.Role role)
	{
		return role switch
		{
			Mercenary.Role.ROLE_NEUTRAL => TAG_ROLE.NEUTRAL, 
			Mercenary.Role.ROLE_CASTER => TAG_ROLE.CASTER, 
			Mercenary.Role.ROLE_STRIKER => TAG_ROLE.FIGHTER, 
			Mercenary.Role.ROLE_PROTECTOR => TAG_ROLE.TANK, 
			_ => TAG_ROLE.INVALID, 
		};
	}

	public static bool LoadAndPositionCardActor(string actorName, string heroCardID, TAG_PREMIUM premium, LoadActorCallback callback)
	{
		if (!string.IsNullOrEmpty(heroCardID))
		{
			DefLoader.Get().LoadFullDef(heroCardID, delegate(string cardID, DefLoader.DisposableFullDef def, object userData)
			{
				LoadAndPositionCardActor_OnFullDefLoaded(actorName, cardID, def, userData, callback);
			}, premium);
			return true;
		}
		return false;
	}

	private static void LoadAndPositionCardActor_OnFullDefLoaded(string actorName, string cardID, DefLoader.DisposableFullDef def, object userData, LoadActorCallback callback)
	{
		TAG_PREMIUM actorPremium = (TAG_PREMIUM)userData;
		LoadActorCallbackInfo callbackInfo = new LoadActorCallbackInfo
		{
			fullDef = def,
			premium = actorPremium
		};
		AssetLoader.Get().InstantiatePrefab(actorName, delegate(AssetReference assetRef, GameObject go, object callbackData)
		{
			LoadAndPositionActorCard_OnActorLoaded(assetRef, go, callbackData, callback);
		}, callbackInfo, AssetLoadingOptions.IgnorePrefabPosition);
	}

	private static void LoadAndPositionActorCard_OnActorLoaded(AssetReference assetRef, GameObject go, object callbackData, LoadActorCallback callback)
	{
		LoadActorCallbackInfo callbackInfo = callbackData as LoadActorCallbackInfo;
		using (callbackInfo.fullDef)
		{
			if (go == null)
			{
				Debug.LogWarning($"GameUtils.OnHeroActorLoaded() - FAILED to load actor \"{assetRef}\"");
				return;
			}
			Actor actor = go.GetComponent<Actor>();
			if (actor == null)
			{
				Debug.LogWarning($"GameUtils.OnActorLoaded() - ERROR actor \"{assetRef}\" has no Actor component");
				return;
			}
			actor.SetPremium(callbackInfo.premium);
			actor.SetEntityDef(callbackInfo.fullDef.EntityDef);
			actor.SetCardDef(callbackInfo.fullDef.DisposableCardDef);
			actor.UpdateAllComponents();
			actor.gameObject.name = callbackInfo.fullDef.CardDef.name + "_actor";
			if ((bool)UniversalInputManager.UsePhoneUI)
			{
				LayerUtils.SetLayer(actor.gameObject, GameLayer.IgnoreFullScreenEffects);
			}
			GemObject healthObject = actor.GetHealthObject();
			if (healthObject != null)
			{
				healthObject.Hide();
			}
			callback?.Invoke(actor);
		}
	}

	public static bool IsBoosterWild(BoosterDbId boosterId)
	{
		if (boosterId == BoosterDbId.INVALID)
		{
			return false;
		}
		return IsBoosterWild(GameDbf.Booster.GetRecord((int)boosterId));
	}

	public static bool IsBoosterWild(BoosterDbfRecord boosterRecord)
	{
		if (boosterRecord != null)
		{
			EventTimingType eventType = boosterRecord.StandardEvent;
			if (eventType != EventTimingType.UNKNOWN && eventType != 0 && EventTimingManager.Get().HasEventEnded(eventType))
			{
				return true;
			}
		}
		return false;
	}

	public static bool IsAdventureWild(AdventureDbId adventureId)
	{
		if (adventureId == AdventureDbId.INVALID)
		{
			return false;
		}
		AdventureDbfRecord adventureRecord = GameDbf.Adventure.GetRecord((int)adventureId);
		if (adventureRecord != null)
		{
			EventTimingType eventType = adventureRecord.StandardEvent;
			if (eventType != EventTimingType.UNKNOWN && eventType != 0 && EventTimingManager.Get().HasEventEnded(eventType))
			{
				return true;
			}
		}
		return false;
	}

	private static bool GetSeasonMonthAndYear(int seasonId, int startId, int startMonth, int startYear, out int month, out int year)
	{
		month = 0;
		year = 0;
		if (seasonId < startId)
		{
			Debug.LogFormat("GetSeasonMonthAndYear called with invalid seasonId {0}. Launch season is 6.", seasonId);
			return false;
		}
		int monthIndex = seasonId - startId + startMonth - 1;
		month = monthIndex % 12 + 1;
		year = startYear + monthIndex / 12;
		return true;
	}

	private static string GetSeasonName(int seasonId, int startId, int startMonth, int startYear)
	{
		if (!GetSeasonMonthAndYear(seasonId, startId, startMonth, startYear, out var month, out var year))
		{
			return null;
		}
		string overriddenGlueSeasonName = $"GLUE_RANKED_SEASON_NAME_{seasonId}";
		string monthName = GameStrings.GetMonthFromDigits(month);
		if (GameStrings.HasKey(overriddenGlueSeasonName))
		{
			return GameStrings.Format(overriddenGlueSeasonName, monthName, year, seasonId);
		}
		return GameStrings.Format("GLUE_RANKED_SEASON_NAME_GENERIC", monthName, year, seasonId);
	}

	public static string GetRankedSeasonName(int seasonId)
	{
		return GetSeasonName(seasonId, 6, 4, 2014);
	}

	public static string GetMercenariesSeasonName(int seasonId)
	{
		return GetSeasonName(seasonId, 1, 11, 2021);
	}

	public static string GetMercenariesSeasonEndDescription(int seasonId, int highestRating)
	{
		if (!GetSeasonMonthAndYear(seasonId, 1, 11, 2021, out var month, out var year))
		{
			return null;
		}
		string monthName = GameStrings.GetMonthFromDigits(month);
		return GameStrings.Format("GLUE_LETTUCE_SEASON_ROLL_DESC", monthName, year, highestRating);
	}

	public static bool IsGSDFlagSet(GameSaveKeyId saveKey, GameSaveKeySubkeyId subkey)
	{
		if (!GameSaveDataManager.Get().IsDataReady(saveKey))
		{
			return false;
		}
		GameSaveDataManager.Get().GetSubkeyValue(saveKey, subkey, out long value);
		return value > 0;
	}

	public static void SetGSDFlag(GameSaveKeyId saveKey, GameSaveKeySubkeyId subkey, bool enableFlag)
	{
		if (IsGSDFlagSet(saveKey, subkey) != enableFlag)
		{
			int value = (enableFlag ? 1 : 0);
			GameSaveDataManager.Get().SaveSubkey(new GameSaveDataManager.SubkeySaveRequest(saveKey, subkey, value));
		}
	}

	public static bool IsGolden500HeroSkinAchievement(int achievementId)
	{
		foreach (KeyValuePair<TAG_CLASS, HeroSkinAchievements> hERO_SKIN_ACHIEVEMENT in HERO_SKIN_ACHIEVEMENTS)
		{
			if (hERO_SKIN_ACHIEVEMENT.Value.Golden500Win == achievementId)
			{
				return true;
			}
		}
		return false;
	}

	public static bool IsHonored1KHeroSkinAchievement(int achievementId)
	{
		foreach (KeyValuePair<TAG_CLASS, HeroSkinAchievements> hERO_SKIN_ACHIEVEMENT in HERO_SKIN_ACHIEVEMENTS)
		{
			if (hERO_SKIN_ACHIEVEMENT.Value.Honored1kWin == achievementId)
			{
				return true;
			}
		}
		return false;
	}

	public static bool HasClassTag(TAG_CLASS classTag, List<TAG_CLASS> tagsToCheck)
	{
		if (tagsToCheck == null)
		{
			return false;
		}
		foreach (TAG_CLASS item in tagsToCheck)
		{
			if (item == classTag)
			{
				return true;
			}
		}
		return false;
	}

	public static bool IsZilliaxCard(EntityDef entityDef)
	{
		if (entityDef == null)
		{
			return false;
		}
		if (!entityDef.HasTag(GAME_TAG.ZILLIAX_CUSTOMIZABLE_COSMETICMODULE) && !entityDef.HasTag(GAME_TAG.ZILLIAX_CUSTOMIZABLE_FUNCTIONALMODULE))
		{
			return entityDef.HasTag(GAME_TAG.ZILLIAX_CUSTOMIZABLE_SAVED_VERSION);
		}
		return true;
	}

	public static bool IsZilliaxModule(EntityDef entityDef)
	{
		if (entityDef == null)
		{
			return false;
		}
		if (!entityDef.HasTag(GAME_TAG.ZILLIAX_CUSTOMIZABLE_COSMETICMODULE))
		{
			return entityDef.HasTag(GAME_TAG.ZILLIAX_CUSTOMIZABLE_FUNCTIONALMODULE);
		}
		return true;
	}

	public static bool IsSavedZilliaxVersion(EntityDef entityDef)
	{
		return entityDef?.HasTag(GAME_TAG.ZILLIAX_CUSTOMIZABLE_SAVED_VERSION) ?? false;
	}

	public static bool IsClassTouristTag(GAME_TAG gameTag)
	{
		if (gameTag != GAME_TAG.DEATH_KNIGHT_TOURIST && gameTag != GAME_TAG.DEMON_HUNTER_TOURIST && gameTag != GAME_TAG.DRUID_TOURIST && gameTag != GAME_TAG.HUNTER_TOURIST && gameTag != GAME_TAG.MAGE_TOURIST && gameTag != GAME_TAG.PALADIN_TOURIST && gameTag != GAME_TAG.PRIEST_TOURIST && gameTag != GAME_TAG.ROGUE_TOURIST && gameTag != GAME_TAG.SHAMAN_TOURIST && gameTag != GAME_TAG.WARLOCK_TOURIST)
		{
			return gameTag == GAME_TAG.WARRIOR_TOURIST;
		}
		return true;
	}

	public static int StarshipLaunchCost(Player player)
	{
		if (player == null)
		{
			return -1;
		}
		int discount = player.GetTag(GAME_TAG.STARSHIP_LAUNCH_COST_DISCOUNT);
		return GetCardTagValue(STARSHIP_LAUNCH_CARD_ID, GAME_TAG.COST) - discount;
	}

	public static bool IsMythicHero(EntityDef entityDef)
	{
		if (entityDef == null)
		{
			return false;
		}
		return entityDef.GetTag<CornerReplacementSpellType>(GAME_TAG.CORNER_REPLACEMENT_TYPE) != CornerReplacementSpellType.NONE;
	}
}
