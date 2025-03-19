using UnityEngine;

[CustomEditClass]
public class RibbonButton : PegUIElement
{
	public GameObject m_highlight;

	[CustomEditField(T = EditType.SOUND_PREFAB)]
	public string m_onReleasedSound;

	[CustomEditField(T = EditType.SOUND_PREFAB)]
	public string m_onHoverSound;

	public void Start()
	{
		AddEventListener(UIEventType.RELEASE, OnButtonReleased);
		AddEventListener(UIEventType.ROLLOVER, OnButtonOver);
		AddEventListener(UIEventType.ROLLOUT, OnButtonOut);
	}

	public void OnButtonOver(UIEvent e)
	{
		if (m_highlight != null)
		{
			m_highlight.SetActive(value: true);
		}
		if (!string.IsNullOrEmpty(m_onHoverSound))
		{
			SoundManager.Get().LoadAndPlay(m_onHoverSound);
		}
	}

	public void OnButtonOut(UIEvent e)
	{
		if (m_highlight != null)
		{
			m_highlight.SetActive(value: false);
		}
	}

	public void OnButtonReleased(UIEvent e)
	{
		if (!string.IsNullOrEmpty(m_onReleasedSound))
		{
			SoundManager.Get().LoadAndPlay(m_onReleasedSound);
		}
	}
}
