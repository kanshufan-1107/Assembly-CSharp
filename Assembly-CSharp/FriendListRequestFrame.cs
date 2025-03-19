using UnityEngine;

public class FriendListRequestFrame : MonoBehaviour
{
	public GameObject m_Background;

	public FriendListUIElement m_AcceptButton;

	public FriendListUIElement m_DeclineButton;

	public UberText m_PlayerNameText;

	public UberText m_TimeText;

	private BnetInvitation m_invite;

	private static string ON_CLICK_SOUND_PREFAB = "Small_Click.prefab:2a1c5335bf08dc84eb6e04fc58160681";

	private static string ON_HOVER_SOUND_PREFAB = "Small_Mouseover.prefab:692610296028713458ea58bc34adb4c9";

	private void Awake()
	{
		m_AcceptButton.AddEventListener(UIEventType.RELEASE, OnAcceptButtonPressed);
		m_AcceptButton.AddEventListener(UIEventType.ROLLOVER, OnButtonHovered);
		m_DeclineButton.AddEventListener(UIEventType.RELEASE, OnDeclineButtonPressed);
		m_DeclineButton.AddEventListener(UIEventType.ROLLOVER, OnButtonHovered);
	}

	private void Update()
	{
		UpdateTimeText();
	}

	private void OnEnable()
	{
		UpdateInvite();
	}

	public BnetInvitation GetInvite()
	{
		return m_invite;
	}

	public void SetInvite(BnetInvitation invite)
	{
		if (!(m_invite == invite))
		{
			m_invite = invite;
			UpdateInvite();
		}
	}

	public void UpdateInvite()
	{
		if (base.gameObject.activeSelf && !(m_invite == null))
		{
			m_PlayerNameText.Text = m_invite.GetInviterName();
			UpdateTimeText();
		}
	}

	private void UpdateTimeText()
	{
		string elapsedTimeString = FriendUtils.GetRequestElapsedTimeString((long)m_invite.GetCreationTimeMicrosec());
		m_TimeText.Text = GameStrings.Format("GLOBAL_FRIENDLIST_REQUEST_SENT_TIME", elapsedTimeString);
	}

	private void OnAcceptButtonPressed(UIEvent e)
	{
		BnetFriendMgr.Get().AcceptInvite(m_invite);
		SoundManager.Get().LoadAndPlay(ON_CLICK_SOUND_PREFAB);
	}

	private void OnDeclineButtonPressed(UIEvent e)
	{
		BnetFriendMgr.Get().IgnoreInvite(m_invite.GetId());
		SoundManager.Get().LoadAndPlay(ON_CLICK_SOUND_PREFAB);
	}

	private void OnButtonHovered(UIEvent e)
	{
		SoundManager.Get().LoadAndPlay(ON_HOVER_SOUND_PREFAB);
	}
}
