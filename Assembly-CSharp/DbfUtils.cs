using System;
using System.Collections.Generic;
using Assets;
using PegasusShared;

public static class DbfUtils
{
	public static ScenarioDbfRecord ConvertFromProtobuf(ScenarioDbRecord protoScenario, out List<ScenarioGuestHeroesDbfRecord> outScenarioGuestHeroRecords, out List<ClassExclusionsDbfRecord> outClassExclusionsRecords)
	{
		outScenarioGuestHeroRecords = new List<ScenarioGuestHeroesDbfRecord>();
		outClassExclusionsRecords = new List<ClassExclusionsDbfRecord>();
		if (protoScenario == null)
		{
			return null;
		}
		ScenarioDbfRecord dbf = new ScenarioDbfRecord();
		dbf.SetID(protoScenario.Id);
		dbf.SetNoteDesc(protoScenario.NoteDesc);
		dbf.SetPlayers(protoScenario.NumHumanPlayers);
		dbf.SetPlayer1HeroCardId((int)protoScenario.Player1HeroCardId);
		dbf.SetPlayer2HeroCardId((int)protoScenario.Player2HeroCardId);
		dbf.SetIsExpert(protoScenario.IsExpert);
		dbf.SetIsCoop(protoScenario.HasIsCoop && protoScenario.IsCoop);
		dbf.SetAdventureId(protoScenario.AdventureId);
		if (protoScenario.HasAdventureModeId)
		{
			dbf.SetModeId(protoScenario.AdventureModeId);
		}
		dbf.SetWingId(protoScenario.WingId);
		dbf.SetSortOrder(protoScenario.SortOrder);
		if (protoScenario.HasClientPlayer2HeroCardId)
		{
			dbf.SetClientPlayer2HeroCardId((int)protoScenario.ClientPlayer2HeroCardId);
		}
		dbf.SetTbTexture(protoScenario.TavernBrawlTexture);
		dbf.SetTbTexturePhone(protoScenario.TavernBrawlTexturePhone);
		if (protoScenario.HasTavernBrawlTexturePhoneOffset)
		{
			dbf.SetTbTexturePhoneOffsetY(protoScenario.TavernBrawlTexturePhoneOffset.Y);
		}
		foreach (ScenarioGuestHeroDbRecord scenarioHero in protoScenario.GuestHeroes)
		{
			ScenarioGuestHeroesDbfRecord heroDbf = new ScenarioGuestHeroesDbfRecord();
			heroDbf.SetScenarioId(scenarioHero.ScenarioId);
			heroDbf.SetGuestHeroId(scenarioHero.GuestHeroId);
			heroDbf.SetSortOrder(scenarioHero.SortOrder);
			outScenarioGuestHeroRecords.Add(heroDbf);
		}
		foreach (ClassExclusionDbRecord classExclusion in protoScenario.ClassExclusions)
		{
			ClassExclusionsDbfRecord classExclusionDbf = new ClassExclusionsDbfRecord();
			classExclusionDbf.SetScenarioId(classExclusion.ScenarioId);
			classExclusionDbf.SetClassId(classExclusion.ClassId);
			outClassExclusionsRecords.Add(classExclusionDbf);
		}
		dbf.SetScriptObject(protoScenario.ScriptObject);
		AddLocStrings(dbf, protoScenario.Strings);
		if (protoScenario.HasDeckRulesetId)
		{
			dbf.SetDeckRulesetId(protoScenario.DeckRulesetId);
		}
		if (protoScenario.HasRuleType)
		{
			int ruleTypeID = (int)protoScenario.RuleType;
			dbf.SetRuleType((Scenario.RuleType)ruleTypeID);
		}
		return dbf;
	}

	public static DeckRulesetDbfRecord ConvertFromProtobuf(DeckRulesetDbRecord proto)
	{
		if (proto == null)
		{
			return null;
		}
		DeckRulesetDbfRecord deckRulesetDbfRecord = new DeckRulesetDbfRecord();
		deckRulesetDbfRecord.SetID(proto.Id);
		return deckRulesetDbfRecord;
	}

	public static DeckRulesetRuleDbfRecord ConvertFromProtobuf(DeckRulesetRuleDbRecord proto, out List<int> outTargetSubsetIds)
	{
		outTargetSubsetIds = null;
		if (proto == null)
		{
			return null;
		}
		DeckRulesetRuleDbfRecord dbf = new DeckRulesetRuleDbfRecord();
		dbf.SetID(proto.Id);
		dbf.SetDeckRulesetId(proto.DeckRulesetId);
		if (proto.HasAppliesToSubsetId)
		{
			dbf.SetAppliesToSubsetId(proto.AppliesToSubsetId);
		}
		if (proto.HasAppliesToIsNot)
		{
			dbf.SetAppliesToIsNot(proto.AppliesToIsNot);
		}
		DeckRulesetRule.RuleType deckRuleSetRule = DeckRulesetRule.RuleType.INVALID_RULE_TYPE;
		deckRuleSetRule = (DeckRulesetRule.RuleType)Enum.Parse(typeof(DeckRulesetRule.RuleType), proto.RuleType, ignoreCase: true);
		dbf.SetRuleType(deckRuleSetRule);
		dbf.SetRuleIsNot(proto.RuleIsNot);
		if (proto.HasMinValue)
		{
			dbf.SetMinValue(proto.MinValue);
		}
		if (proto.HasMaxValue)
		{
			dbf.SetMaxValue(proto.MaxValue);
		}
		if (proto.HasTag)
		{
			dbf.SetTag(proto.Tag);
		}
		if (proto.HasTagMinValue)
		{
			dbf.SetTagMinValue(proto.TagMinValue);
		}
		if (proto.HasTagMaxValue)
		{
			dbf.SetTagMaxValue(proto.TagMaxValue);
		}
		if (proto.HasStringValue)
		{
			dbf.SetStringValue(proto.StringValue);
		}
		dbf.SetShowInvalidCards(proto.ShowInvalidCards);
		outTargetSubsetIds = proto.TargetSubsetIds;
		AddLocStrings(dbf, proto.Strings);
		return dbf;
	}

	public static RewardChestDbfRecord ConvertFromProtobuf(RewardChestDbRecord proto)
	{
		if (proto == null)
		{
			return null;
		}
		RewardChestDbfRecord rewardChestDbfRecord = new RewardChestDbfRecord();
		rewardChestDbfRecord.SetID(proto.Id);
		rewardChestDbfRecord.SetShowToReturningPlayer(proto.HasShowToReturningPlayer && proto.ShowToReturningPlayer);
		AddLocStrings(rewardChestDbfRecord, proto.Strings);
		return rewardChestDbfRecord;
	}

	public static GuestHeroDbfRecord ConvertFromProtobuf(GuestHeroDbRecord proto)
	{
		if (proto == null)
		{
			return null;
		}
		GuestHeroDbfRecord guestHeroDbfRecord = new GuestHeroDbfRecord();
		guestHeroDbfRecord.SetID(proto.Id);
		guestHeroDbfRecord.SetCardId(proto.CardId);
		guestHeroDbfRecord.SetUnlockEvent(DbfShared.GetEventMap().ConvertStringToSpecialEvent(proto.UnlockEvent));
		AddLocStrings(guestHeroDbfRecord, proto.Strings);
		return guestHeroDbfRecord;
	}

	public static DbfLocValue ConvertFromProtobuf(LocalizedString protoLocString)
	{
		DbfLocValue locString = new DbfLocValue();
		foreach (LocalizedStringValue protoLocStringValue in protoLocString.Values)
		{
			locString.SetString((Locale)protoLocStringValue.Locale, TextUtils.DecodeWhitespaces(protoLocStringValue.Value));
		}
		return locString;
	}

	private static void AddLocStrings(DbfRecord record, List<LocalizedString> protoStrings)
	{
		foreach (LocalizedString protoStr in protoStrings)
		{
			record.SetVar(protoStr.Key, ConvertFromProtobuf(protoStr));
		}
	}
}
