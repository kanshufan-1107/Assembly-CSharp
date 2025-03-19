using UnityEngine;

public class DuosPortal : MonoBehaviour
{
	private enum PORTAL_STATE
	{
		BLUE,
		GREEN,
		GREEN_VACUUM,
		RED
	}

	public delegate void PortalClickedCallback();

	public delegate void PortalPingedCallback();

	private bool m_pressed;

	private bool m_clickable = true;

	private Actor m_heroActor;

	private Actor m_teammateHeroActor;

	private Actor m_viewingActor;

	public Texture m_defaultPortalTexture;

	private PORTAL_STATE m_portalColor;

	private PlayMakerFSM fsm;

	private PortalClickedCallback m_portalClickedCallback;

	private PortalPingedCallback m_portaPingedCallback;

	private void Start()
	{
		fsm = GetComponent<PlayMakerFSM>();
	}

	public void PlayPushDownAnimation()
	{
		m_pressed = true;
	}

	public void PlayButtonUpAnimation()
	{
		m_pressed = false;
	}

	public void HandleMouseOver()
	{
		PutInMouseOverState();
	}

	public void HandleMouseOut()
	{
		if (m_pressed)
		{
			PlayButtonUpAnimation();
		}
		PutInMouseOffState();
	}

	public void SetPortalClickedCallback(PortalClickedCallback callback)
	{
		m_portalClickedCallback = callback;
	}

	public void SetPortaPingedCallback(PortalPingedCallback callback)
	{
		m_portaPingedCallback = callback;
	}

	public void PortalPushed()
	{
		if (m_clickable)
		{
			if (m_portalClickedCallback != null)
			{
				m_portalClickedCallback();
			}
			PlayPushDownAnimation();
			if (TeammateBoardViewer.Get().IsViewingTeammate())
			{
				TeammateBoardViewer.Get().HideTeammateBoard();
				ApplyTeammateHeroTexture("ClickedPortal_ViewSelf");
			}
			else
			{
				TeammateBoardViewer.Get().ShowTeammateBoard();
				ApplyMyHeroTexture("ClickedPortal_ViewSelf");
			}
		}
	}

	public void PingPortal(Actor actor, int pingType)
	{
		if (pingType == 0)
		{
			RemovePingIfInteriorIsActor(actor);
			return;
		}
		Transform pingTransform = base.gameObject.transform.Find("Ping");
		if (!(pingTransform == null))
		{
			if (m_portaPingedCallback != null)
			{
				m_portaPingedCallback();
			}
			PlayMakerFSM component = pingTransform.GetComponent<PlayMakerFSM>();
			component.FsmVariables.GetFsmInt("PingType").Value = pingType;
			component.SendEvent("Birth");
			SetPortalInteriorTexture(actor, "PortalPinged");
		}
	}

	public void RemovePing()
	{
		Transform pingTransform = base.gameObject.transform.Find("Ping");
		if (!(pingTransform == null))
		{
			pingTransform.GetComponent<PlayMakerFSM>().SendEvent("Death");
		}
	}

	public void RemovePingIfInteriorIsActor(Actor actor)
	{
		if (!(actor == null) && actor == m_viewingActor)
		{
			RemovePing();
			ApplyHeroTexture(!TeammateBoardViewer.Get().IsViewingTeammate());
		}
	}

	public void SetPortalClickable(bool clickable)
	{
		m_clickable = clickable;
	}

	public void SetPortalInteriorTexture(Actor actor, string swapEvent)
	{
		Texture texture = ((actor != null) ? actor.GetStaticPortraitTexture(TAG_PREMIUM.NORMAL) : null);
		if (texture == null)
		{
			texture = m_defaultPortalTexture;
		}
		fsm.FsmVariables.GetFsmTexture("portraitNew").Value = texture;
		SendEventToPortal(swapEvent);
		m_viewingActor = actor;
	}

	public void SetHeroActor(Actor actor)
	{
		m_heroActor = actor;
	}

	public void SetTeammateHeroActor(Actor actor)
	{
		m_teammateHeroActor = actor;
	}

	public void ApplyMyHeroTexture(string swapEvent = "PortalPinged")
	{
		if (m_heroActor == null)
		{
			Card heroCard = GameState.Get().GetFriendlySidePlayer().GetHeroCard();
			if (GameState.Get().IsMulliganPhase() && MulliganManager.Get() != null && MulliganManager.Get().GetSelectedHero() == null)
			{
				heroCard = GameState.Get().GetOpposingSidePlayer().GetHeroCard();
			}
			Actor heroActor = ((heroCard != null) ? heroCard.GetActor() : null);
			SetHeroActor(heroActor);
		}
		if (!(m_viewingActor == m_heroActor))
		{
			SetPortalInteriorTexture(m_heroActor, swapEvent);
			RemovePing();
		}
	}

	public void ApplyTeammateHeroTexture(string swapEvent = "PortalPinged")
	{
		if (m_teammateHeroActor == null)
		{
			Card heroCard = TeammateBoardViewer.Get().GetTeammateHero()?.GetCard();
			Actor heroActor = ((heroCard != null) ? heroCard.GetActor() : null);
			SetTeammateHeroActor(heroActor);
		}
		if (!(m_viewingActor == m_teammateHeroActor))
		{
			SetPortalInteriorTexture(m_teammateHeroActor, swapEvent);
			RemovePing();
		}
	}

	public void ApplyHeroTexture(bool useTeammates)
	{
		if (useTeammates)
		{
			ApplyTeammateHeroTexture();
		}
		else
		{
			ApplyMyHeroTexture();
		}
	}

	public void ShowPortalGlow(Card heldCard, bool passable)
	{
		if (heldCard == null)
		{
			return;
		}
		if (passable)
		{
			if (heldCard.IsInDeckActionArea())
			{
				if (m_portalColor != PORTAL_STATE.GREEN_VACUUM)
				{
					SendEventToPortal("Portal_Green_VacuumOn");
					m_portalColor = PORTAL_STATE.GREEN_VACUUM;
				}
			}
			else if (m_portalColor != PORTAL_STATE.GREEN)
			{
				int passCost = heldCard.GetEntity().GetRealTimeDeckActionCost();
				fsm.Fsm.GetFsmString("PassPrice").Value = passCost.ToString();
				if (m_portalColor == PORTAL_STATE.GREEN_VACUUM)
				{
					SendEventToPortal("Portal_Green_VacuumOff");
				}
				else
				{
					SendEventToPortal("Portal_Green");
				}
				m_portalColor = PORTAL_STATE.GREEN;
			}
		}
		else if (m_portalColor != PORTAL_STATE.RED)
		{
			SendEventToPortal("Portal_Red");
			m_portalColor = PORTAL_STATE.RED;
		}
	}

	public void ClearPortalGlow()
	{
		if (m_portalColor != 0)
		{
			SendEventToPortal("Portal_Blue");
			m_portalColor = PORTAL_STATE.BLUE;
		}
	}

	public void Shrink()
	{
		SendEventToPortal("Portal_Shrink");
	}

	public void Grow()
	{
		SendEventToPortal("Portal_Grow");
		m_portalColor = PORTAL_STATE.BLUE;
	}

	private void PutInMouseOverState()
	{
	}

	private void PutInMouseOffState()
	{
	}

	private void SendEventToPortal(string portalEvent)
	{
		fsm.SendEvent(portalEvent);
	}
}
