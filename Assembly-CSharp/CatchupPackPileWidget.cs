using UnityEngine;

public class CatchupPackPileWidget : MonoBehaviour
{
	[SerializeField]
	private GameObject m_summaryCard;

	[SerializeField]
	private GameObject m_titleBanner;

	[SerializeField]
	private GameObject m_doneButton;

	private bool m_doneButtonPressed;

	public GameObject SummaryCard => m_summaryCard;

	public GameObject TitleBanner => m_titleBanner;

	public bool IsDoneButtonShowing()
	{
		if (m_doneButton != null)
		{
			return m_doneButton.activeInHierarchy;
		}
		return false;
	}

	private void Awake()
	{
		m_doneButtonPressed = false;
	}

	public void TriggerDoneButtonPressed()
	{
		if (!(m_doneButton == null) && !m_doneButtonPressed)
		{
			PegUIElement pegUI = m_doneButton.GetComponent<PegUIElement>();
			if (pegUI != null)
			{
				pegUI.TriggerPress();
				pegUI.TriggerRelease();
				m_doneButtonPressed = true;
			}
		}
	}
}
