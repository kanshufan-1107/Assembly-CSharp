using System.Collections;
using UnityEngine;

public class DuosPopupTutoiral : MonoBehaviour
{
	public enum DUOS_TUTORIALS
	{
		VIEW_TEAMMATE,
		VIEW_SELF,
		HEALTH,
		COMBAT,
		HERO_SWAP,
		PASS,
		RECEIVE,
		COMBAT_ORDER,
		PING,
		PING_WHEEL,
		TEAMMATE_PINGED,
		BONUS_DAMAGE,
		COUNT
	}

	private static DuosPopupTutoiral s_instance;

	private DuosPortal m_duosPortal;

	private Notification[] m_duosTutorialPopup = new Notification[12];

	private Coroutine[] m_duoTutorialCorutines = new Coroutine[12];

	private bool m_portalTutorialActive;

	[EnumNamedArray(typeof(DUOS_TUTORIALS))]
	public Vector3[] m_positionOffsets = new Vector3[12];

	[EnumNamedArray(typeof(DUOS_TUTORIALS))]
	public float[] m_scales = new float[12];

	private const float TUTORIAL_OFFSET_FROM_PORTAL = 2.5f;

	private const float TUTORIAL_OFFSET_FROM_HEALTH = 1.5f;

	public void Awake()
	{
		s_instance = this;
	}

	public static DuosPopupTutoiral Get()
	{
		return s_instance;
	}

	public void SetDuosPortal(DuosPortal portal)
	{
		m_duosPortal = portal;
	}

	public void StopDuosTutorialPopup(DUOS_TUTORIALS duosTutorial)
	{
		NotificationManager.Get()?.DestroyNotification(m_duosTutorialPopup[(int)duosTutorial], 0f);
		if (IsPortalTutorial(duosTutorial))
		{
			m_portalTutorialActive = false;
		}
	}

	public void StopAllDuosTutorialPopups()
	{
		Notification[] duosTutorialPopup = m_duosTutorialPopup;
		foreach (Notification popup in duosTutorialPopup)
		{
			NotificationManager.Get()?.DestroyNotification(popup, 0f);
		}
	}

	public void PlayDuosTutorial(DUOS_TUTORIALS duosTutorial, Card targetCard = null, Actor targetActor = null, GameObject go = null, Player.Side side = Player.Side.NEUTRAL)
	{
		if (GameMgr.Get().IsBattlegroundDuoGame())
		{
			switch (duosTutorial)
			{
			case DUOS_TUTORIALS.VIEW_TEAMMATE:
				StartDuoTutorialCorutine(duosTutorial, Option.BG_DUOS_TUTORIAL_PLAYED_VIEW_TEAMMATE, PlayViewTeammteTutorial());
				break;
			case DUOS_TUTORIALS.VIEW_SELF:
				StartDuoTutorialCorutine(duosTutorial, Option.BG_DUOS_TUTORIAL_PLAYED_VIEW_SELF, PlayReturnToBoardTutorial());
				break;
			case DUOS_TUTORIALS.HEALTH:
				StartDuoTutorialCorutine(duosTutorial, Option.BG_DUOS_TUTORIAL_PLAYED_HEALTH, PlayHealthTutorial(targetActor));
				break;
			case DUOS_TUTORIALS.COMBAT:
				StartDuoTutorialCorutine(duosTutorial, Option.BG_DUOS_TUTORIAL_PLAYED_COMBAT, PlayCombatTutorial());
				break;
			case DUOS_TUTORIALS.HERO_SWAP:
				StartDuoTutorialCorutine(duosTutorial, Option.BG_DUOS_TUTORIAL_PLAYED_HERO_SWAP, PlayHeroSwapTutorial(targetActor));
				break;
			case DUOS_TUTORIALS.PASS:
				StartDuoTutorialCorutine(duosTutorial, Option.BG_DUOS_TUTORIAL_PLAYED_PASS, PlayPassTutorial());
				break;
			case DUOS_TUTORIALS.RECEIVE:
				StartDuoTutorialCorutine(duosTutorial, Option.BG_DUOS_TUTORIAL_PLAYED_RECEIVE, PlayReceiveTutorial(targetCard));
				break;
			case DUOS_TUTORIALS.COMBAT_ORDER:
				StartDuoTutorialCorutine(duosTutorial, Option.BG_DUOS_TUTORIAL_PLAYED_COMBAT_ORDER, PlayCombatOrderTutorial());
				break;
			case DUOS_TUTORIALS.PING:
				StartDuoTutorialCorutine(duosTutorial, Option.BG_DUOS_TUTORIAL_PLAYED_PING, PlayPingTutorial());
				break;
			case DUOS_TUTORIALS.PING_WHEEL:
				StartDuoTutorialCorutine(duosTutorial, Option.BG_DUOS_TUTORIAL_PLAYED_PING_WHEEL, PlayPingWheelTutorial(go));
				break;
			case DUOS_TUTORIALS.TEAMMATE_PINGED:
				StartDuoTutorialCorutine(duosTutorial, Option.BG_DUOS_TUTORIAL_PLAYED_TEAMMATE_PINGED, PlayTeammatePingedTutorial());
				break;
			case DUOS_TUTORIALS.BONUS_DAMAGE:
				StartDuoTutorialCorutine(duosTutorial, Option.BG_DUOS_TUTORIAL_PLAYED_BONUS_DAMAGE, PlayBonusDamageTutorial(side));
				break;
			}
		}
	}

	private bool IsPortalTutorial(DUOS_TUTORIALS duosTutorial)
	{
		if (duosTutorial == DUOS_TUTORIALS.VIEW_TEAMMATE || duosTutorial == DUOS_TUTORIALS.PASS || duosTutorial == DUOS_TUTORIALS.TEAMMATE_PINGED)
		{
			return true;
		}
		return false;
	}

	private void StartDuoTutorialCorutine(DUOS_TUTORIALS duosTutorial, Option savedFlag, IEnumerator routine)
	{
		if ((!IsPortalTutorial(duosTutorial) || !m_portalTutorialActive) && !Options.Get().GetBool(savedFlag, defaultVal: false) && m_duoTutorialCorutines[(int)duosTutorial] == null)
		{
			m_duoTutorialCorutines[(int)duosTutorial] = StartCoroutine(routine);
		}
	}

	private void PlayDuosTutorialPopup(DUOS_TUTORIALS tutorial, Vector3 position, Notification.PopUpArrowDirection direction, string text, string phoneText = null)
	{
		if (phoneText != null && (bool)UniversalInputManager.UsePhoneUI)
		{
			text = phoneText;
		}
		float scale = m_scales[(int)tutorial];
		Vector3 offsetPostion = position + m_positionOffsets[(int)tutorial];
		Notification notification = NotificationManager.Get().CreatePopupText(UserAttentionBlocker.NONE, offsetPostion, new Vector3(scale, scale, scale), GameStrings.Get(text));
		notification.ShowPopUpArrow(direction);
		notification.PulseReminderEveryXSeconds(3f);
		m_duosTutorialPopup[(int)tutorial] = notification;
		if (IsPortalTutorial(tutorial))
		{
			m_portalTutorialActive = true;
		}
	}

	private IEnumerator PlayViewTeammteTutorial()
	{
		while (m_duosPortal == null)
		{
			yield return null;
		}
		yield return new WaitForSeconds(2f);
		Vector3 position = m_duosPortal.transform.position;
		PlayDuosTutorialPopup(DUOS_TUTORIALS.VIEW_TEAMMATE, position, Notification.PopUpArrowDirection.Down, "GAMEPLAY_BACON_DUO_VIEW_TEAMMATE_TUTORIAL", "GAMEPLAY_BACON_DUO_VIEW_TEAMMATE_TUTORIAL_PHONE");
		Options.Get().SetBool(Option.BG_DUOS_TUTORIAL_PLAYED_VIEW_TEAMMATE, val: true);
	}

	private IEnumerator PlayReturnToBoardTutorial()
	{
		while (m_duosPortal == null)
		{
			yield return null;
		}
		NotificationManager.Get()?.DestroyNotification(m_duosTutorialPopup[0], 0f);
		yield return new WaitForSeconds(0.5f);
		Vector3 position = m_duosPortal.transform.position;
		PlayDuosTutorialPopup(DUOS_TUTORIALS.VIEW_SELF, position, Notification.PopUpArrowDirection.Down, "GAMEPLAY_BACON_DUO_VIEW_SELF_TUTORIAL", "GAMEPLAY_BACON_DUO_VIEW_SELF_TUTORIAL_PHONE");
		Options.Get().SetBool(Option.BG_DUOS_TUTORIAL_PLAYED_VIEW_SELF, val: true);
	}

	private IEnumerator PlayHealthTutorial(Actor friendlyHeroActor)
	{
		yield return new WaitForSeconds(7f);
		Vector3 position = friendlyHeroActor.GetHealthObject().transform.position;
		PlayDuosTutorialPopup(DUOS_TUTORIALS.HEALTH, position, Notification.PopUpArrowDirection.Left, "GAMEPLAY_BACON_DUO_HEALTH_TUTORIAL");
		Options.Get().SetBool(Option.BG_DUOS_TUTORIAL_PLAYED_HEALTH, val: true);
		yield return new WaitForSeconds(5f);
		StopDuosTutorialPopup(DUOS_TUTORIALS.HEALTH);
	}

	private IEnumerator PlayCombatTutorial()
	{
		while (!TurnTimer.Get().IsRopeActive())
		{
			yield return null;
		}
		int playerTeamId = GameState.Get().GetFriendlySidePlayer().GetTag(GAME_TAG.BACON_DUO_TEAM_ID);
		PlayerLeaderboardTeam team = PlayerLeaderboardManager.Get().GetTeamForTeamId(playerTeamId);
		if (!(team == null))
		{
			Vector3 position = team.Members[0].transform.position;
			PlayDuosTutorialPopup(DUOS_TUTORIALS.COMBAT, position, Notification.PopUpArrowDirection.Left, "GAMEPLAY_BACON_DUO_COMBAT_TUTORIAL");
			Options.Get().SetBool(Option.BG_DUOS_TUTORIAL_PLAYED_COMBAT, val: true);
		}
	}

	private IEnumerator PlayHeroSwapTutorial(Actor targetActor)
	{
		yield return new WaitForSeconds(0.5f);
		if (!(targetActor.GetHealthObject() == null))
		{
			Vector3 position = targetActor.GetHealthObject().transform.position;
			PlayDuosTutorialPopup(DUOS_TUTORIALS.HERO_SWAP, position, Notification.PopUpArrowDirection.Left, "GAMEPLAY_BACON_DUO_HERO_SWAP_TUTORIAL");
			GameState.Get().SetBusy(busy: true);
			Options.Get().SetBool(Option.BG_DUOS_TUTORIAL_PLAYED_HERO_SWAP, val: true);
			yield return new WaitForSeconds(2f);
			GameState.Get().SetBusy(busy: false);
			yield return new WaitForSeconds(5f);
			StopDuosTutorialPopup(DUOS_TUTORIALS.HERO_SWAP);
		}
	}

	private IEnumerator PlayPassTutorial()
	{
		Vector3 position = m_duosPortal.transform.position;
		PlayDuosTutorialPopup(DUOS_TUTORIALS.PASS, position, Notification.PopUpArrowDirection.Down, "GAMEPLAY_BACON_DUO_PASS_TUTORIAL");
		Options.Get().SetBool(Option.BG_DUOS_TUTORIAL_PLAYED_PASS, val: true);
		float timer = 0f;
		while (timer < 8f && GameState.Get().GetFriendlySidePlayer().GetNumAvailableResources() != 0)
		{
			timer += Time.deltaTime;
			yield return null;
		}
		StopDuosTutorialPopup(DUOS_TUTORIALS.PASS);
	}

	private IEnumerator PlayReceiveTutorial(Card passedCard)
	{
		yield return new WaitForSeconds(1f);
		if (passedCard == null)
		{
			yield break;
		}
		PlayDuosTutorialPopup(DUOS_TUTORIALS.RECEIVE, Vector3.zero, Notification.PopUpArrowDirection.Down, "GAMEPLAY_BACON_DUO_RECEIVE_TUTORIAL");
		Options.Get().SetBool(Option.BG_DUOS_TUTORIAL_PLAYED_RECEIVE, val: true);
		float timer = 0f;
		while (timer < 5f && passedCard != null)
		{
			Vector3 popupPosition = passedCard.gameObject.transform.position + m_positionOffsets[6];
			if (m_duosTutorialPopup[6] != null)
			{
				m_duosTutorialPopup[6].SetPosition(popupPosition, convertLegacyPosition: true);
			}
			timer += Time.deltaTime;
			yield return null;
		}
		StopDuosTutorialPopup(DUOS_TUTORIALS.RECEIVE);
	}

	private IEnumerator PlayCombatOrderTutorial()
	{
		int playerTeamId = GameState.Get().GetFriendlySidePlayer().GetTag(GAME_TAG.BACON_DUO_TEAM_ID);
		PlayerLeaderboardTeam team = PlayerLeaderboardManager.Get().GetTeamForTeamId(playerTeamId);
		if (!(team == null))
		{
			while (!TurnTimer.Get().IsRopeActive())
			{
				yield return null;
			}
			Vector3 position = team.transform.position;
			PlayDuosTutorialPopup(DUOS_TUTORIALS.COMBAT_ORDER, position, Notification.PopUpArrowDirection.Left, "GAMEPLAY_BACON_DUO_COMBAT_ORDER_TUTORIAL");
			Options.Get().SetBool(Option.BG_DUOS_TUTORIAL_PLAYED_COMBAT_ORDER, val: true);
		}
	}

	private IEnumerator PlayPingTutorial()
	{
		yield return new WaitForSeconds(1f);
		Card leftMost = ZoneMgr.Get().FindZoneOfType<ZonePlay>(Player.Side.OPPOSING).GetCardAtSlot(1);
		if (!(leftMost == null))
		{
			Vector3 position = leftMost.transform.position;
			PlayDuosTutorialPopup(DUOS_TUTORIALS.PING, position, Notification.PopUpArrowDirection.Down, "GAMEPLAY_BACON_DUO_PING_TUTORIAL", "GAMEPLAY_BACON_DUO_PING_TUTORIAL_PHONE");
			Options.Get().SetBool(Option.BG_DUOS_TUTORIAL_PLAYED_PING, val: true);
			yield return new WaitForSeconds(8f);
			StopDuosTutorialPopup(DUOS_TUTORIALS.PING);
		}
	}

	private IEnumerator PlayPingWheelTutorial(GameObject pingObject)
	{
		if (!(pingObject == null))
		{
			Vector3 popupPosition = pingObject.transform.position;
			PlayDuosTutorialPopup(DUOS_TUTORIALS.PING_WHEEL, popupPosition, Notification.PopUpArrowDirection.Down, "GAMEPLAY_BACON_DUO_PING_WHEEL_TUTORIAL", "GAMEPLAY_BACON_DUO_PING_WHEEL_TUTORIAL_PHONE");
			Options.Get().SetBool(Option.BG_DUOS_TUTORIAL_PLAYED_PING_WHEEL, val: true);
			yield return new WaitForSeconds(8f);
			StopDuosTutorialPopup(DUOS_TUTORIALS.PING_WHEEL);
		}
	}

	private IEnumerator PlayTeammatePingedTutorial()
	{
		Vector3 abovePortal = m_duosPortal.transform.position;
		PlayDuosTutorialPopup(DUOS_TUTORIALS.TEAMMATE_PINGED, abovePortal, Notification.PopUpArrowDirection.Down, "GAMEPLAY_BACON_DUO_TEAMMATE_PINGED_TUTORIAL");
		Options.Get().SetBool(Option.BG_DUOS_TUTORIAL_PLAYED_TEAMMATE_PINGED, val: true);
		yield return new WaitForSeconds(8f);
		StopDuosTutorialPopup(DUOS_TUTORIALS.TEAMMATE_PINGED);
	}

	private IEnumerator PlayBonusDamageTutorial(Player.Side winningSide)
	{
		if (GameState.Get().GetPlayerBySide(winningSide).GetTag(GAME_TAG.BACON_TEAMMATE_BONUS_MINION_DAMAGE_LAST_COMBAT) != 0)
		{
			Card rightMost = ZoneMgr.Get().FindZoneOfType<ZonePlay>(winningSide).GetLastCard();
			if (!(rightMost == null))
			{
				Vector3 position = rightMost.transform.position;
				PlayDuosTutorialPopup(DUOS_TUTORIALS.BONUS_DAMAGE, position, Notification.PopUpArrowDirection.Down, "GAMEPLAY_BACON_DUO_BONUS_DAMAGE_TUTORIAL");
				GameState.Get().SetBusy(busy: true);
				Options.Get().SetBool(Option.BG_DUOS_TUTORIAL_PLAYED_BONUS_DAMAGE, val: true);
				yield return new WaitForSeconds(5f);
				GameState.Get().SetBusy(busy: false);
				StopDuosTutorialPopup(DUOS_TUTORIALS.BONUS_DAMAGE);
			}
		}
	}
}
