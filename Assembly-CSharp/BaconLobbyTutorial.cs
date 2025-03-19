using System.Collections;
using UnityEngine;

public class BaconLobbyTutorial : MonoBehaviour
{
	public GameObject m_togglePopupBone;

	public GameObject m_togglePopupPhoneBone;

	public GameObject m_queuePopupBone;

	public GameObject m_queuePopupPhoneBone;

	public GameObject m_arrangePartyPopupBone;

	public GameObject m_arrangePartyPopupPhoneBone;

	public Vector3 m_inviteFriendPopupOffset;

	public Vector3 m_inviteFriendPopupPhoneOffset;

	public float m_inviteFriendPopupScale;

	private Notification m_toggleNotification;

	private Notification m_queueNotification;

	private Notification m_friendListNotification;

	private Notification m_arrangePartyNotification;

	private Coroutine m_toggleModeCoroutine;

	private Coroutine m_queueCoroutine;

	private Coroutine m_arrangePartyCoroutine;

	private void OnDestroy()
	{
		HideAllPopups();
	}

	public void StartModeToggleTutorial()
	{
		if (m_toggleModeCoroutine == null && !Options.Get().GetBool(Option.BG_DUOS_LOBBY_TUTORIAL_PLAYED_TOGGLE) && (!PartyManager.Get().IsInParty() || PartyManager.Get().IsPartyLeader()))
		{
			m_toggleModeCoroutine = StartCoroutine("PlayToggleModeTutorial");
		}
	}

	public void StartQueueTutorial()
	{
		if (m_queueCoroutine == null && !Options.Get().GetBool(Option.BG_DUOS_LOBBY_TUTORIAL_PLAYED_QUEUE) && !PartyManager.Get().IsInParty())
		{
			m_queueCoroutine = StartCoroutine("PlayQueueTutorial");
		}
	}

	public void StartArrangePartyTutorial()
	{
		if (PartyManager.Get().GetCurrentPartySize() >= 3 && PartyManager.Get().IsPartyLeader() && BaconLobbyMgr.Get().IsInDuosMode() && m_arrangePartyCoroutine == null && !Options.Get().GetBool(Option.BG_DUOS_LOBBY_TUTORIAL_PLAYED_ARRANGE_PARTY))
		{
			m_arrangePartyCoroutine = StartCoroutine("PlayArrangePartyTutorial");
		}
	}

	public void HideAllPopups()
	{
		NotificationManager.Get()?.DestroyNotification(m_toggleNotification, 0f);
		NotificationManager.Get()?.DestroyNotification(m_queueNotification, 0f);
		NotificationManager.Get()?.DestroyNotification(m_friendListNotification, 0f);
		StopCoroutine("PlayToggleModeTutorial");
		StopCoroutine("PlayQueueTutorial");
		HideArrangePartyPopup();
	}

	public void HideArrangePartyPopup()
	{
		NotificationManager.Get()?.DestroyNotification(m_arrangePartyNotification, 0f);
		StopCoroutine("PlayArrangePartyTutorial");
	}

	private Notification PlayTutorialPopup(Vector3 position, Vector3 scale, Notification.PopUpArrowDirection direction, string text, string phoneText = null)
	{
		if (phoneText != null && (bool)UniversalInputManager.UsePhoneUI)
		{
			text = phoneText;
		}
		Notification notification = NotificationManager.Get().CreatePopupText(UserAttentionBlocker.NONE, position, scale, GameStrings.Get(text));
		notification.ShowPopUpArrow(direction);
		notification.PulseReminderEveryXSeconds(3f);
		return notification;
	}

	private IEnumerator PlayToggleModeTutorial()
	{
		yield return new WaitForSeconds(3f);
		GameObject bone = (UniversalInputManager.UsePhoneUI ? m_togglePopupPhoneBone : m_togglePopupBone);
		m_toggleNotification = PlayTutorialPopup(bone.transform.position, bone.transform.localScale, Notification.PopUpArrowDirection.Up, "GLUE_BACON_LOBBY_TOGGLE_TUTORIAL");
		Options.Get().SetBool(Option.BG_DUOS_LOBBY_TUTORIAL_PLAYED_TOGGLE, val: true);
		yield return new WaitForSeconds(5f);
		NotificationManager.Get()?.DestroyNotification(m_toggleNotification, 0f);
	}

	private IEnumerator PlayQueueTutorial()
	{
		yield return new WaitForSeconds(1f);
		GameObject bone = (UniversalInputManager.UsePhoneUI ? m_queuePopupPhoneBone : m_queuePopupBone);
		m_queueNotification = PlayTutorialPopup(bone.transform.position, bone.transform.localScale, Notification.PopUpArrowDirection.Right, "GLUE_BACON_LOBBY_QUEUE_DUOS_TUTORIAL");
		Options.Get().SetBool(Option.BG_DUOS_LOBBY_TUTORIAL_PLAYED_QUEUE, val: true);
		yield return new WaitForSeconds(2f);
		float scale = m_inviteFriendPopupScale;
		Vector3 offset = (UniversalInputManager.UsePhoneUI ? m_inviteFriendPopupPhoneOffset : m_inviteFriendPopupOffset);
		Notification.PopUpArrowDirection arrowDir = (UniversalInputManager.UsePhoneUI ? Notification.PopUpArrowDirection.LeftUp : Notification.PopUpArrowDirection.LeftDown);
		m_friendListNotification = PlayTutorialPopup(new Vector3(0f, 0f, 0f), new Vector3(scale, scale, scale), arrowDir, "GLUE_BACON_LOBBY_INVITE_TEAM_TUTORIAL");
		m_friendListNotification.SetPosition(BnetBar.Get().m_friendButton.transform.position + offset);
		yield return new WaitForSeconds(5f);
		NotificationManager.Get()?.DestroyNotification(m_queueNotification, 0f);
		NotificationManager.Get()?.DestroyNotification(m_friendListNotification, 0f);
	}

	private IEnumerator PlayArrangePartyTutorial()
	{
		yield return new WaitForSeconds(1f);
		GameObject bone = (UniversalInputManager.UsePhoneUI ? m_arrangePartyPopupPhoneBone : m_arrangePartyPopupBone);
		m_arrangePartyNotification = PlayTutorialPopup(bone.transform.position, bone.transform.localScale, Notification.PopUpArrowDirection.Right, "GLUE_BACON_LOBBY_DRAG_TUTORIAL");
		Options.Get().SetBool(Option.BG_DUOS_LOBBY_TUTORIAL_PLAYED_ARRANGE_PARTY, val: true);
		yield return new WaitForSeconds(5f);
		NotificationManager.Get()?.DestroyNotification(m_arrangePartyNotification, 0f);
	}
}
