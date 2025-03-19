using Blizzard.GameService.SDK.Client.Integration;
using Hearthstone.UI;
using UnityEngine;

[RequireComponent(typeof(Widget))]
public class BattlegroundsSuggestDialog : DialogBase
{
	public delegate void ResponseCallback(bool accept, BnetGameAccountId playerToInvite);

	public class Info : AlertPopup.PopupInfo
	{
		public BnetGameAccountId PlayerToInviteGameAccountId;

		public string PlayerToInviteName;

		public BnetGameAccountId SuggesterGameAccountId;

		public string SuggesterName;

		public ResponseCallback Callback;
	}

	public const string PrivatePartyInfo = "GLUE_BACON_PRIVATE_PARTY_INFO";

	public const string PartySuggestionIdFormat = "partysuggestion_{0}";

	[SerializeField]
	private UberText m_suggestionText;

	[SerializeField]
	private UberText m_playerToInviteName;

	[SerializeField]
	private UberText m_inviteNote;

	[SerializeField]
	private UIBButton m_acceptButton;

	[SerializeField]
	private UIBButton m_denyButton;

	private BnetGameAccountId m_playerToInvite;

	private BnetGameAccountId m_suggester;

	private ResponseCallback m_responseCallback;

	protected override void Awake()
	{
		base.Awake();
		m_acceptButton.AddEventListener(UIEventType.RELEASE, ConfirmButtonPress);
		m_denyButton.AddEventListener(UIEventType.RELEASE, CancelButtonPress);
	}

	public void SetInfo(Info info)
	{
		m_suggester = info.SuggesterGameAccountId;
		string suggesterBestName = info.SuggesterName;
		BnetPlayer suggesterBnetPlayer = BnetPresenceMgr.Get().GetPlayer(info.SuggesterGameAccountId);
		if (suggesterBnetPlayer != null)
		{
			suggesterBestName = FriendUtils.GetUniqueNameWithColor(suggesterBnetPlayer);
		}
		m_suggestionText.Text = GameStrings.Format("GLOBAL_FRIEND_CHALLENGE_BODY_BATTLEGROUNDS_SUGGESTION", suggesterBestName);
		m_playerToInvite = info.PlayerToInviteGameAccountId;
		m_playerToInviteName.Text = info.PlayerToInviteName;
		if (PartyManager.Get().GetCurrentPartySize() == PartyManager.Get().GetBattlegroundsMaxRankedPartySize())
		{
			m_inviteNote.Text = GameStrings.Format("GLUE_BACON_PRIVATE_PARTY_INFO");
			m_inviteNote.gameObject.SetActive(value: true);
		}
		else
		{
			m_inviteNote.gameObject.SetActive(value: false);
		}
		m_responseCallback = info.Callback;
	}

	private void ConfirmButtonPress(UIEvent e)
	{
		SoundManager.Get().LoadAndPlay("Small_Click.prefab:2a1c5335bf08dc84eb6e04fc58160681");
		if (m_responseCallback != null)
		{
			m_responseCallback(accept: true, m_playerToInvite);
		}
		DialogManager.Get().RemoveUniquePopupRequestFromQueue($"partysuggestion_{m_playerToInvite.Low}");
		Hide();
	}

	private void CancelButtonPress(UIEvent e)
	{
		SoundManager.Get().LoadAndPlay("Small_Click.prefab:2a1c5335bf08dc84eb6e04fc58160681");
		if (m_responseCallback != null)
		{
			m_responseCallback(accept: false, m_playerToInvite);
		}
		DialogManager.Get().RemoveUniquePopupRequestFromQueue($"partysuggestion_{m_playerToInvite.Low}");
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
