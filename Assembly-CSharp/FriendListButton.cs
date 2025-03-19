using UnityEngine;

public class FriendListButton : FriendListUIElement
{
	public GameObject m_Background;

	public UberText m_Text;

	public GameObject m_ActiveGlow;

	public string GetText()
	{
		return m_Text.Text;
	}

	public void SetText(string text)
	{
		m_Text.Text = text;
		UpdateAll();
	}

	public void ShowActiveGlow(bool show)
	{
		if (!(m_ActiveGlow != null))
		{
			return;
		}
		HighlightState highlight = m_ActiveGlow.GetComponentInChildren<HighlightState>();
		if (highlight != null)
		{
			if (show)
			{
				highlight.ChangeState(ActorStateType.HIGHLIGHT_PRIMARY_ACTIVE);
			}
			else
			{
				highlight.ChangeState(ActorStateType.HIGHLIGHT_OFF);
			}
		}
	}

	private void UpdateAll()
	{
		UpdateHighlight();
	}
}
