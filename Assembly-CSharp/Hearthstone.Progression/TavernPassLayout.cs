using Assets;
using Hearthstone.UI;
using Hearthstone.UI.Core;
using UnityEngine;

namespace Hearthstone.Progression;

public class TavernPassLayout : MonoBehaviour
{
	[SerializeField]
	private Global.RewardTrackType m_rewardTrackType = Global.RewardTrackType.GLOBAL;

	private Widget m_widget;

	private int m_productId;

	[Overridable]
	public Global.RewardTrackType RewardTrackType
	{
		get
		{
			return m_rewardTrackType;
		}
		set
		{
			if (m_rewardTrackType != value)
			{
				m_rewardTrackType = value;
				if (!(m_widget == null) && m_widget.IsReady)
				{
					UpdateRewardTrackDataModel();
				}
			}
		}
	}

	[Overridable]
	public int ProductId
	{
		get
		{
			return m_productId;
		}
		set
		{
			if (m_productId != value)
			{
				m_productId = value;
				if (!(m_widget == null) && m_widget.IsReady)
				{
					UpdateRewardTrackDataModel();
				}
			}
		}
	}

	private void Awake()
	{
		m_widget = GetComponent<WidgetTemplate>();
		m_widget.RegisterReadyListener(delegate
		{
			OnWidgetReady();
		});
	}

	private void OnWidgetReady()
	{
		UpdateRewardTrackDataModel();
	}

	private void UpdateRewardTrackDataModel()
	{
		RewardTrack rewardTrack = RewardTrackManager.Get().GetRewardTrack(m_rewardTrackType);
		if (rewardTrack == null || !rewardTrack.IsValid)
		{
			Debug.LogError(string.Format("{0}: no reward track found of type {1}.", "TavernPassLayout", m_rewardTrackType));
			return;
		}
		RewardTrackDbfRecord trackRecord = rewardTrack.RewardTrackAsset;
		if (trackRecord != null)
		{
			m_widget.BindDataModel(rewardTrack.TrackDataModel);
			m_widget.BindDataModel(RewardTrackFactory.CreatePaidRewardListDataModel(trackRecord, m_productId));
		}
	}
}
