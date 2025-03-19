using Hearthstone.DataModels;
using Hearthstone.UI;
using UnityEngine;

public class LoanerDeckTimerDisplay : MonoBehaviour
{
	private FreeDeckMgr m_freeDeckManager;

	private LoanerDecksInfoDataModel m_loanerDeckInfoDataModel;

	private Widget m_widget;

	private LoanerDeckDisplay m_loanerDeckDisplay;

	private void Start()
	{
		m_freeDeckManager = FreeDeckMgr.Get();
		if (m_freeDeckManager.Status != FreeDeckMgr.FreeDeckStatus.TRIAL_PERIOD)
		{
			base.gameObject.SetActive(value: false);
			return;
		}
		m_widget = GetComponent<Widget>();
		m_loanerDeckDisplay = LoanerDeckDisplay.Get();
		if (m_widget != null && m_loanerDeckDisplay != null)
		{
			m_loanerDeckInfoDataModel = m_loanerDeckDisplay.LoanerDeckInfoDataModel;
			if (m_loanerDeckInfoDataModel != null)
			{
				m_widget.BindDataModel(m_loanerDeckInfoDataModel);
			}
			m_widget.RegisterEventListener(m_loanerDeckDisplay.OpenDeckDetailsWidget);
			m_widget.RegisterEventListener(m_loanerDeckDisplay.HideDeckDetailsWidget);
			m_widget.RegisterEventListener(m_loanerDeckDisplay.ConfirmDeckSelection);
		}
	}

	private void OnDestroy()
	{
		if (m_widget != null)
		{
			m_widget.UnbindDataModel(478);
		}
	}
}
