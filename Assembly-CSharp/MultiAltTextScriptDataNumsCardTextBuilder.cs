using System.Collections.Generic;
using System.Reflection;
using Hearthstone;
using UnityEngine;

public class MultiAltTextScriptDataNumsCardTextBuilder : CardTextBuilder
{
	public enum TagReferenceType
	{
		None,
		CardDBID
	}

	private Dictionary<GAME_TAG, TagReferenceType> m_GameTagToTagReferenceType;

	public MultiAltTextScriptDataNumsCardTextBuilder()
	{
		m_useEntityForTextInPlay = true;
	}

	public void SetTagRefType(GAME_TAG tag, TagReferenceType tagRefType)
	{
		if (m_GameTagToTagReferenceType == null)
		{
			m_GameTagToTagReferenceType = new Dictionary<GAME_TAG, TagReferenceType>();
		}
		m_GameTagToTagReferenceType.TryAdd(tag, tagRefType);
	}

	private string GetAlternateTextSubstring(string baseText, Entity entity)
	{
		if (string.IsNullOrEmpty(baseText) || entity == null)
		{
			return string.Empty;
		}
		string[] allSubstrings = baseText.Split('@');
		int desiredAltTextIndex = entity.GetTag(GAME_TAG.USE_ALTERNATE_CARD_TEXT);
		if (desiredAltTextIndex < 0 || desiredAltTextIndex >= allSubstrings.Length)
		{
			string message = MethodBase.GetCurrentMethod().ReflectedType.Name + "." + MethodBase.GetCurrentMethod().Name + "(): " + $"index of alternate text ({desiredAltTextIndex}) on Entity {entity.GetEntityId()} ({entity.GetName()}) " + $"is outside of the valid range [0, {allSubstrings.Length - 1}]. Value rounded to nearest valid one.";
			Log.Gameplay.PrintWarning(message);
			desiredAltTextIndex = Mathf.Clamp(desiredAltTextIndex, 0, allSubstrings.Length - 1);
		}
		return allSubstrings[desiredAltTextIndex];
	}

	private string GetAlternateTextSubstring(string baseText, EntityDef entityDef)
	{
		if (string.IsNullOrEmpty(baseText) || entityDef == null)
		{
			return string.Empty;
		}
		string[] allSubstrings = baseText.Split('@');
		int desiredAltTextIndex = entityDef.GetTag(GAME_TAG.USE_ALTERNATE_CARD_TEXT);
		if (desiredAltTextIndex < 0 || desiredAltTextIndex >= allSubstrings.Length)
		{
			string message = MethodBase.GetCurrentMethod().ReflectedType.Name + "." + MethodBase.GetCurrentMethod().Name + "(): " + $"index of alternate text ({desiredAltTextIndex}) on EntityDef ({entityDef.GetName()}) " + $"is outside of the valid range [0, {allSubstrings.Length - 1}]. Value rounded to nearest valid one.";
			Log.Gameplay.PrintWarning(message);
			desiredAltTextIndex = Mathf.Clamp(desiredAltTextIndex, 0, allSubstrings.Length - 1);
		}
		return allSubstrings[desiredAltTextIndex];
	}

	private string SubstituteScriptDataNums(string rawText, Entity entity)
	{
		if (string.IsNullOrEmpty(rawText))
		{
			return string.Empty;
		}
		if (entity == null)
		{
			return rawText;
		}
		return TextUtils.TryFormat(rawText, GetTagText(entity, GAME_TAG.TAG_SCRIPT_DATA_NUM_1), GetTagText(entity, GAME_TAG.TAG_SCRIPT_DATA_NUM_2), GetTagText(entity, GAME_TAG.TAG_SCRIPT_DATA_NUM_3), GetTagText(entity, GAME_TAG.TAG_SCRIPT_DATA_NUM_4), GetTagText(entity, GAME_TAG.TAG_SCRIPT_DATA_NUM_5), GetTagText(entity, GAME_TAG.TAG_SCRIPT_DATA_NUM_6));
	}

	private string SubstituteScriptDataNums(string rawText, EntityDef entityDef)
	{
		if (string.IsNullOrEmpty(rawText))
		{
			return string.Empty;
		}
		if (entityDef == null)
		{
			return rawText;
		}
		return TextUtils.TryFormat(rawText, GetTagText(entityDef, GAME_TAG.TAG_SCRIPT_DATA_NUM_1), GetTagText(entityDef, GAME_TAG.TAG_SCRIPT_DATA_NUM_2), GetTagText(entityDef, GAME_TAG.TAG_SCRIPT_DATA_NUM_3), GetTagText(entityDef, GAME_TAG.TAG_SCRIPT_DATA_NUM_4), GetTagText(entityDef, GAME_TAG.TAG_SCRIPT_DATA_NUM_5), GetTagText(entityDef, GAME_TAG.TAG_SCRIPT_DATA_NUM_6));
	}

	private string GetTagText(EntityBase entityBase, GAME_TAG tag)
	{
		if (m_GameTagToTagReferenceType == null || !m_GameTagToTagReferenceType.TryGetValue(tag, out var refType))
		{
			refType = TagReferenceType.None;
		}
		if (refType == TagReferenceType.CardDBID)
		{
			return GetTagText_CardDBID(entityBase, tag);
		}
		return entityBase.GetTag(tag).ToString();
	}

	private string GetTagText_CardDBID(EntityBase entityBase, GAME_TAG tag)
	{
		int cardDBID = entityBase.GetTag(tag);
		string name = GameStrings.Get("GAMEPLAY_UNKNOWN_CREATED_BY");
		if (cardDBID != 0)
		{
			CardDbfRecord cardRecord = GameDbf.Card.GetRecord(cardDBID);
			if (cardRecord != null)
			{
				name = cardRecord.Name;
			}
		}
		return name;
	}

	private string BuildCardTextForEntity(Entity entity)
	{
		if (entity == null)
		{
			if (HearthstoneApplication.IsPublic())
			{
				return "Error: parameter entity in " + MethodBase.GetCurrentMethod().ReflectedType.Name + "." + MethodBase.GetCurrentMethod().Name + "() is null.";
			}
			return string.Empty;
		}
		string rawText = CardTextBuilder.GetRawCardTextInHand(entity.GetCardId());
		string withScriptDataNums = SubstituteScriptDataNums(rawText, entity);
		string withSpellpowerAndHealingTokens = TextUtils.TransformCardText(entity, withScriptDataNums);
		return GetAlternateTextSubstring(withSpellpowerAndHealingTokens, entity);
	}

	public override string BuildCardTextInHand(Entity entity)
	{
		return BuildCardTextForEntity(entity);
	}

	public override string BuildCardTextInHand(EntityDef entityDef)
	{
		string rawText = CardTextBuilder.GetRawCardTextInHand(entityDef.GetCardId());
		string withSpellpowerAndHealingTokens = TextUtils.TransformCardText(SubstituteScriptDataNums(rawText, entityDef));
		return GetAlternateTextSubstring(withSpellpowerAndHealingTokens, entityDef);
	}

	public override string BuildCardTextInHistory(Entity entity)
	{
		return BuildCardTextForEntity(entity);
	}

	public override void OnTagChange(Card card, TagDelta tagChange)
	{
		switch ((GAME_TAG)tagChange.tag)
		{
		case GAME_TAG.TAG_SCRIPT_DATA_NUM_1:
		case GAME_TAG.TAG_SCRIPT_DATA_NUM_2:
		case GAME_TAG.USE_ALTERNATE_CARD_TEXT:
		case GAME_TAG.TAG_SCRIPT_DATA_NUM_3:
		case GAME_TAG.TAG_SCRIPT_DATA_NUM_4:
		case GAME_TAG.TAG_SCRIPT_DATA_NUM_5:
		case GAME_TAG.TAG_SCRIPT_DATA_NUM_6:
			if (card != null && card.GetActor() != null)
			{
				card.GetActor().UpdateTextComponents();
			}
			break;
		default:
			base.OnTagChange(card, tagChange);
			break;
		}
	}
}
