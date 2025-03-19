using Hearthstone.DataModels;
using Hearthstone.UI;
using UnityEngine;

public class BaconRewardsVideoDetailView : MonoBehaviour
{
	private BaconBoardCollectionDetails m_boardDetailsDisplay;

	private BaconBoardCollectionDetails m_boardDetailsDisplayRendered;

	private BaconFinisherCollectionDetails m_finisherDetailsDisplay;

	private BaconFinisherCollectionDetails m_finisherDetailsDisplayRendered;

	public VisualController MainVisualController;

	public Widget m_BoardSkinsWidget;

	public AsyncReference m_boardDisplayReference;

	private Widget m_BoardSkinsWidgetInstance;

	public Widget m_boardSkinsRenderedWidget;

	public AsyncReference m_boardDisplayRenderedReference;

	private Widget m_boardSkinsRenderedWidgetInstance;

	public Widget m_FinishersWidget;

	public AsyncReference m_finisherDisplayReference;

	private Widget m_FinisherWidgetInstance;

	public Widget m_finishersRenderedWidget;

	public AsyncReference m_finishersDisplayRenderedReference;

	private Widget m_finisherRenderedWidgetInstance;

	private RewardItemDataModel m_CurrentRewardItem;

	private WidgetTemplate m_widget;

	private void Start()
	{
		m_widget = GetComponent<WidgetTemplate>();
		if (m_widget == null)
		{
			Log.Gameplay.PrintError("Video Details View isn't a widget");
			return;
		}
		if (MainVisualController == null)
		{
			Log.Gameplay.PrintError("Main visual controller is null.");
			return;
		}
		if (m_boardDisplayReference == null)
		{
			Log.Gameplay.PrintError("Board display reference is null.");
			return;
		}
		if (m_boardDisplayRenderedReference == null)
		{
			Log.Gameplay.PrintError("Board display rendered reference is null.");
			return;
		}
		if (m_finisherDisplayReference == null)
		{
			Log.Gameplay.PrintError("Finisher display reference is null.");
			return;
		}
		if (m_finishersDisplayRenderedReference == null)
		{
			Log.Gameplay.PrintError("Finisher display rendered reference is null.");
			return;
		}
		m_boardDisplayReference.RegisterReadyListener<Widget>(OnBoardDisplayReady);
		m_boardDisplayRenderedReference.RegisterReadyListener<Widget>(OnBoardDisplayRenderedReady);
		m_finisherDisplayReference.RegisterReadyListener<Widget>(OnFinisherDisplayReady);
		m_finishersDisplayRenderedReference.RegisterReadyListener<Widget>(OnFinisherDisplayRenderedReady);
		MainVisualController.RegisterDoneChangingStatesListener(ShowReward);
	}

	private void OnBoardDisplayReady(Widget widget)
	{
		m_BoardSkinsWidgetInstance = widget;
		m_boardDetailsDisplay = m_BoardSkinsWidgetInstance.GetComponentInChildren<BaconBoardCollectionDetails>();
	}

	private void OnBoardDisplayRenderedReady(Widget widget)
	{
		m_boardSkinsRenderedWidgetInstance = widget;
		m_boardDetailsDisplayRendered = m_boardSkinsRenderedWidgetInstance.GetComponentInChildren<BaconBoardCollectionDetails>();
	}

	private void OnFinisherDisplayReady(Widget widget)
	{
		m_FinisherWidgetInstance = widget;
		m_finisherDetailsDisplay = m_FinisherWidgetInstance.GetComponentInChildren<BaconFinisherCollectionDetails>();
	}

	private void OnFinisherDisplayRenderedReady(Widget widget)
	{
		m_finisherRenderedWidgetInstance = widget;
		m_finisherDetailsDisplayRendered = m_finisherRenderedWidgetInstance.GetComponentInChildren<BaconFinisherCollectionDetails>();
	}

	private void ShowReward(object o = null)
	{
		m_CurrentRewardItem = m_widget.GetDataModel<RewardItemDataModel>();
		if (m_CurrentRewardItem != null)
		{
			switch (m_CurrentRewardItem.ItemType)
			{
			case RewardItemType.BATTLEGROUNDS_BOARD_SKIN:
				ShowBoardSkinReward();
				break;
			case RewardItemType.BATTLEGROUNDS_FINISHER:
				ShowFinisherReward();
				break;
			}
		}
	}

	private void ShowBoardSkinReward()
	{
		if (m_boardDetailsDisplayRendered != null && UseCosmeticsRendering() && m_CurrentRewardItem.BGBoardSkin != null && !string.IsNullOrEmpty(m_CurrentRewardItem.BGBoardSkin?.DetailsRenderConfig))
		{
			m_boardDetailsDisplayRendered.gameObject.SetActive(value: true);
			m_boardDetailsDisplayRendered.AssignDataModels(m_CurrentRewardItem.BGBoardSkin, null);
			m_boardDetailsDisplayRendered.Show();
			return;
		}
		if (m_CurrentRewardItem.BGBoardSkin != null && m_boardDetailsDisplay != null)
		{
			m_boardDetailsDisplay.ClearVideo();
			m_boardDetailsDisplay.gameObject.SetActive(value: true);
			m_boardDetailsDisplay.AssignDataModels(m_CurrentRewardItem.BGBoardSkin, null);
			m_boardDetailsDisplay.Show();
		}
		if (NetCache.Get() == null || NetCache.Get().GetNetObject<NetCache.NetCacheFeatures>() == null || NetCache.Get().GetNetObject<NetCache.NetCacheFeatures>().Collection == null || string.IsNullOrEmpty(m_CurrentRewardItem.BGBoardSkin?.DetailsRenderConfig))
		{
			TelemetryManager.Client()?.SendCosmeticsRenderingFallback($"BaconRewardsVideoDetailView::Boardskin[{m_CurrentRewardItem.BGBoardSkin.BoardDbiId}]");
		}
	}

	private void ShowFinisherReward()
	{
		if (m_finisherDetailsDisplayRendered != null && UseCosmeticsRendering() && m_CurrentRewardItem.BGFinisher != null && !string.IsNullOrEmpty(m_CurrentRewardItem.BGFinisher?.DetailsRenderConfig))
		{
			m_finisherDetailsDisplayRendered.gameObject.SetActive(value: true);
			m_finisherDetailsDisplayRendered.AssignDataModels(m_CurrentRewardItem.BGFinisher, null);
			m_finisherDetailsDisplayRendered.Show();
			return;
		}
		if (m_CurrentRewardItem.BGFinisher != null && m_finisherDetailsDisplay != null)
		{
			m_finisherDetailsDisplay.ClearVideo();
			m_finisherDetailsDisplay.gameObject.SetActive(value: true);
			m_finisherDetailsDisplay.AssignDataModels(m_CurrentRewardItem.BGFinisher, null);
			m_finisherDetailsDisplay.Show();
		}
		if (NetCache.Get() == null || NetCache.Get().GetNetObject<NetCache.NetCacheFeatures>() == null || NetCache.Get().GetNetObject<NetCache.NetCacheFeatures>().Collection == null || string.IsNullOrEmpty(m_CurrentRewardItem.BGFinisher?.DetailsRenderConfig))
		{
			TelemetryManager.Client()?.SendCosmeticsRenderingFallback($"BaconRewardsVideoDetailView::Finisher[{m_CurrentRewardItem.BGFinisher.FinisherDbiId}]");
		}
	}

	private bool UseCosmeticsRendering()
	{
		return NetCache.Get()?.GetNetObject<NetCache.NetCacheFeatures>()?.Collection?.CosmeticsRenderingEnabled == true;
	}
}
