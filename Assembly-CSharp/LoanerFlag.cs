using Hearthstone.DataModels;
using Hearthstone.UI;
using UnityEngine;

public class LoanerFlag : MonoBehaviour
{
	private LoanerDecksInfoDataModel m_loanerDeckInfoDataModel;

	private TwistSeasonInfoDataModel m_TwistSeasonInfoDataModel;

	private Widget m_widget;

	private void Start()
	{
		m_widget = GetComponent<Widget>();
		LoanerDeckDisplay loanerDeckDisplay = LoanerDeckDisplay.Get();
		if (loanerDeckDisplay != null)
		{
			m_loanerDeckInfoDataModel = loanerDeckDisplay.LoanerDeckInfoDataModel;
			m_TwistSeasonInfoDataModel = TwistDetailsDisplayManager.TwistSeasonInfoModel;
			if (m_loanerDeckInfoDataModel != null && m_TwistSeasonInfoDataModel != null && m_widget != null)
			{
				m_widget.BindDataModel(m_loanerDeckInfoDataModel);
				m_widget.BindDataModel(m_TwistSeasonInfoDataModel);
			}
		}
	}

	public void Hide()
	{
		base.gameObject.SetActive(value: false);
	}
}
