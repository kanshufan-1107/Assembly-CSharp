using System.Collections.Generic;

public class ScriptDataNum1CardTextBuilder : CardTextBuilder
{
	public ScriptDataNum1CardTextBuilder()
	{
		m_useEntityForTextInPlay = true;
	}

	protected static List<int> GetDelimiterIndexList(string text)
	{
		List<int> delimiterIndexList = new List<int>();
		for (int delimiterIndex = text.IndexOf('@'); delimiterIndex >= 0; delimiterIndex = text.IndexOf('@', delimiterIndex + 1))
		{
			delimiterIndexList.Add(delimiterIndex);
		}
		return delimiterIndexList;
	}

	protected string BuildCardTextInternal(Entity entity)
	{
		string rawText = CardTextBuilder.GetRawCardTextInHand(entity.GetCardId());
		string value = entity.GetTag(GAME_TAG.TAG_SCRIPT_DATA_NUM_1).ToString();
		string formattedText = "";
		List<int> delimiterIndexList = GetDelimiterIndexList(rawText);
		if (delimiterIndexList.Count == 2 && entity.GetEntityDef().GetTag(GAME_TAG.TAG_SCRIPT_DATA_NUM_1) == 0)
		{
			formattedText = rawText.Substring(0, delimiterIndexList[0]);
			formattedText += rawText.Substring(delimiterIndexList[0] + 1).Replace("@", value);
		}
		else
		{
			formattedText = rawText.Replace("@", value);
		}
		return TextUtils.TransformCardText(entity, formattedText);
	}

	public override string BuildCardTextInHand(Entity entity)
	{
		return BuildCardTextInternal(entity);
	}

	public override string BuildCardTextInHistory(Entity entity)
	{
		return BuildCardTextInternal(entity);
	}

	public override string BuildCardTextInHand(EntityDef entityDef)
	{
		string rawText = CardTextBuilder.GetRawCardTextInHand(entityDef.GetCardId());
		List<int> delimiterIndexList = GetDelimiterIndexList(rawText);
		if (delimiterIndexList.Count == 2 && entityDef.GetTag(GAME_TAG.TAG_SCRIPT_DATA_NUM_1) == 0)
		{
			rawText = rawText.Substring(0, delimiterIndexList[0]);
			return TextUtils.TransformCardText(rawText);
		}
		string value = entityDef.GetTag(GAME_TAG.TAG_SCRIPT_DATA_NUM_1).ToString();
		return TextUtils.TransformCardText(rawText.Replace("@", value));
	}

	public override void OnTagChange(Card card, TagDelta tagChange)
	{
		if (tagChange.tag == 2)
		{
			if (card != null && card.GetActor() != null)
			{
				card.GetActor().UpdateTextComponents();
			}
		}
		else
		{
			base.OnTagChange(card, tagChange);
		}
	}

	public override string GetTargetingArrowText(Entity entity)
	{
		string targetingArrowText = base.GetTargetingArrowText(entity);
		string value = entity.GetTag(GAME_TAG.TAG_SCRIPT_DATA_NUM_1).ToString();
		return TextUtils.TransformCardText(targetingArrowText.Replace("@", value));
	}
}
