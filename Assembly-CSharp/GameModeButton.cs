using Hearthstone.Core.Streaming;
using Hearthstone.DataModels;
using Hearthstone.Streaming;
using Hearthstone.UI;
using UnityEngine;

[RequireComponent(typeof(WidgetTemplate))]
public class GameModeButton : MonoBehaviour
{
	[SerializeField]
	private AsyncReference m_downloadProgressReference;

	private WidgetTemplate m_widget;

	private Clickable m_buttonClickable;

	private GameModeDownloadProgress m_downloadProgress;

	private IGameDownloadManager DownloadManager => GameDownloadManagerProvider.Get();

	private void Awake()
	{
		m_widget = GetComponent<WidgetTemplate>();
		m_buttonClickable = GetComponent<Clickable>();
		m_downloadProgressReference.RegisterReadyListener<Widget>(OnDownloadProgressReady);
		m_buttonClickable.AddEventListener(UIEventType.ROLLOVER, OnButtonMouseOver);
		m_buttonClickable.AddEventListener(UIEventType.ROLLOUT, OnButtonMouseOut);
	}

	private void OnDownloadProgressReady(Widget downloadProgressWidget)
	{
		if (downloadProgressWidget == null)
		{
			Error.AddDevWarning("UI Error!", "downloadProgressWidget could not be found in game mode button!");
			return;
		}
		m_downloadProgress = downloadProgressWidget.gameObject.GetComponentInChildren<GameModeDownloadProgress>();
		DownloadTags.Content moduleTag = DownloadTags.GetContentTag(GetGameModeButtonDataModel().ModuleTag);
		m_downloadProgress.Init(moduleTag);
	}

	private GameModeButtonDataModel GetGameModeButtonDataModel()
	{
		m_widget.GetDataModel(172, out var dataModel);
		return dataModel as GameModeButtonDataModel;
	}

	private void OnButtonMouseOver(UIEvent e)
	{
		TooltipZone tooltipZone = e.GetElement()?.gameObject.GetComponentInChildren<TooltipZone>();
		if (tooltipZone == null)
		{
			Error.AddDevWarning("UI Error!", "Game mode button missing TooltipZone component");
			return;
		}
		DownloadTags.Content moduleTag = DownloadTags.GetContentTag(GetGameModeButtonDataModel().ModuleTag);
		if (moduleTag != 0)
		{
			if (DownloadManager.IsModuleDownloading(moduleTag))
			{
				tooltipZone.ShowBoxTooltip(GameStrings.Get("GLUE_GAME_MODE_TOOLTIP_DOWNLOAD_REQUIRED_TITLE"), GameStrings.Format("GLUE_GAME_MODE_TOOLTIP_DOWNLOADING_DESCRIPTION", DownloadUtils.GetGameModeName(moduleTag)));
			}
			else if (!DownloadManager.IsModuleReadyToPlay(moduleTag))
			{
				tooltipZone.ShowBoxTooltip(GameStrings.Get("GLUE_GAME_MODE_TOOLTIP_DOWNLOAD_REQUIRED_TITLE"), GameStrings.Format("GLUE_GAME_MODE_TOOLTIP_DOWNLOAD_REQUIRED_DESCRIPTION", DownloadUtils.GetGameModeName(moduleTag)));
			}
		}
	}

	private void OnButtonMouseOut(UIEvent e)
	{
		TooltipZone tooltipZone = e.GetElement().gameObject.GetComponentInChildren<TooltipZone>();
		if (!(tooltipZone == null))
		{
			tooltipZone.HideTooltip();
		}
	}

	private void OnDestroy()
	{
		m_buttonClickable.RemoveEventListener(UIEventType.ROLLOVER, OnButtonMouseOver);
		m_buttonClickable.RemoveEventListener(UIEventType.ROLLOUT, OnButtonMouseOut);
	}
}
