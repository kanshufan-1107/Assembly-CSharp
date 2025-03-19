using System.Collections.Generic;
using UnityEngine;

public class CardRuneBanner : MonoBehaviour
{
	public GameObject m_visualContainer;

	public List<RuneSlotVisual> m_runeSlotVisuals;

	public GameObject m_highlight;

	public GameObject m_runeBannerBackground;

	private RunePattern m_lastShownRunePattern;

	public void Show(RunePattern runePattern)
	{
		Show(runePattern, RuneState.Uninit);
	}

	public void Show(RunePattern runePattern, RuneState runeState)
	{
		foreach (RuneSlotVisual runeSlotVisual in m_runeSlotVisuals)
		{
			runeSlotVisual.Hide();
		}
		switch (runePattern.CombinedValue)
		{
		default:
			return;
		case 1:
			m_runeSlotVisuals[0].Show(runePattern, runeState);
			break;
		case 2:
			m_runeSlotVisuals[1].Show(runePattern, runeState);
			break;
		case 3:
			m_runeSlotVisuals[2].Show(runePattern, runeState);
			break;
		}
		if (m_visualContainer != null)
		{
			m_visualContainer.gameObject.SetActive(value: true);
		}
		if (m_runeBannerBackground != null && runePattern.CombinedValue > 0)
		{
			m_runeBannerBackground.SetActive(value: true);
		}
		m_lastShownRunePattern = runePattern;
	}

	public void SetState(RuneState state)
	{
		foreach (RuneSlotVisual runeSlotVisual in m_runeSlotVisuals)
		{
			runeSlotVisual.SetState(state);
		}
	}

	public void Hide()
	{
		if (m_runeBannerBackground != null)
		{
			m_runeBannerBackground.SetActive(value: false);
		}
		if (m_visualContainer != null)
		{
			m_visualContainer.gameObject.SetActive(value: false);
		}
	}

	public void ShowLastShownRuneBanner(RuneState runeState)
	{
		Show(m_lastShownRunePattern, runeState);
	}

	public void ShowLastShownRuneBanner()
	{
		Show(m_lastShownRunePattern, RuneState.Default);
	}

	public void SetHighlighted(bool highlighted)
	{
		m_highlight.SetActive(highlighted);
	}

	public RunePattern GetCurrentRunePattern()
	{
		return m_lastShownRunePattern;
	}
}
