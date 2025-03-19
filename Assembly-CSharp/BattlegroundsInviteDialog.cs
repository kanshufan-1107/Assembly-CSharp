using Hearthstone.UI;
using UnityEngine;

[RequireComponent(typeof(Widget))]
public class BattlegroundsInviteDialog : DialogBase
{
	[SerializeField]
	private UberText m_challengerName;

	[SerializeField]
	private UberText m_inviteNote;

	[SerializeField]
	private UIBButton m_acceptButton;

	[SerializeField]
	private UIBButton m_denyButton;

	private FriendlyChallengeDialog.ResponseCallback m_responseCallback;

	protected override void Awake()
	{
		base.Awake();
		m_acceptButton.AddEventListener(UIEventType.RELEASE, ConfirmButtonPress);
		m_denyButton.AddEventListener(UIEventType.RELEASE, CancelButtonPress);
	}

	public void SetInfo(FriendlyChallengeDialog.Info info)
	{
		m_challengerName.Text = FriendUtils.GetUniqueName(info.m_challenger);
		bool isNearbyStranger = BnetNearbyPlayerMgr.Get().IsNearbyStranger(info.m_challenger);
		m_inviteNote.gameObject.SetActive(isNearbyStranger);
		m_responseCallback = info.m_callback;
	}

	private void ConfirmButtonPress(UIEvent e)
	{
		SoundManager.Get().LoadAndPlay("Small_Click.prefab:2a1c5335bf08dc84eb6e04fc58160681");
		if (m_responseCallback != null)
		{
			m_responseCallback(accept: true);
		}
		Hide();
	}

	private void CancelButtonPress(UIEvent e)
	{
		SoundManager.Get().LoadAndPlay("Small_Click.prefab:2a1c5335bf08dc84eb6e04fc58160681");
		if (m_responseCallback != null)
		{
			m_responseCallback(accept: false);
		}
		Hide();
	}

	public override void Show()
	{
		base.Show();
		BnetBar.Get().DisableButtonsByDialog(this);
		if ((bool)UniversalInputManager.UsePhoneUI && m_inviteNote.gameObject.activeSelf)
		{
			base.transform.localPosition = new Vector3(base.transform.localPosition.x, base.transform.localPosition.y + 50f, base.transform.localPosition.z);
		}
		DoShowAnimation();
		UniversalInputManager.Get().SetSystemDialogActive(active: true);
		SoundManager.Get().LoadAndPlay("friendly_challenge.prefab:649e070117bcd0d45bac691a03bf2dec");
		DialogBase.DoBlur();
	}

	public override void Hide()
	{
		base.Hide();
		SoundManager.Get().LoadAndPlay("banner_shrink.prefab:d9de7386a7f2017429d126e972232123");
		DialogBase.EndBlur();
	}
}
