using System.Collections.Generic;
using UnityEngine;

public class TeammatePingWheelManager : MonoBehaviour
{
	private Actor m_viewingActor;

	private Spell m_pingWheel;

	private bool m_disablePingWheels;

	private List<Actor> m_actorsWithPing = new List<Actor>();

	private static TeammatePingWheelManager s_instance;

	public void Awake()
	{
		s_instance = this;
	}

	public static TeammatePingWheelManager Get()
	{
		return s_instance;
	}

	public void AddActorHasPing(Actor actor)
	{
		m_actorsWithPing.Add(actor);
	}

	public void RemoveActorHasPing(Actor actor)
	{
		m_actorsWithPing.Remove(actor);
	}

	public void ClearAllPings()
	{
		foreach (Actor actor in new List<Actor>(m_actorsWithPing))
		{
			if (actor != null)
			{
				actor.RemovePing();
			}
		}
		m_actorsWithPing.Clear();
		HideAllPingWheels();
	}

	public void HidePingOptions(Actor actor)
	{
		if (!(m_pingWheel == null) && !(actor == null))
		{
			if ((bool)actor.GetCard())
			{
				actor.GetCard().FocusCardWhilePingwheelIsActive(pingWheelActive: false);
			}
			(GameState.Get().GetGameEntity() as TB_BaconShop).NotifyPingWheelClosed();
			actor.ActivateSpellDeathState(SpellType.TEAMMATE_PING_WHEEL);
			m_pingWheel = null;
			m_viewingActor = null;
		}
	}

	private void ShowPingOptions(Actor actor)
	{
		if (m_disablePingWheels || m_pingWheel != null)
		{
			return;
		}
		Spell pingWheel = actor.ActivateSpellBirthState(SpellType.TEAMMATE_PING_WHEEL);
		if (pingWheel == null)
		{
			return;
		}
		if ((bool)actor.GetCard())
		{
			actor.GetCard().FocusCardWhilePingwheelIsActive(pingWheelActive: true);
		}
		m_pingWheel = pingWheel;
		m_viewingActor = actor;
		if (actor.GetActivePingType() != 0)
		{
			actor.RemovePingAndNotifyTeammate();
		}
		for (int i = 0; i < m_pingWheel.transform.childCount; i++)
		{
			TeammatePingOptions pingOption = m_pingWheel.transform.GetChild(i).gameObject.GetComponent<TeammatePingOptions>();
			if (pingOption != null)
			{
				pingOption.ShowPingOption(actor);
			}
		}
		Transform pingObject = m_pingWheel.transform.GetChild(1);
		if (pingObject != null)
		{
			(GameState.Get().GetGameEntity() as TB_BaconShop).NotifyPingWheelActive(pingObject.gameObject);
		}
	}

	public void ShowPingWheel(Actor actor)
	{
		if (!actor.ArePingsBlocked())
		{
			if (m_viewingActor != actor)
			{
				HidePingOptions(m_viewingActor);
			}
			ShowPingOptions(actor);
		}
	}

	public void HideAllPingWheels()
	{
		HidePingOptions(m_viewingActor);
	}

	public void SetPingWheelDisabled(bool disabled)
	{
		m_disablePingWheels = disabled;
	}

	public Actor GetActorWithActivePingWheel()
	{
		return m_viewingActor;
	}
}
