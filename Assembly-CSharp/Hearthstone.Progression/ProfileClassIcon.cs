using Blizzard.T5.MaterialService.Extensions;
using Hearthstone.DataModels;
using Hearthstone.UI;
using Hearthstone.UI.Core;
using UnityEngine;

namespace Hearthstone.Progression;

public class ProfileClassIcon : MonoBehaviour
{
	public AsyncReference m_XPBarLevelsReference;

	public AsyncReference m_XPBarWinsReference;

	private Widget m_widget;

	private ProgressBar m_progressBar;

	[Overridable]
	public bool IsGolden
	{
		set
		{
			SetPremium(value);
		}
	}

	private void Awake()
	{
		m_widget = GetComponent<WidgetTemplate>();
		m_XPBarLevelsReference.RegisterReadyListener<ProgressBar>(OnXPBarReady);
		m_XPBarWinsReference.RegisterReadyListener<ProgressBar>(OnXPBarReady);
	}

	private void OnXPBarReady(ProgressBar progressBar)
	{
		m_progressBar = progressBar.GetComponent<ProgressBar>();
		if (m_progressBar == null)
		{
			return;
		}
		ProfileClassIconDataModel classIconDataModel = m_widget.GetDataModel<ProfileClassIconDataModel>();
		if (classIconDataModel == null)
		{
			return;
		}
		if (classIconDataModel.IsMaxLevel)
		{
			if (classIconDataModel.IsPremium)
			{
				m_progressBar.SetProgressBar(1f);
			}
			else if (classIconDataModel.IsGolden)
			{
				m_progressBar.SetProgressBar((float)classIconDataModel.Wins / (float)classIconDataModel.PremiumWinsReq);
			}
			else
			{
				m_progressBar.SetProgressBar((float)classIconDataModel.Wins / (float)classIconDataModel.GoldWinsReq);
			}
			m_progressBar.SetLabel(classIconDataModel.WinsText);
		}
		else
		{
			m_progressBar.SetProgressBar((float)classIconDataModel.CurrentLevelXP / (float)classIconDataModel.CurrentLevelXPMax);
		}
	}

	private void SetPremium(bool isPremium)
	{
		if (!isPremium)
		{
			GetComponentInChildren<Renderer>().GetMaterial().SetTexture("_MaskTex", null);
		}
	}
}
