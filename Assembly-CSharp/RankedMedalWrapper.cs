using Hearthstone.DataModels;
using Hearthstone.UI;
using UnityEngine;

[RequireComponent(typeof(WidgetTemplate))]
public class RankedMedalWrapper : MonoBehaviour
{
	private Widget m_widget;

	public bool IsReady
	{
		get
		{
			if (m_widget == null || !m_widget.IsReady || m_widget.IsChangingStates)
			{
				return false;
			}
			if (m_widget.GetComponentInChildren<RankedMedal>(includeInactive: false) == null)
			{
				return false;
			}
			return true;
		}
	}

	private void Awake()
	{
		m_widget = GetComponent<WidgetTemplate>();
	}

	public void BindRankedPlayDataModel(RankedPlayDataModel dataModel)
	{
		if (dataModel != GetRankedPlayDataModel())
		{
			m_widget.BindDataModel(dataModel);
		}
	}

	public void Hide()
	{
		base.gameObject.SetActive(value: false);
	}

	public void Show(bool useLegacyRankedPlay)
	{
		base.gameObject.SetActive(value: true);
		if (useLegacyRankedPlay)
		{
			m_widget.TriggerEvent("SHOW_LEGACY");
		}
		else
		{
			m_widget.TriggerEvent("SHOW_NEW");
		}
	}

	private RankedPlayDataModel GetRankedPlayDataModel()
	{
		m_widget.GetDataModel(123, out var dataModel);
		return dataModel as RankedPlayDataModel;
	}
}
