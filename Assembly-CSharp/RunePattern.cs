using PegasusShared;
using UnityEngine;

public struct RunePattern
{
	public static readonly RuneType[] ValidRuneTypes = new RuneType[3]
	{
		RuneType.RT_BLOOD,
		RuneType.RT_FROST,
		RuneType.RT_UNHOLY
	};

	private int m_blood;

	private int m_frost;

	private int m_unholy;

	public int Blood
	{
		get
		{
			return m_blood;
		}
		private set
		{
			m_blood = ((value >= 0) ? value : 0);
		}
	}

	public int Frost
	{
		get
		{
			return m_frost;
		}
		private set
		{
			m_frost = ((value >= 0) ? value : 0);
		}
	}

	public int Unholy
	{
		get
		{
			return m_unholy;
		}
		private set
		{
			m_unholy = ((value >= 0) ? value : 0);
		}
	}

	public bool HasRunes => CombinedValue > 0;

	public int CombinedValue => Blood + Frost + Unholy;

	public bool HasMaxAmountOfOneRuneType
	{
		get
		{
			if (Blood != DeckRule_DeathKnightRuneLimit.MaxRuneSlots && Frost != DeckRule_DeathKnightRuneLimit.MaxRuneSlots)
			{
				return Unholy == DeckRule_DeathKnightRuneLimit.MaxRuneSlots;
			}
			return true;
		}
	}

	public RunePattern(int blood = 0, int frost = 0, int unholy = 0)
	{
		this = default(RunePattern);
		Blood = blood;
		Frost = frost;
		Unholy = unholy;
	}

	public RunePattern(EntityBase entityBase)
	{
		this = default(RunePattern);
		if (entityBase != null)
		{
			Blood = entityBase.GetTag(GAME_TAG.COST_BLOOD);
			Frost = entityBase.GetTag(GAME_TAG.COST_FROST);
			Unholy = entityBase.GetTag(GAME_TAG.COST_UNHOLY);
		}
	}

	public RunePattern(RuneType[] runes)
	{
		this = default(RunePattern);
		if (runes != null)
		{
			foreach (RuneType runeType in runes)
			{
				AddRunes(runeType, 1);
			}
		}
	}

	public void SetCostsFromEntity(EntityBase entityBase)
	{
		if (entityBase != null)
		{
			Blood = entityBase.GetTag(GAME_TAG.COST_BLOOD);
			Frost = entityBase.GetTag(GAME_TAG.COST_FROST);
			Unholy = entityBase.GetTag(GAME_TAG.COST_UNHOLY);
		}
	}

	public int GetCost(RuneType rune)
	{
		return rune switch
		{
			RuneType.RT_BLOOD => Blood, 
			RuneType.RT_FROST => Frost, 
			RuneType.RT_UNHOLY => Unholy, 
			_ => 0, 
		};
	}

	public bool CanAddRunes(RunePattern runesToAdd, int maxRuneSlots)
	{
		int totalRunes = CombinedValue;
		RuneType[] validRuneTypes = ValidRuneTypes;
		foreach (RuneType runeType in validRuneTypes)
		{
			int amountToAdd = runesToAdd.GetCost(runeType);
			int currentRuneAmount = GetCost(runeType);
			if (amountToAdd > currentRuneAmount)
			{
				totalRunes += amountToAdd - currentRuneAmount;
			}
			if (totalRunes > maxRuneSlots)
			{
				return false;
			}
		}
		return true;
	}

	public bool Matches(RunePattern other)
	{
		if (other.Blood == Blood && other.Frost == Frost)
		{
			return other.Unholy == Unholy;
		}
		return false;
	}

	public RunePattern CombineRunes(RunePattern runesToAdd, int maxRuneSlots)
	{
		RunePattern result = default(RunePattern);
		int emptySlots = maxRuneSlots - CombinedValue;
		int bloodToAdd = runesToAdd.Blood - Blood;
		if (bloodToAdd > 0 && emptySlots > 0)
		{
			Blood = Mathf.Min(runesToAdd.Blood, emptySlots);
			result.Blood = Mathf.Min(bloodToAdd, emptySlots);
			emptySlots -= Blood;
		}
		int frostToAdd = runesToAdd.Frost - Frost;
		if (frostToAdd > 0 && emptySlots > 0)
		{
			Frost = Mathf.Min(runesToAdd.Frost, emptySlots);
			result.Frost = Mathf.Min(frostToAdd, emptySlots);
			emptySlots -= Frost;
		}
		int unholyToAdd = runesToAdd.Unholy - Unholy;
		if (unholyToAdd > 0 && emptySlots > 0)
		{
			Unholy = Mathf.Min(runesToAdd.Unholy, emptySlots);
			result.Unholy = Mathf.Min(unholyToAdd, emptySlots);
		}
		return result;
	}

	public void AddRunes(RuneType rune, int amount)
	{
		switch (rune)
		{
		case RuneType.RT_BLOOD:
			Blood += amount;
			break;
		case RuneType.RT_FROST:
			Frost += amount;
			break;
		case RuneType.RT_UNHOLY:
			Unholy += amount;
			break;
		}
	}

	public RuneType[] ToArray()
	{
		if (CombinedValue <= 0)
		{
			return new RuneType[0];
		}
		RuneType[] runes = new RuneType[CombinedValue];
		int count = 0;
		RuneType[] validRuneTypes = ValidRuneTypes;
		foreach (RuneType runeType in validRuneTypes)
		{
			int runeCost = GetCost(runeType);
			for (int j = 0; j < runeCost; j++)
			{
				runes[count] = runeType;
				count++;
			}
		}
		return runes;
	}
}
