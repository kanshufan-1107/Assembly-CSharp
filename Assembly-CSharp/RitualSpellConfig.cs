using System;
using System.Collections.Generic;
using PegasusGame;
using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/RitualSpellConfig", order = 1)]
public class RitualSpellConfig : ScriptableObject
{
	[Serializable]
	public class ClassSpecificRitualConfig
	{
		public TAG_CLASS m_class;

		public Spell m_ritualPortalSpell;

		public Spell m_ritualEffectSpell;
	}

	public List<ClassSpecificRitualConfig> m_classSpecificRitualConfig = new List<ClassSpecificRitualConfig>();

	public List<string> m_ritualCardIds = new List<string>();

	public TAG_CARDTYPE m_ritualEntityType = TAG_CARDTYPE.HERO;

	public string m_friendlyBoneName = "FriendlyRitual";

	public string m_opponentBoneName = "OpponentRitual";

	public bool m_hideRitualActor = true;

	public bool m_showAttack;

	public bool m_showHealth;

	public bool m_showArmor;

	public GAME_TAG m_proxyRitualEntityTag;

	public bool m_showRitualVisualsInPlay;

	public string m_portalSpellEventName = "showRitualActor";

	public bool IsRitualEntity(Entity entity)
	{
		if (entity == null)
		{
			return false;
		}
		string cardId = entity.GetCardId();
		return IsRitualEntity(cardId);
	}

	public bool IsRitualEntity(string cardId)
	{
		return m_ritualCardIds.Contains(cardId);
	}

	public bool IsRitualEntityInPlay(Player controller)
	{
		if (controller == null)
		{
			return false;
		}
		switch (m_ritualEntityType)
		{
		case TAG_CARDTYPE.HERO:
		{
			Entity hero = controller.GetHero();
			if (hero == null)
			{
				return false;
			}
			return IsRitualEntity(hero);
		}
		case TAG_CARDTYPE.MINION:
			foreach (Card card2 in controller.GetBattlefieldZone().GetCards())
			{
				if (IsRitualEntity(card2.GetEntity()))
				{
					return true;
				}
			}
			break;
		case TAG_CARDTYPE.WEAPON:
		{
			Card weapon = controller.GetWeaponCard();
			return IsRitualEntity(weapon.GetEntity());
		}
		case TAG_CARDTYPE.SPELL:
			foreach (Card card in controller.GetSecretZone().GetCards())
			{
				if (IsRitualEntity(card.GetEntity()))
				{
					return true;
				}
			}
			break;
		}
		return false;
	}

	public Spell GetRitualActivateSpell(Entity ritualEntity)
	{
		if (!IsRitualEntity(ritualEntity))
		{
			return null;
		}
		return GetRitualSpellForClass(ritualEntity.GetClass(), isActivate: true);
	}

	public Spell GetRitualTriggerSpell(Entity ritualEntity)
	{
		if (!IsRitualEntity(ritualEntity))
		{
			return null;
		}
		return GetRitualSpellForClass(ritualEntity.GetClass(), isActivate: false);
	}

	private Spell GetRitualSpellForClass(TAG_CLASS entityClass, bool isActivate)
	{
		foreach (ClassSpecificRitualConfig config in m_classSpecificRitualConfig)
		{
			if (config.m_class == entityClass)
			{
				if (isActivate)
				{
					return config.m_ritualPortalSpell;
				}
				return config.m_ritualEffectSpell;
			}
		}
		return null;
	}

	public bool DoesTaskListContainRitualEntity(PowerTaskList powerTaskList, int entityID)
	{
		if (powerTaskList.GetBlockType() != HistoryBlock.Type.TRIGGER)
		{
			return false;
		}
		foreach (PowerTask task in powerTaskList.GetTaskList())
		{
			if (task.GetPower() is Network.HistChangeEntity changeEntity && changeEntity.Entity.ID == entityID && IsRitualEntity(changeEntity.Entity.CardID))
			{
				return true;
			}
		}
		return false;
	}

	public bool DoesFutureTaskListContainsRitualEntity(List<PowerTaskList> futureTaskLists, PowerTaskList currentTaskList, int entityID)
	{
		foreach (PowerTaskList powerTaskList in futureTaskLists)
		{
			if (powerTaskList != null && powerTaskList.IsDescendantOfBlock(currentTaskList) && DoesTaskListContainRitualEntity(powerTaskList, entityID))
			{
				return true;
			}
		}
		return false;
	}

	public Actor LoadRitualActor(Entity entity)
	{
		if (entity == null)
		{
			return null;
		}
		GameObject ritualActorGO = AssetLoader.Get().InstantiatePrefab(ActorNames.GetZoneActor(entity, TAG_ZONE.PLAY), AssetLoadingOptions.IgnorePrefabPosition);
		if (ritualActorGO == null)
		{
			Log.Spells.PrintError("RitualSpellConfig unable to load Invoke Actor GameObject.");
			return null;
		}
		Actor ritualActor = ritualActorGO.GetComponent<Actor>();
		if (ritualActor == null)
		{
			Log.Spells.PrintError("RitualSpellConfig Invoke Actor GameObject contains no Actor component.");
			UnityEngine.Object.Destroy(ritualActorGO);
			return null;
		}
		ritualActor.SetEntity(entity);
		ritualActor.SetCardDefFromEntity(entity);
		return ritualActor;
	}

	public void UpdateAndPositionActor(Actor actor)
	{
		if (!(actor == null))
		{
			if (m_hideRitualActor)
			{
				actor.Hide();
			}
			string invokeBoneName = ((actor.GetEntity().GetControllerSide() == Player.Side.FRIENDLY) ? m_friendlyBoneName : m_opponentBoneName);
			Transform invokeBone = Board.Get().FindBone(invokeBoneName);
			actor.transform.parent = invokeBone;
			actor.transform.localPosition = Vector3.zero;
		}
	}

	public void UpdateRitualActorComponents(Actor actor)
	{
		actor?.m_attackObject?.SetActive(m_showAttack);
		actor?.m_healthObject?.SetActive(m_showHealth);
		actor?.m_armorSpellBone?.SetActive(m_showArmor);
	}
}
