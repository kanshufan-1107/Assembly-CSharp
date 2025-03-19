using UnityEngine;

internal class ObjectiveController : MonoBehaviour
{
	private Actor m_actor;

	private Entity m_entity;

	private Spell m_spell;

	private void Awake()
	{
		m_actor = GetComponent<Actor>();
		if (m_actor == null)
		{
			Log.Gameplay.PrintError("ObjectiveController.Awake(): GameObject " + base.gameObject.name + " does not have an Actor Component!");
		}
		m_spell = GetComponent<Spell>();
		if (m_spell == null)
		{
			Log.Gameplay.PrintError("ObjectiveController.Awake(): GameObject " + base.gameObject.name + " does not have a Spell Component!");
		}
	}

	public void UpdateObjectiveUI()
	{
		if (IsEntityValid() && m_entity.GetCardId() == "ETC_335" && m_entity.GetTag(GAME_TAG.QUEST_PROGRESS) == 0)
		{
			m_spell.ActivateState(SpellStateType.IDLE);
		}
	}

	private bool IsEntityValid()
	{
		if (m_entity == null)
		{
			m_entity = m_actor.GetEntity();
			if (m_entity == null)
			{
				return false;
			}
		}
		return true;
	}
}
