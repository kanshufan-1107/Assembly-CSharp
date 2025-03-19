using System;

public class ModularEntityCardTextBuilder : CardTextBuilder
{
	public ModularEntityCardTextBuilder()
	{
		m_useEntityForTextInPlay = true;
	}

	public override string BuildCardTextInHand(Entity entity)
	{
		string formattedText = BuildFormattedText(entity);
		formattedText = TextUtils.TransformCardText(entity, formattedText);
		return formattedText.Trim(Environment.NewLine.ToCharArray());
	}

	public override string BuildCardTextInHand(EntityDef entityDef)
	{
		return string.Empty;
	}

	public override bool ContainsBonusDamageToken(Entity entity)
	{
		return TextUtils.HasBonusDamage(BuildFormattedText(entity));
	}

	public override bool ContainsBonusHealingToken(Entity entity)
	{
		return TextUtils.HasBonusHealing(BuildFormattedText(entity));
	}

	public virtual string GetRawCardTextInHandForCardBeingBuilt(Entity ent)
	{
		return CardTextBuilder.GetRawCardTextInHand(ent.GetCardId());
	}

	public override string BuildCardTextInHistory(Entity entity)
	{
		GetPowersText(entity, out var power1, out var power2);
		CardTextHistoryData cardTextHistoryData = entity.GetCardTextHistoryData();
		if (cardTextHistoryData == null)
		{
			Log.All.Print("ModularEntityCardTextBuilder.BuildCardTextInHistory: entity {0} does not have a CardTextHistoryData object.", entity.GetEntityId());
			return "";
		}
		string formattedText = TextUtils.TryFormat(GetRawCardTextInHandForCardBeingBuilt(entity), power1, power2);
		formattedText = TextUtils.TransformCardText(entity, cardTextHistoryData, formattedText);
		return formattedText.Trim(Environment.NewLine.ToCharArray());
	}

	protected void GetPowersText(Entity entity, out string power1, out string power2)
	{
		power1 = string.Empty;
		if (entity.HasTag(GAME_TAG.MODULAR_ENTITY_PART_1))
		{
			int databaseId = entity.GetTag(GAME_TAG.MODULAR_ENTITY_PART_1);
			EntityDef entityDef = DefLoader.Get().GetEntityDef(databaseId);
			if (entityDef != null)
			{
				power1 = CardTextBuilder.GetRawCardTextInHand(entityDef.GetCardId());
				power1 = GetPowerTextSubstring(power1);
			}
		}
		power2 = string.Empty;
		if (entity.HasTag(GAME_TAG.MODULAR_ENTITY_PART_2))
		{
			int databaseId2 = entity.GetTag(GAME_TAG.MODULAR_ENTITY_PART_2);
			EntityDef entityDef2 = DefLoader.Get().GetEntityDef(databaseId2);
			if (entityDef2 != null)
			{
				power2 = CardTextBuilder.GetRawCardTextInHand(entityDef2.GetCardId());
				power2 = GetPowerTextSubstring(power2);
			}
		}
	}

	protected virtual string BuildFormattedText(Entity entity)
	{
		GetPowersText(entity, out var power1, out var power2);
		return TextUtils.TryFormat(GetRawCardTextInHandForCardBeingBuilt(entity), power1, power2);
	}

	private string GetPowerTextSubstring(string powerText)
	{
		int delimiterIndex = powerText.IndexOf('@');
		if (delimiterIndex >= 0)
		{
			return powerText.Substring(delimiterIndex + 1);
		}
		return powerText;
	}
}
