using System.Collections;
using UnityEngine;

[CustomEditClass]
public abstract class ChooserSubButton : AdventureGenericButton
{
	protected const string s_EventFlash = "Flash";

	public GameObject m_NewModePopupBone;

	[CustomEditField(Sections = "Event Table")]
	public StateEventTable m_StateTable;

	public float m_NewModePopupAutomaticHideTime = 1f;

	protected bool m_Glow;

	private Notification m_NewModePopup;

	public void SetHighlight(bool enable)
	{
		UIBHighlightStateControl highlight = GetComponent<UIBHighlightStateControl>();
		if (highlight != null)
		{
			if (m_Glow)
			{
				highlight.Select(selected: true, primary: true);
			}
			else
			{
				highlight.Select(enable);
			}
		}
		UIBHighlight h = GetComponent<UIBHighlight>();
		if (h != null)
		{
			if (enable)
			{
				h.Select();
			}
			else
			{
				h.Reset();
			}
		}
	}

	public void SetNewGlow(bool enable)
	{
		m_Glow = enable;
		UIBHighlightStateControl highlight = GetComponent<UIBHighlightStateControl>();
		if (highlight != null)
		{
			highlight.Select(enable, primary: true);
		}
	}

	public void ShowNewModePopup(string message)
	{
		if (!(m_NewModePopupBone == null))
		{
			m_NewModePopup = NotificationManager.Get().CreatePopupText(UserAttentionBlocker.NONE, m_NewModePopupBone.transform.position, m_NewModePopupBone.transform.localScale, message);
			m_NewModePopup.ShowPopUpArrow(Notification.PopUpArrowDirection.Right);
		}
	}

	public void HideNewModePopupAfterDelay()
	{
		StartCoroutine(HideNewModePopupAfterDelayCoroutine());
	}

	public void Flash()
	{
		m_StateTable.TriggerState("Flash");
	}

	public bool IsReady()
	{
		UIBHighlightStateControl highlight = GetComponent<UIBHighlightStateControl>();
		if (highlight != null)
		{
			return highlight.IsReady();
		}
		return false;
	}

	protected override void OnDestroy()
	{
		if (m_NewModePopup != null)
		{
			m_NewModePopup.Shrink();
		}
		base.OnDestroy();
	}

	public void OnDisable()
	{
		if (m_NewModePopup != null)
		{
			m_NewModePopup.Shrink();
		}
	}

	private IEnumerator HideNewModePopupAfterDelayCoroutine()
	{
		float timer = m_NewModePopupAutomaticHideTime;
		while (timer > 0f)
		{
			timer -= Time.deltaTime;
			yield return new WaitForEndOfFrame();
		}
		if (m_NewModePopup != null)
		{
			m_NewModePopup.Shrink();
		}
	}
}
