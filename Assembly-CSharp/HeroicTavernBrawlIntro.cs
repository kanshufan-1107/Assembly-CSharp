using Hearthstone.UI;
using UnityEngine;

public class HeroicTavernBrawlIntro : MonoBehaviour
{
	[SerializeField]
	private GameObject m_apprenticeModeObject;

	[SerializeField]
	private GameObject m_nonApprenticeModeObject;

	[SerializeField]
	private Widget m_widget;

	private void Awake()
	{
		if (m_widget != null)
		{
			m_widget.RegisterReadyListener(delegate
			{
				SetApprenticeMode();
			});
		}
	}

	private void SetApprenticeMode()
	{
		if (!(m_apprenticeModeObject == null) && !(m_nonApprenticeModeObject == null))
		{
			if (!GameUtils.HasCompletedApprentice())
			{
				m_apprenticeModeObject.SetActive(value: true);
				m_nonApprenticeModeObject.SetActive(value: false);
			}
			else
			{
				m_apprenticeModeObject.SetActive(value: false);
				m_nonApprenticeModeObject.SetActive(value: true);
			}
		}
	}
}
