using UnityEngine;

public class PlayerLeaderboardIcon : MonoBehaviour
{
	public GameObject m_icon;

	public UberText m_text;

	private const string SHOW_PLAYMAKER_STATE = "Show";

	public void SetText(string text)
	{
		if (text == "")
		{
			ClearText();
		}
		else
		{
			m_text.gameObject.SetActive(value: true);
		}
		m_text.Text = text;
	}

	public void ClearText()
	{
		m_text.Text = "";
		m_text.gameObject.SetActive(value: false);
	}

	public void SetPlaymakerValue(string name, int value)
	{
		PlayMakerFSM playmaker = base.gameObject.GetComponent<PlayMakerFSM>();
		if (playmaker != null && playmaker.FsmVariables.GetFsmInt(name) != null)
		{
			playmaker.FsmVariables.GetFsmInt(name).Value = value;
			playmaker.SendEvent("Action");
		}
	}

	public void PlaymakerShow()
	{
		PlayMakerFSM playmaker = base.gameObject.GetComponent<PlayMakerFSM>();
		if (playmaker != null)
		{
			playmaker.SetState("Show");
		}
		else
		{
			base.gameObject.SetActive(value: false);
		}
	}

	public bool PlaymakerIsShowing()
	{
		PlayMakerFSM playmaker = base.gameObject.GetComponent<PlayMakerFSM>();
		if (playmaker != null)
		{
			return playmaker.ActiveStateName == "Show";
		}
		return false;
	}
}
