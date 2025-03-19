using UnityEngine;

public class HistoryInfo
{
	public HistoryInfoType m_infoType;

	public int m_damageChangeAmount;

	public int m_armorChangeAmount;

	public int m_maxHealthChangeAmount;

	public bool m_dontDuplicateUntilEnd;

	public bool m_isBurnedCard;

	public bool m_isPoisonous;

	public bool m_isCriticalHit;

	public TAG_CARD_ALTERNATE_COST m_splatType;

	private Entity m_originalEntity;

	private Entity m_duplicatedEntity;

	private bool m_died;

	public int GetSplatAmount()
	{
		int damageAdjustment = Mathf.Min((m_duplicatedEntity ?? m_originalEntity).GetDamage(), Mathf.Max(0, -m_maxHealthChangeAmount));
		int actualDamageDealt = m_damageChangeAmount + damageAdjustment;
		if (m_armorChangeAmount <= 0)
		{
			return actualDamageDealt;
		}
		return actualDamageDealt + m_armorChangeAmount;
	}

	public int GetCurrentVitality()
	{
		Entity entity = m_duplicatedEntity ?? m_originalEntity;
		int currentVitality = entity.GetCurrentVitality();
		int vitalityAdjustment = m_maxHealthChangeAmount;
		if (vitalityAdjustment < 0)
		{
			int startingDamage = entity.GetDamage();
			vitalityAdjustment = Mathf.Min(0, startingDamage + m_maxHealthChangeAmount);
		}
		return currentVitality + vitalityAdjustment;
	}

	public bool HasValidDisplayEntity()
	{
		HistoryInfoType infoType = m_infoType;
		if ((uint)(infoType - 6) <= 1u)
		{
			return true;
		}
		if (GetDuplicatedEntity() == null)
		{
			return false;
		}
		if (GetDuplicatedEntity().IsHidden() && !GetDuplicatedEntity().IsHiddenSecret() && !GetDuplicatedEntity().IsHiddenForge())
		{
			return false;
		}
		return true;
	}

	public Entity GetDuplicatedEntity()
	{
		return m_duplicatedEntity;
	}

	public Entity GetOriginalEntity()
	{
		return m_originalEntity;
	}

	public void SetOriginalEntity(Entity entity)
	{
		m_originalEntity = entity;
		DuplicateEntity(duplicateHiddenNonSecret: false, isEndOfHistory: false);
	}

	public bool HasDied()
	{
		Entity entity = m_duplicatedEntity ?? m_originalEntity;
		if (!entity.IsCharacter() && !entity.IsWeapon())
		{
			return false;
		}
		if (m_died)
		{
			return true;
		}
		if (GetSplatAmount() >= GetCurrentVitality())
		{
			return true;
		}
		return false;
	}

	public void SetDied(bool set)
	{
		m_died = set;
	}

	public bool CanDuplicateEntity(bool duplicateHiddenNonSecret, bool isEndOfHistory = false)
	{
		if (m_originalEntity == null)
		{
			return false;
		}
		if (m_originalEntity.GetLoadState() != Entity.LoadState.DONE)
		{
			return false;
		}
		if (!isEndOfHistory && m_dontDuplicateUntilEnd)
		{
			return false;
		}
		if (!m_originalEntity.IsHidden())
		{
			return true;
		}
		if (!GameUtils.IsEntityHiddenAfterCurrentTasklist(m_originalEntity))
		{
			return false;
		}
		if (m_originalEntity.IsSecret())
		{
			return true;
		}
		if (m_originalEntity.HasTag(GAME_TAG.FORGE_REVEALED))
		{
			return true;
		}
		if (duplicateHiddenNonSecret)
		{
			return true;
		}
		return false;
	}

	public void DuplicateEntity(bool duplicateHiddenNonSecret, bool isEndOfHistory)
	{
		if (m_duplicatedEntity == null && CanDuplicateEntity(duplicateHiddenNonSecret, isEndOfHistory))
		{
			m_duplicatedEntity = m_originalEntity.CloneForHistory(this);
			if (m_infoType == HistoryInfoType.CARD_PLAYED || m_infoType == HistoryInfoType.WEAPON_PLAYED)
			{
				m_duplicatedEntity.SetTag(GAME_TAG.COST, m_originalEntity.GetTag(GAME_TAG.TAG_LAST_KNOWN_COST_IN_HAND));
			}
		}
	}

	public TAG_CARD_ALTERNATE_COST GetSplatType()
	{
		return (m_duplicatedEntity ?? m_originalEntity)?.GetAlternateCost() ?? TAG_CARD_ALTERNATE_COST.MANA;
	}
}
