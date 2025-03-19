using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SideQuestSpellController : SpellController
{
	public List<SideQuestBannerDef> m_BannerDefs;

	public Spell m_DefaultBannerSpellPrefab;

	private Spell m_bannerSpell;

	private Spell m_triggerSpell;

	private Entity m_sourceEntity;

	protected override bool AddPowerSourceAndTargets(PowerTaskList taskList)
	{
		if (!HasSourceCard(taskList))
		{
			return false;
		}
		Entity sourceEntity = taskList.GetSourceEntity();
		Card sourceCard = sourceEntity.GetCard();
		m_sourceEntity = sourceEntity;
		bool success = false;
		if (taskList.IsStartOfBlock() && InitBannerSpell(sourceEntity))
		{
			success = true;
		}
		Spell triggerSpell = GetTriggerSpell(sourceCard);
		if (triggerSpell != null && InitTriggerSpell(sourceCard, triggerSpell))
		{
			success = true;
		}
		if (!success)
		{
			return false;
		}
		SetSource(sourceCard);
		return true;
	}

	protected override void OnProcessTaskList()
	{
		GetSource().SetSecretTriggered(set: true);
		if (m_taskList.IsStartOfBlock())
		{
			FireSideQuestActorSpell();
			if (FireBannerSpell())
			{
				return;
			}
		}
		if (!FireTriggerSpell())
		{
			base.OnProcessTaskList();
		}
	}

	private bool FireSideQuestActorSpell()
	{
		GetSource().UpdateSideQuestUI(allowQuestComplete: true);
		return true;
	}

	private Spell GetTriggerSpell(Card card)
	{
		Network.HistBlockStart blockStart = m_taskList.GetBlockStart();
		return card.GetTriggerSpell(blockStart.EffectIndex);
	}

	private bool InitTriggerSpell(Card card, Spell triggerSpell)
	{
		if (!triggerSpell.AttachPowerTaskList(m_taskList))
		{
			Network.HistBlockStart blockStart = m_taskList.GetBlockStart();
			Log.Power.Print($"{this}.InitTriggerSpell() - FAILED to attach task list to trigger spell {blockStart.EffectIndex} for {card}");
			return false;
		}
		return true;
	}

	private bool FireTriggerSpell()
	{
		Card card = GetSource();
		Spell triggerSpell = GetTriggerSpell(card);
		if (triggerSpell == null)
		{
			return false;
		}
		if (triggerSpell.GetPowerTaskList() != m_taskList && !InitTriggerSpell(card, triggerSpell))
		{
			return false;
		}
		triggerSpell.AddFinishedCallback(OnTriggerSpellFinished);
		triggerSpell.AddStateFinishedCallback(OnTriggerSpellStateFinished);
		triggerSpell.SafeActivateState(SpellStateType.ACTION);
		return true;
	}

	private void OnTriggerSpellFinished(Spell triggerSpell, object userData)
	{
		OnFinishedTaskList();
	}

	private void OnTriggerSpellStateFinished(Spell triggerSpell, SpellStateType prevStateType, object userData)
	{
		if (triggerSpell.GetActiveState() == SpellStateType.NONE)
		{
			OnFinished();
		}
	}

	private Spell DetermineBannerSpellPrefab(Entity sourceEntity)
	{
		if (m_BannerDefs == null)
		{
			return null;
		}
		TAG_CLASS classTag = sourceEntity.GetClass();
		SpellClassTag spellClassTag = SpellUtils.ConvertClassTagToSpellEnum(classTag);
		if (spellClassTag == SpellClassTag.NONE)
		{
			Debug.LogWarning($"{this}.DetermineBannerSpellPrefab() - entity {sourceEntity} has unknown class tag {classTag}. Using default banner.");
		}
		else if (m_BannerDefs != null && m_BannerDefs.Count > 0)
		{
			for (int i = 0; i < m_BannerDefs.Count; i++)
			{
				SideQuestBannerDef bannerDef = m_BannerDefs[i];
				if (spellClassTag == bannerDef.m_HeroClass)
				{
					return bannerDef.m_SpellPrefab;
				}
			}
			Log.Asset.Print($"{this}.DetermineBannerSpellPrefab() - class type {spellClassTag} has no Banner Def. Using default banner.");
		}
		return m_DefaultBannerSpellPrefab;
	}

	private bool InitBannerSpell(Entity sourceEntity)
	{
		Spell bannerSpellPrefab = DetermineBannerSpellPrefab(sourceEntity);
		if (bannerSpellPrefab == null)
		{
			return false;
		}
		GameObject bannerObject = Object.Instantiate(bannerSpellPrefab.gameObject);
		m_bannerSpell = bannerObject.GetComponent<Spell>();
		return true;
	}

	private bool FireBannerSpell()
	{
		if (m_bannerSpell == null)
		{
			return false;
		}
		StartCoroutine(ContinueWithSecretEvents());
		m_bannerSpell.AddStateFinishedCallback(OnBannerSpellStateFinished);
		m_bannerSpell.Activate();
		return true;
	}

	private IEnumerator ContinueWithSecretEvents()
	{
		if (m_sourceEntity == null || !m_sourceEntity.IsBobQuest())
		{
			yield return new WaitForSeconds(1f);
			while (!HistoryManager.Get().HasBigCard())
			{
				yield return null;
			}
			HistoryManager.Get().NotifyOfSecretSpellFinished();
			yield return new WaitForSeconds(1f);
		}
		if (!FireTriggerSpell())
		{
			base.OnProcessTaskList();
		}
	}

	private void OnBannerSpellStateFinished(Spell spell, SpellStateType prevStateType, object userData)
	{
		if (m_bannerSpell.GetActiveState() == SpellStateType.NONE)
		{
			Object.Destroy(m_bannerSpell.gameObject);
			m_bannerSpell = null;
		}
	}
}
