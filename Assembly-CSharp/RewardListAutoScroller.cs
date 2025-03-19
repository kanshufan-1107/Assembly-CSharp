using System.Collections;
using Hearthstone.UI;
using UnityEngine;

public class RewardListAutoScroller : MonoBehaviour
{
	public UIBScrollable m_scrollable;

	public GameObject[] m_sections;

	public float m_positionOffset;

	private Widget m_listWidget;

	private int m_sectionIndex;

	private bool IsReady
	{
		get
		{
			if (m_listWidget != null && m_listWidget.IsReady)
			{
				return !m_listWidget.IsChangingStates;
			}
			return false;
		}
	}

	public void Init(Widget listWidget, int sectionIndex)
	{
		m_listWidget = listWidget;
		m_sectionIndex = sectionIndex;
	}

	private void OnPlayMakerPopupIntroFinished()
	{
		StartCoroutine(ScrollToSectionWhenReady());
	}

	private IEnumerator ScrollToSectionWhenReady()
	{
		while (!IsReady)
		{
			yield return null;
		}
		if (m_sectionIndex >= 0 && m_sectionIndex < m_sections.Length)
		{
			yield return new WaitForSeconds(0.1f);
			m_scrollable.CenterObjectInView(m_sections[m_sectionIndex], m_positionOffset, null, iTween.EaseType.linear, 0.5f);
		}
	}
}
