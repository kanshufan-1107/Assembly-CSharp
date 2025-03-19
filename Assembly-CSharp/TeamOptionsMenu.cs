using System.Collections;
using UnityEngine;

public class TeamOptionsMenu : MonoBehaviour
{
	public GameObject m_root;

	public PegUIElement m_renameButton;

	public PegUIElement m_deleteButton;

	public PegUIElement m_copyButton;

	public GameObject m_top;

	public GameObject m_bottom;

	public HighlightState m_highlight;

	public Transform m_showBone;

	public Transform m_hideBone;

	public Transform[] m_buttonPositions;

	public Transform[] m_bottomPositions;

	public float[] m_topScales;

	private int m_buttonCount;

	private bool m_shown;

	private LettuceTeam m_team;

	private CollectionTeamInfo m_teamInfo;

	public bool IsShown => m_shown;

	public void Awake()
	{
		m_root.SetActive(value: false);
		if (m_renameButton != null)
		{
			m_renameButton.AddEventListener(UIEventType.RELEASE, OnRenameButtonReleased);
		}
		if (m_deleteButton != null)
		{
			m_deleteButton.AddEventListener(UIEventType.RELEASE, OnDeleteButtonReleased);
		}
		if (m_copyButton != null)
		{
			m_copyButton.AddEventListener(UIEventType.RELEASE, OnCopyButtonReleased);
		}
	}

	public void Show()
	{
		if (!m_shown)
		{
			iTween.Stop(base.gameObject);
			m_root.SetActive(value: true);
			UpdateLayout();
			if (m_buttonCount == 0)
			{
				m_root.SetActive(value: false);
				return;
			}
			Hashtable tweenArgs = iTweenManager.Get().GetTweenHashTable();
			tweenArgs.Add("position", m_showBone.transform.position);
			tweenArgs.Add("time", 0.35f);
			tweenArgs.Add("easetype", iTween.EaseType.easeOutCubic);
			tweenArgs.Add("oncomplete", "FinishShow");
			tweenArgs.Add("oncompletetarget", base.gameObject);
			iTween.MoveTo(m_root, tweenArgs);
			m_shown = true;
		}
	}

	public void Hide(bool animate = true)
	{
		if (m_shown)
		{
			iTween.Stop(base.gameObject);
			if (!animate)
			{
				m_root.SetActive(value: false);
				return;
			}
			m_root.SetActive(value: true);
			Hashtable tweenArgs = iTweenManager.Get().GetTweenHashTable();
			tweenArgs.Add("position", m_hideBone.transform.position);
			tweenArgs.Add("time", 0.35f);
			tweenArgs.Add("easetype", iTween.EaseType.easeOutCubic);
			tweenArgs.Add("oncomplete", "FinishHide");
			tweenArgs.Add("oncompletetarget", base.gameObject);
			iTween.MoveTo(m_root, tweenArgs);
			m_shown = false;
		}
	}

	private void FinishHide()
	{
		if (!m_shown)
		{
			m_root.SetActive(value: false);
		}
	}

	public void SetTeam(LettuceTeam team)
	{
		m_team = team;
	}

	public void SetTeamInfo(CollectionTeamInfo teamInfo)
	{
		m_teamInfo = teamInfo;
	}

	private void OnRenameButtonReleased(UIEvent e)
	{
		m_teamInfo.Hide();
		CollectionDeckTray.Get().GetTeamsContent().RenameCurrentlyEditingTeam();
	}

	private void OnDeleteButtonReleased(UIEvent e)
	{
		AlertPopup.PopupInfo info = new AlertPopup.PopupInfo();
		info.m_headerText = GameStrings.Get("GLUE_COLLECTION_DELETE_CONFIRM_HEADER");
		info.m_showAlertIcon = false;
		info.m_text = GameStrings.Get("GLUE_COLLECTION_DELETE_CONFIRM_DESC");
		info.m_alertTextAlignment = UberText.AlignmentOptions.Center;
		info.m_responseDisplay = AlertPopup.ResponseDisplay.CONFIRM_CANCEL;
		info.m_responseCallback = OnDeleteButtonConfirmationResponse;
		m_teamInfo.Hide();
		DialogManager.Get().ShowPopup(info);
	}

	private void OnCopyButtonReleased(UIEvent e)
	{
		if (CollectionDeckTray.Get().IsShowingTeamContents())
		{
			LettuceTeam team = CollectionManager.Get().GetEditingTeam();
			if (team != null && UIStatus.Get() != null)
			{
				ClipboardUtils.CopyToClipboard(new ShareableMercenariesTeam(team).Serialize());
				UIStatus.Get().AddInfo(GameStrings.Get("GLUE_COLLECTION_DECK_COPIED_TOAST"));
			}
		}
	}

	private void OnDeleteButtonConfirmationResponse(AlertPopup.Response response, object userData)
	{
		if (response != AlertPopup.Response.CANCEL)
		{
			CollectionDeckTray.Get().GetTeamsContent().DeleteTeam(m_team.ID);
		}
	}

	private void UpdateLayout()
	{
		int buttonCount = GetButtonCount();
		if (buttonCount != m_buttonCount)
		{
			m_buttonCount = buttonCount;
			UpdateBackground();
		}
		UpdateButtons();
	}

	private void UpdateBackground()
	{
		if (m_buttonCount != 0)
		{
			float scale = m_topScales[m_buttonCount - 1];
			m_top.transform.transform.localScale = new Vector3(1f, 1f, scale);
			m_bottom.transform.transform.position = m_bottomPositions[m_buttonCount - 1].position;
		}
	}

	private void UpdateButtons()
	{
		int index = 0;
		bool showRename = ShowRenameButton();
		bool showDelete = ShowDeleteButton();
		bool showCopy = ShowCopyButton();
		m_renameButton.gameObject.SetActive(showRename);
		if (showRename)
		{
			m_renameButton.transform.position = m_buttonPositions[index].position;
			index++;
		}
		m_copyButton.gameObject.SetActive(showCopy);
		if (showCopy)
		{
			m_copyButton.transform.position = m_buttonPositions[index].position;
			index++;
		}
		m_deleteButton.gameObject.SetActive(showDelete);
		if (showDelete)
		{
			m_deleteButton.transform.position = m_buttonPositions[index].position;
			index++;
		}
	}

	private int GetButtonCount()
	{
		return 0 + (ShowCopyButton() ? 1 : 0) + (ShowRenameButton() ? 1 : 0) + (ShowDeleteButton() ? 1 : 0);
	}

	private bool ShowRenameButton()
	{
		LettuceTeam team = CollectionManager.Get().GetEditingTeam();
		if (team != null && team.Locked)
		{
			return false;
		}
		return UniversalInputManager.Get().IsTouchMode();
	}

	private bool ShowDeleteButton()
	{
		LettuceTeam team = CollectionManager.Get().GetEditingTeam();
		if (team != null && team.Locked)
		{
			return false;
		}
		return UniversalInputManager.Get().IsTouchMode();
	}

	private bool ShowCopyButton()
	{
		LettuceTeam team = CollectionManager.Get().GetEditingTeam();
		if (team != null && team.Locked)
		{
			return false;
		}
		return true;
	}
}
